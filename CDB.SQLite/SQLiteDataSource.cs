using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace Silnith.CDB.SQL.SQLite;

/// <summary>
/// A data source for SQLite connections.
/// </summary>
/// <remarks>
/// <para>
/// The <c>Microsoft.Data.Sqlite</c> driver is "lightweight", which means it
/// lacks some basic functionality.  A standard data source for vending
/// database connections is one of them.
/// </para>
/// <para>
/// Beware that for an in-memory database, each connection represents an
/// entirely new database.
/// </para>
/// </remarks>
/// <inheritdoc/>
public class SQLiteDataSource : DbDataSource
{
    private readonly SqliteConnectionStringBuilder connectionStringBuilder;

    /// <summary>
    /// Creates a new SQLite data source using the provided connection string builder.
    /// </summary>
    /// <param name="connectionStringBuilder">The connection string builder to use
    /// when creating new connections.</param>
    public SQLiteDataSource(SqliteConnectionStringBuilder connectionStringBuilder)
    {
        this.connectionStringBuilder = connectionStringBuilder;
    }

    /// <inheritdoc/>
    public override string ConnectionString => connectionStringBuilder.ConnectionString;

    /// <inheritdoc/>
    protected override DbConnection CreateDbConnection()
    {
        return new SqliteConnection(ConnectionString);
    }
}
