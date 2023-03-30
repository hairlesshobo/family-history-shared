//==========================================================================
//  Family History Manager - https://code.foxhollow.cc/fhm/
//
//  A cross platform tool to help organize and preserve all types
//  of family history
//==========================================================================
//  Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;
using Microsoft.Extensions.DependencyInjection;

namespace FoxHollow.FHM.Shared.Storage;

public class StorageManager
{
    private List<KeyValuePair<string, Type>> _providerTypes;
    private Dictionary<string, StorageProvider> _repositories;
    private IServiceProvider _services;

    public StorageManager(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _providerTypes = new List<KeyValuePair<string, Type>>();
        _repositories = new Dictionary<string, StorageProvider>();
    }

    public void RegisterProvider<TProvider>()
        where TProvider : StorageProvider
    {
        var providerType = typeof(TProvider);

        if (!_providerTypes.Any(x => x.Value == providerType))
        {
            var property = providerType.GetProperty("Information", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            if (property == null)
                throw new InvalidOperationException("Provided StorageProvider did not set the 'Information' property.");

            var info = (StorageProviderInfo)property.GetValue(null);

            if (info == null)
                throw new InvalidOperationException("Provided StorageProvider did not set the 'Information' property.");

            _providerTypes.Add(new KeyValuePair<string, Type>(info.ID, providerType));
        }
    }

    public void AddRepository(string providerID, ProviderConfigCollection config)
    {
        var provider = _providerTypes.FirstOrDefault(x => x.Key == providerID);

        // couldn't find a provider with that ID
        if (provider.Equals(default(KeyValuePair<string, Type>)))
            throw new EntryPointNotFoundException();

        var providerType = provider.Value;

        StorageProvider repoProvider = (StorageProvider)ActivatorUtilities.CreateInstance(_services, providerType, config);

        _repositories.Add(repoProvider.RepositoryID, repoProvider);
    }

    public static void InitStorageProviders(IServiceProvider services)
    {
        var sm = services.GetRequiredService<StorageManager>();
        sm.RegisterProvider<LocalStorageProvider>();

        var spConfig = new ProviderConfigCollection();
        spConfig.Add(new ProviderConfigValue("RepositoryID", "local"));
        spConfig.Add(new ProviderConfigValue("RootPath", "/"));

        sm.AddRepository(LocalStorageProvider.Information.ID, spConfig);
    }

    public StorageProvider GetRepository(string repositoryID)
    {
        var repository = _repositories[repositoryID];

        if (repository == null)
            throw new StorageRepositoryNotFoundException(repositoryID);

        return repository;
    }
}