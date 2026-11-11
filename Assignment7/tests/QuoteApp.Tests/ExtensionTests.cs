using QuoteApp;

namespace QuoteApp.Tests;

public class ExtensionTests
{
    [Fact]
    public void StockValue_IsPriceTimesStock()
    {
        var item = new Item("A", "Alpha", "X", 25.5m, 4);

        Assert.Equal(102m, item.StockValue());
    }

    [Fact]
    public void InStock_RemovesZeroStockItems()
    {
        var names = Sample.Items.InStock().Select(item => item.Name).ToList();

        Assert.DoesNotContain("Cable", names);
        Assert.Equal(6, names.Count);
    }
}
