using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Silnith.CDB.FileSystem.Visitor;

/// <summary>
/// Visits all the files in the CDB <c>Metadata</c> directory.
/// </summary>
/// <remarks>
/// <para>
/// See OGC CDB Core Standard: Volume 1,
/// Section 3.1.1. Metadata Directory
/// </para>
/// </remarks>
public class MetadataVisitor : VisitorBase
{
    /// <summary>
    /// The metadata files defined in the standard.
    /// Also recognized are files whose names begin with the prefix "Lights_".
    /// </summary>
    private static readonly SortedSet<string> recognizedMetadata = new()
    {
        "Global_Spatial",
        "Datasets",
        "Lights",
        "Model_Components",
        "Materials",
        "Defaults",
        "Version",
        "CDB_Attributes",
        "Geomatics_Attributes",
        "Vendor_Attributes",
        "Configuration",
    };

    private readonly ILogger<MetadataVisitor> logger;

    /// <summary>
    /// A constructor intended for dependency injection.
    /// </summary>
    /// <param name="logger">A logger.</param>
    public MetadataVisitor(ILogger<MetadataVisitor> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
    }

    /// <summary>
    /// Enumerates all recognized files in a CDB <c>Metadata</c> directory.
    /// </summary>
    /// <param name="cdbDir">The CDB root directory.</param>
    /// <returns>An enumeration of all recognized files.</returns>
    public IEnumerable<(ICDBIdentifier, Stream)> EnumerateFiles(DirectoryInfo cdbDir)
    {
        DirectoryInfo metadataDir = new(Path.Combine(cdbDir.FullName, "Metadata"));
        if (!metadataDir.Exists)
        {
            logger.LogTrace("{Directory} does not exist.  Skipping.", metadataDir);
            yield break;
        }

        FileStreamOptions options = new()
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read,
            Options = FileOptions.SequentialScan | FileOptions.Asynchronous,
        };
        // No reason to enumerate child directories, just files.
        foreach (var file in metadataDir.EnumerateFiles("*", enumerationOptions))
        {
            string name = file.Name.Remove(file.Name.Length - file.Extension.Length);
            string extension = file.Extension.Substring(1);
            Metadata metadata = new(name, extension);

            using FileStream fileStream = new(file.FullName, options);
            yield return (metadata, fileStream);
        }
    }
}
