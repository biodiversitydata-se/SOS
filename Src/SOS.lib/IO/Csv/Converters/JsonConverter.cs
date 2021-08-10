using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace SOS.Lib.IO.Csv.Converters
{
    /// <summary>
    ///     Converts an object into a JSON string.
    /// </summary>
    public class JsonConverter<T> : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            return JsonSerializer.Deserialize<T>(text);
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return JsonSerializer.Serialize(value);
        }
    }
}