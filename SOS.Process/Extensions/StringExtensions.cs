using System;

namespace SOS.Process.Extensions
{
    public static class StringExtensions
    {
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
