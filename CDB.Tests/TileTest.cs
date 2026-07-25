using System.IO;

namespace Silnith.CDB.Tests;

[TestClass]
public class TileTest
{
    [TestMethod]
    public void TestFilename_Elevation()
    {
        Tile tile = new(
            new Latitude(-4),
            new Longitude(-8),
            new Dataset(1),
            1,
            1,
            new LevelOfDetail(0),
            0,
            0,
            "shp");

        Assert.AreEqual("S04W008_D001_S001_T001_L00_U0_R0.shp", tile.Filename);
    }

    [TestMethod]
    public void TestRelativePath_Elevation()
    {
        Tile tile = new(
            new Latitude(-4),
            new Longitude(-8),
            new Dataset(1),
            1,
            1,
            new LevelOfDetail(0),
            0,
            0,
            "shp");

        string expected = Path.Combine(
            "Tiles",
            "S04",
            "W008",
            "001_Elevation",
            "L00",
            "U0");
        Assert.AreEqual(expected, tile.RelativePath);
    }

    [TestMethod]
    public void TestFilename_Elevation_NegativeLod()
    {
        Tile tile = new(
            new Latitude(-4),
            new Longitude(-8),
            new Dataset(1),
            1,
            1,
            new LevelOfDetail(-4),
            0,
            0,
            "tif");

        Assert.AreEqual("S04W008_D001_S001_T001_LC04_U0_R0.tif", tile.Filename);
    }

    [TestMethod]
    public void TestRelativePath_Elevation_NegativeLod()
    {
        Tile tile = new(
            new Latitude(-4),
            new Longitude(-8),
            new Dataset(1),
            1,
            1,
            new LevelOfDetail(-4),
            0,
            0,
            "tif");

        string expected = Path.Combine(
            "Tiles",
            "S04",
            "W008",
            "001_Elevation",
            "LC",
            "U0");
        Assert.AreEqual(expected, tile.RelativePath);
    }

    /*
     * Elevation
     * MinMaxElevation
     * MaxCulture
     * Imagery
     * RMTexture
     * RMDescriptor
     * GSFeature
     * GTFeature
     * GeoPolitical
     * VectorMaterial
     * RoadNetwork
     * RailRoadNetwork
     * PowerLineNetwork
     * HydrographyNetwork
     * GSModelGeometry
     * GSModelTexture
     * GSModelSignature
     * GSModelDescriptor
     * GSModelMaterial
     * GSModelCMT
     * GSModelInteriorGeometry
     * GSModelInteriorTexture
     * GSModelInteriorDescriptor
     * GSModelInteriorMaterial
     * GSModelInteriorCMT
     * T2DModelGeometry
     * T2DModelCMT
     * Navigation
     */

    /*
     * ZIP entries:
     * GSModelGeometry
     * GSModelTexture
     * GSModelMaterial
     * GSModelDescriptor
     * GSModelCMT
     * GSModelInteriorGeometry
     * GSModelInteriorTexture
     * GSModelInteriorMaterial
     * GSModelInteriorDescriptor
     * GSModelInteriorCMT
     * GSModelMetadata
     */
}
