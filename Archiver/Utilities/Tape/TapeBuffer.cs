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
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FoxHollow.Archiver.Utilities.Tape
{
    public class TapeBufferStatus
    {
        public bool CanWrite { get; set; }
        public bool CanRead { get; set; }
        public bool InputComplete { get; set; }
        public long BytesWritten { get; set; }
        public long BytesRead { get; set; }
        public double CurrentBufferPercent { get; set; }
        public uint BlocksFull { get; set; }
    }

    public class TapeBuffer : Stream
    {
        #region Constants
        private const uint DefaultStartPercent = 98;
        private const uint DefaultBlockSize = (64 * 1024); // 64 KB blocks, default on most tapes;
        private const uint DefaultBufferCount = 4096; // 64KB * 4096 = 256MiB buffer

        #endregion Constants

        #region Private fields
        private volatile bool _bufferActive = false;
        private volatile bool _endOfStream = false;
        private volatile bool _inputComplete = false;

        private readonly uint _bufferBlockSize;
        private readonly uint _bufferMaxCount;
        private readonly uint _startPercent;


        private volatile uint _bufferReadPointer = 0;
        private volatile uint _bufferWritePointer = 0;

        private long _bytesRead = 0;
        private long _bytesWritten = 0;


        private readonly object _sizeSyncRoot = new object();
        private readonly object _bufferSync = new object();

        private readonly ManualResetEventSlim _enableReads = new ManualResetEventSlim();
        private readonly ManualResetEventSlim _enableWrites = new ManualResetEventSlim();
        private byte[][] _memoryBuffer;
        #endregion Private Fields

        #region Public Properties
        public TapeBufferStatus GetStatus()
        {
            TapeBufferStatus status = new TapeBufferStatus();

            lock (_sizeSyncRoot)
            {
                status.CanWrite = this.CanWrite;
                status.CanRead = this.CanRead;
                status.InputComplete = this.InputComplete;
                status.BytesWritten = this.BytesWritten;
                status.BytesRead = this.BytesRead;
                status.CurrentBufferPercent = this.CurrentBufferPercent;
                status.BlocksFull = this.BlocksFull;
            }

            return status;
        }
        public bool EndOfStream { 
            get
            {
                return _endOfStream;
            }
        }

        public bool InputComplete {
            get
            {
                return _inputComplete;
            }
        }

        public double CurrentBufferPercent {
            get
            {
                return ((double)this.BlocksFull / (double)_bufferMaxCount) * 100.0;
            }
        }

        public uint BlocksFull {
            get
            {
                lock (_sizeSyncRoot)
                {
                    if (_bufferActive == false)
                        return 0;
                    else
                    {
                        if (this.BufferFull)
                            return (uint)_memoryBuffer.Length;
                        else
                        {
                            if (_bufferWritePointer < _bufferReadPointer)
                                return (uint)(_memoryBuffer.Length-_bufferReadPointer)+_bufferWritePointer;
                            else
                                return (uint)_bufferWritePointer-_bufferReadPointer;
                        }
                    }
                }
            }
        }

        public bool BufferActive
        {
            get
            {
                lock (_sizeSyncRoot)
                {
                    return _bufferActive;
                }
            }
        }

        public long BytesRead
        {
            get
            {
                lock (_sizeSyncRoot)
                {
                    return _bytesRead;
                }
            }
        }

        public long BytesWritten
        {
            get
            {
                lock (_sizeSyncRoot)
                {
                    return _bytesWritten;
                }
            }
        }

        public bool BufferFull
        {
            get
            {
                lock (_sizeSyncRoot)
                {
                    return (_bufferActive && _bufferWritePointer == _bufferReadPointer);
                }
            }
        }

        public override bool CanRead
        {
            get
            {
                return _enableReads.IsSet;
            }
        }

        public override bool CanSeek {
            get {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return _enableWrites.IsSet;
            }
        }

        public override long Length
        {
            get
            {
                lock (_sizeSyncRoot)
                {
                    return (long)this.BlocksFull * (long)_bufferBlockSize;
                }
            }
        }

        public override long Position 
        { 
            get => throw new InvalidOperationException(); 
            set => throw new InvalidOperationException(); 
        }
        #endregion Public Properties

        #region Constructor
        public TapeBuffer(uint BlockSize, uint BufferCount)
        {
            _bufferBlockSize = BlockSize;
            _bufferMaxCount = BufferCount;
            _startPercent = DefaultStartPercent;

            Initialize();
        }

        public TapeBuffer(uint BlockSize, uint BufferCount, uint StartPercent)
        {
            _bufferBlockSize = BlockSize;
            _bufferMaxCount = BufferCount;
            _startPercent = StartPercent;

            Initialize();
        }

        private void Initialize()
        {
            _enableWrites.Set();
            _enableReads.Reset();

            // lets pre-allocate the memory
            _memoryBuffer = new byte[_bufferMaxCount][];

            for (int i = 0; i < _memoryBuffer.Length; i++)
            {
                _memoryBuffer[i] = new byte[_bufferBlockSize];
                Array.Fill(_memoryBuffer[i], (byte)0);
            }
        }
        #endregion Constructor

        #region Public Methods
        public int ReadBlock(byte[] buffer)
        {
            _enableReads.Wait();

            lock (_bufferSync)
            {
                Array.Copy(_memoryBuffer[_bufferReadPointer], buffer, _bufferBlockSize);

                _bufferReadPointer++;

                if (_bufferReadPointer >= _memoryBuffer.Length)
                    _bufferReadPointer = 0;

                // if our read pointer matches out write pointer after an increment, we need
                // to stop reading until more data is written. This means the buffer is empty
                // we need to shut down the stream until the buffer refills sufficiently
                if (_bufferReadPointer == _bufferWritePointer)
                {
                    _bufferActive = false;
                    _enableReads.Reset();
                }

                _enableWrites.Set();
            }

            lock (_sizeSyncRoot)
            {
                _bytesRead += _bufferBlockSize;

                // our buffer ran empty
                if (this.BlocksFull == 0)
                {
                    _enableWrites.Set();
                    _enableReads.Reset();

                    // buffer is empty and we reached the end of the stream
                    if (_inputComplete == true)
                    {
                        _endOfStream = true;
                        return -1;
                    }
                }
            }

            return (int)_bufferBlockSize;
        }


        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Must use ReadBlock method instead");
        }

        public void WriteBlock(byte[] buffer)
        {
            if (_inputComplete)
                throw new EndOfStreamException("Input has already been closed, no more data can be accepted");

            _bufferActive = true;

            // make sure writes are enabled. if disabled, that means the buffer is full
            // and cannot hold any more
            _enableWrites.Wait();

            lock (_bufferSync)
            {
                Array.Copy(buffer, _memoryBuffer[_bufferWritePointer], (int)_bufferBlockSize);

                _bufferWritePointer++;

                if (_bufferWritePointer >= _memoryBuffer.Length)
                    _bufferWritePointer = 0;
            }

            // we wait until the buffer fills sufficiently before we open it up for reading
            lock (_sizeSyncRoot)
            {
                _bytesWritten += _bufferBlockSize;

                // looks like we circled around and our write pointer is even with our
                // read pointer. this means the buffer is full and we must stop additional
                // writes until more data is read from the buffer
                if (this.BufferFull)
                {
                    _enableWrites.Reset();

                    // enable reads
                    // this shouldn't be necessary, since the blow code should catch it
                    // but it is a safeguard in case the buffer is completely filled
                    _enableReads.Set();
                }
                else
                {
                    // we filled the buffer enough, enable reads
                    if (this.CanRead == false && this.CurrentBufferPercent > _startPercent)
                        _enableReads.Set();
                }
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset > 0)
                throw new InvalidOperationException("Only allowed to write entire blocks from beginning");

            if (count != (int)_bufferBlockSize)
                throw new InvalidOperationException("Must write entire block at one time");

            WriteBlock(buffer);
        }

        public void InputStreamComplete()
        {
            lock (_sizeSyncRoot)
            {
                if (_inputComplete == false)
                {
                    _inputComplete = true;
                    
                    _enableReads.Set();
                }
            }
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public new void Dispose()
        {
            for (int i = 0; i < _memoryBuffer.Length; i++)
            {
                _memoryBuffer[i] = new byte[] { };
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        #endregion Public Methods
    }
}