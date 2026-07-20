using System.IO;

namespace Silnith.CDB.FileSystem;

/// <summary>
/// Configuration settings for a traditional filesystem CDB.
/// </summary>
public class FileSystemCDBSettings
{
    /// <summary>
    /// The root directory of the CDB data store.
    /// </summary>
    public DirectoryInfo Root
    {
        get;
        set;
    }
}
