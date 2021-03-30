using System;

namespace SOS.Observations.Api.Dtos
{
    public class ProviderInfoDto
    {
        public int? DataProviderId { get; set; }

        public string DataProviderIdentifier { get; set; }

        /// <summary>
        ///     Number of items harvested
        /// </summary>
        public int? HarvestCount { get; set; }

        /// <summary>
        ///     Harvest end date and time
        /// </summary>
        public DateTime? HarvestEnd { get; set; }

        /// <summary>
        /// Harvest note
        /// </summary>
        public string HarvestNotes { get; set; }

        /// <summary>
        ///     Harvest start date and time
        /// </summary>
        public DateTime? HarvestStart { get; set; }

        /// <summary>
        ///     Status of harvest
        /// </summary>
        public string HarvestStatus { get; set; }

        /// <summary>
        /// Last incremental process count 
        /// </summary>
        public int? LatestIncrementalPublicCount { get; set; }

        /// <summary>
        /// Last incremental process count 
        /// </summary>
        public int? LatestIncrementalProtectedCount { get; set; }

        /// <summary>
        /// Last incremental process end 
        /// </summary>
        public DateTime? LatestIncrementalEnd { get; set; }

        /// <summary>
        /// Last incremental process status 
        /// </summary>
        public string LatestIncrementalStatus { get; set; }

        /// <summary>
        /// Last incremental process start 
        /// </summary>
        public DateTime? LatestIncrementalStart { get; set; }

        /// <summary>
        ///     Number of items processed
        /// </summary>
        public int? PublicProcessCount { get; set; }

        /// <summary>
        ///     Number of items processed
        /// </summary>
        public int? ProtectedProcessCount { get; set; }

        /// <summary>
        ///     Process end date and time
        /// </summary>
        public DateTime? ProcessEnd { get; set; }

        /// <summary>
        ///     Process start date and time
        /// </summary>
        public DateTime ProcessStart { get; set; }

        /// <summary>
        ///     Status of processing
        /// </summary>
        public string ProcessStatus { get; set; }

        /// <summary>
        ///     Id of data provider
        /// </summary>
        public string DataProviderType { get; private set; }
    }
}
