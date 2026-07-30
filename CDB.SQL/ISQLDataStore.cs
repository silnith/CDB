using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Silnith.CDB.SQL;

/// <summary>
/// An SQL data store for CDB data.
/// </summary>
/// <remarks>
/// <para>
/// Since there are now multiple implementations with different threading
/// behavior, it seemed an appropriate time to extract an interface.
/// </para>
/// </remarks>
public interface ISQLDataStore : ICDB
{
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

    #region CDB

    /// <summary>
    /// Inserts a name into the table identifying all the unique data stores
    /// contained in the database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An <see cref="SQLDataStore"/> is capable of holding multiple CDB data stores.
    /// Each distinct data store is identified by a name.
    /// </para>
    /// </remarks>
    /// <param name="cdbName">The name of a new CDB data store.</param>
    public void InsertIntoCDB(string cdbName);

    /// <summary>
    /// Inserts a name into the table identifying all the unique data stores
    /// contained in the database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An <see cref="SQLDataStore"/> is capable of holding multiple CDB data stores.
    /// Each distinct data store is identified by a name.
    /// </para>
    /// </remarks>
    /// <param name="cdbName">The name of a new CDB data store.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public Task InsertIntoCDBAsync(string cdbName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all CDB data store names in the database.
    /// </summary>
    /// <returns>All the names of the distinct CDB data stores in the database.</returns>
    public IEnumerable<string> SelectFromCDB();

    /// <summary>
    /// Returns all CDB data store names in the database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>All the names of the distinct CDB data stores in the database.</returns>
    public IAsyncEnumerable<string> SelectFromCDBAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Metadata

    /// <inheritdoc cref="ICDB.WriteMetadata(Metadata, System.IO.Stream)"/>
    public void WriteMetadata(Metadata metadata, byte[] content);

    /// <inheritdoc cref="ICDB.WriteMetadataAsync(Metadata, System.IO.Stream, CancellationToken)"/>
    public Task WriteMetadataAsync(Metadata metadata, byte[] content, CancellationToken cancellationToken = default);

    #endregion

    #region Texture

    /// <inheritdoc cref="ICDB.WriteTexture(Texture, System.IO.Stream)"/>
    public void WriteTexture(Texture texture, byte[] content);

    /// <inheritdoc cref="ICDB.WriteTextureAsync(Texture, System.IO.Stream, CancellationToken)"/>
    public Task WriteTextureAsync(Texture texture, byte[] content, CancellationToken cancellationToken = default);

    #endregion

    #region Texture LOD

    /// <inheritdoc cref="ICDB.WriteTextureLevelOfDetail(TextureLod, System.IO.Stream)"/>
    public void WriteTextureLevelOfDetail(TextureLod textureLod, byte[] content);

    /// <inheritdoc cref="ICDB.WriteTextureLevelOfDetailAsync(TextureLod, System.IO.Stream, CancellationToken)"/>
    public Task WriteTextureLevelOfDetailAsync(TextureLod textureLod, byte[] content, CancellationToken cancellationToken = default);

    #endregion

    #region Geotypical Model

    /// <inheritdoc cref="ICDB.WriteGeotypicalModel(GeotypicalModel, System.IO.Stream)"/>
    public void WriteGeotypicalModel(GeotypicalModel geotypicalModel, byte[] content);

    /// <inheritdoc cref="ICDB.WriteGeotypicalModelAsync(GeotypicalModel, System.IO.Stream, CancellationToken)"/>
    public Task WriteGeotypicalModelAsync(GeotypicalModel geotypicalModel, byte[] content, CancellationToken cancellationToken = default);

    #endregion

    #region Geotypical Model LOD

    /// <inheritdoc cref="ICDB.WriteGeotypicalModelLevelOfDetail(GeotypicalModelLod, System.IO.Stream)"/>
    public void WriteGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod, byte[] content);

    /// <inheritdoc cref="ICDB.WriteGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod, System.IO.Stream, CancellationToken)"/>
    public Task WriteGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, byte[] content, CancellationToken cancellationToken = default);

    #endregion

    #region Moving Model

    /// <inheritdoc cref="ICDB.WriteMovingModel(MovingModel, System.IO.Stream)"/>
    public void WriteMovingModel(MovingModel movingModel, byte[] content);

    /// <inheritdoc cref="ICDB.WriteMovingModelAsync(MovingModel, System.IO.Stream, CancellationToken)"/>
    public Task WriteMovingModelAsync(MovingModel movingModel, byte[] content, CancellationToken cancellationToken = default);

    #endregion

    #region Moving Model LOD

    /// <inheritdoc cref="ICDB.WriteMovingModelLevelOfDetail(MovingModelLod, System.IO.Stream)"/>
    public void WriteMovingModelLevelOfDetail(MovingModelLod movingModelLod, byte[] content);

    /// <inheritdoc cref="ICDB.WriteMovingModelLevelOfDetailAsync(MovingModelLod, System.IO.Stream, CancellationToken)"/>
    public Task WriteMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, byte[] content, CancellationToken cancellationToken = default);

    #endregion

    #region Tile

    /// <inheritdoc cref="ICDB.WriteTile(Tile, System.IO.Stream)"/>
    public void WriteTile(Tile tile, byte[] content);

    /// <inheritdoc cref="ICDB.WriteTileAsync(Tile, System.IO.Stream, CancellationToken)"/>
    public Task WriteTileAsync(Tile tile, byte[] content, CancellationToken cancellationToken = default);

    #endregion

    #region Tile Archived Feature

    /// <inheritdoc cref="ICDB.WriteTileFeature(TileArchivedFeature, System.IO.Stream)"/>
    public void WriteTileFeature(TileArchivedFeature tileArchivedFeature, byte[] content);

    /// <inheritdoc cref="ICDB.WriteTileFeatureAsync(TileArchivedFeature, System.IO.Stream, CancellationToken)"/>
    public Task WriteTileFeatureAsync(TileArchivedFeature tileArchivedFeature, byte[] content, CancellationToken cancellationToken = default);

    #endregion

    #region Tile Archived Texture

    /// <inheritdoc cref="ICDB.WriteTileTexture(TileArchivedTexture, System.IO.Stream)"/>
    public void WriteTileTexture(TileArchivedTexture tileArchivedTexture, byte[] content);

    /// <inheritdoc cref="ICDB.WriteTileTextureAsync(TileArchivedTexture, System.IO.Stream, CancellationToken)"/>
    public Task WriteTileTextureAsync(TileArchivedTexture tileArchivedTexture, byte[] content, CancellationToken cancellationToken = default);

    #endregion

    #region Navigation

    /// <inheritdoc cref="ICDB.WriteNavigation(Navigation, System.IO.Stream)"/>
    public void WriteNavigation(Navigation navigation, byte[] content);

    /// <inheritdoc cref="ICDB.WriteNavigationAsync(Navigation, System.IO.Stream, CancellationToken)"/>
    public Task WriteNavigationAsync(Navigation navigation, byte[] content, CancellationToken cancellationToken = default);

    #endregion

}
