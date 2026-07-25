using System.IO;

namespace Silnith.CDB.Tests;

[TestClass]
public class NavigationTest
{
    [TestMethod]
    public void TestFilename()
    {
        Navigation navigation = new(
            new Dataset(400),
            1,
            2,
            "dbf");

        Assert.AreEqual("D400_S001_T002.dbf", navigation.Filename);
    }

    [TestMethod]
    public void TestRelativePath()
    {
        Navigation navigation = new(
            new Dataset(400),
            1,
            2,
            "dbf");

        string expected = Path.Combine(
            "Navigation",
            "400_NavData");
        Assert.AreEqual(expected, navigation.RelativePath);
    }
}
