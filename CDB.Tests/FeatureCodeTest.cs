using System.IO;

namespace Silnith.CDB.Tests;

[TestClass]
public class FeatureCodeTest
{
    [TestMethod]
    public void TestCode_Building()
    {
        Assert.AreEqual("AL015", new FeatureCode("A", "L", 15).Code);
    }
    [TestMethod]
    public void TestRelativePath_Building()
    {
        Assert.AreEqual(Path.Combine("A_Culture", "L_Misc_Feature", "015_Building"), new FeatureCode("A", "L", 15).RelativePath);
    }
}
