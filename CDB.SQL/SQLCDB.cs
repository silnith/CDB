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
        return sqlDataStore.ReadMetadata(Name, metadata);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMetadataAsync(Metadata metadata, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadMetadataAsync(Name, metadata, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadTexture(Texture texture)
    {
        return sqlDataStore.ReadTexture(Name, texture);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTextureAsync(Texture texture, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadTextureAsync(Name, texture, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadTextureLevelOfDetail(TextureLod textureLod)
    {
        return sqlDataStore.ReadTextureLevelOfDetail(Name, textureLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTextureLevelOfDetailAsync(TextureLod textureLod, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadTextureLevelOfDetailAsync(Name, textureLod, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModel(GeotypicalModel geotypicalModel)
    {
        return sqlDataStore.ReadGeotypicalModel(Name, geotypicalModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadGeotypicalModelAsync(GeotypicalModel geotypicalModel, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadGeotypicalModelAsync(Name, geotypicalModel, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod)
    {
        return sqlDataStore.ReadGeotypicalModelLevelOfDetail(Name, geotypicalModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadGeotypicalModelLevelOfDetailAsync(Name, geotypicalModelLod, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadMovingModel(MovingModel movingModel)
    {
        return sqlDataStore.ReadMovingModel(Name, movingModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMovingModelAsync(MovingModel movingModel, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadMovingModelAsync(Name, movingModel, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadMovingModelLevelOfDetail(MovingModelLod movingModelLod)
    {
        return sqlDataStore.ReadMovingModelLevelOfDetail(Name, movingModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadMovingModelLevelOfDetailAsync(Name, movingModelLod, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadTile(Tile tile)
    {
        return sqlDataStore.ReadTile(Name, tile);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileAsync(Tile tile, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadTileAsync(Name, tile, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadTileFeature(TileArchivedFeature tileFeature)
    {
        return sqlDataStore.ReadTileFeature(Name, tileFeature);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileFeatureAsync(TileArchivedFeature tileFeature, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadTileFeatureAsync(Name, tileFeature, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadTileTexture(TileArchivedTexture tileTexture)
    {
        return sqlDataStore.ReadTileTexture(Name, tileTexture);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileTextureAsync(TileArchivedTexture tileTexture, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadTileTextureAsync(Name, tileTexture, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? ReadNavigation(Navigation navigation)
    {
        return sqlDataStore.ReadNavigation(Name, navigation);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadNavigationAsync(Navigation navigation, CancellationToken cancellationToken)
    {
        return sqlDataStore.ReadNavigationAsync(Name, navigation, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteMetadata(Metadata metadata, Stream content)
    {
        sqlDataStore.WriteMetadata(Name, metadata, content);
    }

    /// <inheritdoc/>
    public Task WriteMetadataAsync(Metadata metadata, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteMetadataAsync(Name, metadata, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTexture(Texture texture, Stream content)
    {
        sqlDataStore.WriteTexture(Name, texture, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureAsync(Texture texture, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteTextureAsync(Name, texture, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTextureLevelOfDetail(TextureLod textureLod, Stream content)
    {
        sqlDataStore.WriteTextureLevelOfDetail(Name, textureLod, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureLevelOfDetailAsync(TextureLod textureLod, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteTextureLevelOfDetailAsync(Name, textureLod, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteGeotypicalModel(GeotypicalModel geotypicalModel, Stream content)
    {
        sqlDataStore.WriteGeotypicalModel(Name, geotypicalModel, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelAsync(GeotypicalModel geotypicalModel, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteGeotypicalModelAsync(Name, geotypicalModel, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod, Stream content)
    {
        sqlDataStore.WriteGeotypicalModelLevelOfDetail(Name, geotypicalModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteGeotypicalModelLevelOfDetailAsync(Name, geotypicalModelLod, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteMovingModel(MovingModel movingModel, Stream content)
    {
        sqlDataStore.WriteMovingModel(Name, movingModel, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelAsync(MovingModel movingModel, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteMovingModelAsync(Name, movingModel, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteMovingModelLevelOfDetail(MovingModelLod movingModelLod, Stream content)
    {
        sqlDataStore.WriteMovingModelLevelOfDetail(Name, movingModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteMovingModelLevelOfDetailAsync(Name, movingModelLod, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTile(Tile tile, Stream content)
    {
        sqlDataStore.WriteTile(Name, tile, content);
    }

    /// <inheritdoc/>
    public Task WriteTileAsync(Tile tile, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteTileAsync(Name, tile, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTileFeature(TileArchivedFeature tileFeature, Stream content)
    {
        sqlDataStore.WriteTileFeature(Name, tileFeature, content);
    }

    /// <inheritdoc/>
    public Task WriteTileFeatureAsync(TileArchivedFeature tileFeature, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteTileFeatureAsync(Name, tileFeature, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteTileTexture(TileArchivedTexture tileTexture, Stream content)
    {
        sqlDataStore.WriteTileTexture(Name, tileTexture, content);
    }

    /// <inheritdoc/>
    public Task WriteTileTextureAsync(TileArchivedTexture tileTexture, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteTileTextureAsync(Name, tileTexture, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteNavigation(Navigation navigation, Stream content)
    {
        sqlDataStore.WriteNavigation(Name, navigation, content);
    }

    /// <inheritdoc/>
    public Task WriteNavigationAsync(Navigation navigation, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.WriteNavigationAsync(Name, navigation, content, cancellationToken);
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
