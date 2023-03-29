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

namespace FoxHollow.FHM.Shared.Storage;

public class StorageManager
{
    private Dictionary<string, Type> _providerTypes;
    private IServiceProvider _services;

    public StorageManager(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _providerTypes = new Dictionary<string, Type>();
    }

    public void RegisterProvider<TProvider>()
        where TProvider : StorageProvider
    {
        var providerType = typeof(TProvider);

        if (!_providerTypes.Values. .Contains(providerType))
            _providerTypes.Add(providerType);
    }

    public void AddRepository(string providerID, ProviderConfigCollection config)
    {

    }

    public IStorageProvider GetRepository(string id)
    {
        
    }
}