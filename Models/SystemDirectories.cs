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
///     Class that describes the system directories
/// </summary>
public class SystemDirectories
{
    /// <summary>
    ///     Full path to the directory of the executable that is currently running
    /// </summary>
    public string Bin { get; internal protected set; }

    /// <summary>
    ///     Full path to the archive index directory
    /// </summary>
    public string Index { get; internal protected set; }

    /// <summary>
    ///     Full path to the archive json index directory
    /// </summary>
    public string JSON { get; internal protected set; }

    /// <summary>
    ///     Full path to the staging directory to use when archiving to disc
    /// </summary>
    public string DiscStaging { get; internal protected set; }

    /// <summary>
    ///     Full path to the directory where ISO files will be created
    /// </summary>
    public string ISO { get; internal protected set; }
}