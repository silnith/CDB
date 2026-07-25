using System.IO;

namespace Silnith.CDB.Tests;

[TestClass]
public class MovingModelTest
{
    [TestMethod]
    public void TestFilename_MModelGeometry_Astronaut()
    {
        MovingModel movingModel = new(
            new Dataset(600),
            1,
            1,
            new(3, 5, 225, 1, 5, 6, 7),
            "flt");

        Assert.AreEqual("D600_S001_T001_3_5_225_1_5_6_7.flt", movingModel.Filename);
    }

    [TestMethod]
    public void TestRelativePath_MModelGeometry_Astronaut()
    {
        MovingModel movingModel = new(
            new Dataset(600),
            1,
            1,
            new(3, 5, 225, 1, 5, 6, 7),
            "flt");

        string expected = Path.Combine(
            "MModel",
            "600_MModelGeometry",
            "3_Life_Form",
            "5_Space",
            "225_United_States",
            "1_Astronaut",
            "3_5_225_1_5_6_7");
        Assert.AreEqual(expected, movingModel.RelativePath);
    }

    [TestMethod]
    public void TestFilename_MModelDescriptor_Astronaut()
    {
        MovingModel movingModel = new(
            new Dataset(603),
            1,
            1,
            new(3, 5, 225, 1, 5, 6, 7),
            "xml");

        Assert.AreEqual("D603_S001_T001_3_5_225_1_5_6_7.xml", movingModel.Filename);
    }

    [TestMethod]
    public void TestRelativePath_MModelDescriptor_Astronaut()
    {
        MovingModel movingModel = new(
            new Dataset(603),
            1,
            1,
            new(3, 5, 225, 1, 5, 6, 7),
            "xml");

        string expected = Path.Combine(
            "MModel",
            "600_MModelGeometry",
            "3_Life_Form",
            "5_Space",
            "225_United_States",
            "1_Astronaut",
            "3_5_225_1_5_6_7");
        Assert.AreEqual(expected, movingModel.RelativePath);
    }
}
