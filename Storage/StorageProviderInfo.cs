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
using System.ServiceModel.Description;

namespace FoxHollow.FHM.Shared.Storage;

/// <summary>
///     Class that is used to describe a storage provider
/// </summary>
public class StorageProviderInfo
{
    /// <summary>
    ///     Unique ID of the storage provider. Alphanumeric only, no spaces
    /// </summary>
    public readonly string ID;

    /// <summary>
    ///     User-friendly name of the storage provider, to be displayed in the UI
    /// </summary>
    public readonly string Name;

    /// <summary>
    ///     User-friendly description of the storage provider, to be displayed in the UI
    /// </summary>
    public readonly string Description;

    /// <summary>
    ///     List of protocols supported by this provider, examples: "smb", "local", "s3"
    /// </summary>
    public readonly string[] Protocols;

    /// <summary>
    ///     Configuration properties that are used by the storage provider
    /// </summary>
    public readonly ProviderConfigProperty[] ConfigProperties;

    /// <summary>
    ///     Constructor used to create new storage provider info
    /// </summary>
    /// <param name="id">Unique ID of the storage provider. Alphanumeric only, no spaces</param>
    /// <param name="name">User-friendly name of the storage provider</param>
    /// <param name="description">User-friendly description of the storage provider</param>
    /// <param name="protocols">List of protocols supported by the storage provider</param>
    internal StorageProviderInfo(string id, string name, string description, string[] protocols, ProviderConfigProperty[] config)
    {
        this.ID = id ?? throw new ArgumentNullException(nameof(id));
        this.Name = name;
        this.Description = description;
        this.Protocols = protocols;
        this.ConfigProperties = config;
    }
}