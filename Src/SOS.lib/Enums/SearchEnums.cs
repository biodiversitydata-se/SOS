using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SOS.Lib.Enums
{
    /// <summary>
    ///     Sort order enum
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SearchSortOrder
    {
        /// <summary>
        ///     Sort ascending
        /// </summary>
        Asc = 0,

        /// <summary>
        ///     Sort descending
        /// </summary>
        Desc = 1
    }

    /// <summary>
    ///     Type of aggregation
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AggregationType
    {
        /// <summary>
        /// 
        /// </summary>
        SightingsPerWeek = 0,
        /// <summary>
        /// 
        /// </summary>
        SightingsPerYear = 1,
        /// <summary>
        /// 
        /// </summary>
        QuantityPerWeek = 2,
        /// <summary>
        /// 
        /// </summary>
        QuantityPerYear = 3
    }
}