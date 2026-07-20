namespace Silnith.CDB.SQL;

/// <summary>
/// Configuration settings for an SQL-based CDB data store.
/// </summary>
public class SQLDataStoreSettings
{
    /// <summary>
    /// Whether to create the schema when connecting to the data store.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The schema creation logic is not idempotent.
    /// </para>
    /// </remarks>
    public bool CreateSchema
    {
        get;
        set;
    }
}
