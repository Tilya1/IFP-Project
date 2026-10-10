using System;
using System.Collections.Immutable;

namespace ColdChainMonitor
{
    public enum StorageClass
    {
        Cold,
        Frozen
    }

    public sealed record Reading(
        string SensorId,
        DateTimeOffset Timestamp,
        StorageClass StorageClass,
        decimal Temperature);

    public sealed record ImportError(
        int LineNumber,
        string RawLine,
        string Message);

    // One alert per reading. Both flags can be true at the same time.
    public sealed record Alert(
        string SensorId,
        DateTimeOffset Timestamp,
        decimal Temperature,
        bool IsOutsideRange,
        bool IsAbruptChange);

    public sealed record ImportResult(
        ImmutableList<Reading> Readings,
        ImmutableList<ImportError> Errors);

    public sealed record MonitoringArchive(
        DateTimeOffset CreatedAtUtc,
        ImmutableList<Reading> Readings,
        ImmutableList<ImportError> Errors,
        ImmutableList<Alert> Alerts);
}
