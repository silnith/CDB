namespace Silnith.CDB.SQL;

/// <summary>
/// Configuration settings for a CDB object stored in an SQL database.
/// </summary>
public class SQLCDBSettings
{
    /// <summary>
    /// A simple identifier for the CDB data store.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value must match one of the names stored in the <c>CDB</c> table
    /// inside the database.
    /// </para>
    /// </remarks>
    public string Name
    {
        get;
        set;
    }
}
