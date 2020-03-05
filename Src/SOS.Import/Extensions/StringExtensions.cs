using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SOS.Lib.Constants;

namespace SOS.Import.Extensions
{
    public static class StringExtensions
    {
        private static readonly CultureInfo GbCultureInfo = CultureInfo.CreateSpecificCulture(Cultures.en_GB);

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

        /// <summary>
        /// Converts first char to lower case.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToLowerFirstChar(this string input)
        {
            string newString = input;
            if (!String.IsNullOrEmpty(newString) && Char.IsUpper(newString[0]))
                newString = Char.ToLower(newString[0]) + newString.Substring(1);
            return newString;
        }

        /// <summary>
        /// Validates a Boolean value that has been
        /// received over the internet.
        /// </summary>
        /// <param name='value'>Boolean string value to validate</param>
        /// <returns>True if the value is a Boolean</returns>
        public static bool WebIsBoolean(this string value)
        {
            Boolean dummyVal;
            return Boolean.TryParse(value, out dummyVal);
        }

        /// <summary>
        /// Parse a Boolean value that has been
        /// received over the internet.
        /// </summary>
        /// <param name='value'>Boolean value to convert to a string.</param>
        /// <returns>The Boolean value as a string.</returns>
        public static Boolean WebParseBoolean(this String value)
        {
            return Boolean.Parse(value);
        }

        /// <summary>
        /// Validates a DateTime value that has been
        /// received over the internet.
        /// </summary>
        /// <param name='value'>DateTime string value to validate</param>
        /// <returns>True if the value is a DateTime</returns>
        public static bool WebIsDateTime(this string value)
        {
            DateTime dummyVal;
            return DateTime.TryParse(value, out dummyVal);
        }

        /// <summary>
        /// Parse a DateTime value that has been
        /// received over the internet.
        /// </summary>
        /// <param name='value'>DateTime value to convert to a string.</param>
        /// <returns>The DateTime value as a string.</returns>
        public static DateTime WebParseDateTime(this String value)
        {
            return DateTime.Parse(value);
        }

        /// <summary>
        /// Validates a Double value that has been
        /// received over the internet.
        /// </summary>
        /// <param name='value'>Double string value to validate</param>
        /// <returns>True if the value is a Double</returns>
        public static bool WebIsDouble(this string value)
        {
            const NumberStyles styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign;
            Double dummyVal;
            return Double.TryParse(value.Replace(",", "."), styles, GbCultureInfo, out dummyVal);
        }

        /// <summary>
        /// Parse a Double value that has been
        /// received over the internet.
        /// </summary>
        /// <param name='value'>Double value to convert to a string.</param>
        /// <returns>The Double value as a string.</returns>
        public static Double WebParseDouble(this String value)
        {
            NumberStyles styles;

            styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign;
            return Double.Parse(value.Replace(",", "."), styles, GbCultureInfo);
        }

        /// <summary>
        /// Validate a Int32 value that has been
        /// received over the internet.
        /// </summary>
        /// <param name='value'>Int32 string value to validate</param>
        /// <returns>True if the value is an Int32</returns>
        public static bool WebIsInt32(this String value)
        {
            Int32 dummyVal;
            return Int32.TryParse(value, out dummyVal);
        }

        /// <summary>
        /// Parse a Int32 value that has been
        /// received over the internet.
        /// </summary>
        /// <param name='value'>Int32 value to convert to a string.</param>
        /// <returns>The Int32 value as a string.</returns>
        public static Int32 WebParseInt32(this String value)
        {
            return Int32.Parse(value);
        }

        /// <summary>
        /// Validate a Int64 value that has been
        /// received over the internet.
        /// </summary>
        /// <param name='value'>Int64 string value to validate</param>
        /// <returns>True if the values is an Int64</returns>
        public static bool WebIsInt64(this String value)
        {
            Int64 dummyVal;
            return Int64.TryParse(value, out dummyVal);
        }

        /// <summary>
        /// Parse a Int64 value that has been
        /// received over the internet.
        /// </summary>
        /// <param name='value'>Int64 value to convert to a string.</param>
        /// <returns>The Int64 value as a string.</returns>
        public static Int64 WebParseInt64(this String value)
        {
            return Int64.Parse(value);
        }
    }
}
