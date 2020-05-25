using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SOS.Administration.Api.Models
{
    /// <summary>
    /// A summary of which data providers are harvested and processed.
    /// </summary>
    public class DataProviderHarvestAndProcessSettingsDto
    {
        /// <summary>
        /// List of all data providers that are scheduled to harvest automatically.
        /// </summary>
        public List<string> IncludedInScheduledHarvest { get; set; }
        
        /// <summary>
        /// List of all data providers that will be included in the processing step.
        /// </summary>
        public List<string> IncludedInProcessing { get; set; }

        /// <summary>
        /// List of all data providers that are not scheduled to harvest automatically.
        /// </summary>
        public List<string> NotIncludedInScheduledHarvest { get; set; }

        /// <summary>
        /// List of all data providers that not will be included in the processing step.
        /// </summary>
        public List<string> NotIncludedInProcessing { get; set; }
    }
}