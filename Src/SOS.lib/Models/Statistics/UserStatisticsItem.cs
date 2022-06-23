using System.Collections.Generic;
using SOS.Lib.Enums;

namespace SOS.Lib.Models.Statistics
{
    public class UserStatisticsItem
    {
        /// <summary>
        /// UserId.
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Number of species (taxa) the user has found.
        /// </summary>
        public int SpeciesCount { get; set; }

        /// <summary>
        /// Experimental. Perhaps move to another class.
        /// </summary>
        public Dictionary<(AreaType AreaType, string FeatureId), int> SpeciesCountByArea { get; set; }

        /// <summary>
        /// Experimental. Perhaps move to another class.
        /// </summary>
        public int ObservationCount { get; set; }

        /// <summary>
        /// Experimental. Perhaps move to another class.
        /// </summary>
        public int ReportCount { get; set; }
    }
}