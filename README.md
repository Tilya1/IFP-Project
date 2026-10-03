# Assignment 2 – Telecom Call Processing

## How to build, run and test

```
dotnet build
dotnet run --project CallProcessing
dotnet test
```

## Structure

- `CallProcessing/CallRecord.cs` – `readonly record struct` with validation in the constructor.
- `CallProcessing/CallPricing.cs` – `CalculateCost`, one switch expression with all tariffs.
- `CallProcessing/CallProcessor.cs` – `ProcessCallsSequential` and `ProcessCallsParallel` (two threads).
- `CallProcessing/Program.cs` – small console demo.
- `CallProcessing.Tests/CallTests.cs` – xUnit tests (groups A, B, C).

## Tariffs (order matters, first match wins)

1. Invalid record / NaN / out of range -> `ArgumentException`
2. Roaming + KZ + duration < 1.0 -> 50.00 flat
3. Non-roaming + KZ -> 15.00 per minute
4. Roaming + duration >= 10.0 -> 120.00 per minute
5. Everything else -> 45.00 per minute (fallback)

Each cost is rounded to 2 decimals with `MidpointRounding.AwayFromZero`.

## Pure and impure parts

- **Pure:** `CalculateCost`. It uses only its parameter, returns a value, does not use
  Console, global variables or change any state. Same input -> same output.
- **Impure:**
  - `ProcessCallsParallel` creates threads and writes into its own output arrays.
  - `Program.Main` writes to the Console.
- `ProcessCallsSequential` only adds numbers into a local variable; it is the correctness baseline.

## Why readonly does not mean valid

`readonly record struct` only means the fields cannot change after creation.
`default(CallRecord)` skips the constructor, so its `RecordId` and `DestinationCountry`
are `null`. That is why `CalculateCost` checks the record again before pricing.

## Task 3 – the race

`globalCallCounter++` is three steps: read, add 1, write. Possible lost update:

1. Thread A reads 5.
2. Thread B reads 5.
3. Thread A writes 6.
4. Thread B writes 6.

Two calls were processed, but the counter grew only by 1. One update is lost.
A sequential loop has no race, because only one thread changes the variable.

## Partitioning

The array is split with `[..mid]` and `[mid..]`. Each thread has its own input part and its
own `decimal[]` output array, so the threads never write to the same memory. No `lock` is needed.
`Join()` waits until both threads finish, so reading the arrays after `Join()` is safe.
If a worker throws, the error is saved, both threads are joined, and the error is thrown again on
the calling thread. No partial total is returned.

## Test results

24 tests, all passed:

- Group A – 6 tariff cases.
- Group B – constructor validation, `default(CallRecord)`, zero minutes, boundaries 0.999/1.0 and 9.999/10.0.
- Group C – 1000 records, parallel run 100 times equals sequential, input unchanged, empty, odd and null arrays.

## Limitations

- Parallel processing needs an even-length array.
- For small arrays two threads are not faster than a simple loop (creating threads costs time).
- Passing tests are evidence, not a proof, that there is no race.
