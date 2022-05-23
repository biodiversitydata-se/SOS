using Hangfire;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Harvest.Harvesters.Interfaces
{
    public interface IChecklistHarvester
    {
        /// <summary>
        /// Harvest checklists based on mode
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        Task<HarvestInfo> HarvestChecklistsAsync(IJobCancellationToken cancellationToken);

        /// <summary>
        ///  Harvest checklists generic by provider
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HarvestInfo> HarvestChecklistsAsync(DataProvider provider, IJobCancellationToken cancellationToken);
    }
}