using Microsoft.Extensions.Logging;
using Silnith.CDB;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CDB.FileSystem;

/// <summary>
/// A CDB data store that reads directly from the filesystem.
/// </summary>
/// <remarks>
/// <para>
/// This is a classic CDB implementation as described in the OGC CDB standard.
/// </para>
/// </remarks>
public class FileSystemCDB : ICDB
{
    private readonly ILogger<FileSystemCDB> logger;

    /// <summary>
    /// Creates a new data store that reads from the specified directory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum legal directory structure underneath <paramref name="cdbRoot"/>
    /// is <c>Metadata/Version.xml</c>.
    /// </para>
    /// </remarks>
    /// <param name="logger">A logger.</param>
    /// <param name="name">A name for the data store.  This can be any string,
    /// but short, simple values are typical.</param>
    /// <param name="cdbRoot">The CDB root directory.  In the CDB standard, this
    /// is usually referred to as a directory named "CDB".  In practice, it can
    /// have any name, but it should contain subdirectories like "Metadata" as
    /// described in the standard.</param>
    public FileSystemCDB(ILogger<FileSystemCDB> logger, string name, DirectoryInfo cdbRoot)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(cdbRoot);

        this.logger = logger;
        Name = name;
        CdbRoot = cdbRoot;
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public DirectoryInfo CdbRoot { get; }

    /// <inheritdoc/>
    public bool TryReadFile(string filePathAndName, Action<Stream> fileFoundAction)
    {
        FileInfo file = new(Path.Combine(CdbRoot.FullName, filePathAndName));
        if (file.Exists)
        {
            logger.LogTrace("Found: {File}", file);
            FileStreamOptions options = new()
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.Read,
                Options = FileOptions.SequentialScan,
            };
            using DoubleBufferedStream doubleBufferedStream = new(new FileStream(file.FullName, options));
            fileFoundAction(doubleBufferedStream);
            return true;
        }
        else
        {
            logger.LogTrace("Not found: {File}", file);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> TryReadFileAsync(string filePathAndName,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        FileInfo file = new(Path.Combine(CdbRoot.FullName, filePathAndName));
        if (file.Exists)
        {
            logger.LogTrace("Found: {File}", file);
            FileStreamOptions options = new()
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.Read,
                Options = FileOptions.SequentialScan | FileOptions.Asynchronous,
            };
            await using DoubleBufferedStream doubleBufferedStream = new(new FileStream(file.FullName, options));
            await fileFoundAsyncAction(doubleBufferedStream, cancellationToken);
            return true;
        }
        else
        {
            logger.LogTrace("Not found: {File}", file);
            return false;
        }
    }

    #region Dispose Pattern

    private bool disposedValue;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting
    /// unmanaged resources.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> if the call came from a
    /// <see cref="IDisposable.Dispose"/> or <see cref="IAsyncDisposable.DisposeAsync"/> method,
    /// <see langword="false"/> if it came from a finalizer.</param>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose"/>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
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

}
