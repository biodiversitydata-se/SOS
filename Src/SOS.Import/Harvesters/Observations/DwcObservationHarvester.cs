using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DwC_A;
using DwC_A.Meta;
using DwC_A.Terms;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.IO;
using SOS.Import.DarwinCore.Interfaces;
using SOS.Import.Repositories.Destination.DarwinCoreArchive.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations
{
    public class DwcObservationHarvester : Interfaces.IDwcObservationHarvester
    {
        private readonly IDarwinCoreArchiveVerbatimRepository _dwcArchiveVerbatimRepository;
        private readonly IDwcArchiveReader _dwcArchiveReader;
        private readonly ILogger<DwcObservationHarvester> _logger;

        public DwcObservationHarvester(
            IDarwinCoreArchiveVerbatimRepository dwcArchiveVerbatimRepository,
            IDwcArchiveReader dwcArchiveReader,
            ILogger<DwcObservationHarvester> logger)
        {
            _dwcArchiveVerbatimRepository = dwcArchiveVerbatimRepository ?? throw new ArgumentNullException(nameof(dwcArchiveVerbatimRepository));
            _dwcArchiveReader = dwcArchiveReader ?? throw new ArgumentNullException(nameof(dwcArchiveReader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Harvest DwC Archive observations
        /// </summary>
        /// <returns></returns>
        public async Task<HarvestInfo> HarvestObservationsAsync(string archivePath, IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(nameof(DwcObservationVerbatim), DataProvider.Dwc, DateTime.Now);
            try
            {
                _logger.LogDebug("Start storing DwC verbatim");
                var observations = await _dwcArchiveReader.ReadArchiveAsync(archivePath);
                await _dwcArchiveVerbatimRepository.DeleteCollectionAsync();
                await _dwcArchiveVerbatimRepository.AddCollectionAsync();
                await _dwcArchiveVerbatimRepository.AddManyAsync(observations);
                _logger.LogDebug("Finish storing DwC verbatim");

                cancellationToken?.ThrowIfCancellationRequested();

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = observations?.Count() ?? 0;
            }
            catch (JobAbortedException e)
            {
                _logger.LogError(e, "Canceled harvest of DwC Archive");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest of DwC Archive");
                harvestInfo.Status = RunStatus.Failed;
            }
            return harvestInfo;
        }
    }
}
