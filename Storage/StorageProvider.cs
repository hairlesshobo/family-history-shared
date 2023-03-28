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
using System.IO;
using System.Threading.Tasks;

namespace FoxHollow.FHM.Shared.Storage;

/// <summary>
///     Interface that describes a storage provider used for accessing the
///     backend data of the family history repository
/// </summary>
public abstract class StorageProvider : IStorageProvider
{
    protected IServiceProvider Services { get; }
    protected ProviderConfigCollection Config { get; }

    public abstract StorageProviderInfo Information { get; }

    public abstract bool Connected { get; }

    public abstract ProviderDirectory RootDirectory { get; protected set; }

    public StorageProvider(IServiceProvider services, ProviderConfigCollection config)
    {
        this.Services = services ?? throw new ArgumentNullException(nameof(services));
        this.Config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public abstract void Connect();

    public abstract Task ConnectAsync();

    public abstract IAsyncEnumerable<ProviderEntryBase> ListDirectory(string path);
}