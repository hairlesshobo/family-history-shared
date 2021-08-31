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