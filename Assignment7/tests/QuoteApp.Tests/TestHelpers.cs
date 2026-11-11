using QuoteApp;

namespace QuoteApp.Tests;

public static class TestHelpers
{
    public static T Value<T>(Result<T> result) =>
        result.Match<T>(
            value => value,
            error => throw new InvalidOperationException("Expected success, got " + error.Code));

    public static DomainError Error<T>(Result<T> result) =>
        result.Match<DomainError>(
            value => throw new InvalidOperationException("Expected failure, got success."),
            error => error);
}

public sealed class TempFile : IDisposable
{
    public TempFile(string content)
    {
        FilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        File.WriteAllText(FilePath, content);
    }

    public string FilePath { get; }

    public void Dispose() => File.Delete(FilePath);
}

public static class Sample
{
    public static readonly Item[] Items =
    {
        new("LAP-1", "Laptop", "Computers", 1000m, 5),
        new("MON-1", "Monitor", "Computers", 250m, 8),
        new("MOU-1", "Mouse", "Accessories", 25.5m, 40),
        new("KEY-1", "Keyboard", "Accessories", 45m, 2),
        new("CAB-1", "Cable", "Accessories", 5m, 0),
        new("HDP-1", "Headphones", "Audio", 80m, 3),
        new("SPK-1", "Speaker", "Audio", 60m, 12)
    };
}

public class TrackingWriter : StringWriter
{
    private readonly bool _fail;

    public TrackingWriter(bool fail = false) => _fail = fail;

    public bool Disposed { get; private set; }

    public override void WriteLine(string? value)
    {
        if (_fail) throw new IOException("disk is full");
        base.WriteLine(value);
    }

    protected override void Dispose(bool disposing)
    {
        Disposed = true;
        base.Dispose(disposing);
    }
}
