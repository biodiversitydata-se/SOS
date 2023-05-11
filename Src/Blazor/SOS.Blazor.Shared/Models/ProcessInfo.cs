using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Blazor.Shared.Models
{
    public class ProcessInfo
    {
        /// <summary>
        ///     Item processed
        /// </summary>
        public int PublicCount { get; set; }

        /// <summary>
        /// Protected observations count
        /// </summary>
        public int ProtectedCount { get; set; }

        /// <summary>
        ///     Harvest end date and time
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        ///     Provider information about meta data
        /// </summary>
        public IEnumerable<ProcessInfo> MetadataInfo { get; set; }

        /// <summary>
        ///     Information about providers
        /// </summary>
        public IEnumerable<ProviderInfo> ProvidersInfo { get; set; }

        /// <summary>
        ///     Harvest start date and time
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        ///     Running status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        ///     Id, equals updated instance (0 or 1)
        /// </summary>
        public string Id { get; set; }
    }


    public class ProviderInfo
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
