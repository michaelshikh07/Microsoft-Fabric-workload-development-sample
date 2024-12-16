using System;
using System.Threading.Tasks;
using Boilerplate.Contracts;
using Boilerplate.Services;
using Boilerplate.Utils;
using Fabric_Extension_BE_Boilerplate.Constants;
using Fabric_Extension_BE_Boilerplate.Contracts.FabricAPI.Workload;
using Microsoft.Extensions.Logging;

namespace Boilerplate.Items;

public class Item2 : ItemBase<Item2, Item2Metadata, Item2ClientMetadata>, IItem2
{
    public Item2(ILogger logger, IItemMetadataStore itemMetadataStore, AuthorizationContext authorizationContext) : base(logger, itemMetadataStore, authorizationContext)
    {
    }

    public override string ItemType => WorkloadConstants.ItemTypes.Item2;
    
    private Item2Metadata _metadata;
    private Item2Metadata Metadata => Ensure.NotNull(_metadata, "The item object must be initialized before use");

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
        throw new NotImplementedException();
    }

    public override Task ExecuteJob(string jobType, Guid jobInstanceId, JobInvokeType invokeType, CreateItemJobInstancePayload creationPayload)
    {
        throw new NotImplementedException();
    }

    public override Task<ItemJobInstanceState> GetJobState(string jobType, Guid jobInstanceId)
    {
        throw new NotImplementedException();
    }
}