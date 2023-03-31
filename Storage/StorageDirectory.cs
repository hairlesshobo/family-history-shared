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
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FoxHollow.FHM.Shared.Storage;

/// <summary>
///     Interface that describes a directory in the backend storage
/// </summary>
public sealed class StorageDirectory : StorageEntryBase
{
    /// <summary>
    ///     Parent directory to which this directory belongs
    /// </summary>
    public StorageDirectory Parent { get; private set; }

    /// <inheritdoc />
    internal StorageDirectory(StorageProvider provider, string path) : base(provider, path)
    {
        this.IsDirectory = true;
    }

    /// <summary>
    ///     List the contents of a directory at the specified path
    /// </summary>
    /// <returns>Enumerable of contents of directory, if it exists</returns>
    public IEnumerable<StorageEntryBase> ListDirectory()
        // TODO: reference this.Path instead
        => this.Provider.ListDirectory(this._providedPath);

    public IEnumerable<StorageFile> ListFiles()
    {
        foreach (var entry in this.ListDirectory())
        {
            if (entry is StorageFile)
                yield return (StorageFile)entry;
        }
    }

    public IEnumerable<StorageFile> ListFiles(string filter)
    {
        foreach (var entry in this.ListFiles())
        {
            if (entry.Name.Contains(filter))
                yield return entry;
        }
    }
}