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

            if (!date2.HasValue)
            {
                return date1.Value.ToUniversalTime().ToString("HH:mm:ssK", CultureInfo.InvariantCulture);
            }

            return string.Format(
                "{0}/{1}",
                date1.Value.ToUniversalTime().ToString("HH:mm:ssK", CultureInfo.InvariantCulture),
                date2.Value.ToUniversalTime().ToString("HH:mm:ssK", CultureInfo.InvariantCulture));
        }

        private static readonly Regex RxNewLineTab = new Regex(@"\r\n?|\n|\t", RegexOptions.Compiled);

        /// <summary>
        /// Replace new line and tabs with the specified string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string RemoveNewLineTabs(string str, string replacement = " ")
        {
            return str == null ? "" : RxNewLineTab.Replace(str, replacement);
        }
    }
}