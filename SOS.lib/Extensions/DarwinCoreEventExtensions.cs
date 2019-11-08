using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using SOS.Lib.Models.DarwinCore;

namespace SOS.Lib.Extensions
{
    public static class DarwinCoreEventExtensions
    {
        public static void PopulateDateFields(
            this DarwinCoreEvent darwinCoreEvent, 
            DateTime observationDateStart,
            DateTime? observationDateEnd)
        {
            darwinCoreEvent.Day = observationDateEnd?.Day ?? observationDateStart.Day;
            darwinCoreEvent.EndDayOfYear = observationDateEnd?.DayOfYear ?? observationDateStart.DayOfYear;
            darwinCoreEvent.EventDate = CreateDateIntervalString(observationDateStart, observationDateEnd);
            darwinCoreEvent.EventTime = CreateTimeIntervalString(observationDateStart, observationDateEnd);
            darwinCoreEvent.Month = observationDateEnd?.Month ?? observationDateStart.Month;
            darwinCoreEvent.StartDayOfYear = observationDateStart.DayOfYear;
            darwinCoreEvent.Year = observationDateEnd?.Year ?? observationDateStart.Year;
        }

        private static string CreateDateIntervalString(DateTime date1, DateTime? date2)
        {
            if (!date2.HasValue)
            {
                return date1.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture);
            }

            return string.Format(
                "{0}/{1}",
                date1.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture),
                date2.Value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture));
        }

        private static string CreateTimeIntervalString(DateTime date1, DateTime? date2)
        {
            if (!date2.HasValue)
            {
                return date1.ToUniversalTime().ToString("HH:mm:ssK", CultureInfo.InvariantCulture);
            }

            return string.Format(
                "{0}/{1}",
                date1.ToUniversalTime().ToString("HH:mm:ssK", CultureInfo.InvariantCulture),
                date2.Value.ToUniversalTime().ToString("HH:mm:ssK", CultureInfo.InvariantCulture));
        }
    }
}
