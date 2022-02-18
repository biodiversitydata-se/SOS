using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.CheckLists.Interfaces
{
    public interface ICheckListHarvester
    {
        /// <summary>
        /// Harvest checklists based on mode
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        Task<HarvestInfo> HarvestCheckListsAsync(IJobCancellationToken cancellationToken);

        /// <summary>
        ///  Harvest check lists generic by provider
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HarvestInfo> HarvestCheckListsAsync(DataProvider provider, IJobCancellationToken cancellationToken);
    }
}