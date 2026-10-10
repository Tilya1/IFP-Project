using System;
using System.Globalization;

namespace ColdChainMonitor
{
    // Pure: no files, no clock, no global state.
    public static class ReadingParser
    {
        public static bool TryParseReading(string line, out Reading reading, out string error)
        {
            reading = null;
            error = null;

            string[] parts = line.Split('|');
            if (parts.Length != 4)
            {
                error = "Line must have exactly 4 fields.";
                return false;
            }

            string sensorId = parts[0].Trim();
            string timestampText = parts[1].Trim();
            string classText = parts[2].Trim();
            string temperatureText = parts[3].Trim();

            // 1. Sensor id
            if (sensorId.Length == 0)
            {
                error = "SensorId is empty.";
                return false;
            }

            // 2. Timestamp, Z means UTC (+00:00)
            DateTimeOffset timestamp;
            if (!DateTimeOffset.TryParseExact(timestampText, "yyyy-MM-dd'T'HH:mm:ss'Z'",
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out timestamp)
                || timestamp.Offset != TimeSpan.Zero)
            {
                error = "Invalid timestamp.";
                return false;
            }

            // 3. Storage class: names only, numbers like 99 are rejected
            StorageClass storageClass;
            if (int.TryParse(classText, out _)
                || !Enum.TryParse(classText, true, out storageClass)
                || !Enum.IsDefined(storageClass))
            {
                error = "Invalid storage class.";
                return false;
            }

            // 4. Temperature: only sign and dot, so 5,5 is rejected
            decimal temperature;
            if (!decimal.TryParse(temperatureText,
                    NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out temperature))
            {
                error = "Invalid temperature.";
                return false;
            }

            reading = new Reading(sensorId, timestamp, storageClass, temperature);
            return true;
        }
    }
}
