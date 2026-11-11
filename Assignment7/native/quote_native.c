#include "quote_native.h"

int64_t qn_discounted_total(int64_t unit_price_cents, int32_t quantity, int32_t discount_percent)
{
    if (unit_price_cents < 0 || unit_price_cents > 100000000)
        return -1;
    if (quantity < 1 || quantity > 10000)
        return -2;
    if (discount_percent < 0 || discount_percent > 100)
        return -3;

    int64_t full = unit_price_cents * quantity;
    return (full * (100 - discount_percent) + 50) / 100;
}
