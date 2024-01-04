﻿using Hangfire;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using System.Xml.Linq;

namespace SOS.Harvest.Harvesters.DwC.Interfaces
{
    public interface IDwcChecklistHarvester : IChecklistHarvester
    {
        /// <summary>
        /// Get EML XML document.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <returns></returns>
        XDocument GetEmlXmlDocument(string archivePath);

        /// <summary>
        /// Harvest checklist from file
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="dataProvider"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HarvestInfo> HarvestChecklistsAsync(
            string archivePath,
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken);
    }
}