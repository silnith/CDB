using System.IO;

namespace Silnith.CDB.Tests;

[TestClass]
public class FeatureCodeTest
{
    [TestMethod]
    public void TestFeatureCodeBuilding()
    {
        Assert.AreEqual(Path.Combine("A_Culture", "L_Misc_Feature", "015_Building"), new FeatureCode("A", "L", 15).Directories);
    }
}
