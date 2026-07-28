using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;

namespace Silnith.CDB.FileSystem.Visitor;

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
    /// Enumerates all recognized files in a CDB.
    /// </summary>
    /// <remarks>
    /// <para>
    /// See OGC CDB Core Standard: Volume 1,
    /// Section 3.1. Top Level CDB Model/Structure Description
    /// </para>
    /// </remarks>
    /// <param name="cdbDir">The CDB root directory.</param>
    /// <returns>An enumeration of all recognized files.</returns>
    public IEnumerable<(ICDBIdentifier, Stream)> EnumerateFiles(DirectoryInfo cdbDir)
    {
        logger.LogTrace("Walking Metadata for {CDB}", cdbDir);
        foreach ((ICDBIdentifier, Stream) tuple in metadataVisitor.EnumerateFiles(cdbDir))
        {
            yield return tuple;
        }
        logger.LogTrace("Walking GTModel for {CDB}", cdbDir);
        foreach ((ICDBIdentifier, Stream) tuple in geotypicalModelVisitor.EnumerateFiles(cdbDir))
        {
            yield return tuple;
        }
        logger.LogTrace("Walking MModel for {CDB}", cdbDir);
        foreach ((ICDBIdentifier, Stream) tuple in movingModelVisitor.EnumerateFiles(cdbDir))
        {
            yield return tuple;
        }
        logger.LogTrace("Walking Tiles for {CDB}", cdbDir);
        foreach ((ICDBIdentifier, Stream) tuple in tiledDatasetVisitor.EnumerateFiles(cdbDir))
        {
            yield return tuple;
        }
        logger.LogTrace("Walking Navigation for {CDB}", cdbDir);
        foreach ((ICDBIdentifier, Stream) tuple in navigationVisitor.EnumerateFiles(cdbDir))
        {
            yield return tuple;
        }
        logger.LogTrace("Finished walking CDB data store {CDB}", cdbDir);
    }
}
