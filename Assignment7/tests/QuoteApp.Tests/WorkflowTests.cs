using QuoteApp;
using static QuoteApp.Tests.TestHelpers;

namespace QuoteApp.Tests;

public class WorkflowTests
{
    private const string Json =
        "[ { \"id\": \"LAP-1\", \"name\": \"Laptop\", \"category\": \"Computers\", \"price\": 1000, \"stock\": 5 }," +
        "  { \"id\": \"KEY-1\", \"name\": \"Keyboard\", \"category\": \"Accessories\", \"price\": 45, \"stock\": 2 } ]";

    [Fact]
    public void LoadReportAndExport_WritesExpectedFile()
    {
        using var data = new TempFile(Json);
        var output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        try
        {
            var catalog = Value(CatalogLoader.Load(data.FilePath));
            var lines = Reports.LowStock(catalog.Items, StockRules.Relaxed)
                .ToLines(row => $"{row.Name}:{row.Stock}")
                .ToList();

            Value(ReportExporter.Save(output, lines));

            Assert.Equal(new[] { "Keyboard:2" }, File.ReadAllLines(output));
        }
        finally
        {
            File.Delete(output);
        }
    }

    [Fact]
    public void LoadAndQuote_UsesNativeCalculation()
    {
        using var data = new TempFile(Json);
        var catalog = Value(CatalogLoader.Load(data.FilePath));
        var native = new NativeTotalAdapter();

        var quote = QuoteService.Create(catalog, new QuoteRequest("LAP-1", 2, "SAVE10"), native.Total);

        Assert.Equal("2 x Laptop, discount 10%, total 1800.00", Value(quote));
    }

    [Fact]
    public void Quote_NativeUnavailable_ReturnsFailure()
    {
        using var data = new TempFile(Json);
        var catalog = Value(CatalogLoader.Load(data.FilePath));
        var broken = new NativeTotalAdapter((p, q, d) => throw new DllNotFoundException("missing"));

        var error = Error(QuoteService.Create(catalog, new QuoteRequest("LAP-1", 1), broken.Total));

        Assert.Equal("NATIVE_UNAVAILABLE", error.Code);
    }
}
