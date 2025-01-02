﻿using System;
using System.Threading.Tasks;
using Microsoft.Fabric.Api.Eventhouse.Models;
using Microsoft.Fabric.Api.KQLDatabase.Models;

namespace Boilerplate.Services;

public interface IFabricApiClient
{
    public Task<Eventhouse> CreateEventhouse(Guid workspaceId, string displayName, string token);

    public Task<Eventhouse> GetEventhouse(Guid workspaceId, Guid eventhouseId, string token);

    public Task<KQLDatabase> GetKqlDatabase(Guid workspaceId, Guid kqlDatabaseId, string token);
}