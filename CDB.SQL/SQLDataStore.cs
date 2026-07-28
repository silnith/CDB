using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Silnith.CDB.SQL;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

/// <summary>
/// A CDB data store that uses an SQL database for its storage.
/// </summary>
/// <remarks>
/// <para>
/// This is an abstract base class.  In order to actually use this, you need to
/// instantiate one of the concrete subclasses provided in the vendor-specific
/// projects.
/// </para>
/// </remarks>
public abstract class SQLDataStore : ISQLDataStore
{
    /// <summary>
    /// Creates a parameter for a database command, sets the name and type of
    /// the parameter, and attaches the parameter to the command.
    /// </summary>
    /// <param name="dbCommand">The command to create a parameter for.</param>
    /// <param name="dbParameterName">The name of the parameter.
    /// Each database system has its own syntax for how these parameters should
    /// be named.</param>
    /// <param name="dbType">The parameter type.</param>
    private static void CreateAndAttachParameter(DbCommand dbCommand, string dbParameterName, DbType dbType)
    {
        DbParameter dbParameter = dbCommand.CreateParameter();
        dbCommand.Parameters.Add(dbParameter);
        dbParameter.DbType = dbType;
        dbParameter.ParameterName = dbParameterName;
    }

    internal readonly DbDataSource dbDataSource;

    /// <summary>
    /// Creates a new CDB storage backend using the provided SQL connection
    /// string.
    /// </summary>
    /// <param name="dbDataSource">The data source.</param>
    /// <param name="options">Configurable settings.</param>
    protected SQLDataStore(DbDataSource dbDataSource, IOptions<SQLDataStoreSettings> options)
    {
        ArgumentNullException.ThrowIfNull(dbDataSource);
        ArgumentNullException.ThrowIfNull(options);

        this.dbDataSource = dbDataSource;

        if (options.Value.CreateSchema)
        {
            CreateSchema();
        }
    }

    public PersistentConnection GetPersistentConnection()
    {
        return new PersistentConnection(this);
    }

    private void CreateSchema()
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();

