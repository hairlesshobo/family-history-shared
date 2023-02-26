using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization.TypeInspectors;

namespace FoxHollow.FHM.Shared.Classes;

public class LazyMemoryStream : MemoryStream
{
    private FileInfo _fileInfo;
    private bool _fileRead;

    public override bool CanTimeout => base.CanTimeout;

    public override int ReadTimeout { get => base.ReadTimeout; set => base.ReadTimeout = value; }
    public override int WriteTimeout { get => base.WriteTimeout; set => base.WriteTimeout = value; }

    public override bool CanRead => base.CanRead;

    public override bool CanSeek => base.CanSeek;

    public override bool CanWrite => base.CanWrite;

    public override int Capacity { get => base.Capacity; set => base.Capacity = value; }

    public override long Length => base.Length;

    public override long Position { get => base.Position; set => base.Position = value; }

    public LazyMemoryStream(FileInfo fileInfo)
    {
        _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
        this.SetLength(_fileInfo.Length);
    }

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
        LoadFromFile();

        return base.BeginRead(buffer, offset, count, callback, state);
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
        throw new NotSupportedException();
    }

    public override void Close()
    {
        base.Close();
    }

    public override void CopyTo(Stream destination, int bufferSize)
    {
        base.CopyTo(destination, bufferSize);
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        return base.CopyToAsync(destination, bufferSize, cancellationToken);
    }

    public override ValueTask DisposeAsync()
    {
        return base.DisposeAsync();
    }

    public override int EndRead(IAsyncResult asyncResult)
    {
        return base.EndRead(asyncResult);
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
        throw new NotSupportedException();
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override void Flush()
    {
        base.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return base.FlushAsync(cancellationToken);
    }

    public override byte[] GetBuffer()
    {
        return base.GetBuffer();
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        LoadFromFile();

        return base.Read(buffer, offset, count);
    }

    public override int Read(Span<byte> buffer)
    {
        LoadFromFile();

        return base.Read(buffer);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        LoadFromFile();

        return base.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        LoadFromFile();

        return base.ReadAsync(buffer, cancellationToken);
    }

    public override int ReadByte()
    {
        LoadFromFile();

        return base.ReadByte();
    }

    public override long Seek(long offset, SeekOrigin loc)
    {
        return base.Seek(offset, loc);
    }

    public override void SetLength(long value)
    {
        base.SetLength(value);
    }

    public override byte[] ToArray()
    {
        return base.ToArray();
    }

    public override string ToString()
    {
        return base.ToString();
    }

    public override bool TryGetBuffer(out ArraySegment<byte> buffer)
    {
        this.LoadFromFile();
        return base.TryGetBuffer(out buffer);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        throw new NotSupportedException();
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public override void WriteByte(byte value)
    {
        throw new NotSupportedException();
    }

    public override void WriteTo(Stream stream)
    {
        LoadFromFile();

        base.WriteTo(stream);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
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