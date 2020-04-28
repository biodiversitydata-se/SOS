using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Configuration.Import
{
    public class VirtualHerbariumServiceConfiguration : RestServiceConfiguration
    {
        /// <summary>
        /// The number of sightings that should be harvested.
        /// If set to null all sightings will be fetched.
        /// </summary>
        public int? MaxNumberOfSightingsHarvested { get; set; } = null;
    }
}