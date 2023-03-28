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
///     Storage provider used to interact with data stored locally
/// </summary>
public class LocalStorageProvider : StorageProvider
{
    /// <ineritdoc />
    public override StorageProviderInfo Information { get; } = new StorageProviderInfo(
        "local",
        "Local Storage",
        "Provider used to interact with data stored locally",
        new string[] { "file" }
    );


    /// <ineritdoc />
    public override bool Connected => throw new NotImplementedException();

    /// <inheritdoc />
    public override ProviderDirectory RootDirectory { get; protected set; }

    /// <summary>
    ///     Constructor that requires DI container
    /// </summary>
    /// <param name="services">DI container</param>
    public LocalStorageProvider(IServiceProvider services, ProviderConfigCollection config)
        : base(services, config)
    { }

    /// <ineritdoc />
    public override void Connect()
    {
        this.RootDirectory = new ProviderDirectory(this, "/");
    }

    /// <ineritdoc />
    public override Task ConnectAsync()
    {
        this.Connect();

        return Task.CompletedTask;
    }

    /// <ineritdoc />
    public override async IAsyncEnumerable<ProviderEntryBase> ListDirectory(string path)
    {
        // TODO: actually make this async
        foreach (var entry in Directory.EnumerateDirectories(path))
            yield return new ProviderDirectory(this, entry);

        foreach (var entry in Directory.EnumerateFiles(path))
            yield return new ProviderFile(this, entry);
    }
}