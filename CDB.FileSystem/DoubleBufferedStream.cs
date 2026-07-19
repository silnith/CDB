using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CDB.FileSystem;

/// <summary>
/// A stream that preloads data from another stream.
/// </summary>
/// <remarks>
/// <para>
/// This always keeps two buffers for the underlying stream.
/// One buffer is stable and used to service read requests.
/// The other buffer is reading the next block of data from
/// the underlying stream using the common thread pool.
/// </para>
/// </remarks>
public class DoubleBufferedStream : Stream
{
    private const long BufferSize = 8 * 1024 * 1024;

    /// <summary>
    /// A buffer that stores a view of an underlying stream.
    /// </summary>
    private class ViewBuffer : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Creates a new view buffer using the provided array.
        /// </summary>
        /// <param name="buffer">The array to use as the buffer.</param>
        public ViewBuffer(byte[] buffer)
        {
            Buffer = buffer;
            Stream = new MemoryStream(Buffer, false);
            Offset = 0;
            Length = 0;
        }

        /// <summary>
        /// Creates a new buffer of the requested size.
        /// </summary>
        /// <param name="bufferSize">The size of the buffer in bytes.</param>
        public ViewBuffer(int bufferSize)
            : this(new byte[bufferSize])
        {
        }

        /// <summary>
        /// Creates a new buffer of the requested size.
        /// </summary>
        /// <param name="bufferSize">The size of the buffer in bytes.</param>
        public ViewBuffer(long bufferSize)
            : this(new byte[bufferSize])
        {
        }

        /// <summary>
        /// The buffer in memory.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When filling the buffer, it is easiest to access it directly.
        /// When reading from the buffer, it is easier to use the standard methods
        /// of <see cref="Stream"/>, which wraps this buffer.
        /// </para>
        /// </remarks>
        public byte[] Buffer
        {
            get;
        }

        /// <summary>
        /// A stream wrapping <see cref="Buffer"/> to provide implementations
        /// of all the standard read methods.
        /// </summary>
        public Stream Stream
        {
            get;
        }

        /// <summary>
        /// The position of this view of the underlying stream
        /// relative to the beginning of the overall stream.
        /// </summary>
        public long Offset
        {
            get;
            set;
        }

        /// <summary>
        /// The length of this view of the underlying stream.
        /// </summary>
        public long Length
        {
            get;
            set;
        }

        /// <summary>
        /// Returns <see langword="true"/> if this buffer contains the stream position.
        /// </summary>
        /// <param name="position">The position in the underlying stream.</param>
        /// <returns><see langword="true"/> if this buffer contains the requested position.</returns>
        public bool Contains(long position)
        {
            return position >= Offset
                && position < Offset + Length;
        }

        /// <summary>
        /// Returns <see langword="true"/> if this buffer could potentially contain
        /// the stream position after it finishes loading data from the underlying
        /// stream.
        /// </summary>
        /// <param name="position">The position in the underlying stream.</param>
        /// <returns><see langword="true"/> if this buffer could potentially contain the requested position.</returns>
        public bool CouldContain(long position)
        {
            return position >= Offset
                && position < Offset + Buffer.Length;
        }

        #region Dispose Pattern

        private bool disposedValue;

        /// <summary>
        /// The actual dispose method that releases resources.
        /// Managed resources will only be released if <paramref name="disposing"/>
        /// is <see langword="true"/>.  Unmanaged resources will be released
        /// unconditionally.
        /// </summary>
        /// <param name="disposing">Whether to release managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stream.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Async Dispose Pattern

        /// <summary>
        /// Disposes of managed resources.
        /// </summary>
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync"/>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            await Stream.DisposeAsync();
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            // Perform async cleanup.
            await DisposeAsyncCore();

