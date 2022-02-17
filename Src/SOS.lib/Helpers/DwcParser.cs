using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using SOS.Lib.Extensions;

namespace SOS.Lib.Helpers
{
    /// <summary>
    ///     DarwinCore parser.
    /// </summary>
    public static class DwcParser
    {
        private static TimeZoneInfo swedenTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

        /// <summary>
        ///     Default date formats used by ParseDate().
        ///     At least Year, Month and Day must be specified.
        /// </summary>
        private static IEnumerable<string> DatePatterns
        {
            get
            {
                return new[]
                {
                    "yyyy-MM-dd'T'HH:mm:ssK",
                    "yyyy-MM-dd'T'HH:mm:ss.fK",
                    "yyyy-MM-dd'T'HH:mm:ss.ffK",
                    "yyyy-MM-dd'T'HH:mm:ss.fffK",
                    "yyyy-MM-dd'T'HH:mm:ss.ffffK",
                    "yyyy-MM-dd'T'HH:mm:ss.fffffK",
                    "yyyy-MM-dd'T'HH:mm:ss.ffffffK",
                    "yyyy-MM-dd'T'HH:mm:ss.fffffffK",
                    "yyyy-MM-dd'T'HH:mmK",
                    "yyyy-MM-dd'T'HHK",
                    "yyyy-MM-dd",
                    "yyyy-MM-d",
                    "yyyy-M-dd",
                    "yyyy-M-d",                    
                    "yyyy-M",
                    "yyyy-0MM-dd", // special case for the TUVA dataset that contains multiple observations in this format.
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
                    "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'",
                    "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'",
                    "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
                    "yyyy'-'MM'-'dd HH':'mm':'ss'Z'",
                    "dddd, dd MMMM yyyy HH:mm:ss",
                    "yyyy MMMM"
                };
            }
        }

        private static IEnumerable<string> TimePatterns
        {
            get
            {
                return new[]
                {
                    "HH:mm:ssK",
                    "HH:mm:ss.fK",
                    "HH:mm:ss.ffK",
                    "HH:mm:ss.fffK",
                    "HH:mm:ss.ffffK",
                    "HH:mm:ss.fffffK",
                    "HH:mm:ss.ffffffK",
                    "HH:mm:ss.fffffffK",
                    "HH:mmK",
                    "HHK",
                    "HH:mm",
                    "hh:mm tt",
                    "H:mm",
                    "h:mm tt",
                    "HH:mm:ss",
                    "HH:mm:ss.f",
                    "HH:mm:ss.fffffff",
                    "HH':'mm':'ss 'GMT'",
                    "HH':'mm':'ss 'GMT'",
                    "HH':'mm':'ss",
                    "HH':'mm':'ss'Z'",
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
        /// <param name="eventTime"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static bool TryParseEventDate(
            string eventDate,
            string year,
            string month,
            string day,
            string eventTime,
            out DateTime? startDate,
            out DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(eventTime))
            {
                return TryParseEventDate(eventDate, year, month, day, out startDate, out endDate);
            }

            if (!TryParseEventDate(eventDate, year, month, day, out startDate, out endDate))
            {
                return false;
            }

            var timeParts = eventTime.Split("/");
            if (timeParts.Length == 2) // Interval
            {
                startDate = ParseTime(timeParts[0], startDate.Value);
                endDate = ParseTime(timeParts[1], endDate ?? endDate.Value);
                return true;
            }

            startDate = ParseTime(eventTime, startDate.Value);
            endDate = ParseTime(eventTime, endDate.Value);
            return true;
        }


        public static bool TryParseEventDate(
            string eventDate,
            string year,
            string month,
            string day,
            string eventTime,
            out DateTime? startDate,
            out DateTime? endDate,
            out TimeSpan? startTime,
            out TimeSpan? endTime)
        {
            if (string.IsNullOrWhiteSpace(eventTime))
            {
                if (TryParseEventDate(eventDate, year, month, day, out startDate, out endDate))
                {
                    startTime = startDate != null && startDate.Value.TimeOfDay.Ticks > 0 ? startDate.Value.TimeOfDay : null;
                    endTime = endDate != null && endDate.Value.TimeOfDay.Ticks > 0 ? endDate.Value.TimeOfDay : null;
                    return true;
                }

                startTime = endTime = null;
                return false;
            }

            if (!TryParseEventDate(eventDate, year, month, day, out startDate, out endDate))
            {
                startTime = endTime = null;
                return false;
            }

            if (startDate == null || endDate == null)
            {
                startTime = endTime = null;
                return false;
            }

            var timeParts = eventTime.Split("/");
            if (timeParts.Length == 2) // Interval
            {
                startDate = ParseTime(timeParts[0], startDate.Value);
                endDate = ParseTime(timeParts[1], endDate ?? endDate.Value);
                if (startDate == null || endDate == null)
                {
                    startTime = endTime = null;
                    return false;
                }

                startTime = startDate.Value.TimeOfDay;
                endTime = endDate.Value.TimeOfDay;
                return true;
            }

            startDate = ParseTime(eventTime, startDate.Value);
            endDate = ParseTime(eventTime, endDate.Value);
            if (startDate == null || endDate == null)
            {
                startTime = endTime = null;
                return false;
            }

            startTime = startDate.Value.TimeOfDay;
            endTime = endDate.Value.TimeOfDay;
            return true;
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

        private static readonly Regex RxTimeZoneWithOnlyTwoDigits = new Regex(@"[+-]\d{2}$", RegexOptions.Compiled);

        /// <summary>
        ///     Parse a date string to a DateTime?
        /// </summary>
        /// <param name="timeStr">The time string.</param>
        /// <param name="dateTime">The date</param>
        /// <param name="dateFormats">The date formats.</param>
        /// <returns></returns>
        public static DateTime? ParseTime(string timeStr, DateTime dateTime, params string[] dateFormats)
        {
            if (string.IsNullOrWhiteSpace(timeStr)) return dateTime;
            if (RxTimeZoneWithOnlyTwoDigits.IsMatch(timeStr))
            {
                timeStr += "00";
            }
            string strDate = $"{dateTime:yyyy-MM-dd}T{timeStr}";
            var date = ParseDate(strDate);
            if (date == null) return dateTime;
            return date;
        }

        public static bool TryParseTime(string timeStr, DateTime dateTime, out DateTime? date)
        {
            if (string.IsNullOrWhiteSpace(timeStr))
            {
                date = null;
                return false;
            }

            if (RxTimeZoneWithOnlyTwoDigits.IsMatch(timeStr))
            {
                timeStr += "00";
            }
            string strDate = $"{dateTime:yyyy-MM-dd}T{timeStr}";
            date = ParseDate(strDate);
            if (date == null)
            {
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