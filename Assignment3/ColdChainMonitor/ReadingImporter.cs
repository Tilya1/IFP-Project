using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;

namespace ColdChainMonitor
{
    public static class ReadingImporter
    {
        // The caller owns the stream. We read from the current Position and leave it open.
        public static ImportResult ReadReadings(Stream input)
        {
            List<Reading> readings = new List<Reading>();
            List<ImportError> errors = new List<ImportError>();
            Dictionary<string, StorageClass> firstClass =
                new Dictionary<string, StorageClass>(StringComparer.OrdinalIgnoreCase);

            using (StreamReader reader = new StreamReader(input, Encoding.UTF8, false, 1024, leaveOpen: true))
            {
                int lineNumber = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    Reading reading;
                    string error;
                    if (!ReadingParser.TryParseReading(line, out reading, out error))
                    {
                        errors.Add(new ImportError(lineNumber, line, error));
                        continue;
                    }

                    StorageClass knownClass;
                    if (firstClass.TryGetValue(reading.SensorId, out knownClass))
                    {
                        if (knownClass != reading.StorageClass)
                        {
                            errors.Add(new ImportError(lineNumber, line, "StorageClass changed for this sensor."));
                            continue;
                        }
                    }
                    else
                    {
                        firstClass[reading.SensorId] = reading.StorageClass;
                    }

                    readings.Add(reading);
                }
            }

            // Everything is already read into lists, nothing lazy is returned
            return new ImportResult(readings.ToImmutableList(), errors.ToImmutableList());
        }
    }
}
