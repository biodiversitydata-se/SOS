using System.Threading.Tasks;
using Hangfire;
using SOS.Import.DarwinCore;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations.Interfaces
{
    public interface IDwcObservationHarvester
    {
        /// <summary>
        /// Harvest observations.
        /// </summary>
        /// <returns></returns>
        Task<HarvestInfo> HarvestObservationsAsync(
            string archivePath,
            DwcaDatasetInfo datasetInfo,
            IJobCancellationToken cancellationToken);
    }
}
