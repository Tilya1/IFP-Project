using System;

namespace CallProcessing
{
    public static class CallPricing
    {
        public static decimal CalculateCost(in CallRecord record)
        {
            decimal cost = record switch
            {
                // 1. Invalid record (also default(CallRecord))
                { RecordId: null } or { DestinationCountry: null }
                    => throw new ArgumentException("Invalid record."),
                _ when string.IsNullOrWhiteSpace(record.RecordId) || string.IsNullOrWhiteSpace(record.DestinationCountry)
                    => throw new ArgumentException("Invalid record."),
                { DurationMinutes: double.NaN }
                    => throw new ArgumentException("Duration is NaN."),
                _ when double.IsInfinity(record.DurationMinutes) || record.DurationMinutes < 0 || record.DurationMinutes > 10000
                    => throw new ArgumentException("Duration is out of range."),

                // 2. Roaming + KZ + less than 1 minute
                { IsRoaming: true, DestinationCountry: "KZ", DurationMinutes: < 1.0 }
                    => 50.00m,

                // 3. Non-roaming + KZ
                { IsRoaming: false, DestinationCountry: "KZ" }
                    => (decimal)record.DurationMinutes * 15.00m,

                // 4. Roaming + 10 minutes or more
                { IsRoaming: true, DurationMinutes: >= 10.0 }
                    => (decimal)record.DurationMinutes * 120.00m,

                // 5. Everything else
                _ => (decimal)record.DurationMinutes * 45.00m
            };

            return Math.Round(cost, 2, MidpointRounding.AwayFromZero);
        }
    }
}
