using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.ClamPortal;

namespace SOS.Import.Services.Interfaces
{
    /// <summary>
    /// Species data service
    /// </summary>
    public interface IClamObservationService
    {
        /// <summary>
        /// Get clam observations
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ClamObservationVerbatim>> GetClamObservationsAsync();
    }
}
