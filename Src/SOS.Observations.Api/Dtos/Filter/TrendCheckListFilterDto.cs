using System;
using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Observation specific filter
    /// </summary>
    public class TrendCheckListFilterDto
    {
        private TimeSpan _minEffortTime;

        ///     Only get data from these providers.
        /// </summary>
        public DataProviderFilterDto DataProvider { get; set; }

        /// <summary>
        /// Minimum spent time to find the taxa
        /// </summary>
        public string MinEffortTime
        {
            get => _minEffortTime == TimeSpan.Zero ? string.Empty : _minEffortTime.ToString();
            set
            {
                TimeSpan.TryParse(value, out _minEffortTime);
            }
        }
    }

    /// <summary>
    /// Observation specific filter
    /// </summary>
    public class TrendObservationFilterDto
    {
        /// <summary>
        ///     Only get data from these providers.
        /// </summary>
        public DataProviderFilterDto DataProvider { get; set; }

        /// <summary>
        /// Requested verification status.
        /// </summary>
        public SearchFilterBaseDto.StatusVerificationDto VerificationStatus { get; set; }
    }

    /// <summary>
    /// Filter used when calculating taxon trend
    /// </summary>
    public class CalculateTrendFilterDto
    {
        /// <summary>
        /// Checklist specific filter
        /// </summary>
        public TrendCheckListFilterDto CheckList { get; set; }

        /// <summary>
        /// Date filter.
        /// </summary>
        public DateFilterDto Date { get; set; }

        /// <summary>
        /// Geographics filter 
        /// </summary>
        public GeographicsFilterDto Geographics { get; set; }

        // todo - add this in later version
        ///// <summary>
        ///// Observation specific filter
        ///// </summary>
        //public TrendObservationFilterDto Observation { get; set; }
        
        /// <summary>
        /// Id of taxon
        /// </summary>
        public int TaxonId { get; set; }
    }
}