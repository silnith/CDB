using System.IO;

namespace Silnith.CDB.Tests;

[TestClass]
public class TextureTest
{
    [TestMethod]
    public void TestFilename_GTModelCMT_Brick()
    {
        Texture texture = new(
            new Dataset(505),
            1,
            1,
            "Brick",
            "xml");

        Assert.AreEqual("D505_S001_T001_Brick.xml", texture.Filename);
    }

    [TestMethod]
    public void TestRelativePath_GTModelCMT_Brick()
    {
        Texture texture = new(
            new Dataset(505),
            1,
            1,
            "Brick",
            "xml");

        string expected = Path.Combine(
            "GTModel",
            "501_GTModelTexture",
            "B",
            "R",
            "Brick");
        Assert.AreEqual(expected, texture.RelativePath);
    }

    [TestMethod]
    [Ignore("Needs a way of handling local metadata.")]
    public void TestFilename_GTModelTexture_Brick_Metadata()
    {
        Texture texture = new(
            new Dataset(505),
            1,
            1,
            "Brick_mtd",
            "xml");

        Assert.AreEqual("D505_S001_T001_Brick_mtd.xml", texture.Filename);
    }

    [TestMethod]
    [Ignore("Needs a way of handling local metadata.")]
    public void TestRelativePath_GTModelTexture_Brick_Metadata()
    {
        Texture texture = new(
            new Dataset(505),
            1,
            1,
            "Brick_mtd",
            "xml");

        string expected = Path.Combine(
            "GTModel",
            "501_GTModelTexture",
            "B",
            "R",
            "Brick");
        Assert.AreEqual(expected, texture.RelativePath);
    }

    [TestMethod]
    public void TestFilename_GTModelInteriorCMT_Brick()
    {
        Texture texture = new(
            new Dataset(513),
            1,
            1,
            "Brick",
            "xml");

        Assert.AreEqual("D513_S001_T001_Brick.xml", texture.Filename);
    }

    [TestMethod]
    public void TestRelativePath_GTModelInteriorCMT_Brick()
    {
        Texture texture = new(
            new Dataset(513),
            1,
            1,
            "Brick",
            "xml");

        string expected = Path.Combine(
            "GTModel",
            "507_GTModelInteriorTexture",
            "B",
            "R",
            "Brick");
        Assert.AreEqual(expected, texture.RelativePath);
    }

    [TestMethod]
    public void TestFilename_MModelCMT_Astronaut()
    {
        Texture texture = new(
            new Dataset(605),
            1,
            1,
            "Astronaut",
            "xml");

        // TODO: W04?
        Assert.AreEqual("D605_S001_T001_Astronaut.xml", texture.Filename);
    }

    [TestMethod]
    public void TestRelativePath_MModelCMT_Astronaut()
    {
        Texture texture = new(
            new Dataset(605),
            1,
            1,
            "Astronaut",
            "xml");

        string expected = Path.Combine(
            "MModel",
            "601_MModelTexture",
            "A",
            "S",
            "Astronaut");
        Assert.AreEqual(expected, texture.RelativePath);
    }

}
