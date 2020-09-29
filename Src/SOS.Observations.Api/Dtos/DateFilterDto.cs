using System;

namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// Date filter.
    /// </summary>
    public class DateFilterDto
    {
        /// <summary>
        ///     Observation start date specified in the ISO 8601 standard.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        ///     Observation end date specified in the ISO 8601 standard.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        ///     If true the whole event timespan must be between StartDate and EndDate
        /// </summary>
        public bool SearchOnlyBetweenDates { get; set; }
    }
}