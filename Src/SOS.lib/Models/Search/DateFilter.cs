using System;
using System.Collections.Generic;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Date related filter
    /// </summary>
    public class DateFilter
    {
        /// <summary>
        /// OverlappingStartDateAndEndDate, Start or EndDate of the observation must be within the specified interval    
        /// BetweenStartDateAndEndDate, Start and EndDate of the observation must be within the specified interval    
        /// OnlyStartDate, Only StartDate of the observation must be within the specified interval            
        /// OnlyEndDate, Only EndDate of the observation must be within the specified interval    
        /// </summary>
        public enum DateRangeFilterType
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
        /// Pre defined time ranges
        /// </summary>
        public enum TimeRange
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
        ///     Which type of date filtering that should be used
        /// </summary>
        public DateRangeFilterType DateFilterType { get; set; } = DateRangeFilterType.OverlappingStartDateAndEndDate;

        /// <summary>
        ///     Observation end date specified in the ISO 8601 standard.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        ///     Observation start date specified in the ISO 8601 standard.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Predefined time ranges.
        /// </summary>
        public IEnumerable<TimeRange> TimeRanges { get; set; }
    }
}