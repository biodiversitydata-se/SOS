using System.Threading.Tasks;
using System.Xml.Linq;
using Hangfire;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations.Interfaces
{
    public interface IDwcObservationHarvester : IObservationHarvester
    {
        /// <summary>
        ///     Harvest observations.
        /// </summary>
        /// <returns></returns>
        Task<HarvestInfo> HarvestObservationsAsync(
            string archivePath,
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken);

        /// <summary>
        ///     Harvest multiple DwC-A files.
        /// </summary>
        /// <param name="filePaths"></param>
        /// <param name="emptyCollectionsBeforeHarvest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HarvestInfo> HarvestMultipleDwcaFilesAsync(
            string[] filePaths,
            bool emptyCollectionsBeforeHarvest,
            IJobCancellationToken cancellationToken);

        /// <summary>
        /// Get EML XML document.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <returns></returns>
        XDocument GetEmlXmlDocument(string archivePath);
    }
}