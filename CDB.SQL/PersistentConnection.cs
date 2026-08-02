using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Silnith.CDB.SQL;

/// <summary>
/// A wrapper around <see cref="SQLCDB"/> that allows transactions.
/// </summary>
public class PersistentConnection : ICDB
{
    private readonly SQLCDB sqlCDB;

    /// <summary>
    /// The database connection.
    /// </summary>
    public DbConnection DbConnection
    {
        get;
    }

    /// <summary>
    /// The database transaction.
    /// </summary>
    public DbTransaction DbTransaction
    {
        get;
        private set;
    }

    private readonly DbCommand selectFromCDBCommand;
    private readonly DbCommand insertIntoCDBCommand;
    private readonly DbCommand selectFromMetadataCommand;
    private readonly DbCommand insertIntoMetadataCommand;
    private readonly DbCommand selectFromTextureCommand;
    private readonly DbCommand insertIntoTextureCommand;
    private readonly DbCommand selectFromTextureLodCommand;
    private readonly DbCommand insertIntoTextureLodCommand;
    private readonly DbCommand selectFromGeotypicalModelCommand;
    private readonly DbCommand insertIntoGeotypicalModelCommand;
    private readonly DbCommand selectFromGeotypicalModelLodCommand;
    private readonly DbCommand insertIntoGeotypicalModelLodCommand;
    private readonly DbCommand selectFromMovingModelCommand;
    private readonly DbCommand insertIntoMovingModelCommand;
    private readonly DbCommand selectFromMovingModelLodCommand;
    private readonly DbCommand insertIntoMovingModelLodCommand;
    private readonly DbCommand selectFromTileCommand;
    private readonly DbCommand insertIntoTileCommand;
    private readonly DbCommand selectFromTileArchivedFeatureCommand;
    private readonly DbCommand insertIntoTileArchivedFeatureCommand;
    private readonly DbCommand selectFromTileArchivedTextureCommand;
    private readonly DbCommand insertIntoTileArchivedTextureCommand;
    private readonly DbCommand selectFromNavigationCommand;
    private readonly DbCommand insertIntoNavigationCommand;
    private readonly DbCommand selectTileExtentsCommand;

