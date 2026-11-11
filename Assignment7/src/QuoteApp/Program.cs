using System.Globalization;
using QuoteApp;

var store = new CatalogStore(new Catalog(Array.Empty<Item>()));
var native = new NativeTotalAdapter();

Console.WriteLine("1. Load catalog");
Show(store.Reload("data/items.json").Map(catalog => $"loaded {catalog.Count} items"));
var items = store.Current.Items;

Console.WriteLine("2. Report 1: low stock (strict, stock <= 10)");
Print(Reports.LowStock(items, StockRules.Strict).ToLines(FormatLow));

Console.WriteLine("3. Report 1: low stock (relaxed, stock <= 3)");
Print(Reports.LowStock(items, StockRules.Relaxed).ToLines(FormatLow));

Console.WriteLine("4. Report 2: stock value by category");
var categoryLines = Reports.ValueByCategory(items).ToLines(FormatCategory).ToList();
Print(categoryLines);

Console.WriteLine("5. Export report 2");
Directory.CreateDirectory("output");
Show(ReportExporter.Save("output/category-report.txt", categoryLines)
    .Map(count => $"{count} lines saved to output/category-report.txt"));

Console.WriteLine("6. Quote with native C calculation");
Show(QuoteService.Create(store.Current, new QuoteRequest("LAP-1", 2, "SAVE10"), native.Total));

Console.WriteLine("7. Domain failure: invalid quantity");
Show(QuoteService.Create(store.Current, new QuoteRequest("LAP-1", 0), native.Total));

Console.WriteLine("8. Domain failure: out of stock");
Show(QuoteService.Create(store.Current, new QuoteRequest("CAB-1", 1), native.Total));

Console.WriteLine("9. File failure: malformed file");
Show(store.Reload("data/broken.json").Map(catalog => $"loaded {catalog.Count} items"));

Console.WriteLine("10. Export failure: folder does not exist");
Show(ReportExporter.Save("no-such-folder/report.txt", categoryLines).Map(count => $"{count} lines saved"));

Console.WriteLine($"Catalog after failures still has {store.Current.Count} items.");

static string FormatLow(LowStockRow row) =>
    $"{row.Name,-12}{row.Category,-12}stock {row.Stock}";

static string FormatCategory(CategoryRow row) =>
    $"{row.Category,-12}products {row.Products}, units {row.Units}, value {row.TotalValue.ToString("F2", CultureInfo.InvariantCulture)}";

static void Print(IEnumerable<string> lines)
{
    var count = 0;
    foreach (var line in lines)
    {
        Console.WriteLine("   " + line);
        count++;
    }
    if (count == 0)
        Console.WriteLine("   (no records)");
}

static void Show<T>(Result<T> result) =>
    Console.WriteLine("   " + result.Match(
        value => "OK: " + value,
        error => $"FAIL [{error.Code}] {error.Message}"));
