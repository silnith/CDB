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
public abstract class SQLCDB : ICDB
{
    /// <summary>
    /// A generic accessor that provides common boilerplate code for the most
    /// common access scenarios.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All of the tables emulating filesystem storage of CDB files follow the
    /// same pattern.  The first column is the identifier for the CDB.  The
    /// last column before the file contents is the file type (extension).  In
    /// between are all the parameters extracted from the file name.
    /// </para>
    /// <para>
    /// The most common access patterns are reading a file and writing a file.
    /// Both of these involve taking all of the parameters from the filename
    /// and either inserting or retrieving the file contents.  The code to call
    /// the select or insert statement is the same across all the tables, the
    /// only difference is how to map the file identifier to the columns.
    /// </para>
    /// <para>
    /// This provides that common code, and abstract methods for mapping the
    /// specific identifier to the columns of the specific table.  Subclasses
    /// can provide these mappings and reuse the code that handles the database
    /// access.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of identifier that holds all the data
    /// necessary to uniquely identify a row in the specific table for that
    /// file type.</typeparam>
    internal abstract class TableAccessor<T>
    {
        protected readonly SQLCDB sqlCDB;

        private readonly string selectStatement;

        private readonly string insertStatement;

        internal TableAccessor(SQLCDB sqlCDB, string selectStatement, string insertStatement)
        {
            this.sqlCDB = sqlCDB;
            this.selectStatement = selectStatement;
            this.insertStatement = insertStatement;
        }

        /// <summary>
        /// Creates the parameters appropriate for an object of type
        /// <typeparamref name="T"/> and attaches them to the prepared statement.
        /// </summary>
        /// <param name="dbCommand">The command to create the parameters for.</param>
        internal abstract void CreateAndAttachObjectParameter(DbCommand dbCommand);

        /// <summary>
        /// Sets the parameters according to the members of an object of type
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dbCommand">The prepared statement to set the parameter for.</param>
        /// <param name="obj">The object from which to get all the parameter values.</param>
        internal abstract void SetObjectParameters(DbCommand dbCommand, T obj);

        /// <summary>
        /// Initializes a prepared statement to be a <c>select</c> query for
        /// the appropriate table with parameters matching all the columns that
        /// make up the composite primary key.
        /// </summary>
        /// <param name="dbCommand">The command to initialize.</param>
        internal void InitializeSelectCommand(DbCommand dbCommand)
        {
            dbCommand.CommandText = selectStatement;
            sqlCDB.CreateAndAttachCdbParameter(dbCommand);
            CreateAndAttachObjectParameter(dbCommand);
        }

