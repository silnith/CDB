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
    }

    private readonly DbCommand insertIntoCDBCommand;
    private readonly DbCommand selectFromCDBCommand;
    private readonly DbCommand insertIntoMetadataCommand;
    private readonly DbCommand selectFromMetadataCommand;
    private readonly DbCommand insertIntoTextureCommand;
    private readonly DbCommand selectFromTextureCommand;
    private readonly DbCommand insertIntoTextureLodCommand;
    private readonly DbCommand selectFromTextureLodCommand;
    private readonly DbCommand insertIntoGeotypicalModelCommand;
    private readonly DbCommand selectFromGeotypicalModelCommand;
    private readonly DbCommand insertIntoGeotypicalModelLodCommand;
    private readonly DbCommand selectFromGeotypicalModelLodCommand;
    private readonly DbCommand insertIntoMovingModelCommand;
    private readonly DbCommand selectFromMovingModelCommand;
    private readonly DbCommand insertIntoMovingModelLodCommand;
    private readonly DbCommand selectFromMovingModelLodCommand;
    private readonly DbCommand insertIntoTileCommand;
    private readonly DbCommand selectFromTileCommand;
    private readonly DbCommand insertIntoTileArchivedFeatureCommand;
    private readonly DbCommand selectFromTileArchivedFeatureCommand;
    private readonly DbCommand insertIntoTileArchivedTextureCommand;
    private readonly DbCommand selectFromTileArchivedTextureCommand;
    private readonly DbCommand insertIntoNavigationCommand;
    private readonly DbCommand selectFromNavigationCommand;

    public PersistentConnection(SQLCDB sqlCDB)
    {
        this.sqlCDB = sqlCDB;
        DbConnection = this.sqlCDB.dbDataSource.OpenConnection();

        DbTransaction = DbConnection.BeginTransaction(IsolationLevel.Serializable);

        insertIntoCDBCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeInsertIntoCDBCommand(insertIntoCDBCommand);
        insertIntoCDBCommand.Prepare();

        selectFromCDBCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeSelectFromCDBCommand(selectFromCDBCommand);
        selectFromCDBCommand.Prepare();

        insertIntoMetadataCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeInsertIntoMetadataCommand(insertIntoMetadataCommand);
        insertIntoMetadataCommand.Prepare();

        selectFromMetadataCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeSelectFromMetadataCommand(selectFromMetadataCommand);
        selectFromMetadataCommand.Prepare();

        insertIntoTextureCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeInsertIntoTextureCommand(insertIntoTextureCommand);
        insertIntoTextureCommand.Prepare();

        selectFromTextureCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeSelectFromTextureCommand(selectFromTextureCommand);
        selectFromTextureCommand.Prepare();

        insertIntoTextureLodCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeInsertIntoTextureLodCommand(insertIntoTextureLodCommand);
        insertIntoTextureLodCommand.Prepare();

        selectFromTextureLodCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeSelectFromTextureLodCommand(selectFromTextureLodCommand);
        selectFromTextureLodCommand.Prepare();

        insertIntoGeotypicalModelCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeInsertIntoGeotypicalModelCommand(insertIntoGeotypicalModelCommand);
        insertIntoGeotypicalModelCommand.Prepare();

        selectFromGeotypicalModelCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeSelectFromGeotypicalModelCommand(selectFromGeotypicalModelCommand);
        selectFromGeotypicalModelCommand.Prepare();

        insertIntoGeotypicalModelLodCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeInsertIntoGeotypicalModelLodCommand(insertIntoGeotypicalModelLodCommand);
        insertIntoGeotypicalModelLodCommand.Prepare();

        selectFromGeotypicalModelLodCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeSelectFromGeotypicalModelLodCommand(selectFromGeotypicalModelLodCommand);
        selectFromGeotypicalModelLodCommand.Prepare();

        insertIntoMovingModelCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeInsertIntoMovingModelCommand(insertIntoMovingModelCommand);
        insertIntoMovingModelCommand.Prepare();

        selectFromMovingModelCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeSelectFromMovingModelCommand(selectFromMovingModelCommand);
        selectFromMovingModelCommand.Prepare();

        insertIntoMovingModelLodCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeInsertIntoMovingModelLodCommand(insertIntoMovingModelLodCommand);
        insertIntoMovingModelLodCommand.Prepare();

        selectFromMovingModelLodCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeSelectFromMovingModelLodCommand(selectFromMovingModelLodCommand);
        selectFromMovingModelLodCommand.Prepare();

        insertIntoTileCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeInsertIntoTileCommand(insertIntoTileCommand);
        insertIntoTileCommand.Prepare();

        selectFromTileCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeSelectFromTileCommand(selectFromTileCommand);
        selectFromTileCommand.Prepare();

        insertIntoTileArchivedFeatureCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeInsertIntoTileArchivedFeatureCommand(insertIntoTileArchivedFeatureCommand);
        insertIntoTileArchivedFeatureCommand.Prepare();

        selectFromTileArchivedFeatureCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeSelectFromTileArchivedFeatureCommand(selectFromTileArchivedFeatureCommand);
        selectFromTileArchivedFeatureCommand.Prepare();

        insertIntoTileArchivedTextureCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeInsertIntoTileArchivedTextureCommand(insertIntoTileArchivedTextureCommand);
        insertIntoTileArchivedTextureCommand.Prepare();

        selectFromTileArchivedTextureCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeSelectFromTileArchivedTextureCommand(selectFromTileArchivedTextureCommand);
        selectFromTileArchivedTextureCommand.Prepare();

        insertIntoNavigationCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeInsertIntoNavigationCommand(insertIntoNavigationCommand);
        insertIntoNavigationCommand.Prepare();

        selectFromNavigationCommand = DbConnection.CreateCommand();
        this.sqlCDB.InitializeSelectFromNavigationCommand(selectFromNavigationCommand);
        selectFromNavigationCommand.Prepare();

    }

    /// <inheritdoc cref="SQLCDB.Name"/>
    public string Name => sqlCDB.Name;

    /// <summary>
    /// Commits all the writes that have happened using this connection.
    /// </summary>
    public void Commit()
    {
        DbTransaction.Commit();
    }

    /// <summary>
    /// Commits all the writes that have happened using this connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    public Task CommitAsync(CancellationToken cancellationToken)
    {
        return DbTransaction.CommitAsync(cancellationToken);
    }

    #region CDB

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

    #endregion

    #region Metadata

    #region Insert

    /// <inheritdoc/>
    public void WriteMetadata(Metadata metadata, Stream content)
    {
        sqlCDB.InsertIntoMetadata(insertIntoMetadataCommand, metadata, content);
    }

    /// <inheritdoc/>
    public Task WriteMetadataAsync(Metadata metadata, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.InsertIntoMetadataAsync(insertIntoMetadataCommand, metadata, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadMetadata(Metadata metadata)
    {
        return sqlCDB.SelectFromMetadata(selectFromMetadataCommand, metadata);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMetadataAsync(Metadata metadata, CancellationToken cancellationToken)
    {
        return sqlCDB.SelectFromMetadataAsync(selectFromMetadataCommand, metadata, cancellationToken);
    }

    #endregion

    #endregion

    #region Texture

    #region Insert

    /// <inheritdoc/>
    public void WriteTexture(Texture texture, Stream content)
    {
        sqlCDB.InsertIntoTexture(insertIntoTextureCommand, texture, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureAsync(Texture texture, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.InsertIntoTextureAsync(insertIntoTextureCommand, texture, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadTexture(Texture texture)
    {
        return sqlCDB.SelectFromTexture(selectFromTextureCommand, texture);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTextureAsync(Texture texture, CancellationToken cancellationToken)
    {
        return sqlCDB.SelectFromTextureAsync(selectFromTextureCommand, texture, cancellationToken);
    }

    #endregion

    #endregion

    #region Texture LOD

    #region Insert

    /// <inheritdoc/>
    public void WriteTextureLevelOfDetail(TextureLod textureLod, Stream content)
    {
        sqlCDB.InsertIntoTextureLod(insertIntoTextureLodCommand, textureLod, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureLevelOfDetailAsync(TextureLod textureLod, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.InsertIntoTextureLodAsync(insertIntoTextureLodCommand, textureLod, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadTextureLevelOfDetail(TextureLod textureLod)
    {
        return sqlCDB.SelectFromTextureLod(selectFromTextureLodCommand, textureLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTextureLevelOfDetailAsync(TextureLod textureLod, CancellationToken cancellationToken)
    {
        return sqlCDB.SelectFromTextureLodAsync(selectFromTextureLodCommand, textureLod, cancellationToken);
    }

    #endregion

    #endregion

    #region Geotypical Model

    #region Insert

    /// <inheritdoc/>
    public void WriteGeotypicalModel(GeotypicalModel geotypicalModel, Stream content)
    {
        sqlCDB.InsertIntoGeotypicalModel(insertIntoGeotypicalModelCommand, geotypicalModel, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelAsync(GeotypicalModel geotypicalModel, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.InsertIntoGeotypicalModelAsync(insertIntoGeotypicalModelCommand, geotypicalModel, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModel(GeotypicalModel geotypicalModel)
    {
        return sqlCDB.SelectFromGeotypicalModel(selectFromGeotypicalModelCommand, geotypicalModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadGeotypicalModelAsync(GeotypicalModel geotypicalModel, CancellationToken cancellationToken)
    {
        return sqlCDB.SelectFromGeotypicalModelAsync(selectFromGeotypicalModelCommand, geotypicalModel, cancellationToken);
    }

    #endregion

    #endregion

    #region Geotypical Model LOD

    #region Insert

    /// <inheritdoc/>
    public void WriteGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod, Stream content)
    {
        sqlCDB.InsertIntoGeotypicalModelLod(insertIntoGeotypicalModelLodCommand, geotypicalModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.InsertIntoGeotypicalModelLodAsync(insertIntoGeotypicalModelLodCommand, geotypicalModelLod, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod)
    {
        return sqlCDB.SelectFromGeotypicalModelLod(selectFromGeotypicalModelLodCommand, geotypicalModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, CancellationToken cancellationToken)
    {
        return sqlCDB.SelectFromGeotypicalModelLodAsync(selectFromGeotypicalModelLodCommand, geotypicalModelLod, cancellationToken);
    }

    #endregion

    #endregion

    #region Moving Model

    #region Insert

    /// <inheritdoc/>
    public void WriteMovingModel(MovingModel movingModel, Stream content)
    {
        sqlCDB.InsertIntoMovingModel(insertIntoMovingModelCommand, movingModel, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelAsync(MovingModel movingModel, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.InsertIntoMovingModelAsync(insertIntoMovingModelCommand, movingModel, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadMovingModel(MovingModel movingModel)
    {
        return sqlCDB.SelectFromMovingModel(selectFromMovingModelCommand, movingModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMovingModelAsync(MovingModel movingModel, CancellationToken cancellationToken)
    {
        return sqlCDB.SelectFromMovingModelAsync(selectFromMovingModelCommand, movingModel, cancellationToken);
    }

    #endregion

    #endregion

    #region Moving Model LOD

    #region Insert

    /// <inheritdoc/>
    public void WriteMovingModelLevelOfDetail(MovingModelLod movingModelLod, Stream content)
    {
        sqlCDB.InsertIntoMovingModelLod(insertIntoMovingModelLodCommand, movingModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.InsertIntoMovingModelLodAsync(insertIntoMovingModelLodCommand, movingModelLod, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadMovingModelLevelOfDetail(MovingModelLod movingModelLod)
    {
        return sqlCDB.SelectFromMovingModelLod(selectFromMovingModelLodCommand, movingModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, CancellationToken cancellationToken)
    {
        return sqlCDB.SelectFromMovingModelLodAsync(selectFromMovingModelLodCommand, movingModelLod, cancellationToken);
    }

    #endregion

    #endregion

    #region Tile

    #region Insert

    /// <inheritdoc/>
    public void WriteTile(Tile tile, Stream content)
    {
        sqlCDB.InsertIntoTile(insertIntoTileCommand, tile, content);
    }

    /// <inheritdoc/>
    public Task WriteTileAsync(Tile tile, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.InsertIntoTileAsync(insertIntoTileCommand, tile, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadTile(Tile tile)
    {
        return sqlCDB.SelectFromTile(selectFromTileCommand, tile);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileAsync(Tile tile, CancellationToken cancellationToken)
    {
        return sqlCDB.SelectFromTileAsync(selectFromTileCommand, tile, cancellationToken);
    }

    #endregion

    #endregion

    #region Tile Archived Feature

    #region Insert

    /// <inheritdoc/>
    public void WriteTileFeature(TileArchivedFeature tileArchivedFeature, Stream content)
    {
        sqlCDB.InsertIntoTileArchivedFeature(insertIntoTileArchivedFeatureCommand, tileArchivedFeature, content);
    }

    /// <inheritdoc/>
    public Task WriteTileFeatureAsync(TileArchivedFeature tileArchivedFeature, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.InsertIntoTileArchivedFeatureAsync(insertIntoTileArchivedFeatureCommand, tileArchivedFeature, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadTileFeature(TileArchivedFeature tileArchivedFeature)
    {
        return sqlCDB.SelectFromTileArchivedFeature(selectFromTileArchivedFeatureCommand, tileArchivedFeature);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileFeatureAsync(TileArchivedFeature tileArchivedFeature, CancellationToken cancellationToken)
    {
        return sqlCDB.SelectFromTileArchivedFeatureAsync(selectFromTileArchivedFeatureCommand, tileArchivedFeature, cancellationToken);
    }

    #endregion

    #endregion

    #region Tile Archived Texture

    #region Insert

    /// <inheritdoc/>
    public void WriteTileTexture(TileArchivedTexture tileArchivedTexture, Stream content)
    {
        sqlCDB.InsertIntoTileArchivedTexture(insertIntoTileArchivedTextureCommand, tileArchivedTexture, content);
    }

    /// <inheritdoc/>
    public Task WriteTileTextureAsync(TileArchivedTexture tileArchivedTexture, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.InsertIntoTileArchivedTextureAsync(insertIntoTileArchivedTextureCommand,
            tileArchivedTexture, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadTileTexture(TileArchivedTexture tileArchivedTexture)
    {
        return sqlCDB.SelectFromTileArchivedTexture(selectFromTileArchivedTextureCommand, tileArchivedTexture);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileTextureAsync(TileArchivedTexture tileArchivedTexture, CancellationToken cancellationToken)
    {
        return sqlCDB.SelectFromTileArchivedTextureAsync(selectFromTileArchivedTextureCommand, tileArchivedTexture, cancellationToken);
    }

    #endregion

    #endregion

    #region Navigation

    #region Insert

    /// <inheritdoc/>
    public void WriteNavigation(Navigation navigation, Stream content)
    {
        sqlCDB.InsertIntoNavigation(insertIntoNavigationCommand, navigation, content);
    }

    /// <inheritdoc/>
    public Task WriteNavigationAsync(Navigation navigation, Stream content, CancellationToken cancellationToken)
    {
        return sqlCDB.InsertIntoNavigationAsync(insertIntoNavigationCommand, navigation, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadNavigation(Navigation navigation)
    {
        return sqlCDB.SelectFromNavigation(selectFromNavigationCommand, navigation);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadNavigationAsync(Navigation navigation, CancellationToken cancellationToken)
    {
        return sqlCDB.SelectFromNavigationAsync(selectFromNavigationCommand, navigation, cancellationToken);
    }

    #endregion

    #endregion

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
                selectFromNavigationCommand.Dispose();
                insertIntoNavigationCommand.Dispose();
                selectFromTileArchivedTextureCommand.Dispose();
                insertIntoTileArchivedTextureCommand.Dispose();
                selectFromTileArchivedFeatureCommand.Dispose();
                insertIntoTileArchivedFeatureCommand.Dispose();
                selectFromTileCommand.Dispose();
                insertIntoTileCommand.Dispose();
                selectFromMovingModelLodCommand.Dispose();
                insertIntoMovingModelLodCommand.Dispose();
                selectFromMovingModelCommand.Dispose();
                insertIntoMovingModelCommand.Dispose();
                selectFromGeotypicalModelLodCommand.Dispose();
                insertIntoGeotypicalModelLodCommand.Dispose();
                selectFromGeotypicalModelCommand.Dispose();
                insertIntoGeotypicalModelCommand.Dispose();
                selectFromTextureLodCommand.Dispose();
                insertIntoTextureLodCommand.Dispose();
                selectFromTextureCommand.Dispose();
                insertIntoTextureCommand.Dispose();
                selectFromMetadataCommand.Dispose();
                insertIntoMetadataCommand.Dispose();
                selectFromCDBCommand.Dispose();
                insertIntoCDBCommand.Dispose();

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
            selectFromNavigationCommand.DisposeAsync().AsTask(),
            insertIntoNavigationCommand.DisposeAsync().AsTask(),
            selectFromTileArchivedTextureCommand.DisposeAsync().AsTask(),
            insertIntoTileArchivedTextureCommand.DisposeAsync().AsTask(),
            selectFromTileArchivedFeatureCommand.DisposeAsync().AsTask(),
            insertIntoTileArchivedFeatureCommand.DisposeAsync().AsTask(),
            selectFromTileCommand.DisposeAsync().AsTask(),
            insertIntoTileCommand.DisposeAsync().AsTask(),
            selectFromMovingModelLodCommand.DisposeAsync().AsTask(),
            insertIntoMovingModelLodCommand.DisposeAsync().AsTask(),
            selectFromMovingModelCommand.DisposeAsync().AsTask(),
            insertIntoMovingModelCommand.DisposeAsync().AsTask(),
            selectFromGeotypicalModelLodCommand.DisposeAsync().AsTask(),
            insertIntoGeotypicalModelLodCommand.DisposeAsync().AsTask(),
            selectFromGeotypicalModelCommand.DisposeAsync().AsTask(),
            insertIntoGeotypicalModelCommand.DisposeAsync().AsTask(),
            selectFromTextureLodCommand.DisposeAsync().AsTask(),
            insertIntoTextureLodCommand.DisposeAsync().AsTask(),
            selectFromTextureCommand.DisposeAsync().AsTask(),
            insertIntoTextureCommand.DisposeAsync().AsTask(),
            selectFromMetadataCommand.DisposeAsync().AsTask(),
            insertIntoMetadataCommand.DisposeAsync().AsTask(),
            selectFromCDBCommand.DisposeAsync().AsTask(),
            insertIntoCDBCommand.DisposeAsync().AsTask());

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
