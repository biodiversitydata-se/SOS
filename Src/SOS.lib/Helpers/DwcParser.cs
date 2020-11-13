using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SOS.Lib.Extensions;

namespace SOS.Lib.Helpers
{
    /// <summary>
    ///     DarwinCore parser.
    /// </summary>
    public static class DwcParser
    {
        /// <summary>
        ///     Default date formats used by ParseDate().
        ///     At least Year, Month & Day must be specified.
        /// </summary>
        private static IEnumerable<string> DatePatterns
        {
            get
            {
                return new[]
                {
                    "yyyy-MM-dd'T'HH:mm:ssK",
                    "yyyy-MM-dd'T'HH:mmK",
                    "yyyy-MM-dd'T'HHK",
                    "yyyy-MM-dd",
                    "dddd, dd MMMM yyyy",
                    "dddd, dd MMMM yyyy HH:mm",
                    "dddd, dd MMMM yyyy hh:mm tt",
                    "dddd, dd MMMM yyyy H:mm",
                    "dddd, dd MMMM yyyy h:mm tt",
                    "dddd, dd MMMM yyyy HH:mm:ss",
                    "yyyy-MM-dd HH:mm",
                    "yyyy-MM-dd hh:mm tt",
                    "yyyy-MM-dd H:mm",
                    "yyyy-MM-dd h:mm tt",
                    "yyyy-MM-dd HH:mm:ss",
                    "yyyy-MM-dd HH:mm:ss.f",
                    "yyyy-MM-dd HH:mm:ss.fffffff",
                    "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK",
                    "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK",
                    "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'",
                    "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'",
                    "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
                    "yyyy'-'MM'-'dd HH':'mm':'ss'Z'",
                    "dddd, dd MMMM yyyy HH:mm:ss",
                    "yyyy MMMM"
                };
            }
        }

        /// <summary>
        ///     Try parse event date.
        /// </summary>
        /// <param name="eventDate"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static bool TryParseEventDate(
            string eventDate,
            string year,
            string month,
            string day,
            out DateTime? startDate,
            out DateTime? endDate)
        {
            if (string.IsNullOrEmpty(eventDate))
            {
                return TryParseEventDate(year, month, day, out startDate, out endDate);
            }

            var dateParts = eventDate.Split("/");
            if (dateParts.Length == 2) // Interval
            {
                return TryParseIntervalEventDate(dateParts[0], dateParts[1], out startDate, out endDate);
            }

            // Try parse single date.
            startDate = ParseDate(eventDate, "yyyy");
            if (startDate.HasValue) // start date is year
            {
                endDate = new DateTime(startDate.Value.Year, 12, 31);
                return true;
            }

            startDate = ParseDate(eventDate, "yyyy-MM");
            if (startDate.HasValue) // start date is year and month
            {
                endDate = new DateTime(startDate.Value.Year, startDate.Value.Month,
                    DateTime.DaysInMonth(startDate.Value.Year, startDate.Value.Month));
                return true;
            }

            startDate = ParseDate(eventDate);
            if (startDate.HasValue)
            {
                endDate = startDate;
                return true;
            }

            return TryParseEventDate(year, month, day, out startDate, out endDate);
        }

        /// <summary>
        ///     Try parse event date from year, month and day.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static bool TryParseEventDate(
            string year,
            string month,
            string day,
            out DateTime? startDate,
            out DateTime? endDate)
        {
            try
            {
                var parsedYear = year.ParseInt();
                var parsedMonth = month.ParseInt();
                var parsedDay = day.ParseInt();
                if (parsedYear.HasValue && parsedMonth.HasValue && parsedDay.HasValue)
                {
                    startDate = new DateTime(parsedYear.Value, parsedMonth.Value, parsedDay.Value);
                    endDate = new DateTime(parsedYear.Value, parsedMonth.Value, parsedDay.Value);
                    return true;
                }

                if (parsedYear.HasValue && parsedMonth.HasValue)
                {
                    startDate = new DateTime(parsedYear.Value, parsedMonth.Value, 1);
                    endDate = new DateTime(parsedYear.Value, parsedMonth.Value,
                        DateTime.DaysInMonth(parsedYear.Value, parsedMonth.Value));
                    return true;
                }

                if (parsedYear.HasValue)
                {
                    startDate = new DateTime(parsedYear.Value, 1, 1);
                    endDate = new DateTime(parsedYear.Value, 12, 31);
                    return true;
                }

                startDate = null;
                endDate = null;
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                startDate = null;
                endDate = null;
                return false;
            }
        }

        /// <summary>
        ///     Try parse an interval date.
        /// </summary>
        /// <param name="strStartDate"></param>
        /// <param name="strEndDate"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static bool TryParseIntervalEventDate(
            string strStartDate,
            string strEndDate,
            out DateTime? startDate,
            out DateTime? endDate)
        {
            startDate = ParseDate(strStartDate, "yyyy");
            if (startDate.HasValue) // start date is year
            {
                endDate = ParseDate(strEndDate, "yyyy");
                if (endDate.HasValue) // end date is year
                {
                    endDate = new DateTime(endDate.Value.Year, 12, 31);
                    return true;
                }

                endDate = ParseDate(strEndDate);
                if (!endDate.HasValue)
                {
                    startDate = null;
                    return false;
                }

                return true;
            }

            startDate = ParseDate(strStartDate);
            if (!startDate.HasValue)
            {
                endDate = null;
                return false;
            }

            endDate = ParseDate(strEndDate, "dd");
            if (endDate.HasValue) // end date is specified as a day
            {
                try
                {
                    endDate = new DateTime(startDate.Value.Year, startDate.Value.Month, endDate.Value.Day);
                    return true;
                }
                catch (ArgumentOutOfRangeException) // handle invalid day value
                {
                    startDate = null;
                    endDate = null;
                    return false;
                }
            }

            endDate = ParseDate(strEndDate, null);
            if (!endDate.HasValue)
            {
                startDate = null;
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Parse a date string to a DateTime?
        /// </summary>
        /// <param name="dateTimeStr">The date string.</param>
        /// <param name="dateFormats">The date formats.</param>
        /// <returns></returns>
        public static DateTime? ParseDate(string dateTimeStr, params string[] dateFormats)
        {
            const DateTimeStyles style = DateTimeStyles.AllowWhiteSpaces;
            if (dateFormats == null || dateFormats.Length == 0)
            {
                dateFormats = DatePatterns.ToArray();
            }

            var result = DateTime.TryParseExact(
                dateTimeStr,
                dateFormats,
                CultureInfo.InvariantCulture,
                style,
                out var dt)
                ? dt
                : null as DateTime?;
            return result;
        }
    }
}