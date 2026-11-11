using QuoteApp;

namespace QuoteApp.Tests;

public class LazyTests
{
    [Fact]
    public void ToLines_IsDeferredUntilEnumerated()
    {
        var calls = 0;
        var rows = Reports.ValueByCategory(Sample.Items);

        var lines = rows.ToLines(row => { calls++; return row.Category; });
        Assert.Equal(0, calls);

        var first = lines.First();
        Assert.Equal("Computers", first);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void ToLines_RunsAgainEachTime_ToListRunsOnce()
    {
        var calls = 0;
        var rows = Reports.ValueByCategory(Sample.Items);
        var lines = rows.ToLines(row => { calls++; return row.Category; });

        _ = lines.Count();
        _ = lines.Count();
        Assert.Equal(6, calls);

        calls = 0;
        var snapshot = lines.ToList();
        _ = snapshot.Count;
        _ = snapshot.Count;
        Assert.Equal(3, calls);
    }
}
