# Test cases

Input order: price, age, student, ticket type, day type.

## Correct input

| # | Price | Age | Student | Ticket | Day | Calculation | Expected | Actual |
|---|---|---|---|---|---|---|---|---|
| 1 | 5000 | 5 | false | Standard | Weekday | 5000 x 0.00 | 0.00 | 0.00 |
| 2 | 5000 | 10 | false | Standard | Weekday | 5000 x 0.50 | 2500.00 | 2500.00 |
| 3 | 5000 | 20 | true | Standard | Weekday | 5000 x 0.85 | 4250.00 | 4250.00 |
| 4 | 5000 | 65 | false | Standard | Weekday | 5000 x 0.70 | 3500.00 | 3500.00 |
| 5 | 5000 | 30 | false | Vip | Weekday | 5000 x 1.25 | 6250.00 | 6250.00 |
| 6 | 5000 | 30 | false | Standard | Weekend | 5000 x 1.10 | 5500.00 | 5500.00 |
| 7 | 5000 | 20 | true | Vip | Weekend | 5000 x 0.85 x 1.25 x 1.10 | 5843.75 | 5843.75 |
| 8 | 0 | 30 | false | Standard | Weekday | zero price stays zero | 0.00 | 0.00 |

## Wrong input

| # | What is entered | Result |
|---|---|---|
| 9 | price = `-100` | `Error: base price cannot be negative.` |
| 10 | price = empty line | `Error: base price is missing.` |
| 11 | price = null (Ctrl+Z / end of input) | `Error: base price is missing.` |
| 12 | age = `abc` | `Error: age must be a whole number.` |
| 13 | student = `maybe` | `Error: student status must be true or false.` |
| 14 | ticket = `Gold` | `Error: ticket type must be Standard or Vip.` |
| 15 | day = `Holiday` | `Error: day type must be Weekday or Weekend.` |

In every wrong case the program stops before the calculation and does not
throw an exception.
