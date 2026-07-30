using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Silnith.CDB.SQL;

/// <summary>
/// A CDB data store that uses SQL as the backing store.
/// </summary>
public class SQLCDB : ICDB
{
    private readonly ISQLDataStore sqlDataStore;

    /// <summary>
    /// Creates a new CDB data store that reads from the specified SQL database.
    /// </summary>
    /// <param name="sqlDataStore">An SQL data store implementation for a specific database.</param>
    /// <param name="options">Configurable settings.</param>
    public SQLCDB(ISQLDataStore sqlDataStore, IOptions<SQLCDBSettings> options)
    {
        this.sqlDataStore = sqlDataStore;
        Name = options.Value.Name;
    }

    /// <summary>
    /// A simple identifier for the CDB data store.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This must match one of the values stored in the <c>CDB</c> table.
    /// </para>
    /// </remarks>
    public string Name
    {
        get;
    }

    /// <inheritdoc/>
    public Stream? ReadMetadata(Metadata metadata)
    {
        return sqlDataStore.ReadMetadata(metadata);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMetadataAsync(Metadata metadata, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadMetadataAsync(metadata, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadTexture(Texture texture)
    {
        return sqlDataStore.ReadTexture(texture);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTextureAsync(Texture texture, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadTextureAsync(texture, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadTextureLevelOfDetail(TextureLod textureLod)
    {
        return sqlDataStore.ReadTextureLevelOfDetail(textureLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTextureLevelOfDetailAsync(TextureLod textureLod, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadTextureLevelOfDetailAsync(textureLod, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModel(GeotypicalModel geotypicalModel)
    {
        return sqlDataStore.ReadGeotypicalModel(geotypicalModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadGeotypicalModelAsync(GeotypicalModel geotypicalModel, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadGeotypicalModelAsync(geotypicalModel, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod)
    {
        return sqlDataStore.ReadGeotypicalModelLevelOfDetail(geotypicalModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadGeotypicalModelLevelOfDetailAsync(geotypicalModelLod, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadMovingModel(MovingModel movingModel)
    {
        return sqlDataStore.ReadMovingModel(movingModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMovingModelAsync(MovingModel movingModel, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadMovingModelAsync(movingModel, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadMovingModelLevelOfDetail(MovingModelLod movingModelLod)
    {
        return sqlDataStore.ReadMovingModelLevelOfDetail(movingModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadMovingModelLevelOfDetailAsync(movingModelLod, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadTile(Tile tile)
    {
        return sqlDataStore.ReadTile(tile);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileAsync(Tile tile, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadTileAsync(tile, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadTileFeature(TileArchivedFeature tileFeature)
    {
        return sqlDataStore.ReadTileFeature(tileFeature);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileFeatureAsync(TileArchivedFeature tileFeature, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadTileFeatureAsync(tileFeature, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadTileTexture(TileArchivedTexture tileTexture)
    {
        return sqlDataStore.ReadTileTexture(tileTexture);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileTextureAsync(TileArchivedTexture tileTexture, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadTileTextureAsync(tileTexture, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadNavigation(Navigation navigation)
    {
        return sqlDataStore.ReadNavigation(navigation);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadNavigationAsync(Navigation navigation, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadNavigationAsync(navigation, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteMetadata(Metadata metadata, Stream content)
    {
        sqlDataStore.WriteMetadata(metadata, content);
    }

    /// <inheritdoc/>
    public Task WriteMetadataAsync(Metadata metadata, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteMetadataAsync(metadata, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTexture(Texture texture, Stream content)
    {
        sqlDataStore.WriteTexture(texture, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureAsync(Texture texture, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteTextureAsync(texture, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTextureLevelOfDetail(TextureLod textureLod, Stream content)
    {
        sqlDataStore.WriteTextureLevelOfDetail(textureLod, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureLevelOfDetailAsync(TextureLod textureLod, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteTextureLevelOfDetailAsync(textureLod, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteGeotypicalModel(GeotypicalModel geotypicalModel, Stream content)
    {
        sqlDataStore.WriteGeotypicalModel(geotypicalModel, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelAsync(GeotypicalModel geotypicalModel, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteGeotypicalModelAsync(geotypicalModel, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod, Stream content)
    {
        sqlDataStore.WriteGeotypicalModelLevelOfDetail(geotypicalModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteGeotypicalModelLevelOfDetailAsync(geotypicalModelLod, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteMovingModel(MovingModel movingModel, Stream content)
    {
        sqlDataStore.WriteMovingModel(movingModel, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelAsync(MovingModel movingModel, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteMovingModelAsync(movingModel, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteMovingModelLevelOfDetail(MovingModelLod movingModelLod, Stream content)
    {
        sqlDataStore.WriteMovingModelLevelOfDetail(movingModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteMovingModelLevelOfDetailAsync(movingModelLod, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTile(Tile tile, Stream content)
    {
        sqlDataStore.WriteTile(tile, content);
    }

    /// <inheritdoc/>
    public Task WriteTileAsync(Tile tile, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteTileAsync(tile, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTileFeature(TileArchivedFeature tileFeature, Stream content)
    {
        sqlDataStore.WriteTileFeature(tileFeature, content);
    }

    /// <inheritdoc/>
    public Task WriteTileFeatureAsync(TileArchivedFeature tileFeature, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteTileFeatureAsync(tileFeature, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTileTexture(TileArchivedTexture tileTexture, Stream content)
    {
        sqlDataStore.WriteTileTexture(tileTexture, content);
    }

    /// <inheritdoc/>
    public Task WriteTileTextureAsync(TileArchivedTexture tileTexture, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteTileTextureAsync(tileTexture, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteNavigation(Navigation navigation, Stream content)
    {
        sqlDataStore.WriteNavigation(navigation, content);
    }

    /// <inheritdoc/>
    public Task WriteNavigationAsync(Navigation navigation, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteNavigationAsync(navigation, content, cancellationToken);
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
                sqlDataStore.Dispose();
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
        await sqlDataStore.DisposeAsync();
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
