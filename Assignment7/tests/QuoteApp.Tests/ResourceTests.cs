using QuoteApp;
using static QuoteApp.Tests.TestHelpers;

namespace QuoteApp.Tests;

public class ResourceTests
{
    [Fact]
    public void SaveTo_Success_WritesLinesAndDisposesWriter()
    {
        var writer = new TrackingWriter();

        var count = Value(ReportExporter.SaveTo(() => writer, new[] { "a", "b" }, "memory"));

        Assert.Equal(2, count);
        Assert.Equal("a" + Environment.NewLine + "b" + Environment.NewLine, writer.ToString());
        Assert.True(writer.Disposed);
    }

    [Fact]
    public void SaveTo_WriteFails_ReturnsErrorAndStillDisposes()
    {
        var writer = new TrackingWriter(fail: true);

        var error = Error(ReportExporter.SaveTo(() => writer, new[] { "a" }, "memory"));

        Assert.Equal("FILE_UNWRITABLE", error.Code);
        Assert.True(writer.Disposed);
    }

    [Fact]
    public void Save_RealFile_CanBeReadBack()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        try
        {
            Value(ReportExporter.Save(path, new[] { "line 1", "line 2" }));

            Assert.Equal(new[] { "line 1", "line 2" }, File.ReadAllLines(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Save_MissingFolder_ReturnsError()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "report.txt");

        var error = Error(ReportExporter.Save(path, new[] { "x" }));

        Assert.Equal("FILE_UNWRITABLE", error.Code);
    }
}
