# Assignment 6 - Exception Handling in FP

Student: Zhumagaliyev Aktilek, group IT-2503 (individual work)

## Project

Console quote calculator. The program loads a product catalog from `data/items.json`, then builds a price quote for an item, quantity and optional coupon.

Workflow: `find item -> validate quantity -> check stock -> calculate quote -> format text`.

Input: item id, quantity, optional coupon code. Output: a text line such as `2 x Laptop, discount 10%, total 1800.00`, or a clear failure message.

## Requirements

- .NET SDK 8.0 or newer (project targets `net8.0` with RollForward, so newer runtimes also work)
- Internet access on the first build (NuGet restore for xUnit)

## Commands

Run everything from the extracted `Assignment6` folder (the working directory matters, the app reads `data/...`).

```
dotnet build
dotnet run --project src/QuoteApp
dotnet test
```

## Types

| Type | Meaning |
|---|---|
| `Result<T>` | Success with a value, or failure with a `DomainError`. Constructor is private, so no success without a value and no failure with a value. |
| `Maybe<T>` | Value present or absent. `0`, `false`, `""` are present values. |
| `DomainError` | Code + message. A business or file failure the caller can show. |

## Outcomes

| Case | Result | Error code |
|---|---|---|
| Normal quote | `Ok("2 x Laptop, discount 10%, total 1800.00")` | - |
| Quantity below 1 | `Fail` | `INVALID_QUANTITY` (names the item and quantity) |
| Item id not in catalog | `Fail` | `ITEM_NOT_FOUND` (names the id) |
| Stock lower than quantity (stock 0 included) | `Fail` | `OUT_OF_STOCK` (item, requested, available) |
| File does not exist | `Fail` | `FILE_MISSING` (path) |
| Invalid JSON or invalid items | `Fail` | `FILE_MALFORMED` (path + reason) |
| Other IO problem | `Fail` | `FILE_UNREADABLE` (path + reason) |

Optional lookup: the coupon is optional. Empty or unknown coupon gives a 0% discount (documented behavior). Coupon `NONE` exists with value 0, which is a real value and is different from an unknown coupon.

Stock `0` is a valid value: the item exists, so the error is `OUT_OF_STOCK`, not `ITEM_NOT_FOUND`.

Validation failure (`Result`) means the request is wrong. Workflow state (`Maybe`) only says whether something was found. A required missing item is converted to a failure with `ToResult`.

## Where is what

| Requirement | File / method |
|---|---|
| Task 1 Result, three domain failures | `Result.cs`, `DomainError.cs`, `QuoteService.ValidateQuantity / FindItem / CheckStock` |
| Task 2 Maybe lookup | `Maybe.cs`, `Catalog.Find`, `Coupons.Find`, `MaybeExtensions.ToResult` |
| Task 3 Map and Bind | `ResultExtensions.Map / Bind`, chain in `QuoteService.Create` |
| Task 4 Exception boundary | `CatalogLoader.Load` (the only place with try/catch) |
| No partial update | `CatalogStore.Reload` replaces the catalog only on success |
| Task 5 Demonstration | `Program.cs` |

Map vs Bind: `Map` takes a function `T -> U` (cannot fail), `Bind` takes `T -> Result<U>` (can fail). In `Create`, `Bind` is used for validation and stock check, `Map` for building and formatting the quote.

The final result is handled once, in `Program.cs` (`Show`), with `Match`.

## Tests (xUnit, 17 test methods)

| Test | Expected |
|---|---|
| `Map_OnSuccess_TransformsValue` | 2 -> 20 |
| `Bind_OnSuccess_ReturnsNextResult` | next Result is kept (success and failure) |
| `Map_OnFailure_SkipsFunction` | counter is 0, error kept |
| `Bind_OnFailure_SkipsFunction` | counter is 0, error kept |
| `Maybe_ZeroFalseAndEmptyString_ArePresent` | `0`, `false`, `""` are present |
| `Maybe_None_IsAbsent` | absent |
| `Quote_Success_ReturnsExpectedText` | `2 x Laptop, discount 10%, total 1800.00` |
| `Quote_InvalidQuantity_Fails` (0 and -1) | `INVALID_QUANTITY` |
| `Quote_MissingItem_Fails` | `ITEM_NOT_FOUND` |
| `Quote_ZeroStock_IsOutOfStockNotMissing` | `OUT_OF_STOCK` |
| `Quote_MissingItem_StopsBeforeQuantityCheck` | `ITEM_NOT_FOUND` even with quantity -5 |
| `Coupons_ZeroDiscountIsFound_UnknownIsAbsent` | 0 found, 10 found, unknown absent |
| `Quote_UnknownCoupon_GivesNoDiscount` | discount 0% |
| `Load_MissingFile_FailsAndKeepsStore` | `FILE_MISSING`, catalog unchanged |
| `Load_MalformedJson_FailsAndKeepsStore` | `FILE_MALFORMED`, catalog unchanged |
| `Load_InvalidItems_FailsAndKeepsStore` | `FILE_MALFORMED`, catalog unchanged |
| `Load_ValidFile_ReplacesStore` | catalog replaced |

File tests use temporary files that are deleted after each test.

Latest test outcome: run `dotnet test`, expected result is all 17 passed.

## Contribution

Zhumagaliyev Aktilek, IT-2503: all code, tests and documentation of this stage.
