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
    /// Export property sets.
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
        /// All properties where we know there exist at least one observation with a value.
        /// </summary>
        AllWithValues,

        /// <summary>
        /// All properties exported
        /// </summary>
        All,

        /// <summary>
        /// The field doesn't belong to any field set.
        /// </summary>
        None
    }

    /// <summary>
    /// Property label type.
    /// </summary>
    public enum PropertyLabelType
    {
        /// <summary>
        /// Use the property name as title.
        /// </summary>
        PropertyName = 0,
        /// <summary>
        /// Use the property path as title.
        /// </summary>
        PropertyPath = 1,
        /// <summary>
        /// Use the swedish property name as title.
        /// </summary>
        Swedish = 2,

        /// <summary>
        /// Use the english property name as title.
        /// </summary>
        English = 3
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

    public enum DateFilterComparison
    {
        StartDate,
        EndDate,
        BothStartDateAndEndDate,
        StartDateEndDateMonthRange
    }
}