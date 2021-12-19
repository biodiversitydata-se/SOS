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

        /// <summary>
        /// Remove control characters and other non-printable characters.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="newLineTabReplacement"></param>
        /// <param name="otherCharReplacement"></param>
        /// <returns></returns>
        public static string RemoveIllegalCharacters(string str, string newLineTabReplacement = " ", string otherCharReplacement="")
        {
            //return str == null ? "" : RxIllegalCharacters.Replace(str, otherCharReplacement); // Fastest, but doesn't handle new line and tab correctly.            
            return str == null ? "" : RxIllegalCharacters.Replace(str, match => // Slower, but handles new line and tab correctly.
            {
                if (RxNewLineTab.IsMatch(match.Value))
                    return newLineTabReplacement;
                else
                    return otherCharReplacement;
            }).Trim();
        }

        private static readonly Regex RxNewLineTab = new Regex(@"\r\n?|\n|\t", RegexOptions.Compiled);
        private static readonly Regex RxIllegalCharacters = new Regex(@"\p{C}+", RegexOptions.Compiled); // Match all control characters and other non-printable characters
        //private static readonly Regex RxIllegalCharacters = new Regex(@"\p{Cc}+", RegexOptions.Compiled); // Match all basic control characters (65 chars)
    }
}