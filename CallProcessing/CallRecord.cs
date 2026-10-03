using System;

namespace CallProcessing
{
    public readonly record struct CallRecord
    {
        public string RecordId { get; }
        public string DestinationCountry { get; }
        public double DurationMinutes { get; }
        public bool IsRoaming { get; }

        public CallRecord(string recordId, string destinationCountry, double durationMinutes, bool isRoaming)
        {
            if (string.IsNullOrWhiteSpace(recordId))
            {
                throw new ArgumentException("RecordId cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(destinationCountry))
            {
                throw new ArgumentException("DestinationCountry cannot be empty.");
            }
            if (double.IsNaN(durationMinutes) || double.IsInfinity(durationMinutes))
            {
                throw new ArgumentException("Duration must be a finite number.");
            }
            if (durationMinutes < 0 || durationMinutes > 10000)
            {
                throw new ArgumentException("Duration must be from 0 to 10000 minutes.");
            }

            RecordId = recordId;
            DestinationCountry = destinationCountry;
            DurationMinutes = durationMinutes;
            IsRoaming = isRoaming;
        }
    }
}
