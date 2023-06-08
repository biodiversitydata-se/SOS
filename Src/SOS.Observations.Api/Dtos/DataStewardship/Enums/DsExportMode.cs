using System.Runtime.Serialization;

namespace SOS.Observations.Api.Dtos.DataStewardship.Enums
{
    /// <summary>
    /// json or csv
    /// </summary>
    /// <value>json or csv</value>        
    public enum DsExportMode
    {
        /// <summary>
        /// Enum JsonEnum for json
        /// </summary>
        [EnumMember(Value = "json")]
        Json = 0,
        /// <summary>
        /// Enum CsvEnum for csv
        /// </summary>
        [EnumMember(Value = "csv")]
        Csv = 1
    }
}