        CreateSchema(dbConnection);
    }

    protected internal virtual void CreateSchema(DbConnection dbConnection)
    {
        using DbTransaction dbTransaction = dbConnection.BeginTransaction(IsolationLevel.Serializable);

        using DbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.Transaction = dbTransaction;

        dbCommand.CommandText = CreateTableCDBStatement;
        _ = dbCommand.ExecuteNonQuery();

        dbCommand.CommandText = CreateTableMetadataStatement;
        _ = dbCommand.ExecuteNonQuery();

        // Need an index on texture name.
        dbCommand.CommandText = CreateTableTextureStatement;
        _ = dbCommand.ExecuteNonQuery();

        // Need an index on texture name.
        dbCommand.CommandText = CreateTableTextureLodStatement;
        _ = dbCommand.ExecuteNonQuery();

        // Need an index on dataset (for everything)
        // Need an index on feature_category, feature_subcategory, feature_type
        dbCommand.CommandText = CreateTableGeotypicalModelStatement;
        _ = dbCommand.ExecuteNonQuery();

        // Need an index on feature_category, feature_subcategory, feature_type, lod
        dbCommand.CommandText = CreateTableGeotypicalModelLodStatement;
        _ = dbCommand.ExecuteNonQuery();

        // Maybe an index on kind, domain, country, category.
        // Need an index on kind, domain, country, category, subcategory, specific, extra.
        dbCommand.CommandText = CreateTableMovingModelStatement;
        _ = dbCommand.ExecuteNonQuery();

        // Maybe an index on kind, domain, country, category.
        // Need an index on kind, domain, country, category, subcategory, specific, extra.
        dbCommand.CommandText = CreateTableMovingModelLodStatement;
        _ = dbCommand.ExecuteNonQuery();

        // Need an index on latitude, longitude, dataset, cs1, cs2, lod, up
        dbCommand.CommandText = CreateTableTileStatement;
        _ = dbCommand.ExecuteNonQuery();

        // Need an index on latitude, longitude, dataset, cs1, cs2, lod, up
        dbCommand.CommandText = CreateTableTileArchivedFeatureStatement;
        _ = dbCommand.ExecuteNonQuery();

        // Need an index on latitude, longitude, dataset, cs1, cs2, lod, up
        dbCommand.CommandText = CreateTableTileArchivedTextureStatement;
        _ = dbCommand.ExecuteNonQuery();

        dbCommand.CommandText = CreateTableNavigationStatement;
        _ = dbCommand.ExecuteNonQuery();

        dbTransaction.Commit();
    }

    #region Shared SQL Parameters

    /// <summary>
    /// The name of the SQL parameter for the CDB name.
    /// The value must be of type <see cref="DbType.String"/>.
    /// </summary>
    protected abstract string CdbParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for a Tile latitude.
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string LatitudeParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for a Tile longitude.
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string LongitudeParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the dataset.
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string DatasetParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the component selector 1.
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string ComponentSelector1ParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the component selector 2.
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string ComponentSelector2ParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the level of detail.
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string LevelOfDetailParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for a Tile UREF.
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string UpParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for a Tile RREF.
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string RightParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the metadata name.
    /// The value must be of type <see cref="DbType.String"/>.
    /// </summary>
    protected abstract string MetadataNameParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the texture name.
    /// The value must be of type <see cref="DbType.String"/>.
    /// </summary>
    protected abstract string TextureNameParamName
    {
        get;
    }

    #region Feature Code Parameters

    /// <summary>
    /// The name of the SQL parameter for the Feature Code category.
    /// The value must be of type <see cref="DbType.String"/>.
    /// </summary>
    protected abstract string FeatureCategoryParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the Feature Code subcategory.
    /// The value must be of type <see cref="DbType.String"/>.
    /// </summary>
    protected abstract string FeatureSubcategoryParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the Feature Code type.
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string FeatureTypeParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the Feature Code subcode.
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string FeatureSubcodeParamName
    {
        get;
    }

    #endregion

    /// <summary>
    /// The name of the SQL parameter for the geotypical model name.
    /// The value must be of type <see cref="DbType.String"/>.
    /// </summary>
    protected abstract string ModelNameParamName
    {
        get;
    }

    #region DIS Code Parameters

    /// <summary>
    /// The name of the SQL parameter for the DIS Code component "kind".
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string DISKindParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the DIS Code component "domain".
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string DISDomainParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the DIS Code component "country".
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string DISCountryParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the DIS Code component "category".
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string DISCategoryParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the DIS Code component "subcategory".
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string DISSubcategoryParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the DIS Code component "specific".
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string DISSpecificParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the DIS Code component "extra".
    /// The value must be of type <see cref="DbType.Int32"/>.
    /// </summary>
    protected abstract string DISExtraParamName
    {
        get;
    }

    #endregion

    /// <summary>
    /// The name of the SQL parameter for the file type.
    /// The value must be of type <see cref="DbType.String"/>.
    /// </summary>
    protected abstract string FileTypeParamName
    {
        get;
    }

    /// <summary>
    /// The name of the SQL parameter for the file content.
    /// The value must be of type <see cref="DbType.Binary"/>.
    /// </summary>
    protected abstract string ContentParamName
    {
        get;
    }

    /// <summary>
    /// The name of the column in the CDB table that contains the CDB name.
    /// The type is <see cref="DbType.String"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Got that?
    /// </para>
    /// </remarks>
    protected abstract string CDBNameColumnName
    {
        get;
    }

    /// <summary>
    /// The name of the column (in most tables) that contains the file contents.
    /// The type is <see cref="DbType.Binary"/>.
    /// </summary>
    protected abstract string ContentColumnName
    {
        get;
    }

    #endregion

    #region CDB

    /// <summary>
    /// The SQL DDL statement that creates the CDB table with one column for
    /// the name of the CDB instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The name of the column is <see cref="CDBNameColumnName"/>.
    /// </para>
    /// </remarks>
    protected abstract string CreateTableCDBStatement
    {
        get;
    }

    #region Insert

    /// <summary>
    /// The SQL statement that inserts a new name into the CDB table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string InsertIntoCDBStatement
    {
        get;
    }

    internal void InitializeInsertIntoCDBCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = InsertIntoCDBStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
    }

    /// <inheritdoc/>
    public int InsertIntoCDB(string cdbName)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoCDBCommand = dbConnection.CreateCommand();
        InitializeInsertIntoCDBCommand(insertIntoCDBCommand);
        insertIntoCDBCommand.Prepare();

        return InsertIntoCDB(insertIntoCDBCommand, cdbName);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoCDB(string)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoCDBCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoCDB(DbCommand insertIntoCDBCommand,
        string cdbName)
    {
        insertIntoCDBCommand.Parameters[CdbParamName].Value = cdbName;

        return insertIntoCDBCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoCDBAsync(string cdbName, CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoCDBCommand = dbConnection.CreateCommand();
        InitializeInsertIntoCDBCommand(insertIntoCDBCommand);
        await insertIntoCDBCommand.PrepareAsync(cancellationToken);

        return await InsertIntoCDBAsync(insertIntoCDBCommand, cdbName, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoCDBAsync(string, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoCDBCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoCDBAsync(DbCommand insertIntoCDBCommand,
        string cdbName, CancellationToken cancellationToken = default)
    {
        insertIntoCDBCommand.Parameters[CdbParamName].Value = cdbName;

        return insertIntoCDBCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    #endregion

    #region Select

    /// <summary>
    /// The SQL statement that selects the CDB name from the CDB table.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This has no parameters.
    /// </para>
    /// <list type="bullet">
    ///   <listheader><description>Selected Columns</description></listheader>
    ///   <item><description><see cref="CDBNameColumnName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string SelectFromCDBStatement
    {
        get;
    }

    internal void InitializeSelectFromCDBCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = SelectFromCDBStatement;
    }

    /// <inheritdoc/>
    public IEnumerable<string> SelectFromCDB()
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand selectFromCDBCommand = dbConnection.CreateCommand();
        InitializeSelectFromCDBCommand(selectFromCDBCommand);
        selectFromCDBCommand.Prepare();

        return SelectFromCDB(selectFromCDBCommand);
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromCDBAsync(CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromCDBCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual IEnumerable<string> SelectFromCDB(DbCommand selectFromCDBCommand)
    {
        using DbDataReader dbDataReader = selectFromCDBCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult);
        do
        {
            while (dbDataReader.Read())
            {
                string name = dbDataReader.GetString(CDBNameColumnName);
                yield return name;
            }
        } while (dbDataReader.NextResult());
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> SelectFromCDBAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand selectFromCDBCommand = dbConnection.CreateCommand();
        InitializeSelectFromCDBCommand(selectFromCDBCommand);
        await selectFromCDBCommand.PrepareAsync(cancellationToken);

        await foreach (string cdb in SelectFromCDBAsync(selectFromCDBCommand, cancellationToken).WithCancellation(cancellationToken))
        {
            yield return cdb;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromCDBAsync(CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromCDBCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async IAsyncEnumerable<string> SelectFromCDBAsync(DbCommand selectFromCDBCommand,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using DbDataReader dbDataReader = await selectFromCDBCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult, cancellationToken);
        do
        {
            while (await dbDataReader.ReadAsync(cancellationToken))
            {
                string name = dbDataReader.GetString(CDBNameColumnName);
                yield return name;
            }
        } while (await dbDataReader.NextResultAsync(cancellationToken));
    }

    #endregion

    #endregion

    #region Metadata

    /// <summary>
    /// The SQL DDL statement to create the Metadata table.
    /// </summary>
    protected abstract string CreateTableMetadataStatement
    {
        get;
    }

    #region Insert

    /// <summary>
    /// The SQL statement to insert a row into the Metadata table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="MetadataNameParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    ///   <item><description><see cref="ContentParamName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string InsertIntoMetadataStatement
    {
        get;
    }

    private void CreateAndAttachMetadataParameters(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, MetadataNameParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, FileTypeParamName, DbType.String);
    }

    private void SetMetadataParameters(DbCommand dbCommand, Metadata metadata)
    {
        dbCommand.Parameters[MetadataNameParamName].Value = metadata.Name;
        dbCommand.Parameters[FileTypeParamName].Value = metadata.FileType;
    }

    internal void InitializeInsertIntoMetadataCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = InsertIntoMetadataStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachMetadataParameters(dbCommand);
        CreateAndAttachParameter(dbCommand, ContentParamName, DbType.Binary);
    }

    /// <inheritdoc/>
    public int InsertIntoMetadata(string cdbName, Metadata metadata, byte[] content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoMetadataCommand = dbConnection.CreateCommand();
        InitializeInsertIntoMetadataCommand(insertIntoMetadataCommand);
        insertIntoMetadataCommand.Prepare();

        return InsertIntoMetadata(insertIntoMetadataCommand, cdbName, metadata, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoMetadata(string, Metadata, byte[])"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoMetadataCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoMetadata(DbCommand insertIntoMetadataCommand,
        string cdbName, Metadata metadata, byte[] content)
    {
        insertIntoMetadataCommand.Parameters[CdbParamName].Value = cdbName;
        SetMetadataParameters(insertIntoMetadataCommand, metadata);
        insertIntoMetadataCommand.Parameters[ContentParamName].Value = content;

        return insertIntoMetadataCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public int InsertIntoMetadata(string cdbName, Metadata metadata, Stream content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoMetadataCommand = dbConnection.CreateCommand();
        InitializeInsertIntoMetadataCommand(insertIntoMetadataCommand);
        insertIntoMetadataCommand.Prepare();

        return InsertIntoMetadata(insertIntoMetadataCommand, cdbName, metadata, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoMetadata(string, Metadata, Stream)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoMetadataCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoMetadata(DbCommand insertIntoMetadataCommand,
        string cdbName, Metadata metadata, Stream content)
    {
        insertIntoMetadataCommand.Parameters[CdbParamName].Value = cdbName;
        SetMetadataParameters(insertIntoMetadataCommand, metadata);
        insertIntoMetadataCommand.Parameters[ContentParamName].Value = content;

        return insertIntoMetadataCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoMetadataAsync(string cdbName, Metadata metadata, byte[] content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoMetadataCommand = dbConnection.CreateCommand();
        InitializeInsertIntoMetadataCommand(insertIntoMetadataCommand);
        await insertIntoMetadataCommand.PrepareAsync(cancellationToken);

        return await InsertIntoMetadataAsync(insertIntoMetadataCommand, cdbName, metadata, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoMetadataAsync(string, Metadata, byte[], CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoMetadataCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoMetadataAsync(DbCommand insertIntoMetadataCommand,
        string cdbName, Metadata metadata, byte[] content,
        CancellationToken cancellationToken)
    {
        insertIntoMetadataCommand.Parameters[CdbParamName].Value = cdbName;
        SetMetadataParameters(insertIntoMetadataCommand, metadata);
        insertIntoMetadataCommand.Parameters[ContentParamName].Value = content;

        return insertIntoMetadataCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoMetadataAsync(string cdbName, Metadata metadata, Stream content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoMetadataCommand = dbConnection.CreateCommand();
        InitializeInsertIntoMetadataCommand(insertIntoMetadataCommand);
        await insertIntoMetadataCommand.PrepareAsync(cancellationToken);

        return await InsertIntoMetadataAsync(insertIntoMetadataCommand, cdbName, metadata, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoMetadataAsync(string, Metadata, Stream, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoMetadataCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoMetadataAsync(DbCommand insertIntoMetadataCommand,
        string cdbName, Metadata metadata, Stream content,
        CancellationToken cancellationToken)
    {
        insertIntoMetadataCommand.Parameters[CdbParamName].Value = cdbName;
        SetMetadataParameters(insertIntoMetadataCommand, metadata);
        insertIntoMetadataCommand.Parameters[ContentParamName].Value = content;

        return insertIntoMetadataCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    #endregion

    #region Select

    /// <summary>
    /// The SQL statement to select a row from the Metadata table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="MetadataNameParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// <list type="bullet">
    ///   <listheader><description>Selected Columns</description></listheader>
    ///   <item><description><see cref="ContentColumnName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string SelectFromMetadataStatement
    {
        get;
    }

    internal void InitializeSelectFromMetadataCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = SelectFromMetadataStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachMetadataParameters(dbCommand);
    }

    /// <inheritdoc/>
    public bool TrySelectFromMetadata(string cdbName, Metadata metadata, Action<Stream> fileFoundAction)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand selectFromMetadataCommand = dbConnection.CreateCommand();
        InitializeSelectFromMetadataCommand(selectFromMetadataCommand);
        selectFromMetadataCommand.Prepare();

        return TrySelectFromMetadata(selectFromMetadataCommand, cdbName, metadata,
            fileFoundAction);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromMetadata(string, Metadata, Action{Stream})"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromMetadataCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual bool TrySelectFromMetadata(DbCommand selectFromMetadataCommand,
        string cdbName, Metadata metadata,
        Action<Stream> fileFoundAction)
    {
        selectFromMetadataCommand.Parameters[CdbParamName].Value = cdbName;
        SetMetadataParameters(selectFromMetadataCommand, metadata);

        using DbDataReader dbDataReader = selectFromMetadataCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        do
        {
            while (dbDataReader.Read())
            {
                using Stream stream = dbDataReader.GetStream(ContentColumnName);
                fileFoundAction(stream);
                return true;
            }
        } while (dbDataReader.NextResult());
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> TrySelectFromMetadataAsync(string cdbName, Metadata metadata,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand selectFromMetadataCommand = dbConnection.CreateCommand();
        InitializeSelectFromMetadataCommand(selectFromMetadataCommand);
        await selectFromMetadataCommand.PrepareAsync(cancellationToken);

        return await TrySelectFromMetadataAsync(selectFromMetadataCommand, cdbName, metadata,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromMetadataAsync(string, Metadata, Func{Stream, CancellationToken, Task}, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromMetadataCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<bool> TrySelectFromMetadataAsync(DbCommand selectFromMetadataCommand,
        string cdbName, Metadata metadata,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        selectFromMetadataCommand.Parameters[CdbParamName].Value = cdbName;
        SetMetadataParameters(selectFromMetadataCommand, metadata);

        await using DbDataReader dbDataReader = await selectFromMetadataCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        do
        {
            while (await dbDataReader.ReadAsync(cancellationToken))
            {
                await using Stream stream = dbDataReader.GetStream(ContentColumnName);
                await fileFoundAsyncAction(stream, cancellationToken);
                return true;
            }
        } while (await dbDataReader.NextResultAsync(cancellationToken));
        return false;
    }

    /// <inheritdoc/>
    public Stream? SelectFromMetadata(string cdbName, Metadata metadata)
    {
        DbConnection dbConnection = dbDataSource.OpenConnection();
        try
        {
            DbCommand selectFromMetadataCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromMetadataCommand(selectFromMetadataCommand);
                selectFromMetadataCommand.Prepare();

                Stream? stream = SelectFromMetadata(selectFromMetadataCommand, cdbName, metadata);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromMetadataCommand, dbConnection);
                }
                else
                {
                    selectFromMetadataCommand.Dispose();
                    dbConnection.Dispose();
                    return null;
                }
            }
            catch (Exception)
            {
                selectFromMetadataCommand.Dispose();
                throw;
            }
        }
        catch (Exception)
        {
            dbConnection.Dispose();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromMetadata(string, Metadata)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromMetadataCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Stream? SelectFromMetadata(DbCommand selectFromMetadataCommand,
        string cdbName, Metadata metadata)
    {
        selectFromMetadataCommand.Parameters[CdbParamName].Value = cdbName;
        SetMetadataParameters(selectFromMetadataCommand, metadata);

        DbDataReader dbDataReader = selectFromMetadataCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        try
        {
            do
            {
                while (dbDataReader.Read())
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (dbDataReader.NextResult());
            dbDataReader.Dispose();
            return null;
        }
        catch (Exception)
        {
            dbDataReader.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Stream?> SelectFromMetadataAsync(string cdbName, Metadata metadata,
        CancellationToken cancellationToken)
    {
        DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            DbCommand selectFromMetadataCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromMetadataCommand(selectFromMetadataCommand);
                await selectFromMetadataCommand.PrepareAsync(cancellationToken);

                Stream? stream = await SelectFromMetadataAsync(selectFromMetadataCommand, cdbName, metadata, cancellationToken);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromMetadataCommand, dbConnection);
                }
                else
                {
                    await selectFromMetadataCommand.DisposeAsync();
                    await dbConnection.DisposeAsync();
                    return null;
                }
            }
            catch (Exception)
            {
                await selectFromMetadataCommand.DisposeAsync();
                throw;
            }
        }
        catch (Exception)
        {
            await dbConnection.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromMetadataAsync(string, Metadata, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromMetadataCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<Stream?> SelectFromMetadataAsync(DbCommand selectFromMetadataCommand,
        string cdbName, Metadata metadata,
        CancellationToken cancellationToken)
    {
        selectFromMetadataCommand.Parameters[CdbParamName].Value = cdbName;
        SetMetadataParameters(selectFromMetadataCommand, metadata);

        DbDataReader dbDataReader = await selectFromMetadataCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        try
        {
            do
            {
                while (await dbDataReader.ReadAsync(cancellationToken))
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (await dbDataReader.NextResultAsync(cancellationToken));
            await dbDataReader.DisposeAsync();
            return null;
        }
        catch (Exception)
        {
            await dbDataReader.DisposeAsync();
            throw;
        }
    }

    #endregion

    #endregion

    #region Texture

    /// <summary>
    /// The SQL DDL statement to create the Texture table.
    /// </summary>
    protected abstract string CreateTableTextureStatement
    {
        get;
    }

    #region Insert

    /// <summary>
    /// The SQL statement to insert a row into the Texture table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="TextureNameParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    ///   <item><description><see cref="ContentParamName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string InsertIntoTextureStatement
    {
        get;
    }

    internal void InitializeInsertIntoTextureCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = InsertIntoTextureStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachTextureParameters(dbCommand);
        CreateAndAttachParameter(dbCommand, ContentParamName, DbType.Binary);
    }

    private void CreateAndAttachTextureParameters(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, DatasetParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector1ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector2ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, TextureNameParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, FileTypeParamName, DbType.String);
    }

    private void SetTextureParameters(DbCommand dbCommand, Texture texture)
    {
        dbCommand.Parameters[DatasetParamName].Value = texture.Dataset.Value;
        dbCommand.Parameters[ComponentSelector1ParamName].Value = texture.ComponentSelector1;
        dbCommand.Parameters[ComponentSelector2ParamName].Value = texture.ComponentSelector2;
        dbCommand.Parameters[TextureNameParamName].Value = texture.Name;
        dbCommand.Parameters[FileTypeParamName].Value = texture.FileType;
    }

    /// <inheritdoc/>
    public int InsertIntoTexture(string cdbName, Texture texture, byte[] content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoTextureCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTextureCommand(insertIntoTextureCommand);
        insertIntoTextureCommand.Prepare();

        return InsertIntoTexture(insertIntoTextureCommand, cdbName, texture, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTexture(string, Texture, byte[])"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoTexture(DbCommand insertIntoTextureCommand,
        string cdbName, Texture texture, byte[] content)
    {
        insertIntoTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureParameters(insertIntoTextureCommand, texture);
        insertIntoTextureCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTextureCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public int InsertIntoTexture(string cdbName, Texture texture, Stream content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoTextureCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTextureCommand(insertIntoTextureCommand);
        insertIntoTextureCommand.Prepare();

        return InsertIntoTexture(insertIntoTextureCommand, cdbName, texture, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTexture(string, Texture, Stream)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoTexture(DbCommand insertIntoTextureCommand,
        string cdbName, Texture texture, Stream content)
    {
        insertIntoTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureParameters(insertIntoTextureCommand, texture);
        insertIntoTextureCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTextureCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoTextureAsync(string cdbName, Texture texture, byte[] content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoTextureCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTextureCommand(insertIntoTextureCommand);
        await insertIntoTextureCommand.PrepareAsync(cancellationToken);

        return await InsertIntoTextureAsync(insertIntoTextureCommand, cdbName, texture, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTextureAsync(string, Texture, byte[], CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoTextureAsync(DbCommand insertIntoTextureCommand,
        string cdbName, Texture texture, byte[] content,
        CancellationToken cancellationToken)
    {
        insertIntoTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureParameters(insertIntoTextureCommand, texture);
        insertIntoTextureCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTextureCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoTextureAsync(string cdbName, Texture texture, Stream content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoTextureCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTextureCommand(insertIntoTextureCommand);
        await insertIntoTextureCommand.PrepareAsync(cancellationToken);

        return await InsertIntoTextureAsync(insertIntoTextureCommand, cdbName, texture, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTextureAsync(string, Texture, Stream, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoTextureAsync(DbCommand insertIntoTextureCommand,
        string cdbName, Texture texture, Stream content,
        CancellationToken cancellationToken)
    {
        insertIntoTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureParameters(insertIntoTextureCommand, texture);
        insertIntoTextureCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTextureCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    #endregion

    #region Select

    /// <summary>
    /// The SQL statement to select a row from the Texture table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="TextureNameParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// <list type="bullet">
    ///   <listheader><description>Selected Columns</description></listheader>
    ///   <item><description><see cref="ContentColumnName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string SelectFromTextureStatement
    {
        get;
    }

    internal void InitializeSelectFromTextureCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = SelectFromTextureStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachTextureParameters(dbCommand);
    }

    /// <inheritdoc/>
    public bool TrySelectFromTexture(string cdbName, Texture texture,
        Action<Stream> fileFoundAction)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand selectFromTextureCommand = dbConnection.CreateCommand();
        InitializeSelectFromTextureCommand(selectFromTextureCommand);
        selectFromTextureCommand.Prepare();

        return TrySelectFromTexture(selectFromTextureCommand, cdbName, texture,
            fileFoundAction);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromTexture(string, Texture, Action{Stream})"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual bool TrySelectFromTexture(DbCommand selectFromTextureCommand,
        string cdbName, Texture texture,
        Action<Stream> fileFoundAction)
    {
        selectFromTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureParameters(selectFromTextureCommand, texture);

        using DbDataReader dbDataReader = selectFromTextureCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        do
        {
            while (dbDataReader.Read())
            {
                using Stream stream = dbDataReader.GetStream(ContentColumnName);
                fileFoundAction(stream);
                return true;
            }
        } while (dbDataReader.NextResult());
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> TrySelectFromTextureAsync(string cdbName, Texture texture,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand selectFromTextureCommand = dbConnection.CreateCommand();
        InitializeSelectFromTextureCommand(selectFromTextureCommand);
        await selectFromTextureCommand.PrepareAsync(cancellationToken);

        return await TrySelectFromTextureAsync(selectFromTextureCommand, cdbName, texture,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromTextureAsync(string, Texture, Func{Stream, CancellationToken, Task}, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<bool> TrySelectFromTextureAsync(DbCommand selectFromTextureCommand,
        string cdbName, Texture texture,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        selectFromTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureParameters(selectFromTextureCommand, texture);

        await using DbDataReader dbDataReader = await selectFromTextureCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        do
        {
            while (await dbDataReader.ReadAsync(cancellationToken))
            {
                await using Stream stream = dbDataReader.GetStream(ContentColumnName);
                await fileFoundAsyncAction(stream, cancellationToken);
                return true;
            }
        } while (await dbDataReader.NextResultAsync(cancellationToken));
        return false;
    }

    /// <inheritdoc/>
    public Stream? SelectFromTexture(string cdbName, Texture texture)
    {
        DbConnection dbConnection = dbDataSource.OpenConnection();
        try
        {
            DbCommand selectFromTextureCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromTextureCommand(selectFromTextureCommand);
                selectFromTextureCommand.Prepare();

                Stream? stream = SelectFromTexture(selectFromTextureCommand, cdbName, texture);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromTextureCommand, dbConnection);
                }
                else
                {
                    selectFromTextureCommand.Dispose();
                    dbConnection.Dispose();
                    return null;
                }
            }
            catch (Exception)
            {
                selectFromTextureCommand.Dispose();
                throw;
            }
        }
        catch (Exception)
        {
            dbConnection.Dispose();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromTexture(string, Texture)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Stream? SelectFromTexture(DbCommand selectFromTextureCommand,
        string cdbName, Texture texture)
    {
        selectFromTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureParameters(selectFromTextureCommand, texture);

        DbDataReader dbDataReader = selectFromTextureCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        try
        {
            do
            {
                while (dbDataReader.Read())
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (dbDataReader.NextResult());
            dbDataReader.Dispose();
            return null;
        }
        catch (Exception)
        {
            dbDataReader.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Stream?> SelectFromTextureAsync(string cdbName, Texture texture,
        CancellationToken cancellationToken)
    {
        DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            DbCommand selectFromTextureCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromTextureCommand(selectFromTextureCommand);
                await selectFromTextureCommand.PrepareAsync(cancellationToken);

                Stream? stream = await SelectFromTextureAsync(selectFromTextureCommand, cdbName, texture, cancellationToken);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromTextureCommand, dbConnection);
                }
                else
                {
                    await selectFromTextureCommand.DisposeAsync();
                    await dbConnection.DisposeAsync();
                    return null;
                }
            }
            catch (Exception)
            {
                await selectFromTextureCommand.DisposeAsync();
                throw;
            }
        }
        catch (Exception)
        {
            await dbConnection.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromTextureAsync(string, Texture, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<Stream?> SelectFromTextureAsync(DbCommand selectFromTextureCommand,
        string cdbName, Texture texture,
        CancellationToken cancellationToken)
    {
        selectFromTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureParameters(selectFromTextureCommand, texture);

        DbDataReader dbDataReader = await selectFromTextureCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        try
        {
            do
            {
                while (await dbDataReader.ReadAsync(cancellationToken))
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (await dbDataReader.NextResultAsync(cancellationToken));
            await dbDataReader.DisposeAsync();
            return null;
        }
        catch (Exception)
        {
            await dbDataReader.DisposeAsync();
            throw;
        }
    }

    #endregion

    #endregion

    #region Texture LOD

    /// <summary>
    /// The SQL DDL statement to create the Texture Level of Detail table.
    /// </summary>
    protected abstract string CreateTableTextureLodStatement
    {
        get;
    }

    #region Insert

    /// <summary>
    /// The SQL statement to insert a row into the Texture Level of Detail table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><description><see cref="TextureNameParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    ///   <item><description><see cref="ContentParamName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string InsertIntoTextureLodStatement
    {
        get;
    }

    internal void InitializeInsertIntoTextureLodCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = InsertIntoTextureLodStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachTextureLodParameters(dbCommand);
        CreateAndAttachParameter(dbCommand, ContentParamName, DbType.Binary);
    }

    private void CreateAndAttachTextureLodParameters(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, DatasetParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector1ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector2ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, LevelOfDetailParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, TextureNameParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, FileTypeParamName, DbType.String);
    }

    private void SetTextureLodParameters(DbCommand dbCommand, TextureLod textureLod)
    {
        dbCommand.Parameters[DatasetParamName].Value = textureLod.Dataset.Value;
        dbCommand.Parameters[ComponentSelector1ParamName].Value = textureLod.ComponentSelector1;
        dbCommand.Parameters[ComponentSelector2ParamName].Value = textureLod.ComponentSelector2;
        dbCommand.Parameters[LevelOfDetailParamName].Value = textureLod.LevelOfDetail.Value;
        dbCommand.Parameters[TextureNameParamName].Value = textureLod.Name;
        dbCommand.Parameters[FileTypeParamName].Value = textureLod.FileType;
    }

    /// <inheritdoc/>
    public int InsertIntoTextureLod(string cdbName, TextureLod textureLod, byte[] content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoTextureLodCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTextureLodCommand(insertIntoTextureLodCommand);
        insertIntoTextureLodCommand.Prepare();

        return InsertIntoTextureLod(insertIntoTextureLodCommand, cdbName, textureLod, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTextureLod(string, TextureLod, byte[])"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTextureLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoTextureLod(DbCommand insertIntoTextureLodCommand,
        string cdbName, TextureLod textureLod, byte[] content)
    {
        insertIntoTextureLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureLodParameters(insertIntoTextureLodCommand, textureLod);
        insertIntoTextureLodCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTextureLodCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public int InsertIntoTextureLod(string cdbName, TextureLod textureLod, Stream content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoTextureLodCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTextureLodCommand(insertIntoTextureLodCommand);
        insertIntoTextureLodCommand.Prepare();

        return InsertIntoTextureLod(insertIntoTextureLodCommand, cdbName, textureLod, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTextureLod(string, TextureLod, Stream)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTextureLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoTextureLod(DbCommand insertIntoTextureLodCommand,
        string cdbName, TextureLod textureLod, Stream content)
    {
        insertIntoTextureLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureLodParameters(insertIntoTextureLodCommand, textureLod);
        insertIntoTextureLodCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTextureLodCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoTextureLodAsync(string cdbName, TextureLod textureLod, byte[] content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoTextureLodCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTextureLodCommand(insertIntoTextureLodCommand);
        await insertIntoTextureLodCommand.PrepareAsync(cancellationToken);

        return await InsertIntoTextureLodAsync(insertIntoTextureLodCommand, cdbName, textureLod, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTextureLodAsync(string, TextureLod, byte[], CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTextureLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoTextureLodAsync(DbCommand insertIntoTextureLodCommand,
        string cdbName, TextureLod textureLod, byte[] content,
        CancellationToken cancellationToken)
    {
        insertIntoTextureLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureLodParameters(insertIntoTextureLodCommand, textureLod);
        insertIntoTextureLodCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTextureLodCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoTextureLodAsync(string cdbName, TextureLod textureLod, Stream content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoTextureLodCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTextureLodCommand(insertIntoTextureLodCommand);
        await insertIntoTextureLodCommand.PrepareAsync(cancellationToken);

        return await InsertIntoTextureLodAsync(insertIntoTextureLodCommand, cdbName, textureLod, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTextureLodAsync(string, TextureLod, Stream, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTextureLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoTextureLodAsync(DbCommand insertIntoTextureLodCommand,
        string cdbName, TextureLod textureLod, Stream content,
        CancellationToken cancellationToken)
    {
        insertIntoTextureLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureLodParameters(insertIntoTextureLodCommand, textureLod);
        insertIntoTextureLodCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTextureLodCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    #endregion

    #region Select

    /// <summary>
    /// The SQL statement to select a row from the Texture Level of Detail table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><description><see cref="TextureNameParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// <list type="bullet">
    ///   <listheader><description>Selected Columns</description></listheader>
    ///   <item><description><see cref="ContentColumnName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string SelectFromTextureLodStatement
    {
        get;
    }

    internal void InitializeSelectFromTextureLodCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = SelectFromTextureLodStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachTextureLodParameters(dbCommand);
    }

    /// <inheritdoc/>
    public bool TrySelectFromTextureLod(string cdbName, TextureLod textureLod,
        Action<Stream> fileFoundAction)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand selectFromTextureLodCommand = dbConnection.CreateCommand();
        InitializeSelectFromTextureLodCommand(selectFromTextureLodCommand);
        selectFromTextureLodCommand.Prepare();

        return TrySelectFromTextureLod(selectFromTextureLodCommand, cdbName, textureLod,
            fileFoundAction);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromTextureLod(string, TextureLod, Action{Stream})"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTextureLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual bool TrySelectFromTextureLod(DbCommand selectFromTextureLodCommand,
        string cdbName, TextureLod textureLod,
        Action<Stream> fileFoundAction)
    {
        selectFromTextureLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureLodParameters(selectFromTextureLodCommand, textureLod);

        using DbDataReader dbDataReader = selectFromTextureLodCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        do
        {
            while (dbDataReader.Read())
            {
                using Stream stream = dbDataReader.GetStream(ContentColumnName);
                fileFoundAction(stream);
                return true;
            }
        } while (dbDataReader.NextResult());
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> TrySelectFromTextureLodAsync(string cdbName, TextureLod textureLod,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand selectFromTextureLodCommand = dbConnection.CreateCommand();
        InitializeSelectFromTextureLodCommand(selectFromTextureLodCommand);
        await selectFromTextureLodCommand.PrepareAsync(cancellationToken);

        return await TrySelectFromTextureLodAsync(selectFromTextureLodCommand, cdbName, textureLod,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromTextureLodAsync(string, TextureLod, Func{Stream, CancellationToken, Task}, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTextureLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<bool> TrySelectFromTextureLodAsync(DbCommand selectFromTextureLodCommand,
        string cdbName, TextureLod textureLod,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        selectFromTextureLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureLodParameters(selectFromTextureLodCommand, textureLod);

        await using DbDataReader dbDataReader = await selectFromTextureLodCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        do
        {
            while (await dbDataReader.ReadAsync(cancellationToken))
            {
                await using Stream stream = dbDataReader.GetStream(ContentColumnName);
                await fileFoundAsyncAction(stream, cancellationToken);
                return true;
            }
        } while (await dbDataReader.NextResultAsync(cancellationToken));
        return false;
    }

    /// <inheritdoc/>
    public Stream? SelectFromTextureLod(string cdbName, TextureLod textureLod)
    {
        DbConnection dbConnection = dbDataSource.OpenConnection();
        try
        {
            DbCommand selectFromTextureLodCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromTextureLodCommand(selectFromTextureLodCommand);
                selectFromTextureLodCommand.Prepare();

                Stream? stream = SelectFromTextureLod(selectFromTextureLodCommand, cdbName, textureLod);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromTextureLodCommand, dbConnection);
                }
                else
                {
                    selectFromTextureLodCommand.Dispose();
                    dbConnection.Dispose();
                    return null;
                }
            }
            catch (Exception)
            {
                selectFromTextureLodCommand.Dispose();
                throw;
            }
        }
        catch (Exception)
        {
            dbConnection.Dispose();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromTextureLod(string, TextureLod)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTextureLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Stream? SelectFromTextureLod(DbCommand selectFromTextureLodCommand,
        string cdbName, TextureLod textureLod)
    {
        selectFromTextureLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureLodParameters(selectFromTextureLodCommand, textureLod);

        DbDataReader dbDataReader = selectFromTextureLodCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        try
        {
            do
            {
                while (dbDataReader.Read())
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (dbDataReader.NextResult());
            dbDataReader.Dispose();
            return null;
        }
        catch (Exception)
        {
            dbDataReader.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Stream?> SelectFromTextureLodAsync(string cdbName, TextureLod textureLod,
        CancellationToken cancellationToken)
    {
        DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            DbCommand selectFromTextureLodCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromTextureLodCommand(selectFromTextureLodCommand);
                await selectFromTextureLodCommand.PrepareAsync(cancellationToken);

                Stream? stream = await SelectFromTextureLodAsync(selectFromTextureLodCommand, cdbName, textureLod, cancellationToken);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromTextureLodCommand, dbConnection);
                }
                else
                {
                    await selectFromTextureLodCommand.DisposeAsync();
                    await dbConnection.DisposeAsync();
                    return null;
                }
            }
            catch (Exception)
            {
                await selectFromTextureLodCommand.DisposeAsync();
                throw;
            }
        }
        catch (Exception)
        {
            await dbConnection.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromTextureLodAsync(string, TextureLod, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTextureLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<Stream?> SelectFromTextureLodAsync(DbCommand selectFromTextureLodCommand,
        string cdbName, TextureLod textureLod,
        CancellationToken cancellationToken)
    {
        selectFromTextureLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetTextureLodParameters(selectFromTextureLodCommand, textureLod);

        DbDataReader dbDataReader = await selectFromTextureLodCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        try
        {
            do
            {
                while (await dbDataReader.ReadAsync(cancellationToken))
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (await dbDataReader.NextResultAsync(cancellationToken));
            await dbDataReader.DisposeAsync();
            return null;
        }
        catch (Exception)
        {
            await dbDataReader.DisposeAsync();
            throw;
        }
    }

    #endregion

    #endregion

    #region Geotypical Model

    /// <summary>
    /// The SQL DDL statement to create the Geotypical Model table.
    /// </summary>
    protected abstract string CreateTableGeotypicalModelStatement
    {
        get;
    }

    #region Insert

    /// <summary>
    /// The SQL statement to insert a row into the Geotypical Model table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="FeatureCategoryParamName"/></description></item>
    ///   <item><description><see cref="FeatureSubcategoryParamName"/></description></item>
    ///   <item><description><see cref="FeatureTypeParamName"/></description></item>
    ///   <item><description><see cref="FeatureSubcodeParamName"/></description></item>
    ///   <item><description><see cref="ModelNameParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    ///   <item><description><see cref="ContentParamName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string InsertIntoGeotypicalModelStatement
    {
        get;
    }

    internal void InitializeInsertIntoGeotypicalModelCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = InsertIntoGeotypicalModelStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachGeotypicalModelParameters(dbCommand);
        CreateAndAttachParameter(dbCommand, ContentParamName, DbType.Binary);
    }

    private void CreateAndAttachGeotypicalModelParameters(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, DatasetParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector1ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector2ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, FeatureCategoryParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, FeatureSubcategoryParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, FeatureTypeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, FeatureSubcodeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ModelNameParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, FileTypeParamName, DbType.String);
    }

    private void SetGeotypicalModelParameters(DbCommand dbCommand, GeotypicalModel geotypicalModel)
    {
        dbCommand.Parameters[DatasetParamName].Value = geotypicalModel.Dataset.Value;
        dbCommand.Parameters[ComponentSelector1ParamName].Value = geotypicalModel.ComponentSelector1;
        dbCommand.Parameters[ComponentSelector2ParamName].Value = geotypicalModel.ComponentSelector2;
        dbCommand.Parameters[FeatureCategoryParamName].Value = geotypicalModel.FeatureCode.Category;
        dbCommand.Parameters[FeatureSubcategoryParamName].Value = geotypicalModel.FeatureCode.Subcategory;
        dbCommand.Parameters[FeatureTypeParamName].Value = geotypicalModel.FeatureCode.Type;
        dbCommand.Parameters[FeatureSubcodeParamName].Value = geotypicalModel.FeatureSubcode;
        dbCommand.Parameters[ModelNameParamName].Value = geotypicalModel.Name;
        dbCommand.Parameters[FileTypeParamName].Value = geotypicalModel.FileType;
    }

    /// <inheritdoc/>
    public int InsertIntoGeotypicalModel(string cdbName, GeotypicalModel geotypicalModel, byte[] content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoGeotypicalModelCommand = dbConnection.CreateCommand();
        InitializeInsertIntoGeotypicalModelCommand(insertIntoGeotypicalModelCommand);
        insertIntoGeotypicalModelCommand.Prepare();

        return InsertIntoGeotypicalModel(insertIntoGeotypicalModelCommand, cdbName, geotypicalModel, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoGeotypicalModel(string, GeotypicalModel, byte[])"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoGeotypicalModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoGeotypicalModel(DbCommand insertIntoGeotypicalModelCommand,
        string cdbName, GeotypicalModel geotypicalModel, byte[] content)
    {
        insertIntoGeotypicalModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelParameters(insertIntoGeotypicalModelCommand, geotypicalModel);
        insertIntoGeotypicalModelCommand.Parameters[ContentParamName].Value = content;

        return insertIntoGeotypicalModelCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public int InsertIntoGeotypicalModel(string cdbName, GeotypicalModel geotypicalModel, Stream content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoGeotypicalModelCommand = dbConnection.CreateCommand();
        InitializeInsertIntoGeotypicalModelCommand(insertIntoGeotypicalModelCommand);
        insertIntoGeotypicalModelCommand.Prepare();

        return InsertIntoGeotypicalModel(insertIntoGeotypicalModelCommand, cdbName, geotypicalModel, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoGeotypicalModel(string, GeotypicalModel, Stream)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoGeotypicalModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoGeotypicalModel(DbCommand insertIntoGeotypicalModelCommand,
        string cdbName, GeotypicalModel geotypicalModel, Stream content)
    {
        insertIntoGeotypicalModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelParameters(insertIntoGeotypicalModelCommand, geotypicalModel);
        insertIntoGeotypicalModelCommand.Parameters[ContentParamName].Value = content;

        return insertIntoGeotypicalModelCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoGeotypicalModelAsync(string cdbName, GeotypicalModel geotypicalModel, byte[] content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoGeotypicalModelCommand = dbConnection.CreateCommand();
        InitializeInsertIntoGeotypicalModelCommand(insertIntoGeotypicalModelCommand);
        await insertIntoGeotypicalModelCommand.PrepareAsync(cancellationToken);

        return await InsertIntoGeotypicalModelAsync(insertIntoGeotypicalModelCommand, cdbName, geotypicalModel, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoGeotypicalModelAsync(string, GeotypicalModel, byte[], CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoGeotypicalModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoGeotypicalModelAsync(DbCommand insertIntoGeotypicalModelCommand,
        string cdbName, GeotypicalModel geotypicalModel, byte[] content,
        CancellationToken cancellationToken)
    {
        insertIntoGeotypicalModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelParameters(insertIntoGeotypicalModelCommand, geotypicalModel);
        insertIntoGeotypicalModelCommand.Parameters[ContentParamName].Value = content;

        return insertIntoGeotypicalModelCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoGeotypicalModelAsync(string cdbName, GeotypicalModel geotypicalModel, Stream content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoGeotypicalModelCommand = dbConnection.CreateCommand();
        InitializeInsertIntoGeotypicalModelCommand(insertIntoGeotypicalModelCommand);
        await insertIntoGeotypicalModelCommand.PrepareAsync(cancellationToken);

        return await InsertIntoGeotypicalModelAsync(insertIntoGeotypicalModelCommand, cdbName, geotypicalModel, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoGeotypicalModelAsync(string, GeotypicalModel, Stream, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoGeotypicalModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoGeotypicalModelAsync(DbCommand insertIntoGeotypicalModelCommand,
        string cdbName, GeotypicalModel geotypicalModel, Stream content,
        CancellationToken cancellationToken)
    {
        insertIntoGeotypicalModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelParameters(insertIntoGeotypicalModelCommand, geotypicalModel);
        insertIntoGeotypicalModelCommand.Parameters[ContentParamName].Value = content;

        return insertIntoGeotypicalModelCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    #endregion

    #region Select

    /// <summary>
    /// The SQL statement to select a row from the Geotypical Model table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="FeatureCategoryParamName"/></description></item>
    ///   <item><description><see cref="FeatureSubcategoryParamName"/></description></item>
    ///   <item><description><see cref="FeatureTypeParamName"/></description></item>
    ///   <item><description><see cref="FeatureSubcodeParamName"/></description></item>
    ///   <item><description><see cref="ModelNameParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// <list type="bullet">
    ///   <listheader><description>Selected Columns</description></listheader>
    ///   <item><description><see cref="ContentColumnName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string SelectFromGeotypicalModelStatement
    {
        get;
    }

    internal void InitializeSelectFromGeotypicalModelCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = SelectFromGeotypicalModelStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachGeotypicalModelParameters(dbCommand);
    }

    /// <inheritdoc/>
    public bool TrySelectFromGeotypicalModel(string cdbName, GeotypicalModel geotypicalModel,
        Action<Stream> fileFoundAction)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand selectFromGeotypicalModelCommand = dbConnection.CreateCommand();
        InitializeSelectFromGeotypicalModelCommand(selectFromGeotypicalModelCommand);
        selectFromGeotypicalModelCommand.Prepare();

        return TrySelectFromGeotypicalModel(selectFromGeotypicalModelCommand, cdbName, geotypicalModel,
            fileFoundAction);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromGeotypicalModel(string, GeotypicalModel, Action{Stream})"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromGeotypicalModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual bool TrySelectFromGeotypicalModel(DbCommand selectFromGeotypicalModelCommand,
        string cdbName, GeotypicalModel geotypicalModel,
        Action<Stream> fileFoundAction)
    {
        selectFromGeotypicalModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelParameters(selectFromGeotypicalModelCommand, geotypicalModel);

        using DbDataReader dbDataReader = selectFromGeotypicalModelCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        do
        {
            while (dbDataReader.Read())
            {
                using Stream stream = dbDataReader.GetStream(ContentColumnName);
                fileFoundAction(stream);
                return true;
            }
        } while (dbDataReader.NextResult());
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> TrySelectFromGeotypicalModelAsync(string cdbName, GeotypicalModel geotypicalModel,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand selectFromGeotypicalModelCommand = dbConnection.CreateCommand();
        InitializeSelectFromGeotypicalModelCommand(selectFromGeotypicalModelCommand);
        await selectFromGeotypicalModelCommand.PrepareAsync(cancellationToken);

        return await TrySelectFromGeotypicalModelAsync(selectFromGeotypicalModelCommand, cdbName, geotypicalModel,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromGeotypicalModelAsync(string, GeotypicalModel, Func{Stream, CancellationToken, Task}, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromGeotypicalModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<bool> TrySelectFromGeotypicalModelAsync(DbCommand selectFromGeotypicalModelCommand,
        string cdbName, GeotypicalModel geotypicalModel,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        selectFromGeotypicalModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelParameters(selectFromGeotypicalModelCommand, geotypicalModel);

        await using DbDataReader dbDataReader = await selectFromGeotypicalModelCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        do
        {
            while (await dbDataReader.ReadAsync(cancellationToken))
            {
                await using Stream stream = dbDataReader.GetStream(ContentColumnName);
                await fileFoundAsyncAction(stream, cancellationToken);
                return true;
            }
        } while (await dbDataReader.NextResultAsync(cancellationToken));
        return false;
    }

    /// <inheritdoc/>
    public Stream? SelectFromGeotypicalModel(string cdbName, GeotypicalModel geotypicalModel)
    {
        DbConnection dbConnection = dbDataSource.OpenConnection();
        try
        {
            DbCommand selectFromGeotypicalModelCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromGeotypicalModelCommand(selectFromGeotypicalModelCommand);
                selectFromGeotypicalModelCommand.Prepare();

                Stream? stream = SelectFromGeotypicalModel(selectFromGeotypicalModelCommand, cdbName, geotypicalModel);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromGeotypicalModelCommand, dbConnection);
                }
                else
                {
                    selectFromGeotypicalModelCommand.Dispose();
                    dbConnection.Dispose();
                    return null;
                }
            }
            catch (Exception)
            {
                selectFromGeotypicalModelCommand.Dispose();
                throw;
            }
        }
        catch (Exception)
        {
            dbConnection.Dispose();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromGeotypicalModel(string, GeotypicalModel)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromGeotypicalModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Stream? SelectFromGeotypicalModel(DbCommand selectFromGeotypicalModelCommand,
        string cdbName, GeotypicalModel geotypicalModel)
    {
        selectFromGeotypicalModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelParameters(selectFromGeotypicalModelCommand, geotypicalModel);

        DbDataReader dbDataReader = selectFromGeotypicalModelCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        try
        {
            do
            {
                while (dbDataReader.Read())
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (dbDataReader.NextResult());
            dbDataReader.Dispose();
            return null;
        }
        catch (Exception)
        {
            dbDataReader.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Stream?> SelectFromGeotypicalModelAsync(string cdbName, GeotypicalModel geotypicalModel,
        CancellationToken cancellationToken)
    {
        DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            DbCommand selectFromGeotypicalModelCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromGeotypicalModelCommand(selectFromGeotypicalModelCommand);
                await selectFromGeotypicalModelCommand.PrepareAsync(cancellationToken);

                Stream? stream = await SelectFromGeotypicalModelAsync(selectFromGeotypicalModelCommand, cdbName, geotypicalModel, cancellationToken);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromGeotypicalModelCommand, dbConnection);
                }
                else
                {
                    await selectFromGeotypicalModelCommand.DisposeAsync();
                    await dbConnection.DisposeAsync();
                    return null;
                }
            }
            catch (Exception)
            {
                await selectFromGeotypicalModelCommand.DisposeAsync();
                throw;
            }
        }
        catch (Exception)
        {
            await dbConnection.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromGeotypicalModelAsync(string, GeotypicalModel, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromGeotypicalModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<Stream?> SelectFromGeotypicalModelAsync(DbCommand selectFromGeotypicalModelCommand,
        string cdbName, GeotypicalModel geotypicalModel,
        CancellationToken cancellationToken)
    {
        selectFromGeotypicalModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelParameters(selectFromGeotypicalModelCommand, geotypicalModel);

        DbDataReader dbDataReader = await selectFromGeotypicalModelCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        try
        {
            do
            {
                while (await dbDataReader.ReadAsync(cancellationToken))
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (await dbDataReader.NextResultAsync(cancellationToken));
            await dbDataReader.DisposeAsync();
            return null;
        }
        catch (Exception)
        {
            await dbDataReader.DisposeAsync();
            throw;
        }
    }

    #endregion

    #endregion

    #region Geotypical Model LOD

    /// <summary>
    /// The SQL DDL statement to create the Geotypical Model Level of Detail table.
    /// </summary>
    protected abstract string CreateTableGeotypicalModelLodStatement
    {
        get;
    }

    #region Insert

    /// <summary>
    /// The SQL statement to insert a row into the Geotypical Model Level of Detail table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><description><see cref="FeatureCategoryParamName"/></description></item>
    ///   <item><description><see cref="FeatureSubcategoryParamName"/></description></item>
    ///   <item><description><see cref="FeatureTypeParamName"/></description></item>
    ///   <item><description><see cref="FeatureSubcodeParamName"/></description></item>
    ///   <item><description><see cref="ModelNameParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    ///   <item><description><see cref="ContentParamName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string InsertIntoGeotypicalModelLodStatement
    {
        get;
    }

    internal void InitializeInsertIntoGeotypicalModelLodCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = InsertIntoGeotypicalModelLodStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachGeotypicalModelLodParameters(dbCommand);
        CreateAndAttachParameter(dbCommand, ContentParamName, DbType.Binary);
    }

    private void CreateAndAttachGeotypicalModelLodParameters(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, DatasetParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector1ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector2ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, LevelOfDetailParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, FeatureCategoryParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, FeatureSubcategoryParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, FeatureTypeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, FeatureSubcodeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ModelNameParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, FileTypeParamName, DbType.String);
    }

    private void SetGeotypicalModelLodParameters(DbCommand dbCommand, GeotypicalModelLod geotypicalModelLod)
    {
        dbCommand.Parameters[DatasetParamName].Value = geotypicalModelLod.Dataset.Value;
        dbCommand.Parameters[ComponentSelector1ParamName].Value = geotypicalModelLod.ComponentSelector1;
        dbCommand.Parameters[ComponentSelector2ParamName].Value = geotypicalModelLod.ComponentSelector2;
        dbCommand.Parameters[LevelOfDetailParamName].Value = geotypicalModelLod.LevelOfDetail.Value;
        dbCommand.Parameters[FeatureCategoryParamName].Value = geotypicalModelLod.FeatureCode.Category;
        dbCommand.Parameters[FeatureSubcategoryParamName].Value = geotypicalModelLod.FeatureCode.Subcategory;
        dbCommand.Parameters[FeatureTypeParamName].Value = geotypicalModelLod.FeatureCode.Type;
        dbCommand.Parameters[FeatureSubcodeParamName].Value = geotypicalModelLod.FeatureSubcode;
        dbCommand.Parameters[ModelNameParamName].Value = geotypicalModelLod.Name;
        dbCommand.Parameters[FileTypeParamName].Value = geotypicalModelLod.FileType;
    }

    /// <inheritdoc/>
    public int InsertIntoGeotypicalModelLod(string cdbName, GeotypicalModelLod geotypicalModelLod, byte[] content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoGeotypicalModelLodCommand = dbConnection.CreateCommand();
        InitializeInsertIntoGeotypicalModelLodCommand(insertIntoGeotypicalModelLodCommand);
        insertIntoGeotypicalModelLodCommand.Prepare();

        return InsertIntoGeotypicalModelLod(insertIntoGeotypicalModelLodCommand, cdbName, geotypicalModelLod, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoGeotypicalModelLod(string, GeotypicalModelLod, byte[])"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoGeotypicalModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoGeotypicalModelLod(DbCommand insertIntoGeotypicalModelLodCommand,
        string cdbName, GeotypicalModelLod geotypicalModelLod, byte[] content)
    {
        insertIntoGeotypicalModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelLodParameters(insertIntoGeotypicalModelLodCommand, geotypicalModelLod);
        insertIntoGeotypicalModelLodCommand.Parameters[ContentParamName].Value = content;

        return insertIntoGeotypicalModelLodCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public int InsertIntoGeotypicalModelLod(string cdbName, GeotypicalModelLod geotypicalModelLod, Stream content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoGeotypicalModelLodCommand = dbConnection.CreateCommand();
        InitializeInsertIntoGeotypicalModelLodCommand(insertIntoGeotypicalModelLodCommand);
        insertIntoGeotypicalModelLodCommand.Prepare();

        return InsertIntoGeotypicalModelLod(insertIntoGeotypicalModelLodCommand, cdbName, geotypicalModelLod, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoGeotypicalModelLod(string, GeotypicalModelLod, Stream)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoGeotypicalModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoGeotypicalModelLod(DbCommand insertIntoGeotypicalModelLodCommand,
        string cdbName, GeotypicalModelLod geotypicalModelLod, Stream content)
    {
        insertIntoGeotypicalModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelLodParameters(insertIntoGeotypicalModelLodCommand, geotypicalModelLod);
        insertIntoGeotypicalModelLodCommand.Parameters[ContentParamName].Value = content;

        return insertIntoGeotypicalModelLodCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoGeotypicalModelLodAsync(string cdbName, GeotypicalModelLod geotypicalModelLod, byte[] content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoGeotypicalModelLodCommand = dbConnection.CreateCommand();
        InitializeInsertIntoGeotypicalModelLodCommand(insertIntoGeotypicalModelLodCommand);
        await insertIntoGeotypicalModelLodCommand.PrepareAsync(cancellationToken);

        return await InsertIntoGeotypicalModelLodAsync(insertIntoGeotypicalModelLodCommand, cdbName, geotypicalModelLod, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoGeotypicalModelLodAsync(string, GeotypicalModelLod, byte[], CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoGeotypicalModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoGeotypicalModelLodAsync(DbCommand insertIntoGeotypicalModelLodCommand,
        string cdbName, GeotypicalModelLod geotypicalModelLod, byte[] content,
        CancellationToken cancellationToken)
    {
        insertIntoGeotypicalModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelLodParameters(insertIntoGeotypicalModelLodCommand, geotypicalModelLod);
        insertIntoGeotypicalModelLodCommand.Parameters[ContentParamName].Value = content;

        return insertIntoGeotypicalModelLodCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoGeotypicalModelLodAsync(string cdbName, GeotypicalModelLod geotypicalModelLod, Stream content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoGeotypicalModelLodCommand = dbConnection.CreateCommand();
        InitializeInsertIntoGeotypicalModelLodCommand(insertIntoGeotypicalModelLodCommand);
        await insertIntoGeotypicalModelLodCommand.PrepareAsync(cancellationToken);

        return await InsertIntoGeotypicalModelLodAsync(insertIntoGeotypicalModelLodCommand, cdbName, geotypicalModelLod, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoGeotypicalModelLodAsync(string, GeotypicalModelLod, Stream, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoGeotypicalModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoGeotypicalModelLodAsync(DbCommand insertIntoGeotypicalModelLodCommand,
        string cdbName, GeotypicalModelLod geotypicalModelLod, Stream content,
        CancellationToken cancellationToken)
    {
        insertIntoGeotypicalModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelLodParameters(insertIntoGeotypicalModelLodCommand, geotypicalModelLod);
        insertIntoGeotypicalModelLodCommand.Parameters[ContentParamName].Value = content;

        return insertIntoGeotypicalModelLodCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    #endregion

    #region Select

    /// <summary>
    /// The SQL statement to select a row from the Geotypical Model Level of Detail table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><description><see cref="FeatureCategoryParamName"/></description></item>
    ///   <item><description><see cref="FeatureSubcategoryParamName"/></description></item>
    ///   <item><description><see cref="FeatureTypeParamName"/></description></item>
    ///   <item><description><see cref="FeatureSubcodeParamName"/></description></item>
    ///   <item><description><see cref="ModelNameParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// <list type="bullet">
    ///   <listheader><description>Selected Columns</description></listheader>
    ///   <item><description><see cref="ContentColumnName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string SelectFromGeotypicalModelLodStatement
    {
        get;
    }

    internal void InitializeSelectFromGeotypicalModelLodCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = SelectFromGeotypicalModelLodStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachGeotypicalModelLodParameters(dbCommand);
    }

    /// <inheritdoc/>
    public bool TrySelectFromGeotypicalModelLod(string cdbName, GeotypicalModelLod geotypicalModelLod,
        Action<Stream> fileFoundAction)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand selectFromGeotypicalModelLodCommand = dbConnection.CreateCommand();
        InitializeSelectFromGeotypicalModelLodCommand(selectFromGeotypicalModelLodCommand);
        selectFromGeotypicalModelLodCommand.Prepare();

        return TrySelectFromGeotypicalModelLod(selectFromGeotypicalModelLodCommand, cdbName, geotypicalModelLod, fileFoundAction);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromGeotypicalModelLod(string, GeotypicalModelLod, Action{Stream})"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromGeotypicalModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual bool TrySelectFromGeotypicalModelLod(DbCommand selectFromGeotypicalModelLodCommand,
        string cdbName, GeotypicalModelLod geotypicalModelLod,
        Action<Stream> fileFoundAction)
    {
        selectFromGeotypicalModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelLodParameters(selectFromGeotypicalModelLodCommand, geotypicalModelLod);

        using DbDataReader dbDataReader = selectFromGeotypicalModelLodCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        do
        {
            while (dbDataReader.Read())
            {
                using Stream stream = dbDataReader.GetStream(ContentColumnName);
                fileFoundAction(stream);
                return true;
            }
        } while (dbDataReader.NextResult());
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> TrySelectFromGeotypicalModelLodAsync(string cdbName, GeotypicalModelLod geotypicalModelLod,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand selectFromGeotypicalModelLodCommand = dbConnection.CreateCommand();
        InitializeSelectFromGeotypicalModelLodCommand(selectFromGeotypicalModelLodCommand);
        await selectFromGeotypicalModelLodCommand.PrepareAsync(cancellationToken);

        return await TrySelectFromGeotypicalModelLodAsync(selectFromGeotypicalModelLodCommand, cdbName, geotypicalModelLod,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromGeotypicalModelLodAsync(string, GeotypicalModelLod, Func{Stream, CancellationToken, Task}, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromGeotypicalModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<bool> TrySelectFromGeotypicalModelLodAsync(DbCommand selectFromGeotypicalModelLodCommand,
        string cdbName, GeotypicalModelLod geotypicalModelLod,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        selectFromGeotypicalModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelLodParameters(selectFromGeotypicalModelLodCommand, geotypicalModelLod);

        await using DbDataReader dbDataReader = await selectFromGeotypicalModelLodCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        do
        {
            while (await dbDataReader.ReadAsync(cancellationToken))
            {
                await using Stream stream = dbDataReader.GetStream(ContentColumnName);
                await fileFoundAsyncAction(stream, cancellationToken);
                return true;
            }
        } while (await dbDataReader.NextResultAsync(cancellationToken));
        return false;
    }

    /// <inheritdoc/>
    public Stream? SelectFromGeotypicalModelLod(string cdbName, GeotypicalModelLod geotypicalModelLod)
    {
        DbConnection dbConnection = dbDataSource.OpenConnection();
        try
        {
            DbCommand selectFromGeotypicalModelLodCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromGeotypicalModelLodCommand(selectFromGeotypicalModelLodCommand);
                selectFromGeotypicalModelLodCommand.Prepare();

                Stream? stream = SelectFromGeotypicalModelLod(selectFromGeotypicalModelLodCommand, cdbName, geotypicalModelLod);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromGeotypicalModelLodCommand, dbConnection);
                }
                else
                {
                    selectFromGeotypicalModelLodCommand.Dispose();
                    dbConnection.Dispose();
                    return null;
                }
            }
            catch (Exception)
            {
                selectFromGeotypicalModelLodCommand.Dispose();
                throw;
            }
        }
        catch (Exception)
        {
            dbConnection.Dispose();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromGeotypicalModelLod(string, GeotypicalModelLod)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromGeotypicalModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Stream? SelectFromGeotypicalModelLod(DbCommand selectFromGeotypicalModelLodCommand,
        string cdbName, GeotypicalModelLod geotypicalModelLod)
    {
        selectFromGeotypicalModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelLodParameters(selectFromGeotypicalModelLodCommand, geotypicalModelLod);

        DbDataReader dbDataReader = selectFromGeotypicalModelLodCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        try
        {
            do
            {
                while (dbDataReader.Read())
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (dbDataReader.NextResult());
            dbDataReader.Dispose();
            return null;
        }
        catch (Exception)
        {
            dbDataReader.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Stream?> SelectFromGeotypicalModelLodAsync(string cdbName, GeotypicalModelLod geotypicalModelLod,
        CancellationToken cancellationToken)
    {
        DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            DbCommand selectFromGeotypicalModelLodCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromGeotypicalModelLodCommand(selectFromGeotypicalModelLodCommand);
                await selectFromGeotypicalModelLodCommand.PrepareAsync(cancellationToken);

                Stream? stream = await SelectFromGeotypicalModelLodAsync(selectFromGeotypicalModelLodCommand, cdbName, geotypicalModelLod, cancellationToken);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromGeotypicalModelLodCommand, dbConnection);
                }
                else
                {
                    await selectFromGeotypicalModelLodCommand.DisposeAsync();
                    await dbConnection.DisposeAsync();
                    return null;
                }
            }
            catch (Exception)
            {
                await selectFromGeotypicalModelLodCommand.DisposeAsync();
                throw;
            }
        }
        catch (Exception)
        {
            await dbConnection.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromGeotypicalModelLodAsync(string, GeotypicalModelLod, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromGeotypicalModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<Stream?> SelectFromGeotypicalModelLodAsync(DbCommand selectFromGeotypicalModelLodCommand,
        string cdbName, GeotypicalModelLod geotypicalModelLod,
        CancellationToken cancellationToken)
    {
        selectFromGeotypicalModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetGeotypicalModelLodParameters(selectFromGeotypicalModelLodCommand, geotypicalModelLod);

        DbDataReader dbDataReader = await selectFromGeotypicalModelLodCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        try
        {
            do
            {
                while (await dbDataReader.ReadAsync(cancellationToken))
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (await dbDataReader.NextResultAsync(cancellationToken));
            await dbDataReader.DisposeAsync();
            return null;
        }
        catch (Exception)
        {
            await dbDataReader.DisposeAsync();
            throw;
        }
    }

    #endregion

    #endregion

    #region Moving Model

    /// <summary>
    /// The SQL DDL statement to create the Moving Model table.
    /// </summary>
    protected abstract string CreateTableMovingModelStatement
    {
        get;
    }

    #region Insert

    /// <summary>
    /// The SQL statement to insert a row into the Moving Model table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="DISKindParamName"/></description></item>
    ///   <item><description><see cref="DISDomainParamName"/></description></item>
    ///   <item><description><see cref="DISCountryParamName"/></description></item>
    ///   <item><description><see cref="DISCategoryParamName"/></description></item>
    ///   <item><description><see cref="DISSubcategoryParamName"/></description></item>
    ///   <item><description><see cref="DISSpecificParamName"/></description></item>
    ///   <item><description><see cref="DISExtraParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    ///   <item><description><see cref="ContentParamName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string InsertIntoMovingModelStatement
    {
        get;
    }

    internal void InitializeInsertIntoMovingModelCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = InsertIntoMovingModelStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachMovingModelParameters(dbCommand);
        CreateAndAttachParameter(dbCommand, ContentParamName, DbType.String);
    }

    private void CreateAndAttachMovingModelParameters(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, DatasetParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector1ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector2ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DISKindParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DISDomainParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DISCountryParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DISCategoryParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DISSubcategoryParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DISSpecificParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DISExtraParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, FileTypeParamName, DbType.String);
    }

    private void SetMovingModelParameters(DbCommand dbCommand, MovingModel movingModel)
    {
        dbCommand.Parameters[DatasetParamName].Value = movingModel.Dataset.Value;
        dbCommand.Parameters[ComponentSelector1ParamName].Value = movingModel.ComponentSelector1;
        dbCommand.Parameters[ComponentSelector2ParamName].Value = movingModel.ComponentSelector2;
        dbCommand.Parameters[DISKindParamName].Value = movingModel.MMDC.Kind;
        dbCommand.Parameters[DISDomainParamName].Value = movingModel.MMDC.Domain;
        dbCommand.Parameters[DISCountryParamName].Value = movingModel.MMDC.Country;
        dbCommand.Parameters[DISCategoryParamName].Value = movingModel.MMDC.Category;
        dbCommand.Parameters[DISSubcategoryParamName].Value = movingModel.MMDC.Subcategory;
        dbCommand.Parameters[DISSpecificParamName].Value = movingModel.MMDC.Specific;
        dbCommand.Parameters[DISExtraParamName].Value = movingModel.MMDC.Extra;
        dbCommand.Parameters[FileTypeParamName].Value = movingModel.FileType;
    }

    /// <inheritdoc/>
    public int InsertIntoMovingModel(string cdbName, MovingModel movingModel, byte[] content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoMovingModelCommand = dbConnection.CreateCommand();
        InitializeInsertIntoMovingModelCommand(insertIntoMovingModelCommand);
        insertIntoMovingModelCommand.Prepare();

        return InsertIntoMovingModel(insertIntoMovingModelCommand, cdbName, movingModel, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoMovingModel(string, MovingModel, byte[])"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoMovingModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoMovingModel(DbCommand insertIntoMovingModelCommand,
        string cdbName, MovingModel movingModel, byte[] content)
    {
        insertIntoMovingModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelParameters(insertIntoMovingModelCommand, movingModel);
        insertIntoMovingModelCommand.Parameters[ContentParamName].Value = content;

        return insertIntoMovingModelCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public int InsertIntoMovingModel(string cdbName, MovingModel movingModel, Stream content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoMovingModelCommand = dbConnection.CreateCommand();
        InitializeInsertIntoMovingModelCommand(insertIntoMovingModelCommand);
        insertIntoMovingModelCommand.Prepare();

        return InsertIntoMovingModel(insertIntoMovingModelCommand, cdbName, movingModel, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoMovingModel(string, MovingModel, Stream)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoMovingModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoMovingModel(DbCommand insertIntoMovingModelCommand,
        string cdbName, MovingModel movingModel, Stream content)
    {
        insertIntoMovingModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelParameters(insertIntoMovingModelCommand, movingModel);
        insertIntoMovingModelCommand.Parameters[ContentParamName].Value = content;

        return insertIntoMovingModelCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoMovingModelAsync(string cdbName, MovingModel movingModel, byte[] content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoMovingModelCommand = dbConnection.CreateCommand();
        InitializeInsertIntoMovingModelCommand(insertIntoMovingModelCommand);
        await insertIntoMovingModelCommand.PrepareAsync(cancellationToken);

        return await InsertIntoMovingModelAsync(insertIntoMovingModelCommand, cdbName, movingModel, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoMovingModelAsync(string, MovingModel, byte[], CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoMovingModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoMovingModelAsync(DbCommand insertIntoMovingModelCommand,
        string cdbName, MovingModel movingModel, byte[] content,
        CancellationToken cancellationToken)
    {
        insertIntoMovingModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelParameters(insertIntoMovingModelCommand, movingModel);
        insertIntoMovingModelCommand.Parameters[ContentParamName].Value = content;

        return insertIntoMovingModelCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoMovingModelAsync(string cdbName, MovingModel movingModel, Stream content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoMovingModelCommand = dbConnection.CreateCommand();
        InitializeInsertIntoMovingModelCommand(insertIntoMovingModelCommand);
        await insertIntoMovingModelCommand.PrepareAsync(cancellationToken);

        return await InsertIntoMovingModelAsync(insertIntoMovingModelCommand, cdbName, movingModel, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoMovingModelAsync(string, MovingModel, Stream, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoMovingModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoMovingModelAsync(DbCommand insertIntoMovingModelCommand,
        string cdbName, MovingModel movingModel, Stream content,
        CancellationToken cancellationToken)
    {
        insertIntoMovingModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelParameters(insertIntoMovingModelCommand, movingModel);
        insertIntoMovingModelCommand.Parameters[ContentParamName].Value = content;

        return insertIntoMovingModelCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    #endregion

    #region Select

    /// <summary>
    /// The SQL statement to select a row from the Moving Model table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="DISKindParamName"/></description></item>
    ///   <item><description><see cref="DISDomainParamName"/></description></item>
    ///   <item><description><see cref="DISCountryParamName"/></description></item>
    ///   <item><description><see cref="DISCategoryParamName"/></description></item>
    ///   <item><description><see cref="DISSubcategoryParamName"/></description></item>
    ///   <item><description><see cref="DISSpecificParamName"/></description></item>
    ///   <item><description><see cref="DISExtraParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// <list type="bullet">
    ///   <listheader><description>Selected Columns</description></listheader>
    ///   <item><description><see cref="ContentColumnName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string SelectFromMovingModelStatement
    {
        get;
    }

    internal void InitializeSelectFromMovingModelCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = SelectFromMovingModelStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachMovingModelParameters(dbCommand);
    }

    /// <inheritdoc/>
    public bool TrySelectFromMovingModel(string cdbName, MovingModel movingModel,
        Action<Stream> fileFoundAction)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand selectFromMovingModelCommand = dbConnection.CreateCommand();
        InitializeSelectFromMovingModelCommand(selectFromMovingModelCommand);
        selectFromMovingModelCommand.Prepare();

        return TrySelectFromMovingModel(selectFromMovingModelCommand, cdbName, movingModel, fileFoundAction);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromMovingModel(string, MovingModel, Action{Stream})"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromMovingModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual bool TrySelectFromMovingModel(DbCommand selectFromMovingModelCommand,
        string cdbName, MovingModel movingModel,
        Action<Stream> fileFoundAction)
    {
        selectFromMovingModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelParameters(selectFromMovingModelCommand, movingModel);

        using DbDataReader dbDataReader = selectFromMovingModelCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        do
        {
            while (dbDataReader.Read())
            {
                using Stream stream = dbDataReader.GetStream(ContentColumnName);
                fileFoundAction(stream);
                return true;
            }
        } while (dbDataReader.NextResult());
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> TrySelectFromMovingModelAsync(string cdbName, MovingModel movingModel,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand selectFromMovingModelCommand = dbConnection.CreateCommand();
        InitializeSelectFromMovingModelCommand(selectFromMovingModelCommand);
        await selectFromMovingModelCommand.PrepareAsync(cancellationToken);

        return await TrySelectFromMovingModelAsync(selectFromMovingModelCommand, cdbName, movingModel,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromMovingModelAsync(string, MovingModel, Func{Stream, CancellationToken, Task}, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromMovingModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<bool> TrySelectFromMovingModelAsync(DbCommand selectFromMovingModelCommand,
        string cdbName, MovingModel movingModel,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        selectFromMovingModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelParameters(selectFromMovingModelCommand, movingModel);

        await using DbDataReader dbDataReader = await selectFromMovingModelCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        do
        {
            while (await dbDataReader.ReadAsync(cancellationToken))
            {
                await using Stream stream = dbDataReader.GetStream(ContentColumnName);
                await fileFoundAsyncAction(stream, cancellationToken);
                return true;
            }
        } while (await dbDataReader.NextResultAsync(cancellationToken));
        return false;
    }

    /// <inheritdoc/>
    public Stream? SelectFromMovingModel(string cdbName, MovingModel movingModel)
    {
        DbConnection dbConnection = dbDataSource.OpenConnection();
        try
        {
            DbCommand selectFromMovingModelCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromMovingModelCommand(selectFromMovingModelCommand);
                selectFromMovingModelCommand.Prepare();

                Stream? stream = SelectFromMovingModel(selectFromMovingModelCommand, cdbName, movingModel);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromMovingModelCommand, dbConnection);
                }
                else
                {
                    selectFromMovingModelCommand.Dispose();
                    dbConnection.Dispose();
                    return null;
                }
            }
            catch (Exception)
            {
                selectFromMovingModelCommand.Dispose();
                throw;
            }
        }
        catch (Exception)
        {
            dbConnection.Dispose();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromMovingModel(string, MovingModel)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromMovingModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Stream? SelectFromMovingModel(DbCommand selectFromMovingModelCommand,
        string cdbName, MovingModel movingModel)
    {
        selectFromMovingModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelParameters(selectFromMovingModelCommand, movingModel);

        DbDataReader dbDataReader = selectFromMovingModelCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        try
        {
            do
            {
                while (dbDataReader.Read())
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (dbDataReader.NextResult());
            dbDataReader.Dispose();
            return null;
        }
        catch (Exception)
        {
            dbDataReader.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Stream?> SelectFromMovingModelAsync(string cdbName, MovingModel movingModel,
        CancellationToken cancellationToken)
    {
        DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            DbCommand selectFromMovingModelCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromMovingModelCommand(selectFromMovingModelCommand);
                await selectFromMovingModelCommand.PrepareAsync(cancellationToken);

                Stream? stream = await SelectFromMovingModelAsync(selectFromMovingModelCommand, cdbName, movingModel, cancellationToken);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromMovingModelCommand, dbConnection);
                }
                else
                {
                    await selectFromMovingModelCommand.DisposeAsync();
                    await dbConnection.DisposeAsync();
                    return null;
                }
            }
            catch (Exception)
            {
                await selectFromMovingModelCommand.DisposeAsync();
                throw;
            }
        }
        catch (Exception)
        {
            await dbConnection.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromMovingModelAsync(string, MovingModel, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromMovingModelCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<Stream?> SelectFromMovingModelAsync(DbCommand selectFromMovingModelCommand,
        string cdbName, MovingModel movingModel,
        CancellationToken cancellationToken)
    {
        selectFromMovingModelCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelParameters(selectFromMovingModelCommand, movingModel);

        DbDataReader dbDataReader = await selectFromMovingModelCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        try
        {
            do
            {
                while (await dbDataReader.ReadAsync(cancellationToken))
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (await dbDataReader.NextResultAsync(cancellationToken));
            await dbDataReader.DisposeAsync();
            return null;
        }
        catch (Exception)
        {
            await dbDataReader.DisposeAsync();
            throw;
        }
    }

    #endregion

    #endregion

    #region Moving Model LOD

    /// <summary>
    /// The SQL DDL statement to create the Moving Model Level of Detail table.
    /// </summary>
    protected abstract string CreateTableMovingModelLodStatement
    {
        get;
    }

    #region Insert

    /// <summary>
    /// The SQL statement to insert a row into the Moving Model Level of Detail table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><description><see cref="DISKindParamName"/></description></item>
    ///   <item><description><see cref="DISDomainParamName"/></description></item>
    ///   <item><description><see cref="DISCountryParamName"/></description></item>
    ///   <item><description><see cref="DISCategoryParamName"/></description></item>
    ///   <item><description><see cref="DISSubcategoryParamName"/></description></item>
    ///   <item><description><see cref="DISSpecificParamName"/></description></item>
    ///   <item><description><see cref="DISExtraParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    ///   <item><description><see cref="ContentParamName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string InsertIntoMovingModelLodStatement
    {
        get;
    }

    internal void InitializeInsertIntoMovingModelLodCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = InsertIntoMovingModelLodStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachMovingModelLodParameters(dbCommand);
        CreateAndAttachParameter(dbCommand, ContentParamName, DbType.String);
    }

    private void CreateAndAttachMovingModelLodParameters(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, DatasetParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector1ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector2ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, LevelOfDetailParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DISKindParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DISDomainParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DISCountryParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DISCategoryParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DISSubcategoryParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DISSpecificParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DISExtraParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, FileTypeParamName, DbType.String);
    }

    private void SetMovingModelLodParameters(DbCommand dbCommand, MovingModelLod movingModelLod)
    {
        dbCommand.Parameters[DatasetParamName].Value = movingModelLod.Dataset.Value;
        dbCommand.Parameters[ComponentSelector1ParamName].Value = movingModelLod.ComponentSelector1;
        dbCommand.Parameters[ComponentSelector2ParamName].Value = movingModelLod.ComponentSelector2;
        dbCommand.Parameters[LevelOfDetailParamName].Value = movingModelLod.LevelOfDetail.Value;
        dbCommand.Parameters[DISKindParamName].Value = movingModelLod.MMDC.Kind;
        dbCommand.Parameters[DISDomainParamName].Value = movingModelLod.MMDC.Domain;
        dbCommand.Parameters[DISCountryParamName].Value = movingModelLod.MMDC.Country;
        dbCommand.Parameters[DISCategoryParamName].Value = movingModelLod.MMDC.Category;
        dbCommand.Parameters[DISSubcategoryParamName].Value = movingModelLod.MMDC.Subcategory;
        dbCommand.Parameters[DISSpecificParamName].Value = movingModelLod.MMDC.Specific;
        dbCommand.Parameters[DISExtraParamName].Value = movingModelLod.MMDC.Extra;
        dbCommand.Parameters[FileTypeParamName].Value = movingModelLod.FileType;
    }

    /// <inheritdoc/>
    public int InsertIntoMovingModelLod(string cdbName, MovingModelLod movingModelLod, byte[] content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoMovingModelLodCommand = dbConnection.CreateCommand();
        InitializeInsertIntoMovingModelLodCommand(insertIntoMovingModelLodCommand);
        insertIntoMovingModelLodCommand.Prepare();

        return InsertIntoMovingModelLod(insertIntoMovingModelLodCommand, cdbName, movingModelLod, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoMovingModelLod(string, MovingModelLod, byte[])"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoMovingModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoMovingModelLod(DbCommand insertIntoMovingModelLodCommand,
        string cdbName, MovingModelLod movingModelLod, byte[] content)
    {
        insertIntoMovingModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelLodParameters(insertIntoMovingModelLodCommand, movingModelLod);
        insertIntoMovingModelLodCommand.Parameters[ContentParamName].Value = content;

        return insertIntoMovingModelLodCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public int InsertIntoMovingModelLod(string cdbName, MovingModelLod movingModelLod, Stream content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoMovingModelLodCommand = dbConnection.CreateCommand();
        InitializeInsertIntoMovingModelLodCommand(insertIntoMovingModelLodCommand);
        insertIntoMovingModelLodCommand.Prepare();

        return InsertIntoMovingModelLod(insertIntoMovingModelLodCommand, cdbName, movingModelLod, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoMovingModelLod(string, MovingModelLod, Stream)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoMovingModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoMovingModelLod(DbCommand insertIntoMovingModelLodCommand,
        string cdbName, MovingModelLod movingModelLod, Stream content)
    {
        insertIntoMovingModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelLodParameters(insertIntoMovingModelLodCommand, movingModelLod);
        insertIntoMovingModelLodCommand.Parameters[ContentParamName].Value = content;

        return insertIntoMovingModelLodCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoMovingModelLodAsync(string cdbName, MovingModelLod movingModelLod, byte[] content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoMovingModelLodCommand = dbConnection.CreateCommand();
        InitializeInsertIntoMovingModelLodCommand(insertIntoMovingModelLodCommand);
        await insertIntoMovingModelLodCommand.PrepareAsync(cancellationToken);

        return await InsertIntoMovingModelLodAsync(insertIntoMovingModelLodCommand, cdbName, movingModelLod, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoMovingModelLodAsync(string, MovingModelLod, byte[], CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoMovingModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoMovingModelLodAsync(DbCommand insertIntoMovingModelLodCommand,
        string cdbName, MovingModelLod movingModelLod, byte[] content,
        CancellationToken cancellationToken)
    {
        insertIntoMovingModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelLodParameters(insertIntoMovingModelLodCommand, movingModelLod);
        insertIntoMovingModelLodCommand.Parameters[ContentParamName].Value = content;

        return insertIntoMovingModelLodCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoMovingModelLodAsync(string cdbName, MovingModelLod movingModelLod, Stream content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoMovingModelLodCommand = dbConnection.CreateCommand();
        InitializeInsertIntoMovingModelLodCommand(insertIntoMovingModelLodCommand);
        await insertIntoMovingModelLodCommand.PrepareAsync(cancellationToken);

        return await InsertIntoMovingModelLodAsync(insertIntoMovingModelLodCommand, cdbName, movingModelLod, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoMovingModelLodAsync(string, MovingModelLod, Stream, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoMovingModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoMovingModelLodAsync(DbCommand insertIntoMovingModelLodCommand,
        string cdbName, MovingModelLod movingModelLod, Stream content,
        CancellationToken cancellationToken)
    {
        insertIntoMovingModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelLodParameters(insertIntoMovingModelLodCommand, movingModelLod);
        insertIntoMovingModelLodCommand.Parameters[ContentParamName].Value = content;

        return insertIntoMovingModelLodCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    #endregion

    #region Select

    /// <summary>
    /// The SQL statement to select a row from the Moving Model Level of Detail table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><description><see cref="DISKindParamName"/></description></item>
    ///   <item><description><see cref="DISDomainParamName"/></description></item>
    ///   <item><description><see cref="DISCountryParamName"/></description></item>
    ///   <item><description><see cref="DISCategoryParamName"/></description></item>
    ///   <item><description><see cref="DISSubcategoryParamName"/></description></item>
    ///   <item><description><see cref="DISSpecificParamName"/></description></item>
    ///   <item><description><see cref="DISExtraParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// <list type="bullet">
    ///   <listheader><description>Selected Columns</description></listheader>
    ///   <item><description><see cref="ContentColumnName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string SelectFromMovingModelLodStatement
    {
        get;
    }

    internal void InitializeSelectFromMovingModelLodCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = SelectFromMovingModelLodStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachMovingModelLodParameters(dbCommand);
    }

    /// <inheritdoc/>
    public bool TrySelectFromMovingModelLod(string cdbName, MovingModelLod movingModelLod,
        Action<Stream> fileFoundAction)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand selectFromMovingModelLodCommand = dbConnection.CreateCommand();
        InitializeSelectFromMovingModelLodCommand(selectFromMovingModelLodCommand);
        selectFromMovingModelLodCommand.Prepare();

        return TrySelectFromMovingModelLod(selectFromMovingModelLodCommand, cdbName, movingModelLod, fileFoundAction);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromMovingModelLod(string, MovingModelLod, Action{Stream})"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromMovingModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual bool TrySelectFromMovingModelLod(DbCommand selectFromMovingModelLodCommand,
        string cdbName, MovingModelLod movingModelLod,
        Action<Stream> fileFoundAction)
    {
        selectFromMovingModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelLodParameters(selectFromMovingModelLodCommand, movingModelLod);

        using DbDataReader dbDataReader = selectFromMovingModelLodCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        do
        {
            while (dbDataReader.Read())
            {
                using Stream stream = dbDataReader.GetStream(ContentColumnName);
                fileFoundAction(stream);
                return true;
            }
        } while (dbDataReader.NextResult());
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> TrySelectFromMovingModelLodAsync(string cdbName, MovingModelLod movingModelLod,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand selectFromMovingModelLodCommand = dbConnection.CreateCommand();
        InitializeSelectFromMovingModelLodCommand(selectFromMovingModelLodCommand);
        await selectFromMovingModelLodCommand.PrepareAsync(cancellationToken);

        return await TrySelectFromMovingModelLodAsync(selectFromMovingModelLodCommand, cdbName, movingModelLod,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromMovingModelLodAsync(string, MovingModelLod, Func{Stream, CancellationToken, Task}, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromMovingModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<bool> TrySelectFromMovingModelLodAsync(DbCommand selectFromMovingModelLodCommand,
        string cdbName, MovingModelLod movingModelLod,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        selectFromMovingModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelLodParameters(selectFromMovingModelLodCommand, movingModelLod);

        await using DbDataReader dbDataReader = await selectFromMovingModelLodCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        do
        {
            while (await dbDataReader.ReadAsync(cancellationToken))
            {
                await using Stream stream = dbDataReader.GetStream(ContentColumnName);
                await fileFoundAsyncAction(stream, cancellationToken);
                return true;
            }
        } while (await dbDataReader.NextResultAsync(cancellationToken));
        return false;
    }

    /// <inheritdoc/>
    public Stream? SelectFromMovingModelLod(string cdbName, MovingModelLod movingModelLod)
    {
        DbConnection dbConnection = dbDataSource.OpenConnection();
        try
        {
            DbCommand selectFromMovingModelLodCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromMovingModelLodCommand(selectFromMovingModelLodCommand);
                selectFromMovingModelLodCommand.Prepare();

                Stream? stream = SelectFromMovingModelLod(selectFromMovingModelLodCommand, cdbName, movingModelLod);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromMovingModelLodCommand, dbConnection);
                }
                else
                {
                    selectFromMovingModelLodCommand.Dispose();
                    dbConnection.Dispose();
                    return null;
                }
            }
            catch (Exception)
            {
                selectFromMovingModelLodCommand.Dispose();
                throw;
            }
        }
        catch (Exception)
        {
            dbConnection.Dispose();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromMovingModelLod(string, MovingModelLod)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromMovingModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Stream? SelectFromMovingModelLod(DbCommand selectFromMovingModelLodCommand,
        string cdbName, MovingModelLod movingModelLod)
    {
        selectFromMovingModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelLodParameters(selectFromMovingModelLodCommand, movingModelLod);

        DbDataReader dbDataReader = selectFromMovingModelLodCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        try
        {
            do
            {
                while (dbDataReader.Read())
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (dbDataReader.NextResult());
            dbDataReader.Dispose();
            return null;
        }
        catch (Exception)
        {
            dbDataReader.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Stream?> SelectFromMovingModelLodAsync(string cdbName, MovingModelLod movingModelLod,
        CancellationToken cancellationToken)
    {
        DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            DbCommand selectFromMovingModelLodCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromMovingModelLodCommand(selectFromMovingModelLodCommand);
                await selectFromMovingModelLodCommand.PrepareAsync(cancellationToken);

                Stream? stream = await SelectFromMovingModelLodAsync(selectFromMovingModelLodCommand, cdbName, movingModelLod, cancellationToken);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromMovingModelLodCommand, dbConnection);
                }
                else
                {
                    await selectFromMovingModelLodCommand.DisposeAsync();
                    await dbConnection.DisposeAsync();
                    return null;
                }
            }
            catch (Exception)
            {
                await selectFromMovingModelLodCommand.DisposeAsync();
                throw;
            }
        }
        catch (Exception)
        {
            await dbConnection.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromMovingModelLodAsync(string, MovingModelLod, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromMovingModelLodCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<Stream?> SelectFromMovingModelLodAsync(DbCommand selectFromMovingModelLodCommand,
        string cdbName, MovingModelLod movingModelLod,
        CancellationToken cancellationToken)
    {
        selectFromMovingModelLodCommand.Parameters[CdbParamName].Value = cdbName;
        SetMovingModelLodParameters(selectFromMovingModelLodCommand, movingModelLod);

        DbDataReader dbDataReader = await selectFromMovingModelLodCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        try
        {
            do
            {
                while (await dbDataReader.ReadAsync(cancellationToken))
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (await dbDataReader.NextResultAsync(cancellationToken));
            await dbDataReader.DisposeAsync();
            return null;
        }
        catch (Exception)
        {
            await dbDataReader.DisposeAsync();
            throw;
        }
    }

    #endregion

    #endregion

    #region Tile

    /// <summary>
    /// The SQL DDL statement to create the Tile table.
    /// </summary>
    protected abstract string CreateTableTileStatement
    {
        get;
    }

    #region Insert

    /// <summary>
    /// The SQL statement to insert a row into the Tile table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="LatitudeParamName"/></description></item>
    ///   <item><description><see cref="LongitudeParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><description><see cref="UpParamName"/></description></item>
    ///   <item><description><see cref="RightParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    ///   <item><description><see cref="ContentParamName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string InsertIntoTileStatement
    {
        get;
    }

    internal void InitializeInsertIntoTileCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = InsertIntoTileStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachTileParameters(dbCommand);
        CreateAndAttachParameter(dbCommand, ContentParamName, DbType.Binary);
    }

    private void CreateAndAttachTileParameters(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, LatitudeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, LongitudeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DatasetParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector1ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector2ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, LevelOfDetailParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, UpParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, RightParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, FileTypeParamName, DbType.String);
    }

    private void SetTileParameters(DbCommand dbCommand, Tile tile)
    {
        dbCommand.Parameters[LatitudeParamName].Value = tile.LatitudeValue.Value;
        dbCommand.Parameters[LongitudeParamName].Value = tile.LongitudeValue.Value;
        dbCommand.Parameters[DatasetParamName].Value = tile.DatasetValue.Value;
        dbCommand.Parameters[ComponentSelector1ParamName].Value = tile.ComponentSelector1;
        dbCommand.Parameters[ComponentSelector2ParamName].Value = tile.ComponentSelector2;
        dbCommand.Parameters[LevelOfDetailParamName].Value = tile.Level.Value;
        dbCommand.Parameters[UpParamName].Value = tile.Up;
        dbCommand.Parameters[RightParamName].Value = tile.Right;
        dbCommand.Parameters[FileTypeParamName].Value = tile.FileType;
    }

    /// <inheritdoc/>
    public int InsertIntoTile(string cdbName, Tile tile, byte[] content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoTileCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTileCommand(insertIntoTileCommand);
        insertIntoTileCommand.Prepare();

        return InsertIntoTile(insertIntoTileCommand, cdbName, tile, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTile(string, Tile, byte[])"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTileCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoTile(DbCommand insertIntoTileCommand,
        string cdbName, Tile tile, byte[] content)
    {
        insertIntoTileCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileParameters(insertIntoTileCommand, tile);
        insertIntoTileCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTileCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public int InsertIntoTile(string cdbName, Tile tile, Stream content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoTileCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTileCommand(insertIntoTileCommand);
        insertIntoTileCommand.Prepare();

        return InsertIntoTile(insertIntoTileCommand, cdbName, tile, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTile(string, Tile, Stream)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTileCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoTile(DbCommand insertIntoTileCommand,
        string cdbName, Tile tile, Stream content)
    {
        insertIntoTileCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileParameters(insertIntoTileCommand, tile);
        insertIntoTileCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTileCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoTileAsync(string cdbName, Tile tile, byte[] content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoTileCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTileCommand(insertIntoTileCommand);
        await insertIntoTileCommand.PrepareAsync(cancellationToken);

        return await InsertIntoTileAsync(insertIntoTileCommand, cdbName, tile, content, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTileAsync(string, Tile, byte[], CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTileCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoTileAsync(DbCommand insertIntoTileCommand,
        string cdbName, Tile tile, byte[] content,
        CancellationToken cancellationToken)
    {
        insertIntoTileCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileParameters(insertIntoTileCommand, tile);
        insertIntoTileCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTileCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoTileAsync(string cdbName, Tile tile, Stream content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoTileCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTileCommand(insertIntoTileCommand);
        await insertIntoTileCommand.PrepareAsync(cancellationToken);

        return await InsertIntoTileAsync(insertIntoTileCommand, cdbName, tile, content, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTileAsync(string, Tile, Stream, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTileCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoTileAsync(DbCommand insertIntoTileCommand,
        string cdbName, Tile tile, Stream content,
        CancellationToken cancellationToken)
    {
        insertIntoTileCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileParameters(insertIntoTileCommand, tile);
        insertIntoTileCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTileCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    #endregion

    #region Select

    /// <summary>
    /// The SQL statement to select a row from the Tile table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="LatitudeParamName"/></description></item>
    ///   <item><description><see cref="LongitudeParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><description><see cref="UpParamName"/></description></item>
    ///   <item><description><see cref="RightParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// <list type="bullet">
    ///   <listheader><description>Selected Columns</description></listheader>
    ///   <item><description><see cref="ContentColumnName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string SelectFromTileStatement
    {
        get;
    }

    internal void InitializeSelectFromTileCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = SelectFromTileStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachTileParameters(dbCommand);
    }

    /// <inheritdoc/>
    public bool TrySelectFromTile(string cdbName, Tile tile,
        Action<Stream> fileFoundAction)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand selectFromTileCommand = dbConnection.CreateCommand();
        InitializeSelectFromTileCommand(selectFromTileCommand);
        selectFromTileCommand.Prepare();

        return TrySelectFromTile(selectFromTileCommand, cdbName, tile, fileFoundAction);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromTile(string, Tile, Action{Stream})"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTileCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual bool TrySelectFromTile(DbCommand selectFromTileCommand,
        string cdbName, Tile tile,
        Action<Stream> fileFoundAction)
    {
        selectFromTileCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileParameters(selectFromTileCommand, tile);

        using DbDataReader dbDataReader = selectFromTileCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        do
        {
            while (dbDataReader.Read())
            {
                using Stream stream = dbDataReader.GetStream(ContentColumnName);
                fileFoundAction(stream);
                return true;
            }
        } while (dbDataReader.NextResult());
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> TrySelectFromTileAsync(string cdbName, Tile tile,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand selectFromTileCommand = dbConnection.CreateCommand();
        InitializeSelectFromTileCommand(selectFromTileCommand);
        await selectFromTileCommand.PrepareAsync(cancellationToken);

        return await TrySelectFromTileAsync(selectFromTileCommand, cdbName, tile, fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromTileAsync(string, Tile, Func{Stream, CancellationToken, Task}, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTileCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<bool> TrySelectFromTileAsync(DbCommand selectFromTileCommand,
        string cdbName, Tile tile,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        selectFromTileCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileParameters(selectFromTileCommand, tile);

        await using DbDataReader dbDataReader = await selectFromTileCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        do
        {
            while (await dbDataReader.ReadAsync(cancellationToken))
            {
                await using Stream stream = dbDataReader.GetStream(ContentColumnName);
                await fileFoundAsyncAction(stream, cancellationToken);
                return true;
            }
        } while (await dbDataReader.NextResultAsync(cancellationToken));
        return false;
    }

    /// <inheritdoc/>
    public Stream? SelectFromTile(string cdbName, Tile tile)
    {
        DbConnection dbConnection = dbDataSource.OpenConnection();
        try
        {
            DbCommand selectFromTileCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromTileCommand(selectFromTileCommand);
                selectFromTileCommand.Prepare();

                Stream? stream = SelectFromTile(selectFromTileCommand, cdbName, tile);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromTileCommand, dbConnection);
                }
                else
                {
                    selectFromTileCommand.Dispose();
                    dbConnection.Dispose();
                    return null;
                }
            }
            catch (Exception)
            {
                selectFromTileCommand.Dispose();
                throw;
            }
        }
        catch (Exception)
        {
            dbConnection.Dispose();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromTile(string, Tile)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTileCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Stream? SelectFromTile(DbCommand selectFromTileCommand,
        string cdbName, Tile tile)
    {
        selectFromTileCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileParameters(selectFromTileCommand, tile);

        DbDataReader dbDataReader = selectFromTileCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        try
        {
            do
            {
                while (dbDataReader.Read())
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (dbDataReader.NextResult());
            dbDataReader.Dispose();
            return null;
        }
        catch (Exception)
        {
            dbDataReader.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Stream?> SelectFromTileAsync(string cdbName, Tile tile,
        CancellationToken cancellationToken)
    {
        DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            DbCommand selectFromTileCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromTileCommand(selectFromTileCommand);
                await selectFromTileCommand.PrepareAsync(cancellationToken);

                Stream? stream = await SelectFromTileAsync(selectFromTileCommand, cdbName, tile, cancellationToken);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromTileCommand, dbConnection);
                }
                else
                {
                    await selectFromTileCommand.DisposeAsync();
                    await dbConnection.DisposeAsync();
                    return null;
                }
            }
            catch (Exception)
            {
                await selectFromTileCommand.DisposeAsync();
                throw;
            }
        }
        catch (Exception)
        {
            await dbConnection.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromTileAsync(string, Tile, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTileCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<Stream?> SelectFromTileAsync(DbCommand selectFromTileCommand,
        string cdbName, Tile tile,
        CancellationToken cancellationToken)
    {
        selectFromTileCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileParameters(selectFromTileCommand, tile);

        DbDataReader dbDataReader = await selectFromTileCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        try
        {
            do
            {
                while (await dbDataReader.ReadAsync(cancellationToken))
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (await dbDataReader.NextResultAsync(cancellationToken));
            await dbDataReader.DisposeAsync();
            return null;
        }
        catch (Exception)
        {
            await dbDataReader.DisposeAsync();
            throw;
        }
    }

    #endregion

    #endregion

    #region Tile Archived Feature

    /// <summary>
    /// The SQL DDL statement to create the TileArchivedFeature table.
    /// </summary>
    protected abstract string CreateTableTileArchivedFeatureStatement
    {
        get;
    }

    #region Insert

    /// <summary>
    /// The SQL statement to insert a row into the TileArchivedFeature table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="LatitudeParamName"/></description></item>
    ///   <item><description><see cref="LongitudeParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><description><see cref="UpParamName"/></description></item>
    ///   <item><description><see cref="RightParamName"/></description></item>
    ///   <item><description><see cref="FeatureCategoryParamName"/></description></item>
    ///   <item><description><see cref="FeatureSubcategoryParamName"/></description></item>
    ///   <item><description><see cref="FeatureTypeParamName"/></description></item>
    ///   <item><description><see cref="FeatureSubcodeParamName"/></description></item>
    ///   <item><description><see cref="ModelNameParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    ///   <item><description><see cref="ContentParamName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string InsertIntoTileArchivedFeatureStatement
    {
        get;
    }

    internal void InitializeInsertIntoTileArchivedFeatureCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = InsertIntoTileArchivedFeatureStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachTileArchivedFeatureParameters(dbCommand);
        CreateAndAttachParameter(dbCommand, ContentParamName, DbType.Binary);
    }

    private void CreateAndAttachTileArchivedFeatureParameters(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, LatitudeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, LongitudeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DatasetParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector1ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector2ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, LevelOfDetailParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, UpParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, RightParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, FeatureCategoryParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, FeatureSubcategoryParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, FeatureTypeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, FeatureSubcodeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ModelNameParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, FileTypeParamName, DbType.String);
    }

    private void SetTileArchivedFeatureParameters(DbCommand dbCommand, TileArchivedFeature tileArchivedFeature)
    {
        dbCommand.Parameters[LatitudeParamName].Value = tileArchivedFeature.LatitudeValue.Value;
        dbCommand.Parameters[LongitudeParamName].Value = tileArchivedFeature.LongitudeValue.Value;
        dbCommand.Parameters[DatasetParamName].Value = tileArchivedFeature.DatasetValue.Value;
        dbCommand.Parameters[ComponentSelector1ParamName].Value = tileArchivedFeature.ComponentSelector1;
        dbCommand.Parameters[ComponentSelector2ParamName].Value = tileArchivedFeature.ComponentSelector2;
        dbCommand.Parameters[LevelOfDetailParamName].Value = tileArchivedFeature.Level.Value;
        dbCommand.Parameters[UpParamName].Value = tileArchivedFeature.Up;
        dbCommand.Parameters[RightParamName].Value = tileArchivedFeature.Right;
        dbCommand.Parameters[FeatureCategoryParamName].Value = tileArchivedFeature.FeatureCode.Category;
        dbCommand.Parameters[FeatureSubcategoryParamName].Value = tileArchivedFeature.FeatureCode.Subcategory;
        dbCommand.Parameters[FeatureTypeParamName].Value = tileArchivedFeature.FeatureCode.Type;
        dbCommand.Parameters[FeatureSubcodeParamName].Value = tileArchivedFeature.FeatureSubcode;
        dbCommand.Parameters[ModelNameParamName].Value = tileArchivedFeature.Name;
        dbCommand.Parameters[FileTypeParamName].Value = tileArchivedFeature.FileType;
    }

    /// <inheritdoc/>
    public int InsertIntoTileArchivedFeature(string cdbName, TileArchivedFeature tileArchivedFeature, byte[] content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoTileArchivedFeatureCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTileArchivedFeatureCommand(insertIntoTileArchivedFeatureCommand);
        insertIntoTileArchivedFeatureCommand.Prepare();

        return InsertIntoTileArchivedFeature(insertIntoTileArchivedFeatureCommand, cdbName, tileArchivedFeature, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTileArchivedFeature(string, TileArchivedFeature, byte[])"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTileArchivedFeatureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoTileArchivedFeature(DbCommand insertIntoTileArchivedFeatureCommand,
        string cdbName, TileArchivedFeature tileArchivedFeature, byte[] content)
    {
        insertIntoTileArchivedFeatureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedFeatureParameters(insertIntoTileArchivedFeatureCommand, tileArchivedFeature);
        insertIntoTileArchivedFeatureCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTileArchivedFeatureCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public int InsertIntoTileArchivedFeature(string cdbName, TileArchivedFeature tileArchivedFeature, Stream content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoTileArchivedFeatureCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTileArchivedFeatureCommand(insertIntoTileArchivedFeatureCommand);
        insertIntoTileArchivedFeatureCommand.Prepare();

        return InsertIntoTileArchivedFeature(insertIntoTileArchivedFeatureCommand, cdbName, tileArchivedFeature, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTileArchivedFeature(string, TileArchivedFeature, Stream)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTileArchivedFeatureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoTileArchivedFeature(DbCommand insertIntoTileArchivedFeatureCommand,
        string cdbName, TileArchivedFeature tileArchivedFeature, Stream content)
    {
        insertIntoTileArchivedFeatureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedFeatureParameters(insertIntoTileArchivedFeatureCommand, tileArchivedFeature);
        insertIntoTileArchivedFeatureCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTileArchivedFeatureCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoTileArchivedFeatureAsync(string cdbName, TileArchivedFeature tileArchivedFeature, byte[] content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoTileArchivedFeatureCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTileArchivedFeatureCommand(insertIntoTileArchivedFeatureCommand);
        await insertIntoTileArchivedFeatureCommand.PrepareAsync(cancellationToken);

        return await InsertIntoTileArchivedFeatureAsync(insertIntoTileArchivedFeatureCommand, cdbName, tileArchivedFeature, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTileArchivedFeatureAsync(string, TileArchivedFeature, byte[], CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTileArchivedFeatureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoTileArchivedFeatureAsync(DbCommand insertIntoTileArchivedFeatureCommand,
        string cdbName, TileArchivedFeature tileArchivedFeature, byte[] content,
        CancellationToken cancellationToken)
    {
        insertIntoTileArchivedFeatureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedFeatureParameters(insertIntoTileArchivedFeatureCommand, tileArchivedFeature);
        insertIntoTileArchivedFeatureCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTileArchivedFeatureCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoTileArchivedFeatureAsync(string cdbName, TileArchivedFeature tileArchivedFeature, Stream content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoTileArchivedFeatureCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTileArchivedFeatureCommand(insertIntoTileArchivedFeatureCommand);
        await insertIntoTileArchivedFeatureCommand.PrepareAsync(cancellationToken);

        return await InsertIntoTileArchivedFeatureAsync(insertIntoTileArchivedFeatureCommand, cdbName, tileArchivedFeature, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTileArchivedFeatureAsync(string, TileArchivedFeature, Stream, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTileArchivedFeatureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoTileArchivedFeatureAsync(DbCommand insertIntoTileArchivedFeatureCommand,
        string cdbName, TileArchivedFeature tileArchivedFeature, Stream content,
        CancellationToken cancellationToken)
    {
        insertIntoTileArchivedFeatureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedFeatureParameters(insertIntoTileArchivedFeatureCommand, tileArchivedFeature);
        insertIntoTileArchivedFeatureCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTileArchivedFeatureCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    #endregion

    #region Select

    /// <summary>
    /// The SQL statement to select a row from the TileArchivedFeature table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="LatitudeParamName"/></description></item>
    ///   <item><description><see cref="LongitudeParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><description><see cref="UpParamName"/></description></item>
    ///   <item><description><see cref="RightParamName"/></description></item>
    ///   <item><description><see cref="FeatureCategoryParamName"/></description></item>
    ///   <item><description><see cref="FeatureSubcategoryParamName"/></description></item>
    ///   <item><description><see cref="FeatureTypeParamName"/></description></item>
    ///   <item><description><see cref="FeatureSubcodeParamName"/></description></item>
    ///   <item><description><see cref="ModelNameParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// <list type="bullet">
    ///   <listheader><description>Selected Columns</description></listheader>
    ///   <item><description><see cref="ContentColumnName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string SelectFromTileArchivedFeatureStatement
    {
        get;
    }

    internal void InitializeSelectFromTileArchivedFeatureCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = SelectFromTileArchivedFeatureStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachTileArchivedFeatureParameters(dbCommand);
    }

    /// <inheritdoc/>
    public bool TrySelectFromTileArchivedFeature(string cdbName, TileArchivedFeature tileArchivedFeature,
        Action<Stream> fileFoundAction)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand selectFromTileArchivedFeatureCommand = dbConnection.CreateCommand();
        InitializeSelectFromTileArchivedFeatureCommand(selectFromTileArchivedFeatureCommand);
        selectFromTileArchivedFeatureCommand.Prepare();

        return TrySelectFromTileArchivedFeature(selectFromTileArchivedFeatureCommand, cdbName, tileArchivedFeature, fileFoundAction);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromTileArchivedFeature(string, TileArchivedFeature, Action{Stream})"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTileArchivedFeatureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual bool TrySelectFromTileArchivedFeature(DbCommand selectFromTileArchivedFeatureCommand,
        string cdbName, TileArchivedFeature tileArchivedFeature,
        Action<Stream> fileFoundAction)
    {
        selectFromTileArchivedFeatureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedFeatureParameters(selectFromTileArchivedFeatureCommand, tileArchivedFeature);

        using DbDataReader dbDataReader = selectFromTileArchivedFeatureCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        do
        {
            while (dbDataReader.Read())
            {
                using Stream stream = dbDataReader.GetStream(ContentColumnName);
                fileFoundAction(stream);
                return true;
            }
        } while (dbDataReader.NextResult());
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> TrySelectFromTileArchivedFeatureAsync(string cdbName, TileArchivedFeature tileArchivedFeature,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand selectFromTileArchivedFeatureCommand = dbConnection.CreateCommand();
        InitializeSelectFromTileArchivedFeatureCommand(selectFromTileArchivedFeatureCommand);
        await selectFromTileArchivedFeatureCommand.PrepareAsync(cancellationToken);

        return await TrySelectFromTileArchivedFeatureAsync(selectFromTileArchivedFeatureCommand, cdbName, tileArchivedFeature,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromTileArchivedFeatureAsync(string, TileArchivedFeature, Func{Stream, CancellationToken, Task}, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTileArchivedFeatureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<bool> TrySelectFromTileArchivedFeatureAsync(DbCommand selectFromTileArchivedFeatureCommand,
        string cdbName, TileArchivedFeature tileArchivedFeature,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        selectFromTileArchivedFeatureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedFeatureParameters(selectFromTileArchivedFeatureCommand, tileArchivedFeature);

        await using DbDataReader dbDataReader = await selectFromTileArchivedFeatureCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        do
        {
            while (await dbDataReader.ReadAsync(cancellationToken))
            {
                await using Stream stream = dbDataReader.GetStream(ContentColumnName);
                await fileFoundAsyncAction(stream, cancellationToken);
                return true;
            }
        } while (await dbDataReader.NextResultAsync(cancellationToken));
        return false;
    }

    /// <inheritdoc/>
    public Stream? SelectFromTileArchivedFeature(string cdbName, TileArchivedFeature tileArchivedFeature)
    {
        DbConnection dbConnection = dbDataSource.OpenConnection();
        try
        {
            DbCommand selectFromTileArchivedFeatureCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromTileArchivedFeatureCommand(selectFromTileArchivedFeatureCommand);
                selectFromTileArchivedFeatureCommand.Prepare();

                Stream? stream = SelectFromTileArchivedFeature(selectFromTileArchivedFeatureCommand, cdbName, tileArchivedFeature);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromTileArchivedFeatureCommand, dbConnection);
                }
                else
                {
                    selectFromTileArchivedFeatureCommand.Dispose();
                    dbConnection.Dispose();
                    return null;
                }
            }
            catch (Exception)
            {
                selectFromTileArchivedFeatureCommand.Dispose();
                throw;
            }
        }
        catch (Exception)
        {
            dbConnection.Dispose();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromTileArchivedFeature(string, TileArchivedFeature)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTileArchivedFeatureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Stream? SelectFromTileArchivedFeature(DbCommand selectFromTileArchivedFeatureCommand,
        string cdbName, TileArchivedFeature tileArchivedFeature)
    {
        selectFromTileArchivedFeatureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedFeatureParameters(selectFromTileArchivedFeatureCommand, tileArchivedFeature);

        DbDataReader dbDataReader = selectFromTileArchivedFeatureCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        try
        {
            do
            {
                while (dbDataReader.Read())
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (dbDataReader.NextResult());
            dbDataReader.Dispose();
            return null;
        }
        catch (Exception)
        {
            dbDataReader.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Stream?> SelectFromTileArchivedFeatureAsync(string cdbName, TileArchivedFeature tileArchivedFeature,
        CancellationToken cancellationToken)
    {
        DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            DbCommand selectFromTileArchivedFeatureCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromTileArchivedFeatureCommand(selectFromTileArchivedFeatureCommand);
                await selectFromTileArchivedFeatureCommand.PrepareAsync(cancellationToken);

                Stream? stream = await SelectFromTileArchivedFeatureAsync(selectFromTileArchivedFeatureCommand, cdbName, tileArchivedFeature, cancellationToken);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromTileArchivedFeatureCommand, dbConnection);
                }
                else
                {
                    await selectFromTileArchivedFeatureCommand.DisposeAsync();
                    await dbConnection.DisposeAsync();
                    return null;
                }
            }
            catch (Exception)
            {
                await selectFromTileArchivedFeatureCommand.DisposeAsync();
                throw;
            }
        }
        catch (Exception)
        {
            await dbConnection.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromTileArchivedFeatureAsync(string, TileArchivedFeature, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTileArchivedFeatureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<Stream?> SelectFromTileArchivedFeatureAsync(DbCommand selectFromTileArchivedFeatureCommand,
        string cdbName, TileArchivedFeature tileArchivedFeature,
        CancellationToken cancellationToken)
    {
        selectFromTileArchivedFeatureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedFeatureParameters(selectFromTileArchivedFeatureCommand, tileArchivedFeature);

        DbDataReader dbDataReader = await selectFromTileArchivedFeatureCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        try
        {
            do
            {
                while (await dbDataReader.ReadAsync(cancellationToken))
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (await dbDataReader.NextResultAsync(cancellationToken));
            await dbDataReader.DisposeAsync();
            return null;
        }
        catch (Exception)
        {
            await dbDataReader.DisposeAsync();
            throw;
        }
    }

    #endregion

    #endregion

    #region Tile Archived Texture

    /// <summary>
    /// The SQL DDL statement to create the TileArchivedTexture table.
    /// </summary>
    protected abstract string CreateTableTileArchivedTextureStatement
    {
        get;
    }

    #region Insert

    /// <summary>
    /// The SQL statement to insert a row into the TileArchivedTexture table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="LatitudeParamName"/></description></item>
    ///   <item><description><see cref="LongitudeParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><description><see cref="UpParamName"/></description></item>
    ///   <item><description><see cref="RightParamName"/></description></item>
    ///   <item><description><see cref="TextureNameParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    ///   <item><description><see cref="ContentParamName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string InsertIntoTileArchivedTextureStatement
    {
        get;
    }

    internal void InitializeInsertIntoTileArchivedTextureCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = InsertIntoTileArchivedTextureStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachTileArchivedTextureParameters(dbCommand);
        CreateAndAttachParameter(dbCommand, ContentParamName, DbType.Binary);
    }

    private void CreateAndAttachTileArchivedTextureParameters(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, LatitudeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, LongitudeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, DatasetParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector1ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector2ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, LevelOfDetailParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, UpParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, RightParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, TextureNameParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, FileTypeParamName, DbType.String);
    }

    private void SetTileArchivedTextureParameters(DbCommand dbCommand, TileArchivedTexture tileArchivedTexture)
    {
        dbCommand.Parameters[LatitudeParamName].Value = tileArchivedTexture.LatitudeValue.Value;
        dbCommand.Parameters[LongitudeParamName].Value = tileArchivedTexture.LongitudeValue.Value;
        dbCommand.Parameters[DatasetParamName].Value = tileArchivedTexture.DatasetValue.Value;
        dbCommand.Parameters[ComponentSelector1ParamName].Value = tileArchivedTexture.ComponentSelector1;
        dbCommand.Parameters[ComponentSelector2ParamName].Value = tileArchivedTexture.ComponentSelector2;
        dbCommand.Parameters[LevelOfDetailParamName].Value = tileArchivedTexture.Level.Value;
        dbCommand.Parameters[UpParamName].Value = tileArchivedTexture.Up;
        dbCommand.Parameters[RightParamName].Value = tileArchivedTexture.Right;
        dbCommand.Parameters[TextureNameParamName].Value = tileArchivedTexture.Name;
        dbCommand.Parameters[FileTypeParamName].Value = tileArchivedTexture.FileType;
    }

    /// <inheritdoc/>
    public int InsertIntoTileArchivedTexture(string cdbName, TileArchivedTexture tileArchivedTexture, byte[] content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoTileArchivedTextureCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTileArchivedTextureCommand(insertIntoTileArchivedTextureCommand);
        insertIntoTileArchivedTextureCommand.Prepare();

        return InsertIntoTileArchivedTexture(insertIntoTileArchivedTextureCommand, cdbName, tileArchivedTexture, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTileArchivedTextureAsync(string, TileArchivedTexture, byte[], CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTileArchivedTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoTileArchivedTexture(DbCommand insertIntoTileArchivedTextureCommand,
        string cdbName, TileArchivedTexture tileArchivedTexture, byte[] content)
    {
        insertIntoTileArchivedTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedTextureParameters(insertIntoTileArchivedTextureCommand, tileArchivedTexture);
        insertIntoTileArchivedTextureCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTileArchivedTextureCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public int InsertIntoTileArchivedTexture(string cdbName, TileArchivedTexture tileArchivedTexture, Stream content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoTileArchivedTextureCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTileArchivedTextureCommand(insertIntoTileArchivedTextureCommand);
        insertIntoTileArchivedTextureCommand.Prepare();

        return InsertIntoTileArchivedTexture(insertIntoTileArchivedTextureCommand, cdbName, tileArchivedTexture, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTileArchivedTextureAsync(string, TileArchivedTexture, Stream, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTileArchivedTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoTileArchivedTexture(DbCommand insertIntoTileArchivedTextureCommand,
        string cdbName, TileArchivedTexture tileArchivedTexture, Stream content)
    {
        insertIntoTileArchivedTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedTextureParameters(insertIntoTileArchivedTextureCommand, tileArchivedTexture);
        insertIntoTileArchivedTextureCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTileArchivedTextureCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoTileArchivedTextureAsync(string cdbName, TileArchivedTexture tileArchivedTexture, byte[] content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoTileArchivedTextureCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTileArchivedTextureCommand(insertIntoTileArchivedTextureCommand);
        await insertIntoTileArchivedTextureCommand.PrepareAsync(cancellationToken);

        return await InsertIntoTileArchivedTextureAsync(insertIntoTileArchivedTextureCommand, cdbName, tileArchivedTexture, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTileArchivedTextureAsync(string, TileArchivedTexture, byte[], CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTileArchivedTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoTileArchivedTextureAsync(DbCommand insertIntoTileArchivedTextureCommand,
        string cdbName, TileArchivedTexture tileArchivedTexture, byte[] content,
        CancellationToken cancellationToken)
    {
        insertIntoTileArchivedTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedTextureParameters(insertIntoTileArchivedTextureCommand, tileArchivedTexture);
        insertIntoTileArchivedTextureCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTileArchivedTextureCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoTileArchivedTextureAsync(string cdbName, TileArchivedTexture tileArchivedTexture, Stream content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoTileArchivedTextureCommand = dbConnection.CreateCommand();
        InitializeInsertIntoTileArchivedTextureCommand(insertIntoTileArchivedTextureCommand);
        await insertIntoTileArchivedTextureCommand.PrepareAsync(cancellationToken);

        return await InsertIntoTileArchivedTextureAsync(insertIntoTileArchivedTextureCommand, cdbName, tileArchivedTexture, content,
            cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoTileArchivedTextureAsync(string, TileArchivedTexture, Stream, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoTileArchivedTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoTileArchivedTextureAsync(DbCommand insertIntoTileArchivedTextureCommand,
        string cdbName, TileArchivedTexture tileArchivedTexture, Stream content,
        CancellationToken cancellationToken)
    {
        insertIntoTileArchivedTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedTextureParameters(insertIntoTileArchivedTextureCommand, tileArchivedTexture);
        insertIntoTileArchivedTextureCommand.Parameters[ContentParamName].Value = content;

        return insertIntoTileArchivedTextureCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    #endregion

    #region Select

    /// <summary>
    /// The SQL statement to select a row from the TileArchivedTexture table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="LatitudeParamName"/></description></item>
    ///   <item><description><see cref="LongitudeParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><description><see cref="UpParamName"/></description></item>
    ///   <item><description><see cref="RightParamName"/></description></item>
    ///   <item><description><see cref="TextureNameParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// <list type="bullet">
    ///   <listheader><description>Selected Columns</description></listheader>
    ///   <item><description><see cref="ContentColumnName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string SelectFromTileArchivedTextureStatement
    {
        get;
    }

    internal void InitializeSelectFromTileArchivedTextureCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = SelectFromTileArchivedTextureStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachTileArchivedTextureParameters(dbCommand);
    }

    /// <inheritdoc/>
    public bool TrySelectFromTileArchivedTexture(string cdbName, TileArchivedTexture tileArchivedTexture,
        Action<Stream> fileFoundAction)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand selectFromTileArchivedTextureCommand = dbConnection.CreateCommand();
        InitializeSelectFromTileArchivedTextureCommand(selectFromTileArchivedTextureCommand);
        selectFromTileArchivedTextureCommand.Prepare();

        return TrySelectFromTileArchivedTexture(selectFromTileArchivedTextureCommand, cdbName, tileArchivedTexture, fileFoundAction);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromTileArchivedTexture(string, TileArchivedTexture, Action{Stream})"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTileArchivedTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual bool TrySelectFromTileArchivedTexture(DbCommand selectFromTileArchivedTextureCommand,
        string cdbName, TileArchivedTexture tileArchivedTexture,
        Action<Stream> fileFoundAction)
    {
        selectFromTileArchivedTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedTextureParameters(selectFromTileArchivedTextureCommand, tileArchivedTexture);

        using DbDataReader dbDataReader = selectFromTileArchivedTextureCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        do
        {
            while (dbDataReader.Read())
            {
                using Stream stream = dbDataReader.GetStream(ContentColumnName);
                fileFoundAction(stream);
                return true;
            }
        } while (dbDataReader.NextResult());
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> TrySelectFromTileArchivedTextureAsync(string cdbName, TileArchivedTexture tileArchivedTexture,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand selectFromTileArchivedTextureCommand = dbConnection.CreateCommand();
        InitializeSelectFromTileArchivedTextureCommand(selectFromTileArchivedTextureCommand);
        await selectFromTileArchivedTextureCommand.PrepareAsync(cancellationToken);

        return await TrySelectFromTileArchivedTextureAsync(selectFromTileArchivedTextureCommand, cdbName, tileArchivedTexture,
            fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromTileArchivedTextureAsync(string, TileArchivedTexture, Func{Stream, CancellationToken, Task}, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTileArchivedTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<bool> TrySelectFromTileArchivedTextureAsync(DbCommand selectFromTileArchivedTextureCommand,
        string cdbName, TileArchivedTexture tileArchivedTexture,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        selectFromTileArchivedTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedTextureParameters(selectFromTileArchivedTextureCommand, tileArchivedTexture);

        await using DbDataReader dbDataReader = await selectFromTileArchivedTextureCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        do
        {
            while (await dbDataReader.ReadAsync(cancellationToken))
            {
                await using Stream stream = dbDataReader.GetStream(ContentColumnName);
                await fileFoundAsyncAction(stream, cancellationToken);
                return true;
            }
        } while (await dbDataReader.NextResultAsync(cancellationToken));
        return false;
    }

    /// <inheritdoc/>
    public Stream? SelectFromTileArchivedTexture(string cdbName, TileArchivedTexture tileArchivedTexture)
    {
        DbConnection dbConnection = dbDataSource.OpenConnection();
        try
        {
            DbCommand selectFromTileArchivedTextureCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromTileArchivedTextureCommand(selectFromTileArchivedTextureCommand);
                selectFromTileArchivedTextureCommand.Prepare();

                Stream? stream = SelectFromTileArchivedTexture(selectFromTileArchivedTextureCommand, cdbName, tileArchivedTexture);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromTileArchivedTextureCommand, dbConnection);
                }
                else
                {
                    selectFromTileArchivedTextureCommand.Dispose();
                    dbConnection.Dispose();
                    return null;
                }
            }
            catch (Exception)
            {
                selectFromTileArchivedTextureCommand.Dispose();
                throw;
            }
        }
        catch (Exception)
        {
            dbConnection.Dispose();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromTileArchivedTexture(string, TileArchivedTexture)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTileArchivedTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Stream? SelectFromTileArchivedTexture(DbCommand selectFromTileArchivedTextureCommand,
        string cdbName, TileArchivedTexture tileArchivedTexture)
    {
        selectFromTileArchivedTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedTextureParameters(selectFromTileArchivedTextureCommand, tileArchivedTexture);

        DbDataReader dbDataReader = selectFromTileArchivedTextureCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        try
        {
            do
            {
                while (dbDataReader.Read())
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (dbDataReader.NextResult());
            dbDataReader.Dispose();
            return null;
        }
        catch (Exception)
        {
            dbDataReader.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Stream?> SelectFromTileArchivedTextureAsync(string cdbName, TileArchivedTexture tileArchivedTexture,
        CancellationToken cancellationToken)
    {
        DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            DbCommand selectFromTileArchivedTextureCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromTileArchivedTextureCommand(selectFromTileArchivedTextureCommand);
                await selectFromTileArchivedTextureCommand.PrepareAsync(cancellationToken);

                Stream? stream = await SelectFromTileArchivedTextureAsync(selectFromTileArchivedTextureCommand, cdbName, tileArchivedTexture, cancellationToken);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromTileArchivedTextureCommand, dbConnection);
                }
                else
                {
                    await selectFromTileArchivedTextureCommand.DisposeAsync();
                    await dbConnection.DisposeAsync();
                    return null;
                }
            }
            catch (Exception)
            {
                await selectFromTileArchivedTextureCommand.DisposeAsync();
                throw;
            }
        }
        catch (Exception)
        {
            await dbConnection.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromTileArchivedTextureAsync(string, TileArchivedTexture, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromTileArchivedTextureCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<Stream?> SelectFromTileArchivedTextureAsync(DbCommand selectFromTileArchivedTextureCommand,
        string cdbName, TileArchivedTexture tileArchivedTexture,
        CancellationToken cancellationToken)
    {
        selectFromTileArchivedTextureCommand.Parameters[CdbParamName].Value = cdbName;
        SetTileArchivedTextureParameters(selectFromTileArchivedTextureCommand, tileArchivedTexture);

        DbDataReader dbDataReader = await selectFromTileArchivedTextureCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        try
        {
            do
            {
                while (await dbDataReader.ReadAsync(cancellationToken))
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (await dbDataReader.NextResultAsync(cancellationToken));
            await dbDataReader.DisposeAsync();
            return null;
        }
        catch (Exception)
        {
            await dbDataReader.DisposeAsync();
            throw;
        }
    }

    #endregion

    #endregion

    #region Navigation

    /// <summary>
    /// The SQL DDL statement to create the Navigation table.
    /// </summary>
    protected abstract string CreateTableNavigationStatement
    {
        get;
    }

    #region Insert

    /// <summary>
    /// The SQL statement to insert a row into the Navigation table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    ///   <item><description><see cref="ContentParamName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string InsertIntoNavigationStatement
    {
        get;
    }

    internal void InitializeInsertIntoNavigationCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = InsertIntoNavigationStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachNavigationParameters(dbCommand);
        CreateAndAttachParameter(dbCommand, ContentParamName, DbType.Binary);
    }

    private void CreateAndAttachNavigationParameters(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, DatasetParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector1ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, ComponentSelector2ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, FileTypeParamName, DbType.String);
    }

    private void SetNavigationParameters(DbCommand dbCommand, Navigation navigation)
    {
        dbCommand.Parameters[DatasetParamName].Value = navigation.Dataset.Value;
        dbCommand.Parameters[ComponentSelector1ParamName].Value = navigation.ComponentSelector1;
        dbCommand.Parameters[ComponentSelector2ParamName].Value = navigation.ComponentSelector2;
        dbCommand.Parameters[FileTypeParamName].Value = navigation.FileType;
    }

    /// <inheritdoc/>
    public int InsertIntoNavigation(string cdbName, Navigation navigation, byte[] content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoNavigationCommand = dbConnection.CreateCommand();
        InitializeInsertIntoNavigationCommand(insertIntoNavigationCommand);
        insertIntoNavigationCommand.Prepare();

        return InsertIntoNavigation(insertIntoNavigationCommand, cdbName, navigation, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoNavigation(string, Navigation, byte[])"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoNavigationCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoNavigation(DbCommand insertIntoNavigationCommand,
        string cdbName, Navigation navigation, byte[] content)
    {
        insertIntoNavigationCommand.Parameters[CdbParamName].Value = cdbName;
        SetNavigationParameters(insertIntoNavigationCommand, navigation);
        insertIntoNavigationCommand.Parameters[ContentParamName].Value = content;

        return insertIntoNavigationCommand.ExecuteNonQuery();
    }


    /// <inheritdoc/>
    public int InsertIntoNavigation(string cdbName, Navigation navigation, Stream content)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoNavigationCommand = dbConnection.CreateCommand();
        InitializeInsertIntoNavigationCommand(insertIntoNavigationCommand);
        insertIntoNavigationCommand.Prepare();

        return InsertIntoNavigation(insertIntoNavigationCommand, cdbName, navigation, content);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoNavigation(string, Navigation, Stream)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoNavigationCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual int InsertIntoNavigation(DbCommand insertIntoNavigationCommand, 
        string cdbName, Navigation navigation, Stream content)
    {
        insertIntoNavigationCommand.Parameters[CdbParamName].Value = cdbName;
        SetNavigationParameters(insertIntoNavigationCommand, navigation);
        insertIntoNavigationCommand.Parameters[ContentParamName].Value = content;

        return insertIntoNavigationCommand.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoNavigationAsync(string cdbName, Navigation navigation, byte[] content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoNavigationCommand = dbConnection.CreateCommand();
        InitializeInsertIntoNavigationCommand(insertIntoNavigationCommand);
        await insertIntoNavigationCommand.PrepareAsync(cancellationToken);

        return await InsertIntoNavigationAsync(insertIntoNavigationCommand, cdbName, navigation, content, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoNavigationAsync(string, Navigation, byte[], CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoNavigationCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoNavigationAsync(DbCommand insertIntoNavigationCommand,
        string cdbName, Navigation navigation, byte[] content,
        CancellationToken cancellationToken)
    {
        insertIntoNavigationCommand.Parameters[CdbParamName].Value = cdbName;
        SetNavigationParameters(insertIntoNavigationCommand, navigation);
        insertIntoNavigationCommand.Parameters[ContentParamName].Value = content;

        return insertIntoNavigationCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> InsertIntoNavigationAsync(string cdbName, Navigation navigation, Stream content,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoNavigationCommand = dbConnection.CreateCommand();
        InitializeInsertIntoNavigationCommand(insertIntoNavigationCommand);
        await insertIntoNavigationCommand.PrepareAsync(cancellationToken);

        return await InsertIntoNavigationAsync(insertIntoNavigationCommand, cdbName, navigation, content, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.InsertIntoNavigationAsync(string, Navigation, Stream, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="insertIntoNavigationCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Task<int> InsertIntoNavigationAsync(DbCommand insertIntoNavigationCommand,
        string cdbName, Navigation navigation, Stream content,
        CancellationToken cancellationToken)
    {
        insertIntoNavigationCommand.Parameters[CdbParamName].Value = cdbName;
        SetNavigationParameters(insertIntoNavigationCommand, navigation);
        insertIntoNavigationCommand.Parameters[ContentParamName].Value = content;

        return insertIntoNavigationCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    #endregion

    #region Select

    /// <summary>
    /// The SQL statement to select a row from the Navigation table.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><description>Parameters</description></listheader>
    ///   <item><description><see cref="CdbParamName"/></description></item>
    ///   <item><description><see cref="DatasetParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// <list type="bullet">
    ///   <listheader><description>Selected Columns</description></listheader>
    ///   <item><description><see cref="ContentColumnName"/></description></item>
    /// </list>
    /// </remarks>
    protected abstract string SelectFromNavigationStatement
    {
        get;
    }

    internal void InitializeSelectFromNavigationCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = SelectFromNavigationStatement;
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
        CreateAndAttachNavigationParameters(dbCommand);
    }

    /// <inheritdoc/>
    public bool TrySelectFromNavigation(string cdbName, Navigation navigation,
        Action<Stream> fileFoundAction)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand selectFromNavigationCommand = dbConnection.CreateCommand();
        InitializeSelectFromNavigationCommand(selectFromNavigationCommand);
        selectFromNavigationCommand.Prepare();

        return TrySelectFromNavigation(selectFromNavigationCommand, cdbName, navigation, fileFoundAction);
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromNavigation(string, Navigation)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromNavigationCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual bool TrySelectFromNavigation(DbCommand selectFromNavigationCommand,
        string cdbName, Navigation navigation,
        Action<Stream> fileFoundAction)
    {
        selectFromNavigationCommand.Parameters[CdbParamName].Value = cdbName;
        SetNavigationParameters(selectFromNavigationCommand, navigation);

        using DbDataReader dbDataReader = selectFromNavigationCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        do
        {
            while (dbDataReader.Read())
            {
                using Stream stream = dbDataReader.GetStream(ContentColumnName);
                fileFoundAction(stream);
                return true;
            }
        } while (dbDataReader.NextResult());
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> TrySelectFromNavigationAsync(string cdbName, Navigation navigation,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand selectFromNavigationCommand = dbConnection.CreateCommand();
        InitializeSelectFromNavigationCommand(selectFromNavigationCommand);
        await selectFromNavigationCommand.PrepareAsync(cancellationToken);

        return await TrySelectFromNavigationAsync(selectFromNavigationCommand, cdbName, navigation, fileFoundAsyncAction, cancellationToken);
    }

    /// <inheritdoc cref="ISQLDataStore.TrySelectFromNavigationAsync(string, Navigation, Func{Stream, CancellationToken, Task}, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromNavigationCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<bool> TrySelectFromNavigationAsync(DbCommand selectFromNavigationCommand,
        string cdbName, Navigation navigation,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken)
    {
        selectFromNavigationCommand.Parameters[CdbParamName].Value = cdbName;
        SetNavigationParameters(selectFromNavigationCommand, navigation);

        await using DbDataReader dbDataReader = await selectFromNavigationCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        do
        {
            while (await dbDataReader.ReadAsync(cancellationToken))
            {
                await using Stream stream = dbDataReader.GetStream(ContentColumnName);
                await fileFoundAsyncAction(stream, cancellationToken);
                return true;
            }
        } while (await dbDataReader.NextResultAsync(cancellationToken));
        return false;
    }

    /// <inheritdoc/>
    public Stream? SelectFromNavigation(string cdbName, Navigation navigation)
    {
        DbConnection dbConnection = dbDataSource.OpenConnection();
        try
        {
            DbCommand selectFromNavigationCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromNavigationCommand(selectFromNavigationCommand);
                selectFromNavigationCommand.Prepare();

                Stream? stream = SelectFromNavigation(selectFromNavigationCommand, cdbName, navigation);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromNavigationCommand, dbConnection);
                }
                else
                {
                    selectFromNavigationCommand.Dispose();
                    dbConnection.Dispose();
                    return null;
                }
            }
            catch (Exception)
            {
                selectFromNavigationCommand.Dispose();
                throw;
            }
        }
        catch (Exception)
        {
            dbConnection.Dispose();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromNavigation(string, Navigation)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromNavigationCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual Stream? SelectFromNavigation(DbCommand selectFromNavigationCommand,
        string cdbName, Navigation navigation)
    {
        selectFromNavigationCommand.Parameters[CdbParamName].Value = cdbName;
        SetNavigationParameters(selectFromNavigationCommand, navigation);

        DbDataReader dbDataReader = selectFromNavigationCommand.ExecuteReader(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
        try
        {
            do
            {
                while (dbDataReader.Read())
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (dbDataReader.NextResult());
            dbDataReader.Dispose();
            return null;
        }
        catch (Exception)
        {
            dbDataReader.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Stream?> SelectFromNavigationAsync(string cdbName, Navigation navigation,
        CancellationToken cancellationToken)
    {
        DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            DbCommand selectFromNavigationCommand = dbConnection.CreateCommand();
            try
            {
                InitializeSelectFromNavigationCommand(selectFromNavigationCommand);
                await selectFromNavigationCommand.PrepareAsync(cancellationToken);

                Stream? stream = await SelectFromNavigationAsync(selectFromNavigationCommand, cdbName, navigation, cancellationToken);
                if (stream is not null)
                {
                    return new WrappedStream(stream, selectFromNavigationCommand, dbConnection);
                }
                else
                {
                    await selectFromNavigationCommand.DisposeAsync();
                    await dbConnection.DisposeAsync();
                    return null;
                }
            }
            catch (Exception)
            {
                await selectFromNavigationCommand.DisposeAsync();
                throw;
            }
        }
        catch (Exception)
        {
            await dbConnection.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc cref="ISQLDataStore.SelectFromNavigationAsync(string, Navigation, CancellationToken)"/>
    /// <remarks>
    /// <para>
    /// This is what subclasses should override to customize behavior.
    /// </para>
    /// </remarks>
    /// <param name="selectFromNavigationCommand">The prepared statement to use to execute the query.</param>
    protected internal virtual async Task<Stream?> SelectFromNavigationAsync(DbCommand selectFromNavigationCommand,
        string cdbName, Navigation navigation,
        CancellationToken cancellationToken)
    {
        selectFromNavigationCommand.Parameters[CdbParamName].Value = cdbName;
        SetNavigationParameters(selectFromNavigationCommand, navigation);

        DbDataReader dbDataReader = await selectFromNavigationCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);
        try
        {
            do
            {
                while (await dbDataReader.ReadAsync(cancellationToken))
                {
                    Stream stream = dbDataReader.GetStream(ContentColumnName);
                    return new WrappedStream(stream, dbDataReader);
                }
            } while (await dbDataReader.NextResultAsync(cancellationToken));
            await dbDataReader.DisposeAsync();
            return null;
        }
        catch (Exception)
        {
            await dbDataReader.DisposeAsync();
            throw;
        }
    }

    #endregion

    #endregion

    /// <summary>
    /// Dumps the raw SQL statements that the data store uses.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All of them.
    /// This is a complete list of every statement that the class will ever
    /// execute against the database.
    /// </para>
    /// </remarks>
    /// <param name="textWriter">The text writer to dump the statements into.</param>
    public void DumpStatements(TextWriter textWriter)
    {
        textWriter.Write(CreateTableCDBStatement);
        textWriter.WriteLine(';');
        textWriter.Write(InsertIntoCDBStatement);
        textWriter.WriteLine(';');
        textWriter.Write(SelectFromCDBStatement);
        textWriter.WriteLine(';');
        textWriter.WriteLine();
        textWriter.Write(CreateTableMetadataStatement);
        textWriter.WriteLine(';');
        textWriter.Write(InsertIntoMetadataStatement);
        textWriter.WriteLine(';');
        textWriter.Write(SelectFromMetadataStatement);
        textWriter.WriteLine(';');
        textWriter.WriteLine();
        textWriter.Write(CreateTableTextureStatement);
        textWriter.WriteLine(';');
        textWriter.Write(InsertIntoTextureStatement);
        textWriter.WriteLine(';');
        textWriter.Write(SelectFromTextureStatement);
        textWriter.WriteLine(';');
        textWriter.WriteLine();
        textWriter.Write(CreateTableTextureLodStatement);
        textWriter.WriteLine(';');
        textWriter.Write(InsertIntoTextureLodStatement);
        textWriter.WriteLine(';');
        textWriter.Write(SelectFromTextureLodStatement);
        textWriter.WriteLine(';');
        textWriter.WriteLine();
        textWriter.Write(CreateTableGeotypicalModelStatement);
        textWriter.WriteLine(';');
        textWriter.Write(InsertIntoGeotypicalModelStatement);
        textWriter.WriteLine(';');
        textWriter.Write(SelectFromGeotypicalModelStatement);
        textWriter.WriteLine(';');
        textWriter.WriteLine();
        textWriter.Write(CreateTableGeotypicalModelLodStatement);
        textWriter.WriteLine(';');
        textWriter.Write(InsertIntoGeotypicalModelLodStatement);
        textWriter.WriteLine(';');
        textWriter.Write(SelectFromGeotypicalModelLodStatement);
        textWriter.WriteLine(';');
        textWriter.WriteLine();
        textWriter.Write(CreateTableMovingModelStatement);
        textWriter.WriteLine(';');
        textWriter.Write(InsertIntoMovingModelStatement);
        textWriter.WriteLine(';');
        textWriter.Write(SelectFromMovingModelStatement);
        textWriter.WriteLine(';');
        textWriter.WriteLine();
        textWriter.Write(CreateTableMovingModelLodStatement);
        textWriter.WriteLine(';');
        textWriter.Write(InsertIntoMovingModelLodStatement);
        textWriter.WriteLine(';');
        textWriter.Write(SelectFromMovingModelLodStatement);
        textWriter.WriteLine(';');
        textWriter.WriteLine();
        textWriter.Write(CreateTableTileStatement);
        textWriter.WriteLine(';');
        textWriter.Write(InsertIntoTileStatement);
        textWriter.WriteLine(';');
        textWriter.Write(SelectFromTileStatement);
        textWriter.WriteLine(';');
        textWriter.WriteLine();
        textWriter.Write(CreateTableTileArchivedFeatureStatement);
        textWriter.WriteLine(';');
        textWriter.Write(InsertIntoTileArchivedFeatureStatement);
        textWriter.WriteLine(';');
        textWriter.Write(SelectFromTileArchivedFeatureStatement);
        textWriter.WriteLine(';');
        textWriter.WriteLine();
        textWriter.Write(CreateTableTileArchivedTextureStatement);
        textWriter.WriteLine(';');
        textWriter.Write(InsertIntoTileArchivedTextureStatement);
        textWriter.WriteLine(';');
        textWriter.Write(SelectFromTileArchivedTextureStatement);
        textWriter.WriteLine(';');
        textWriter.WriteLine();
        textWriter.Write(CreateTableNavigationStatement);
        textWriter.WriteLine(';');
        textWriter.Write(InsertIntoNavigationStatement);
        textWriter.WriteLine(';');
        textWriter.Write(SelectFromNavigationStatement);
        textWriter.WriteLine(';');
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
    protected virtual ValueTask DisposeAsyncCore()
    {
        return ValueTask.CompletedTask;
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

#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
