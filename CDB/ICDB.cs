using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Silnith.CDB;

/// <summary>
/// A shared interface for individual instances of a CDB data store.
/// </summary>
/// <remarks>
/// <para>
/// This represents one single storage location as described in the OGC CDB standard.
/// Typically this would be a filesystem hierarchy rooted in a single directory.
/// Alternate implementations are possible, however, that can translate the standard
/// file paths and names into keys for other storage mechanisms.
/// </para>
/// <para>
/// A list of CDB versions would consists of multiple instances of this interface.
/// The file replacement mechanism would involve querying multiple instances of
/// this interface.
/// </para>
/// </remarks>
public interface ICDB : IDisposable
{

    /// <summary>
    /// Tries to read a file out of the CDB.
    /// If the file was found, runs <paramref name="fileFoundAction"/> on its contents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <paramref name="filePathAndName"/> should always begin with one of
    /// the known root directories.  These are:
    /// </para>
    /// <list type="bullet">
    /// <item><term><c>/Metadata/</c></term></item>
    /// <item><term><c>/GTModel/</c></term></item>
    /// <item><term><c>/MModel/</c></term></item>
    /// <item><term><c>/Tiles/</c></term></item>
    /// <item><term><c>/Navigation/</c></term></item>
    /// </list>
    /// </remarks>
    /// <param name="filePathAndName">The relative path and filename of the file to read.
    /// The path should be relative to the CDB root.</param>
    /// <param name="fileFoundAction">The action to run if the file is found.
    /// The stream will be automatically closed after the action returns or
    /// throws an exception.</param>
    /// <returns><see langword="true"/> if the file was found.</returns>
    public bool TryReadFile(string filePathAndName, Action<Stream> fileFoundAction);

    /// <summary>
    /// Tries to read a file out of the CDB.
    /// If the file was found, runs <paramref name="fileFoundAsyncAction"/> on its contents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <paramref name="filePathAndName"/> should always begin with one of
    /// the known root directories.  These are:
    /// </para>
    /// <list type="bullet">
    /// <item><term><c>/Metadata/</c></term></item>
    /// <item><term><c>/GTModel/</c></term></item>
    /// <item><term><c>/MModel/</c></term></item>
    /// <item><term><c>/Tiles/</c></term></item>
    /// <item><term><c>/Navigation/</c></term></item>
    /// </list>
    /// </remarks>
    /// <param name="filePathAndName">The relative path and filename of the file to read.
    /// The path should be relative to the CDB root.</param>
    /// <param name="fileFoundAsyncAction">The action to run if the file is found.
    /// The stream will be automatically closed after the action returns or
    /// throws an exception.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see langword="true"/> if the file was found.</returns>
    public Task<bool> TryReadFileAsync(string filePathAndName,
        Func<Stream, CancellationToken, Task> fileFoundAsyncAction,
        CancellationToken cancellationToken);

}
