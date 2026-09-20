# Test cases

The program asks for the values in this order:
price, items, express, delivery type, delivery zone.

## Correct input

| # | Price | Items | Express | Type | Zone | Calculation | Result |
|---|---|---|---|---|---|---|---|
| 1 | 1000 | 2 | false | Pickup | City | 1000 x 1.00 x 0.80 x 1.00 | 800.00 |
| 2 | 1000 | 5 | false | Courier | City | 1000 x 1.10 x 1.00 x 1.00 | 1100.00 |
| 3 | 1000 | 8 | false | DoorToDoor | OutsideCity | 1000 x 1.20 x 1.15 x 1.25 | 1725.00 |
| 4 | 1000 | 3 | true | Courier | OutsideCity | 1000 x 1.00 x 1.00 x 1.25 x 1.30 | 1625.00 |
| 5 | 1000 | 10 | true | DoorToDoor | Remote | 1000 x 1.20 x 1.15 x 1.50 x 1.30 | 2691.00 |
| 6 | 0 | 5 | false | Courier | City | zero price stays zero | 0.00 |

Test 6 shows that a price of zero is allowed. It is not an error.

## Wrong input

| # | What is entered | Result |
|---|---|---|
| 7 | price = `-100` | `Error: base price cannot be negative.` and the program stops |
| 8 | price = empty line | `Error: base price must be a number.` and the program stops |
| 9 | price = `abc` | `Error: base price must be a number.` and the program stops |
| 10 | items = `two` | `Error: number of items must be a whole number.` and the program stops |
| 11 | items = `0` | `Error: number of items must be 1 or more.` and the program stops |
| 12 | express = `maybe` | `Error: express delivery must be true or false.` and the program stops |
| 13 | type = `Drone` | `Error: delivery type must be Pickup, Courier or DoorToDoor.` |
| 14 | zone = `Space` | `Error: delivery zone must be City, OutsideCity or Remote.` |

In every wrong case the program stops before the calculation and does not
throw an exception.
