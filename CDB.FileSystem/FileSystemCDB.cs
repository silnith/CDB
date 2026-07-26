using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Silnith.CDB.FileSystem.Visitor;
using System;
using System.IO;
using System.IO.Compression;
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

    private static readonly FileStreamOptions fileStreamOptions = new()
    {
        Mode = FileMode.Open,
        Access = FileAccess.Read,
        Share = FileShare.Read,
        Options = FileOptions.SequentialScan | FileOptions.Asynchronous,
        BufferSize = 0,
    };

    private Stream? ReadFile(ICDBFileIdentifier identifier)
    {
        FileInfo file = new(Path.Combine(CdbRoot.FullName, identifier.RelativePath, identifier.Filename));
        if (file.Exists)
        {
            logger.LogTrace("Found: {File}", file);
            return new DoubleBufferedStream(new FileStream(file.FullName, fileStreamOptions));
        }
        else
        {
            logger.LogTrace("Not found: {File}", file);
            return null;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "No.")]
    private Task<Stream?> ReadFileAsync(ICDBFileIdentifier identifier, CancellationToken cancellationToken)
    {
        FileInfo file = new(Path.Combine(CdbRoot.FullName, identifier.RelativePath, identifier.Filename));
        if (file.Exists)
        {
            logger.LogTrace("Found: {File}", file);
            return Task.FromResult<Stream?>(new DoubleBufferedStream(new FileStream(file.FullName, fileStreamOptions)));
        }
        else
        {
            logger.LogTrace("Not found: {File}", file);
            return Task.FromResult<Stream?>(null);
        }
    }

    private Stream? ReadArchivedFile(ICDBArchivedIdentifier archivedIdentifier)
    {
        Stream? tileStream = ReadFile(archivedIdentifier.ArchiveIdentifier);
        if (tileStream is null)
        {
            return null;
        }
        try
        {
            ZipArchive zipArchive = new(tileStream, ZipArchiveMode.Read);
            try
            {
                ZipArchiveEntry? zipArchiveEntry = zipArchive.GetEntry(archivedIdentifier.EntryName);
                if (zipArchiveEntry is not null)
                {
                    Stream stream = zipArchiveEntry.Open();
                    return new WrappedStream(stream, zipArchive, tileStream);
                }
                else
                {
                    zipArchive.Dispose();
                    tileStream.Dispose();
                    return null;
                }
            }
            catch (Exception)
            {
                zipArchive.Dispose();
                throw;
            }
        }
        catch (Exception)
        {
            tileStream.Dispose();
            throw;
        }
    }

    private async Task<Stream?> ReadArchivedFileAsync(ICDBArchivedIdentifier archivedIdentifier, CancellationToken cancellationToken)
    {
        Stream? tileStream = await ReadFileAsync(archivedIdentifier.ArchiveIdentifier, cancellationToken);
        if (tileStream is null)
        {
            return null;
        }
        try
        {
            ZipArchive zipArchive = new(tileStream, ZipArchiveMode.Read);
            try
            {
                ZipArchiveEntry? zipArchiveEntry = zipArchive.GetEntry(archivedIdentifier.EntryName);
                if (zipArchiveEntry is not null)
                {
                    Stream stream = zipArchiveEntry.Open();
                    return new WrappedStream(stream, zipArchive, tileStream);
                }
                else
                {
                    zipArchive.Dispose();
                    await tileStream.DisposeAsync();
                    return null;
                }
            }
            catch (Exception)
            {
                zipArchive.Dispose();
                throw;
            }
        }
        catch (Exception)
        {
            await tileStream.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc/>
    public Stream? ReadMetadata(Metadata metadata)
    {
        return ReadFile(metadata);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMetadataAsync(Metadata metadata, CancellationToken cancellationToken)
    {
        return ReadFileAsync(metadata, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadTexture(Texture texture)
    {
        return ReadFile(texture);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTextureAsync(Texture texture, CancellationToken cancellationToken)
    {
        return ReadFileAsync(texture, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadTextureLevelOfDetail(TextureLod textureLod)
    {
        return ReadFile(textureLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTextureLevelOfDetailAsync(TextureLod textureLod, CancellationToken cancellationToken)
    {
        return ReadFileAsync(textureLod, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModel(GeotypicalModel geotypicalModel)
    {
        return ReadFile(geotypicalModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadGeotypicalModelAsync(GeotypicalModel geotypicalModel, CancellationToken cancellationToken)
    {
        return ReadFileAsync(geotypicalModel, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod)
    {
        return ReadFile(geotypicalModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, CancellationToken cancellationToken)
    {
        return ReadFileAsync(geotypicalModelLod, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadMovingModel(MovingModel movingModel)
    {
        return ReadFile(movingModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMovingModelAsync(MovingModel movingModel, CancellationToken cancellationToken)
    {
        return ReadFileAsync(movingModel, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadMovingModelLevelOfDetail(MovingModelLod movingModelLod)
    {
        return ReadFile(movingModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, CancellationToken cancellationToken)
    {
        return ReadFileAsync(movingModelLod, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadTile(Tile tile)
    {
        return ReadFile(tile);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileAsync(Tile tile, CancellationToken cancellationToken)
    {
        return ReadFileAsync(tile, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadTileFeature(TileArchivedFeature tileFeature)
    {
        return ReadArchivedFile(tileFeature);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileFeatureAsync(TileArchivedFeature tileFeature, CancellationToken cancellationToken)
    {
        return ReadArchivedFileAsync(tileFeature, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadTileTexture(TileArchivedTexture tileTexture)
    {
        return ReadArchivedFile(tileTexture);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileTextureAsync(TileArchivedTexture tileTexture, CancellationToken cancellationToken)
    {
        return ReadArchivedFileAsync(tileTexture, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadNavigation(Navigation navigation)
    {
        return ReadFile(navigation);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadNavigationAsync(Navigation navigation, CancellationToken cancellationToken)
    {
        return ReadFileAsync(navigation, cancellationToken);
    }

    /// <inheritdoc/>
    public bool TryReadFile(string filePathAndName, Action<Stream> fileFoundAction)
    {
        FileInfo file = new(Path.Combine(CdbRoot.FullName, filePathAndName));
        if (file.Exists)
        {
            logger.LogTrace("Found: {File}", file);
            using DoubleBufferedStream doubleBufferedStream = new(new FileStream(file.FullName, fileStreamOptions));
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
            await using DoubleBufferedStream doubleBufferedStream = new(new FileStream(file.FullName, fileStreamOptions));
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

    #region Async Dispose Pattern

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting
    /// unmanaged resources.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync"/>
    protected virtual async ValueTask DisposeAsyncCore()
    {
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        // Perform async cleanup.
        await DisposeAsyncCore();

        // Dispose of unmanaged resources.
        Dispose(false);

        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    #endregion

}
