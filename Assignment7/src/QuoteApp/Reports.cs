namespace QuoteApp;

public static class Reports
{
    public static IReadOnlyList<LowStockRow> LowStock(IEnumerable<Item> items, Func<Item, bool> isLow) =>
        items
            .Where(isLow)
            .Select(item => new LowStockRow(item.Name, item.Category, item.Stock))
            .OrderBy(row => row.Stock)
            .ThenBy(row => row.Name, StringComparer.Ordinal)
            .ToList();

    public static IReadOnlyList<CategoryRow> ValueByCategory(IEnumerable<Item> items) =>
        items
            .InStock()
            .GroupBy(item => item.Category)
            .Select(group => new CategoryRow(
                group.Key,
                group.Count(),
                group.Sum(item => item.Stock),
                group.Sum(item => item.StockValue())))
            .OrderByDescending(row => row.TotalValue)
            .ThenBy(row => row.Category, StringComparer.Ordinal)
            .ToList();
}

public static class StockRules
{
    public static readonly Func<int, Func<Item, bool>> IsLowStock =
        limit => item => item.Stock <= limit;

    public static readonly Func<Item, bool> Strict = IsLowStock(10);

    public static readonly Func<Item, bool> Relaxed = IsLowStock(3);
}
