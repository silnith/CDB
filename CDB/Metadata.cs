using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Silnith.CDB;

/// <summary>
/// Metadata as described in 3.1.1. Metadata Directory.
/// </summary>
/// <param name="Name">The metadata name.</param>
/// <param name="FileType">The file type.</param>
public record Metadata(string Name, string FileType) : ICDBFileIdentifier
{
    /// <inheritdoc/>
    public Stream? ReadFromCDB(ICDB cdb)
    {
        return cdb.ReadMetadata(this);
    }

    /// <inheritdoc/>
    public Task<Stream?> ReadFromCDBAsync(ICDB cdb, CancellationToken cancellationToken)
    {
        return cdb.ReadMetadataAsync(this, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteToCDB(ICDB cdb, Stream stream)
    {
        cdb.WriteMetadata(this, stream);
    }

    /// <inheritdoc/>
    public Task WriteToCDBAsync(ICDB cdb, Stream stream, CancellationToken cancellationToken)
    {
        return cdb.WriteMetadataAsync(this, stream, cancellationToken);
    }

    /// <inheritdoc/>
    public string RelativePath => "Metadata";

    /// <inheritdoc/>
    public string Filename => $"{Name}.{FileType}";

}
