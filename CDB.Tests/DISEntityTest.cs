using System.IO;

namespace Silnith.CDB.Tests;

[TestClass]
public class DISEntityTest
{

    [TestMethod]
    public void TestBelgiumTank()
    {
        Assert.AreEqual(Path.Combine("1_Platform", "1_Land", "21_Belgium", "1_Tank", "1_1_21_1_5_6_7"), new DISEntity(1, 1, 21, 1, 5, 6, 7).Directories);
    }

    [TestMethod]
    public void TestAmericanAstronaut()
    {
        Assert.AreEqual(Path.Combine("3_Life_Form", "5_Space", "225_United_States", "1_Astronaut", "3_5_225_1_5_6_7"), new DISEntity(3, 5, 225, 1, 5, 6, 7).Directories);
    }

}
