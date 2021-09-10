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
using Archiver.Shared.Exceptions;
using Archiver.Shared.Models;
using Archiver.Shared.Native;
using Archiver.Shared.Utilities;

namespace Archiver.Shared.Classes
{

    public class LinuxNativeStreamReader : Stream
    {
        public enum StreamSourceType
        {
            File = 0,
            Disk = 1,
            Tape = 2
        }
        
        public override bool CanRead => true;

        public override bool CanSeek => (_type == StreamSourceType.Disk || _type == StreamSourceType.File);

        public override bool CanWrite => false;

        public override long Length => _length >= 0 ? _length : throw new NotSupportedException();

        public override long Position 
        { 
            get => (long)_position;
            set => throw new NotImplementedException(); 
        }

        private string _devicePath = null;
        private int _handle = -1;
        private bool _eofReached = false;
        private ulong _position = 0;
        private StreamSourceType _type;
        private long _length = -1;

        public LinuxNativeStreamReader(OpticalDrive drive)
        {
            _type = StreamSourceType.Disk;
            _devicePath = drive.FullPath;
        }

        private void Init()
        {
            if (!File.Exists(_devicePath))
                throw new DriveNotFoundException();

            _handle = Linux.Open(_devicePath, Linux.OpenType.ReadOnly);

            if (_type == StreamSourceType.Disk)
                _length = (long)DiskUtils.Linux.GetDiskSize(_handle);
            else if (_type == StreamSourceType.File)
                _length = DiskUtils.Linux.GetFileSize(_devicePath);
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int result = Linux.Read(_handle, buffer, count);

            _position += (ulong)result;

            if (result < 0)
                throw new NativeMethodException("Read");

            if (result == 0)
                _eofReached = true;

            return result;
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

            if (_handle >= 0)
                Linux.Close(_handle);
        }
    }
}