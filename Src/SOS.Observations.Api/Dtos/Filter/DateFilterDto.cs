using System;

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
        ///     Which type of date filtering that should be used
        /// </summary>
        public DateFilterTypeDto DateFilterType { get; set; } = DateFilterTypeDto.OverlappingStartDateAndEndDate;
    }
}