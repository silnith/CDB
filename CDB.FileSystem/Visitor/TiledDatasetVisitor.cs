using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Silnith.CDB.FileSystem.Visitor;

/// <summary>
/// Visits a directory hierarchy described in 3.6.2. Tiled Dataset Directory Structure,
/// and calls a delegate for every file that matches the expected
/// structure and name.
/// </summary>
public class TiledDatasetVisitor : VisitorBase
{
    /// <summary>
    /// A pattern that matches level 5 directories.
    /// </summary>
    /// <remarks>
    /// <list type="table">
    /// <listheader><term>Capture Group</term><description>Meaning</description></listheader>
    /// <item><term>up</term><description>Parseable as an integer.</description></item>
    /// </list>
    /// </remarks>
    private static Regex UpDirPattern
    {
        get;
    } = new(@"^U(?<up>\d+)$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking);

    private readonly ILogger<TiledDatasetVisitor> logger;

    private readonly LevelOfDetailDirectoryWalker levelOfDetailDirectoryWalker;

    /// <summary>
    /// A constructor for dependency injection.
    /// </summary>
    /// <param name="logger">A logger.</param>
    /// <param name="levelOfDetailDirectoryWalker">A level of detail directory walker.</param>
    public TiledDatasetVisitor(ILogger<TiledDatasetVisitor> logger,
        LevelOfDetailDirectoryWalker levelOfDetailDirectoryWalker)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(levelOfDetailDirectoryWalker);

        this.logger = logger;
        this.levelOfDetailDirectoryWalker = levelOfDetailDirectoryWalker;
    }

