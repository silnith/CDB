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
    /// Walks the <c>Metadata</c> directory and visits all files.
    /// </summary>
    /// <param name="cdbDir">The CDB root directory.</param>
    /// <param name="processMetadataFile">The action to take for every metadata file.</param>
    public void VisitMetadata(DirectoryInfo cdbDir,
        Action<Metadata, FileInfo> processMetadataFile)
    {
        DirectoryInfo metadataDir = new(Path.Combine(cdbDir.FullName, "Metadata"));
        if (!metadataDir.Exists)
        {
            logger.LogTrace("{Directory} does not exist.  Skipping.", metadataDir);
            return;
        }

        // No reason to enumerate child directories, just files.
        foreach (var file in metadataDir.EnumerateFiles("*", enumerationOptions))
        {
            string name = file.Name.Remove(file.Name.Length - file.Extension.Length);
            string extension = file.Extension.Substring(1);
            Metadata metadata = new(name, extension);

            processMetadataFile(metadata, file);
        }
    }
}
