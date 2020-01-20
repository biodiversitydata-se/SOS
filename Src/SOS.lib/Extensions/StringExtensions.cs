using System;
using System.Text.RegularExpressions;

namespace SOS.Lib.Extensions
{
    public static class StringExtensions
    {
        public static string UntilNonAlfanumeric(this string value)
        {
            var regex = new Regex(@"\w+");
            return regex.Match(value).Value;
        }

        public static string FromNonAlfanumeric(this string value)
        {
            var regex = new Regex(@"\w+$");
            return regex.Match(value).Value;
        }

        /// <summary>
        /// Truncates a string to a specific length.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <param name="maxLength">The max length.</param>
        /// <returns>A truncated string.</returns>
        public static string WithMaxLength(this string value, int maxLength)
        {
            if (value == null)
            {
                return null;
            }

            if (maxLength < 0)
            {
                return "";
            }

            return value.Substring(0, Math.Min(value.Length, maxLength));
        }
    }
}
