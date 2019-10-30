using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Import.Models
{
    public class KulAggregationOptions
    {
        /// <summary>
        /// The number of sightings that should be harvested.
        /// If set to null all sightings will be fetched.
        /// </summary>
        public int? MaxNumberOfSightingsHarvested { get; set; } = null;

        /// <summary>
        /// The year to start harvest from.
        /// </summary>
        public int StartHarvestYear { get; set; } = 1987;
    }
}
