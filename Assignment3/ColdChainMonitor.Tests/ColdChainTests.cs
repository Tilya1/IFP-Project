using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ColdChainMonitor;
using Xunit;

namespace ColdChainMonitor.Tests
{
    public class ColdChainTests
    {
        private const string SuppliedFile =
            "S1|2026-09-22T08:10:00Z|Cold|5.2\n" +
            "S2|2026-09-22T08:01:00Z|Frozen|-18.0\n" +
            "S1|2026-09-22T08:00:00Z|Cold|4.0\n" +
            "S1|2026-09-22T08:05:00Z|Cold|8.7\n" +
            "S2|2026-09-22T08:06:00Z|Frozen|-12.0\n" +
            "S1|2026-09-22T08:15:00Z|Cold|8.0\n" +
            "S3|bad-date|Cold|3.5\n" +
            "S2|2026-09-22T08:11:00Z|99|-17.0\n" +
            "S1|2026-09-22T08:20:00Z|Cold|5,5\n" +
            "S1|2026-09-22T08:25:00Z|Frozen|-17.0\n";

        private static MemoryStream ToStream(string text)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(text));
        }

        private static ImportResult Import(string text)
        {
            using (MemoryStream stream = ToStream(text))
            {
                return ReadingImporter.ReadReadings(stream);
            }
        }

        private static Reading R(string id, string time, StorageClass cls, decimal temp)
        {
            return new Reading(id, DateTimeOffset.Parse(time), cls, temp);
        }

        // 1
        [Fact]
        public void SuppliedFile_Gives6Readings_4Errors_2Alerts()
        {
            ImportResult result = Import(SuppliedFile);
            var alerts = AlertAnalyzer.FindAlerts(result.Readings);

            Assert.Equal(6, result.Readings.Count);
            Assert.Equal(4, result.Errors.Count);
            Assert.Equal(new[] { 7, 8, 9, 10 }, result.Errors.Select(e => e.LineNumber).ToArray());
            Assert.Equal(2, alerts.Count);

            Assert.Equal("S1", alerts[0].SensorId);
            Assert.Equal(8.7m, alerts[0].Temperature);
            Assert.True(alerts[0].IsOutsideRange);
            Assert.True(alerts[0].IsAbruptChange);

            Assert.Equal("S2", alerts[1].SensorId);
            Assert.Equal(-12.0m, alerts[1].Temperature);
            Assert.True(alerts[1].IsOutsideRange);
            Assert.True(alerts[1].IsAbruptChange);
        }

        // 2
        [Fact]
        public void NumericEnum99_IsRejected()
        {
            StorageClass parsed;
            Assert.True(Enum.TryParse("99", true, out parsed)); // TryParse alone accepts it

            Reading reading;
            string error;
            bool ok = ReadingParser.TryParseReading("S2|2026-09-22T08:11:00Z|99|-17.0", out reading, out error);

            Assert.False(ok);
        }

        // 3
        [Fact]
        public void CommaDecimal_IsRejected()
        {
            Reading reading;
            string error;
            bool ok = ReadingParser.TryParseReading("S1|2026-09-22T08:20:00Z|Cold|5,5", out reading, out error);

            Assert.False(ok);
        }

        // 4
        [Fact]
        public void StorageClassChange_IsImportError()
        {
            ImportResult result = Import(
                "S1|2026-09-22T08:00:00Z|Cold|4.0\n" +
                "s1|2026-09-22T08:05:00Z|Frozen|-17.0\n");

            Assert.Single(result.Readings);
            Assert.Single(result.Errors);
            Assert.Equal(2, result.Errors[0].LineNumber);
        }

        // 5
        [Theory]
        [InlineData("Cold", 8.0)]
        [InlineData("Frozen", -15.0)]
        public void BoundaryValues_AreSafe(string cls, double temp)
        {
            Reading reading = new Reading("S9", DateTimeOffset.Parse("2026-09-22T08:00:00Z"),
                Enum.Parse<StorageClass>(cls), (decimal)temp);

            Assert.False(AlertAnalyzer.IsOutsideRange(reading));
            Assert.Empty(AlertAnalyzer.FindAlerts(new[] { reading }));
        }

        // 6
        [Fact]
        public void AdjacentComparison_NeverCrossesSensors()
        {
            // By time: S1 5.0, S2 -18.0, S1 5.5, S2 -17.5. Across sensors the jump is huge,
            // inside each sensor it is only 0.5.
            Reading[] readings =
            {
                R("S1", "2026-09-22T08:00:00Z", StorageClass.Cold, 5.0m),
                R("S2", "2026-09-22T08:01:00Z", StorageClass.Frozen, -18.0m),
                R("S1", "2026-09-22T08:02:00Z", StorageClass.Cold, 5.5m),
                R("S2", "2026-09-22T08:03:00Z", StorageClass.Frozen, -17.5m)
            };

            Assert.Empty(AlertAnalyzer.FindAlerts(readings));
        }

        // 7
        [Fact]
        public void InputOrder_DoesNotChangeAlerts()
        {
            ImportResult result = Import(SuppliedFile);
            var normal = AlertAnalyzer.FindAlerts(result.Readings);
            var reversed = AlertAnalyzer.FindAlerts(result.Readings.Reverse());

            Assert.Equal(normal.ToArray(), reversed.ToArray());
        }

        // 8
        [Fact]
        public void ReadReadings_LeavesStreamOpen()
        {
            MemoryStream stream = ToStream(SuppliedFile);

            ReadingImporter.ReadReadings(stream);

            Assert.True(stream.CanRead);
        }

        // 9
        [Fact]
        public void WriteArchive_LeavesStreamOpen()
        {
            MemoryStream stream = new MemoryStream();
            MonitoringArchive archive = MakeArchive();

            ArchiveStorage.WriteArchive(stream, archive);

            Assert.True(stream.CanWrite);
        }

        // 10
        [Fact]
        public void MemoryStream_RoundTrip_KeepsValues_AndEnumNames()
        {
            MonitoringArchive archive = MakeArchive();
            MemoryStream stream = new MemoryStream();

            ArchiveStorage.WriteArchive(stream, archive);
            string json = Encoding.UTF8.GetString(stream.ToArray());
            stream.Position = 0;
            MonitoringArchive restored = ArchiveStorage.ReadArchive(stream);

            Assert.Equal(archive.CreatedAtUtc, restored.CreatedAtUtc);
            Assert.Equal(archive.Readings.ToArray(), restored.Readings.ToArray());
            Assert.Equal(archive.Errors.ToArray(), restored.Errors.ToArray());
            Assert.Equal(archive.Alerts.ToArray(), restored.Alerts.ToArray());
            Assert.Contains("\"storageClass\": \"Cold\"", json);
            Assert.DoesNotContain("\"storageClass\": 0", json);
        }

        // 11
        [Fact]
        public void Results_StillWork_AfterStreamIsDisposed()
        {
            ImportResult result;
            using (MemoryStream stream = ToStream(SuppliedFile))
            {
                result = ReadingImporter.ReadReadings(stream);
            }

            int count = 0;
            foreach (Reading reading in result.Readings)
            {
                count++;
            }
            Assert.Equal(6, count);
        }

        // 12
        [Fact]
        public void ReadArchive_LeavesStreamOpen()
        {
            MemoryStream stream = new MemoryStream();
            ArchiveStorage.WriteArchive(stream, MakeArchive());
            stream.Position = 0;

            ArchiveStorage.ReadArchive(stream);

            Assert.True(stream.CanRead);
        }

        // 13
        [Fact]
        public void ReadReadings_StartsFromCurrentPosition()
        {
            MemoryStream stream = ToStream("S1|2026-09-22T08:00:00Z|Cold|4.0\nS2|2026-09-22T08:01:00Z|Frozen|-18.0\n");
            stream.Position = Encoding.UTF8.GetByteCount("S1|2026-09-22T08:00:00Z|Cold|4.0\n");

            ImportResult result = ReadingImporter.ReadReadings(stream);

            Assert.Single(result.Readings);
            Assert.Equal("S2", result.Readings[0].SensorId);
        }

        // 14 (extra)
        [Fact]
        public void BadTimestamp_IsRejected()
        {
            Reading reading;
            string error;

            Assert.False(ReadingParser.TryParseReading("S3|bad-date|Cold|3.5", out reading, out error));
        }

        private static MonitoringArchive MakeArchive()
        {
            ImportResult result = Import(SuppliedFile);
            return new MonitoringArchive(
                DateTimeOffset.Parse("2026-10-10T12:00:00Z"),
                result.Readings,
                result.Errors,
                AlertAnalyzer.FindAlerts(result.Readings));
        }
    }
}