    /// <summary>
    /// Enumerates all recognized files in a CDB <c>Tiles</c> directory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// See OGC CDB Core Standard: Volume 1,
    /// Section 3.6. CDB Tiled Datasets
    /// </para>
    /// </remarks>
    /// <param name="cdbDir">The CDB root directory.</param>
    /// <returns>An enumeration of all recognized files.</returns>
    public IEnumerable<(ICDBIdentifier, Stream)> EnumerateFiles(DirectoryInfo cdbDir)
    {
        DirectoryInfo tilesDir = new(Path.Combine(cdbDir.FullName, "Tiles"));
        if (!tilesDir.Exists)
        {
            logger.LogTrace("{Directory} does not exist.  Skipping.",
                tilesDir);
            yield break;
        }

        FileStreamOptions options = new()
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read,
            Options = FileOptions.SequentialScan | FileOptions.Asynchronous,
        };
        foreach (DirectoryInfo latitudeDir in tilesDir.EnumerateDirectories("*", enumerationOptions))
        {
            Match latitudeMatch = Latitude.TiledDatasetDirectoryPattern.Match(latitudeDir.Name);
            if (!latitudeMatch.Success)
            {
                logger.LogTrace("{Directory} is not a Latitude directory.  Skipping.",
                    latitudeDir);
                continue;
            }
            Latitude latitudeFromDirectory = Latitude.FromTiledDatasetDirectoryMatch(latitudeMatch);

            foreach (DirectoryInfo longitudeDir in latitudeDir.EnumerateDirectories("*", enumerationOptions))
            {
                Match longitudeMatch = Longitude.TiledDatasetDirectoryPattern.Match(longitudeDir.Name);
                if (!longitudeMatch.Success)
                {
                    logger.LogTrace("{Directory} is not a Longitude directory.  Skipping.",
                        longitudeDir);
                    continue;
                }
                Longitude longitudeFromDirectory = Longitude.FromTiledDatasetDirectoryMatch(longitudeMatch);

                foreach (DirectoryInfo datasetDir in longitudeDir.EnumerateDirectories("*", enumerationOptions))
                {
                    Match datasetMatch = Dataset.DirectoryPattern.Match(datasetDir.Name);
                    if (!datasetMatch.Success)
                    {
                        logger.LogTrace("{Directory} is not a Dataset directory.  Skipping.",
                            datasetDir);
                        continue;
                    }
                    Dataset datasetFromDirectory = Dataset.FromDirectoryMatch(datasetMatch);

                    foreach ((LevelOfDetail? levelOfDetailFromDirectory, DirectoryInfo lodDir) in levelOfDetailDirectoryWalker.EnumerateTiledDatasetDirectories(datasetDir))
                    {
                        foreach (DirectoryInfo upDir in lodDir.EnumerateDirectories("*", enumerationOptions))
                        {
                            Match upMatch = UpDirPattern.Match(upDir.Name);
                            if (!upMatch.Success)
                            {
                                logger.LogTrace("{Directory} is not an UREF directory.  Skipping.",
                                    upDir);
                                continue;
                            }
                            int upFromDirectory = int.Parse(upMatch.Groups["up"].Value, CultureInfo.InvariantCulture);

                            foreach (FileInfo file in upDir.EnumerateFiles("*", enumerationOptions))
                            {
                                Match tileMatch = Tile.TiledDatasetFilenamePattern.Match(file.Name);
                                if (!tileMatch.Success)
                                {
                                    continue;
                                }
                                Tile tile = Tile.FromTiledDatasetFilenameMatch(tileMatch);

                                if (latitudeFromDirectory != tile.LatitudeValue)
                                {
                                    logger.LogError("Latitude from directory level 1 {DirectoryLatitude} does not match file {FileLatitude}.", latitudeFromDirectory, tile.LatitudeValue);
                                }
                                if (longitudeFromDirectory != tile.LongitudeValue)
                                {
                                    logger.LogError("Longitude from directory level 2 {DirectoryLongitude} does not match file {FileLongitude}.", longitudeFromDirectory, tile.LongitudeValue);
                                }
                                if (datasetFromDirectory != tile.DatasetValue)
                                {
                                    logger.LogError("Dataset from directory level 3 {DirectoryDataset} does not match file {FileDataset}.", datasetFromDirectory, tile.DatasetValue);
                                }
                                if (levelOfDetailFromDirectory is not null && levelOfDetailFromDirectory != tile.Level)
                                {
                                    logger.LogError("Level of detail from directory level 4 {DirectoryLod} does not match file {FileLod}", levelOfDetailFromDirectory, tile.Level);
                                }
                                if (levelOfDetailFromDirectory is null && tile.Level.Value >= 0)
                                {
                                    logger.LogError("File {Tile} should be in level 4 directory {LevelOfDetailDirectory}.", tile, $"L{tile.Level.Value:D2}");
                                }
                                if (upFromDirectory != tile.Up)
                                {
                                    logger.LogError("Up value from directory level 5 {DirectoryUref} does not match file {FileUref}", upFromDirectory, tile.Up);
                                }

                                using FileStream fileStream = new(file.FullName, options);
                                yield return (tile, fileStream);

                                if (CultureInfo.InvariantCulture.CompareInfo.Compare(tile.FileType, "zip", CompareOptions.IgnoreCase) == 0)
                                {
                                    using ZipArchive zipArchive = ZipFile.OpenRead(file.FullName);
                                    foreach (var entry in zipArchive.Entries)
                                    {
                                        /*
                                         * Unfortunately, file names that match the "feature code" pattern
                                         * can also match the "texture name" pattern, because it just groups
                                         * everything after the known stuff as the name of a texture.
                                         * Therefore, order is crucial here.
                                         */
                                        Match featureMatch = TileArchivedFeature.ArchivedFilenamePattern.Match(entry.Name);
                                        if (featureMatch.Success)
                                        {
                                            TileArchivedFeature tileArchivedFeature = TileArchivedFeature.FromArchivedFilenameMatch(featureMatch);

                                            using Stream stream = entry.Open();
                                            yield return (tileArchivedFeature, stream);
                                        }
                                        else
                                        {
                                            Match textureMatch = TileArchivedTexture.ArchivedFilenamePattern.Match(entry.Name);
                                            if (textureMatch.Success)
                                            {
                                                TileArchivedTexture tileArchivedTexture = TileArchivedTexture.FromArchivedFilenameMatch(textureMatch);

                                                using Stream stream = entry.Open();
                                                yield return (tileArchivedTexture, stream);
                                            }
                                            else
                                            {
                                                // Unrecognized file, ignore it.
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
