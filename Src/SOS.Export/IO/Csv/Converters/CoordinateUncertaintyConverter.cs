using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace SOS.Export.IO.Csv.Converters
{
    /// <summary>
    /// Replaces newline and tabs with space.
    /// </summary>
    public class CoordinateUncertaintyConverter<T> : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            if (int.TryParse(text, out int result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Replaces newline and tabs with space.
        /// </summary>
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value == null)
            {
                return "";
            }

            int val = (int) value;
            if (val == 0)
            {
                return 1.ToString();
            }
            else
            {
                return val.ToString();
            }
        }
    }
}
