﻿// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Boilerplate.Contracts;
using Boilerplate.Items;
using Fabric_Extension_BE_Boilerplate.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Boilerplate.Services
{
    public class ItemFactory : IItemFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IItemMetadataStore _itemMetadataStore;
        private readonly ILakehouseClientService _lakeHouseClientService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IFabricApiClient _fabricApiClient;
        private readonly IKustoClientService _kustoClientService;

        public ItemFactory(
            IServiceProvider serviceProvider,
            IItemMetadataStore itemMetadataStore,
            ILakehouseClientService lakeHouseClientService,
            IAuthenticationService authenticationService,
            IFabricApiClient fabricApiClient,
            IKustoClientService kustoClientService)
        {
            _serviceProvider = serviceProvider;
            _itemMetadataStore = itemMetadataStore;
            _lakeHouseClientService = lakeHouseClientService;
            _authenticationService = authenticationService;
            _fabricApiClient = fabricApiClient;
            _kustoClientService = kustoClientService;
        }

        public IItem CreateItem(string itemType, AuthorizationContext authorizationContext)
        {
            switch (itemType)
            {
                case WorkloadConstants.ItemTypes.Item1:
                    return new Item1(_serviceProvider.GetService<ILogger<Item1>>(), _itemMetadataStore, _lakeHouseClientService, _authenticationService, authorizationContext);
                case WorkloadConstants.ItemTypes.Item2:
                    return new Item2(_serviceProvider.GetService<ILogger<Item2>>(), _itemMetadataStore, _fabricApiClient, _authenticationService, authorizationContext, _kustoClientService);

                default:
                    throw new NotSupportedException($"Items of type {itemType} are not supported");
            }
        }
    }
}
