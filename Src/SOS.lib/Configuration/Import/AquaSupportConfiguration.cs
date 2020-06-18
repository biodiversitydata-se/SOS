using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Configuration.Import
{
    public class AquaSupportConfiguration : RestServiceConfiguration
    {
        /// <summary>
        ///     The year to start harvest from.
        /// </summary>
        public int StartHarvestYear { get; set; } = 1987;
    }
}