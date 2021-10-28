using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
        ///     Truncates a string to a specific length.
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

        public static bool ContainsAny(this string strValue, StringComparison stringComparison, params string[] list)
        {
            return list.Any(s => s.Equals(strValue, stringComparison));
        }

        public static bool ContainsAny(this string strValue, params string[] list)
        {
            return list.Any(s => s.Equals(strValue, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     Parse a Double value.
        /// </summary>
        /// <param name='value'>Double value to convert to a string.</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>The Double value.</returns>
        public static double? ParseDouble(this string value, bool trim = true)
        {
            return TryParseDouble(value, out var result, trim) ? (double?) result : null;
        }

        /// <summary>
        ///     Validates a Double value.
        /// </summary>
        /// <param name='value'>String Double value to validate</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the value is a Double</returns>
        public static bool IsDouble(this string value, bool trim = true)
        {
            return TryParseDouble(value, out var _, trim);
        }

        /// <summary>
        ///     Try parse a Double value.
        /// </summary>
        /// <param name='value'>Double string value to parse</param>
        /// <param name="result">The parsed value.</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the value is a Double</returns>
        public static bool TryParseDouble(this string value, out double result, bool trim = true)
        {
            if (value == null)
            {
                result = double.NaN;
                return false;
            }

            const NumberStyles styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent |
                                        NumberStyles.AllowLeadingSign;
            if (trim)
            {
                return double.TryParse(value.Trim().Replace(",", "."), styles,
                    CultureInfo.CreateSpecificCulture("en-GB"), out result);
            }

            return double.TryParse(value.Replace(",", "."), styles, CultureInfo.CreateSpecificCulture("en-GB"),
                out result);
        }

        /// <summary>
        ///     Parse a Boolean value.
        /// </summary>
        /// <param name='value'>String Boolean value to parse.</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>The Boolean value.</returns>
        public static bool? ParseBoolean(this string value, bool trim = true)
        {
            return TryParseBoolean(value, out var result, trim) ? (bool?) result : null;
        }

        /// <summary>
        ///     Validates a Boolean value.
        /// </summary>
        /// <param name='value'>Boolean string value to validate</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the value is a Boolean</returns>
        public static bool IsBoolean(this string value, bool trim = true)
        {
            return TryParseBoolean(value, out var _, trim);
        }

        /// <summary>
        ///     Parse a Boolean value.
        /// </summary>
        /// <param name='value'>String Boolean value to parse.</param>
        /// <param name="result">The parsed value.</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the Boolean value could be parsed.</returns>
        public static bool TryParseBoolean(this string value, out bool result, bool trim = true)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            if (trim)
            {
                return bool.TryParse(value.Trim(), out result);
            }

            return bool.TryParse(value, out result);
        }

        /// <summary>
        ///     Parse a DateTime value.
        /// </summary>
        /// <param name='value'>String DateTime value to parse.</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>The DateTime value.</returns>
        public static DateTime? ParseDateTime(this string value, bool trim = true)
        {
            return TryParseDateTime(value, out var result, trim) ? (DateTime?) result : null;
        }

        /// <summary>
        ///     Validates a DateTime value.
        /// </summary>
        /// <param name='value'>DateTime string value to validate</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the value is a DateTime</returns>
        public static bool IsDateTime(this string value, bool trim = true)
        {
            return TryParseDateTime(value, out var _, trim);
        }

        /// <summary>
        ///     Parse a DateTime value.
        /// </summary>
        /// <param name='value'>String DateTime value to parse.</param>
        /// <param name="result">The parsed value.</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the DateTime could be parsed.</returns>
        public static bool TryParseDateTime(this string value, out DateTime result, bool trim = true)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            if (trim)
            {
                return DateTime.TryParse(value.Trim(), out result);
            }

            return DateTime.TryParse(value, out result);
        }

        /// <summary>
        /// Parses the value as Double and then converts it to Int32.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="trim"></param>
        /// <returns></returns>
        public static int? ParseDoubleConvertToInt(this string value, bool trim = true)
        {
            if (TryParseDouble(value, out double result, trim))
            {
                return Convert.ToInt32(result);
            }

            return null;
        }

        /// <summary>
        ///     Parse a Int32 value.
        /// </summary>
        /// <param name='value'>String Int32 value to parse.</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>The Int32 value.</returns>
        public static int? ParseInt(this string value, bool trim = true)
        {
            return TryParseInt(value, out var result, trim) ? (int?) result : null;
        }

        /// <summary>
        ///     Validates a Int32 value.
        /// </summary>
        /// <param name='value'>Int32 string value to validate</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the value is a Int32</returns>
        public static bool IsInt(this string value, bool trim = true)
        {
            return TryParseInt(value, out var _, trim);
        }

        /// <summary>
        ///     Parse a Int32 value.
        /// </summary>
        /// <param name='value'>String Int32 value to parse.</param>
        /// <param name="result">The parsed value.</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the Int32 could be parsed.</returns>
        public static bool TryParseInt(this string value, out int result, bool trim = true)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            if (trim)
            {
                return int.TryParse(value.Trim(), out result);
            }

            return int.TryParse(value, out result);
        }

        /// <summary>
        ///     Parse a Int64 value.
        /// </summary>
        /// <param name='value'>String Int64 value to parse.</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>The Int64 value.</returns>
        public static long? ParseLong(this string value, bool trim = true)
        {
            return TryParseLong(value, out var result, trim) ? (long?) result : null;
        }

        /// <summary>
        ///     Validates a Int64 value.
        /// </summary>
        /// <param name='value'>Int64 string value to validate</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the value is a Int64</returns>
        public static bool IsLong(this string value, bool trim = true)
        {
            return TryParseLong(value, out var _, trim);
        }

        /// <summary>
        ///     Parse a Int64 value.
        /// </summary>
        /// <param name='value'>String Int64 value to parse.</param>
        /// <param name="result">The parsed value.</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the Int64 could be parsed.</returns>
        public static bool TryParseLong(this string value, out long result, bool trim = true)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            if (trim)
            {
                return long.TryParse(value.Trim(), out result);
            }

            return long.TryParse(value, out result);
        }

        /// <summary>
        ///     Returns a boolean indicating whether the string has a value, returns false if it's null or empty
        /// </summary>
        /// <param name="value">The string to check</param>
        /// <returns>A boolean indicating whether the string has a value</returns>
        public static bool HasValue(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }


        /// <summary>
        /// Make sure first char is lower case
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : char.ToLower(value[0]) + value.Substring(1);
        }

        /// <summary>
        /// Returns the input string with the first character converted to uppercase
        /// </summary>
        public static string ToUpperFirst(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            Span<char> a = stackalloc char[s.Length];
            s.AsSpan(1).CopyTo(a.Slice(1));
            a[0] = char.ToUpper(s[0]);
            return new string(a);
        }

        /// <summary>
        /// Returns the input string with the first character converted to lowercase
        /// </summary>
        public static string ToLowerFirst(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            Span<char> a = stackalloc char[s.Length];
            s.AsSpan(1).CopyTo(a.Slice(1));
            a[0] = char.ToLower(s[0]);
            return new string(a);
        }

        /// <summary>
        /// Compute unique hash of string
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToHash(this string source)
        {
            var sha1 = SHA1.Create();
            var buf = Encoding.UTF8.GetBytes(source);
            var hash = sha1.ComputeHash(buf, 0, buf.Length);
            return BitConverter.ToString(hash).Replace("-", "");
        }

        /// <summary>
        /// Remove white spaces.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string RemoveWhiteSpace(this string self)
        {
            return new string(self.Where(c => !Char.IsWhiteSpace(c)).ToArray());
        }
    }
}