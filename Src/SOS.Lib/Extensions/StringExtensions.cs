using Standart.Hash.xxHash;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SOS.Lib.Extensions;

public static class StringExtensions
{
    private static readonly Regex RxFromNonAlfanumeric = new Regex(@"\w+$", RegexOptions.Compiled);
    private static readonly Regex RxUntilNonAlfanumeric = new Regex(@"\w+", RegexOptions.Compiled);
    private static readonly Regex RxNewLineTab = new Regex(@"\r\n?|\n|\t", RegexOptions.Compiled);
    private static readonly Regex RxIllegalCharacters = new Regex(@"\p{C}+", RegexOptions.Compiled); // Match all control characters and other non-printable characters
    private static readonly Regex RxEnlosingQuotes = new Regex(@"^\"".*\""$", RegexOptions.Compiled);

    extension(string value)
    {
        private string CleanEnclosingQuotes()
        {
            if ((value?.Length ?? 0) < 2)
            {
                return value;
            }

            if (RxEnlosingQuotes.IsMatch(value))
            {
                value = value.Substring(1, value.Length - 2);
            }

            return value;
        }

        /// <summary>
        /// Remove unprintable characters 
        /// </summary>
        /// <returns></returns>
        public string Clean()
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return RxIllegalCharacters.Replace(value, match => // Slower, but handles new line and tab correctly.
            {
                if (RxNewLineTab.IsMatch(match.Value))
                    return " ";
                else
                    return "";
            }).CleanEnclosingQuotes().Trim();
        }

        public string CleanNewLineTab()
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return RxNewLineTab.Replace(value, match => // Slower, but handles new line and tab correctly.
            {
                return "";
            }).CleanEnclosingQuotes().RemoveWhiteSpace().Trim();
        }

        /// <summary>
        /// Checks if the string contains unprintable characters.
        /// </summary>
        /// <returns></returns>
        public bool ContainsIllegalCharacters()
        {
            if (string.IsNullOrEmpty(value)) return false;

            return RxIllegalCharacters.IsMatch(value);
        }

