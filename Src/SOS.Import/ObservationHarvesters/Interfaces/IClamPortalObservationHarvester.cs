using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.ObservationHarvesters.Interfaces
{
    /// <summary>
    /// Clam and tree portal observation factory interface
    /// </summary>
    public interface IClamPortalObservationHarvester
    {
        /// <summary>
        /// Harvest clams.
        /// </summary>
        /// <returns></returns>
        Task<HarvestInfo> HarvestClamsAsync(IJobCancellationToken cancellationToken);
    }
}
