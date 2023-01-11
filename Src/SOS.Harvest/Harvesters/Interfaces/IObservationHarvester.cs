using Hangfire;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Harvest.Harvesters.Interfaces
{
    public interface IObservationHarvester
    {
        /// <summary>
        /// Harvest all observations
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken);

        /// <summary>
        ///  Harvest observations based on mode
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="fromDate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HarvestInfo> HarvestObservationsAsync(JobRunModes mode, DateTime? fromDate, IJobCancellationToken cancellationToken);

        /// <summary>
        ///  Harvest observations generic by provider
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HarvestInfo> HarvestObservationsAsync(DataProvider provider, IJobCancellationToken cancellationToken);
    }
}
