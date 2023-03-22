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

namespace FoxHollow.FHM.Shared.Structures;

public struct HashGenerationProgress
{
    // TODO: Rename others to to ElapsedTime
    /// <summary>
    ///     Snapshot of total time elapsed during this operation
    /// </summary>
    public TimeSpan ElapsedTime { get; set; }

    /// <summary>
    ///     Total bytes that have been processed
    /// </summary>
    public long TotalBytesProcessed { get; set; }

    /// <summary>
    ///     Number of bytes processed since the last update was called
    /// </summary>
    public long BytesProcessedSinceLastUpdate { get; set; }

    /// <summary>
    ///     Total bytes to be processed
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    ///     Current completion percent as a decimal in the range of 0.0 - 1.0
    /// </summary>
    public double PercentCompleted { get; set; }

    /// <summary>
    ///     Transfer rate (in the unit of bytes/second) since last progress update
    /// </summary>
    public double InstantRate { get; set; }

    /// <summary>
    ///     Average transfer rate (in the unit of bytes/second) since the beginning of the operation
    /// </summary>
    public double AverageRate { get; set; }

    /// <summary>
    ///     Flag indiciating whether the process is complete
    /// </summary>
    public bool Complete { get; set; }
}