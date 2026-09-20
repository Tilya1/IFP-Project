# Test cases

Input order: base price → items → delivery type → delivery zone → express.

## Valid input

| # | Price | Items | Type | Zone | Express | Calculation | Expected |
|---|---|---|---|---|---|---|---|
| 1 | 1000 | 2 | Pickup | City | false | 1000 × 1.00 × 0.80 × 1.00 × 1.00 | **800.00** |
| 2 | 1000 | 5 | Courier | City | false | 1000 × 1.10 × 1.00 × 1.00 × 1.00 | **1100.00** |
| 3 | 1000 | 8 | DoorToDoor | OutsideCity | false | 1000 × 1.20 × 1.15 × 1.25 × 1.00 | **1725.00** |
| 4 | 1000 | 3 | Courier | OutsideCity | true | 1000 × 1.00 × 1.00 × 1.25 × 1.30 | **1625.00** |
| 5 | 1000 | 10 | DoorToDoor | Remote | true | 1000 × 1.20 × 1.15 × 1.50 × 1.30 | **2691.00** |
| 6 | 1250.50 | 4 | DoorToDoor | OutsideCity | true | 1250.50 × 1.10 × 1.15 × 1.25 × 1.30 | **2570.56** |
| 7 | 0 | 5 | Courier | City | false | zero price stays zero | **0.00** |

Case 7 shows that a zero price is accepted (it is not an error) and the
result is `0.00`.

## Invalid input

| # | Input | Stage | Expected behaviour |
|---|---|---|---|
| 8 | price = `-100` | base price | `Input error: Base price cannot be negative.` — exit code 1 |
| 9 | price = `` (empty line / null) | base price | `Input error: Base price is required. Input was empty.` — exit code 1 |
| 10 | price = `abc` | base price | `Input error: 'abc' is not a valid decimal number.` — exit code 1 |
| 11 | items = `two` | item count | `Input error: 'two' is not a valid whole number.` — exit code 1 |
| 12 | items = `0` | item count | `Input error: Number of items must be at least 1.` — exit code 1 |
| 13 | type = `Drone` | delivery type | `Input error: 'Drone' is not a valid DeliveryType. Allowed values: Pickup, Courier, DoorToDoor.` — exit code 1 |
| 14 | zone = `Space` | delivery zone | `Input error: 'Space' is not a valid DeliveryZone. Allowed values: City, OutsideCity, Remote.` — exit code 1 |
| 15 | express = `maybe` | express | `Input error: 'maybe' is not a valid boolean. Enter true or false.` — exit code 1 |

In every invalid case the program stops before any calculation runs and
no unhandled exception is produced.
