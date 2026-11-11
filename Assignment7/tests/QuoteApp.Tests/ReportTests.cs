using QuoteApp;

namespace QuoteApp.Tests;

public class ReportTests
{
    [Fact]
    public void LowStock_SortsByStockThenName()
    {
        var items = new[]
        {
            new Item("B", "Bravo", "X", 1m, 2),
            new Item("A", "Alpha", "X", 1m, 2),
            new Item("C", "Charlie", "X", 1m, 1),
            new Item("D", "Delta", "X", 1m, 50)
        };

        var rows = Reports.LowStock(items, StockRules.Strict);

        Assert.Equal(new[] { "Charlie", "Alpha", "Bravo" }, rows.Select(row => row.Name));
    }

    [Fact]
    public void LowStock_EmptyInput_ReturnsEmptyList()
    {
        var rows = Reports.LowStock(Array.Empty<Item>(), StockRules.Strict);

        Assert.Empty(rows);
    }

    [Fact]
    public void LowStock_NoMatches_ReturnsEmptyList()
    {
        var items = new[] { new Item("A", "Alpha", "X", 1m, 100) };

        var rows = Reports.LowStock(items, StockRules.Strict);

        Assert.Empty(rows);
    }

    [Fact]
    public void ValueByCategory_GroupsCountsAndSums()
    {
        var rows = Reports.ValueByCategory(Sample.Items);

        Assert.Equal(new[] { "Computers", "Accessories", "Audio" }, rows.Select(row => row.Category));
        Assert.Equal(new CategoryRow("Computers", 2, 13, 7000m), rows[0]);
        Assert.Equal(new CategoryRow("Accessories", 2, 42, 1110m), rows[1]);
        Assert.Equal(new CategoryRow("Audio", 2, 15, 960m), rows[2]);
    }

    [Fact]
    public void ValueByCategory_EqualValues_SortedByCategoryName()
    {
        var items = new[]
        {
            new Item("A", "One", "Zeta", 10m, 1),
            new Item("B", "Two", "Alpha", 5m, 2)
        };

        var rows = Reports.ValueByCategory(items);

        Assert.Equal(new[] { "Alpha", "Zeta" }, rows.Select(row => row.Category));
    }

    [Fact]
    public void ValueByCategory_EmptyInput_ReturnsEmptyList()
    {
        Assert.Empty(Reports.ValueByCategory(Array.Empty<Item>()));
    }
}
