using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace SOS.Export.IO.Csv.Converters
{
    /// <summary>
    /// Replaces newline and tabs with space.
    /// </summary>
    public class LineBreakTabStringConverter<T> : DefaultTypeConverter
    {
        private readonly Regex _rxNewLineTab = new Regex(@"\r\n?|\n|\t", RegexOptions.Compiled);

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            return text;
        }

        /// <summary>
        /// Replaces newline and tabs with space.
        /// </summary>
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return value == null ? "" : _rxNewLineTab.Replace(value.ToString(), " ");
        }
    }
}
