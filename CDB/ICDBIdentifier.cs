using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Silnith.CDB;

/// <summary>
/// A unique identifier for something that can be unambiguously read from a CDB data store.
/// </summary>
public interface ICDBIdentifier
{
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
