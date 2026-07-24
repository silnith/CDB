namespace Silnith.CDB.Tests;

[TestClass]
public class DatasetTest
{

    [TestMethod]
    public void TestElevationCode()
    {
        Assert.AreEqual("D001", new Dataset(1).Code);
    }

    [TestMethod]
    public void TestElevationDirectory()
    {
        Assert.AreEqual("001_Elevation", new Dataset(1).Directory);
    }

    [TestMethod]
    public void TestGTModelGeometryCode()
    {
        Assert.AreEqual("D500", new Dataset(500).Code);
    }

    [TestMethod]
    public void TestGTModelGeometryDirectory()
    {
        Assert.AreEqual("500_GTModelGeometry", new Dataset(500).Directory);
    }

    [TestMethod]
    public void TestGTModelGeometryLevelOfDetailCode()
    {
        Assert.AreEqual("D510", new Dataset(510).Code);
    }

    [TestMethod]
    public void TestGTModelGeometryLevelOfDetailDirectory()
    {
        Assert.AreEqual("500_GTModelGeometry", new Dataset(510).Directory);
    }

    [TestMethod]
    public void TestGTModelDescriptorCode()
    {
        Assert.AreEqual("D503", new Dataset(503).Code);
    }

    [TestMethod]
    public void TestGTModelDescriptorDirectory()
    {
        Assert.AreEqual("500_GTModelGeometry", new Dataset(503).Directory);
    }

    [TestMethod]
    public void TestGTModelTextureDeprecatedCode()
    {
        Assert.AreEqual("D501", new Dataset(501).Code);
    }

    [TestMethod]
    public void TestGTModelTextureDeprecatedDirectory()
    {
        Assert.AreEqual("501_GTModelTexture", new Dataset(501).Directory);
    }

    [TestMethod]
    public void TestGTModelTextureCode()
    {
        Assert.AreEqual("D511", new Dataset(511).Code);
    }

    [TestMethod]
    public void TestGTModelTextureDirectory()
    {
        Assert.AreEqual("501_GTModelTexture", new Dataset(511).Directory);
    }

    [TestMethod]
    public void TestGTModelMaterialCode()
    {
        Assert.AreEqual("D504", new Dataset(504).Code);
    }

    [TestMethod]
    public void TestGTModelMaterialDirectory()
    {
        Assert.AreEqual("501_GTModelTexture", new Dataset(504).Directory);
    }

    [TestMethod]
    public void TestGTModelCMTCode()
    {
        Assert.AreEqual("D505", new Dataset(505).Code);
    }

    [TestMethod]
    public void TestGTModelCMTDirectory()
    {
        Assert.AreEqual("501_GTModelTexture", new Dataset(505).Directory);
    }

    [TestMethod]
    public void TestGTModelInteriorGeometryCode()
    {
        Assert.AreEqual("D506", new Dataset(506).Code);
    }

    [TestMethod]
    public void TestGTModelInteriorGeometryDirectory()
    {
        Assert.AreEqual("506_GTModelInteriorGeometry", new Dataset(506).Directory);
    }

    [TestMethod]
    public void TestGTModelInteriorDescriptorCode()
    {
        Assert.AreEqual("D508", new Dataset(508).Code);
    }

    [TestMethod]
    public void TestGTModelInteriorDescriptorDirectory()
    {
        Assert.AreEqual("506_GTModelInteriorGeometry", new Dataset(508).Directory);
    }

    [TestMethod]
    public void TestGTModelInteriorTextureCode()
    {
        Assert.AreEqual("D507", new Dataset(507).Code);
    }

    [TestMethod]
    public void TestGTModelInteriorTextureDirectory()
    {
        Assert.AreEqual("507_GTModelInteriorTexture", new Dataset(507).Directory);
    }

    [TestMethod]
    public void TestGTModelInteriorMaterialCode()
    {
        Assert.AreEqual("D509", new Dataset(509).Code);
    }

    [TestMethod]
    public void TestGTModelInteriorMaterialDirectory()
    {
        Assert.AreEqual("507_GTModelInteriorTexture", new Dataset(509).Directory);
    }

    [TestMethod]
    public void TestGTModelInteriorCMTCode()
    {
        Assert.AreEqual("D513", new Dataset(513).Code);
    }

    [TestMethod]
    public void TestGTModelInteriorCMTDirectory()
    {
        Assert.AreEqual("507_GTModelInteriorTexture", new Dataset(513).Directory);
    }

    [TestMethod]
    public void TestGTModelSignatureDeprecatedCode()
    {
        Assert.AreEqual("D502", new Dataset(502).Code);
    }

    [TestMethod]
    public void TestGTModelSignatureDeprecatedDirectory()
    {
        Assert.AreEqual("502_GTModelSignature", new Dataset(502).Directory);
    }

    [TestMethod]
    public void TestGTModelSignatureCode()
    {
        Assert.AreEqual("D512", new Dataset(512).Code);
    }

    [TestMethod]
    public void TestGTModelSignatureDirectory()
    {
        Assert.AreEqual("502_GTModelSignature", new Dataset(512).Directory);
    }

    [TestMethod]
    public void TestMModelGeometryCode()
    {
        Assert.AreEqual("D600", new Dataset(600).Code);
    }

    [TestMethod]
    public void TestMModelGeometryDirectory()
    {
        Assert.AreEqual("600_MModelGeometry", new Dataset(600).Directory);
    }

    [TestMethod]
    public void TestMModelDescriptorCode()
    {
        Assert.AreEqual("D603", new Dataset(603).Code);
    }

    [TestMethod]
    public void TestMModelDescriptorDirectory()
    {
        Assert.AreEqual("600_MModelGeometry", new Dataset(603).Directory);
    }

    [TestMethod]
    public void TestMModelTextureCode()
    {
        Assert.AreEqual("D601", new Dataset(601).Code);
    }

    [TestMethod]
    public void TestMModelTextureDirectory()
    {
        Assert.AreEqual("601_MModelTexture", new Dataset(601).Directory);
    }

    [TestMethod]
    public void TestMModelMaterialCode()
    {
        Assert.AreEqual("D604", new Dataset(604).Code);
    }

    [TestMethod]
    public void TestMModelMaterialDirectory()
    {
        Assert.AreEqual("601_MModelTexture", new Dataset(604).Directory);
    }

    [TestMethod]
    public void TestMModelCMTCode()
    {
        Assert.AreEqual("D605", new Dataset(605).Code);
    }

    [TestMethod]
    public void TestMModelCMTDirectory()
    {
        Assert.AreEqual("601_MModelTexture", new Dataset(605).Directory);
    }

}
