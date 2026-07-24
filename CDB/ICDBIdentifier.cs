namespace Silnith.CDB;

/// <summary>
/// A unique identifier for a file in a CDB data store.
/// </summary>
public interface ICDBIdentifier
{
    /// <summary>
    /// The name of the file.
    /// </summary>
    public string Filename
    {
        get;
    }

    /// <summary>
    /// The relative path from the CDB root directory to the file.
    /// </summary>
    public string RelativePath
    {
        get;
    }
}
