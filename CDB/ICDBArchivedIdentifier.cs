using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Silnith.CDB;

/// <summary>
/// A unique identifier for an archived file in a CDB data store.
/// This is a file that exists inside of an archive, so cannot be read directly.
/// </summary>
public interface ICDBArchivedIdentifier
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
    public ICDBIdentifier ArchiveIdentifier
    {
        get;
    }

    /// <summary>
    /// Reads the file for this identifier from the provided CDB.
    /// </summary>
    /// <param name="cdb">The CDB to read this file from.</param>
    /// <returns>The file contents, or <see langword="null"/>.</returns>
    public Stream? ReadFromCDB(ICDB cdb);

    /// <summary>
    /// Reads the file for this identifier from the provided CDB.
    /// </summary>
    /// <param name="cdb">The CDB to read this file from.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The file contents, or <see langword="null"/>.</returns>
    public Task<Stream?> ReadFromCDBAsync(ICDB cdb, CancellationToken cancellationToken = default);

}
