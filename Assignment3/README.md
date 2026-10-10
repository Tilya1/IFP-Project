# Assignment 3 – ColdChainMonitor

## How to run

```
cd ColdChainMonitor
dotnet run
cd ..
dotnet test
```

`dotnet run` reads `readings.txt`, prints errors and alerts, writes `archive.json` and reads it back.

## Structure

- `Models.cs` – `StorageClass`, `Reading`, `ImportError`, `Alert`, `ImportResult`, `MonitoringArchive` (records + `ImmutableList`).
- `ReadingParser.cs` – `TryParseReading`, parses one line (pure).
- `ReadingImporter.cs` – `ReadReadings(Stream)`, adds line numbers and the StorageClass rule.
- `AlertAnalyzer.cs` – `FindAlerts`, range and abrupt-change checks (pure).
- `ArchiveStorage.cs` – `WriteArchive` / `ReadArchive` with System.Text.Json.
- `Program.cs` – owns the FileStreams and the clock.

## Result for readings.txt

- 6 valid readings, 4 errors (line 7 bad timestamp, line 8 numeric enum `99`, line 9 comma `5,5`, line 10 class change).
- 2 alerts: S1 08:05 8.7 and S2 08:06 -12.0, both outside range and abrupt change.
- S1 08:15 8.0 is safe (limits are inclusive).

## Answers

**1. Who owns each stream?**
`Program.cs` opens every `FileStream` with `using`, so it owns and closes them. The helper methods only use
the stream. They create `StreamReader` with `leaveOpen: true`, so closing the reader does not close the caller's stream.

**2. Why is a lazy sequence tied to a StreamReader unsafe?**
A lazy iterator runs later, when someone loops over it. By that time the method has returned and `using` has
already disposed the reader, so reading fails. We read everything into a `List` first (materialization), so the
result does not need the reader any more.

**3. Why reset MemoryStream.Position?**
After writing, Position is at the end. Deserialization reads from the current Position, so it would see nothing.
Setting `Position = 0` moves back to the start of the written bytes.

**4. Why group before making pairs?**
If we sort all readings by time, neighbours can be from different sensors (S1 then S2). Comparing them is wrong.
So first `GroupBy(SensorId)`, then sort each group by time, then compare neighbours inside one sensor.

**5. Why is Enum.TryParse not enough?**
`Enum.TryParse("99")` returns true and gives value 99, which is not a declared member. We reject numeric text
and check `Enum.IsDefined`.

**6. Pure and I/O parts**
- Pure: `TryParseReading`, `IsOutsideRange`, `FindAlerts`.
- I/O: `ReadReadings` (reads a stream), `WriteArchive` / `ReadArchive` (stream), `Program.cs` (files, console, clock).

**7. Why compare values, not references?**
Deserialization creates new objects, so references are always different. We check that the values and the
contents of each collection are equal (`SequenceEqual`).

**8. Why deterministic order and injected CreatedAtUtc?**
Alerts are always sorted by sensor and time, and `CreatedAtUtc` comes from `Program.cs`. So tests get the same
result every time and can compare exact values.

## Debugging tasks

- **4.2** – `ReadLines` returns a lazy iterator, but `using` disposes the reader when the method returns. Fix: read into a list before returning.
- **4.3** – after `Serialize`, Position is at the end, so `Deserialize` reads nothing. Fix: `stream.Position = 0;` before reading.
- **4.4** – `OrderBy(Timestamp)` over all readings mixes sensors, so `Zip` compares S1 with S2. Fix: `GroupBy` by sensor first, then order and pair inside each group.

## Tests

14 tests in `ColdChainMonitor.Tests`: all 13 mandatory cases + 1 extra (bad timestamp). All pass.
