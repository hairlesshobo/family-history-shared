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
using System.Threading;
using System.Threading.Tasks;

namespace FoxHollow.FHM.Shared.Classes;

/// <summary>
///     This stream is a memory stream that loads from a file, but
///     it does so in a lazy manner. This means that the stream is only
///     populated upon the first attempt to read from the stream.
/// </summary>
public class LazyMemoryStream : MemoryStream
{
    private FileInfo _fileInfo;
    private bool _fileRead;

    /// <summary>
    ///     Initialize a new instance of the lazy memory stream, loading
    ///     data from the provided file info
    /// </summary>
    /// <param name="fileInfo">FileInfo object that points to the file to load</param>
    public LazyMemoryStream(FileInfo fileInfo)
    {
        _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
        this.SetLength(_fileInfo.Length);
    }

    /// <inheritdoc />
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
        LoadFromFile();

        return base.BeginRead(buffer, offset, count, callback, state);
    }

    /// <inheritdoc />
    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override void EndWrite(IAsyncResult asyncResult)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        LoadFromFile();

        return base.Read(buffer, offset, count);
    }

    /// <inheritdoc />
    public override int Read(Span<byte> buffer)
    {
        LoadFromFile();

        return base.Read(buffer);
    }

    /// <inheritdoc />
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        LoadFromFile();

        return base.ReadAsync(buffer, offset, count, cancellationToken);
    }

    /// <inheritdoc />
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        LoadFromFile();

        return base.ReadAsync(buffer, cancellationToken);
    }

    /// <inheritdoc />
    public override int ReadByte()
    {
        LoadFromFile();

        return base.ReadByte();
    }

    /// <inheritdoc />
    public override bool TryGetBuffer(out ArraySegment<byte> buffer)
    {
        this.LoadFromFile();
        return base.TryGetBuffer(out buffer);
    }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override void WriteByte(byte value)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override void WriteTo(Stream stream)
    {
        LoadFromFile();

        base.WriteTo(stream);
    }

    /// <summary>
    ///     This should be called from any method that reads from the stream
    ///     Its purpose is to populate the memory stream with the contents of
    ///     the file. This is the "lazy" aspect of this stream in that it
    ///     only reads the file if something actually tries to read the stream.
    /// </summary>
    public void LoadFromFile()
    {
        if (!_fileRead)
        {
            using (FileStream stream = new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                byte[] block = new byte[1024 * 1024];
                int bytesRead = 0;

                while ((bytesRead = stream.Read(block, 0, block.Length)) != 0)
                    base.Write(block, 0, bytesRead);

                // rewind the memory stream to the beginning after populating from the file
                this.Seek(0, SeekOrigin.Begin);
            }

            _fileRead = true;
        }
    }
}