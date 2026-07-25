using System.IO;

namespace Silnith.CDB.Tests;

[TestClass]
public class GeotypicalModelTest
{
    [TestMethod]
    public void TestFilename_GTModelGeometry_Castle()
    {
        GeotypicalModel geotypicalModel = new(
            new Dataset(500),
            1,
            1,
            new FeatureCode("A", "L", 15),
            4,
            "castle",
            "flt");

        Assert.AreEqual("D500_S001_T001_AL015_004_castle.flt", geotypicalModel.Filename);
    }

    [TestMethod]
    public void TestRelativePath_GTModelGeometry_Castle()
    {
        GeotypicalModel geotypicalModel = new(
            new Dataset(500),
            1,
            1,
            new FeatureCode("A", "L", 15),
            4,
            "castle",
            "flt");

        string expected = Path.Combine(
            "GTModel",
            "500_GTModelGeometry",
            "A_Culture",
            "L_Misc_Feature",
            "015_Building");
        Assert.AreEqual(expected, geotypicalModel.RelativePath);
    }

    [TestMethod]
    public void TestFilename_GTModelDescriptor_Castle()
    {
        GeotypicalModel geotypicalModel = new(
            new Dataset(503),
            1,
            1,
            new FeatureCode("A", "L", 15),
            4,
            "castle",
            "xml");

        Assert.AreEqual("D503_S001_T001_AL015_004_castle.xml", geotypicalModel.Filename);
    }

    /// <summary>
    /// Test that the relative path uses a different dataset.
    /// </summary>
    [TestMethod]
    public void TestRelativePath_GTModelDescriptor_Castle()
    {
        GeotypicalModel geotypicalModel = new(
            new Dataset(503),
            1,
            1,
            new FeatureCode("A", "L", 15),
            4,
            "castle",
            "xml");

        string expected = Path.Combine(
            "GTModel",
            "500_GTModelGeometry",
            "A_Culture",
            "L_Misc_Feature",
            "015_Building");
        Assert.AreEqual(expected, geotypicalModel.RelativePath);
    }

    [TestMethod]
    public void TestFilename_GTModelInteriorDescriptor_Castle()
    {
        GeotypicalModel geotypicalModel = new(
            new Dataset(508),
            1,
            1,
            new FeatureCode("A", "L", 15),
            4,
            "castle",
            "xml");

        Assert.AreEqual("D508_S001_T001_AL015_004_castle.xml", geotypicalModel.Filename);
    }

    [TestMethod]
    public void TestRelativePath_GTModelInteriorDescriptor_Castle()
    {
        GeotypicalModel geotypicalModel = new(
            new Dataset(508),
            1,
            1,
            new FeatureCode("A", "L", 15),
            4,
            "castle",
            "xml");

        string expected = Path.Combine(
            "GTModel",
            "506_GTModelInteriorGeometry",
            "A_Culture",
            "L_Misc_Feature",
            "015_Building");
        Assert.AreEqual(expected, geotypicalModel.RelativePath);
    }


    [TestMethod]
    [Ignore("Needs a way of handling local metadata.")]
    public void TestFilename_GTModelInteriorGeometry_Castle_Metadata()
    {
        GeotypicalModel geotypicalModel = new(
            new Dataset(508),
            1,
            1,
            new FeatureCode("A", "L", 15),
            4,
            "castle_mtd",
            "flt");

        Assert.AreEqual("D508_S001_T001_AL015_004_castle_mtd.flt", geotypicalModel.Filename);
    }

    [TestMethod]
    [Ignore("Needs a way of handling local metadata.")]
    public void TestRelativePath_GTModelInteriorGeometry_Castle_Metadata()
    {
        GeotypicalModel geotypicalModel = new(
            new Dataset(508),
            1,
            1,
            new FeatureCode("A", "L", 15),
            4,
            "castle_mtd",
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
}
