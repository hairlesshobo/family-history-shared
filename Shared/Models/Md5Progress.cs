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

namespace Archiver.Shared.Models
{
    public class Md5Progress 
    {
        // TODO: Rename to TotalBytesProcessed
        public long TotalCopiedBytes { get; set; } = 0;
        // TODO: Rename to BytesProcessedSinceLastUpdate
        public long BytesCopiedSinceLastupdate { get; set; } = 0;
        public long TotalBytes { get; set; } = 0;
        // TODO: rename to PercentCompleted
        /// <summary>
        ///     Current completion percent
        /// </summary>
        public double PercentCopied { get; set; } = 0.0;
        // TODO: rename to InstantRate
        public double InstantTransferRate { get; set; } = 0.0;
        // TODO: rename to AverageRate
        public double AverageTransferRate { get; set; } = 0.0;
        public TimeSpan ElapsedTime { get; set; }
        public bool Complete { get; set; } = false;
        // TODO: possible remove so that this can be turned into a struct?
        public string FileName { get; set; } = String.Empty;
    }
}