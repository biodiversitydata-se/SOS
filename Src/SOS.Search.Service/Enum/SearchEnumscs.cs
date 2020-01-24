using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SOS.Search.Service.Enum
{
    /// <summary>
    /// Sort order enum
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SearchSortOrder
    {
        /// <summary>
        /// Sort ascending
        /// </summary>
        Asc = 0,
        /// <summary>
        /// Sort descending
        /// </summary>
        Desc = 1
    }
}
