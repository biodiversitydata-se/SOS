namespace SOS.Lib.Configuration.Import
{
    public class SharkServiceConfiguration
    {
        // <summary>
        /// The number of sightings that should be harvested.
        /// If set to null all sightings will be fetched.
        /// </summary>
        public int? MaxNumberOfSightingsHarvested { get; set; } = null;

        /// <summary>
        /// Address to web service
        /// </summary>
        public string WebServiceAddress { get; set; }
    }
}