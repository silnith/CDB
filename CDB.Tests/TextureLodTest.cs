using System.IO;

namespace Silnith.CDB.Tests;

[TestClass]
public class TextureLodTest
{
    [TestMethod]
    public void TestFilename_GTModelTexture_Brick()
    {
        TextureLod texture = new(
            new Dataset(511),
            1,
            1,
            new LevelOfDetail(4),
            "Brick",
            "rgb");

        Assert.AreEqual("D511_S001_T001_L04_Brick.rgb", texture.Filename);
    }

    [TestMethod]
    public void TestRelativePath_GTModelTexture_Brick()
    {
        TextureLod texture = new(
            new Dataset(511),
            1,
            1,
            new LevelOfDetail(4),
            "Brick",
            "rgb");

        string expected = Path.Combine(
            "GTModel",
            "501_GTModelTexture",
            "B",
            "R",
            "Brick");
        Assert.AreEqual(expected, texture.RelativePath);
    }

    [TestMethod]
    public void TestFilename_GTModelMaterial_Brick()
    {
        TextureLod texture = new(
            new Dataset(504),
            1,
            1,
            new LevelOfDetail(4),
            "Brick",
            "tif");

        Assert.AreEqual("D504_S001_T001_L04_Brick.tif", texture.Filename);
    }

    [TestMethod]
    public void TestRelativePath_GTModelMaterial_Brick()
    {
        TextureLod texture = new(
            new Dataset(504),
            1,
            1,
            new LevelOfDetail(4),
            "Brick",
            "tif");

        string expected = Path.Combine(
            "GTModel",
            "501_GTModelTexture",
            "B",
            "R",
            "Brick");
        Assert.AreEqual(expected, texture.RelativePath);
    }

    [TestMethod]
    public void TestFilename_GTModelInteriorTexture_Brick()
    {
        TextureLod texture = new(
            new Dataset(507),
            1,
            1,
            new LevelOfDetail(4),
            "Brick",
            "rgb");

        Assert.AreEqual("D507_S001_T001_L04_Brick.rgb", texture.Filename);
    }

    [TestMethod]
    public void TestRelativePath_GTModelInteriorTexture_Brick()
    {
        TextureLod texture = new(
            new Dataset(507),
            1,
            1,
            new LevelOfDetail(4),
            "Brick",
            "rgb");

        string expected = Path.Combine(
            "GTModel",
            "507_GTModelInteriorTexture",
            "B",
            "R",
            "Brick");
        Assert.AreEqual(expected, texture.RelativePath);
    }

    [TestMethod]
    public void TestFilename_GTModelInteriorMaterial_Brick()
    {
        TextureLod texture = new(
            new Dataset(509),
            1,
            1,
            new LevelOfDetail(4),
            "Brick",
            "tif");

        Assert.AreEqual("D509_S001_T001_L04_Brick.tif", texture.Filename);
    }

    [TestMethod]
    public void TestRelativePath_GTModelInteriorMaterial_Brick()
    {
        TextureLod texture = new(
            new Dataset(509),
            1,
            1,
            new LevelOfDetail(4),
            "Brick",
            "tif");

        string expected = Path.Combine(
            "GTModel",
            "507_GTModelInteriorTexture",
            "B",
            "R",
            "Brick");
        Assert.AreEqual(expected, texture.RelativePath);
    }

    [TestMethod]
    public void TestFilename_MModelTexture_Astronaut()
    {
        TextureLod texture = new(
            new Dataset(601),
            1,
            1,
            new LevelOfDetail(4),
            "Astronaut",
            "rgb");

        // TODO: W04?
        Assert.AreEqual("D601_S001_T001_L04_Astronaut.rgb", texture.Filename);
    }

    [TestMethod]
    public void TestRelativePath_MModelTexture_Astronaut()
    {
        TextureLod texture = new(
            new Dataset(601),
            1,
            1,
            new LevelOfDetail(4),
            "Astronaut",
            "rgb");

        string expected = Path.Combine(
            "MModel",
            "601_MModelTexture",
            "A",
            "S",
            "Astronaut");
        Assert.AreEqual(expected, texture.RelativePath);
    }

    [TestMethod]
    public void TestFilename_MModelMaterial_Astronaut()
    {
        TextureLod texture = new(
            new Dataset(604),
            1,
            1,
            new LevelOfDetail(4),
            "Astronaut",
            "tif");

        // TODO: W04?
        Assert.AreEqual("D604_S001_T001_L04_Astronaut.tif", texture.Filename);
    }

    [TestMethod]
    public void TestRelativePath_MModelMaterial_Astronaut()
    {
        TextureLod texture = new(
            new Dataset(604),
            1,
            1,
            new LevelOfDetail(4),
            "Astronaut",
            "tif");

        string expected = Path.Combine(
            "MModel",
            "601_MModelTexture",
            "A",
            "S",
            "Astronaut");
        Assert.AreEqual(expected, texture.RelativePath);
    }

}
