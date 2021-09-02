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
    /// Export property sets
    /// </summary>
    public enum OutputFieldSet
    {
        /// <summary>
        /// Minimum of properties exported
        /// </summary>
        Minimum,
        /// <summary>
        /// A extended set of properties exported
        /// </summary>
        Extended,
        /// <summary>
        /// All properties exported
        /// </summary>
        All
    }

    public enum SightingDeterminationFilter
    {
        NoFilter,
        NotUnsureDetermination,
        OnlyUnsureDetermination
    }

    public enum SightingUnspontaneousFilter
    {
        NoFilter,
        NotUnspontaneous,
        Unspontaneous
    }

    public enum SightingNotRecoveredFilter
    {
        NoFilter,
        OnlyNotRecovered,
        DontIncludeNotRecovered
    }

    public enum SightingNotPresentFilter
    {
        DontIncludeNotPresent,
        OnlyNotPresent,
        IncludeNotPresent
    }

    public enum MonthsFilterComparison
    {
        StartDate,
        EndDate,
        BothStartDateAndEndDate
    }
}