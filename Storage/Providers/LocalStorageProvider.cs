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
using System.Threading.Tasks;

namespace FoxHollow.FHM.Shared.Storage;

/// <summary>
///     Storage provider used to interact with data stored locally
/// </summary>
public class LocalStorageProvider : IStorageProvider
{
    /// <ineritdoc />
    public StorageProviderInfo Information { get; } = new StorageProviderInfo(
        "local",
        "Local Storage",
        "Provider used to interact with data stored locally",
        new string[] { "file" }
    );

    /// <ineritdoc />
    public bool Connected => throw new NotImplementedException();

    /// <ineritdoc />
    public void Connect()
    {
        throw new NotImplementedException();
    }

    /// <ineritdoc />
    public Task ConnectAsync()
    {
        throw new NotImplementedException();
    }
}