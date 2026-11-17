namespace AlgorithmsLab3.Tests;

public sealed class ScratchFileTests
{
    [Fact]
    public void CanWriteToTemp()
    {
        var dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "algorithms-lab3-write-probe");
        System.IO.Directory.CreateDirectory(dir);
        System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "probe.txt"), "ok");
        Assert.True(System.IO.File.Exists(System.IO.Path.Combine(dir, "probe.txt")));
    }

    [Fact]
    public void CanWriteToBaseDirectory()
    {
        var path = System.IO.Path.Combine(AppContext.BaseDirectory, "write-probe.txt");
        System.IO.File.WriteAllText(path, "ok");
        Assert.True(System.IO.File.Exists(path));
    }

    [Fact]
    public void CanWriteToUserProfileTemp()
    {
        var dir = @"C:\Users\user\AppData\Local\Temp\algorithms-lab3-write-probe2";
        System.IO.Directory.CreateDirectory(dir);
        System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "probe.txt"), "ok");
        Assert.True(System.IO.File.Exists(System.IO.Path.Combine(dir, "probe.txt")));
    }
}
