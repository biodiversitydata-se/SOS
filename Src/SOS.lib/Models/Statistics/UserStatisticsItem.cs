using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
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
        /// Number of observations.
        /// </summary>
        public int ObservationCount { get; set; }

        /// <summary>
        /// Experimental. Perhaps move to another class.
        /// </summary>
        public Dictionary<string, int> SpeciesCountByFeatureId { get; set; }

        public UserStatisticsItem Clone()
        {
            return (UserStatisticsItem)MemberwiseClone();
        }
    }
}