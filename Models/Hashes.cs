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

namespace FoxHollow.FHM.Shared.Models;

/// <summary>
///     Simple model that describes file hashes in more than one format
/// </summary>
public class Hashes
{
    /// <summary>
    ///     MD5 hash
    /// </summary>
    public string Md5Hash { get; private set; }

    /// <summary>
    ///     SHA1 hash
    /// </summary>
    public string Sha1Hash { get; private set; }

    /// <summary>
    ///     Create a new instance by providing the required hashes
    /// </summary>
    /// <param name="md5">MD5 hash to set</param>
    /// <param name="sha1">SHA1 hash to set</param>
    public Hashes(string md5 = null, string sha1 = null)
    {
        this.Md5Hash = md5;
        this.Sha1Hash = sha1;
    }
}