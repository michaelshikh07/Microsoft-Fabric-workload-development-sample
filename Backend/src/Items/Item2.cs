using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boilerplate.Constants;
using Boilerplate.Contracts;
using Boilerplate.Contracts.KustoItems;
using Boilerplate.Services;
using Boilerplate.Utils;
using Fabric_Extension_BE_Boilerplate.Constants;
using Fabric_Extension_BE_Boilerplate.Contracts.FabricAPI.Workload;
using Kusto.Data.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Fabric.Api.KQLDatabase.Models;

namespace Boilerplate.Items;

public class Item2 : ItemBase<Item2, Item2Metadata, Item2ClientMetadata>, IItem2
{
    private const string KustoIotDataTableName = "IotData";
    
    private static readonly IList<string> FabricScopes = new[] { $"{EnvironmentConstants.FabricBackendResourceId}/{WorkloadScopes.EventhouseReadWriteAll}" };
    public override string ItemType => WorkloadConstants.ItemTypes.Item2;

    private readonly IFabricApiClient _fabricApiClient;
    private readonly IAuthenticationService _authenticationService;
    private readonly IKustoClientService _kustoClientService;
    
    private Item2Metadata _metadata;
    private Item2Metadata Metadata => Ensure.NotNull(_metadata, "The item object must be initialized before use");

    public Item2(
        ILogger logger,
        IItemMetadataStore itemMetadataStore,
        IFabricApiClient fabricApiClient,
        IAuthenticationService authenticationService,
        AuthorizationContext authorizationContext,
        IKustoClientService kustoClientService)
        : base(logger, itemMetadataStore, authorizationContext)
    {
        _fabricApiClient = fabricApiClient;
        _authenticationService = authenticationService;
        _kustoClientService = kustoClientService;
    }

    public override Task<ItemPayload> GetItemPayload()
    {
        var typeSpecificMetadata = GetTypeSpecificMetadata();
        return Task.FromResult(new ItemPayload
        {
            Item2Metadata = typeSpecificMetadata.ToClientMetadata()
        });
    }

    protected override void SetDefinition(CreateItemPayload payload)
    {
        if (payload?.Item2Metadata == null)
        {
            Logger.LogInformation("No payload is provided for {0}, objectId={1}", ItemType, ItemObjectId);
            _metadata = Item2Metadata.Default.Clone();
            return;
        }
        
        _metadata = payload.Item2Metadata.Clone();
    }

    protected override void UpdateDefinition(UpdateItemPayload payload)
    {
        throw new NotImplementedException();
    }

    protected override Item2Metadata GetTypeSpecificMetadata()
    {
        return Metadata.Clone();
    }

    protected override void SetTypeSpecificMetadata(Item2Metadata itemMetadata)
    {
        _metadata = itemMetadata.Clone();
    }

    protected override async Task CreateAdditionalResources()
    {
        try
        {
            var metadata = Metadata.Clone();
            
            var fabricToken = await _authenticationService.GetAccessTokenOnBehalfOf(AuthorizationContext, FabricScopes);

            var eventhouseDisplayName = $"{DisplayName}_Eventhouse";
            var eventhouseItem = await _fabricApiClient.CreateEventhouse(WorkspaceObjectId, eventhouseDisplayName, fabricToken);
            eventhouseItem = await _fabricApiClient.GetEventhouse(WorkspaceObjectId, eventhouseItem.Id.Value, fabricToken);

            var defaultKqlDatabaseId = eventhouseItem.Properties.DatabasesItemIds.FirstOrDefault();
            var kqlDatabaseItem = await _fabricApiClient.GetKqlDatabase(WorkspaceObjectId, defaultKqlDatabaseId, fabricToken);

            metadata.EventhouseItemId = eventhouseItem.Id;
            metadata.EventhouseDisplayName = eventhouseItem.DisplayName;
            metadata.KqlDatabaseItemId = kqlDatabaseItem.Id;
            metadata.KqlDatabaseDisplayName = kqlDatabaseItem.DisplayName;
            metadata.KqlDatabaseQueryUrl = kqlDatabaseItem.Properties.QueryServiceUri;

            _metadata = metadata;
            
            // fire and forget, prepare initial data on kusto side
            _ = PrepareKqlDatabaseData(kqlDatabaseItem);

            Logger.LogInformation($"CreateAdditionalResources: successfully create Eventhouse {eventhouseItem.Id} with default KQL database {defaultKqlDatabaseId}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to create additional resources for item: {DisplayName} in workspace: {WorkspaceObjectId}. Error: {ex.Message}");
        }
    }

    public override Task ExecuteJob(string jobType, Guid jobInstanceId, JobInvokeType invokeType, CreateItemJobInstancePayload creationPayload)
    {
        throw new NotImplementedException();
    }

    public override Task<ItemJobInstanceState> GetJobState(string jobType, Guid jobInstanceId)
    {
        throw new NotImplementedException();
    }

    private async Task PrepareKqlDatabaseData(KQLDatabase kqlDatabase)
    {
        try
        {
            var scopes = new[] {$"{kqlDatabase.Properties.QueryServiceUri}/.default"};
            var kustoDataPlaneToken = await _authenticationService.GetAccessTokenOnBehalfOf(AuthorizationContext, scopes);
            var kustoClientRequestProperties = new ClientRequestProperties
            {
                AuthorizationScheme = "Bearer",
                SecurityToken = kustoDataPlaneToken
            };

            // create the Table
            var tableCreateCommand = CslCommandGenerator.GenerateTableCreateCommand(KustoIotDataTableName, typeof(KustoIotDataTableRecord), forceNormalizeColumnName: false);
            await _kustoClientService.ExecuteControlCommandAsync(kqlDatabase.Properties.QueryServiceUri, kqlDatabase.Id.ToString(), tableCreateCommand, kustoClientRequestProperties, default);

            // ingest records to the Table
            var records = KustoIotDataTableRecordExtensions.GenerateRecords(3);
            var csvData = string.Join(Environment.NewLine, records.Select(r => r.ToCsvFormat()));
            var ingestCommand = CslCommandGenerator.GenerateTableIngestPushCommand(KustoIotDataTableName, compressed: false, csvData);
            await _kustoClientService.ExecuteControlCommandAsync(kqlDatabase.Properties.QueryServiceUri, kqlDatabase.Id.ToString(), ingestCommand, kustoClientRequestProperties, default);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to prepare kql database data for database id {kqlDatabase.Id} in workspace: {kqlDatabase.WorkspaceId}. Error: {ex.Message}");
        }
    }
}