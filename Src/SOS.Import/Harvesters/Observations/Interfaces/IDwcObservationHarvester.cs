using System.Threading.Tasks;
using Hangfire;
using SOS.Import.DarwinCore;
using SOS.Lib.Models.Interfaces;
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
            IIdIdentifierTuple idIdentifierTuple,
            IJobCancellationToken cancellationToken);

        /// <summary>
        /// Harvest multiple DwC-A files.
        /// </summary>
        /// <param name="filePaths"></param>
        /// <param name="emptyCollectionsBeforeHarvest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HarvestInfo> HarvestMultipleDwcaFilesAsync(
            string[] filePaths,
            bool emptyCollectionsBeforeHarvest,
            IJobCancellationToken cancellationToken);
    }
}
