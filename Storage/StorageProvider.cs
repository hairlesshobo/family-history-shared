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
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace FoxHollow.FHM.Shared.Storage;

/// <summary>
///     Abstract class that describes a storage provider used for accessing 
///     the backend data of the family history repository
/// </summary>
public abstract class StorageProvider
{
    protected IServiceProvider Services { get; }
    protected ProviderConfigCollection Config { get; }


    /// <summary>
    ///     Has the storage driver been connected?
    /// </summary>
    public bool Connected { get; protected set; }

    /// <summary>
    ///     Handle to the root directory of the storage provider
    /// </summary>
    public StorageDirectory RootDirectory { get; protected set; }

    /// <summary>
    ///     Unique ID of the configured repository for this StorageProvider instance
    /// </summary>
    public string RepositoryID { get; private set; }


    /// <summary>
    ///     Constructor that requires DI container and provider configuration
    /// </summary>
    /// <param name="services">DI container</param>
    /// <param name="config">Provider configuration</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if either required parameter is not provided to the constructor
    /// </exception>
    public StorageProvider(IServiceProvider services, ProviderConfigCollection config)
    {
        this.Services = services ?? throw new ArgumentNullException(nameof(services));
        this.Config = config ?? throw new ArgumentNullException(nameof(config));

        this.RequireValidConfig();

        this.RepositoryID = this.Config["RepositoryID"];
    }

    /// <summary>
    ///     Make the initial connection to the backend storage provider
    /// </summary>
    public abstract void Connect();

    /// <summary>
    ///     Asynchronously make the connectoin to the backend storage provider
    /// </summary>
    public abstract Task ConnectAsync();

    /// <summary>
    ///     List the contents of the directory at the provided path
    /// </summary>
    /// <param name="path">Path to list directory contents for</param>
    /// <returns>Enumerable of file/directory entries</returns>
    public abstract IEnumerable<StorageEntryBase> ListDirectory(string path);

    /// <summary>
    ///     Get the directory handle of a directory at the specified path
    /// </summary>
    /// <param name="path">Path to get directory handle for</param>
    /// <returns>Directory handle, null if the path does not exist</returns>
    public abstract StorageDirectory GetDirectory(string path);

    public abstract StorageDirectory CreateDirectory(string path);

    public abstract bool Exists(string path);

    /// <summary>
    ///     Test the provided config to ensure that the globally required config
    ///     values were provided
    /// </summary>
    /// <exception cref="ConfigurationErrorsException">Throw if any required config value is missing</exception>
    protected virtual void RequireValidConfig()
    {
        if (this.Config["RepositoryID"] == null)
            throw new ConfigurationErrorsException("'RepositoryID' is required by StorageProvider");
    }
}