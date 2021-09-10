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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;
using Archiver.Shared.Native;
using Archiver.Shared.Utilities;
using Microsoft.Win32.SafeHandles;

namespace Archiver.Shared.Classes
{

    public class WindowsRawDiscStreamReader : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => throw new NotImplementedException(); //(_type == StreamSourceType.Disk || _type == StreamSourceType.File);

        public override bool CanWrite => false;

        public override long Length => _length >= 0 ? _length : throw new NotSupportedException();

        public override long Position 
        { 
            get => _position;
            set => throw new NotImplementedException(); 
        }

        private string _devicePath = null;
        private SafeFileHandle _handle = null;
        private long _position = 0;
        private long _length = -1;
        private const int _bytesPerSector = 2048;
        private const int _readSectorCount = 256;
        private OpticalDrive _drive;

        public WindowsRawDiscStreamReader(OpticalDrive drive)
        {
            _drive = drive ?? throw new ArgumentNullException(nameof(drive));

            _handle = Windows.CreateFile(
                _drive.FullPath,
                Native.Windows.EFileAccess.GenericRead,
                Native.Windows.EFileShare.Read | Native.Windows.EFileShare.Write,
                IntPtr.Zero,
                Native.Windows.ECreationDisposition.OpenExisting,
                Native.Windows.EFileAttributes.Write_Through
                    | Native.Windows.EFileAttributes.NoBuffering
                    | Native.Windows.EFileAttributes.RandomAccess,
                IntPtr.Zero
            );

            if (_handle.IsInvalid)
                throw new NativeMethodException("CreateFile");

            DiskUtils.Windows.SetAllowExtendedIO(_handle);

            _length = DiskUtils.Windows.GetLengthInformation(_handle).Length;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset != 0)
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "Specifying a non-zero offset is not supported");

            // if the user is asking for more bytes than we have left.. decrease the count to
            // equal the number of bytes remaining
            if ((_length - _position) < count)
                count = (int)(_length - _position);

            if (count == 0)
                return 0;

            IntPtr ptr = IntPtr.Zero;

            try
            {
                uint bytesRead = 0;

                ptr = Marshal.AllocHGlobal(count);
                
                // TODO: is this actually needed?
                Array.Fill(buffer, (byte)0);

                bool result = Native.Windows.ReadFile(
                    _handle,
                    ptr,
                    (uint)count,
                    ref bytesRead,
                    IntPtr.Zero
                );

                //* There has to be a better way to detect EOF
                if (result == false && bytesRead == 0 && Marshal.GetLastWin32Error() == Windows.ERROR_SECTOR_NOT_FOUND)
                    return 0;

                if (result == false)
                    throw new NativeMethodException("ReadFile");

                if (bytesRead > 0)
                    Marshal.Copy(ptr, buffer, 0, (int)bytesRead);

                _position += bytesRead;

                return (int)bytesRead;

            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override void Close()
        {
            base.Close();

            if (_handle != null)
                _handle.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (_handle != null)
                _handle.Dispose();
            
            base.Dispose(disposing);
        }
    }
}