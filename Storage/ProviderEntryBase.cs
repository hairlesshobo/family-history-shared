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

namespace FoxHollow.FHM.Shared.Storage;

public abstract class ProviderEntryBase
{
    /// <summary>
    ///     Handle to the storage provider this file belongs to
    /// </summary>
    IStorageProvider Provider { get; }
    
    // string FullPath { get; }
    // string OriginalPath { get; }
    // string LinkTarget { get; }

    DateTime LastWriteTimeUtc { get; }
    DateTime LastAccessTimeUtc { get; }
    DateTime CreationTimeUtc { get; }
    string Name { get; }
    string Path { get; }
    string RawPath { get; }
    string Extension { get; }

}