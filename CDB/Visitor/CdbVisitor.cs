using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Silnith.CDB.Visitor;

/// <summary>
/// Visits all the files in a CDB data store.
/// </summary>
public class CdbVisitor : VisitorBase
{
    private readonly ILogger<CdbVisitor> logger;
    private readonly MetadataVisitor metadataVisitor;
    private readonly GeotypicalModelVisitor geotypicalModelVisitor;
    private readonly MovingModelVisitor movingModelVisitor;
    private readonly TiledDatasetVisitor tiledDatasetVisitor;
    private readonly NavigationVisitor navigationVisitor;

    /// <summary>
    /// A constructor intended for dependency injection.
    /// </summary>
    /// <param name="logger">A logger.</param>
    /// <param name="metadataVisitor">A visitor for the <c>Metadata</c> directory.</param>
    /// <param name="geotypicalModelVisitor">A visitor for the <c>GTModel</c> directory.</param>
    /// <param name="movingModelVisitor">A visitor for the <c>MModel</c> directory.</param>
    /// <param name="tiledDatasetVisitor">A visitor for the <c>Tiles</c> directory.</param>
    /// <param name="navigationVisitor">A visitor for the <c>Navigation</c> directory.</param>
    public CdbVisitor(ILogger<CdbVisitor> logger,
        MetadataVisitor metadataVisitor,
        GeotypicalModelVisitor geotypicalModelVisitor,
        MovingModelVisitor movingModelVisitor,
        TiledDatasetVisitor tiledDatasetVisitor,
        NavigationVisitor navigationVisitor)
    {
        this.logger = logger;
        this.metadataVisitor = metadataVisitor;
        this.geotypicalModelVisitor = geotypicalModelVisitor;
        this.movingModelVisitor = movingModelVisitor;
        this.tiledDatasetVisitor = tiledDatasetVisitor;
        this.navigationVisitor = navigationVisitor;
    }

    /// <summary>
    /// Walks the CDB and visits all recognized files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// See OGC CDB Core Standard: Volume 1,
    /// Section 3.1. Top Level CDB Model/Structure Description
    /// </para>
    /// </remarks>
    /// <param name="cdbDir">The root directory of the CDB data store.</param>
    /// <param name="processMetadataFile">The action to take for each metadata file.</param>
    /// <param name="processTextureFile">The action to take for each texture file.</param>
    /// <param name="processTextureLodFile">The action to take for each texture level of detail file.</param>
    /// <param name="processGeotypicalModelFile">The action to take for each geotypical model file.</param>
    /// <param name="processGeotypicalModelLodFile">The action to take for each geotypical model level of detail file.</param>
    /// <param name="processMovingModelFile">The action to take for each moving model file.</param>
    /// <param name="processMovingModelLodFile">The action to take for each moving model level of detail file.</param>
    /// <param name="processTiledDatasetFile">The action to take for each tiled dataset file.</param>
    /// <param name="processNavigationFile">The action to take for each navigation file.</param>
    public void WalkDataStore(DirectoryInfo cdbDir,
        Action<Metadata, FileInfo> processMetadataFile,
        Action<Texture, FileInfo> processTextureFile,
        Action<TextureLod, FileInfo> processTextureLodFile,
        Action<GeotypicalModel, FileInfo> processGeotypicalModelFile,
        Action<GeotypicalModelLod, FileInfo> processGeotypicalModelLodFile,
        Action<MovingModel, FileInfo> processMovingModelFile,
        Action<MovingModelLod, FileInfo> processMovingModelLodFile,
        Action<Tile, FileInfo> processTiledDatasetFile,
        Action<Navigation, FileInfo> processNavigationFile)
    {
        logger.LogTrace("Walking Metadata for {CDB}", cdbDir);
        metadataVisitor.VisitMetadata(cdbDir,
            processMetadataFile);
        logger.LogTrace("Walking GTModel for {CDB}", cdbDir);
        geotypicalModelVisitor.VisitGeotypicalModels(cdbDir,
            processGeotypicalModelFile,
            processGeotypicalModelLodFile,
            processTextureFile,
            processTextureLodFile);
        logger.LogTrace("Walking MModel for {CDB}", cdbDir);
        movingModelVisitor.VisitMovingModels(cdbDir,
            processMovingModelFile,
            processMovingModelLodFile,
            processTextureFile,
            processTextureLodFile);
        logger.LogTrace("Walking Tiles for {CDB}", cdbDir);
        tiledDatasetVisitor.VisitTiles(cdbDir,
            processTiledDatasetFile);
        logger.LogTrace("Walking Navigation for {CDB}", cdbDir);
        navigationVisitor.VisitNavigationDatasets(cdbDir,
            processNavigationFile);
        logger.LogTrace("Finished walking CDB data store {CDB}", cdbDir);
    }
}
