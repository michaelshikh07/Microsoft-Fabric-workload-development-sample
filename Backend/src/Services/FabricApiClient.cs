using System;
using System.Threading.Tasks;
using Azure;
using Boilerplate.Constants;
using Microsoft.Fabric.Api;
using Microsoft.Fabric.Api.Eventhouse.Models;

namespace Boilerplate.Services;

public class FabricApiClient : IFabricApiClient
{
    private Uri _fabricBaseUri;

    public FabricApiClient()
    {
        _fabricBaseUri = new Uri(EnvironmentConstants.FabricApiBaseUrl);
    }

    public async Task<Response<Eventhouse>> CreateEventhouse(Guid workspaceId, string displayName, string token)
    {
        var fabricClient = new FabricClient(token, _fabricBaseUri);
        var createEventhouseRequest = new CreateEventhouseRequest(displayName);

        return await fabricClient.Eventhouse.Items.CreateEventhouseAsync(workspaceId, createEventhouseRequest);
    }

    public async Task<Response<Eventhouse>> GetEventhouse(Guid workspaceId, Guid eventhouseId, string displayName, string token)
    {
        var fabricClient = new FabricClient(token, _fabricBaseUri);
        var createEventhouseRequest = new CreateEventhouseRequest(displayName);

        return await fabricClient.Eventhouse.Items.GetEventhouseAsync(workspaceId, eventhouseId);
    }
}