        public string FromNonAlfanumeric()
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return RxFromNonAlfanumeric.Match(value).Value;
        }

        public string UntilNonAlfanumeric()
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return RxUntilNonAlfanumeric.Match(value).Value;
        }

        /// <summary>
        ///     Truncates a string to a specific length.
        /// </summary>
        /// <param name="maxLength">The max length.</param>
        /// <returns>A truncated string.</returns>
        public string WithMaxLength(int maxLength)
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
        ///     Parse a Double value.
        /// </summary>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>The Double value.</returns>
        public double? ParseDouble(bool trim = true)
        {
            return TryParseDouble(value, out var result, trim) ? (double?)result : null;
        }

        /// <summary>
        ///     Validates a Double value.
        /// </summary>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the value is a Double</returns>
        public bool IsDouble(bool trim = true)
        {
            return TryParseDouble(value, out var _, trim);
        }

        /// <summary>
        ///     Try parse a Double value.
        /// </summary>
        /// <param name="result">The parsed value.</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the value is a Double</returns>
        public bool TryParseDouble(out double result, bool trim = true)
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
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>The Boolean value.</returns>
        public bool? ParseBoolean(bool trim = true)
        {
            return TryParseBoolean(value, out var result, trim) ? (bool?)result : null;
        }

        /// <summary>
        ///     Validates a Boolean value.
        /// </summary>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the value is a Boolean</returns>
        public bool IsBoolean(bool trim = true)
        {
            return TryParseBoolean(value, out var _, trim);
        }

        /// <summary>
        ///     Parse a Boolean value.
        /// </summary>
        /// <param name="result">The parsed value.</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the Boolean value could be parsed.</returns>
        public bool TryParseBoolean(out bool result, bool trim = true)
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
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>The DateTime value.</returns>
        public DateTime? ParseDateTime(bool trim = true)
        {
            return TryParseDateTime(value, out var result, trim) ? (DateTime?)result : null;
        }

        /// <summary>
        ///     Validates a DateTime value.
        /// </summary>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the value is a DateTime</returns>
        public bool IsDateTime(bool trim = true)
        {
            return TryParseDateTime(value, out var _, trim);
        }

        /// <summary>
        ///     Parse a DateTime value.
        /// </summary>
        /// <param name="result">The parsed value.</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the DateTime could be parsed.</returns>
        public bool TryParseDateTime(out DateTime result, bool trim = true)
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
        /// <param name="trim"></param>
        /// <returns></returns>
        public int? ParseDoubleConvertToInt(bool trim = true)
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
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>The Int32 value.</returns>
        public int? ParseInt(bool trim = true)
        {
            return TryParseInt(value, out var result, trim) ? (int?)result : null;
        }

        /// <summary>
        ///     Validates a Int32 value.
        /// </summary>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the value is a Int32</returns>
        public bool IsInt(bool trim = true)
        {
            return TryParseInt(value, out var _, trim);
        }

        /// <summary>
        ///     Parse a Int32 value.
        /// </summary>
        /// <param name="result">The parsed value.</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the Int32 could be parsed.</returns>
        public bool TryParseInt(out int result, bool trim = true)
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
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>The Int64 value.</returns>
        public long? ParseLong(bool trim = true)
        {
            return TryParseLong(value, out var result, trim) ? (long?)result : null;
        }

        /// <summary>
        ///     Validates a Int64 value.
        /// </summary>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the value is a Int64</returns>
        public bool IsLong(bool trim = true)
        {
            return TryParseLong(value, out var _, trim);
        }

        /// <summary>
        ///     Parse a Int64 value.
        /// </summary>
        /// <param name="result">The parsed value.</param>
        /// <param name="trim">If true the value string will be trimmed.</param>
        /// <returns>True if the Int64 could be parsed.</returns>
        public bool TryParseLong(out long result, bool trim = true)
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
        /// <returns>A boolean indicating whether the string has a value</returns>
        public bool HasValue()
        {
            return !string.IsNullOrWhiteSpace(value);
        }


        /// <summary>
        /// Make sure first char is lower case
        /// </summary>
        /// <returns></returns>
        public string ToCamelCase()
        {
            return string.IsNullOrEmpty(value) ? string.Empty : char.ToLower(value[0]) + value.Substring(1);
        }
    }

    extension(string strValue)
    {
        public bool ContainsAny(StringComparison stringComparison, params string[] list)
        {
            return list.Any(s => s.Equals(strValue, stringComparison));
        }

        public bool ContainsAny(params string[] list)
        {
            return list.Any(s => s.Equals(strValue, StringComparison.OrdinalIgnoreCase));
        }
    }

    extension(string value)
    {
        public bool GetBoolean(bool defaultValue = false)
        {
            if (value != null)
            {
                if (bool.TryParse(value, out var boolValue))
                {
                    return boolValue;
                }
                return defaultValue;
            }
            else
            {
                return defaultValue;
            }
        }
    }

    extension(string s)
    {
        /// <summary>
        /// Returns the input string with the first character converted to uppercase
        /// </summary>
        public string ToUpperFirst()
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
        public string ToLowerFirst()
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            Span<char> a = stackalloc char[s.Length];
            s.AsSpan(1).CopyTo(a.Slice(1));
            a[0] = char.ToLower(s[0]);
            return new string(a);
        }
    }

    extension(string source)
    {
        /// <summary>
        /// Compute unique hash of string
        /// </summary>
        /// <returns></returns>
        public string ToHash()
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            byte[] data = Encoding.UTF8.GetBytes(source);
            return $"{xxHash3.ComputeHash(data, data.Length)}";
        }
    }

    extension(string self)
    {
        /// <summary>
        /// Remove white spaces.
        /// </summary>
        /// <returns></returns>
        public string RemoveWhiteSpace()
        {
            return new string(self.Where(c => !Char.IsWhiteSpace(c)).ToArray());
        }
    }
}