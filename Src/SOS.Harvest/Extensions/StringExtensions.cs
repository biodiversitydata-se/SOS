using SOS.Lib.Constants;
using System.Globalization;

namespace SOS.Harvest.Extensions;

public static class StringExtensions
{
    private static readonly CultureInfo GbCultureInfo = CultureInfo.CreateSpecificCulture(Cultures.en_GB);

    extension(string value)
    {
        /// <summary>
        ///     Truncates a string to a specific length.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <param name="maxLength">The max length.</param>
        /// <returns>A truncated string.</returns>
        public string? WithMaxLength(int maxLength)
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
        ///     Validates a Boolean value that has been
        ///     received over the internet.
        /// </summary>
        /// <param name='value'>Boolean string value to validate</param>
        /// <returns>True if the value is a Boolean</returns>
        public bool WebIsBoolean()
        {
            bool dummyVal;
            return bool.TryParse(value, out dummyVal);
        }

        /// <summary>
        ///     Parse a Boolean value that has been
        ///     received over the internet.
        /// </summary>
        /// <param name='value'>Boolean value to convert to a string.</param>
        /// <returns>The Boolean value as a string.</returns>
        public bool WebParseBoolean()
        {
            return bool.Parse(value);
        }

        /// <summary>
        ///     Validates a DateTime value that has been
        ///     received over the internet.
        /// </summary>
        /// <param name='value'>DateTime string value to validate</param>
        /// <returns>True if the value is a DateTime</returns>
        public bool WebIsDateTime()
        {
            DateTime dummyVal;
            return DateTime.TryParse(value, out dummyVal);
        }

        /// <summary>
        ///     Parse a DateTime value that has been
        ///     received over the internet.
        /// </summary>
        /// <param name='value'>DateTime value to convert to a string.</param>
        /// <returns>The DateTime value as a string.</returns>
        public DateTime WebParseDateTime()
        {
            return DateTime.Parse(value);
        }

        /// <summary>
        ///     Validates a Double value that has been
        ///     received over the internet.
        /// </summary>
        /// <param name='value'>Double string value to validate</param>
        /// <returns>True if the value is a Double</returns>
        public bool WebIsDouble()
        {
            const NumberStyles styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent |
                                        NumberStyles.AllowLeadingSign;
            double dummyVal;
            return double.TryParse(value.Replace(",", "."), styles, GbCultureInfo, out dummyVal);
        }

        /// <summary>
        ///     Parse a Double value that has been
        ///     received over the internet.
        /// </summary>
        /// <param name='value'>Double value to convert to a string.</param>
        /// <returns>The Double value as a string.</returns>
        public double WebParseDouble()
        {
            NumberStyles styles;

            styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign;
            return double.Parse(value.Replace(",", "."), styles, GbCultureInfo);
        }

        /// <summary>
        ///     Validate a Int32 value that has been
        ///     received over the internet.
        /// </summary>
        /// <param name='value'>Int32 string value to validate</param>
        /// <returns>True if the value is an Int32</returns>
        public bool WebIsInt32()
        {
            int dummyVal;
            return int.TryParse(value, out dummyVal);
        }

        /// <summary>
        ///     Parse a Int32 value that has been
        ///     received over the internet.
        /// </summary>
        /// <param name='value'>Int32 value to convert to a string.</param>
        /// <returns>The Int32 value as a string.</returns>
        public int WebParseInt32()
        {
            return int.Parse(value);
        }

        /// <summary>
        ///     Validate a Int64 value that has been
        ///     received over the internet.
        /// </summary>
        /// <param name='value'>Int64 string value to validate</param>
        /// <returns>True if the values is an Int64</returns>
        public bool WebIsInt64()
        {
            long dummyVal;
            return long.TryParse(value, out dummyVal);
        }

        /// <summary>
        ///     Parse a Int64 value that has been
        ///     received over the internet.
        /// </summary>
        /// <param name='value'>Int64 value to convert to a string.</param>
        /// <returns>The Int64 value as a string.</returns>
        public long WebParseInt64()
        {
            return long.Parse(value);
        }
    }

    extension(string input)
    {
        /// <summary>
        ///     Converts first char to lower case.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string ToLowerFirstChar()
        {
            var newString = input;
            if (!string.IsNullOrEmpty(newString) && char.IsUpper(newString[0]))
                newString = char.ToLower(newString[0]) + newString.Substring(1);
            return newString;
        }
    }
}