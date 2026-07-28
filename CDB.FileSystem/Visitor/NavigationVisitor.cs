using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Silnith.CDB.FileSystem.Visitor;

/// <summary>
/// Visits all the files in the global Navigation dataset.
/// </summary>
public class NavigationVisitor : VisitorBase
{
    private readonly ILogger<NavigationVisitor> logger;

    /// <summary>
    /// A constructor intended for dependency injection.
    /// </summary>
    /// <param name="logger">A logger.</param>
    public NavigationVisitor(ILogger<NavigationVisitor> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
    }

    /// <summary>
    /// Enumerates all recognized files in a CDB <c>Navigation</c> directory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// See OGC CDB Core Standard: Volume 1,
    /// Section 3.7. Navigation Library Dataset
    /// </para>
    /// </remarks>
    /// <param name="cdbDir">The CDB root directory.</param>
    /// <returns>An enumeration of all recognized files.</returns>
    public IEnumerable<(ICDBIdentifier, Stream)> EnumerateFiles(DirectoryInfo cdbDir)
    {
        DirectoryInfo navigationDir = new(Path.Combine(cdbDir.FullName, "Navigation"));
        if (!navigationDir.Exists)
        {
            logger.LogTrace("{Directory} does not exist.  Skipping.",
                navigationDir);
            yield break;
        }

        FileStreamOptions options = new()
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read,
            Options = FileOptions.SequentialScan | FileOptions.Asynchronous,
        };
        foreach (DirectoryInfo datasetDir in navigationDir.EnumerateDirectories("*", enumerationOptions))
        {
            Match datasetMatch = Dataset.DirectoryPattern.Match(datasetDir.Name);
            if (!datasetMatch.Success)
            {
                logger.LogTrace("{Directory} is not a Dataset directory.  Skipping.",
                    datasetDir);
                continue;
            }
            Dataset datasetFromDirectory = Dataset.FromDirectoryMatch(datasetMatch);
            string datasetName = datasetMatch.Groups["name"].Value;
            if (datasetFromDirectory.Value != 400
                || datasetName != "NavData")
            {
                logger.LogWarning("Dataset from directory {DatasetDirectory} is not 400", datasetDir);
            }

            foreach (FileInfo file in datasetDir.EnumerateFiles("*", enumerationOptions))
            {
                Match match = Navigation.FilenamePattern.Match(file.Name);
                if (!match.Success)
                {
                    logger.LogTrace("{File} is not a Navigation file.",
                        file);
                    continue;
                }
                Navigation navigation = Navigation.FromFilenameMatch(match);

                if (datasetFromDirectory != navigation.Dataset)
                {
                    logger.LogWarning("Dataset from directory {DirectoryDataset} does not match file {FileDataset}",
                        datasetFromDirectory, navigation.Dataset);
                }

                using FileStream fileStream = new(file.FullName, options);
                yield return (navigation, fileStream);
            }
        }
    }
}
