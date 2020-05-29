using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations.Interfaces
{
    /// <summary>
    ///     Clam Portal observation harvester interface
    /// </summary>
    public interface IClamPortalObservationHarvester
    {
        /// <summary>
        ///     Harvest clams.
        /// </summary>
        /// <returns></returns>
        Task<HarvestInfo> HarvestClamsAsync(IJobCancellationToken cancellationToken);
    }
}