# Assignment 7 - Extension Methods, LINQ and Native Interop

Student: Zhumagaliyev Aktilek, group IT-2503 (individual work)

## Project

This assignment extends the QuoteApp project from Assignment 6. The app loads a product catalog from `data/items.json` and then does four things:

- builds two LINQ reports (low stock, and stock value by category);
- exports the category report to `output/category-report.txt`;
- calculates a price quote, with the total computed by a small C library;
- shows failure cases: invalid quantity, out of stock, a broken file and a bad export path.

## Requirements

- .NET SDK 8.0 or newer. The project targets `net8.0` with `RollForward Major`, so it also runs on .NET 10.
- A C compiler. On macOS this is `clang` from Xcode Command Line Tools (`xcode-select --install`). On Linux it is `gcc`.
- Internet access on the first build, for the NuGet restore of xUnit.

## Commands

Run all commands from the `Assignment7` folder. The working directory matters because the app reads `data/...`.

```
sh native/build.sh
dotnet build
dotnet run --project src/QuoteApp
dotnet test
```

Build the native library first. The C# projects copy it from `native/build/` into their `bin` folders during `dotnet build`.

## Requirement map

| Requirement | File | Method / type |
|---|---|---|
| Extension methods | `src/QuoteApp/ItemExtensions.cs` | `StockValue`, `InStock`, `ToLines` |
| LINQ report 1 | `src/QuoteApp/Reports.cs` | `Reports.LowStock` |
| LINQ report 2 (multi-stage) | `src/QuoteApp/Reports.cs` | `Reports.ValueByCategory` |
| Currying and partial application | `src/QuoteApp/Reports.cs` | `StockRules.IsLowStock`, `Strict`, `Relaxed` |
| `yield return` and deferred execution | `src/QuoteApp/ItemExtensions.cs` | `ToLines` |
| Materialisation | `src/QuoteApp/Reports.cs`, `Program.cs` | `.ToList()` |
| `using` and disposal | `src/QuoteApp/ReportExporter.cs` | `SaveTo` |
| JSON loading | `src/QuoteApp/CatalogLoader.cs` | `CatalogLoader.Load` |
| C library | `native/quote_native.c`, `native/quote_native.h`, `native/build.sh` | `qn_discounted_total` |
| P/Invoke binding | `src/QuoteApp/NativeTotal.cs` | `NativeMethods.DiscountedTotal` |
| Adapter returning Result | `src/QuoteApp/NativeTotal.cs` | `NativeTotalAdapter.Total` |
| Native call in the workflow | `src/QuoteApp/QuoteService.cs`, `Program.cs` | `QuoteService.Create(..., native.Total)` |
| Console and file IO | `src/QuoteApp/Program.cs` | top-level program |

## Extension methods

`StockValue(this Item item)` extends one `Item` and returns `Price * Stock`. It is used inside `ValueByCategory` to sum the value of a category.

`InStock(this IEnumerable<Item> items)` extends a sequence of items and keeps only the items with `Stock > 0`. It is the first stage of `ValueByCategory`.

`ToLines<T>(this IEnumerable<T> rows, Func<T, string> format)` is generic. It extends a sequence of any row type and turns each row into a text line. `Program.cs` uses it to print both reports and to prepare the lines for export.

The receiver is the type after `this`. Because `InStock` and `ToLines` extend `IEnumerable<T>`, they work on arrays, lists and LINQ results, and they can be chained like normal LINQ methods.

## LINQ reports

Both reports are pure functions in `Reports.cs`. They take items and return a list. They do not touch the console or files. All IO is in `Program.cs`, `CatalogLoader.cs` and `ReportExporter.cs`.

**Report 1, `LowStock(items, isLow)`**

`Where(isLow) -> Select(LowStockRow) -> OrderBy(Stock) -> ThenBy(Name) -> ToList`

It lists the items that pass the filter, with the lowest stock first. When two items have the same stock, they are sorted by name.

