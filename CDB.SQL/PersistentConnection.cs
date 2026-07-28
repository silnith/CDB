using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Silnith.CDB.SQL;

public class PersistentConnection : ISQLDataStore
{
    private readonly SQLDataStore sqlDataStore;

    public DbConnection DbConnection
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

    public PersistentConnection(SQLDataStore sqlDataStore)
    {
        this.sqlDataStore = sqlDataStore;
        DbConnection = this.sqlDataStore.dbDataSource.OpenConnection();

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

    #region CDB

    #region Insert

    /// <inheritdoc/>
    public int InsertIntoCDB(string cdbName)
    {
        return sqlDataStore.InsertIntoCDB(insertIntoCDBCommand, cdbName);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoCDBAsync(string cdbName, CancellationToken cancellationToken)
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
    public int InsertIntoMetadata(string cdbName, Metadata metadata, byte[] content)
    {
        return sqlDataStore.InsertIntoMetadata(insertIntoMetadataCommand, cdbName, metadata, content);
    }

    /// <inheritdoc/>
    public int InsertIntoMetadata(string cdbName, Metadata metadata, Stream content)
    {
        return sqlDataStore.InsertIntoMetadata(insertIntoMetadataCommand, cdbName, metadata, content);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoMetadataAsync(string cdbName, Metadata metadata, byte[] content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoMetadataAsync(insertIntoMetadataCommand, cdbName, metadata, content,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoMetadataAsync(string cdbName, Metadata metadata, Stream content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoMetadataAsync(insertIntoMetadataCommand, cdbName, metadata, content,
            cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public bool TrySelectFromMetadata(string cdbName, Metadata metadata, Action<Stream> fileFoundAction)
    {
        return sqlDataStore.TrySelectFromMetadata(selectFromMetadataCommand, cdbName, metadata,
            fileFoundAction);
    }

    /// <inheritdoc/>
    public Task<bool> TrySelectFromMetadataAsync(string cdbName, Metadata metadata,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.TrySelectFromMetadataAsync(selectFromMetadataCommand, cdbName, metadata,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? SelectFromMetadata(string cdbName, Metadata metadata)
    {
        return sqlDataStore.SelectFromMetadata(selectFromMetadataCommand, cdbName, metadata);
    }

    /// <inheritdoc/>
    public Task<Stream?> SelectFromMetadataAsync(string cdbName, Metadata metadata,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromMetadataAsync(selectFromMetadataCommand, cdbName, metadata,
            cancellationToken);
    }

    #endregion

    #endregion

    #region Texture

    #region Insert

    /// <inheritdoc/>
    public int InsertIntoTexture(string cdbName, Texture texture, byte[] content)
    {
        return sqlDataStore.InsertIntoTexture(insertIntoTextureCommand, cdbName, texture, content);
    }

    /// <inheritdoc/>
    public int InsertIntoTexture(string cdbName, Texture texture, Stream content)
    {
        return sqlDataStore.InsertIntoTexture(insertIntoTextureCommand, cdbName, texture, content);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoTextureAsync(string cdbName, Texture texture, byte[] content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTextureAsync(insertIntoTextureCommand, cdbName, texture, content,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoTextureAsync(string cdbName, Texture texture, Stream content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTextureAsync(insertIntoTextureCommand, cdbName, texture, content,
            cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public bool TrySelectFromTexture(string cdbName, Texture texture,
        Action<Stream> fileFoundAction)
    {
        return sqlDataStore.TrySelectFromTexture(selectFromTextureCommand, cdbName, texture,
            fileFoundAction);
    }

    /// <inheritdoc/>
    public Task<bool> TrySelectFromTextureAsync(string cdbName, Texture texture,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.TrySelectFromTextureAsync(selectFromTextureCommand, cdbName, texture,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? SelectFromTexture(string cdbName, Texture texture)
    {
        return sqlDataStore.SelectFromTexture(selectFromTextureCommand, cdbName, texture);
    }

    /// <inheritdoc/>
    public Task<Stream?> SelectFromTextureAsync(string cdbName, Texture texture,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromTextureAsync(selectFromTextureCommand, cdbName, texture,
            cancellationToken);
    }

    #endregion

    #endregion

    #region Texture LOD

    #region Insert

    /// <inheritdoc/>
    public int InsertIntoTextureLod(string cdbName, TextureLod textureLod, byte[] content)
    {
        return sqlDataStore.InsertIntoTextureLod(insertIntoTextureLodCommand, cdbName, textureLod, content);
    }

    /// <inheritdoc/>
    public int InsertIntoTextureLod(string cdbName, TextureLod textureLod, Stream content)
    {
        return sqlDataStore.InsertIntoTextureLod(insertIntoTextureLodCommand, cdbName, textureLod, content);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoTextureLodAsync(string cdbName, TextureLod textureLod, byte[] content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTextureLodAsync(insertIntoTextureLodCommand, cdbName, textureLod, content,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoTextureLodAsync(string cdbName, TextureLod textureLod, Stream content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTextureLodAsync(insertIntoTextureLodCommand, cdbName, textureLod, content,
            cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public bool TrySelectFromTextureLod(string cdbName, TextureLod textureLod,
        Action<Stream> fileFoundAction)
    {
        return sqlDataStore.TrySelectFromTextureLod(selectFromTextureLodCommand, cdbName, textureLod,
            fileFoundAction);
    }

    /// <inheritdoc/>
    public Task<bool> TrySelectFromTextureLodAsync(string cdbName, TextureLod textureLod,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.TrySelectFromTextureLodAsync(selectFromTextureLodCommand, cdbName, textureLod,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? SelectFromTextureLod(string cdbName, TextureLod textureLod)
    {
        return sqlDataStore.SelectFromTextureLod(selectFromTextureLodCommand, cdbName, textureLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> SelectFromTextureLodAsync(string cdbName, TextureLod textureLod,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromTextureLodAsync(selectFromTextureLodCommand, cdbName, textureLod,
            cancellationToken);
    }

    #endregion

    #endregion

    #region Geotypical Model

    #region Insert

    /// <inheritdoc/>
    public int InsertIntoGeotypicalModel(string cdbName, GeotypicalModel geotypicalModel, byte[] content)
    {
        return sqlDataStore.InsertIntoGeotypicalModel(insertIntoGeotypicalModelCommand, cdbName, geotypicalModel, content);
    }

    /// <inheritdoc/>
    public int InsertIntoGeotypicalModel(string cdbName, GeotypicalModel geotypicalModel, Stream content)
    {
        return sqlDataStore.InsertIntoGeotypicalModel(insertIntoGeotypicalModelCommand, cdbName, geotypicalModel, content);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoGeotypicalModelAsync(string cdbName, GeotypicalModel geotypicalModel, byte[] content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoGeotypicalModelAsync(insertIntoGeotypicalModelCommand, cdbName, geotypicalModel, content,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoGeotypicalModelAsync(string cdbName, GeotypicalModel geotypicalModel, Stream content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoGeotypicalModelAsync(insertIntoGeotypicalModelCommand, cdbName, geotypicalModel, content,
            cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public bool TrySelectFromGeotypicalModel(string cdbName, GeotypicalModel geotypicalModel,
        Action<Stream> fileFoundAction)
    {
        return sqlDataStore.TrySelectFromGeotypicalModel(selectFromGeotypicalModelCommand, cdbName, geotypicalModel,
            fileFoundAction);
    }

    /// <inheritdoc/>
    public Task<bool> TrySelectFromGeotypicalModelAsync(string cdbName, GeotypicalModel geotypicalModel,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.TrySelectFromGeotypicalModelAsync(selectFromGeotypicalModelCommand, cdbName, geotypicalModel,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? SelectFromGeotypicalModel(string cdbName, GeotypicalModel geotypicalModel)
    {
        return sqlDataStore.SelectFromGeotypicalModel(selectFromGeotypicalModelCommand, cdbName, geotypicalModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> SelectFromGeotypicalModelAsync(string cdbName, GeotypicalModel geotypicalModel,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromGeotypicalModelAsync(selectFromGeotypicalModelCommand, cdbName, geotypicalModel,
            cancellationToken);
    }

    #endregion

    #endregion

    #region Geotypical Model LOD

    #region Insert

    /// <inheritdoc/>
    public int InsertIntoGeotypicalModelLod(string cdbName, GeotypicalModelLod geotypicalModelLod, byte[] content)
    {
        return sqlDataStore.InsertIntoGeotypicalModelLod(insertIntoGeotypicalModelLodCommand, cdbName, geotypicalModelLod, content);
    }

    /// <inheritdoc/>
    public int InsertIntoGeotypicalModelLod(string cdbName, GeotypicalModelLod geotypicalModelLod, Stream content)
    {
        return sqlDataStore.InsertIntoGeotypicalModelLod(insertIntoGeotypicalModelLodCommand, cdbName, geotypicalModelLod, content);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoGeotypicalModelLodAsync(string cdbName, GeotypicalModelLod geotypicalModelLod, byte[] content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoGeotypicalModelLodAsync(insertIntoGeotypicalModelLodCommand, cdbName, geotypicalModelLod, content,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoGeotypicalModelLodAsync(string cdbName, GeotypicalModelLod geotypicalModelLod, Stream content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoGeotypicalModelLodAsync(insertIntoGeotypicalModelLodCommand, cdbName, geotypicalModelLod, content,
            cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public bool TrySelectFromGeotypicalModelLod(string cdbName, GeotypicalModelLod geotypicalModelLod,
        Action<Stream> fileFoundAction)
    {
        return sqlDataStore.TrySelectFromGeotypicalModelLod(selectFromGeotypicalModelLodCommand, cdbName, geotypicalModelLod,
            fileFoundAction);
    }

    /// <inheritdoc/>
    public Task<bool> TrySelectFromGeotypicalModelLodAsync(string cdbName, GeotypicalModelLod geotypicalModelLod,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.TrySelectFromGeotypicalModelLodAsync(selectFromGeotypicalModelLodCommand, cdbName, geotypicalModelLod,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? SelectFromGeotypicalModelLod(string cdbName, GeotypicalModelLod geotypicalModelLod)
    {
        return sqlDataStore.SelectFromGeotypicalModelLod(selectFromGeotypicalModelLodCommand, cdbName, geotypicalModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> SelectFromGeotypicalModelLodAsync(string cdbName, GeotypicalModelLod geotypicalModelLod,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromGeotypicalModelLodAsync(selectFromGeotypicalModelLodCommand, cdbName, geotypicalModelLod,
            cancellationToken);
    }

    #endregion

    #endregion

    #region Moving Model

    #region Insert

    /// <inheritdoc/>
    public int InsertIntoMovingModel(string cdbName, MovingModel movingModel, byte[] content)
    {
        return sqlDataStore.InsertIntoMovingModel(insertIntoMovingModelCommand, cdbName, movingModel, content);
    }

    /// <inheritdoc/>
    public int InsertIntoMovingModel(string cdbName, MovingModel movingModel, Stream content)
    {
        return sqlDataStore.InsertIntoMovingModel(insertIntoMovingModelCommand, cdbName, movingModel, content);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoMovingModelAsync(string cdbName, MovingModel movingModel, byte[] content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoMovingModelAsync(insertIntoMovingModelCommand, cdbName, movingModel, content,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoMovingModelAsync(string cdbName, MovingModel movingModel, Stream content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoMovingModelAsync(insertIntoMovingModelCommand, cdbName, movingModel, content,
            cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public bool TrySelectFromMovingModel(string cdbName, MovingModel movingModel,
        Action<Stream> fileFoundAction)
    {
        return sqlDataStore.TrySelectFromMovingModel(selectFromMovingModelCommand, cdbName, movingModel,
            fileFoundAction);
    }

    /// <inheritdoc/>
    public Task<bool> TrySelectFromMovingModelAsync(string cdbName, MovingModel movingModel,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.TrySelectFromMovingModelAsync(selectFromMovingModelCommand, cdbName, movingModel,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? SelectFromMovingModel(string cdbName, MovingModel movingModel)
    {
        return sqlDataStore.SelectFromMovingModel(selectFromMovingModelCommand, cdbName, movingModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> SelectFromMovingModelAsync(string cdbName, MovingModel movingModel,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromMovingModelAsync(selectFromMovingModelCommand, cdbName, movingModel,
            cancellationToken);
    }

    #endregion

    #endregion

    #region Moving Model LOD

    #region Insert

    /// <inheritdoc/>
    public int InsertIntoMovingModelLod(string cdbName, MovingModelLod movingModelLod, byte[] content)
    {
        return sqlDataStore.InsertIntoMovingModelLod(insertIntoMovingModelLodCommand, cdbName, movingModelLod, content);
    }

    /// <inheritdoc/>
    public int InsertIntoMovingModelLod(string cdbName, MovingModelLod movingModelLod, Stream content)
    {
        return sqlDataStore.InsertIntoMovingModelLod(insertIntoMovingModelLodCommand, cdbName, movingModelLod, content);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoMovingModelLodAsync(string cdbName, MovingModelLod movingModelLod, byte[] content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoMovingModelLodAsync(insertIntoMovingModelLodCommand, cdbName, movingModelLod, content,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoMovingModelLodAsync(string cdbName, MovingModelLod movingModelLod, Stream content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoMovingModelLodAsync(insertIntoMovingModelLodCommand, cdbName, movingModelLod, content,
            cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public bool TrySelectFromMovingModelLod(string cdbName, MovingModelLod movingModelLod,
        Action<Stream> fileFoundAction)
    {
        return sqlDataStore.TrySelectFromMovingModelLod(selectFromMovingModelLodCommand, cdbName, movingModelLod, fileFoundAction);
    }

    /// <inheritdoc/>
    public Task<bool> TrySelectFromMovingModelLodAsync(string cdbName, MovingModelLod movingModelLod,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.TrySelectFromMovingModelLodAsync(selectFromMovingModelLodCommand, cdbName, movingModelLod,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? SelectFromMovingModelLod(string cdbName, MovingModelLod movingModelLod)
    {
        return sqlDataStore.SelectFromMovingModelLod(selectFromMovingModelLodCommand, cdbName, movingModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> SelectFromMovingModelLodAsync(string cdbName, MovingModelLod movingModelLod,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromMovingModelLodAsync(selectFromMovingModelLodCommand, cdbName, movingModelLod,
            cancellationToken);
    }

    #endregion

    #endregion

    #region Tile

    #region Insert

    /// <inheritdoc/>
    public int InsertIntoTile(string cdbName, Tile tile, byte[] content)
    {
        return sqlDataStore.InsertIntoTile(insertIntoTileCommand, cdbName, tile, content);
    }

    /// <inheritdoc/>
    public int InsertIntoTile(string cdbName, Tile tile, Stream content)
    {
        return sqlDataStore.InsertIntoTile(insertIntoTileCommand, cdbName, tile, content);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoTileAsync(string cdbName, Tile tile, byte[] content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTileAsync(insertIntoTileCommand, cdbName, tile, content,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoTileAsync(string cdbName, Tile tile, Stream content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTileAsync(insertIntoTileCommand, cdbName, tile, content,
            cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public bool TrySelectFromTile(string cdbName, Tile tile,
        Action<Stream> fileFoundAction)
    {
        return sqlDataStore.TrySelectFromTile(selectFromTileCommand, cdbName, tile, fileFoundAction);
    }

    /// <inheritdoc/>
    public Task<bool> TrySelectFromTileAsync(string cdbName, Tile tile,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.TrySelectFromTileAsync(selectFromTileCommand, cdbName, tile, fileFoundAsyncAction,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? SelectFromTile(string cdbName, Tile tile)
    {
        return sqlDataStore.SelectFromTile(selectFromTileCommand, cdbName, tile);
    }

    /// <inheritdoc/>
    public Task<Stream?> SelectFromTileAsync(string cdbName, Tile tile,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromTileAsync(selectFromTileCommand, cdbName, tile,
            cancellationToken);
    }

    #endregion

    #endregion

    #region Tile Archived Feature

    #region Insert

    /// <inheritdoc/>
    public int InsertIntoTileArchivedFeature(string cdbName, TileArchivedFeature tileArchivedFeature, byte[] content)
    {
        return sqlDataStore.InsertIntoTileArchivedFeature(insertIntoTileArchivedFeatureCommand, cdbName, tileArchivedFeature, content);
    }

    /// <inheritdoc/>
    public int InsertIntoTileArchivedFeature(string cdbName, TileArchivedFeature tileArchivedFeature, Stream content)
    {
        return sqlDataStore.InsertIntoTileArchivedFeature(insertIntoTileArchivedFeatureCommand, cdbName, tileArchivedFeature, content);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoTileArchivedFeatureAsync(string cdbName, TileArchivedFeature tileArchivedFeature, byte[] content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTileArchivedFeatureAsync(insertIntoTileArchivedFeatureCommand, cdbName, tileArchivedFeature, content,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoTileArchivedFeatureAsync(string cdbName, TileArchivedFeature tileArchivedFeature, Stream content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTileArchivedFeatureAsync(insertIntoTileArchivedFeatureCommand, cdbName, tileArchivedFeature, content,
            cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public bool TrySelectFromTileArchivedFeature(string cdbName, TileArchivedFeature tileArchivedFeature,
        Action<Stream> fileFoundAction)
    {
        return sqlDataStore.TrySelectFromTileArchivedFeature(selectFromTileArchivedFeatureCommand, cdbName, tileArchivedFeature,
            fileFoundAction);
    }

    /// <inheritdoc/>
    public Task<bool> TrySelectFromTileArchivedFeatureAsync(string cdbName, TileArchivedFeature tileArchivedFeature,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.TrySelectFromTileArchivedFeatureAsync(selectFromTileArchivedFeatureCommand, cdbName, tileArchivedFeature,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? SelectFromTileArchivedFeature(string cdbName, TileArchivedFeature tileArchivedFeature)
    {
        return sqlDataStore.SelectFromTileArchivedFeature(selectFromTileArchivedFeatureCommand, cdbName, tileArchivedFeature);
    }

    /// <inheritdoc/>
    public Task<Stream?> SelectFromTileArchivedFeatureAsync(string cdbName, TileArchivedFeature tileArchivedFeature,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromTileArchivedFeatureAsync(selectFromTileArchivedFeatureCommand, cdbName, tileArchivedFeature,
            cancellationToken);
    }

    #endregion

    #endregion

    #region Tile Archived Texture

    #region Insert

    /// <inheritdoc/>
    public int InsertIntoTileArchivedTexture(string cdbName, TileArchivedTexture tileArchivedTexture, byte[] content)
    {
        return sqlDataStore.InsertIntoTileArchivedTexture(insertIntoTileArchivedTextureCommand, cdbName, tileArchivedTexture, content);
    }

    /// <inheritdoc/>
    public int InsertIntoTileArchivedTexture(string cdbName, TileArchivedTexture tileArchivedTexture, Stream content)
    {
        return sqlDataStore.InsertIntoTileArchivedTexture(insertIntoTileArchivedTextureCommand, cdbName, tileArchivedTexture, content);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoTileArchivedTextureAsync(string cdbName, TileArchivedTexture tileArchivedTexture, byte[] content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTileArchivedTextureAsync(insertIntoTileArchivedTextureCommand,
            cdbName, tileArchivedTexture, content,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoTileArchivedTextureAsync(string cdbName, TileArchivedTexture tileArchivedTexture, Stream content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoTileArchivedTextureAsync(insertIntoTileArchivedTextureCommand,
            cdbName, tileArchivedTexture, content,
            cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public bool TrySelectFromTileArchivedTexture(string cdbName, TileArchivedTexture tileArchivedTexture,
        Action<Stream> fileFoundAction)
    {
        return sqlDataStore.TrySelectFromTileArchivedTexture(selectFromTileArchivedTextureCommand, cdbName, tileArchivedTexture,
            fileFoundAction);
    }

    /// <inheritdoc/>
    public Task<bool> TrySelectFromTileArchivedTextureAsync(string cdbName, TileArchivedTexture tileArchivedTexture,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.TrySelectFromTileArchivedTextureAsync(selectFromTileArchivedTextureCommand, cdbName, tileArchivedTexture,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? SelectFromTileArchivedTexture(string cdbName, TileArchivedTexture tileArchivedTexture)
    {
        return sqlDataStore.SelectFromTileArchivedTexture(selectFromTileArchivedTextureCommand, cdbName, tileArchivedTexture);
    }

    /// <inheritdoc/>
    public Task<Stream?> SelectFromTileArchivedTextureAsync(string cdbName, TileArchivedTexture tileArchivedTexture,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromTileArchivedTextureAsync(selectFromTileArchivedTextureCommand, cdbName, tileArchivedTexture,
            cancellationToken);
    }

    #endregion

    #endregion

    #region Navigation

    #region Insert

    /// <inheritdoc/>
    public int InsertIntoNavigation(string cdbName, Navigation navigation, byte[] content)
    {
        return sqlDataStore.InsertIntoNavigation(insertIntoNavigationCommand, cdbName, navigation, content);
    }

    /// <inheritdoc/>
    public int InsertIntoNavigation(string cdbName, Navigation navigation, Stream content)
    {
        return sqlDataStore.InsertIntoNavigation(insertIntoNavigationCommand, cdbName, navigation, content);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoNavigationAsync(string cdbName, Navigation navigation, byte[] content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoNavigationAsync(insertIntoNavigationCommand, cdbName, navigation, content,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> InsertIntoNavigationAsync(string cdbName, Navigation navigation, Stream content,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.InsertIntoNavigationAsync(insertIntoNavigationCommand, cdbName, navigation, content,
            cancellationToken);
    }

    #endregion

    #region Select

    /// <inheritdoc/>
    public bool TrySelectFromNavigation(string cdbName, Navigation navigation,
        Action<Stream> fileFoundAction)
    {
        return sqlDataStore.TrySelectFromNavigation(selectFromNavigationCommand, cdbName, navigation,
            fileFoundAction);
    }

    /// <inheritdoc/>
    public Task<bool> TrySelectFromNavigationAsync(string cdbName, Navigation navigation,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.TrySelectFromNavigationAsync(selectFromNavigationCommand, cdbName, navigation,
            fileFoundAsyncAction,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Stream? SelectFromNavigation(string cdbName, Navigation navigation)
    {
        return sqlDataStore.SelectFromNavigation(selectFromNavigationCommand, cdbName, navigation);
    }

    /// <inheritdoc/>
    public Task<Stream?> SelectFromNavigationAsync(string cdbName, Navigation navigation,
        CancellationToken cancellationToken)
    {
        return sqlDataStore.SelectFromNavigationAsync(selectFromNavigationCommand, cdbName, navigation,
            cancellationToken);
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