        /// <summary>
        /// Executes a <c>select</c> query using the provided prepared statement
        /// and input.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This takes the prepared statement as a parameter to support multiple
        /// use cases.  Typical calls will get a database connection to read a
        /// single file, disposing afterwards.  Bulk import scenarios will want
        /// to maintain a persistent connection and provide transaction support.
        /// </para>
        /// <para>
        /// The prepared statement must have been initialized by <see cref="InitializeSelectCommand(DbCommand)"/>.
        /// </para>
        /// </remarks>
        /// <param name="dbCommand">The prepared statement.</param>
        /// <param name="obj">The object containing all the necessary members for uniquely identifying a row.</param>
        /// <returns>The contents of the <see cref="ContentColumnName"/> column, or <see langword="null"/>.</returns>
        internal Stream? SelectUsingPreparedStatement(DbCommand dbCommand, T obj)
        {
            sqlCDB.SetCdbParameter(dbCommand);
            SetObjectParameters(dbCommand, obj);

            DbDataReader dbDataReader = dbCommand.ExecuteReader(
                CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
            try
            {
                do
                {
                    while (dbDataReader.Read())
                    {
                        Stream stream = dbDataReader.GetStream(sqlCDB.ContentColumnName);
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

        /// <summary>
        /// Executes a select query using the provided prepared statement and input.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This takes the prepared statement as a parameter to support multiple
        /// use cases.  Typical calls will get a database connection to read a
        /// single file, disposing afterwards.  Bulk import scenarios will want
        /// to maintain a persistent connection and provide transaction support.
        /// </para>
        /// <para>
        /// The prepared statement must have been initialized by <see cref="InitializeSelectCommand(DbCommand)"/>.
        /// </para>
        /// </remarks>
        /// <param name="dbCommand">The prepared statement.</param>
        /// <param name="obj">The object containing all the necessary members for uniquely identifying a row.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The contents of the <see cref="ContentColumnName"/> column, or <see langword="null"/>.</returns>
        internal async Task<Stream?> SelectUsingPreparedStatementAsync(DbCommand dbCommand, T obj, CancellationToken cancellationToken = default)
        {
            sqlCDB.SetCdbParameter(dbCommand);
            SetObjectParameters(dbCommand, obj);

            DbDataReader dbDataReader = await dbCommand.ExecuteReaderAsync(
                CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow,
                cancellationToken);
            try
            {
                do
                {
                    while (await dbDataReader.ReadAsync(cancellationToken))
                    {
                        Stream stream = dbDataReader.GetStream(sqlCDB.ContentColumnName);
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

        /// <summary>
        /// Opens a new database connection and calls <see cref="SelectUsingPreparedStatement(DbCommand, T)"/>.
        /// </summary>
        /// <param name="obj">The object containing all the necessary members for uniquely identifying a row.</param>
        /// <returns>The contents of the <see cref="ContentColumnName"/> column, or <see langword="null"/>.</returns>
        internal Stream? SelectUsingNewConnection(T obj)
        {
            DbConnection dbConnection = sqlCDB.dbDataSource.OpenConnection();
            try
            {
                DbCommand dbCommand = dbConnection.CreateCommand();
                try
                {
                    InitializeSelectCommand(dbCommand);
                    dbCommand.Prepare();

                    Stream? stream = SelectUsingPreparedStatement(dbCommand, obj);
                    if (stream is not null)
                    {
                        return new WrappedStream(stream, dbCommand, dbConnection);
                    }
                    else
                    {
                        dbCommand.Dispose();
                        dbConnection.Dispose();
                        return null;
                    }
                }
                catch (Exception)
                {
                    dbCommand.Dispose();
                    throw;
                }
            }
            catch (Exception)
            {
                dbConnection.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Opens a new database connection and calls <see cref="SelectUsingPreparedStatementAsync(DbCommand, T, CancellationToken)"/>.
        /// </summary>
        /// <param name="obj">The object containing all the necessary members for uniquely identifying a row.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The contents of the <see cref="ContentColumnName"/> column, or <see langword="null"/>.</returns>
        internal async Task<Stream?> SelectUsingNewConnectionAsync(T obj, CancellationToken cancellationToken = default)
        {
            DbConnection dbConnection = await sqlCDB.dbDataSource.OpenConnectionAsync(cancellationToken);
            try
            {
                DbCommand dbCommand = dbConnection.CreateCommand();
                try
                {
                    InitializeSelectCommand(dbCommand);
                    await dbCommand.PrepareAsync(cancellationToken);

                    Stream? stream = await SelectUsingPreparedStatementAsync(dbCommand, obj, cancellationToken);
                    if (stream is not null)
                    {
                        return new WrappedStream(stream, dbCommand, dbConnection);
                    }
                    else
                    {
                        await dbCommand.DisposeAsync();
                        await dbConnection.DisposeAsync();
                        return null;
                    }
                }
                catch (Exception)
                {
                    await dbCommand.DisposeAsync();
                    throw;
                }
            }
            catch (Exception)
            {
                await dbConnection.DisposeAsync();
                throw;
            }
        }

        /// <summary>
        /// Initializes a prepared statement to be an <c>insert</c> command for
        /// the appropriate table with parameters matching all the columns that
        /// make up the composite primary key.
        /// </summary>
        /// <param name="dbCommand">The command to initialize.</param>
        internal void InitializeInsertCommand(DbCommand dbCommand)
        {
            dbCommand.CommandText = insertStatement;
            sqlCDB.CreateAndAttachCdbParameter(dbCommand);
            CreateAndAttachObjectParameter(dbCommand);
            sqlCDB.CreateAndAttachContentParameter(dbCommand);
        }

        /// <summary>
        /// Executes an insert command using the provided prepared statement and input.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This takes the prepared statement as a parameter to support multiple
        /// use cases.  Typical calls will get a database connection to read a
        /// single file, disposing afterwards.  Bulk import scenarios will want
        /// to maintain a persistent connection and provide transaction support.
        /// </para>
        /// <para>
        /// The prepared statement must have been initialized by <see cref="InitializeInsertCommand(DbCommand)"/>.
        /// </para>
        /// </remarks>
        /// <param name="dbCommand">The prepared statement.</param>
        /// <param name="obj">The object containing all the necessary members for uniquely identifying a row.</param>
        /// <param name="content">The file contents.</param>
        internal void InsertUsingPreparedStatement(DbCommand dbCommand, T obj, Stream content)
        {
            sqlCDB.SetCdbParameter(dbCommand);
            SetObjectParameters(dbCommand, obj);
            sqlCDB.SetContentParameter(dbCommand, content);

            dbCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes an insert command using the provided prepared statement and input.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This takes the prepared statement as a parameter to support multiple
        /// use cases.  Typical calls will get a database connection to read a
        /// single file, disposing afterwards.  Bulk import scenarios will want
        /// to maintain a persistent connection and provide transaction support.
        /// </para>
        /// <para>
        /// The prepared statement must have been initialized by <see cref="InitializeInsertCommand(DbCommand)"/>.
        /// </para>
        /// </remarks>
        /// <param name="dbCommand">The prepared statement.</param>
        /// <param name="obj">The object containing all the necessary members for uniquely identifying a row.</param>
        /// <param name="content">The file contents.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        internal Task InsertUsingPreparedStatementAsync(DbCommand dbCommand, T obj, Stream content, CancellationToken cancellationToken = default)
        {
            sqlCDB.SetCdbParameter(dbCommand);
            SetObjectParameters(dbCommand, obj);
            sqlCDB.SetContentParameter(dbCommand, content);

            return dbCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Opens a new database connection and calls <see cref="InsertUsingPreparedStatement(DbCommand, T, Stream)"/>.
        /// </summary>
        /// <param name="obj">The object containing all the necessary members for uniquely identifying a row.</param>
        /// <param name="content">The file contents.</param>
        internal void InsertUsingNewConnection(T obj, Stream content)
        {
            using DbConnection dbConnection = sqlCDB.dbDataSource.OpenConnection();
            using DbCommand dbCommand = dbConnection.CreateCommand();
            InitializeInsertCommand(dbCommand);
            dbCommand.Prepare();

            InsertUsingPreparedStatement(dbCommand, obj, content);
        }

        /// <summary>
        /// Opens a new database connection and calls <see cref="InsertUsingPreparedStatement(DbCommand, T, Stream)"/>.
        /// </summary>
        /// <param name="obj">The object containing all the necessary members for uniquely identifying a row.</param>
        /// <param name="content">The file contents.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        internal async Task InsertUsingNewConnectionAsync(T obj, Stream content, CancellationToken cancellationToken = default)
        {
            await using DbConnection dbConnection = await sqlCDB.dbDataSource.OpenConnectionAsync(cancellationToken);
            await using DbCommand dbCommand = dbConnection.CreateCommand();
            InitializeInsertCommand(dbCommand);
            await dbCommand.PrepareAsync(cancellationToken);

            await InsertUsingPreparedStatementAsync(dbCommand, obj, content, cancellationToken);
        }

    }

    /// <summary>
    /// Creates a parameter for a database command, sets the name and type of
    /// the parameter, and attaches the parameter to the command.
    /// </summary>
    /// <param name="dbCommand">The command to create a parameter for.</param>
    /// <param name="dbParameterName">The name of the parameter.
    /// Each database system has its own syntax for how these parameters should
    /// be named.</param>
    /// <param name="dbType">The parameter type.</param>
    internal static void CreateAndAttachParameter(DbCommand dbCommand, string dbParameterName, DbType dbType)
    {
        DbParameter dbParameter = dbCommand.CreateParameter();
        dbCommand.Parameters.Add(dbParameter);
        dbParameter.DbType = dbType;
        dbParameter.ParameterName = dbParameterName;
    }

    internal readonly DbDataSource dbDataSource;

    internal readonly TableAccessor<Metadata> metadataAccessor;
    internal readonly TableAccessor<Texture> textureAccessor;
    internal readonly TableAccessor<TextureLod> textureLodAccessor;
    internal readonly TableAccessor<GeotypicalModel> geotypicalModelAccessor;
    internal readonly TableAccessor<GeotypicalModelLod> geotypicalModelLodAccessor;
    internal readonly TableAccessor<MovingModel> movingModelAccessor;
    internal readonly TableAccessor<MovingModelLod> movingModelLodAccessor;
    internal readonly TableAccessor<Tile> tileAccessor;
    internal readonly TableAccessor<TileArchivedFeature> tileFeatureAccessor;
    internal readonly TableAccessor<TileArchivedTexture> tileTextureAccessor;
    internal readonly TableAccessor<Navigation> navigationAccessor;

    /// <summary>
    /// Creates a new CDB storage backend using the provided SQL data source.
    /// </summary>
    /// <param name="dbDataSource">The data source.</param>
    /// <param name="options">Configurable settings.</param>
    protected SQLCDB(DbDataSource dbDataSource, IOptions<SQLCDBSettings> options)
    {
        ArgumentNullException.ThrowIfNull(dbDataSource);
        ArgumentNullException.ThrowIfNull(options);

        this.dbDataSource = dbDataSource;
        Name = options.Value.Name;

        if (options.Value.CreateSchema)
        {
            CreateSchema();
            InsertIntoCDB(Name);
        }

        metadataAccessor = new MetadataTableAccessor(this);
        textureAccessor = new TextureTableAccessor(this);
        textureLodAccessor = new TextureLodTableAccessor(this);
        geotypicalModelAccessor = new GeotypicalModelTableAccessor(this);
        geotypicalModelLodAccessor = new GeotypicalModelLodTableAccessor(this);
        movingModelAccessor = new MovingModelTableAccessor(this);
        movingModelLodAccessor = new MovingModelLodTableAccessor(this);
        tileAccessor = new TileTableAccessor(this);
        tileFeatureAccessor = new TileArchivedFeatureTableAccessor(this);
        tileTextureAccessor = new TileArchivedTextureTableAccessor(this);
        navigationAccessor = new NavigationTableAccessor(this);
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

    /// <summary>
    /// Returns a CDB accessor that can be used to batch writes (and reads)
    /// into transactions.
    /// </summary>
    /// <returns>A connection to the CDB that supports transactional behavior.</returns>
    public PersistentConnection GetPersistentConnection()
    {
        return new PersistentConnection(this);
    }

    /// <summary>
    /// Creates all of the tables, constraints, and foreign key relationships.
    /// </summary>
    private void CreateSchema()
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();

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

        foreach (string createIndexStatement in CreateIndexStatements)
        {
            dbCommand.CommandText = createIndexStatement;
            _ = dbCommand.ExecuteNonQuery();
        }

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

    /// <summary>
    /// Creates the <see cref="CdbParamName"/> parameter and attaches it to the
    /// prepared statement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The CDB name is a foreign key in every table, so this parameter is used
    /// everywhere.
    /// </para>
    /// </remarks>
    /// <param name="dbCommand">The command to create the parameter for.</param>
    /// <seealso cref="SetCdbParameter(DbCommand)"/>
    private void CreateAndAttachCdbParameter(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, CdbParamName, DbType.String);
    }

    /// <summary>
    /// Sets the <see cref="CdbParamName"/> parameter to <see cref="Name"/> for
    /// the prepared statement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The database schema is designed to allow multiple CDBs to be stored in
    /// a single database, differentiated by name.  The <see cref="ICDB"/>
    /// interface is intended to represent one distinct CDB.  Multiple instances
    /// of this class could be instantiated using the same data source, but with
    /// different values for <see cref="Name"/>.
    /// </para>
    /// </remarks>
    /// <param name="dbCommand">The prepared statement to set the parameter for.</param>
    /// <seealso cref="CreateAndAttachCdbParameter(DbCommand)"/>
    private void SetCdbParameter(DbCommand dbCommand)
    {
        dbCommand.Parameters[CdbParamName].Value = Name;
    }

    /// <summary>
    /// Creates the <see cref="ContentParamName"/> parameter and attaches it to
    /// the prepared statement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is for the actual file contents.  The binary large object (blob).
    /// </para>
    /// </remarks>
    /// <param name="dbCommand">The command to create the parameter for.</param>
    /// <seealso cref="SetContentParameter(DbCommand, Stream)"/>
    private void CreateAndAttachContentParameter(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, ContentParamName, DbType.Binary);
    }

    /// <summary>
    /// Sets the parameter that contains the file contents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is virtual because not all database systems accept the same input
    /// types, so specific implementations may need to convert the stream into
    /// something else.
    /// </para>
    /// </remarks>
    /// <param name="dbCommand">The prepared statement to set the parameter for.</param>
    /// <param name="content">The file contents.</param>
    /// <seealso cref="CreateAndAttachContentParameter(DbCommand)"/>
    protected virtual void SetContentParameter(DbCommand dbCommand, Stream content)
    {
        dbCommand.Parameters[ContentParamName].Value = content;
    }

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

    /// <summary>
    /// Initializes a prepared statement to be a <see cref="SelectFromCDBStatement"/>.
    /// </summary>
    /// <param name="dbCommand">The command to initialize.</param>
    internal void InitializeSelectFromCDBCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = SelectFromCDBStatement;
    }

    /// <summary>
    /// Returns all CDB data store names in the database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is not part of the CDB API.  It is a meta utility for managing the
    /// data store and its possible multi-tenancy.
    /// </para>
    /// </remarks>
    /// <returns>All the names of the distinct CDB data stores in the database.</returns>
    public IEnumerable<string> SelectFromCDB()
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand selectFromCDBCommand = dbConnection.CreateCommand();
        InitializeSelectFromCDBCommand(selectFromCDBCommand);
        selectFromCDBCommand.Prepare();

        return SelectFromCDB(selectFromCDBCommand);
    }

    /// <inheritdoc cref="SelectFromCDB()"/>
    /// <param name="selectFromCDBCommand">The prepared statement to use to execute the query.</param>
    internal IEnumerable<string> SelectFromCDB(DbCommand selectFromCDBCommand)
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

    /// <summary>
    /// Returns all CDB data store names in the database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is not part of the CDB API.  It is a meta utility for managing the
    /// data store and its possible multi-tenancy.
    /// </para>
    /// </remarks>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>All the names of the distinct CDB data stores in the database.</returns>
    public async IAsyncEnumerable<string> SelectFromCDBAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
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

    /// <inheritdoc cref="SelectFromCDBAsync(CancellationToken)"/>
    /// <param name="selectFromCDBCommand">The prepared statement to use to execute the query.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    internal async IAsyncEnumerable<string> SelectFromCDBAsync(DbCommand selectFromCDBCommand, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using DbDataReader dbDataReader = await selectFromCDBCommand.ExecuteReaderAsync(
            CommandBehavior.SequentialAccess | CommandBehavior.SingleResult,
            cancellationToken);
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

    /// <summary>
    /// Initializes a prepared statement to be an <see cref="InsertIntoCDBStatement"/>.
    /// </summary>
    /// <param name="dbCommand">The command to initialize.</param>
    internal void InitializeInsertIntoCDBCommand(DbCommand dbCommand)
    {
        dbCommand.CommandText = InsertIntoCDBStatement;
        CreateAndAttachCdbParameter(dbCommand);
    }

    /// <summary>
    /// Inserts a name into the table identifying all the unique data stores
    /// contained in the database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is not part of the CDB API.  It is a meta utility for managing the
    /// data store and its possible multi-tenancy.
    /// </para>
    /// </remarks>
    /// <param name="cdbName">The name of a new CDB data store.</param>
    public void InsertIntoCDB(string cdbName)
    {
        using DbConnection dbConnection = dbDataSource.OpenConnection();
        using DbCommand insertIntoCDBCommand = dbConnection.CreateCommand();
        InitializeInsertIntoCDBCommand(insertIntoCDBCommand);
        insertIntoCDBCommand.Prepare();

        InsertIntoCDB(insertIntoCDBCommand, cdbName);
    }

    /// <inheritdoc cref="InsertIntoCDB(string)"/>
    /// <param name="insertIntoCDBCommand">The prepared statement to use to execute the query.</param>
    /// <param name="cdbName">The name of a new CDB data store.</param>
    internal void InsertIntoCDB(DbCommand insertIntoCDBCommand, string cdbName)
    {
        /*
         * This does not call SetCdbParameter because SetCdbParameter is
         * specialized to only use the Name property.  This can set any string.
         */
        insertIntoCDBCommand.Parameters[CdbParamName].Value = cdbName;

        insertIntoCDBCommand.ExecuteNonQuery();
    }

    /// <summary>
    /// Inserts a name into the table identifying all the unique data stores
    /// contained in the database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is not part of the CDB API.  It is a meta utility for managing the
    /// data store and its possible multi-tenancy.
    /// </para>
    /// </remarks>
    /// <param name="cdbName">The name of a new CDB data store.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task InsertIntoCDBAsync(string cdbName, CancellationToken cancellationToken = default)
    {
        await using DbConnection dbConnection = await dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand insertIntoCDBCommand = dbConnection.CreateCommand();
        InitializeInsertIntoCDBCommand(insertIntoCDBCommand);
        await insertIntoCDBCommand.PrepareAsync(cancellationToken);

        await InsertIntoCDBAsync(insertIntoCDBCommand, cdbName, cancellationToken);
    }

    /// <inheritdoc cref="InsertIntoCDBAsync(string, CancellationToken)"/>
    /// <param name="insertIntoCDBCommand">The prepared statement to use to execute the query.</param>
    /// <param name="cdbName">The name of a new CDB data store.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    internal Task InsertIntoCDBAsync(DbCommand insertIntoCDBCommand, string cdbName, CancellationToken cancellationToken = default)
    {
        /*
         * This does not call SetCdbParameter because SetCdbParameter is
         * specialized to only use the Name property.  This can set any string.
         */
        insertIntoCDBCommand.Parameters[CdbParamName].Value = cdbName;

        return insertIntoCDBCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    #endregion

    #endregion

    #region Metadata

    /// <summary>
    /// A table accessor for type <see cref="Metadata"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><term><see cref="Metadata"/> Property</term><description>SQL Statement Parameter</description></listheader>
    ///   <item><term><see cref="Metadata.Name"/></term><description><see cref="MetadataNameParamName"/></description></item>
    ///   <item><term><see cref="Metadata.FileType"/></term><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// </remarks>
    private class MetadataTableAccessor : TableAccessor<Metadata>
    {
        internal MetadataTableAccessor(SQLCDB sqlCDB)
            : base(sqlCDB, sqlCDB.SelectFromMetadataStatement, sqlCDB.InsertIntoMetadataStatement)
        {
        }

        /// <inheritdoc/>
        internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
        {
            CreateAndAttachParameter(dbCommand, sqlCDB.MetadataNameParamName, DbType.String);
            CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
        }

        /// <inheritdoc/>
        internal override void SetObjectParameters(DbCommand dbCommand, Metadata obj)
        {
            dbCommand.Parameters[sqlCDB.MetadataNameParamName].Value = obj.Name;
            dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
        }
    }

    /// <summary>
    /// The SQL DDL statement to create the Metadata table.
    /// </summary>
    protected abstract string CreateTableMetadataStatement
    {
        get;
    }

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

    /// <inheritdoc/>
    public Stream? ReadMetadata(Metadata metadata)
    {
        return metadataAccessor.SelectUsingNewConnection(metadata);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMetadataAsync(Metadata metadata, CancellationToken cancellationToken)
    {
        return metadataAccessor.SelectUsingNewConnectionAsync(metadata, cancellationToken);
    }

    #endregion

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

    /// <inheritdoc/>
    public void WriteMetadata(Metadata metadata, Stream content)
    {
        metadataAccessor.InsertUsingNewConnection(metadata, content);
    }

    /// <inheritdoc/>
    public Task WriteMetadataAsync(Metadata metadata, Stream content, CancellationToken cancellationToken)
    {
        return metadataAccessor.InsertUsingNewConnectionAsync(metadata, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Texture

    /// <summary>
    /// A table accessor for type <see cref="Texture"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><term><see cref="Texture"/> Property</term><description>SQL Statement Parameter</description></listheader>
    ///   <item><term><see cref="Texture.Dataset"/></term><description><see cref="DatasetParamName"/></description></item>
    ///   <item><term><see cref="Texture.ComponentSelector1"/></term><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><term><see cref="Texture.ComponentSelector2"/></term><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><term><see cref="Texture.Name"/></term><description><see cref="TextureNameParamName"/></description></item>
    ///   <item><term><see cref="Texture.FileType"/></term><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// </remarks>
    private class TextureTableAccessor : TableAccessor<Texture>
    {
        internal TextureTableAccessor(SQLCDB sqlCDB)
            : base(sqlCDB, sqlCDB.SelectFromTextureStatement, sqlCDB.InsertIntoTextureStatement)
        {
        }

        /// <inheritdoc/>
        internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
        {
            CreateAndAttachParameter(dbCommand, sqlCDB.DatasetParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector1ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector2ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.TextureNameParamName, DbType.String);
            CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
        }

        /// <inheritdoc/>
        internal override void SetObjectParameters(DbCommand dbCommand, Texture obj)
        {
            dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.Dataset.Value;
            dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
            dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
            dbCommand.Parameters[sqlCDB.TextureNameParamName].Value = obj.Name;
            dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
        }
    }

    /// <summary>
    /// The SQL DDL statement to create the Texture table.
    /// </summary>
    protected abstract string CreateTableTextureStatement
    {
        get;
    }

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

    /// <inheritdoc/>
    public Stream? ReadTexture(Texture texture)
    {
        return textureAccessor.SelectUsingNewConnection(texture);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTextureAsync(Texture texture, CancellationToken cancellationToken)
    {
        return textureAccessor.SelectUsingNewConnectionAsync(texture, cancellationToken);
    }

    #endregion

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

    /// <inheritdoc/>
    public void WriteTexture(Texture texture, Stream content)
    {
        textureAccessor.InsertUsingNewConnection(texture, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureAsync(Texture texture, Stream content, CancellationToken cancellationToken)
    {
        return textureAccessor.InsertUsingNewConnectionAsync(texture, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Texture LOD

    /// <summary>
    /// A table accessor for type <see cref="TextureLod"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><term><see cref="TextureLod"/> Property</term><description>SQL Statement Parameter</description></listheader>
    ///   <item><term><see cref="TextureLod.Dataset"/></term><description><see cref="DatasetParamName"/></description></item>
    ///   <item><term><see cref="TextureLod.ComponentSelector1"/></term><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><term><see cref="TextureLod.ComponentSelector2"/></term><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><term><see cref="TextureLod.LevelOfDetail"/></term><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><term><see cref="TextureLod.Name"/></term><description><see cref="TextureNameParamName"/></description></item>
    ///   <item><term><see cref="TextureLod.FileType"/></term><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// </remarks>
    private class TextureLodTableAccessor : TableAccessor<TextureLod>
    {
        internal TextureLodTableAccessor(SQLCDB sqlCDB)
            : base(sqlCDB, sqlCDB.SelectFromTextureLodStatement, sqlCDB.InsertIntoTextureLodStatement)
        {
        }

        /// <inheritdoc/>
        internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
        {
            CreateAndAttachParameter(dbCommand, sqlCDB.DatasetParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector1ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector2ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.LevelOfDetailParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.TextureNameParamName, DbType.String);
            CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
        }

        /// <inheritdoc/>
        internal override void SetObjectParameters(DbCommand dbCommand, TextureLod obj)
        {
            dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.Dataset.Value;
            dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
            dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
            dbCommand.Parameters[sqlCDB.LevelOfDetailParamName].Value = obj.LevelOfDetail.Value;
            dbCommand.Parameters[sqlCDB.TextureNameParamName].Value = obj.Name;
            dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
        }
    }

    /// <summary>
    /// The SQL DDL statement to create the Texture Level of Detail table.
    /// </summary>
    protected abstract string CreateTableTextureLodStatement
    {
        get;
    }

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

    /// <inheritdoc/>
    public Stream? ReadTextureLevelOfDetail(TextureLod textureLod)
    {
        return textureLodAccessor.SelectUsingNewConnection(textureLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTextureLevelOfDetailAsync(TextureLod textureLod, CancellationToken cancellationToken)
    {
        return textureLodAccessor.SelectUsingNewConnectionAsync(textureLod, cancellationToken);
    }

    #endregion

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

    /// <inheritdoc/>
    public void WriteTextureLevelOfDetail(TextureLod textureLod, Stream content)
    {
        textureLodAccessor.InsertUsingNewConnection(textureLod, content);
    }

    /// <inheritdoc/>
    public Task WriteTextureLevelOfDetailAsync(TextureLod textureLod, Stream content, CancellationToken cancellationToken)
    {
        return textureLodAccessor.InsertUsingNewConnectionAsync(textureLod, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Geotypical Model

    /// <summary>
    /// A table accessor for type <see cref="GeotypicalModel"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><term><see cref="GeotypicalModel"/> Property</term><description>SQL Statement Parameter</description></listheader>
    ///   <item><term><see cref="GeotypicalModel.Dataset"/></term><description><see cref="DatasetParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModel.ComponentSelector1"/></term><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModel.ComponentSelector2"/></term><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModel.FeatureCode"/> <see cref="FeatureCode.Category"/></term><description><see cref="FeatureCategoryParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModel.FeatureCode"/> <see cref="FeatureCode.Subcategory"/></term><description><see cref="FeatureSubcategoryParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModel.FeatureCode"/> <see cref="FeatureCode.Type"/></term><description><see cref="FeatureTypeParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModel.FeatureSubcode"/></term><description><see cref="FeatureSubcodeParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModel.Name"/></term><description><see cref="ModelNameParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModel.FileType"/></term><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// </remarks>
    private class GeotypicalModelTableAccessor : TableAccessor<GeotypicalModel>
    {
        internal GeotypicalModelTableAccessor(SQLCDB sqlCDB)
            : base(sqlCDB, sqlCDB.SelectFromGeotypicalModelStatement, sqlCDB.InsertIntoGeotypicalModelStatement)
        {
        }

        /// <inheritdoc/>
        internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
        {
            CreateAndAttachParameter(dbCommand, sqlCDB.DatasetParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector1ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector2ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.FeatureCategoryParamName, DbType.String);
            CreateAndAttachParameter(dbCommand, sqlCDB.FeatureSubcategoryParamName, DbType.String);
            CreateAndAttachParameter(dbCommand, sqlCDB.FeatureTypeParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.FeatureSubcodeParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ModelNameParamName, DbType.String);
            CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
        }

        /// <inheritdoc/>
        internal override void SetObjectParameters(DbCommand dbCommand, GeotypicalModel obj)
        {
            dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.Dataset.Value;
            dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
            dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
            dbCommand.Parameters[sqlCDB.FeatureCategoryParamName].Value = obj.FeatureCode.Category;
            dbCommand.Parameters[sqlCDB.FeatureSubcategoryParamName].Value = obj.FeatureCode.Subcategory;
            dbCommand.Parameters[sqlCDB.FeatureTypeParamName].Value = obj.FeatureCode.Type;
            dbCommand.Parameters[sqlCDB.FeatureSubcodeParamName].Value = obj.FeatureSubcode;
            dbCommand.Parameters[sqlCDB.ModelNameParamName].Value = obj.Name;
            dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
        }
    }

    /// <summary>
    /// The SQL DDL statement to create the Geotypical Model table.
    /// </summary>
    protected abstract string CreateTableGeotypicalModelStatement
    {
        get;
    }

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

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModel(GeotypicalModel geotypicalModel)
    {
        return geotypicalModelAccessor.SelectUsingNewConnection(geotypicalModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadGeotypicalModelAsync(GeotypicalModel geotypicalModel, CancellationToken cancellationToken)
    {
        return geotypicalModelAccessor.SelectUsingNewConnectionAsync(geotypicalModel, cancellationToken);
    }

    #endregion

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

    /// <inheritdoc/>
    public void WriteGeotypicalModel(GeotypicalModel geotypicalModel, Stream content)
    {
        geotypicalModelAccessor.InsertUsingNewConnection(geotypicalModel, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelAsync(GeotypicalModel geotypicalModel, Stream content, CancellationToken cancellationToken)
    {
        return geotypicalModelAccessor.InsertUsingNewConnectionAsync(geotypicalModel, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Geotypical Model LOD

    /// <summary>
    /// A table accessor for type <see cref="GeotypicalModelLod"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><term><see cref="GeotypicalModelLod"/> Property</term><description>SQL Statement Parameter</description></listheader>
    ///   <item><term><see cref="GeotypicalModelLod.Dataset"/></term><description><see cref="DatasetParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModelLod.ComponentSelector1"/></term><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModelLod.ComponentSelector2"/></term><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModelLod.LevelOfDetail"/></term><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModelLod.FeatureCode"/> <see cref="FeatureCode.Category"/></term><description><see cref="FeatureCategoryParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModelLod.FeatureCode"/> <see cref="FeatureCode.Subcategory"/></term><description><see cref="FeatureSubcategoryParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModelLod.FeatureCode"/> <see cref="FeatureCode.Type"/></term><description><see cref="FeatureTypeParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModelLod.FeatureSubcode"/></term><description><see cref="FeatureSubcodeParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModelLod.Name"/></term><description><see cref="ModelNameParamName"/></description></item>
    ///   <item><term><see cref="GeotypicalModelLod.FileType"/></term><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// </remarks>
    private class GeotypicalModelLodTableAccessor : TableAccessor<GeotypicalModelLod>
    {
        internal GeotypicalModelLodTableAccessor(SQLCDB sqlCDB)
            : base(sqlCDB, sqlCDB.SelectFromGeotypicalModelLodStatement, sqlCDB.InsertIntoGeotypicalModelLodStatement)
        {
        }

        /// <inheritdoc/>
        internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
        {
            CreateAndAttachParameter(dbCommand, sqlCDB.DatasetParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector1ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector2ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.LevelOfDetailParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.FeatureCategoryParamName, DbType.String);
            CreateAndAttachParameter(dbCommand, sqlCDB.FeatureSubcategoryParamName, DbType.String);
            CreateAndAttachParameter(dbCommand, sqlCDB.FeatureTypeParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.FeatureSubcodeParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ModelNameParamName, DbType.String);
            CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
        }

        /// <inheritdoc/>
        internal override void SetObjectParameters(DbCommand dbCommand, GeotypicalModelLod obj)
        {
            dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.Dataset.Value;
            dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
            dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
            dbCommand.Parameters[sqlCDB.LevelOfDetailParamName].Value = obj.LevelOfDetail.Value;
            dbCommand.Parameters[sqlCDB.FeatureCategoryParamName].Value = obj.FeatureCode.Category;
            dbCommand.Parameters[sqlCDB.FeatureSubcategoryParamName].Value = obj.FeatureCode.Subcategory;
            dbCommand.Parameters[sqlCDB.FeatureTypeParamName].Value = obj.FeatureCode.Type;
            dbCommand.Parameters[sqlCDB.FeatureSubcodeParamName].Value = obj.FeatureSubcode;
            dbCommand.Parameters[sqlCDB.ModelNameParamName].Value = obj.Name;
            dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
        }
    }

    /// <summary>
    /// The SQL DDL statement to create the Geotypical Model Level of Detail table.
    /// </summary>
    protected abstract string CreateTableGeotypicalModelLodStatement
    {
        get;
    }

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

    /// <inheritdoc/>
    public Stream? ReadGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod)
    {
        return geotypicalModelLodAccessor.SelectUsingNewConnection(geotypicalModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, CancellationToken cancellationToken)
    {
        return geotypicalModelLodAccessor.SelectUsingNewConnectionAsync(geotypicalModelLod, cancellationToken);
    }

    #endregion

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

    /// <inheritdoc/>
    public void WriteGeotypicalModelLevelOfDetail(GeotypicalModelLod geotypicalModelLod, Stream content)
    {
        geotypicalModelLodAccessor.InsertUsingNewConnection(geotypicalModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteGeotypicalModelLevelOfDetailAsync(GeotypicalModelLod geotypicalModelLod, Stream content, CancellationToken cancellationToken)
    {
        return geotypicalModelLodAccessor.InsertUsingNewConnectionAsync(geotypicalModelLod, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Moving Model

    /// <summary>
    /// A table accessor for type <see cref="MovingModel"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><term><see cref="MovingModel"/> Property</term><description>SQL Statement Parameter</description></listheader>
    ///   <item><term><see cref="MovingModel.Dataset"/></term><description><see cref="DatasetParamName"/></description></item>
    ///   <item><term><see cref="MovingModel.ComponentSelector1"/></term><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><term><see cref="MovingModel.ComponentSelector2"/></term><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><term><see cref="MovingModel.MMDC"/> <see cref="DISEntity.Kind"/></term><description><see cref="DISKindParamName"/></description></item>
    ///   <item><term><see cref="MovingModel.MMDC"/> <see cref="DISEntity.Domain"/></term><description><see cref="DISDomainParamName"/></description></item>
    ///   <item><term><see cref="MovingModel.MMDC"/> <see cref="DISEntity.Country"/></term><description><see cref="DISCountryParamName"/></description></item>
    ///   <item><term><see cref="MovingModel.MMDC"/> <see cref="DISEntity.Category"/></term><description><see cref="DISCategoryParamName"/></description></item>
    ///   <item><term><see cref="MovingModel.MMDC"/> <see cref="DISEntity.Subcategory"/></term><description><see cref="DISSubcategoryParamName"/></description></item>
    ///   <item><term><see cref="MovingModel.MMDC"/> <see cref="DISEntity.Specific"/></term><description><see cref="DISSpecificParamName"/></description></item>
    ///   <item><term><see cref="MovingModel.MMDC"/> <see cref="DISEntity.Extra"/></term><description><see cref="DISExtraParamName"/></description></item>
    ///   <item><term><see cref="MovingModel.FileType"/></term><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// </remarks>
    private class MovingModelTableAccessor : TableAccessor<MovingModel>
    {
        internal MovingModelTableAccessor(SQLCDB sqlCDB)
            : base(sqlCDB, sqlCDB.SelectFromMovingModelStatement, sqlCDB.InsertIntoMovingModelStatement)
        {
        }

        /// <inheritdoc/>
        internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
        {
            CreateAndAttachParameter(dbCommand, sqlCDB.DatasetParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector1ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector2ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DISKindParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DISDomainParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DISCountryParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DISCategoryParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DISSubcategoryParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DISSpecificParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DISExtraParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
        }

        /// <inheritdoc/>
        internal override void SetObjectParameters(DbCommand dbCommand, MovingModel obj)
        {
            dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.Dataset.Value;
            dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
            dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
            dbCommand.Parameters[sqlCDB.DISKindParamName].Value = obj.MMDC.Kind;
            dbCommand.Parameters[sqlCDB.DISDomainParamName].Value = obj.MMDC.Domain;
            dbCommand.Parameters[sqlCDB.DISCountryParamName].Value = obj.MMDC.Country;
            dbCommand.Parameters[sqlCDB.DISCategoryParamName].Value = obj.MMDC.Category;
            dbCommand.Parameters[sqlCDB.DISSubcategoryParamName].Value = obj.MMDC.Subcategory;
            dbCommand.Parameters[sqlCDB.DISSpecificParamName].Value = obj.MMDC.Specific;
            dbCommand.Parameters[sqlCDB.DISExtraParamName].Value = obj.MMDC.Extra;
            dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
        }
    }

    /// <summary>
    /// The SQL DDL statement to create the Moving Model table.
    /// </summary>
    protected abstract string CreateTableMovingModelStatement
    {
        get;
    }

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

    /// <inheritdoc/>
    public Stream? ReadMovingModel(MovingModel movingModel)
    {
        return movingModelAccessor.SelectUsingNewConnection(movingModel);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMovingModelAsync(MovingModel movingModel, CancellationToken cancellationToken)
    {
        return movingModelAccessor.SelectUsingNewConnectionAsync(movingModel, cancellationToken);
    }

    #endregion

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

    /// <inheritdoc/>
    public void WriteMovingModel(MovingModel movingModel, Stream content)
    {
        movingModelAccessor.InsertUsingNewConnection(movingModel, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelAsync(MovingModel movingModel, Stream content, CancellationToken cancellationToken)
    {
        return movingModelAccessor.InsertUsingNewConnectionAsync(movingModel, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Moving Model LOD

    /// <summary>
    /// A table accessor for type <see cref="MovingModelLod"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><term><see cref="MovingModelLod"/> Property</term><description>SQL Statement Parameter</description></listheader>
    ///   <item><term><see cref="MovingModelLod.Dataset"/></term><description><see cref="DatasetParamName"/></description></item>
    ///   <item><term><see cref="MovingModelLod.ComponentSelector1"/></term><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><term><see cref="MovingModelLod.ComponentSelector2"/></term><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><term><see cref="MovingModelLod.LevelOfDetail"/></term><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><term><see cref="MovingModelLod.MMDC"/> <see cref="DISEntity.Kind"/></term><description><see cref="DISKindParamName"/></description></item>
    ///   <item><term><see cref="MovingModelLod.MMDC"/> <see cref="DISEntity.Domain"/></term><description><see cref="DISDomainParamName"/></description></item>
    ///   <item><term><see cref="MovingModelLod.MMDC"/> <see cref="DISEntity.Country"/></term><description><see cref="DISCountryParamName"/></description></item>
    ///   <item><term><see cref="MovingModelLod.MMDC"/> <see cref="DISEntity.Category"/></term><description><see cref="DISCategoryParamName"/></description></item>
    ///   <item><term><see cref="MovingModelLod.MMDC"/> <see cref="DISEntity.Subcategory"/></term><description><see cref="DISSubcategoryParamName"/></description></item>
    ///   <item><term><see cref="MovingModelLod.MMDC"/> <see cref="DISEntity.Specific"/></term><description><see cref="DISSpecificParamName"/></description></item>
    ///   <item><term><see cref="MovingModelLod.MMDC"/> <see cref="DISEntity.Extra"/></term><description><see cref="DISExtraParamName"/></description></item>
    ///   <item><term><see cref="MovingModelLod.FileType"/></term><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// </remarks>
    private class MovingModelLodTableAccessor : TableAccessor<MovingModelLod>
    {
        internal MovingModelLodTableAccessor(SQLCDB sqlCDB)
            : base(sqlCDB, sqlCDB.SelectFromMovingModelLodStatement, sqlCDB.InsertIntoMovingModelLodStatement)
        {
        }

        /// <inheritdoc/>
        internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
        {
            CreateAndAttachParameter(dbCommand, sqlCDB.DatasetParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector1ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector2ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.LevelOfDetailParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DISKindParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DISDomainParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DISCountryParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DISCategoryParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DISSubcategoryParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DISSpecificParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DISExtraParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
        }

        /// <inheritdoc/>
        internal override void SetObjectParameters(DbCommand dbCommand, MovingModelLod obj)
        {
            dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.Dataset.Value;
            dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
            dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
            dbCommand.Parameters[sqlCDB.LevelOfDetailParamName].Value = obj.LevelOfDetail.Value;
            dbCommand.Parameters[sqlCDB.DISKindParamName].Value = obj.MMDC.Kind;
            dbCommand.Parameters[sqlCDB.DISDomainParamName].Value = obj.MMDC.Domain;
            dbCommand.Parameters[sqlCDB.DISCountryParamName].Value = obj.MMDC.Country;
            dbCommand.Parameters[sqlCDB.DISCategoryParamName].Value = obj.MMDC.Category;
            dbCommand.Parameters[sqlCDB.DISSubcategoryParamName].Value = obj.MMDC.Subcategory;
            dbCommand.Parameters[sqlCDB.DISSpecificParamName].Value = obj.MMDC.Specific;
            dbCommand.Parameters[sqlCDB.DISExtraParamName].Value = obj.MMDC.Extra;
            dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
        }
    }

    /// <summary>
    /// The SQL DDL statement to create the Moving Model Level of Detail table.
    /// </summary>
    protected abstract string CreateTableMovingModelLodStatement
    {
        get;
    }

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

    /// <inheritdoc/>
    public Stream? ReadMovingModelLevelOfDetail(MovingModelLod movingModelLod)
    {
        return movingModelLodAccessor.SelectUsingNewConnection(movingModelLod);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, CancellationToken cancellationToken)
    {
        return movingModelLodAccessor.SelectUsingNewConnectionAsync(movingModelLod, cancellationToken);
    }

    #endregion

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

    /// <inheritdoc/>
    public void WriteMovingModelLevelOfDetail(MovingModelLod movingModelLod, Stream content)
    {
        movingModelLodAccessor.InsertUsingNewConnection(movingModelLod, content);
    }

    /// <inheritdoc/>
    public Task WriteMovingModelLevelOfDetailAsync(MovingModelLod movingModelLod, Stream content, CancellationToken cancellationToken)
    {
        return movingModelLodAccessor.InsertUsingNewConnectionAsync(movingModelLod, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Tile

    /// <summary>
    /// A table accessor for type <see cref="Tile"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><term><see cref="Tile"/> Property</term><description>SQL Statement Parameter</description></listheader>
    ///   <item><term><see cref="Tile.LatitudeValue"/></term><description><see cref="LatitudeParamName"/></description></item>
    ///   <item><term><see cref="Tile.LongitudeValue"/></term><description><see cref="LongitudeParamName"/></description></item>
    ///   <item><term><see cref="Tile.DatasetValue"/></term><description><see cref="DatasetParamName"/></description></item>
    ///   <item><term><see cref="Tile.ComponentSelector1"/></term><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><term><see cref="Tile.ComponentSelector2"/></term><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><term><see cref="Tile.Level"/></term><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><term><see cref="Tile.Up"/></term><description><see cref="UpParamName"/></description></item>
    ///   <item><term><see cref="Tile.Right"/></term><description><see cref="RightParamName"/></description></item>
    ///   <item><term><see cref="Tile.FileType"/></term><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// </remarks>
    private class TileTableAccessor : TableAccessor<Tile>
    {
        internal TileTableAccessor(SQLCDB sqlCDB)
            : base(sqlCDB, sqlCDB.SelectFromTileStatement, sqlCDB.InsertIntoTileStatement)
        {
        }

        /// <inheritdoc/>
        internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
        {
            CreateAndAttachParameter(dbCommand, sqlCDB.LatitudeParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.LongitudeParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DatasetParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector1ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector2ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.LevelOfDetailParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.UpParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.RightParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
        }

        /// <inheritdoc/>
        internal override void SetObjectParameters(DbCommand dbCommand, Tile obj)
        {
            dbCommand.Parameters[sqlCDB.LatitudeParamName].Value = obj.LatitudeValue.Value;
            dbCommand.Parameters[sqlCDB.LongitudeParamName].Value = obj.LongitudeValue.Value;
            dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.DatasetValue.Value;
            dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
            dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
            dbCommand.Parameters[sqlCDB.LevelOfDetailParamName].Value = obj.Level.Value;
            dbCommand.Parameters[sqlCDB.UpParamName].Value = obj.Up;
            dbCommand.Parameters[sqlCDB.RightParamName].Value = obj.Right;
            dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
        }
    }

    /// <summary>
    /// The SQL DDL statement to create the Tile table.
    /// </summary>
    protected abstract string CreateTableTileStatement
    {
        get;
    }

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

    /// <inheritdoc/>
    public Stream? ReadTile(Tile tile)
    {
        return tileAccessor.SelectUsingNewConnection(tile);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileAsync(Tile tile, CancellationToken cancellationToken)
    {
        return tileAccessor.SelectUsingNewConnectionAsync(tile, cancellationToken);
    }

    #endregion

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

    /// <inheritdoc/>
    public void WriteTile(Tile tile, Stream content)
    {
        tileAccessor.InsertUsingNewConnection(tile, content);
    }

    /// <inheritdoc/>
    public Task WriteTileAsync(Tile tile, Stream content, CancellationToken cancellationToken)
    {
        return tileAccessor.InsertUsingNewConnectionAsync(tile, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Tile Archived Feature

    /// <summary>
    /// A table accessor for type <see cref="TileArchivedFeature"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><term><see cref="TileArchivedFeature"/> Property</term><description>SQL Statement Parameter</description></listheader>
    ///   <item><term><see cref="TileArchivedFeature.LatitudeValue"/></term><description><see cref="LatitudeParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedFeature.LongitudeValue"/></term><description><see cref="LongitudeParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedFeature.DatasetValue"/></term><description><see cref="DatasetParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedFeature.ComponentSelector1"/></term><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedFeature.ComponentSelector2"/></term><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedFeature.Level"/></term><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedFeature.Up"/></term><description><see cref="UpParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedFeature.Right"/></term><description><see cref="RightParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedFeature.FeatureCode"/> <see cref="FeatureCode.Category"/></term><description><see cref="FeatureCategoryParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedFeature.FeatureCode"/> <see cref="FeatureCode.Subcategory"/></term><description><see cref="FeatureSubcategoryParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedFeature.FeatureCode"/> <see cref="FeatureCode.Type"/></term><description><see cref="FeatureTypeParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedFeature.FeatureSubcode"/></term><description><see cref="FeatureSubcodeParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedFeature.Name"/></term><description><see cref="ModelNameParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedFeature.FileType"/></term><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// </remarks>
    private class TileArchivedFeatureTableAccessor : TableAccessor<TileArchivedFeature>
    {
        internal TileArchivedFeatureTableAccessor(SQLCDB sqlCDB)
            : base(sqlCDB, sqlCDB.SelectFromTileArchivedFeatureStatement, sqlCDB.InsertIntoTileArchivedFeatureStatement)
        {
        }

        /// <inheritdoc/>
        internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
        {
            CreateAndAttachParameter(dbCommand, sqlCDB.LatitudeParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.LongitudeParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DatasetParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector1ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector2ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.LevelOfDetailParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.UpParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.RightParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.FeatureCategoryParamName, DbType.String);
            CreateAndAttachParameter(dbCommand, sqlCDB.FeatureSubcategoryParamName, DbType.String);
            CreateAndAttachParameter(dbCommand, sqlCDB.FeatureTypeParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.FeatureSubcodeParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ModelNameParamName, DbType.String);
            CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
        }

        /// <inheritdoc/>
        internal override void SetObjectParameters(DbCommand dbCommand, TileArchivedFeature obj)
        {
            dbCommand.Parameters[sqlCDB.LatitudeParamName].Value = obj.LatitudeValue.Value;
            dbCommand.Parameters[sqlCDB.LongitudeParamName].Value = obj.LongitudeValue.Value;
            dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.DatasetValue.Value;
            dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
            dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
            dbCommand.Parameters[sqlCDB.LevelOfDetailParamName].Value = obj.Level.Value;
            dbCommand.Parameters[sqlCDB.UpParamName].Value = obj.Up;
            dbCommand.Parameters[sqlCDB.RightParamName].Value = obj.Right;
            dbCommand.Parameters[sqlCDB.FeatureCategoryParamName].Value = obj.FeatureCode.Category;
            dbCommand.Parameters[sqlCDB.FeatureSubcategoryParamName].Value = obj.FeatureCode.Subcategory;
            dbCommand.Parameters[sqlCDB.FeatureTypeParamName].Value = obj.FeatureCode.Type;
            dbCommand.Parameters[sqlCDB.FeatureSubcodeParamName].Value = obj.FeatureSubcode;
            dbCommand.Parameters[sqlCDB.ModelNameParamName].Value = obj.Name;
            dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
        }
    }

    /// <summary>
    /// The SQL DDL statement to create the TileArchivedFeature table.
    /// </summary>
    protected abstract string CreateTableTileArchivedFeatureStatement
    {
        get;
    }

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

    /// <inheritdoc/>
    public Stream? ReadTileFeature(TileArchivedFeature tileArchivedFeature)
    {
        return tileFeatureAccessor.SelectUsingNewConnection(tileArchivedFeature);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileFeatureAsync(TileArchivedFeature tileArchivedFeature, CancellationToken cancellationToken)
    {
        return tileFeatureAccessor.SelectUsingNewConnectionAsync(tileArchivedFeature, cancellationToken);
    }

    #endregion

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

    /// <inheritdoc/>
    public void WriteTileFeature(TileArchivedFeature tileArchivedFeature, Stream content)
    {
        tileFeatureAccessor.InsertUsingNewConnection(tileArchivedFeature, content);
    }

    /// <inheritdoc/>
    public Task WriteTileFeatureAsync(TileArchivedFeature tileArchivedFeature, Stream content, CancellationToken cancellationToken)
    {
        return tileFeatureAccessor.InsertUsingNewConnectionAsync(tileArchivedFeature, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Tile Archived Texture

    /// <summary>
    /// A table accessor for type <see cref="TileArchivedTexture"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><term><see cref="TileArchivedTexture"/> Property</term><description>SQL Statement Parameter</description></listheader>
    ///   <item><term><see cref="TileArchivedTexture.LatitudeValue"/></term><description><see cref="LatitudeParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedTexture.LongitudeValue"/></term><description><see cref="LongitudeParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedTexture.DatasetValue"/></term><description><see cref="DatasetParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedTexture.ComponentSelector1"/></term><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedTexture.ComponentSelector2"/></term><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedTexture.Level"/></term><description><see cref="LevelOfDetailParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedTexture.Up"/></term><description><see cref="UpParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedTexture.Right"/></term><description><see cref="RightParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedTexture.Name"/></term><description><see cref="TextureNameParamName"/></description></item>
    ///   <item><term><see cref="TileArchivedTexture.FileType"/></term><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// </remarks>
    private class TileArchivedTextureTableAccessor : TableAccessor<TileArchivedTexture>
    {
        internal TileArchivedTextureTableAccessor(SQLCDB sqlCDB)
            : base(sqlCDB, sqlCDB.SelectFromTileArchivedTextureStatement, sqlCDB.InsertIntoTileArchivedTextureStatement)
        {
        }

        /// <inheritdoc/>
        internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
        {
            CreateAndAttachParameter(dbCommand, sqlCDB.LatitudeParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.LongitudeParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.DatasetParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector1ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector2ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.LevelOfDetailParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.UpParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.RightParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.TextureNameParamName, DbType.String);
            CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
        }

        /// <inheritdoc/>
        internal override void SetObjectParameters(DbCommand dbCommand, TileArchivedTexture obj)
        {
            dbCommand.Parameters[sqlCDB.LatitudeParamName].Value = obj.LatitudeValue.Value;
            dbCommand.Parameters[sqlCDB.LongitudeParamName].Value = obj.LongitudeValue.Value;
            dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.DatasetValue.Value;
            dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
            dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
            dbCommand.Parameters[sqlCDB.LevelOfDetailParamName].Value = obj.Level.Value;
            dbCommand.Parameters[sqlCDB.UpParamName].Value = obj.Up;
            dbCommand.Parameters[sqlCDB.RightParamName].Value = obj.Right;
            dbCommand.Parameters[sqlCDB.TextureNameParamName].Value = obj.Name;
            dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
        }
    }

    /// <summary>
    /// The SQL DDL statement to create the TileArchivedTexture table.
    /// </summary>
    protected abstract string CreateTableTileArchivedTextureStatement
    {
        get;
    }

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

    /// <inheritdoc/>
    public Stream? ReadTileTexture(TileArchivedTexture tileArchivedTexture)
    {
        return tileTextureAccessor.SelectUsingNewConnection(tileArchivedTexture);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadTileTextureAsync(TileArchivedTexture tileArchivedTexture, CancellationToken cancellationToken)
    {
        return tileTextureAccessor.SelectUsingNewConnectionAsync(tileArchivedTexture, cancellationToken);
    }

    #endregion

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

    /// <inheritdoc/>
    public void WriteTileTexture(TileArchivedTexture tileArchivedTexture, Stream content)
    {
        tileTextureAccessor.InsertUsingNewConnection(tileArchivedTexture, content);
    }

    /// <inheritdoc/>
    public Task WriteTileTextureAsync(TileArchivedTexture tileArchivedTexture, Stream content, CancellationToken cancellationToken)
    {
        return tileTextureAccessor.InsertUsingNewConnectionAsync(tileArchivedTexture, content, cancellationToken);
    }

    #endregion

    #endregion

    #region Navigation

    /// <summary>
    /// A table accessor for type <see cref="Navigation"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <listheader><term><see cref="Navigation"/> Property</term><description>SQL Statement Parameter</description></listheader>
    ///   <item><term><see cref="Navigation.Dataset"/></term><description><see cref="DatasetParamName"/></description></item>
    ///   <item><term><see cref="Navigation.ComponentSelector1"/></term><description><see cref="ComponentSelector1ParamName"/></description></item>
    ///   <item><term><see cref="Navigation.ComponentSelector2"/></term><description><see cref="ComponentSelector2ParamName"/></description></item>
    ///   <item><term><see cref="Navigation.FileType"/></term><description><see cref="FileTypeParamName"/></description></item>
    /// </list>
    /// </remarks>
    private class NavigationTableAccessor : TableAccessor<Navigation>
    {
        internal NavigationTableAccessor(SQLCDB sqlCDB)
            : base(sqlCDB, sqlCDB.SelectFromNavigationStatement, sqlCDB.InsertIntoNavigationStatement)
        {
        }

        /// <inheritdoc/>
        internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
        {
            CreateAndAttachParameter(dbCommand, sqlCDB.DatasetParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector1ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector2ParamName, DbType.Int32);
            CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
        }

        /// <inheritdoc/>
        internal override void SetObjectParameters(DbCommand dbCommand, Navigation obj)
        {
            dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.Dataset.Value;
            dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
            dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
            dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
        }
    }

    /// <summary>
    /// The SQL DDL statement to create the Navigation table.
    /// </summary>
    protected abstract string CreateTableNavigationStatement
    {
        get;
    }

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

    /// <inheritdoc/>
    public Stream? ReadNavigation(Navigation navigation)
    {
        return navigationAccessor.SelectUsingNewConnection(navigation);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadNavigationAsync(Navigation navigation, CancellationToken cancellationToken)
    {
        return navigationAccessor.SelectUsingNewConnectionAsync(navigation, cancellationToken);
    }

    #endregion

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

    /// <inheritdoc/>
    public void WriteNavigation(Navigation navigation, Stream content)
    {
        navigationAccessor.InsertUsingNewConnection(navigation, content);
    }

    /// <inheritdoc/>
    public Task WriteNavigationAsync(Navigation navigation, Stream content, CancellationToken cancellationToken)
    {
        return navigationAccessor.InsertUsingNewConnectionAsync(navigation, content, cancellationToken);
    }

    #endregion

    #endregion

    /// <summary>
    /// Any SQL DDL statements to create indexes necessary for queries to run
    /// efficiently.
    /// </summary>
    protected abstract IEnumerable<string> CreateIndexStatements
    {
        get;
    }

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
        textWriter.WriteLine();
        foreach (string createIndexStatement in CreateIndexStatements)
        {
            textWriter.Write(createIndexStatement);
            textWriter.WriteLine(';');
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