            // Dispose of unmanaged resources.
            Dispose(disposing: false);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        #endregion

    }

    /// <summary>
    /// The wrapped stream.
    /// </summary>
    private readonly Stream underlyingStream;

    /// <summary>
    /// The buffer that has data in it ready to be returned.
    /// </summary>
    private ViewBuffer readBuffer;

    /// <summary>
    /// The buffer that is currently receiving data from the underlying stream
    /// in a thread pool task.
    /// </summary>
    private ViewBuffer writeBuffer;

    /// <summary>
    /// A cancellation source for cancelling the thread pool task filling the
    /// <see cref="writeBuffer"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is only needed in the event that the calling code invokes
    /// <see cref="Seek(long, SeekOrigin)"/> to a place completely outside of
    /// the currently buffered data.
    /// </para>
    /// </remarks>
    private CancellationTokenSource writeTaskCancellationSource;

    /// <summary>
    /// The currently executing task that is filling the <see cref="writeBuffer"/>
    /// with data.
    /// </summary>
    private Task fillWriteBufferTask;

    /// <summary>
    /// Creates a new double buffered stream wrapping the provided stream.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This takes ownership of the stream, and disposes of the underlying
    /// stream when this object is disposed.
    /// </para>
    /// </remarks>
    /// <param name="stream">The stream to wrap.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <see langword="null"/>.</exception>
    public DoubleBufferedStream(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        underlyingStream = stream;
        readBuffer = new ViewBuffer(BufferSize);
        writeBuffer = new ViewBuffer(BufferSize);

        /*
         * This should not be necessary.  The call to CreateNewFillWriteBufferTask
         * initializes this member variable.  However, the .NET compiler seems
         * unable to deduce that.
         * 
         * Anyway, this object will be immediately overwritten by the method.
         */
        writeTaskCancellationSource = new();

        Position = 0;
        fillWriteBufferTask = CreateNewFillWriteBufferTask();
    }

    /// <summary>
    /// Creates a new <see cref="CancellationTokenSource"/> to control the new
    /// read request that will fill the write buffer.  Then fires off a read
    /// request to fill the write buffer.  The new task goes into the common
    /// thread pool.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Since this is designed to launch something concurrently, calling <c>await</c>
    /// on it only makes sense right before actually switching the write buffer
    /// to be the read buffer.
    /// </para>
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the <see cref="FillWriteBuffer"/>
    /// call running in the thread pool.</returns>
    private Task CreateNewFillWriteBufferTask()
    {
        writeTaskCancellationSource = new();

        return Task.Run(FillWriteBuffer, writeTaskCancellationSource.Token);
    }

    /// <summary>
    /// Fetches the next block of data from the underlying stream into the
    /// write buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is always run in the common thread pool.
    /// </para>
    /// </remarks>
    /// <seealso cref="CreateNewFillWriteBufferTask"/>
    /// <seealso cref="Task.Run(Func{Task?}, CancellationToken)"/>
    private async Task FillWriteBuffer()
    {
        writeBuffer.Offset = underlyingStream.Position;
        writeBuffer.Stream.Position = 0;
        int bytesRead = await underlyingStream.ReadAsync(writeBuffer.Buffer, writeTaskCancellationSource.Token);
        writeBuffer.Length = bytesRead;
    }

    /// <summary>
    /// Replaces the current read buffer with the current write buffer.
    /// If the write buffer is still being filled, this will block until it is ready.
    /// Afterwards, kicks off a new read request to the underlying stream,
    /// reinitializing the <see cref="fillWriteBufferTask"/>.
    /// </summary>
    private void SwapBuffers()
    {
        fillWriteBufferTask.GetAwaiter().GetResult();
        (writeBuffer, readBuffer) = (readBuffer, writeBuffer);
        fillWriteBufferTask = CreateNewFillWriteBufferTask();
    }

    /// <summary>
    /// Replaces the current read buffer with the current write buffer.
    /// If the write buffer is still being filled, this will block until it is ready.
    /// Afterwards, kicks off a new read request to the underlying stream,
    /// reinitializing the <see cref="fillWriteBufferTask"/>.
    /// </summary>
    private async Task SwapBuffersAsync()
    {
        await fillWriteBufferTask;
        (writeBuffer, readBuffer) = (readBuffer, writeBuffer);
        fillWriteBufferTask = CreateNewFillWriteBufferTask();
    }

    /// <summary>
    /// Panic!  Wipe everything and start over.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This invalidates the current read buffer so no new data will be read
    /// from it, cancels the existing task filling the write buffer, and
    /// creates a new task to fill the write buffer based on the current
    /// position in the stream.
    /// </para>
    /// <para>
    /// A call to <see cref="SwapBuffers"/> will be necessary in order to
    /// actually access any data after this.
    /// </para>
    /// </remarks>
    private void ReinitializeBuffers()
    {
        /*
         * Set the read buffer to contain nothing, thus invalidating it.
         * This prevents any new data being read from it.
         */
        readBuffer.Length = 0;
        /*
         * Gracefully shut down the existing read task.  (We always have one.)
         */
        writeTaskCancellationSource.Cancel();
        try
        {
            fillWriteBufferTask.GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            // This is expected.
        }
        /*
         * Kick off the new read from the underlying data stream.
         * The next call to Read will find that the current read buffer does
         * not contain the current position, and it will call SwapBuffers.
         */
        underlyingStream.Position = Position;
        fillWriteBufferTask = CreateNewFillWriteBufferTask();
    }

    /// <summary>
    /// Panic!  Wipe everything and start over.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This invalidates the current read buffer so no new data will be read
    /// from it, cancels the existing task filling the write buffer, and
    /// creates a new task to fill the write buffer based on the current
    /// position in the stream.
    /// </para>
    /// <para>
    /// A call to <see cref="SwapBuffers"/> will be necessary in order to
    /// actually access any data after this.
    /// </para>
    /// </remarks>
    private async Task ReinitializeBuffersAsync()
    {
        /*
         * Set the read buffer to contain nothing, thus invalidating it.
         * This prevents any new data being read from it.
         */
        readBuffer.Length = 0;
        /*
         * Gracefully shut down the existing read task.  (We always have one.)
         */
        writeTaskCancellationSource.Cancel();
        try
        {
            await fillWriteBufferTask;
        }
        catch (Exception)
        {
            // This is expected.
        }
        /*
         * Kick off the new read from the underlying data stream.
         * The next call to Read will find that the current read buffer does
         * not contain the current position, and it will call SwapBuffers.
         */
        underlyingStream.Position = Position;
        fillWriteBufferTask = CreateNewFillWriteBufferTask();
    }

    /// <inheritdoc/>
    public override bool CanRead => true;

    /// <inheritdoc/>
    public override bool CanSeek => true;

    /// <inheritdoc/>
    public override bool CanWrite => false;

    /// <inheritdoc/>
    public override long Length => underlyingStream.Length;

    /// <inheritdoc/>
    public override long Position
    {
        get;
        set;
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        return Read(buffer.AsSpan(offset, count));
    }

    /// <inheritdoc/>
    public override int Read(Span<byte> buffer)
    {
        if (Position >= Length)
        {
            /*
             * The underlying stream has been exhausted.
             */
            return 0;
        }

        if (readBuffer.Contains(Position))
        {
            /*
             * We know at least the beginning of the requested data is available in the read buffer.
             * Not entirely sure about the end.
             */
            readBuffer.Stream.Position = Position - readBuffer.Offset;
            int bytesRead = readBuffer.Stream.Read(buffer);

            Position += bytesRead;

            /*
             * No need to check if we exhausted the read buffer, because we always
             * have the write buffer being filled in the background.
             */

            return bytesRead;
        }
        else
        {
            /*
             * We are outside the read buffer.
             * Check if the write buffer is expected to contain the requested data.
             */
            if (writeBuffer.CouldContain(Position))
            {
                /*
                 * The next load from the underlying stream at least requested
                 * that the desired position be included.
                 * Assume that it will succeed and continue to the buffer swap.
                 */
            }
            else
            {
                /*
                 * We do not have the data, and the next load will not provide it.
                 * Scrap everything and start over.
                 */
                ReinitializeBuffers();
            }

            /*
             * The write buffer should contain the requested data, once the load
             * completes.  Swap the buffers, blocking if necessary for the load
             * to complete.
             */
            SwapBuffers();
            /*
             * Recurse.  The next invocation will use the swapped buffers.
             */
            return Read(buffer);
        }
    }

    /// <inheritdoc/>
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return await ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
    }

    /// <inheritdoc/>
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (Position >= Length)
        {
            /*
             * The underlying stream has been exhausted.
             */
            return 0;
        }

        if (readBuffer.Contains(Position))
        {
            /*
             * We know at least the beginning of the requested data is available in the read buffer.
             * Not entirely sure about the end.
             */
            readBuffer.Stream.Position = Position - readBuffer.Offset;
            int bytesRead = await readBuffer.Stream.ReadAsync(buffer, cancellationToken);

            Position += bytesRead;

            /*
             * No need to check if we exhausted the read buffer, because we always
             * have the write buffer being filled in the background.
             */

            return bytesRead;
        }
        else
        {
            /*
             * We are outside the read buffer.
             * Check if the write buffer is expected to contain the requested data.
             */
            if (writeBuffer.CouldContain(Position))
            {
                /*
                 * The next load from the underlying stream at least requested
                 * that the desired position be included.
                 * Assume that it will succeed and continue to the buffer swap.
                 */
            }
            else
            {
                /*
                 * We do not have the data, and the next load will not provide it.
                 * Scrap everything and start over.
                 */
                await ReinitializeBuffersAsync();
            }

            /*
             * The write buffer should contain the requested data, once the load
             * completes.  Swap the buffers, blocking if necessary for the load
             * to complete.
             */
            await SwapBuffersAsync();
            /*
             * Recurse.  The next invocation will use the swapped buffers.
             */
            return await ReadAsync(buffer, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override int ReadByte()
    {
        if (Position >= Length)
        {
            /*
             * The underlying stream has been exhausted.
             */
            return -1;
        }

        if (readBuffer.Contains(Position))
        {
            /*
             * We know there is at least one byte available.
             */
            readBuffer.Stream.Position = Position - readBuffer.Offset;
            int theByte = readBuffer.Stream.ReadByte();

            Position++;

            /*
             * No need to check if we exhausted the read buffer, because we always
             * have the write buffer being filled in the background.
             */

            return theByte;
        }
        else
        {
            /*
             * We are outside the read buffer.
             * Check if the write buffer is expected to contain the requested data.
             */
            if (writeBuffer.CouldContain(Position))
            {
                /*
                 * The next load from the underlying stream at least requested
                 * that the desired position be included.
                 * Assume that it will succeed and continue to the buffer swap.
                 */
            }
            else
            {
                /*
                 * We do not have the data, and the next load will not provide it.
                 * Scrap everything and start over.
                 */
                ReinitializeBuffers();
            }

            /*
             * The write buffer should contain the requested data, once the load
             * completes.  Swap the buffers, blocking if necessary for the load
             * to complete.
             */
            SwapBuffers();
            /*
             * Recurse.  The next invocation will use the swapped buffers.
             */
            return ReadByte();
        }
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        long newPosition = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, "Not a recognized enum value."),
        };

        if (newPosition < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "Attempt to seek before the stream beginning.");
        }
        if (newPosition > Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "Attempt to seek after the stream end.");
        }

        Position = newPosition;

        if (Position == Length)
        {
            /*
             * The stream is exhausted, do not bother checking the status of the
             * read and write buffers.
             */
        }
        else
        {
            if (readBuffer.Contains(Position))
            {
                /*
                 * We're good, do nothing.
                 */
            }
            else
            {
                if (writeBuffer.CouldContain(Position))
                {
                    /*
                     * We will speculatively assume that everything will be good, and do nothing.
                     * The buffer swap and all that jazz will happen on the next Read.
                     */
                }
                else
                {
                    /*
                     * Disaster, they moved completely outside all of our existing work.
                     * Scrap everything and start over.
                     */
                    ReinitializeBuffers();
                }
            }
        }

        return Position;
    }

    /// <inheritdoc/>
    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
    
    /// <inheritdoc/>
    public override void Flush()
    {
        /*
         * We do not support writing, so there will never be anything to flush.
         */
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            writeTaskCancellationSource.Cancel();
            try
            {
                fillWriteBufferTask.GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                /*
                 * We do not care.
                 */
            }

            writeBuffer.Dispose();
            readBuffer.Dispose();

            underlyingStream.Dispose();
        }

        base.Dispose(disposing);
    }

    /*
     * The Stream base class does not implement Microsoft's recommended async disposable pattern.
     */

}
