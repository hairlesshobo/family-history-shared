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
    public delegate void MD5_CompleteDelegate(string hash);
    public delegate void MD5_ProgressChangedDelegate(Md5Progress progress);

    public class Md5Progress 
    {
        public long TotalCopiedBytes { get; set; } = 0;
        public long BytesCopiedSinceLastupdate { get; set; } = 0;
        public long TotalBytes { get; set; } = 0;
        public double PercentCopied { get; set; } = 0.0;
        public double InstantTransferRate { get; set; } = 0.0;
        public double AverageTransferRate { get; set; } = 0.0;
        public TimeSpan ElapsedTime { get; set; }
        public bool Complete { get; set; } = false;
        public string FileName { get; set; } = String.Empty;
    }
}