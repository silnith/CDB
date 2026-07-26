namespace Silnith.CDB;

/// <summary>
/// A unique identifier for an archived file in a CDB data store.
/// This is a file that exists inside of an archive, so cannot be read directly.
/// </summary>
public interface ICDBArchivedIdentifier : ICDBIdentifier
{
    /// <summary>
    /// The name of the entry in the archive that contains this file.
    /// </summary>
    public string EntryName
    {
        get;
    }

    /// <summary>
    /// The identifier for the archive that contains this file.
    /// </summary>
    public ICDBFileIdentifier ArchiveIdentifier
    {
        get;
    }

}
