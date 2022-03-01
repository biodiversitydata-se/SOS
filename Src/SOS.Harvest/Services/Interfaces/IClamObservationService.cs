using SOS.Lib.Models.Verbatim.ClamPortal;

namespace SOS.Harvest.Services.Interfaces
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public interface IClamObservationService
    {
        /// <summary>
        ///     Get clam observations
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ClamObservationVerbatim>> GetClamObservationsAsync();
    }
}