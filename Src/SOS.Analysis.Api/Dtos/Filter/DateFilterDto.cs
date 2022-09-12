using System.Text.Json.Serialization;
using SOS.Analysis.Api.Dtos.Filter.Enums;
using SOS.Lib.JsonConverters;

namespace SOS.Analysis.Api.Dtos.Filter
{
    /// <summary>
    /// Date filter.
    /// </summary>
    public class DateFilterDto
    {
        /// <summary>
        ///     Observation start date specified in the ISO 8601 standard. If no timezone is specified, GMT+1 (CEST) is assumed
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        ///     Observation end date specified in the ISO 8601 standard. If no timezone is specified, GMT+1 (CEST) is assumed
        /// </summary>
        [JsonConverter(typeof(EndDayConverter))]
        public DateTime? EndDate { get; set; }

        /// <summary>
        ///     Which type of date filtering that should be used
        /// </summary>
        public DateFilterTypeDto DateFilterType { get; set; } = DateFilterTypeDto.OverlappingStartDateAndEndDate;

        /// <summary>
        /// Predefined time ranges
        /// </summary>
        public List<TimeRangeDto>? TimeRanges { get; set; }
    }
}