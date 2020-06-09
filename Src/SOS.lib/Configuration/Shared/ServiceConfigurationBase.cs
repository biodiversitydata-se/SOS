namespace SOS.Lib.Configuration.Shared
{
    public class ServiceConfigurationBase
    {
        /// <summary>
        ///     A secret token is needed for authorization when making calls to KUL web service.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        ///     Max number of sightings that will be returned.
        /// </summary>
        public int MaxReturnedChangesInOnePage { get; set; } = 100000;

        /// <summary>
        ///     The number of sightings that should be harvested.
        ///     If set to null all sightings will be fetched.
        /// </summary>
        public int? MaxNumberOfSightingsHarvested { get; set; } = null;
    }
}