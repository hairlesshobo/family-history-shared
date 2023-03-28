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

/// <summary>
///     Interface that describes a directory in the backend storage
/// </summary>
public sealed class ProviderDirectory : ProviderEntryBase
{
    /// <summary>
    ///     Parent directory to which this directory belongs
    /// </summary>
    public ProviderDirectory Parent { get; private set; }

    /// <inheritdoc />
    public ProviderDirectory(IStorageProvider provider, string path) : base(provider, path)
    {
        this.IsDirectory = true;
    }

    /// <summary>
    ///     List the contents of a directory at the specified path
    /// </summary>
    /// <returns>Asynchronous enumerable of contents of directory, if it exists</returns>
    public IAsyncEnumerable<ProviderEntryBase> ListDirectoryAsync()
        // TODO: reference this.Path instead
        => this.Provider.ListDirectory(this._providedPath);

}