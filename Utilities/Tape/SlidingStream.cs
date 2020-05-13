using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Archiver.Utilities.Tape
{
    public class SlidingStream : Stream
    {
        #region Other stream member implementations
        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion Other stream member implementations

        private const uint DefaultBufferSize = (1024 * 1024 * 256); // 256 MB

        public SlidingStream()
        {
            ReadTimeout = -1;
            _bufferFilled = false;
        }

        public SlidingStream(ulong BufferSize): this()
        {
            _bufferMaxSize = BufferSize;
        }

        private bool _bufferFilled = false;
        private ulong _currentBufferSize = 0;
        private ulong _bufferMaxSize = DefaultBufferSize;
        private readonly object _sizeSyncRoot = new object();
        private readonly object _writeSyncRoot = new object();
        private readonly object _readSyncRoot = new object();
        private readonly LinkedList<ArraySegment<byte>> _pendingSegments = new LinkedList<ArraySegment<byte>>();
        private readonly ManualResetEventSlim _dataAvailableResetEvent = new ManualResetEventSlim();

        public override int ReadTimeout { get; set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_dataAvailableResetEvent.Wait(ReadTimeout))
                throw new TimeoutException("No data available");

            lock (_readSyncRoot)
            {
                int currentCount = 0;
                int currentOffset = 0;

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

                    if (_pendingSegments.Count == 0)
                    {
                        _dataAvailableResetEvent.Reset();

                        return currentCount;
                    }
                }

                return currentCount;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_writeSyncRoot)
            {
                byte[] copy = new byte[count];
                Array.Copy(buffer, offset, copy, 0, count);

                _pendingSegments.AddLast(new ArraySegment<byte>(copy));

                // we wait until the buffer fills before we open it up for reading
                lock (_sizeSyncRoot)
                {
                    _currentBufferSize += (uint)count;

                    if (!_bufferFilled && _currentBufferSize >= _bufferMaxSize)
                    {
                        _bufferFilled = true;
                        _dataAvailableResetEvent.Set();
                    }
                }
            }   
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}