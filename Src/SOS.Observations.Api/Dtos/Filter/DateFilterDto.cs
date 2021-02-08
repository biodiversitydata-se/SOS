using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SOS.Lib.JsonConverters;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// OverlappingStartDateAndEndDate, Start or EndDate of the observation must be within the specified interval    
    /// BetweenStartDateAndEndDate, Start and EndDate of the observation must be within the specified interval    
    /// OnlyStartDate, Only StartDate of the observation must be within the specified interval            
    /// OnlyEndDate, Only EndDate of the observation must be within the specified interval    
    /// </summary>
    public enum DateFilterTypeDto
    {
        /// <summary>
        /// Start or EndDate of the observation must be within the specified interval
        /// </summary>
        OverlappingStartDateAndEndDate,
        /// <summary>
        /// Start and EndDate of the observation must be within the specified interval
        /// </summary>
        BetweenStartDateAndEndDate,
        /// <summary>
        /// Only StartDate of the observation must be within the specified interval
        /// </summary>
        OnlyStartDate,
        /// <summary>
        /// Only EndDate of the observation must be within the specified interval
        /// </summary>
        OnlyEndDate       
    }

    public enum TimeRangeDto
    {
        /// <summary>
        /// 04:00-09:00
        /// </summary>
        Morning,
        /// <summary>
        /// 09:00-13:00
        /// </summary>
        Forenoon,
        /// <summary>
        /// 13:00-18:00
        /// </summary>
        Afternoon,
        /// <summary>
        /// 18:00-23:00
        /// </summary>
        Evening,
        /// <summary>
        /// 23:00-04:00
        /// </summary>
        Night
    }

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
        /// Pre defined time ranges
        /// </summary>
        public IEnumerable<TimeRangeDto> TimeRanges { get; set; }
    }
}