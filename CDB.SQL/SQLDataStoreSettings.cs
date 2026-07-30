using System.ComponentModel.DataAnnotations;

namespace Silnith.CDB.SQL;

/// <summary>
/// Configuration settings for an SQL-based CDB data store.
/// </summary>
public class SQLDataStoreSettings
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
    [Required]
    public string Name
    {
        get;
        set;
    }

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
