using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ColdChainMonitor
{
    // Pure: works only with the readings it receives.
    public static class AlertAnalyzer
    {
        public static bool IsOutsideRange(Reading reading)
        {
            switch (reading.StorageClass)
            {
                case StorageClass.Cold:
                    return reading.Temperature < 2.0m || reading.Temperature > 8.0m;
                case StorageClass.Frozen:
                    return reading.Temperature < -22.0m || reading.Temperature > -15.0m;
                default:
                    return false;
            }
        }

        public static ImmutableList<Alert> FindAlerts(IEnumerable<Reading> readings)
        {
            List<Alert> alerts = new List<Alert>();

            // 1. Group by sensor first, so we never compare two different sensors
            var groups = readings.GroupBy(r => r.SensorId, StringComparer.OrdinalIgnoreCase);

            foreach (var group in groups)
            {
                string sensorName = group.First().SensorId;

                // 2. Sort by time (OrderBy is stable, equal times keep input order)
                List<Reading> ordered = group.OrderBy(r => r.Timestamp).ToList();

                // 3. Compare each reading with the previous one of the same sensor
                for (int i = 0; i < ordered.Count; i++)
                {
                    Reading current = ordered[i];
                    bool outside = IsOutsideRange(current);
                    bool abrupt = false;
                    if (i > 0)
                    {
                        Reading previous = ordered[i - 1];
                        abrupt = Math.Abs(current.Temperature - previous.Temperature) > 4.0m;
                    }

                    if (outside || abrupt)
                    {
                        alerts.Add(new Alert(sensorName, current.Timestamp, current.Temperature, outside, abrupt));
                    }
                }
            }

            return alerts
                .OrderBy(a => a.SensorId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(a => a.Timestamp)
                .ToImmutableList();
        }
    }
}
