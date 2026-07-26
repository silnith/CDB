using System;
using System.IO;

namespace Silnith.CDB;

/// <summary>
/// A trivial wrapper for a stream returned by a disposable object.
/// When this stream is disposed, it will also dispose the disposable objects
/// that produced it.
/// </summary>
/// <remarks>
/// <para>
/// This is necessary for accessing resources that require intermediate
/// resources be created in order to read them.  (i.e. database
/// connections, prepared statements, file archives, etc.)
/// </para>
/// </remarks>
public class WrappedStream : Stream
{
    private readonly Stream stream;
    private readonly IDisposable[] disposables;

    /// <summary>
    /// Creates a wrapper for a stream that will dispose of the associated
    /// objects when the stream is disposed.
    /// </summary>
    /// <param name="stream">The stream to wrap.</param>
    /// <param name="disposables">Any additional objects that should be disposed
    /// when the stream is disposed.</param>
    public WrappedStream(Stream stream, params IDisposable[] disposables)
    {
        this.stream = stream;
        this.disposables = disposables;
    }

    /// <inheritdoc/>
    public override bool CanRead => stream.CanRead;

    /// <inheritdoc/>
    public override bool CanSeek => stream.CanSeek;

    /// <inheritdoc/>
    public override bool CanWrite => stream.CanWrite;

    /// <inheritdoc/>
    public override long Length => stream.Length;

    /// <inheritdoc/>
    public override long Position
    {
        get
        {
            return stream.Position;
        }
        set
        {
            stream.Position = value;
        }
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        stream.Flush();
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        return stream.Read(buffer, offset, count);
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        return stream.Seek(offset, origin);
    }

    /// <inheritdoc/>
    public override void SetLength(long value)
    {
        stream.SetLength(value);
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        stream.Write(buffer, offset, count);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            stream.Dispose();
            foreach (IDisposable disposable in disposables)
            {
                disposable.Dispose();
            }
        }

        base.Dispose(disposing);
    }
}
