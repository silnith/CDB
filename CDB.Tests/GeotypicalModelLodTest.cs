using System.IO;

namespace Silnith.CDB.Tests;

[TestClass]
public class GeotypicalModelLodTest
{
    [TestMethod]
    public void TestFilename_GTModelGeometryLod_Castle()
    {
        GeotypicalModelLod geotypicalModel = new(
            new Dataset(510),
            1,
            1,
            new LevelOfDetail(4),
            new FeatureCode("A", "L", 15),
            4,
            "castle",
            "flt");

        Assert.AreEqual("D510_S001_T001_L04_AL015_004_castle.flt", geotypicalModel.Filename);
    }

    /// <summary>
    /// Test that the relative path uses a different dataset.
    /// </summary>
    [TestMethod]
    public void TestRelativePath_GTModelGeometryLod_Castle()
    {
        GeotypicalModelLod geotypicalModel = new(
            new Dataset(510),
            1,
            1,
            new LevelOfDetail(4),
            new FeatureCode("A", "L", 15),
            4,
            "castle",
            "flt");

        string expected = Path.Combine(
            "GTModel",
            "500_GTModelGeometry",
            "A_Culture",
            "L_Misc_Feature",
            "015_Building",
            "L04");
        Assert.AreEqual(expected, geotypicalModel.RelativePath);
    }

    [TestMethod]
    public void TestFilename_GTModelInteriorGeometry_Castle()
    {
        GeotypicalModelLod geotypicalModel = new(
            new Dataset(506),
            1,
            1,
            new LevelOfDetail(4),
            new FeatureCode("A", "L", 15),
            4,
            "castle",
            "flt");

        Assert.AreEqual("D506_S001_T001_L04_AL015_004_castle.flt", geotypicalModel.Filename);
    }

    [TestMethod]
    public void TestRelativePath_GTModelInteriorGeometry_Castle()
    {
        GeotypicalModelLod geotypicalModel = new(
            new Dataset(506),
            1,
            1,
            new LevelOfDetail(4),
            new FeatureCode("A", "L", 15),
            4,
            "castle",
            "flt");

        string expected = Path.Combine(
            "GTModel",
            "506_GTModelInteriorGeometry",
            "A_Culture",
            "L_Misc_Feature",
            "015_Building",
            "L04");
        Assert.AreEqual(expected, geotypicalModel.RelativePath);
    }

    [TestMethod]
    public void TestFilename_GTModelSignature_Castle()
    {
        GeotypicalModelLod geotypicalModel = new(
            new Dataset(512),
            1,
            1,
            new LevelOfDetail(4),
            new FeatureCode("A", "L", 15),
            4,
            "castle",
            "shp");

        Assert.AreEqual("D512_S001_T001_L04_AL015_004_castle.shp", geotypicalModel.Filename);
    }

    [TestMethod]
    public void TestRelativePath_GTModelSignature_Castle()
    {
        GeotypicalModelLod geotypicalModel = new(
            new Dataset(512),
            1,
            1,
            new LevelOfDetail(4),
            new FeatureCode("A", "L", 15),
            4,
            "castle",
            "shp");

        string expected = Path.Combine(
            "GTModel",
            "502_GTModelSignature",
            "A_Culture",
            "L_Misc_Feature",
            "015_Building",
            "L04");
        Assert.AreEqual(expected, geotypicalModel.RelativePath);
    }
}
