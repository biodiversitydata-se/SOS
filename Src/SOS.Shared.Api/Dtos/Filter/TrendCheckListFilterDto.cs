﻿using SOS.Shared.Api.Dtos.Enum;

namespace SOS.Shared.Api.Dtos.Filter
{
    /// <summary>
    /// Checklist filter
    /// </summary>
    public class TrendChecklistFilterDto
    {
        private TimeSpan _minEffortTime;

        /// <summary>
        /// Only get data from these providers.
        /// </summary>
        public DataProviderFilterDto? DataProvider { get; set; }

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
        public DataProviderFilterDto? DataProvider { get; set; }

        /// <summary>
        /// Requested verification status.
        /// </summary>
        public StatusVerificationDto VerificationStatus { get; set; } = StatusVerificationDto.BothVerifiedAndNotVerified;
    }

    /// <summary>
    /// Filter used when calculating taxon trend
    /// </summary>
    public class CalculateTrendFilterDto
    {
        /// <summary>
        /// Checklist specific filter
        /// </summary>
        public TrendChecklistFilterDto? Checklist { get; set; }

        /// <summary>
        /// Date filter.
        /// </summary>
        public DateFilterDto? Date { get; set; }

        /// <summary>
        /// Geographics filter 
        /// </summary>
        public GeographicsFilterDto? Geographics { get; set; }

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