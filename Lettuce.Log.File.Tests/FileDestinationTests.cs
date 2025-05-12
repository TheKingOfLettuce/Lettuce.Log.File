namespace Lettuce.Log.File.Tests;

[TestClass]
public class FileDestinationTests {
    private const string FILE_NAME = "Lettuce.Log.File.Tests.log";
    private const string ROLLED_FILE_NAME_1 = "Lettuce.Log.File.Tests__1.log";
    private const string ROLLED_FILE_NAME_2 = "Lettuce.Log.File.Tests__2.log";
    private const string ROLLED_FILE_NAME_3 = "Lettuce.Log.File.Tests__3.log";

    private static string LogDir = Path.Combine(Directory.GetCurrentDirectory(), "TestLogs");
    private static string FileLogPath = Path.Combine(LogDir, FILE_NAME);
    private static string RolledLog1Path = Path.Combine(LogDir, ROLLED_FILE_NAME_1);
    private static string RolledLog2Path = Path.Combine(LogDir, ROLLED_FILE_NAME_2);
    private static string RolledLog3Path = Path.Combine(LogDir, ROLLED_FILE_NAME_3);
    private static string HUNDRED_BYTE_STRING = new string('A', 100);

    [TestCleanup]
    public void CleanLogFiles() {
        Directory.Delete(LogDir, true);
    }

    [TestMethod]
    public void TestRollingBehavior_ShouldNotRollLogs() {
        using (FileDestination destination = new FileDestination(FileLogPath, 1, 1)) {
            destination.LogMessage(HUNDRED_BYTE_STRING, Core.LogEventLevel.VERBOSE);
        }

        Assert.IsTrue(Path.Exists(FileLogPath));
        Assert.IsFalse(Path.Exists(RolledLog1Path));
        Assert.IsFalse(Path.Exists(RolledLog2Path));
        Assert.IsFalse(Path.Exists(RolledLog3Path));
    }

    [TestMethod]
    public void TestRollingBehavior_ShouldRollOneLog() {
        using (FileDestination destination = new FileDestination(FileLogPath, 1, 1)) {
            int i = 0;
            while (i < 11) {
                destination.LogMessage(HUNDRED_BYTE_STRING, Core.LogEventLevel.VERBOSE);
                i++;
            }
        }
        
        Assert.IsTrue(Path.Exists(FileLogPath));
        Assert.IsTrue(Path.Exists(RolledLog1Path));
        Assert.IsFalse(Path.Exists(RolledLog2Path));
        Assert.IsFalse(Path.Exists(RolledLog3Path));
    }

    [TestMethod]
    public void TestRollingBehavior_ShouldRollOneLog_IfMultipleRolls() {
        using (FileDestination destination = new FileDestination(FileLogPath, 1, 1)) {
            int i = 0;
            while (i < 51) {
                destination.LogMessage(HUNDRED_BYTE_STRING, Core.LogEventLevel.VERBOSE);
                i++;
            }
        }
        
        Assert.IsTrue(Path.Exists(FileLogPath));
        Assert.IsTrue(Path.Exists(RolledLog1Path));
        Assert.IsFalse(Path.Exists(RolledLog2Path));
        Assert.IsFalse(Path.Exists(RolledLog3Path));
    }

    [TestMethod]
    public void TestRollingBehavior_ShouldRollTwoLog() {
        using (FileDestination destination = new FileDestination(FileLogPath, 2, 1)) {
            int i = 0;
            while (i < 25) {
                destination.LogMessage(HUNDRED_BYTE_STRING, Core.LogEventLevel.VERBOSE);
                i++;
            }
        }
        
        Assert.IsTrue(Path.Exists(FileLogPath));
        Assert.IsTrue(Path.Exists(RolledLog1Path));
        Assert.IsTrue(Path.Exists(RolledLog2Path));
        Assert.IsFalse(Path.Exists(RolledLog3Path));
    }

    [TestMethod]
    public void TestRollingBehavior_ShouldRollTwoLog_IfMultipleRolls() {
        using (FileDestination destination = new FileDestination(FileLogPath, 2, 1)) {
            int i = 0;
            while (i < 51) {
                destination.LogMessage(HUNDRED_BYTE_STRING, Core.LogEventLevel.VERBOSE);
                i++;
            }
        }
        
        Assert.IsTrue(Path.Exists(FileLogPath));
        Assert.IsTrue(Path.Exists(RolledLog1Path));
        Assert.IsTrue(Path.Exists(RolledLog2Path));
        Assert.IsFalse(Path.Exists(RolledLog3Path));
    }
}