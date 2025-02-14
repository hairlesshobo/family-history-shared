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
using System.IO;
using FoxHollow.FHM.Shared.Exceptions;
using FoxHollow.FHM.Shared.Models;
using FoxHollow.FHM.Shared.Native;
using FoxHollow.FHM.Shared.Utilities;

namespace FoxHollow.FHM.Shared.Classes;

/// <summary>
///     Class that is used to read from a device in Linux using native system calls
/// </summary>
public class LinuxNativeStreamReader : Stream
{
    /// <summary>
    ///     Enum used to indicate which type of device is the source of this stream
    /// </summary>
    public enum StreamSourceType
    {
        /// <summary>
        ///     This stream reader points to a file
        /// </summary>
        File = 0,

        /// <summary>
        ///     This stream reader points to a disc
        /// </summary>
        Disk = 1,

        /// <summary>
        ///     This stream reader points to a tape
        /// </summary>
        Tape = 2
    }

    private string _devicePath = null;
    private int _handle = -1;
    // private bool _eofReached = false;
    private ulong _position = 0;
    private StreamSourceType _type;
    private long _length = -1;

    /// <summary>
    ///     Flag indicating if this stream can be read
    /// </summary>
    public override bool CanRead => true;

    /// <summary>
    ///     Flag indicating if this stream can seek
    /// </summary>
    public override bool CanSeek => (_type == StreamSourceType.Disk || _type == StreamSourceType.File);

    /// <summary>
    ///     Flag indicating if this stream can be written to
    /// </summary>
    public override bool CanWrite => false;

    /// <summary>
    ///     Number of bytes of this stream
    /// </summary>
    public override long Length => _length >= 0 ? _length : throw new NotSupportedException();

    /// <summary>
    ///     Current position within this stream
    /// </summary>
    public override long Position
    {
        get => (long)_position;
        set => throw new NotImplementedException();
    }

    /// <summary>
    ///     Create a new instance of the stream reader that points to an optical drive device
    /// </summary>
    /// <param name="drive">Optical drive to open</param>
    public LinuxNativeStreamReader(OpticalDrive drive)
    {
        _type = StreamSourceType.Disk;
        _devicePath = drive.FullPath;

        Init();
    }

    /// <summary>
    ///     Private method to initialize the stream
    /// </summary>
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

    /// <summary>
    ///     Read bytes from the stream into the provided buffer
    /// </summary>
    /// <param name="buffer">Buffer to store data in</param>
    /// <param name="offset">Offset to use when writing to the buffer</param>
    /// <param name="count">Number of bytes to read</param>
    /// <returns>Number of bytes that was read from the stream. 0 indicates the end of the stream was reached</returns>
    public override int Read(byte[] buffer, int offset, int count)
    {
        int result = Linux.Read(_handle, buffer, count);

        _position += (ulong)result;

        if (result < 0)
            throw new NativeMethodException("Read");

        // if (result == 0)
        //     _eofReached = true;

        return result;
    }

    /// <summary>
    ///     Close the stream and release any unmanaged resources associated with it
    /// </summary>
    public override void Close()
    {
        base.Close();

        if (_handle >= 0)
            Linux.Close(_handle);
    }

    /// <summary>
    ///     Seek to the specified position in the stream.
    ///    !! not implemented !!
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="origin"></param>
    /// <returns>new position</returns>
    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotImplementedException();

    /// <summary>
    ///     Not supported
    /// </summary>
    public override void Flush()
        => new NotSupportedException();

    /// <summary>
    ///     Not supported
    /// </summary>
    public override void SetLength(long value)
        => throw new NotSupportedException();

    /// <summary>
    ///     Not supported
    /// </summary>
    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();
}