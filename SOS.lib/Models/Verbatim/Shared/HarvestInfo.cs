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
        /// Harvest end date and time
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Id of data set
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Harvest start date and time
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Number of sightings
        /// </summary>
        public int SightingCount { get; set; }
    }
}
