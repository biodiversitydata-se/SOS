using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Models
{
    /// <summary>
    /// json or csv
    /// </summary>
    /// <value>json or csv</value>        
    public enum ExportMode
    {
        /// <summary>
        /// Enum JsonEnum for json
        /// </summary>
        [EnumMember(Value = "json")]
        JsonEnum = 0,
        /// <summary>
        /// Enum CsvEnum for csv
        /// </summary>
        [EnumMember(Value = "csv")]
        CsvEnum = 1        
    }
}