    /// <summary>
    /// Creates a connection that allows batching operations into transactions.
    /// </summary>
    /// <param name="sqlCDB">The CDB to connect to.</param>
    public PersistentConnection(SQLCDB sqlCDB)
    {
        this.sqlCDB = sqlCDB;
        DbConnection = this.sqlCDB.dbDataSource.OpenConnection();

        DbTransaction = DbConnection.BeginTransaction(IsolationLevel.Serializable);

        selectFromCDBCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeSelectFromCDBCommand(selectFromCDBCommand);
        selectFromCDBCommand.Prepare();

        insertIntoCDBCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeInsertIntoCDBCommand(insertIntoCDBCommand);
        insertIntoCDBCommand.Prepare();

        selectFromMetadataCommand = DbConnection.CreateCommand();
        this.sqlCDB.metadataAccessor.InitializeSelectCommand(selectFromMetadataCommand);
        selectFromMetadataCommand.Prepare();

        insertIntoMetadataCommand = DbConnection.CreateCommand();
        this.sqlCDB.metadataAccessor.InitializeInsertCommand(insertIntoMetadataCommand);
        insertIntoMetadataCommand.Prepare();

        selectFromTextureCommand = DbConnection.CreateCommand();
        this.sqlCDB.textureAccessor.InitializeSelectCommand(selectFromTextureCommand);
        selectFromTextureCommand.Prepare();

        insertIntoTextureCommand = DbConnection.CreateCommand();
        this.sqlCDB.textureAccessor.InitializeInsertCommand(insertIntoTextureCommand);
        insertIntoTextureCommand.Prepare();

        selectFromTextureLodCommand = DbConnection.CreateCommand();
        this.sqlCDB.textureLodAccessor.InitializeSelectCommand(selectFromTextureLodCommand);
        selectFromTextureLodCommand.Prepare();

        insertIntoTextureLodCommand = DbConnection.CreateCommand();
        this.sqlCDB.textureLodAccessor.InitializeInsertCommand(insertIntoTextureLodCommand);
        insertIntoTextureLodCommand.Prepare();

        selectFromGeotypicalModelCommand = DbConnection.CreateCommand();
        this.sqlCDB.geotypicalModelAccessor.InitializeSelectCommand(selectFromGeotypicalModelCommand);
        selectFromGeotypicalModelCommand.Prepare();

        insertIntoGeotypicalModelCommand = DbConnection.CreateCommand();
        this.sqlCDB.geotypicalModelAccessor.InitializeInsertCommand(insertIntoGeotypicalModelCommand);
        insertIntoGeotypicalModelCommand.Prepare();

        selectFromGeotypicalModelLodCommand = DbConnection.CreateCommand();
        this.sqlCDB.geotypicalModelLodAccessor.InitializeSelectCommand(selectFromGeotypicalModelLodCommand);
        selectFromGeotypicalModelLodCommand.Prepare();

        insertIntoGeotypicalModelLodCommand = DbConnection.CreateCommand();
        this.sqlCDB.geotypicalModelLodAccessor.InitializeInsertCommand(insertIntoGeotypicalModelLodCommand);
        insertIntoGeotypicalModelLodCommand.Prepare();

        selectFromMovingModelCommand = DbConnection.CreateCommand();
        this.sqlCDB.movingModelAccessor.InitializeSelectCommand(selectFromMovingModelCommand);
        selectFromMovingModelCommand.Prepare();

        insertIntoMovingModelCommand = DbConnection.CreateCommand();
        this.sqlCDB.movingModelAccessor.InitializeInsertCommand(insertIntoMovingModelCommand);
        insertIntoMovingModelCommand.Prepare();

        selectFromMovingModelLodCommand = DbConnection.CreateCommand();
        this.sqlCDB.movingModelLodAccessor.InitializeSelectCommand(selectFromMovingModelLodCommand);
        selectFromMovingModelLodCommand.Prepare();

        insertIntoMovingModelLodCommand = DbConnection.CreateCommand();
        this.sqlCDB.movingModelLodAccessor.InitializeInsertCommand(insertIntoMovingModelLodCommand);
        insertIntoMovingModelLodCommand.Prepare();

        selectFromTileCommand = DbConnection.CreateCommand();
        this.sqlCDB.tileAccessor.InitializeSelectCommand(selectFromTileCommand);
        selectFromTileCommand.Prepare();

        insertIntoTileCommand = DbConnection.CreateCommand();
        this.sqlCDB.tileAccessor.InitializeInsertCommand(insertIntoTileCommand);
        insertIntoTileCommand.Prepare();

        selectFromTileArchivedFeatureCommand = DbConnection.CreateCommand();
        this.sqlCDB.tileFeatureAccessor.InitializeSelectCommand(selectFromTileArchivedFeatureCommand);
        selectFromTileArchivedFeatureCommand.Prepare();

        insertIntoTileArchivedFeatureCommand = DbConnection.CreateCommand();
        this.sqlCDB.tileFeatureAccessor.InitializeInsertCommand(insertIntoTileArchivedFeatureCommand);
        insertIntoTileArchivedFeatureCommand.Prepare();

        selectFromTileArchivedTextureCommand = DbConnection.CreateCommand();
        this.sqlCDB.tileTextureAccessor.InitializeSelectCommand(selectFromTileArchivedTextureCommand);
        selectFromTileArchivedTextureCommand.Prepare();

        insertIntoTileArchivedTextureCommand = DbConnection.CreateCommand();
        this.sqlCDB.tileTextureAccessor.InitializeInsertCommand(insertIntoTileArchivedTextureCommand);
        insertIntoTileArchivedTextureCommand.Prepare();

        selectFromNavigationCommand = DbConnection.CreateCommand();
        this.sqlCDB.navigationAccessor.InitializeSelectCommand(selectFromNavigationCommand);
        selectFromNavigationCommand.Prepare();

        insertIntoNavigationCommand = DbConnection.CreateCommand();
        this.sqlCDB.navigationAccessor.InitializeInsertCommand(insertIntoNavigationCommand);
        insertIntoNavigationCommand.Prepare();

        selectTileExtentsCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeTileExtentsQuery(selectTileExtentsCommand);
        selectTileExtentsCommand.Prepare();
    }

