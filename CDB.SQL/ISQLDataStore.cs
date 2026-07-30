using System;
using System.Collections.Generic;
using System.IO;
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
public interface ISQLDataStore : IDisposable, IAsyncDisposable
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

    #region Insert

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
    /// <returns>The number of database rows affected.</returns>
    public int InsertIntoCDB(string cdbName);

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
    /// <returns>The number of database rows affected.</returns>
    public Task<int> InsertIntoCDBAsync(string cdbName,
        CancellationToken cancellationToken = default);

    #endregion

    #region Select

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

    #endregion

    #region Metadata

    #region Insert

    /// <summary>
    /// Inserts a metadata file into the database.
    /// </summary>
    /// <param name="metadata">The metadata identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoMetadata(Metadata metadata, byte[] content);

    /// <summary>
    /// Inserts a metadata file into the database.
    /// </summary>
    /// <param name="metadata">The metadata identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int WriteMetadata(Metadata metadata, Stream content);

    /// <summary>
    /// Inserts a metadata file into the database.
    /// </summary>
    /// <param name="metadata">The metadata identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoMetadataAsync(Metadata metadata, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a metadata file into the database.
    /// </summary>
    /// <param name="metadata">The metadata identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> WriteMetadataAsync(Metadata metadata, Stream content, CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a metadata file from the database.
    /// </summary>
    /// <param name="metadata">The metadata identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? ReadMetadata(Metadata metadata);

    /// <summary>
    /// Returns a metadata file from the database.
    /// </summary>
    /// <param name="metadata">The metadata identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> ReadMetadataAsync(Metadata metadata, CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Texture

    #region Insert

    /// <summary>
    /// Inserts a texture file into the database.
    /// </summary>
    /// <param name="texture">The texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoTexture(Texture texture, byte[] content);

    /// <summary>
    /// Inserts a texture file into the database.
    /// </summary>
    /// <param name="texture">The texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int WriteTexture(Texture texture, Stream content);

    /// <summary>
    /// Inserts a texture file into the database.
    /// </summary>
    /// <param name="texture">The texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoTextureAsync(Texture texture, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a texture file into the database.
    /// </summary>
    /// <param name="texture">The texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> WriteTextureAsync(Texture texture, Stream content, CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a texture file from the database.
    /// </summary>
    /// <param name="texture">The texture identifier.</param>
    /// 
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? ReadTexture(Texture texture);

    /// <summary>
    /// Returns a texture file from the database.
    /// </summary>
    /// <param name="texture">The texture identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> ReadTextureAsync(Texture texture, CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Texture LOD

    #region Insert

    /// <summary>
    /// Inserts a texture mipmap file into the database.
    /// </summary>
    /// <param name="textureLod">The texture mipmap identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoTextureLod(TextureLod textureLod, byte[] content);

    /// <summary>
    /// Inserts a texture mipmap file into the database.
    /// </summary>
    /// <param name="textureLod">The texture mipmap identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int WriteTextureLevelOfDetail(TextureLod textureLod, Stream content);

    /// <summary>
    /// Inserts a texture mipmap file into the database.
    /// </summary>
    /// <param name="textureLod">The texture mipmap identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoTextureLodAsync(TextureLod textureLod, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a texture mipmap file into the database.
    /// </summary>
    /// <param name="textureLod">The texture mipmap identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> WriteTextureLevelOfDetailAsync(TextureLod textureLod, Stream content, CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a texture level of detail file from the database.
    /// </summary>
    /// <param name="textureLod">The texture level of detail identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? ReadTextureLevelOfDetail(TextureLod textureLod);

    /// <summary>
    /// Returns a texture level of detail file from the database.
    /// </summary>
    /// <param name="textureLod">The texture level of detail identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> ReadTextureLevelOfDetailAsync(TextureLod textureLod, CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Geotypical Model

    #region Insert

    /// <summary>
    /// Inserts a geotypical model file into the database.
    /// </summary>
    /// <param name="geotypicalModel">The geotypical model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoGeotypicalModel(GeotypicalModel geotypicalModel, byte[] content);

    /// <summary>
    /// Inserts a geotypical model file into the database.
    /// </summary>
    /// <param name="geotypicalModel">The geotypical model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int WriteGeotypicalModel(GeotypicalModel geotypicalModel, Stream content);

    /// <summary>
    /// Inserts a geotypical model file into the database.
    /// </summary>
    /// <param name="geotypicalModel">The geotypical model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoGeotypicalModelAsync(GeotypicalModel geotypicalModel, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a geotypical model file into the database.
    /// </summary>
    /// <param name="geotypicalModel">The geotypical model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> WriteGeotypicalModelAsync(GeotypicalModel geotypicalModel, Stream content, CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a geotypical model file from the database.
    /// </summary>
    /// <param name="geotypicalModel">The geotypical model identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? ReadGeotypicalModel(GeotypicalModel geotypicalModel);

    /// <summary>
    /// Returns a geotypical model file from the database.
    /// </summary>
    /// <param name="geotypicalModel">The geotypical model identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> ReadGeotypicalModelAsync(GeotypicalModel geotypicalModel, CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Geotypical Model LOD

    #region Insert

    /// <summary>
    /// Inserts a geotypical model level of detail file into the database.
    /// </summary>
    /// <param name="geotypicalModelLod">The geotypical model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoGeotypicalModelLod(GeotypicalModelLod geotypicalModelLod, byte[] content);

    /// <summary>
    /// Inserts a geotypical model level of detail file into the database.
    /// </summary>
    /// <param name="geotypicalModelLod">The geotypical model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int WriteGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod, Stream content);

    /// <summary>
    /// Inserts a geotypical model level of detail file into the database.
    /// </summary>
    /// <param name="geotypicalModelLod">The geotypical model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoGeotypicalModelLodAsync(GeotypicalModelLod geotypicalModelLod, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a geotypical model level of detail file into the database.
    /// </summary>
    /// <param name="geotypicalModelLod">The geotypical model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> WriteGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, Stream content, CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a geotypical model level of detail file from the database.
    /// </summary>
    /// <param name="geotypicalModelLod">The geotypical model level of detail identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? ReadGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod);

    /// <summary>
    /// Returns a geotypical model level of detail file from the database.
    /// </summary>
    /// <param name="geotypicalModelLod">The geotypical model level of detail identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> ReadGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Moving Model

    #region Insert

    /// <summary>
    /// Inserts a moving model file into the database.
    /// </summary>
    /// <param name="movingModel">The moving model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoMovingModel(MovingModel movingModel, byte[] content);

    /// <summary>
    /// Inserts a moving model file into the database.
    /// </summary>
    /// <param name="movingModel">The moving model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int WriteMovingModel(MovingModel movingModel, Stream content);

    /// <summary>
    /// Inserts a moving model file into the database.
    /// </summary>
    /// <param name="movingModel">The moving model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoMovingModelAsync(MovingModel movingModel, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a moving model file into the database.
    /// </summary>
    /// <param name="movingModel">The moving model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> WriteMovingModelAsync(MovingModel movingModel, Stream content, CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a moving model file from the database.
    /// </summary>
    /// <param name="movingModel">The moving model identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? ReadMovingModel(MovingModel movingModel);

    /// <summary>
    /// Returns a moving model file from the database.
    /// </summary>
    /// <param name="movingModel">The moving model identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> ReadMovingModelAsync(MovingModel movingModel, CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Moving Model LOD

    #region Insert

    /// <summary>
    /// Inserts a moving model level of detail file into the database.
    /// </summary>
    /// <param name="movingModelLod">The moving model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoMovingModelLod(MovingModelLod movingModelLod, byte[] content);

    /// <summary>
    /// Inserts a moving model level of detail file into the database.
    /// </summary>
    /// <param name="movingModelLod">The moving model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int WriteMovingModelLevelOfDetail(MovingModelLod movingModelLod, Stream content);

    /// <summary>
    /// Inserts a moving model level of detail file into the database.
    /// </summary>
    /// <param name="movingModelLod">The moving model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoMovingModelLodAsync(MovingModelLod movingModelLod, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a moving model level of detail file into the database.
    /// </summary>
    /// <param name="movingModelLod">The moving model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> WriteMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, Stream content, CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a moving model level of detail file from the database.
    /// </summary>
    /// <param name="movingModelLod">The moving model level of detail identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? ReadMovingModelLevelOfDetail(MovingModelLod movingModelLod);

    /// <summary>
    /// Returns a moving model level of detail file from the database.
    /// </summary>
    /// <param name="movingModelLod">The moving model level of detail identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> ReadMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Tile

    #region Insert

    /// <summary>
    /// Inserts a tiled dataset file into the database.
    /// </summary>
    /// <param name="tile">The tile identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoTile(Tile tile, byte[] content);

    /// <summary>
    /// Inserts a tiled dataset file into the database.
    /// </summary>
    /// <param name="tile">The tile identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int WriteTile(Tile tile, Stream content);

    /// <summary>
    /// Inserts a tiled dataset file into the database.
    /// </summary>
    /// <param name="tile">The tile identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoTileAsync(Tile tile, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a tiled dataset file into the database.
    /// </summary>
    /// <param name="tile">The tile identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> WriteTileAsync(Tile tile, Stream content, CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a tiled dataset file from the database.
    /// </summary>
    /// <param name="tile">The tile identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? ReadTile(Tile tile);

    /// <summary>
    /// Returns a tiled dataset file from the database.
    /// </summary>
    /// <param name="tile">The tile identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> ReadTileAsync(Tile tile, CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Tile Archived Feature

    #region Insert

    /// <summary>
    /// Inserts an un-archived tiled dataset feature file into the database.
    /// </summary>
    /// <param name="tileArchivedFeature">The un-archived tiled dataset feature identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoTileArchivedFeature(TileArchivedFeature tileArchivedFeature, byte[] content);

    /// <summary>
    /// Inserts an un-archived tiled dataset feature file into the database.
    /// </summary>
    /// <param name="tileArchivedFeature">The un-archived tiled dataset feature identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int WriteTileFeature(TileArchivedFeature tileArchivedFeature, Stream content);

    /// <summary>
    /// Inserts an un-archived tiled dataset feature file into the database.
    /// </summary>
    /// <param name="tileArchivedFeature">The un-archived tiled dataset feature identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoTileArchivedFeatureAsync(TileArchivedFeature tileArchivedFeature, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts an un-archived tiled dataset feature file into the database.
    /// </summary>
    /// <param name="tileArchivedFeature">The un-archived tiled dataset feature identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> WriteTileFeatureAsync(TileArchivedFeature tileArchivedFeature, Stream content, CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns an un-archived tiled dataset feature file from the database.
    /// </summary>
    /// <param name="tileArchivedFeature">The tiled dataset feature identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? ReadTileFeature(TileArchivedFeature tileArchivedFeature);

    /// <summary>
    /// Returns an un-archived tiled dataset feature file from the database.
    /// </summary>
    /// <param name="tileArchivedFeature">The tiled dataset feature identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> ReadTileFeatureAsync(TileArchivedFeature tileArchivedFeature, CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Tile Archived Texture

    #region Insert

    /// <summary>
    /// Inserts an un-archived tiled dataset texture file into the database.
    /// </summary>
    /// <param name="tileArchivedTexture">The un-archived tiled dataset texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoTileArchivedTexture(TileArchivedTexture tileArchivedTexture, byte[] content);

    /// <summary>
    /// Inserts an un-archived tiled dataset texture file into the database.
    /// </summary>
    /// <param name="tileArchivedTexture">The un-archived tiled dataset texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int WriteTileTexture(TileArchivedTexture tileArchivedTexture, Stream content);

    /// <summary>
    /// Inserts an un-archived tiled dataset texture file into the database.
    /// </summary>
    /// <param name="tileArchivedTexture">The un-archived tiled dataset texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoTileArchivedTextureAsync(TileArchivedTexture tileArchivedTexture, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts an un-archived tiled dataset texture file into the database.
    /// </summary>
    /// <param name="tileArchivedTexture">The un-archived tiled dataset texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> WriteTileTextureAsync(TileArchivedTexture tileArchivedTexture, Stream content, CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns an un-archived tiled dataset texture file from the database.
    /// </summary>
    /// <param name="tileArchivedTexture">The tiled dataset texture identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? ReadTileTexture(TileArchivedTexture tileArchivedTexture);

    /// <summary>
    /// Returns an un-archived tiled dataset texture file from the database.
    /// </summary>
    /// <param name="tileArchivedTexture">The tiled dataset texture identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> ReadTileTextureAsync(TileArchivedTexture tileArchivedTexture, CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Navigation

    #region Insert

    /// <summary>
    /// Inserts a navigation file into the database.
    /// </summary>
    /// <param name="navigation">The navigation identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoNavigation(Navigation navigation, byte[] content);

    /// <summary>
    /// Inserts a navigation file into the database.
    /// </summary>
    /// <param name="navigation">The navigation identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int WriteNavigation(Navigation navigation, Stream content);

    /// <summary>
    /// Inserts a navigation file into the database.
    /// </summary>
    /// <param name="navigation">The navigation identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoNavigationAsync(Navigation navigation, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a navigation file into the database.
    /// </summary>
    /// <param name="navigation">The navigation identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> WriteNavigationAsync(Navigation navigation, Stream content, CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a navigation file from the database.
    /// </summary>
    /// <param name="navigation">The navigation identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? ReadNavigation(Navigation navigation);

    /// <summary>
    /// Returns a navigation file from the database.
    /// </summary>
    /// <param name="navigation">The navigation identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> ReadNavigationAsync(Navigation navigation, CancellationToken cancellationToken = default);

    #endregion

    #endregion

}
