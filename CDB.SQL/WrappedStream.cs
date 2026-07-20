using System.Data.Common;
using System.IO;

namespace Silnith.CDB.SQL;

/// <summary>
/// A trivial wrapper for a stream returned by a database query.
/// When this stream is disposed, it will also dispose the database objects
/// that produced it.
/// </summary>
public class WrappedStream : Stream
{
    private readonly DbConnection dbConnection;
    private readonly DbCommand dbCommand;
    private readonly DbDataReader dbDataReader;
    private readonly Stream stream;

    public WrappedStream(DbConnection dbConnection, DbCommand dbCommand, DbDataReader dbDataReader, Stream stream)
    {
        this.dbConnection = dbConnection;
        this.dbCommand = dbCommand;
        this.dbDataReader = dbDataReader;
        this.stream = stream;
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
            dbDataReader.Dispose();
            dbCommand.Dispose();
            dbConnection.Dispose();
        }

        base.Dispose(disposing);
    }
}
