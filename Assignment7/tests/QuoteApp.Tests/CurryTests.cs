using QuoteApp;

namespace QuoteApp.Tests;

public class CurryTests
{
    [Fact]
    public void IsLowStock_LimitIsIncluded()
    {
        var atMostFive = StockRules.IsLowStock(5);

        Assert.True(atMostFive(new Item("A", "A", "X", 1m, 5)));
        Assert.False(atMostFive(new Item("B", "B", "X", 1m, 6)));
    }

    [Fact]
    public void StrictAndRelaxed_GiveDifferentResultsOnSameData()
    {
        var strict = Reports.LowStock(Sample.Items, StockRules.Strict);
        var relaxed = Reports.LowStock(Sample.Items, StockRules.Relaxed);

        Assert.Equal(new[] { "Cable", "Keyboard", "Headphones", "Laptop", "Monitor" }, strict.Select(row => row.Name));
        Assert.Equal(new[] { "Cable", "Keyboard", "Headphones" }, relaxed.Select(row => row.Name));
    }
}
