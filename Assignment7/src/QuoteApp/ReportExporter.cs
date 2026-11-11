namespace QuoteApp;

public static class ReportExporter
{
    public static Result<int> Save(string path, IReadOnlyList<string> lines) =>
        SaveTo(() => new StreamWriter(path), lines, path);

    public static Result<int> SaveTo(Func<TextWriter> open, IReadOnlyList<string> lines, string name)
    {
        try
        {
            using var writer = open();
            foreach (var line in lines)
                writer.WriteLine(line);
            return Result<int>.Ok(lines.Count);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return Result<int>.Fail(Errors.FileUnwritable(name, e.Message));
        }
    }
}
