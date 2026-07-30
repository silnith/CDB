using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Silnith.CDB.FileSystem.Visitor;
using System;
using System.Collections.Generic;
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

    private void WriteFile(ICDBFileIdentifier identifier, Stream content)
    {
        string fullPath = Path.Combine(CdbRoot.FullName, identifier.RelativePath, identifier.Filename);
        logger.LogTrace("Writing: {File}", new FileInfo(fullPath));
        _ = Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        FileStreamOptions options = new()
        {
            Mode = FileMode.Create,
            Access = FileAccess.Write,
            Share = FileShare.None,
            PreallocationSize = content.Length,
            Options = FileOptions.SequentialScan | FileOptions.Asynchronous,
        };
        using FileStream fileStream = new(fullPath, options);
        content.CopyTo(fileStream);
    }

    private async Task WriteFileAsync(ICDBFileIdentifier identifier, Stream content, CancellationToken cancellationToken)
    {
        string fullPath = Path.Combine(CdbRoot.FullName, identifier.RelativePath, identifier.Filename);
        logger.LogTrace("Writing: {File}", new FileInfo(fullPath));
        _ = Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        FileStreamOptions options = new()
        {
            Mode = FileMode.Create,
            Access = FileAccess.Write,
            Share = FileShare.None,
            PreallocationSize = content.Length,
            Options = FileOptions.SequentialScan | FileOptions.Asynchronous,
        };
        await using FileStream fileStream = new(fullPath, options);
        await content.CopyToAsync(fileStream, cancellationToken);
    }

    private void WriteArchivedFileEntry(ICDBArchivedIdentifier archivedIdentifier, Stream content)
    {
        ICDBFileIdentifier archiveIdentifier = archivedIdentifier.ArchiveIdentifier;
        string fullPath = Path.Combine(CdbRoot.FullName, archiveIdentifier.RelativePath, archiveIdentifier.Filename);
        logger.LogTrace("Opening archive: {File}", new FileInfo(fullPath));
        _ = Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        using ZipArchive zipArchive = ZipFile.Open(fullPath, ZipArchiveMode.Update);
        ZipArchiveEntry zipArchiveEntry = zipArchive.GetEntry(archivedIdentifier.EntryName)
            ?? zipArchive.CreateEntry(archivedIdentifier.EntryName);
        logger.LogTrace("Writing entry: {Entry}", archivedIdentifier.EntryName);
        using Stream stream = zipArchiveEntry.Open();
        stream.SetLength(0);
        content.CopyTo(stream);
    }

    private async Task WriteArchivedFileEntryAsync(ICDBArchivedIdentifier archivedIdentifier, Stream content, CancellationToken cancellationToken)
    {
        ICDBFileIdentifier archiveIdentifier = archivedIdentifier.ArchiveIdentifier;
        string fullPath = Path.Combine(CdbRoot.FullName, archiveIdentifier.RelativePath, archiveIdentifier.Filename);
        logger.LogTrace("Opening archive: {File}", new FileInfo(fullPath));
        _ = Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        using ZipArchive zipArchive = ZipFile.Open(fullPath, ZipArchiveMode.Update);
        ZipArchiveEntry zipArchiveEntry = zipArchive.GetEntry(archivedIdentifier.EntryName)
            ?? zipArchive.CreateEntry(archivedIdentifier.EntryName);
        logger.LogTrace("Writing entry: {Entry}", archivedIdentifier.EntryName);
        await using Stream stream = zipArchiveEntry.Open();
        stream.SetLength(0);
        await content.CopyToAsync(stream, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteMetadata(Metadata metadata, Stream content)
    {
        WriteFile(metadata, content);
    }

    /// <inheritdoc/>
    public Task WriteMetadataAsync(Metadata metadata, Stream content, CancellationToken cancellationToken)
    {
        return WriteFileAsync(metadata, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTexture(Texture texture, Stream content)
    {
        WriteFile(texture, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureAsync(Texture texture, Stream content, CancellationToken cancellationToken)
    {
        return WriteFileAsync(texture, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTextureLevelOfDetail(TextureLod textureLod, Stream content)
    {
        WriteFile(textureLod, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureLevelOfDetailAsync(TextureLod textureLod, Stream content, CancellationToken cancellationToken)
    {
        return WriteFileAsync(textureLod, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteGeotypicalModel(GeotypicalModel geotypicalModel, Stream content)
    {
        WriteFile(geotypicalModel, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelAsync(GeotypicalModel geotypicalModel, Stream content, CancellationToken cancellationToken)
    {
        return WriteFileAsync(geotypicalModel, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod, Stream content)
    {
        WriteFile(geotypicalModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, Stream content, CancellationToken cancellationToken)
    {
        return WriteFileAsync(geotypicalModelLod, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteMovingModel(MovingModel movingModel, Stream content)
    {
        WriteFile(movingModel, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelAsync(MovingModel movingModel, Stream content, CancellationToken cancellationToken)
    {
        return WriteFileAsync(movingModel, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteMovingModelLevelOfDetail(MovingModelLod movingModelLod, Stream content)
    {
        WriteFile(movingModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, Stream content, CancellationToken cancellationToken)
    {
        return WriteFileAsync(movingModelLod, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTile(Tile tile, Stream content)
    {
        WriteFile(tile, content);
    }

    /// <inheritdoc/>
    public Task WriteTileAsync(Tile tile, Stream content, CancellationToken cancellationToken)
    {
        return WriteFileAsync(tile, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTileFeature(TileArchivedFeature tileFeature, Stream content)
    {
        WriteArchivedFileEntry(tileFeature, content);
    }

    /// <inheritdoc/>
    public Task WriteTileFeatureAsync(TileArchivedFeature tileFeature, Stream content, CancellationToken cancellationToken)
    {
        return WriteArchivedFileEntryAsync(tileFeature, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTileTexture(TileArchivedTexture tileTexture, Stream content)
    {
        WriteArchivedFileEntry(tileTexture, content);
    }

    /// <inheritdoc/>
    public Task WriteTileTextureAsync(TileArchivedTexture tileTexture, Stream content, CancellationToken cancellationToken)
    {
        return WriteArchivedFileEntryAsync(tileTexture, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteNavigation(Navigation navigation, Stream content)
    {
        WriteFile(navigation, content);
    }

    /// <inheritdoc/>
    public Task WriteNavigationAsync(Navigation navigation, Stream content, CancellationToken cancellationToken)
    {
        return WriteFileAsync(navigation, content, cancellationToken);
    }

    /// <summary>
    /// Enumerates all recognized files in a CDB.
    /// </summary>
    /// <returns>An enumeration of all recognized files.</returns>
    public IEnumerable<(ICDBIdentifier, Stream)> EnumerateFiles()
    {
        logger.LogTrace("Walking Metadata for {CDB}", CdbRoot);
        foreach ((ICDBIdentifier, Stream) tuple in metadataVisitor.EnumerateFiles(CdbRoot))
        {
            yield return tuple;
        }
        logger.LogTrace("Walking GTModel for {CDB}", CdbRoot);
        foreach ((ICDBIdentifier, Stream) tuple in gtModelVisitor.EnumerateFiles(CdbRoot))
        {
            yield return tuple;
        }
        logger.LogTrace("Walking MModel for {CDB}", CdbRoot);
        foreach ((ICDBIdentifier, Stream) tuple in movingModelVisitor.EnumerateFiles(CdbRoot))
        {
            yield return tuple;
        }
        logger.LogTrace("Walking Tiles for {CDB}", CdbRoot);
        foreach ((ICDBIdentifier, Stream) tuple in tiledDatasetVisitor.EnumerateFiles(CdbRoot))
        {
            yield return tuple;
        }
        logger.LogTrace("Walking Navigation for {CDB}", CdbRoot);
        foreach ((ICDBIdentifier, Stream) tuple in navigationVisitor.EnumerateFiles(CdbRoot))
        {
            yield return tuple;
        }
        logger.LogTrace("Finished walking CDB data store {CDB}", CdbRoot);
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
    protected virtual ValueTask DisposeAsyncCore()
    {
        return ValueTask.CompletedTask;
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
