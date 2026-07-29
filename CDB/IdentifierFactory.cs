using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Silnith.CDB;

/// <summary>
/// A factory for parsing CDB identifiers from relative paths.
/// </summary>
public static class IdentifierFactory
{
    private static Regex UpDirPattern
    {
        get;
    } = new(@"^U(?<up>\d+)$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking);

    /// <summary>
    /// Parses the relative path into the CDB and returns an identifier if the
    /// path matches a valid file name.
    /// </summary>
    /// <param name="relativePathAndName">The path relative to the root of the CDB.</param>
    /// <returns>A CDB identifier, or <see langword="null"/>.</returns>
    public static ICDBIdentifier? ParseIdentifier(string relativePathAndName)
    {
        string[] pathComponents = relativePathAndName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (pathComponents.Length < 2)
        {
            return null;
        }
        switch (pathComponents[0].ToLowerInvariant())
        {
            case "metadata":
                {
                    string filename = pathComponents[^1];
                    string extension = Path.GetExtension(filename)[1..];
                    pathComponents[^1] = Path.GetFileNameWithoutExtension(filename);
                    string name = Path.Combine(pathComponents.Skip(1).ToArray());
                    return new Metadata(name, extension);
                }
            case "gtmodel":
                {
                    switch (pathComponents.Length)
                    {
                        case 7:
                            {
                                Match datasetMatch = Dataset.DirectoryPattern.Match(pathComponents[1]);
                                Match categoryMatch = FeatureCode.CategoryDirectoryPattern.Match(pathComponents[2]);
                                Match subcategoryMatch = FeatureCode.SubcategoryDirectoryPattern.Match(pathComponents[3]);
                                Match typeMatch = FeatureCode.TypeDirectoryPattern.Match(pathComponents[4]);
                                Match lodMatch = LevelOfDetail.ModelGeometryDirectoryPattern.Match(pathComponents[5]);
                                Match geotypicalModelLodMatch = GeotypicalModelLod.FilenamePattern.Match(pathComponents[6]);
                                if (datasetMatch.Success
                                    && categoryMatch.Success
                                    && subcategoryMatch.Success
                                    && typeMatch.Success
                                    && lodMatch.Success
                                    && geotypicalModelLodMatch.Success)
                                {
                                    return GeotypicalModelLod.FromFilenameMatch(geotypicalModelLodMatch);
                                }
                                return null;
                            }
                        case 6:
                            {
                                Match datasetMatch = Dataset.DirectoryPattern.Match(pathComponents[1]);
                                Match categoryMatch = FeatureCode.CategoryDirectoryPattern.Match(pathComponents[2]);
                                Match subcategoryMatch = FeatureCode.SubcategoryDirectoryPattern.Match(pathComponents[3]);
                                Match typeMatch = FeatureCode.TypeDirectoryPattern.Match(pathComponents[4]);
                                Match geotypicalModelMatch = GeotypicalModel.FilenamePattern.Match(pathComponents[5]);
                                if (datasetMatch.Success
                                    && categoryMatch.Success
                                    && subcategoryMatch.Success
                                    && typeMatch.Success
                                    && geotypicalModelMatch.Success)
                                {
                                    return GeotypicalModel.FromFilenameMatch(geotypicalModelMatch);
                                }

                                Match textureFirstMatch = Texture.PrefixPattern.Match(pathComponents[2]);
                                Match textureSecondMatch = Texture.PrefixPattern.Match(pathComponents[3]);
                                // assume pathComponents[4]
                                string textureName = pathComponents[4];
                                Match textureLodMatch = TextureLod.FilenamePattern.Match(pathComponents[5]);
                                Match textureMatch = Texture.FilenamePattern.Match(pathComponents[5]);
                                if (datasetMatch.Success
                                    && textureFirstMatch.Success
                                    && textureSecondMatch.Success
                                    && textureName.Length >= 2
                                    && textureName.Substring(0, 1).ToLowerInvariant() == textureFirstMatch.Groups["prefix"].Value.ToLowerInvariant()
                                    && textureName.Substring(1, 1).ToLowerInvariant() == textureSecondMatch.Groups["prefix"].Value.ToLowerInvariant())
                                {
                                    if (textureLodMatch.Success)
                                    {
                                        return TextureLod.FromFilenameMatch(textureLodMatch);
                                    }
                                    else if (textureMatch.Success)
                                    {
                                        return Texture.FromFilenameMatch(textureMatch);
                                    }
                                }
                                return null;
                            }
                        default:
                            return null;
                    }
                }
            case "mmodel":
                {
                    switch (pathComponents.Length)
                    {
                        case 9:
                            {
                                Match datasetMatch = Dataset.DirectoryPattern.Match(pathComponents[1]);
                                Match kindMatch = DISEntity.ParentDirectoryPattern.Match(pathComponents[2]);
                                Match domainMatch = DISEntity.ParentDirectoryPattern.Match(pathComponents[3]);
                                Match countryMatch = DISEntity.ParentDirectoryPattern.Match(pathComponents[4]);
                                Match categoryMatch = DISEntity.ParentDirectoryPattern.Match(pathComponents[5]);
                                Match disMatch = DISEntity.DirectoryPattern.Match(pathComponents[6]);
                                Match lodMatch = LevelOfDetail.ModelGeometryDirectoryPattern.Match(pathComponents[7]);
                                Match movingModelLodMatch = MovingModelLod.FilenamePattern.Match(pathComponents[8]);
                                if (datasetMatch.Success
                                    && kindMatch.Success
                                    && domainMatch.Success
                                    && countryMatch.Success
                                    && categoryMatch.Success
                                    && disMatch.Success
                                    && lodMatch.Success
                                    && movingModelLodMatch.Success)
                                {
                                    return MovingModelLod.FromFilenameMatch(movingModelLodMatch);
                                }
                                return null;
                            }
                        case 8:
                            {
                                Match datasetMatch = Dataset.DirectoryPattern.Match(pathComponents[1]);
                                Match kindMatch = DISEntity.ParentDirectoryPattern.Match(pathComponents[2]);
                                Match domainMatch = DISEntity.ParentDirectoryPattern.Match(pathComponents[3]);
                                Match countryMatch = DISEntity.ParentDirectoryPattern.Match(pathComponents[4]);
                                Match categoryMatch = DISEntity.ParentDirectoryPattern.Match(pathComponents[5]);
                                Match disMatch = DISEntity.DirectoryPattern.Match(pathComponents[6]);
                                Match movingModelMatch = MovingModel.FilenamePattern.Match(pathComponents[7]);
                                if (datasetMatch.Success
                                    && kindMatch.Success
                                    && domainMatch.Success
                                    && countryMatch.Success
                                    && categoryMatch.Success
                                    && disMatch.Success
                                    && movingModelMatch.Success)
                                {
                                    return MovingModel.FromFilenameMatch(movingModelMatch);
                                }
                                return null;
                            }
                        case 6:
                            {
                                Match datasetMatch = Dataset.DirectoryPattern.Match(pathComponents[1]);
                                Match textureFirstMatch = Texture.PrefixPattern.Match(pathComponents[2]);
                                Match textureSecondMatch = Texture.PrefixPattern.Match(pathComponents[3]);
                                // assume pathComponents[4]
                                string textureName = pathComponents[4];
                                Match textureLodMatch = TextureLod.FilenamePattern.Match(pathComponents[5]);
                                Match textureMatch = Texture.FilenamePattern.Match(pathComponents[5]);
                                if (datasetMatch.Success
                                    && textureFirstMatch.Success
                                    && textureSecondMatch.Success
                                    && textureName.Length >= 2
                                    && textureName.Substring(0, 1).ToLowerInvariant() == textureFirstMatch.Groups["prefix"].Value.ToLowerInvariant()
                                    && textureName.Substring(1, 1).ToLowerInvariant() == textureSecondMatch.Groups["prefix"].Value.ToLowerInvariant())
                                {
                                    if (textureLodMatch.Success)
                                    {
                                        return TextureLod.FromFilenameMatch(textureLodMatch);
                                    }
                                    else if (textureMatch.Success)
                                    {
                                        return Texture.FromFilenameMatch(textureMatch);
                                    }
                                }
                                return null;
                            }
                        default:
                            return null;
                    }
                }
            case "tiles":
                {
                    if (pathComponents.Length == 7)
                    {
                        Match latitudeMatch = Latitude.TiledDatasetDirectoryPattern.Match(pathComponents[1]);
                        Match longitudeMatch = Longitude.TiledDatasetDirectoryPattern.Match(pathComponents[2]);
                        Match datasetMatch = Dataset.DirectoryPattern.Match(pathComponents[3]);
                        Match lodMatch = LevelOfDetail.TiledDatasetDirectoryPattern.Match(pathComponents[4]);
                        Match lodCoarseMatch = LevelOfDetail.TiledDatasetCoarsePattern.Match(pathComponents[4]);
                        Match upMatch = UpDirPattern.Match(pathComponents[5]);
                        Match tileMatch = Tile.TiledDatasetFilenamePattern.Match(pathComponents[6]);
                        if (latitudeMatch.Success
                            && longitudeMatch.Success
                            && datasetMatch.Success
                            && (lodMatch.Success || lodCoarseMatch.Success)
                            && upMatch.Success
                            && tileMatch.Success)
                        {
                            return Tile.FromTiledDatasetFilenameMatch(tileMatch);
                        }
                    }
                    return null;
                }
            case "navigation":
                {
                    if (pathComponents.Length == 3)
                    {
                        Match datasetMatch = Dataset.DirectoryPattern.Match(pathComponents[1]);
                        Match navigationMatch = Navigation.FilenamePattern.Match(pathComponents[2]);
                        if (datasetMatch.Success
                            && navigationMatch.Success)
                        {
                            return Navigation.FromFilenameMatch(navigationMatch);
                        }
                    }
                    return null;
                }
            default:
                return null;
        }
    }
}
