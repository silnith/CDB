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
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="metadata">The metadata identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoMetadata(string cdbName, Metadata metadata, byte[] content);

    /// <summary>
    /// Inserts a metadata file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="metadata">The metadata identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoMetadata(string cdbName, Metadata metadata, Stream content);

    /// <summary>
    /// Inserts a metadata file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="metadata">The metadata identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoMetadataAsync(string cdbName, Metadata metadata, byte[] content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a metadata file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="metadata">The metadata identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoMetadataAsync(string cdbName, Metadata metadata, Stream content,
        CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a metadata file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="metadata">The metadata identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? SelectFromMetadata(string cdbName, Metadata metadata);

    /// <summary>
    /// Returns a metadata file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="metadata">The metadata identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> SelectFromMetadataAsync(string cdbName, Metadata metadata,
        CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Texture

    #region Insert

    /// <summary>
    /// Inserts a texture file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="texture">The texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoTexture(string cdbName, Texture texture, byte[] content);

    /// <summary>
    /// Inserts a texture file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="texture">The texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoTexture(string cdbName, Texture texture, Stream content);

    /// <summary>
    /// Inserts a texture file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="texture">The texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoTextureAsync(string cdbName, Texture texture, byte[] content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a texture file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="texture">The texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoTextureAsync(string cdbName, Texture texture, Stream content,
        CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a texture file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="texture">The texture identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? SelectFromTexture(string cdbName, Texture texture);

    /// <summary>
    /// Returns a texture file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="texture">The texture identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> SelectFromTextureAsync(string cdbName, Texture texture,
        CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Texture LOD

    #region Insert

    /// <summary>
    /// Inserts a texture mipmap file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="textureLod">The texture mipmap identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoTextureLod(string cdbName, TextureLod textureLod, byte[] content);

    /// <summary>
    /// Inserts a texture mipmap file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="textureLod">The texture mipmap identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoTextureLod(string cdbName, TextureLod textureLod, Stream content);

    /// <summary>
    /// Inserts a texture mipmap file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="textureLod">The texture mipmap identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoTextureLodAsync(string cdbName, TextureLod textureLod, byte[] content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a texture mipmap file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="textureLod">The texture mipmap identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoTextureLodAsync(string cdbName, TextureLod textureLod, Stream content,
        CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a texture level of detail file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="textureLod">The texture level of detail identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? SelectFromTextureLod(string cdbName, TextureLod textureLod);

    /// <summary>
    /// Returns a texture level of detail file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="textureLod">The texture level of detail identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> SelectFromTextureLodAsync(string cdbName, TextureLod textureLod,
        CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Geotypical Model

    #region Insert

    /// <summary>
    /// Inserts a geotypical model file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="geotypicalModel">The geotypical model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoGeotypicalModel(string cdbName, GeotypicalModel geotypicalModel, byte[] content);

    /// <summary>
    /// Inserts a geotypical model file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="geotypicalModel">The geotypical model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoGeotypicalModel(string cdbName, GeotypicalModel geotypicalModel, Stream content);

    /// <summary>
    /// Inserts a geotypical model file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="geotypicalModel">The geotypical model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoGeotypicalModelAsync(string cdbName, GeotypicalModel geotypicalModel, byte[] content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a geotypical model file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="geotypicalModel">The geotypical model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoGeotypicalModelAsync(string cdbName, GeotypicalModel geotypicalModel, Stream content,
        CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a geotypical model file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="geotypicalModel">The geotypical model identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? SelectFromGeotypicalModel(string cdbName, GeotypicalModel geotypicalModel);

    /// <summary>
    /// Returns a geotypical model file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="geotypicalModel">The geotypical model identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> SelectFromGeotypicalModelAsync(string cdbName, GeotypicalModel geotypicalModel,
        CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Geotypical Model LOD

    #region Insert

    /// <summary>
    /// Inserts a geotypical model level of detail file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="geotypicalModelLod">The geotypical model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoGeotypicalModelLod(string cdbName, GeotypicalModelLod geotypicalModelLod, byte[] content);

    /// <summary>
    /// Inserts a geotypical model level of detail file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="geotypicalModelLod">The geotypical model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoGeotypicalModelLod(string cdbName, GeotypicalModelLod geotypicalModelLod, Stream content);

    /// <summary>
    /// Inserts a geotypical model level of detail file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="geotypicalModelLod">The geotypical model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoGeotypicalModelLodAsync(string cdbName, GeotypicalModelLod geotypicalModelLod, byte[] content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a geotypical model level of detail file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="geotypicalModelLod">The geotypical model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoGeotypicalModelLodAsync(string cdbName, GeotypicalModelLod geotypicalModelLod, Stream content,
        CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a geotypical model level of detail file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="geotypicalModelLod">The geotypical model level of detail identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? SelectFromGeotypicalModelLod(string cdbName, GeotypicalModelLod geotypicalModelLod);

    /// <summary>
    /// Returns a geotypical model level of detail file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="geotypicalModelLod">The geotypical model level of detail identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> SelectFromGeotypicalModelLodAsync(string cdbName, GeotypicalModelLod geotypicalModelLod,
        CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Moving Model

    #region Insert

    /// <summary>
    /// Inserts a moving model file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="movingModel">The moving model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoMovingModel(string cdbName, MovingModel movingModel, byte[] content);

    /// <summary>
    /// Inserts a moving model file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="movingModel">The moving model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoMovingModel(string cdbName, MovingModel movingModel, Stream content);

    /// <summary>
    /// Inserts a moving model file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="movingModel">The moving model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoMovingModelAsync(string cdbName, MovingModel movingModel, byte[] content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a moving model file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="movingModel">The moving model identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoMovingModelAsync(string cdbName, MovingModel movingModel, Stream content,
        CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a moving model file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="movingModel">The moving model identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? SelectFromMovingModel(string cdbName, MovingModel movingModel);

    /// <summary>
    /// Returns a moving model file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="movingModel">The moving model identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> SelectFromMovingModelAsync(string cdbName, MovingModel movingModel,
        CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Moving Model LOD

    #region Insert

    /// <summary>
    /// Inserts a moving model level of detail file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="movingModelLod">The moving model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoMovingModelLod(string cdbName, MovingModelLod movingModelLod, byte[] content);

    /// <summary>
    /// Inserts a moving model level of detail file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="movingModelLod">The moving model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoMovingModelLod(string cdbName, MovingModelLod movingModelLod, Stream content);

    /// <summary>
    /// Inserts a moving model level of detail file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="movingModelLod">The moving model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoMovingModelLodAsync(string cdbName, MovingModelLod movingModelLod, byte[] content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a moving model level of detail file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="movingModelLod">The moving model level of detail identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoMovingModelLodAsync(string cdbName, MovingModelLod movingModelLod, Stream content,
        CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a moving model level of detail file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="movingModelLod">The moving model level of detail identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? SelectFromMovingModelLod(string cdbName, MovingModelLod movingModelLod);

    /// <summary>
    /// Returns a moving model level of detail file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="movingModelLod">The moving model level of detail identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> SelectFromMovingModelLodAsync(string cdbName, MovingModelLod movingModelLod,
        CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Tile

    #region Insert

    /// <summary>
    /// Inserts a tiled dataset file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="tile">The tile identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoTile(string cdbName, Tile tile, byte[] content);

    /// <summary>
    /// Inserts a tiled dataset file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="tile">The tile identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoTile(string cdbName, Tile tile, Stream content);

    /// <summary>
    /// Inserts a tiled dataset file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="tile">The tile identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoTileAsync(string cdbName, Tile tile, byte[] content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a tiled dataset file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="tile">The tile identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoTileAsync(string cdbName, Tile tile, Stream content,
        CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a tiled dataset file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="tile">The tile identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? SelectFromTile(string cdbName, Tile tile);

    /// <summary>
    /// Returns a tiled dataset file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="tile">The tile identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> SelectFromTileAsync(string cdbName, Tile tile,
        CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Tile Archived Feature

    #region Insert

    /// <summary>
    /// Inserts an un-archived tiled dataset feature file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="tileArchivedFeature">The un-archived tiled dataset feature identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoTileArchivedFeature(string cdbName, TileArchivedFeature tileArchivedFeature, byte[] content);

    /// <summary>
    /// Inserts an un-archived tiled dataset feature file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="tileArchivedFeature">The un-archived tiled dataset feature identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoTileArchivedFeature(string cdbName, TileArchivedFeature tileArchivedFeature, Stream content);

    /// <summary>
    /// Inserts an un-archived tiled dataset feature file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="tileArchivedFeature">The un-archived tiled dataset feature identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoTileArchivedFeatureAsync(string cdbName, TileArchivedFeature tileArchivedFeature, byte[] content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts an un-archived tiled dataset feature file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="tileArchivedFeature">The un-archived tiled dataset feature identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoTileArchivedFeatureAsync(string cdbName, TileArchivedFeature tileArchivedFeature, Stream content,
        CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns an un-archived tiled dataset feature file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="tileArchivedFeature">The tiled dataset feature identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? SelectFromTileArchivedFeature(string cdbName, TileArchivedFeature tileArchivedFeature);

    /// <summary>
    /// Returns an un-archived tiled dataset feature file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="tileArchivedFeature">The tiled dataset feature identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> SelectFromTileArchivedFeatureAsync(string cdbName, TileArchivedFeature tileArchivedFeature,
        CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Tile Archived Texture

    #region Insert

    /// <summary>
    /// Inserts an un-archived tiled dataset texture file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="tileArchivedTexture">The un-archived tiled dataset texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoTileArchivedTexture(string cdbName, TileArchivedTexture tileArchivedTexture, byte[] content);

    /// <summary>
    /// Inserts an un-archived tiled dataset texture file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="tileArchivedTexture">The un-archived tiled dataset texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoTileArchivedTexture(string cdbName, TileArchivedTexture tileArchivedTexture, Stream content);

    /// <summary>
    /// Inserts an un-archived tiled dataset texture file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="tileArchivedTexture">The un-archived tiled dataset texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoTileArchivedTextureAsync(string cdbName, TileArchivedTexture tileArchivedTexture, byte[] content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts an un-archived tiled dataset texture file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="tileArchivedTexture">The un-archived tiled dataset texture identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoTileArchivedTextureAsync(string cdbName, TileArchivedTexture tileArchivedTexture, Stream content,
        CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns an un-archived tiled dataset texture file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="tileArchivedTexture">The tiled dataset texture identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? SelectFromTileArchivedTexture(string cdbName, TileArchivedTexture tileArchivedTexture);

    /// <summary>
    /// Returns an un-archived tiled dataset texture file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="tileArchivedTexture">The tiled dataset texture identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> SelectFromTileArchivedTextureAsync(string cdbName, TileArchivedTexture tileArchivedTexture,
        CancellationToken cancellationToken = default);

    #endregion

    #endregion

    #region Navigation

    #region Insert

    /// <summary>
    /// Inserts a navigation file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="navigation">The navigation identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoNavigation(string cdbName, Navigation navigation, byte[] content);

    /// <summary>
    /// Inserts a navigation file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="navigation">The navigation identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <returns>The number of rows affected.</returns>
    public int InsertIntoNavigation(string cdbName, Navigation navigation, Stream content);

    /// <summary>
    /// Inserts a navigation file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="navigation">The navigation identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoNavigationAsync(string cdbName, Navigation navigation, byte[] content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a navigation file into the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store to insert the file into.</param>
    /// <param name="navigation">The navigation identifier.</param>
    /// <param name="content">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> InsertIntoNavigationAsync(string cdbName, Navigation navigation, Stream content,
        CancellationToken cancellationToken = default);

    #endregion

    #region Select

    /// <summary>
    /// Returns a navigation file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="navigation">The navigation identifier.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Stream? SelectFromNavigation(string cdbName, Navigation navigation);

    /// <summary>
    /// Returns a navigation file from the database.
    /// </summary>
    /// <param name="cdbName">The name of the CDB data store.</param>
    /// <param name="navigation">The navigation identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A stream containing the file contents, or <see langword="null"/> if the file was not found.</returns>
    public Task<Stream?> SelectFromNavigationAsync(string cdbName, Navigation navigation,
        CancellationToken cancellationToken = default);

    #endregion

    #endregion

}
