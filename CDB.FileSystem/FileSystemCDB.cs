using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Silnith.CDB.FileSystem.Visitor;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Silnith.CDB.FileSystem;

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

    private readonly MetadataVisitor metadataVisitor;
    private readonly GeotypicalModelVisitor gtModelVisitor;
    private readonly MovingModelVisitor movingModelVisitor;
    private readonly TiledDatasetVisitor tiledDatasetVisitor;
    private readonly NavigationVisitor navigationVisitor;

    /// <summary>
    /// Creates a new data store that reads from the specified directory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum legal directory structure underneath <see cref="FileSystemCDBSettings.Root"/>
    /// is <c>Metadata/Version.xml</c>.
    /// </para>
    /// </remarks>
    /// <param name="logger">A logger.</param>
    /// <param name="metadataVisitor"></param>
    /// <param name="gtModelVisitor"></param>
    /// <param name="movingModelVisitor"></param>
    /// <param name="tiledDatasetVisitor"></param>
    /// <param name="navigationVisitor"></param>
    /// <param name="options">Configurable settings.</param>
    /// 
    /// 
    public FileSystemCDB(ILogger<FileSystemCDB> logger,
        MetadataVisitor metadataVisitor,
        GeotypicalModelVisitor gtModelVisitor,
        MovingModelVisitor movingModelVisitor,
        TiledDatasetVisitor tiledDatasetVisitor,
        NavigationVisitor navigationVisitor,
        IOptions<FileSystemCDBSettings> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(metadataVisitor);
        ArgumentNullException.ThrowIfNull(gtModelVisitor);
        ArgumentNullException.ThrowIfNull(movingModelVisitor);
        ArgumentNullException.ThrowIfNull(tiledDatasetVisitor);
        ArgumentNullException.ThrowIfNull(navigationVisitor);
        ArgumentNullException.ThrowIfNull(options);

        this.logger = logger;
        this.metadataVisitor = metadataVisitor;
        this.gtModelVisitor = gtModelVisitor;
        this.movingModelVisitor = movingModelVisitor;
        this.tiledDatasetVisitor = tiledDatasetVisitor;
        this.navigationVisitor = navigationVisitor;
        CdbRoot = options.Value.Root;
    }

    /// <summary>
    /// The root directory of the CDB data store.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Consumers of the data store cannot assume that the files in the CDB
    /// are directly accessible inside of this directory.  Clients must use
    /// the public API to access files.
    /// </para>
    /// </remarks>
    public DirectoryInfo CdbRoot
    {
        get;
    }

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
                Options = FileOptions.SequentialScan | FileOptions.Asynchronous,
                BufferSize = 0,
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
                BufferSize = 0,
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

    /// <summary>
    /// Walks every file in the CDB and executes an action on the file based on
    /// the type of data stored in the file.
    /// </summary>
    public void WalkAllFiles(
        Action<Metadata, FileInfo> processMetadataFile,
        Action<Texture, FileInfo> processTextureFile,
        Action<TextureLod, FileInfo> processTextureLodFile,
        Action<GeotypicalModel, FileInfo> processGeotypicalModelFile,
        Action<GeotypicalModelLod, FileInfo> processGeotypicalModelLodFile,
        Action<MovingModel, FileInfo> processMovingModelFile,
        Action<MovingModelLod, FileInfo> processMovingModelLodFile,
        Action<Tile, FileInfo> processTiledDatasetFile,
        Action<TileArchivedFeature, FileInfo> processTileArchivedFeatureFile,
        Action<TileArchivedTexture, FileInfo> processTileArchivedTextureFile,
        Action<Navigation, FileInfo> processNavigationFile)
    {
        // Metadata
        {
            metadataVisitor.VisitMetadata(CdbRoot, processMetadataFile);
        }
        // GTModel
        {
            gtModelVisitor.VisitGeotypicalModels(CdbRoot,
                processGeotypicalModelFile,
                processGeotypicalModelLodFile,
                processTextureFile,
                processTextureLodFile);
        }
        // MModel
        {
            movingModelVisitor.VisitMovingModels(CdbRoot,
                processMovingModelFile,
                processMovingModelLodFile,
                processTextureFile,
                processTextureLodFile);
        }
        // Tiles
        {
            tiledDatasetVisitor.VisitTiles(CdbRoot, processTiledDatasetFile);
        }
        // Navigation
        {
            navigationVisitor.VisitNavigationDatasets(CdbRoot, processNavigationFile);
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
