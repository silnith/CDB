using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Silnith.CDB.SQL;

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
public abstract class TableAccessor<T>
{
    /// <inheritdoc cref="SQLCDB.CreateAndAttachParameter(DbCommand, string, DbType)"/>
    protected static void CreateAndAttachParameter(DbCommand dbCommand, string dbParameterName, DbType dbType)
    {
        /*
         * This indirection exists to make it easier to track how many references
         * are coming from this inheritance hierarchy versus the original class
         * itself.
         */
        SQLCDB.CreateAndAttachParameter(dbCommand, dbParameterName, dbType);
    }

    protected readonly SQLCDB sqlCDB;

    private readonly string selectStatement;

    private readonly string insertStatement;

    protected TableAccessor(SQLCDB sqlCDB, string selectStatement, string insertStatement)
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
    public void InitializeSelectCommand(DbCommand dbCommand)
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
    /// <returns>The contents of the <see cref="SQLCDB.ContentColumnName"/> column, or <see langword="null"/>.</returns>
    public Stream? SelectUsingPreparedStatement(DbCommand dbCommand, T obj)
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
    /// <returns>The contents of the <see cref="SQLCDB.ContentColumnName"/> column, or <see langword="null"/>.</returns>
    public async Task<Stream?> SelectUsingPreparedStatementAsync(DbCommand dbCommand, T obj, CancellationToken cancellationToken = default)
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
    /// <returns>The contents of the <see cref="SQLCDB.ContentColumnName"/> column, or <see langword="null"/>.</returns>
    public Stream? SelectUsingNewConnection(T obj)
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
    /// <returns>The contents of the <see cref="SQLCDB.ContentColumnName"/> column, or <see langword="null"/>.</returns>
    public async Task<Stream?> SelectUsingNewConnectionAsync(T obj, CancellationToken cancellationToken = default)
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
    public void InitializeInsertCommand(DbCommand dbCommand)
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
    public void InsertUsingPreparedStatement(DbCommand dbCommand, T obj, Stream content)
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
    public Task InsertUsingPreparedStatementAsync(DbCommand dbCommand, T obj, Stream content, CancellationToken cancellationToken = default)
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
    public void InsertUsingNewConnection(T obj, Stream content)
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
    public async Task InsertUsingNewConnectionAsync(T obj, Stream content, CancellationToken cancellationToken = default)
    {
        await using DbConnection dbConnection = await sqlCDB.dbDataSource.OpenConnectionAsync(cancellationToken);
        await using DbCommand dbCommand = dbConnection.CreateCommand();
        InitializeInsertCommand(dbCommand);
        await dbCommand.PrepareAsync(cancellationToken);

        await InsertUsingPreparedStatementAsync(dbCommand, obj, content, cancellationToken);
    }

}
