/**
 *  Archiver - Cross platform, multi-destination backup and archiving utility
 * 
 *  Copyright (c) 2020-2021 Steve Cross <flip@foxhollow.cc>
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;

namespace FoxHollow.FHM.Shared.Structures
{
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
}