**Report 2, `ValueByCategory(items)`** (multi-stage)

`InStock -> GroupBy(Category) -> Select(Count, Sum(Stock), Sum(StockValue)) -> OrderByDescending(TotalValue) -> ThenBy(Category) -> ToList`

It groups the in-stock items by category and counts products, units and total value. The most valuable category comes first. When two categories have the same value, they are sorted by category name.

Expected output on `data/items.json`:

```
Computers   products 2, units 13, value 7000.00
Accessories products 2, units 42, value 1110.00
Audio       products 2, units 15, value 960.00
```

The Cable has stock 0, so it is not counted in Accessories.

## Currying and partial application

```csharp
Func<int, Func<Item, bool>> IsLowStock = limit => item => item.Stock <= limit;
Func<Item, bool> Strict  = IsLowStock(10);
Func<Item, bool> Relaxed = IsLowStock(3);
```

`IsLowStock` takes the limit first and returns a filter function. `Strict` and `Relaxed` are two configured versions of it. They are passed into `Reports.LowStock` as an argument, so there is no global setting that can change.

On the same data, `Strict` returns 5 items (Cable, Keyboard, Headphones, Laptop, Monitor) and `Relaxed` returns 3 items (Cable, Keyboard, Headphones).

## Deferred execution

`ToLines` uses `yield return`. Calling it does not run anything yet. A line is formatted only when someone asks for it with `foreach`, `First()`, `Count()` or `ToList()`.

`LazyTests` shows this with a counter inside the format function:

- after calling `ToLines`, the counter is 0;
- after `First()`, the counter is 1;
- `Count()` called twice on the same lazy sequence runs the formatter twice (6 calls for 3 rows);
- after `ToList()`, the list is a snapshot. Reading it again does not call the formatter again.

## Resources and disposal

| Resource | Owner | Released |
|---|---|---|
| `StreamWriter` for the report file | `ReportExporter.SaveTo` | at the end of the `try` block by `using var writer` |
| Catalog JSON file | `File.ReadAllText` in `CatalogLoader.Load` | inside `File.ReadAllText` |

`SaveTo` receives a function that opens the writer, so the writer is created and owned inside `SaveTo`. `using` disposes it when the method leaves the `try` block. This happens on success and also when `WriteLine` throws. The exception is caught and returned as a `FILE_UNWRITABLE` failure. `ResourceTests` checks disposal in both cases with a `TrackingWriter`.

`File.ReadAllText` is a convenience API. It opens the file, reads all of it and closes it by itself, so there is no handle for our code to dispose. Missing, malformed and unreadable files become `FILE_MISSING`, `FILE_MALFORMED` and `FILE_UNREADABLE` failures.

## Resource note

Data is materialised at three points:

1. `CatalogLoader.Load` reads the whole JSON file into a string and then into a `List<Item>`. `Catalog` copies it into an array.
2. Each report ends with `ToList()`, so a report is computed once and stored.
3. In `Program.cs`, `ToLines(...).ToList()` stores the formatted lines before printing and exporting them.

The catalog stays in memory while the program runs, inside `CatalogStore.Current`. Reports and lines are small and are kept only until `Program` ends.

Trade-off: streaming with `yield return` uses less memory and does no work until it is needed, but every enumeration repeats the work, and the source could change between enumerations. A snapshot (`ToList`) costs memory once, but it is computed one time and gives the same result every time it is read. This project uses snapshots because the data is small and the same lines are both printed and exported.

## Native library

### Contract

```c
int64_t qn_discounted_total(int64_t unit_price_cents, int32_t quantity, int32_t discount_percent);
```

| Parameter | Type | Valid range |
|---|---|---|
| `unit_price_cents` | `int64_t` / C# `long` | 0 to 100000000 (1,000,000.00) |
| `quantity` | `int32_t` / C# `int` | 1 to 10000 |
| `discount_percent` | `int32_t` / C# `int` | 0 to 100 |