    /// <inheritdoc cref="SQLCDB.Name"/>
    public string Name => sqlCDB.Name;

    /// <summary>
    /// Commits all the writes that have happened using this connection.
    /// </summary>
    public void Commit()
    {
        DbTransaction.Commit();
        DbTransaction.Dispose();

        DbTransaction = DbConnection.BeginTransaction(IsolationLevel.Serializable);
    }

    /// <summary>
    /// Commits all the writes that have happened using this connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await DbTransaction.CommitAsync(cancellationToken);
        await DbTransaction.DisposeAsync();

        DbTransaction = DbConnection.BeginTransaction(IsolationLevel.Serializable);
    }

    #region CDB

    #region Select

    /// <inheritdoc/>
    public IEnumerable<string> SelectFromCDB()
    {
        return sqlCDB.SelectFromCDB(selectFromCDBCommand);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> SelectFromCDBAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (string cdb in sqlCDB.SelectFromCDBAsync(selectFromCDBCommand, cancellationToken).WithCancellation(cancellationToken))
        {
            yield return cdb;
        }
    }

    #endregion

    #region Insert

    /// <inheritdoc/>
    public void InsertIntoCDB(string cdbName)
    {
        sqlCDB.InsertIntoCDB(insertIntoCDBCommand, cdbName);
    }

    /// <inheritdoc/>
    public Task InsertIntoCDBAsync(string cdbName, CancellationToken cancellationToken)
    {
        return sqlCDB.InsertIntoCDBAsync(insertIntoCDBCommand, cdbName, cancellationToken);
    }

    #endregion

    #endregion

    #region Metadata

    #region Select

    /// <inheritdoc/>
    public Stream? ReadMetadata(Metadata metadata)
    {
        return sqlCDB.metadataAccessor.SelectUsingPreparedStatement(selectFromMetadataCommand, metadata);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMetadataAsync(Metadata metadata, CancellationToken cancellationToken)
    {
        return sqlCDB.metadataAccessor.SelectUsingPreparedStatementAsync(selectFromMetadataCommand, metadata, cancellationToken);
    }

    #endregion

    #region Insert

    /// <inheritdoc/>
    public void WriteMetadata(Metadata metadata, Stream content)
    {
        sqlCDB.metadataAccessor.InsertUsingPreparedStatement(insertIntoMetadataCommand, metadata, content);
    }

    /// <inheritdoc/>
    public Task WriteMetadataAsync(Metadata metadata, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.metadataAccessor.InsertUsingPreparedStatementAsync(insertIntoMetadataCommand, metadata, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Texture

    #region Select

    /// <inheritdoc/>
    public Stream? ReadTexture(Texture texture)
    {
        return sqlCDB.textureAccessor.SelectUsingPreparedStatement(selectFromTextureCommand, texture);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTextureAsync(Texture texture, CancellationToken cancellationToken)
    {
        return sqlCDB.textureAccessor.SelectUsingPreparedStatementAsync(selectFromTextureCommand, texture, cancellationToken);
    }

    #endregion

    #region Insert

    /// <inheritdoc/>
    public void WriteTexture(Texture texture, Stream content)
    {
        sqlCDB.textureAccessor.InsertUsingPreparedStatement(insertIntoTextureCommand, texture, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureAsync(Texture texture, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.textureAccessor.InsertUsingPreparedStatementAsync(insertIntoTextureCommand, texture, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Texture LOD

    #region Select

    /// <inheritdoc/>
    public Stream? ReadTextureLevelOfDetail(TextureLod textureLod)
    {
        return sqlCDB.textureLodAccessor.SelectUsingPreparedStatement(selectFromTextureLodCommand, textureLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTextureLevelOfDetailAsync(TextureLod textureLod, CancellationToken cancellationToken)
    {
        return sqlCDB.textureLodAccessor.SelectUsingPreparedStatementAsync(selectFromTextureLodCommand, textureLod, cancellationToken);
    }

    #endregion

    #region Insert

    /// <inheritdoc/>
    public void WriteTextureLevelOfDetail(TextureLod textureLod, Stream content)
    {
        sqlCDB.textureLodAccessor.InsertUsingPreparedStatement(insertIntoTextureLodCommand, textureLod, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureLevelOfDetailAsync(TextureLod textureLod, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.textureLodAccessor.InsertUsingPreparedStatementAsync(insertIntoTextureLodCommand, textureLod, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Geotypical Model

    #region Select

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModel(GeotypicalModel geotypicalModel)
    {
        return sqlCDB.geotypicalModelAccessor.SelectUsingPreparedStatement(selectFromGeotypicalModelCommand, geotypicalModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadGeotypicalModelAsync(GeotypicalModel geotypicalModel, CancellationToken cancellationToken)
    {
        return sqlCDB.geotypicalModelAccessor.SelectUsingPreparedStatementAsync(selectFromGeotypicalModelCommand, geotypicalModel, cancellationToken);
    }

    #endregion

    #region Insert

    /// <inheritdoc/>
    public void WriteGeotypicalModel(GeotypicalModel geotypicalModel, Stream content)
    {
        sqlCDB.geotypicalModelAccessor.InsertUsingPreparedStatement(insertIntoGeotypicalModelCommand, geotypicalModel, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelAsync(GeotypicalModel geotypicalModel, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.geotypicalModelAccessor.InsertUsingPreparedStatementAsync(insertIntoGeotypicalModelCommand, geotypicalModel, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Geotypical Model LOD

    #region Select

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod)
    {
        return sqlCDB.geotypicalModelLodAccessor.SelectUsingPreparedStatement(selectFromGeotypicalModelLodCommand, geotypicalModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, CancellationToken cancellationToken)
    {
        return sqlCDB.geotypicalModelLodAccessor.SelectUsingPreparedStatementAsync(selectFromGeotypicalModelLodCommand, geotypicalModelLod, cancellationToken);
    }

    #endregion

    #region Insert

    /// <inheritdoc/>
    public void WriteGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod, Stream content)
    {
        sqlCDB.geotypicalModelLodAccessor.InsertUsingPreparedStatement(insertIntoGeotypicalModelLodCommand, geotypicalModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.geotypicalModelLodAccessor.InsertUsingPreparedStatementAsync(insertIntoGeotypicalModelLodCommand, geotypicalModelLod, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Moving Model

    #region Select

    /// <inheritdoc/>
    public Stream? ReadMovingModel(MovingModel movingModel)
    {
        return sqlCDB.movingModelAccessor.SelectUsingPreparedStatement(selectFromMovingModelCommand, movingModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMovingModelAsync(MovingModel movingModel, CancellationToken cancellationToken)
    {
        return sqlCDB.movingModelAccessor.SelectUsingPreparedStatementAsync(selectFromMovingModelCommand, movingModel, cancellationToken);
    }

    #endregion

    #region Insert

    /// <inheritdoc/>
    public void WriteMovingModel(MovingModel movingModel, Stream content)
    {
        sqlCDB.movingModelAccessor.InsertUsingPreparedStatement(insertIntoMovingModelCommand, movingModel, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelAsync(MovingModel movingModel, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.movingModelAccessor.InsertUsingPreparedStatementAsync(insertIntoMovingModelCommand, movingModel, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Moving Model LOD

    #region Select

    /// <inheritdoc/>
    public Stream? ReadMovingModelLevelOfDetail(MovingModelLod movingModelLod)
    {
        return sqlCDB.movingModelLodAccessor.SelectUsingPreparedStatement(selectFromMovingModelLodCommand, movingModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, CancellationToken cancellationToken)
    {
        return sqlCDB.movingModelLodAccessor.SelectUsingPreparedStatementAsync(selectFromMovingModelLodCommand, movingModelLod, cancellationToken);
    }

    #endregion

    #region Insert

    /// <inheritdoc/>
    public void WriteMovingModelLevelOfDetail(MovingModelLod movingModelLod, Stream content)
    {
        sqlCDB.movingModelLodAccessor.InsertUsingPreparedStatement(insertIntoMovingModelLodCommand, movingModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.movingModelLodAccessor.InsertUsingPreparedStatementAsync(insertIntoMovingModelLodCommand, movingModelLod, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Tile

    #region Select

    /// <inheritdoc/>
    public Stream? ReadTile(Tile tile)
    {
        return sqlCDB.tileAccessor.SelectUsingPreparedStatement(selectFromTileCommand, tile);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileAsync(Tile tile, CancellationToken cancellationToken)
    {
        return sqlCDB.tileAccessor.SelectUsingPreparedStatementAsync(selectFromTileCommand, tile, cancellationToken);
    }

    #endregion

    #region Insert

    /// <inheritdoc/>
    public void WriteTile(Tile tile, Stream content)
    {
        sqlCDB.tileAccessor.InsertUsingPreparedStatement(insertIntoTileCommand, tile, content);
    }

    /// <inheritdoc/>
    public Task WriteTileAsync(Tile tile, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.tileAccessor.InsertUsingPreparedStatementAsync(insertIntoTileCommand, tile, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Tile Archived Feature

    #region Select

    /// <inheritdoc/>
    public Stream? ReadTileFeature(TileArchivedFeature tileArchivedFeature)
    {
        return sqlCDB.tileFeatureAccessor.SelectUsingPreparedStatement(selectFromTileArchivedFeatureCommand, tileArchivedFeature);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileFeatureAsync(TileArchivedFeature tileArchivedFeature, CancellationToken cancellationToken)
    {
        return sqlCDB.tileFeatureAccessor.SelectUsingPreparedStatementAsync(selectFromTileArchivedFeatureCommand, tileArchivedFeature, cancellationToken);
    }

    #endregion

    #region Insert

    /// <inheritdoc/>
    public void WriteTileFeature(TileArchivedFeature tileArchivedFeature, Stream content)
    {
        sqlCDB.tileFeatureAccessor.InsertUsingPreparedStatement(insertIntoTileArchivedFeatureCommand, tileArchivedFeature, content);
    }

    /// <inheritdoc/>
    public Task WriteTileFeatureAsync(TileArchivedFeature tileArchivedFeature, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.tileFeatureAccessor.InsertUsingPreparedStatementAsync(insertIntoTileArchivedFeatureCommand, tileArchivedFeature, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Tile Archived Texture

    #region Select

    /// <inheritdoc/>
    public Stream? ReadTileTexture(TileArchivedTexture tileArchivedTexture)
    {
        return sqlCDB.tileTextureAccessor.SelectUsingPreparedStatement(selectFromTileArchivedTextureCommand, tileArchivedTexture);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileTextureAsync(TileArchivedTexture tileArchivedTexture, CancellationToken cancellationToken)
    {
        return sqlCDB.tileTextureAccessor.SelectUsingPreparedStatementAsync(selectFromTileArchivedTextureCommand, tileArchivedTexture, cancellationToken);
    }

    #endregion

    #region Insert

    /// <inheritdoc/>
    public void WriteTileTexture(TileArchivedTexture tileArchivedTexture, Stream content)
    {
        sqlCDB.tileTextureAccessor.InsertUsingPreparedStatement(insertIntoTileArchivedTextureCommand, tileArchivedTexture, content);
    }

    /// <inheritdoc/>
    public Task WriteTileTextureAsync(TileArchivedTexture tileArchivedTexture, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.tileTextureAccessor.InsertUsingPreparedStatementAsync(insertIntoTileArchivedTextureCommand, tileArchivedTexture, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Navigation

    #region Select

    /// <inheritdoc/>
    public Stream? ReadNavigation(Navigation navigation)
    {
        return sqlCDB.navigationAccessor.SelectUsingPreparedStatement(selectFromNavigationCommand, navigation);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadNavigationAsync(Navigation navigation, CancellationToken cancellationToken)
    {
        return sqlCDB.navigationAccessor.SelectUsingPreparedStatementAsync(selectFromNavigationCommand, navigation, cancellationToken);
    }

    #endregion

    #region Insert

    /// <inheritdoc/>
    public void WriteNavigation(Navigation navigation, Stream content)
    {
        sqlCDB.navigationAccessor.InsertUsingPreparedStatement(insertIntoNavigationCommand, navigation, content);
    }

    /// <inheritdoc/>
    public Task WriteNavigationAsync(Navigation navigation, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.navigationAccessor.InsertUsingPreparedStatementAsync(insertIntoNavigationCommand, navigation, content, cancellationToken);
    }

    #endregion

    #endregion

    /// <inheritdoc/>
    public IEnumerable<(Latitude, Longitude)> GetTileExtents()
    {
        return sqlCDB.GetTileExtents(selectTileExtentsCommand);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<(Latitude, Longitude)> GetTileExtentsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach ((Latitude, Longitude) tuple in sqlCDB.GetTileExtentsAsync(selectTileExtentsCommand, cancellationToken).WithCancellation(cancellationToken))
        {
            yield return tuple;
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
                selectTileExtentsCommand.Dispose();
                insertIntoNavigationCommand.Dispose();
                selectFromNavigationCommand.Dispose();
                insertIntoTileArchivedTextureCommand.Dispose();
                selectFromTileArchivedTextureCommand.Dispose();
                insertIntoTileArchivedFeatureCommand.Dispose();
                selectFromTileArchivedFeatureCommand.Dispose();
                insertIntoTileCommand.Dispose();
                selectFromTileCommand.Dispose();
                insertIntoMovingModelLodCommand.Dispose();
                selectFromMovingModelLodCommand.Dispose();
                insertIntoMovingModelCommand.Dispose();
                selectFromMovingModelCommand.Dispose();
                insertIntoGeotypicalModelLodCommand.Dispose();
                selectFromGeotypicalModelLodCommand.Dispose();
                insertIntoGeotypicalModelCommand.Dispose();
                selectFromGeotypicalModelCommand.Dispose();
                insertIntoTextureLodCommand.Dispose();
                selectFromTextureLodCommand.Dispose();
                insertIntoTextureCommand.Dispose();
                selectFromTextureCommand.Dispose();
                insertIntoMetadataCommand.Dispose();
                selectFromMetadataCommand.Dispose();
                insertIntoCDBCommand.Dispose();
                selectFromCDBCommand.Dispose();

                DbTransaction.Dispose();

                DbConnection.Dispose();
            }

            disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
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
        await Task.WhenAll(
            selectTileExtentsCommand.DisposeAsync().AsTask(),
            insertIntoNavigationCommand.DisposeAsync().AsTask(),
            selectFromNavigationCommand.DisposeAsync().AsTask(),
            insertIntoTileArchivedTextureCommand.DisposeAsync().AsTask(),
            selectFromTileArchivedTextureCommand.DisposeAsync().AsTask(),
            insertIntoTileArchivedFeatureCommand.DisposeAsync().AsTask(),
            selectFromTileArchivedFeatureCommand.DisposeAsync().AsTask(),
            insertIntoTileCommand.DisposeAsync().AsTask(),
            selectFromTileCommand.DisposeAsync().AsTask(),
            insertIntoMovingModelLodCommand.DisposeAsync().AsTask(),
            selectFromMovingModelLodCommand.DisposeAsync().AsTask(),
            insertIntoMovingModelCommand.DisposeAsync().AsTask(),
            selectFromMovingModelCommand.DisposeAsync().AsTask(),
            insertIntoGeotypicalModelLodCommand.DisposeAsync().AsTask(),
            selectFromGeotypicalModelLodCommand.DisposeAsync().AsTask(),
            insertIntoGeotypicalModelCommand.DisposeAsync().AsTask(),
            selectFromGeotypicalModelCommand.DisposeAsync().AsTask(),
            insertIntoTextureLodCommand.DisposeAsync().AsTask(),
            selectFromTextureLodCommand.DisposeAsync().AsTask(),
            insertIntoTextureCommand.DisposeAsync().AsTask(),
            selectFromTextureCommand.DisposeAsync().AsTask(),
            insertIntoMetadataCommand.DisposeAsync().AsTask(),
            selectFromMetadataCommand.DisposeAsync().AsTask(),
            insertIntoCDBCommand.DisposeAsync().AsTask(),
            selectFromCDBCommand.DisposeAsync().AsTask());

        await DbTransaction.DisposeAsync();

        await DbConnection.DisposeAsync();
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
