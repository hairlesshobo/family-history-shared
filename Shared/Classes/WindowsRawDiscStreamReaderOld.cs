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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Utilities;
using Microsoft.Win32.SafeHandles;

namespace Archiver.Shared.Classes
{
    public class WindowsRawDiscStreamReaderOld : FileStream
    {
        private long _length = -1;

        public override bool CanRead => base.CanRead;

        public override bool CanSeek => base.CanSeek;

        public override bool CanWrite => false;

        public override long Length => _length >= 0 ? _length : throw new NotSupportedException();


        private WindowsRawDiscStreamReaderOld(SafeFileHandle handle)
            : base(handle, FileAccess.Read)
        {
            _length = (long)DiskUtils.Windows.GetDiskSize(handle);
        }

        public static WindowsRawDiscStreamReaderOld OpenDisc(string driveName)
        {
            // TODO: Fix
            // string drivePath = PathUtils.GetDrivePath(driveName);
            string drivePath = driveName;

            SafeFileHandle handle = Native.Windows.CreateFile(
                drivePath,
                Native.Windows.GENERIC_READ,
                Native.Windows.FILE_SHARE_READ | Native.Windows.FILE_SHARE_WRITE,
                IntPtr.Zero,
                Native.Windows.OPEN_EXISTING,
                0,
                IntPtr.Zero
            );

            if (handle.IsInvalid)
                throw new NativeMethodException("CreateFile");

            return new WindowsRawDiscStreamReaderOld(handle);
        }
    }
}