It returns the total in cents: `price * quantity * (100 - percent) / 100`, rounded half up to a whole cent.

On invalid input it does not crash. It returns a negative code: `-1` bad price, `-2` bad quantity, `-3` bad discount.

Example: `qn_discounted_total(100000, 2, 10)` returns `180000`, which is 1800.00.

### C# side

`NativeMethods.DiscountedTotal` is the `[DllImport("quotenative")]` binding with `CallingConvention.Cdecl`. .NET adds the platform name, so it loads `libquotenative.dylib` on macOS and `libquotenative.so` on Linux.

`NativeTotalAdapter.Total` is the safe wrapper. It:

- checks price, quantity and discount before calling C, and returns `INVALID_PRICE`, `INVALID_QUANTITY` or `INVALID_DISCOUNT`;
- converts the decimal price to cents;
- turns a negative native result into `NATIVE_REJECTED`;
- turns `DllNotFoundException`, `EntryPointNotFoundException` and `BadImageFormatException` into `NATIVE_UNAVAILABLE`.

So the rest of the program only sees `Result<decimal>`.

The adapter has a second constructor that takes a function instead of the real native call. Tests use it to simulate a missing library.

### Platform

| Item | Value |
|---|---|
| Declared OS | macOS (Apple Silicon, arm64). Also built and tested on Linux x64 with gcc. |
| Compiler | Apple clang (Xcode Command Line Tools) on macOS, gcc on Linux |
| Build command | `sh native/build.sh` |
| Binary location | `native/build/libquotenative.dylib` (macOS) or `native/build/libquotenative.so` (Linux), copied to `bin/Debug/net8.0/` by `dotnet build` |

The binary is not committed to Git (`native/build/` is in `.gitignore`), because it must be built for the machine that runs it.

### Integration difficulty

The first problem was that .NET could not find the library. `DllImport` only searches the app's output folder and system paths, not `native/build/`. I fixed it by adding a `<None Include="..\..\native\build\*.*">` item with `CopyToOutputDirectory` to both `.csproj` files, so the library is copied next to the `.dll`. Because of this, `sh native/build.sh` must run before `dotnet build`.

A second point was money. A C `double` can give values like `1799.9999`. I pass the price as whole cents in `int64_t` and round in integer math, so the result is exact.

## Tests

40 test methods (44 cases with the `InlineData` rows) in `tests/QuoteApp.Tests`.

| File | Tests | Kind |
|---|---|---|
| `ReportTests.cs` | sorting with ties by name, empty input, no matches, group counts and sums, ties by category name, empty grouping | pure |
| `ExtensionTests.cs` | `StockValue`, `InStock` removes zero stock | pure |
| `CurryTests.cs` | limit is included, Strict and Relaxed differ on the same data | pure |
| `LazyTests.cs` | deferred until enumerated, repeat enumeration vs `ToList` snapshot | pure |
| `QuoteTests.cs` | success, invalid quantity, missing item, out of stock, short-circuit, coupons | pure |
| `ResultTests.cs` | Map/Bind on success and failure, Maybe | pure |
| `NativeTests.cs` | real native call vs manual values, missing library gives Result failure, negative code, invalid input never reaches C | native |
| `ResourceTests.cs` | disposed on success, disposed on failure, real file, missing folder | resource |
| `FileTests.cs` | missing file, malformed JSON, invalid items, valid file | file |
| `WorkflowTests.cs` | JSON -> report -> export file; JSON -> native quote; quote with missing library | complete workflow |

Manual values for the native test: `1000.00 x 2 - 10% = 1800.00`, `25.50 x 3 - 10% = 68.85`, `3.33 x 1 - 15% = 2.8305 -> 2.83`.

## Contribution

Individual work. All code, tests and this README were done by Zhumagaliyev Aktilek.
