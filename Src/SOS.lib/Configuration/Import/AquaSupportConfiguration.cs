namespace SOS.Lib.Configuration.Import
{
    public class AquaSupportConfiguration 
    {
        /// <summary>
        ///     Address for the service
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        ///     The year to start harvest from.
        /// </summary>
        public int StartHarvestYear { get; set; } = 1987;

        /// <summary>
        ///     A secret token is needed for authorization when making calls to AquaSupport web service.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        ///     The number of sightings that should be harvested.
        ///     If set to null all sightings will be fetched.
        /// </summary>
        public int? MaxNumberOfSightingsHarvested { get; set; } = null;
    }
}