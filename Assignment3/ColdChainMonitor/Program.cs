using System;
using System.IO;
using System.Linq;

namespace ColdChainMonitor
{
    class Program
    {
        static void Main()
        {
            // The clock is read only here and passed into the archive
            DateTimeOffset createdAtUtc = DateTimeOffset.UtcNow;

            // 1. Import
            ImportResult result;
            using (FileStream input = File.OpenRead("readings.txt"))
            {
                result = ReadingImporter.ReadReadings(input);
            }

            // 2. Analyze
            var alerts = AlertAnalyzer.FindAlerts(result.Readings);
            MonitoringArchive archive = new MonitoringArchive(createdAtUtc, result.Readings, result.Errors, alerts);

            // 3. Write JSON
            using (FileStream output = File.Create("archive.json"))
            {
                ArchiveStorage.WriteArchive(output, archive);
            }

            // 4. Read JSON back
            MonitoringArchive restored;
            using (FileStream input = File.OpenRead("archive.json"))
            {
                restored = ArchiveStorage.ReadArchive(input);
            }

            // 5. Print
            Console.WriteLine("Valid readings: " + result.Readings.Count);
            Console.WriteLine("Import errors:  " + result.Errors.Count);
            foreach (ImportError error in result.Errors)
            {
                Console.WriteLine("  line " + error.LineNumber + ": " + error.Message + " -> " + error.RawLine);
            }

            Console.WriteLine("Alerts:         " + alerts.Count);
            foreach (Alert alert in alerts)
            {
                string reasons = "";
                if (alert.IsOutsideRange) reasons += "Outside range; ";
                if (alert.IsAbruptChange) reasons += "abrupt change";
                Console.WriteLine("  " + alert.SensorId + " " + alert.Timestamp.ToString("HH:mm") + " " + alert.Temperature + " -> " + reasons);
            }

            bool same = restored.CreatedAtUtc == archive.CreatedAtUtc
                && restored.Readings.SequenceEqual(archive.Readings)
                && restored.Errors.SequenceEqual(archive.Errors)
                && restored.Alerts.SequenceEqual(archive.Alerts);
            Console.WriteLine("Round trip OK:  " + same);
        }
    }
}
