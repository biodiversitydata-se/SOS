using System.Collections.Generic;
using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Configuration.Import
{
    public class SharkServiceConfiguration : RestServiceConfiguration
    {
        /// <summary>
        /// The number of sightings that should be harvested.
        /// If set to null all sightings will be fetched.
        /// </summary>
        public int? MaxNumberOfSightingsHarvested { get; set; } = null;

        /// <summary>
        /// Data types to handle
        /// </summary>
        public IEnumerable<string> ValidDataTypes { get; set; }
    }
}