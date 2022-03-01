using System.Xml.Linq;
using Hangfire;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Harvest.Harvesters.Interfaces;

namespace SOS.Harvest.Harvesters.DwC.Interfaces
{
    public interface IDwcCheckListHarvester : ICheckListHarvester
    {
        /// <summary>
        /// Get EML XML document.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <returns></returns>
        XDocument GetEmlXmlDocument(string archivePath);

        /// <summary>
        /// Harvest check list from file
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="dataProvider"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HarvestInfo> HarvestCheckListsAsync(
            string archivePath,
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken);
    }
}