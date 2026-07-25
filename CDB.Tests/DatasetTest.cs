namespace Silnith.CDB.Tests;

[TestClass]
public class DatasetTest
{

    [TestMethod]
    public void TestCode_Elevation()
    {
        Assert.AreEqual("D001", new Dataset(1).Code);
    }

    [TestMethod]
    public void TestDirectory_Elevation()
    {
        Assert.AreEqual("001_Elevation", new Dataset(1).Directory);
    }

    [TestMethod]
    public void TestCode_GTModelGeometry()
    {
        Assert.AreEqual("D500", new Dataset(500).Code);
    }

    [TestMethod]
    public void TestDirectory_GTModelGeometry()
    {
        Assert.AreEqual("500_GTModelGeometry", new Dataset(500).Directory);
    }

    [TestMethod]
    public void TestCode_GTModelGeometryLevelOfDetail()
    {
        Assert.AreEqual("D510", new Dataset(510).Code);
    }

    [TestMethod]
    public void TestDirectory_GTModelGeometryLevelOfDetail()
    {
        Assert.AreEqual("500_GTModelGeometry", new Dataset(510).Directory);
    }

    [TestMethod]
    public void TestCode_GTModelDescriptor()
    {
        Assert.AreEqual("D503", new Dataset(503).Code);
    }

    [TestMethod]
    public void TestDirectory_GTModelDescriptor()
    {
        Assert.AreEqual("500_GTModelGeometry", new Dataset(503).Directory);
    }

    [TestMethod]
    public void TestCode_GTModelTextureDeprecated()
    {
        Assert.AreEqual("D501", new Dataset(501).Code);
    }

    [TestMethod]
    public void TestDirectory_GTModelTextureDeprecated()
    {
        Assert.AreEqual("501_GTModelTexture", new Dataset(501).Directory);
    }

    [TestMethod]
    public void TestCode_GTModelTexture()
    {
        Assert.AreEqual("D511", new Dataset(511).Code);
    }

    [TestMethod]
    public void TestDirectory_GTModelTexture()
    {
        Assert.AreEqual("501_GTModelTexture", new Dataset(511).Directory);
    }

    [TestMethod]
    public void TestCode_GTModelMaterial()
    {
        Assert.AreEqual("D504", new Dataset(504).Code);
    }

    [TestMethod]
    public void TestDirectory_GTModelMaterial()
    {
        Assert.AreEqual("501_GTModelTexture", new Dataset(504).Directory);
    }

    [TestMethod]
    public void TestCode_GTModelCMT()
    {
        Assert.AreEqual("D505", new Dataset(505).Code);
    }

    [TestMethod]
    public void TestDirectory_GTModelCMT()
    {
        Assert.AreEqual("501_GTModelTexture", new Dataset(505).Directory);
    }

    [TestMethod]
    public void TestCode_GTModelInteriorGeometry()
    {
        Assert.AreEqual("D506", new Dataset(506).Code);
    }

    [TestMethod]
    public void TestDirectory_GTModelInteriorGeometry()
    {
        Assert.AreEqual("506_GTModelInteriorGeometry", new Dataset(506).Directory);
    }

    [TestMethod]
    public void TestCode_GTModelInteriorDescriptor()
    {
        Assert.AreEqual("D508", new Dataset(508).Code);
    }

    [TestMethod]
    public void TestDirectory_GTModelInteriorDescriptor()
    {
        Assert.AreEqual("506_GTModelInteriorGeometry", new Dataset(508).Directory);
    }

    [TestMethod]
    public void TestCode_GTModelInteriorTexture()
    {
        Assert.AreEqual("D507", new Dataset(507).Code);
    }

    [TestMethod]
    public void TestDirectory_GTModelInteriorTexture()
    {
        Assert.AreEqual("507_GTModelInteriorTexture", new Dataset(507).Directory);
    }

    [TestMethod]
    public void TestCode_GTModelInteriorMaterial()
    {
        Assert.AreEqual("D509", new Dataset(509).Code);
    }

    [TestMethod]
    public void TestDirectory_GTModelInteriorMaterial()
    {
        Assert.AreEqual("507_GTModelInteriorTexture", new Dataset(509).Directory);
    }

    [TestMethod]
    public void TestCode_GTModelInteriorCMT()
    {
        Assert.AreEqual("D513", new Dataset(513).Code);
    }

    [TestMethod]
    public void TestDirectory_GTModelInteriorCMT()
    {
        Assert.AreEqual("507_GTModelInteriorTexture", new Dataset(513).Directory);
    }

    [TestMethod]
    public void TestCode_GTModelSignatureDeprecated()
    {
        Assert.AreEqual("D502", new Dataset(502).Code);
    }

    [TestMethod]
    public void TestDirectory_GTModelSignatureDeprecated()
    {
        Assert.AreEqual("502_GTModelSignature", new Dataset(502).Directory);
    }

    [TestMethod]
    public void TestCode_GTModelSignature()
    {
        Assert.AreEqual("D512", new Dataset(512).Code);
    }

    [TestMethod]
    public void TestDirectory_GTModelSignature()
    {
        Assert.AreEqual("502_GTModelSignature", new Dataset(512).Directory);
    }

    [TestMethod]
    public void TestCode_MModelGeometry()
    {
        Assert.AreEqual("D600", new Dataset(600).Code);
    }

    [TestMethod]
    public void TestDirectory_MModelGeometry()
    {
        Assert.AreEqual("600_MModelGeometry", new Dataset(600).Directory);
    }

    [TestMethod]
    public void TestCode_MModelDescriptor()
    {
        Assert.AreEqual("D603", new Dataset(603).Code);
    }

    [TestMethod]
    public void TestDirectory_MModelDescriptor()
    {
        Assert.AreEqual("600_MModelGeometry", new Dataset(603).Directory);
    }

    [TestMethod]
    public void TestCode_MModelTexture()
    {
        Assert.AreEqual("D601", new Dataset(601).Code);
    }

    [TestMethod]
    public void TestDirectory_MModelTexture()
    {
        Assert.AreEqual("601_MModelTexture", new Dataset(601).Directory);
    }

    [TestMethod]
    public void TestCode_MModelMaterial()
    {
        Assert.AreEqual("D604", new Dataset(604).Code);
    }

    [TestMethod]
    public void TestDirectory_MModelMaterial()
    {
        Assert.AreEqual("601_MModelTexture", new Dataset(604).Directory);
    }

    [TestMethod]
    public void TestCode_MModelCMT()
    {
        Assert.AreEqual("D605", new Dataset(605).Code);
    }

    [TestMethod]
    public void TestDirectory_MModelCMT()
    {
        Assert.AreEqual("601_MModelTexture", new Dataset(605).Directory);
    }

}
