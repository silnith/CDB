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
    private readonly SQLCDB sqlDataStore;

    public DbConnection DbConnection
    {
        get;
    }

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

    public PersistentConnection(SQLCDB sqlDataStore)
    {
        this.sqlDataStore = sqlDataStore;
        DbConnection = this.sqlDataStore.dbDataSource.OpenConnection();

        DbTransaction = DbConnection.BeginTransaction(IsolationLevel.Serializable);

        insertIntoCDBCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeInsertIntoCDBCommand(insertIntoCDBCommand);
        insertIntoCDBCommand.Prepare();

        selectFromCDBCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeSelectFromCDBCommand(selectFromCDBCommand);
        selectFromCDBCommand.Prepare();

        insertIntoMetadataCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeInsertIntoMetadataCommand(insertIntoMetadataCommand);
        insertIntoMetadataCommand.Prepare();

        selectFromMetadataCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeSelectFromMetadataCommand(selectFromMetadataCommand);
        selectFromMetadataCommand.Prepare();

        insertIntoTextureCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeInsertIntoTextureCommand(insertIntoTextureCommand);
        insertIntoTextureCommand.Prepare();

        selectFromTextureCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeSelectFromTextureCommand(selectFromTextureCommand);
        selectFromTextureCommand.Prepare();

        insertIntoTextureLodCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeInsertIntoTextureLodCommand(insertIntoTextureLodCommand);
        insertIntoTextureLodCommand.Prepare();

        selectFromTextureLodCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeSelectFromTextureLodCommand(selectFromTextureLodCommand);
        selectFromTextureLodCommand.Prepare();

        insertIntoGeotypicalModelCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeInsertIntoGeotypicalModelCommand(insertIntoGeotypicalModelCommand);
        insertIntoGeotypicalModelCommand.Prepare();

        selectFromGeotypicalModelCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeSelectFromGeotypicalModelCommand(selectFromGeotypicalModelCommand);
        selectFromGeotypicalModelCommand.Prepare();

        insertIntoGeotypicalModelLodCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeInsertIntoGeotypicalModelLodCommand(insertIntoGeotypicalModelLodCommand);
        insertIntoGeotypicalModelLodCommand.Prepare();

        selectFromGeotypicalModelLodCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeSelectFromGeotypicalModelLodCommand(selectFromGeotypicalModelLodCommand);
        selectFromGeotypicalModelLodCommand.Prepare();

        insertIntoMovingModelCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeInsertIntoMovingModelCommand(insertIntoMovingModelCommand);
        insertIntoMovingModelCommand.Prepare();

        selectFromMovingModelCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeSelectFromMovingModelCommand(selectFromMovingModelCommand);
        selectFromMovingModelCommand.Prepare();

        insertIntoMovingModelLodCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeInsertIntoMovingModelLodCommand(insertIntoMovingModelLodCommand);
        insertIntoMovingModelLodCommand.Prepare();

        selectFromMovingModelLodCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeSelectFromMovingModelLodCommand(selectFromMovingModelLodCommand);
        selectFromMovingModelLodCommand.Prepare();

        insertIntoTileCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeInsertIntoTileCommand(insertIntoTileCommand);
        insertIntoTileCommand.Prepare();

        selectFromTileCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeSelectFromTileCommand(selectFromTileCommand);
        selectFromTileCommand.Prepare();

        insertIntoTileArchivedFeatureCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeInsertIntoTileArchivedFeatureCommand(insertIntoTileArchivedFeatureCommand);
        insertIntoTileArchivedFeatureCommand.Prepare();

        selectFromTileArchivedFeatureCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeSelectFromTileArchivedFeatureCommand(selectFromTileArchivedFeatureCommand);
        selectFromTileArchivedFeatureCommand.Prepare();

        insertIntoTileArchivedTextureCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeInsertIntoTileArchivedTextureCommand(insertIntoTileArchivedTextureCommand);
        insertIntoTileArchivedTextureCommand.Prepare();

        selectFromTileArchivedTextureCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeSelectFromTileArchivedTextureCommand(selectFromTileArchivedTextureCommand);
        selectFromTileArchivedTextureCommand.Prepare();

        insertIntoNavigationCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeInsertIntoNavigationCommand(insertIntoNavigationCommand);
        insertIntoNavigationCommand.Prepare();

        selectFromNavigationCommand = DbConnection.CreateCommand();
        sqlDataStore.InitializeSelectFromNavigationCommand(selectFromNavigationCommand);
        selectFromNavigationCommand.Prepare();

    }

    /// <inheritdoc cref="SQLCDB.Name"/>
    public string Name => sqlDataStore.Name;

    public void Commit()
    {
        DbTransaction.Commit();
    }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        return DbTransaction.CommitAsync(cancellationToken);
    }

    #region CDB

    #region Insert

    /// <inheritdoc/>
    public void InsertIntoCDB(string cdbName)
    {
        sqlDataStore.InsertIntoCDB(insertIntoCDBCommand, cdbName);
    }

    /// <inheritdoc/>
    public Task InsertIntoCDBAsync(string cdbName, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoCDBAsync(insertIntoCDBCommand, cdbName, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public IEnumerable<string> SelectFromCDB()
    {
        return sqlDataStore.SelectFromCDB(selectFromCDBCommand);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> SelectFromCDBAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (string cdb in sqlDataStore.SelectFromCDBAsync(selectFromCDBCommand, cancellationToken).WithCancellation(cancellationToken))
        {
            yield return cdb;
        }
    }

    #endregion

    #endregion

    #region Metadata

    #region Insert

    /// <inheritdoc/>
    public void WriteMetadata(Metadata metadata, byte[] content)
    {
        sqlDataStore.InsertIntoMetadata(insertIntoMetadataCommand, metadata, content);
    }

    /// <inheritdoc/>
    public void WriteMetadata(Metadata metadata, Stream content)
    {
        sqlDataStore.InsertIntoMetadata(insertIntoMetadataCommand, metadata, content);
    }

    /// <inheritdoc/>
    public Task WriteMetadataAsync(Metadata metadata, byte[] content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoMetadataAsync(insertIntoMetadataCommand, metadata, content, cancellationToken);
    }

    /// <inheritdoc/>
    public Task WriteMetadataAsync(Metadata metadata, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoMetadataAsync(insertIntoMetadataCommand, metadata, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadMetadata(Metadata metadata)
    {
        return sqlDataStore.SelectFromMetadata(selectFromMetadataCommand, metadata);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMetadataAsync(Metadata metadata, CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromMetadataAsync(selectFromMetadataCommand, metadata, cancellationToken);
    }

    #endregion

    #endregion

    #region Texture

    #region Insert

    /// <inheritdoc/>
    public void WriteTexture(Texture texture, byte[] content)
    {
        sqlDataStore.InsertIntoTexture(insertIntoTextureCommand, texture, content);
    }

    /// <inheritdoc/>
    public void WriteTexture(Texture texture, Stream content)
    {
        sqlDataStore.InsertIntoTexture(insertIntoTextureCommand, texture, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureAsync(Texture texture, byte[] content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTextureAsync(insertIntoTextureCommand, texture, content, cancellationToken);
    }

    /// <inheritdoc/>
    public Task WriteTextureAsync(Texture texture, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTextureAsync(insertIntoTextureCommand, texture, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadTexture(Texture texture)
    {
        return sqlDataStore.SelectFromTexture(selectFromTextureCommand, texture);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTextureAsync(Texture texture, CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromTextureAsync(selectFromTextureCommand, texture, cancellationToken);
    }

    #endregion

    #endregion

    #region Texture LOD

    #region Insert

    /// <inheritdoc/>
    public void WriteTextureLevelOfDetail(TextureLod textureLod, byte[] content)
    {
        sqlDataStore.InsertIntoTextureLod(insertIntoTextureLodCommand, textureLod, content);
    }

    /// <inheritdoc/>
    public void WriteTextureLevelOfDetail(TextureLod textureLod, Stream content)
    {
        sqlDataStore.InsertIntoTextureLod(insertIntoTextureLodCommand, textureLod, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureLevelOfDetailAsync(TextureLod textureLod, byte[] content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTextureLodAsync(insertIntoTextureLodCommand, textureLod, content, cancellationToken);
    }

    /// <inheritdoc/>
    public Task WriteTextureLevelOfDetailAsync(TextureLod textureLod, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTextureLodAsync(insertIntoTextureLodCommand, textureLod, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadTextureLevelOfDetail(TextureLod textureLod)
    {
        return sqlDataStore.SelectFromTextureLod(selectFromTextureLodCommand, textureLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTextureLevelOfDetailAsync(TextureLod textureLod, CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromTextureLodAsync(selectFromTextureLodCommand, textureLod, cancellationToken);
    }

    #endregion

    #endregion

    #region Geotypical Model

    #region Insert

    /// <inheritdoc/>
    public void WriteGeotypicalModel(GeotypicalModel geotypicalModel, byte[] content)
    {
        sqlDataStore.InsertIntoGeotypicalModel(insertIntoGeotypicalModelCommand, geotypicalModel, content);
    }

    /// <inheritdoc/>
    public void WriteGeotypicalModel(GeotypicalModel geotypicalModel, Stream content)
    {
        sqlDataStore.InsertIntoGeotypicalModel(insertIntoGeotypicalModelCommand, geotypicalModel, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelAsync(GeotypicalModel geotypicalModel, byte[] content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoGeotypicalModelAsync(insertIntoGeotypicalModelCommand, geotypicalModel, content, cancellationToken);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelAsync(GeotypicalModel geotypicalModel, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoGeotypicalModelAsync(insertIntoGeotypicalModelCommand, geotypicalModel, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModel(GeotypicalModel geotypicalModel)
    {
        return sqlDataStore.SelectFromGeotypicalModel(selectFromGeotypicalModelCommand, geotypicalModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadGeotypicalModelAsync(GeotypicalModel geotypicalModel, CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromGeotypicalModelAsync(selectFromGeotypicalModelCommand, geotypicalModel, cancellationToken);
    }

    #endregion

    #endregion

    #region Geotypical Model LOD

    #region Insert

    /// <inheritdoc/>
    public void WriteGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod, byte[] content)
    {
        sqlDataStore.InsertIntoGeotypicalModelLod(insertIntoGeotypicalModelLodCommand, geotypicalModelLod, content);
    }

    /// <inheritdoc/>
    public void WriteGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod, Stream content)
    {
        sqlDataStore.InsertIntoGeotypicalModelLod(insertIntoGeotypicalModelLodCommand, geotypicalModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, byte[] content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoGeotypicalModelLodAsync(insertIntoGeotypicalModelLodCommand, geotypicalModelLod, content, cancellationToken);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoGeotypicalModelLodAsync(insertIntoGeotypicalModelLodCommand, geotypicalModelLod, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod)
    {
        return sqlDataStore.SelectFromGeotypicalModelLod(selectFromGeotypicalModelLodCommand, geotypicalModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromGeotypicalModelLodAsync(selectFromGeotypicalModelLodCommand, geotypicalModelLod, cancellationToken);
    }

    #endregion

    #endregion

    #region Moving Model

    #region Insert

    /// <inheritdoc/>
    public void WriteMovingModel(MovingModel movingModel, byte[] content)
    {
        sqlDataStore.InsertIntoMovingModel(insertIntoMovingModelCommand, movingModel, content);
    }

    /// <inheritdoc/>
    public void WriteMovingModel(MovingModel movingModel, Stream content)
    {
        sqlDataStore.InsertIntoMovingModel(insertIntoMovingModelCommand, movingModel, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelAsync(MovingModel movingModel, byte[] content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoMovingModelAsync(insertIntoMovingModelCommand, movingModel, content, cancellationToken);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelAsync(MovingModel movingModel, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoMovingModelAsync(insertIntoMovingModelCommand, movingModel, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadMovingModel(MovingModel movingModel)
    {
        return sqlDataStore.SelectFromMovingModel(selectFromMovingModelCommand, movingModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMovingModelAsync(MovingModel movingModel, CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromMovingModelAsync(selectFromMovingModelCommand, movingModel, cancellationToken);
    }

    #endregion

    #endregion

    #region Moving Model LOD

    #region Insert

    /// <inheritdoc/>
    public void WriteMovingModelLevelOfDetail(MovingModelLod movingModelLod, byte[] content)
    {
        sqlDataStore.InsertIntoMovingModelLod(insertIntoMovingModelLodCommand, movingModelLod, content);
    }

    /// <inheritdoc/>
    public void WriteMovingModelLevelOfDetail(MovingModelLod movingModelLod, Stream content)
    {
        sqlDataStore.InsertIntoMovingModelLod(insertIntoMovingModelLodCommand, movingModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, byte[] content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoMovingModelLodAsync(insertIntoMovingModelLodCommand, movingModelLod, content, cancellationToken);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoMovingModelLodAsync(insertIntoMovingModelLodCommand, movingModelLod, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadMovingModelLevelOfDetail(MovingModelLod movingModelLod)
    {
        return sqlDataStore.SelectFromMovingModelLod(selectFromMovingModelLodCommand, movingModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromMovingModelLodAsync(selectFromMovingModelLodCommand, movingModelLod, cancellationToken);
    }

    #endregion

    #endregion

    #region Tile

    #region Insert

    /// <inheritdoc/>
    public void WriteTile(Tile tile, byte[] content)
    {
        sqlDataStore.InsertIntoTile(insertIntoTileCommand, tile, content);
    }

    /// <inheritdoc/>
    public void WriteTile(Tile tile, Stream content)
    {
        sqlDataStore.InsertIntoTile(insertIntoTileCommand, tile, content);
    }

    /// <inheritdoc/>
    public Task WriteTileAsync(Tile tile, byte[] content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTileAsync(insertIntoTileCommand, tile, content, cancellationToken);
    }

    /// <inheritdoc/>
    public Task WriteTileAsync(Tile tile, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTileAsync(insertIntoTileCommand, tile, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadTile(Tile tile)
    {
        return sqlDataStore.SelectFromTile(selectFromTileCommand, tile);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileAsync(Tile tile, CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromTileAsync(selectFromTileCommand, tile, cancellationToken);
    }

    #endregion

    #endregion

    #region Tile Archived Feature

    #region Insert

    /// <inheritdoc/>
    public void WriteTileFeature(TileArchivedFeature tileArchivedFeature, byte[] content)
    {
        sqlDataStore.InsertIntoTileArchivedFeature(insertIntoTileArchivedFeatureCommand, tileArchivedFeature, content);
    }

    /// <inheritdoc/>
    public void WriteTileFeature(TileArchivedFeature tileArchivedFeature, Stream content)
    {
        sqlDataStore.InsertIntoTileArchivedFeature(insertIntoTileArchivedFeatureCommand, tileArchivedFeature, content);
    }

    /// <inheritdoc/>
    public Task WriteTileFeatureAsync(TileArchivedFeature tileArchivedFeature, byte[] content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTileArchivedFeatureAsync(insertIntoTileArchivedFeatureCommand, tileArchivedFeature, content, cancellationToken);
    }

    /// <inheritdoc/>
    public Task WriteTileFeatureAsync(TileArchivedFeature tileArchivedFeature, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTileArchivedFeatureAsync(insertIntoTileArchivedFeatureCommand, tileArchivedFeature, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadTileFeature(TileArchivedFeature tileArchivedFeature)
    {
        return sqlDataStore.SelectFromTileArchivedFeature(selectFromTileArchivedFeatureCommand, tileArchivedFeature);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileFeatureAsync(TileArchivedFeature tileArchivedFeature, CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromTileArchivedFeatureAsync(selectFromTileArchivedFeatureCommand, tileArchivedFeature, cancellationToken);
    }

    #endregion

    #endregion

    #region Tile Archived Texture

    #region Insert

    /// <inheritdoc/>
    public void WriteTileTexture(TileArchivedTexture tileArchivedTexture, byte[] content)
    {
        sqlDataStore.InsertIntoTileArchivedTexture(insertIntoTileArchivedTextureCommand, tileArchivedTexture, content);
    }

    /// <inheritdoc/>
    public void WriteTileTexture(TileArchivedTexture tileArchivedTexture, Stream content)
    {
        sqlDataStore.InsertIntoTileArchivedTexture(insertIntoTileArchivedTextureCommand, tileArchivedTexture, content);
    }

    /// <inheritdoc/>
    public Task WriteTileTextureAsync(TileArchivedTexture tileArchivedTexture, byte[] content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTileArchivedTextureAsync(insertIntoTileArchivedTextureCommand,
            tileArchivedTexture, content, cancellationToken);
    }

    /// <inheritdoc/>
    public Task WriteTileTextureAsync(TileArchivedTexture tileArchivedTexture, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTileArchivedTextureAsync(insertIntoTileArchivedTextureCommand,
            tileArchivedTexture, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadTileTexture(TileArchivedTexture tileArchivedTexture)
    {
        return sqlDataStore.SelectFromTileArchivedTexture(selectFromTileArchivedTextureCommand, tileArchivedTexture);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileTextureAsync(TileArchivedTexture tileArchivedTexture, CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromTileArchivedTextureAsync(selectFromTileArchivedTextureCommand, tileArchivedTexture, cancellationToken);
    }

    #endregion

    #endregion

    #region Navigation

    #region Insert

    /// <inheritdoc/>
    public void WriteNavigation(Navigation navigation, byte[] content)
    {
        sqlDataStore.InsertIntoNavigation(insertIntoNavigationCommand, navigation, content);
    }

    /// <inheritdoc/>
    public void WriteNavigation(Navigation navigation, Stream content)
    {
        sqlDataStore.InsertIntoNavigation(insertIntoNavigationCommand, navigation, content);
    }

    /// <inheritdoc/>
    public Task WriteNavigationAsync(Navigation navigation, byte[] content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoNavigationAsync(insertIntoNavigationCommand, navigation, content, cancellationToken);
    }

    /// <inheritdoc/>
    public Task WriteNavigationAsync(Navigation navigation, Stream content, CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoNavigationAsync(insertIntoNavigationCommand, navigation, content, cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public Stream? ReadNavigation(Navigation navigation)
    {
        return sqlDataStore.SelectFromNavigation(selectFromNavigationCommand, navigation);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadNavigationAsync(Navigation navigation, CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromNavigationAsync(selectFromNavigationCommand, navigation, cancellationToken);
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
