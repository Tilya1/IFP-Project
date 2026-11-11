#ifndef QUOTE_NATIVE_H
#define QUOTE_NATIVE_H

#include <stdint.h>

#ifdef _WIN32
#define QN_EXPORT __declspec(dllexport)
#else
#define QN_EXPORT __attribute__((visibility("default")))
#endif

QN_EXPORT int64_t qn_discounted_total(int64_t unit_price_cents, int32_t quantity, int32_t discount_percent);

#endif
