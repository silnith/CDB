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
    /// The file type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All CDB identifiers have a file type.
    /// </para>
    /// </remarks>
    public string FileType
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

    /// <summary>
    /// Writes the stream contents to the CDB as the new value for this identifier.
    /// </summary>
    /// <param name="cdb">The CDB to write this file to.</param>
    /// <param name="stream">The file contents.</param>
    public void WriteToCDB(ICDB cdb, Stream stream);

    /// <summary>
    /// Writes the stream contents to the CDB as the new value for this identifier.
    /// </summary>
    /// <param name="cdb">The CDB to write this file to.</param>
    /// <param name="stream">The file contents.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public Task WriteToCDBAsync(ICDB cdb, Stream stream, CancellationToken cancellationToken = default);

}
