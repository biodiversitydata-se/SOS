using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.ClamTreePortal;

namespace SOS.Import.Services.Interfaces
{
    /// <summary>
    /// Species data service
    /// </summary>
    public interface IClamTreeObservationService
    {
        /// <summary>
        /// Get clam observations
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ClamObservationVerbatim>> GetClamObservations();

        /// <summary>
        /// Get tree observations
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TreeObservationVerbatim>> GetTreeObservations(int pageNumber, int pageSize);
    }
}
