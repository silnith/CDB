using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Silnith.CDB;

/// <summary>
/// A shared interface for individual instances of a CDB data store.
/// </summary>
/// <remarks>
/// <para>
/// This represents one single storage location as described in the OGC CDB standard.
/// Typically this would be a filesystem hierarchy rooted in a single directory.
/// Alternate implementations are possible, however, that can translate the standard
/// file paths and names into keys for other storage mechanisms.
/// </para>
/// <para>
/// A list of CDB versions would consists of multiple instances of this interface.
/// The file replacement mechanism would involve querying multiple instances of
/// this interface.
/// </para>
/// </remarks>
/// <seealso href="https://docs.ogc.org/is/15-113r7/15-113r7.html"/>
public interface ICDB : IDisposable, IAsyncDisposable
{

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="metadata">The identifier for the file to read.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Stream? ReadMetadata(Metadata metadata);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="metadata">The identifier for the file to read.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task<Stream?> ReadMetadataAsync(Metadata metadata, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="texture">The identifier for the file to read.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Stream? ReadTexture(Texture texture);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="texture">The identifier for the file to read.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task<Stream?> ReadTextureAsync(Texture texture, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="textureLod">The identifier for the file to read.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Stream? ReadTextureLevelOfDetail(TextureLod textureLod);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="textureLod">The identifier for the file to read.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task<Stream?> ReadTextureLevelOfDetailAsync(TextureLod textureLod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="geotypicalModel">The identifier for the file to read.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Stream? ReadGeotypicalModel(GeotypicalModel geotypicalModel);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="geotypicalModel">The identifier for the file to read.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task<Stream?> ReadGeotypicalModelAsync(GeotypicalModel geotypicalModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="geotypicalModelLod">The identifier for the file to read.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Stream? ReadGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="geotypicalModelLod">The identifier for the file to read.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task<Stream?> ReadGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="movingModel">The identifier for the file to read.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Stream? ReadMovingModel(MovingModel movingModel);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="movingModel">The identifier for the file to read.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task<Stream?> ReadMovingModelAsync(MovingModel movingModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="movingModelLod">The identifier for the file to read.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Stream? ReadMovingModelLevelOfDetail(MovingModelLod movingModelLod);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="movingModelLod">The identifier for the file to read.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task<Stream?> ReadMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="tile">The identifier for the file to read.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Stream? ReadTile(Tile tile);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="tile">The identifier for the file to read.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task<Stream?> ReadTileAsync(Tile tile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="tileFeature">The identifier for the file to read.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Stream? ReadTileFeature(TileArchivedFeature tileFeature);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="tileFeature">The identifier for the file to read.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task<Stream?> ReadTileFeatureAsync(TileArchivedFeature tileFeature, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="tileTexture">The identifier for the file to read.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Stream? ReadTileTexture(TileArchivedTexture tileTexture);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="tileTexture">The identifier for the file to read.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task<Stream?> ReadTileTextureAsync(TileArchivedTexture tileTexture, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="navigation">The identifier for the file to read.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Stream? ReadNavigation(Navigation navigation);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="navigation">The identifier for the file to read.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task<Stream?> ReadNavigationAsync(Navigation navigation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to read a file out of the CDB.
    /// If the file was found, runs <paramref name="fileFoundAction"/> on its contents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <paramref name="filePathAndName"/> should always begin with one of
    /// the known root directories.  These are:
    /// </para>
    /// <list type="bullet">
    /// <item><term><c>/Metadata/</c></term></item>
    /// <item><term><c>/GTModel/</c></term></item>
    /// <item><term><c>/MModel/</c></term></item>
    /// <item><term><c>/Tiles/</c></term></item>
    /// <item><term><c>/Navigation/</c></term></item>
    /// </list>
    /// </remarks>
    /// <param name="filePathAndName">The relative path and filename of the file to read.
    /// The path should be relative to the CDB root.</param>
    /// <param name="fileFoundAction">The action to run if the file is found.
    /// The stream will be automatically closed after the action returns or
    /// throws an exception.</param>
    /// <returns><see langword="true"/> if the file was found.</returns>
    public bool TryReadFile(string filePathAndName, Action<Stream> fileFoundAction);

    /// <summary>
    /// Tries to read a file out of the CDB.
    /// If the file was found, runs <paramref name="fileFoundAsyncAction"/> on its contents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <paramref name="filePathAndName"/> should always begin with one of
    /// the known root directories.  These are:
    /// </para>
    /// <list type="bullet">
    /// <item><term><c>/Metadata/</c></term></item>
    /// <item><term><c>/GTModel/</c></term></item>
    /// <item><term><c>/MModel/</c></term></item>
    /// <item><term><c>/Tiles/</c></term></item>
    /// <item><term><c>/Navigation/</c></term></item>
    /// </list>
    /// </remarks>
    /// <param name="filePathAndName">The relative path and filename of the file to read.
    /// The path should be relative to the CDB root.</param>
    /// <param name="fileFoundAsyncAction">The action to run if the file is found.
    /// The stream will be automatically closed after the action returns or
    /// throws an exception.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see langword="true"/> if the file was found.</returns>
    public Task<bool> TryReadFileAsync(string filePathAndName,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken);

    /// <summary>
    /// Writes the contents of a stream to the data store.
    /// </summary>
    /// <param name="metadata">The identifier for the file to write.</param>
    /// <param name="content">The new file contents.</param>
    public void WriteMetadata(Metadata metadata, Stream content);

    /// <summary>
    /// Writes the contents of a stream to the data store.
    /// </summary>
    /// <param name="metadata">The identifier for the file to write.</param>
    /// <param name="content">The new file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public Task WriteMetadataAsync(Metadata metadata, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the contents of a stream to the data store.
    /// </summary>
    /// <param name="texture">The identifier for the file to write.</param>
    /// <param name="content">The new file contents.</param>
    public void WriteTexture(Texture texture, Stream content);

    /// <summary>
    /// Writes the contents of a stream to the data store.
    /// </summary>
    /// <param name="texture">The identifier for the file to write.</param>
    /// <param name="content">The new file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public Task WriteTextureAsync(Texture texture, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="textureLod">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public void WriteTextureLevelOfDetail(TextureLod textureLod, Stream content);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="textureLod">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task WriteTextureLevelOfDetailAsync(TextureLod textureLod, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="geotypicalModel">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public void WriteGeotypicalModel(GeotypicalModel geotypicalModel, Stream content);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="geotypicalModel">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task WriteGeotypicalModelAsync(GeotypicalModel geotypicalModel, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="geotypicalModelLod">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public void WriteGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod, Stream content);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="geotypicalModelLod">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task WriteGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="movingModel">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public void WriteMovingModel(MovingModel movingModel, Stream content);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="movingModel">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task WriteMovingModelAsync(MovingModel movingModel, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="movingModelLod">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public void WriteMovingModelLevelOfDetail(MovingModelLod movingModelLod, Stream content);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="movingModelLod">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task WriteMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="tile">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public void WriteTile(Tile tile, Stream content);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="tile">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task WriteTileAsync(Tile tile, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="tileFeature">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public void WriteTileFeature(TileArchivedFeature tileFeature, Stream content);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="tileFeature">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task WriteTileFeatureAsync(TileArchivedFeature tileFeature, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="tileTexture">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public void WriteTileTexture(TileArchivedTexture tileTexture, Stream content);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="tileTexture">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task WriteTileTextureAsync(TileArchivedTexture tileTexture, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="navigation">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public void WriteNavigation(Navigation navigation, Stream content);

    /// <summary>
    /// Returns a stream of the file contents, or <see langword="null"/>.
    /// </summary>
    /// <param name="navigation">The identifier for the file to read.</param>
    /// <param name="content">The new file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream of the file contents, or <see langword="null"/>.</returns>
    public Task WriteNavigationAsync(Navigation navigation, Stream content, CancellationToken cancellationToken = default);

}
