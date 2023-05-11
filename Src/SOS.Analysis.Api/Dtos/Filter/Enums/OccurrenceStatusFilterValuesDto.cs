namespace SOS.Analysis.Api.Dtos.Filter.Enums
{
    /// <summary>
    /// Possible values for the OccurrenceStatus filter.
    /// </summary>
    public enum OccurrenceStatusFilterValuesDto
    {
        /// <summary>
        /// Include only present observations.
        /// </summary>
        Present = 0,

        /// <summary>
        /// Include only absent observations.
        /// </summary>
        Absent = 1,

        /// <summary>
        /// Include both present and absent observations.
        /// </summary>
        BothPresentAndAbsent = 2
    }
}