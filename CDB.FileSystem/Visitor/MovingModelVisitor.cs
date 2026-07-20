using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Silnith.CDB.FileSystem.Visitor;

/// <summary>
/// Visits all the files in the Moving Models Datasets.
/// </summary>
/// <remarks>
/// <para>
/// See OGC CDB Core Standard: Volume 1,
/// Section 3.5. MModel Library Datasets.
/// </para>
/// </remarks>
public class MovingModelVisitor : VisitorBase
{
    private readonly ILogger<MovingModelVisitor> logger;

    private readonly DISEntityDirectoryWalker disEntityDirectoryWalker;

    private readonly TextureDirectoryWalker textureDirectoryWalker;

    private readonly LevelOfDetailDirectoryWalker levelOfDetailDirectoryWalker;

    /// <summary>
    /// A constructor for dependency injection.
    /// </summary>
    /// <param name="logger">A logger.</param>
    /// <param name="levelOfDetailDirectoryWalker">A level of detail directory walker.</param>
    /// <param name="textureDirectoryWalker">A texture directory walker.</param>
    /// <param name="disEntityDirectoryWalker">A DIS entity directory walker.</param>
    public MovingModelVisitor(ILogger<MovingModelVisitor> logger,
        DISEntityDirectoryWalker disEntityDirectoryWalker,
        TextureDirectoryWalker textureDirectoryWalker,
        LevelOfDetailDirectoryWalker levelOfDetailDirectoryWalker)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(disEntityDirectoryWalker);
        ArgumentNullException.ThrowIfNull(textureDirectoryWalker);
        ArgumentNullException.ThrowIfNull(levelOfDetailDirectoryWalker);

