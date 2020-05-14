using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Archiver.Utilities.Tape
{
    public class SlidingStream : Stream
    {
        #region Other stream member implementations
        public override bool CanRead
        {
            get
            {
                return true;
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
                return true;
            }
        }

        public override long Length
        {
            get
            {
                return (long)_currentBufferSize;
            }
        }

        public override long Position 
        { 
            get => throw new InvalidOperationException(); 
            set => throw new InvalidOperationException(); 
        }
        #endregion Other stream member implementations

        private const uint DefaultBufferSize = (1024 * 1024 * 256); // 256 MB
        private const uint DefaultStartPercent = 98;

        public SlidingStream()
        {
            ReadTimeout = -1;
            _bufferFull = false;
            _bufferEmpty = true;
            _bufferActive = false;
            _enableWrites.Set();
            _enableReads.Reset();
        }

        public SlidingStream(ulong BufferSize): this()
        {
            _bufferMaxSize = BufferSize;
        }

        public SlidingStream(ulong BufferSize, uint StartPercent) : this(BufferSize)
        {
            _startPercent = StartPercent;
        }

        private bool _bufferActive = false;
        private bool _bufferFull = false;
        private bool _bufferEmpty = true;
        private bool _endOfStream = false;
        private bool _inputComplete = false;
        private ulong _currentBufferSize = 0;
        private ulong _bufferMaxSize = DefaultBufferSize;
        private uint _startPercent = DefaultStartPercent;
        private readonly object _sizeSyncRoot = new object();
        private readonly object _writeSyncRoot = new object();
        private readonly object _readSyncRoot = new object();

        private readonly LinkedList<ArraySegment<byte>> _pendingSegments = new LinkedList<ArraySegment<byte>>();
        private readonly ManualResetEventSlim _enableReads = new ManualResetEventSlim();
        private readonly ManualResetEventSlim _enableWrites = new ManualResetEventSlim();

        public override int ReadTimeout { get; set; }
        public bool EndOfStream { 
            get
            {
                return _endOfStream;
            }
        }

        public double CurrentBufferPercent {
            get
            {
                return ((double)_bufferMaxSize / (double)_currentBufferSize) * 100.0;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // we want to make sure we do not allow reads until there is data to be read
            if (_currentBufferSize == 0)
                _enableReads.Reset();

            _enableReads.Wait(-1);

            lock (_readSyncRoot)
            {
                int currentCount = 0;
                int currentOffset = 0;

                lock (_sizeSyncRoot)
                {
                    _currentBufferSize -= (ulong)count;
                }

                while (currentCount != count)
                {
                    ArraySegment<byte> segment = _pendingSegments.First.Value;
                    _pendingSegments.RemoveFirst();

                    int index = segment.Offset;
                    for (; index < segment.Count; index++)
                    {
                        if (currentOffset < offset)
                        {
                            currentOffset++;
                        }
                        else
                        {
                            buffer[currentCount] = segment.Array[index];
                            currentCount++;
                        }
                    }

                    if (currentCount == count)
                    {
                        if (index < segment.Offset + segment.Count)
                        {
                            _pendingSegments.AddFirst(new ArraySegment<byte>(segment.Array, index, segment.Offset + segment.Count - index));
                        }
                    }

                    // our buffer ran empty
                    lock (_sizeSyncRoot)
                    {
                        if (_currentBufferSize == 0)
                        {
                            _bufferEmpty = true;
                            _bufferActive = false;
                            _bufferFull = false;
                            _enableWrites.Set();
                            _enableReads.Reset();

                            // buffer is empty and we reached the end of the stream
                            if (_inputComplete == true)
                                _endOfStream = true;
                        }
                    }

                    if (_pendingSegments.Count == 0)
                    {
                        _enableReads.Reset();

                        return currentCount;
                    }
                }

                return currentCount;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_sizeSyncRoot)
            {
                if (_inputComplete)
                    throw new EndOfStreamException("Input has already been closed, no more data can be accepted");

                // if we got here but the buffer is full, lets make sure we don't allow any 
                // more to be written to it
                // if (_bufferFull == true)
                    // _enableWrites.Reset();
            }

            // make sure writes are enabled. if disabled, that means the buffer is full
            // and cannot hold any more
            _enableWrites.Wait(-1);

            lock (_writeSyncRoot)
            {
                byte[] copy = new byte[count];
                Array.Copy(buffer, offset, copy, 0, count);

                _pendingSegments.AddLast(new ArraySegment<byte>(copy));

                // we wait until the buffer fills before we open it up for reading
                lock (_sizeSyncRoot)
                {
                    _bufferEmpty = false;

                    _currentBufferSize += (uint)count;

                    if (_bufferFull == false && _currentBufferSize >= _bufferMaxSize)
                    {
                        _bufferFull = true;
                        _enableWrites.Reset();
                    }

                    // we filled the buffer enough, enable reads
                    if (_bufferActive == false && this.CurrentBufferPercent > _startPercent)
                    {
                        _bufferActive = true;
                        _enableReads.Set();
                    }
                }
            }   
        }

        public void InputStreamComplete()
        {
            lock (_sizeSyncRoot)
            {
                if (_inputComplete == false && _currentBufferSize > 0)
                {
                    _bufferEmpty = false;
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
    }
}