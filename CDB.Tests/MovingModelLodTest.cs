using System.IO;

namespace Silnith.CDB.Tests;

[TestClass]
public class MovingModelLodTest
{
    [TestMethod]
    public void TestFilename_MModelGeometry_Astronaut()
    {
        MovingModelLod movingModel = new(
            new Dataset(606),
            1,
            1,
            new LevelOfDetail(-4),
            new(3, 5, 225, 1, 5, 6, 7),
            "shp");

        Assert.AreEqual("D606_S001_T001_LC04_3_5_225_1_5_6_7.shp", movingModel.Filename);
    }

    [TestMethod]
    public void TestRelativePath_MModelGeometry_Astronaut()
    {
        MovingModelLod movingModel = new(
            new Dataset(606),
            1,
            1,
            new LevelOfDetail(-4),
            new(3, 5, 225, 1, 5, 6, 7),
            "shp");

        string expected = Path.Combine(
            "MModel",
            "606_MModelSignature",
            "3_Life_Form",
            "5_Space",
            "225_United_States",
            "1_Astronaut",
            "3_5_225_1_5_6_7",
            "LC04");
        Assert.AreEqual(expected, movingModel.RelativePath);
    }
}
