using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SOS.Lib.Helpers
{
    public static class DwcFormattingHelper
    {
        /// <summary>
        /// Create a date interval formatting according to the ISO 8601 standard.
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

            if (!date2.HasValue)
            {
                return date1.Value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture);
            }

            return string.Format(
                "{0}/{1}",
                date1.Value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture),
                date2.Value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Create a time interval formatting according to the ISO 8601 standard.
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

            if (!date2.HasValue)
            {
                return date1.Value.ToUniversalTime().ToString("HH:mm:ssK", CultureInfo.InvariantCulture);
            }

            return string.Format(
                "{0}/{1}",
                date1.Value.ToUniversalTime().ToString("HH:mm:ssK", CultureInfo.InvariantCulture),
                date2.Value.ToUniversalTime().ToString("HH:mm:ssK", CultureInfo.InvariantCulture));
        }
    }
}
