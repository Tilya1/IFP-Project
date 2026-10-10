using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ColdChainMonitor
{
    public static class ArchiveStorage
    {
        public static JsonSerializerOptions CreateOptions()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.WriteIndented = true;
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        // Writes at the current Position, leaves the stream open.
        public static void WriteArchive(Stream output, MonitoringArchive archive)
        {
            JsonSerializer.Serialize(output, archive, CreateOptions());
        }

        // Reads from the current Position, leaves the stream open.
        // Bad JSON throws JsonException, "null" throws InvalidDataException.
        public static MonitoringArchive ReadArchive(Stream input)
        {
            MonitoringArchive archive = JsonSerializer.Deserialize<MonitoringArchive>(input, CreateOptions());
            if (archive == null)
            {
                throw new InvalidDataException("Archive is null.");
            }
            return archive;
        }
    }
}
