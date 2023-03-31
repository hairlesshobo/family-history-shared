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
using System.IO;
using FoxHollow.FHM.Shared.Utilities;

namespace FoxHollow.FHM.Shared.Storage;

/// <summary>
///     Abstract class used as a base for both ProviderDirectory and ProviderFile
/// </summary>
public abstract class StorageEntryBase
{
    /// <summary>
    ///     Path provided to constructor, used internally only
    /// </summary>
    protected string _providedPath;

    /// <summary>
    ///     Handle to the storage provider this file belongs to
    /// </summary>
    public StorageProvider Provider { get; }
    
    // string FullPath { get; }
    // string OriginalPath { get; }
    // string LinkTarget { get; }

    public DateTime LastWriteTimeUtc { get; }
    public DateTime LastAccessTimeUtc { get; }
    public DateTime CreationTimeUtc { get; }
    public string Name { get; }
    public string BaseName { get; }

    public string Path { get; private set; }
    public string RawPath { get; }
    public string Extension { get; }

    public bool IsFile { get; protected set; }
    public bool IsDirectory { get; protected set; }

    /// <summary>
    ///     Constructor that requires a storage provider and entry path
    /// </summary>
    /// <param name="provider">Storage provider handle</param>
    /// <param name="path">Entry path</param>
    public StorageEntryBase(StorageProvider provider, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));

        this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        this._providedPath = path;

        this.Path = PathUtils.CleanPath(path);
        this.Name = PathUtils.GetFileName(this.Path);
        this.Extension = PathUtils.GetExtension(this.Path);
    }

}