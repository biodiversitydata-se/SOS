using System;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Verbatim.Shared
{
    public class HarvestInfo : IEntity<string>
    {
        /// <summary>
        /// Id of data provider
        /// </summary>
        public DataProviderId DataProvider { get; set; }

        /// <summary>
        /// Id of data set
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Harvest date and time
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// Number of sightings
        /// </summary>
        public int SightingCount { get; set; }
    }
}
