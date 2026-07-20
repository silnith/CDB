using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Silnith.CDB.Tests;

[TestClass]
public class CDBInstanceTest
{
    [TestMethod]
    public void TestTryReadFile()
    {
        Mock<ICDB> cdb1 = new(MockBehavior.Strict);
        Mock<ICDB> cdb2 = new(MockBehavior.Strict);
        Mock<ICDB> cdb3 = new(MockBehavior.Strict);

        cdb1.Setup(cdb => cdb.TryReadFile(It.IsAny<string>(), It.IsAny<Action<Stream>>()))
            .Returns(false);
        cdb2.Setup(cdb => cdb.TryReadFile(It.IsAny<string>(), It.IsAny<Action<Stream>>()))
            .Returns(true);
        cdb3.Setup(cdb => cdb.TryReadFile(It.IsAny<string>(), It.IsAny<Action<Stream>>()))
            .Returns(true);

        CDBInstance cdbInstance = new(new List<ICDB>()
        {
            cdb1.Object,
            cdb2.Object,
            cdb3.Object
        });

        Action<Stream> action = stream => { };

        bool found = cdbInstance.TryReadFile("Metadata/Version.xml", action);

        Assert.IsTrue(found);

        cdb1.Verify(cdb => cdb.TryReadFile("Metadata/Version.xml", action), Times.Once());
        cdb2.Verify(cdb => cdb.TryReadFile("Metadata/Version.xml", action), Times.Once());
        cdb3.Verify(cdb => cdb.TryReadFile("Metadata/Version.xml", action), Times.Never());
    }

    [TestMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0039:Use local function", Justification = "This breaks Moq.")]
    public async Task TestTryReadFileAsync()
    {
        Mock<ICDB> cdb1 = new(MockBehavior.Strict);
        Mock<ICDB> cdb2 = new(MockBehavior.Strict);
        Mock<ICDB> cdb3 = new(MockBehavior.Strict);

        cdb1.Setup(cdb => cdb.TryReadFileAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Stream, CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()).Result)
            .Callback<string, Func<Stream, CancellationToken, Task>, CancellationToken>(
                (filePathAndName, asyncAction, cancellationToken) =>
                {
                    Debug.WriteLine("Thread {0}: Mock 1 processing.", Environment.CurrentManagedThreadId);
                    Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).GetAwaiter().GetResult();
                    Debug.WriteLine("Thread {0}: Mock 1 finished.", Environment.CurrentManagedThreadId);
                })
            .Returns(false);
        cdb2.Setup(cdb => cdb.TryReadFileAsync(It.IsAny<string>(), It.IsAny<Func<Stream, CancellationToken, Task>>(), It.IsAny<CancellationToken>()).Result)
            .Callback<string, Func<Stream, CancellationToken, Task>, CancellationToken>(
                (filePathAndName, asyncAction, cancellationToken) =>
                {
                    Debug.WriteLine("Thread {0}: Mock 2 processing.", Environment.CurrentManagedThreadId);
                    Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken).GetAwaiter().GetResult();
                    using MemoryStream stream = new(new byte[64]);
                    Debug.WriteLine("Thread {0}: Mock 2 calling action.", Environment.CurrentManagedThreadId);
                    asyncAction(stream, cancellationToken).GetAwaiter().GetResult();
                    Debug.WriteLine("Thread {0}: Mock 2 finished action.", Environment.CurrentManagedThreadId);
                })
            .Returns(true);
        cdb3.Setup(cdb => cdb.TryReadFileAsync(It.IsAny<string>(), It.IsAny<Func<Stream, CancellationToken, Task>>(), It.IsAny<CancellationToken>()).Result)
            .Callback<string, Func<Stream, CancellationToken, Task>, CancellationToken>(
                (filePathAndName, asyncAction, cancellationToken) =>
                {
                    Debug.WriteLine("Thread {0}: Mock 3 processing.", Environment.CurrentManagedThreadId);
                    Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken).GetAwaiter().GetResult();
                    using MemoryStream stream = new(new byte[256]);
                    Debug.WriteLine("Thread {0}: Mock 3 calling action.", Environment.CurrentManagedThreadId);
                    asyncAction(stream, cancellationToken).GetAwaiter().GetResult();
                    Debug.WriteLine("Thread {0}: Mock 3 finished action.", Environment.CurrentManagedThreadId);
                })
            .Returns(true);

        CDBInstance cdbInstance = new(new List<ICDB>()
        {
            cdb1.Object,
            cdb2.Object,
            cdb3.Object
        });

        int callCount = 0;
        long length = -1;
        Func<Stream, CancellationToken, Task> action = (stream, token) =>
        {
            Debug.WriteLine("Thread {0}: Processing file.", Environment.CurrentManagedThreadId);
            _ = Interlocked.Increment(ref callCount);
            _ = Interlocked.Exchange(ref length, stream.Length);
            return Task.CompletedTask;
        };

        const string FilePathAndName = "Metadata/Version.xml";
        Debug.WriteLine("Thread {0}: Beginning read.", Environment.CurrentManagedThreadId);
        bool found = await cdbInstance.TryReadFileAsync(FilePathAndName, action, CancellationToken.None);
        Debug.WriteLine("Thread {0}: Finished read.", Environment.CurrentManagedThreadId);

        Assert.IsTrue(found);
        Assert.AreEqual(1, callCount);
        Assert.AreEqual(64, length);

        cdb1.Verify(cdb => cdb.TryReadFileAsync("Metadata/Version.xml", It.IsAny<Func<Stream, CancellationToken, Task>>(), It.IsAny<CancellationToken>()), Times.Once());
        cdb2.Verify(cdb => cdb.TryReadFileAsync("Metadata/Version.xml", It.IsAny<Func<Stream, CancellationToken, Task>>(), It.IsAny<CancellationToken>()), Times.Once());
        cdb3.Verify(cdb => cdb.TryReadFileAsync("Metadata/Version.xml", It.IsAny<Func<Stream, CancellationToken, Task>>(), It.IsAny<CancellationToken>()), Times.Once());
    }
}
