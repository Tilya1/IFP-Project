using QuoteApp;
using static QuoteApp.Tests.TestHelpers;

namespace QuoteApp.Tests;

public class FileTests
{
    private static CatalogStore NewStore(out Catalog original)
    {
        original = new Catalog(new[] { new Item("A-1", "Original", "Test", 1m, 1) });
        return new CatalogStore(original);
    }

    [Fact]
    public void Load_MissingFile_FailsAndKeepsStore()
    {
        var store = NewStore(out var original);
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        var error = Error(store.Reload(path));

        Assert.Equal("FILE_MISSING", error.Code);
        Assert.Contains(path, error.Message);
        Assert.Same(original, store.Current);
    }

    [Fact]
    public void Load_MalformedJson_FailsAndKeepsStore()
    {
        var store = NewStore(out var original);
        using var file = new TempFile("[ { \"id\": \"A-1\", \"price\": ");

        var error = Error(store.Reload(file.FilePath));

        Assert.Equal("FILE_MALFORMED", error.Code);
        Assert.Same(original, store.Current);
    }

    [Fact]
    public void Load_InvalidItems_FailsAndKeepsStore()
    {
        var store = NewStore(out var original);
        using var file = new TempFile("[ { \"id\": \"A-1\", \"name\": \"X\", \"category\": \"Test\", \"price\": -5, \"stock\": 1 } ]");

        var error = Error(store.Reload(file.FilePath));

        Assert.Equal("FILE_MALFORMED", error.Code);
        Assert.Same(original, store.Current);
    }

    [Fact]
    public void Load_ValidFile_ReplacesStore()
    {
        var store = NewStore(out var original);
        using var file = new TempFile("[ { \"id\": \"B-1\", \"name\": \"New\", \"category\": \"Test\", \"price\": 2, \"stock\": 3 } ]");

        var catalog = Value(store.Reload(file.FilePath));

        Assert.Equal(1, catalog.Count);
        Assert.Same(catalog, store.Current);
        Assert.NotSame(original, store.Current);
    }
}