        this.logger = logger;
        this.disEntityDirectoryWalker = disEntityDirectoryWalker;
        this.textureDirectoryWalker = textureDirectoryWalker;
        this.levelOfDetailDirectoryWalker = levelOfDetailDirectoryWalker;
    }

    /// <summary>
    /// Walks the <c>MModel</c> directory and visits all recognized files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// See OGC CDB Core Standard: Volume 1,
    /// Section 3.5. MModel Library Datasets.
    /// </para>
    /// </remarks>
    /// <param name="cdbDir">The CDB root directory.</param>
    /// <param name="processMovingModelFile">The action to take for every moving model file.</param>
    /// <param name="processMovingModelLodFile">The action to take for every moving model level of detail file.</param>
    /// <param name="processTextureFile">The action to take for every texture file.</param>
    /// <param name="processTextureLodFile">The action to take for every texture level of detail file.</param>
    public void VisitMovingModels(DirectoryInfo cdbDir,
        Action<MovingModel, FileInfo> processMovingModelFile,
        Action<MovingModelLod, FileInfo> processMovingModelLodFile,
        Action<Texture, FileInfo> processTextureFile,
        Action<TextureLod, FileInfo> processTextureLodFile)
    {
        DirectoryInfo mModelDir = new(Path.Combine(cdbDir.FullName, "MModel"));
        if (!mModelDir.Exists)
        {
            logger.LogTrace("{Directory} does not exist.  Skipping.",
                mModelDir);
            return;
        }

        foreach (DirectoryInfo datasetDir in mModelDir.EnumerateDirectories("*", enumerationOptions))
        {
            Match datasetMatch = Dataset.DirectoryPattern.Match(datasetDir.Name);
            if (!datasetMatch.Success)
            {
                logger.LogTrace("{Directory} is not a Dataset directory.  Skipping.",
                    datasetDir);
                continue;
            }
            Dataset datasetFromDirectory = Dataset.FromDirectoryMatch(datasetMatch);

            // See 3.5.1. MModel Directory Structure 1: Geometry and Descriptor
            disEntityDirectoryWalker.WalkDirectories(datasetDir, (disEntityType, entityDir) =>
            {
                // See 3.5.1.1. MModelGeometry Naming Convention
                // See 3.5.1.2. MModelDescriptor Naming Convention
                foreach (FileInfo file in entityDir.EnumerateFiles("*", enumerationOptions))
                {
                    Match movingModelMatch = MovingModel.FilenamePattern.Match(file.Name);
                    if (!movingModelMatch.Success)
                    {
                        logger.LogWarning("{File} is not a Moving Model.  Skipping.",
                            file);
                        continue;
                    }
                    MovingModel movingModel = MovingModel.FromFilenameMatch(movingModelMatch);

                    if (datasetFromDirectory != movingModel.Dataset)
                    {
                        if (movingModel.Dataset.Value == 603 && datasetFromDirectory.Value == 600)
                        {
                            // See 3.5.1.2. MModelDescriptor Naming Convention
                        }
                        else
                        {
                            logger.LogWarning("Dataset from directory {DirectoryDataset} does not match file {FileDataset}",
                                datasetFromDirectory, movingModel.Dataset);
                        }
                    }
                    if (disEntityType != movingModel.MMDC)
                    {
                        logger.LogWarning("DIS entity from directory {DirectoryDISCode} does not match file {FileDISCode}",
                            disEntityType, movingModel.MMDC);
                    }

                    processMovingModelFile(movingModel, file);
                }

                // See 3.5.3. MModel Directory Structure 3: Signature
                levelOfDetailDirectoryWalker.WalkModelGeometryDirectories(entityDir, (lod, lodDir) =>
                {
                    foreach (FileInfo file in lodDir.EnumerateFiles("*", enumerationOptions))
                    {
                        // See 3.5.3.1. Naming Convention
                        Match movingModelLodMatch = MovingModelLod.FilenamePattern.Match(file.Name);
                        if (!movingModelLodMatch.Success)
                        {
                            logger.LogTrace("{File} is not a Moving Model Level of Detail.  Skipping.",
                                file);
                            continue;
                        }
                        MovingModelLod movingModelLod = MovingModelLod.FromFilenameMatch(movingModelLodMatch);

                        if (datasetFromDirectory != movingModelLod.Dataset)
                        {
                            logger.LogWarning("Dataset from directory {DirectoryDataset} does not match file {FileDataset}",
                                datasetFromDirectory, movingModelLod.Dataset);
                        }
                        if (disEntityType != movingModelLod.MMDC)
                        {
                            logger.LogWarning("DIS entity from directory {DirectoryDISCode} does not match file {FileDISCode}",
                                disEntityType, movingModelLod.MMDC);
                        }
                        if (lod != movingModelLod.LevelOfDetail)
                        {
                            logger.LogWarning("Level of detail from directory {DirectoryLod} does not match file {FileLod}",
                                lod, movingModelLod.LevelOfDetail);
                        }

                        processMovingModelLodFile(movingModelLod, file);
                    }
                });
            });
            // See 3.5.2. MModel Directory Structure 2: Texture, Material, and CMT
            textureDirectoryWalker.WalkDirectories(datasetDir, (textureName, textureDir) =>
            {
                foreach (FileInfo file in textureDir.EnumerateFiles("*", enumerationOptions))
                {
                    // See 3.5.2.1. MModelTexture Naming Convention
                    // See 3.5.2.2. MModelMaterial Naming Convention
                    Match textureLodMatch = TextureLod.FilenamePattern.Match(file.Name);
                    if (textureLodMatch.Success)
                    {
                        TextureLod textureLod = TextureLod.FromFilenameMatch(textureLodMatch);

                        if (datasetFromDirectory != textureLod.Dataset)
                        {
                            if (textureLod.Dataset.Value == 604 && datasetFromDirectory.Value == 601)
                            {
                                // See 3.5.2.2. MModelMaterial Naming Convention
                            }
                            else
                            {
                                logger.LogWarning("Dataset from directory {DirectoryDataset} does not match file {FileDataset}",
                                datasetFromDirectory, textureLod.Dataset);
                            }
                        }
                        if (textureName != textureLod.Name)
                        {
                            logger.LogWarning("Level of detail from directory {DirectoryTexture} does not match file {FileTexture}",
                                textureName, textureLod.Name);
                        }

                        processTextureLodFile(textureLod, file);
                    }
                    else
                    {
                        // See 3.5.2.3. MModelCMT Naming Convention
                        Match textureMatch = Texture.FilenamePattern.Match(file.Name);
                        if (textureMatch.Success)
                        {
                            Texture texture = Texture.FromFilenameMatch(textureMatch);

                            if (datasetFromDirectory != texture.Dataset)
                            {
                                if (texture.Dataset.Value == 605 && datasetFromDirectory.Value == 601)
                                {
                                    // See 3.5.2.3. MModelCMT Naming Convention
                                }
                                else
                                {
                                    logger.LogWarning("Dataset from directory {DirectoryDataset} does not match file {FileDataset}",
                                    datasetFromDirectory, texture.Dataset);
                                }
                            }
                            if (textureName != texture.Name)
                            {
                                logger.LogWarning("Texture name from directory {DirectoryTexture} does not match file {FileTexture}",
                                    textureName, texture.Name);
                            }

                            processTextureFile(texture, file);
                        }
                        else
                        {
                            logger.LogWarning("{File} is not a texture.  Skipping.",
                                file);
                            continue;
                        }
                    }
                }
            });
        }
    }
}
