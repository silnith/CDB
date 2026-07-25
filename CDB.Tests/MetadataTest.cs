using System.IO;

namespace Silnith.CDB.Tests;

[TestClass]
public class MetadataTest
{
    [TestMethod]
    public void TestFilename()
    {
        Metadata metadata = new("Version", "xml");

        Assert.AreEqual("Version.xml", metadata.Filename);
    }

    [TestMethod]
    public void TestRelativePath()
    {
        Metadata metadata = new("Version", "xml");

        string expected = Path.Combine(
            "Metadata");
        Assert.AreEqual(expected, metadata.RelativePath);
    }
}
