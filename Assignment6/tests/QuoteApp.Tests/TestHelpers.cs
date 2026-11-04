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
