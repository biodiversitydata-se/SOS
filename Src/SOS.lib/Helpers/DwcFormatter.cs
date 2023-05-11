using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SOS.Lib.Helpers
{
    public static class DwcFormatter
    {
        /// <summary>
        ///     Create a date formatted according to the ISO 8601 standard.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>A date that is formatted according to the ISO 8601 standard.</returns>
        public static string CreateDateString(DateTime? date)
        {
            return date?.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Create a time formatted according to the ISO 8601 standard.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>A time that is formatted according to the ISO 8601 standard.</returns>
        public static string CreateTimeString(DateTime? date)
        {
            return date?.ToUniversalTime().ToString("HH:mm:ssK", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Create a date interval formatting according to the ISO 8601 standard.
        /// </summary>
        /// <param name="date1">From date.</param>
        /// <param name="date2">To date.</param>
        /// <returns>A date interval that is formatted according to the ISO 8601 standard.</returns>
        public static string CreateDateIntervalString(DateTime? date1, DateTime? date2)
        {
            if (!date1.HasValue)
            {
                return null;
            }

            string format = "yyyy-MM-dd";
            if (!date2.HasValue || date1 == date2)
            {
                if (date1.Value.TimeOfDay.TotalSeconds > 0)
                {
                    format = "yyyy-MM-dd'T'HH:mm:ssK";
                }
                return date1.Value.ToString(format, CultureInfo.InvariantCulture);
            }

            if (date1.Value.TimeOfDay.TotalSeconds > 0 || date2.Value.TimeOfDay.TotalSeconds > 0)
            {
                format = "yyyy-MM-dd'T'HH:mm:ssK";
            }

            return string.Format(
                "{0}/{1}",
                date1.Value.ToString(format, CultureInfo.InvariantCulture),
                date2.Value.ToString(format, CultureInfo.InvariantCulture));
        }

        /// <summary>
        ///     Create a time interval formatting according to the ISO 8601 standard.
        /// </summary>
        /// <param name="date1">From date.</param>
        /// <param name="date2">To date.</param>
        /// <returns>A time interval that is formatted according to the ISO 8601 standard.</returns>
        public static string CreateTimeIntervalString(DateTime? date1, DateTime? date2)
        {
            if (!date1.HasValue)
            {
                return null;
            }

            if (!date2.HasValue || date1 == date2)
            {
                if (Math.Abs(date1.Value.TimeOfDay.TotalSeconds) < 0.001) return null;

                return date1.Value.ToString("HH:mm:ssK", CultureInfo.InvariantCulture);
            }

            if (Math.Abs(date1.Value.TimeOfDay.TotalSeconds) < 0.001 || Math.Abs(date2.Value.TimeOfDay.TotalSeconds) < 0.001) return null;

            return string.Format(
                "{0}/{1}",
                date1.Value.ToString("HH:mm:ssK", CultureInfo.InvariantCulture),
                date2.Value.ToString("HH:mm:ssK", CultureInfo.InvariantCulture));
        }        
    }
}