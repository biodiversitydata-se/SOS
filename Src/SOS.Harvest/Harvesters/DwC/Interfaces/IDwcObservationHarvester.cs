using System.Xml.Linq;
using Hangfire;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Harvest.Harvesters.DwC.Interfaces
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
        /// Harvest observations.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="dataProvider"></param>
        /// <param name="initializeTempCollection"></param>
        /// <param name="permanentizeTempCollection"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HarvestInfo> HarvestObservationsAsync(
           string archivePath,
           DataProvider dataProvider,
           bool initializeTempCollection,
           bool permanentizeTempCollection,
           IJobCancellationToken cancellationToken);

        /// <summary>
        /// Get EML XML document.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <returns></returns>
        XDocument GetEmlXmlDocument(string archivePath);
    }
}