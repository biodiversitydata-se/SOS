namespace SOS.Lib.Configuration.Import
{
    public class KulServiceConfiguration : SluWcfConfiguration
    {
        /// <summary>
        ///     The year to start harvest from.
        /// </summary>
        public int StartHarvestYear { get; set; } = 1987;
    }
}