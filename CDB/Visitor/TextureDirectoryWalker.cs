using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Silnith.CDB.Visitor;

/// <summary>
/// Walks a directory hierarchy described in 3.3.8.4. Texture Name,
/// and calls a delegate for every leaf directory that matches the expected
/// structure.
/// </summary>
public class TextureDirectoryWalker : VisitorBase
{
    private readonly ILogger<TextureDirectoryWalker> logger;

    /// <summary>
    /// A constructor for dependency injection.
    /// </summary>
    /// <param name="logger">A logger.</param>
    public TextureDirectoryWalker(ILogger<TextureDirectoryWalker> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
    }

    /// <summary>
    /// Walks a directory tree matching that described in the CDB specification
    /// volume 1, Section 3.3.8.4. Texture Name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This walks a directory hierarchy conforming to the pattern <c>/H/O/house/</c>.
    /// </para>
    /// </remarks>
    /// <param name="dir">The root directory containing child directories of the form <c>/H/O/house/</c>.</param>
    /// <param name="processTextureDirectory">The action to take for every leaf directory in the directory hierarchy.
    /// The first parameter is the texture name from the directory hierarchy.</param>
    public void WalkDirectories(DirectoryInfo dir,
        Action<string, DirectoryInfo> processTextureDirectory)
    {
        foreach (DirectoryInfo level1Dir in dir.EnumerateDirectories("*", enumerationOptions))
        {
            Match level1Match = Texture.PrefixPattern.Match(level1Dir.Name);
            if (!level1Match.Success)
            {
                logger.LogTrace("{Directory} is not the first level of a texture hierarchy.  Skipping.",
                    level1Dir);
                continue;
            }
            string level1Prefix = level1Match.Groups["prefix"].Value;

            foreach (DirectoryInfo level2Dir in level1Dir.EnumerateDirectories("*", enumerationOptions))
            {
                Match level2Match = Texture.PrefixPattern.Match(level2Dir.Name);
                if (!level2Match.Success)
                {
                    logger.LogTrace("{Directory} is not the second level of a texture hierarchy.  Skipping.",
                        level2Dir);
                    continue;
                }
                string level2Prefix = level2Match.Groups["prefix"].Value;

                foreach (DirectoryInfo textureDir in level2Dir.EnumerateDirectories("*", enumerationOptions))
                {
                    string textureName = textureDir.Name;

                    if (!textureName.StartsWith(level1Prefix, true, CultureInfo.InvariantCulture)
                        || !textureName.StartsWith(level1Prefix + level2Prefix, true, CultureInfo.InvariantCulture))
                    {
                        logger.LogWarning("Unexpected directory in texture directory hierarchy.  {TextureName} does not begin with {Prefix}.",
                            textureName, level1Prefix + level2Prefix);
                    }

                    processTextureDirectory(textureName, textureDir);
                }
            }
        }
    }
}
