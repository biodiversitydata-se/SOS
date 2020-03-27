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
    /// <summary>
    /// DwC-A observation harvester.
    /// </summary>
    public class DwcObservationHarvester : Interfaces.IDwcObservationHarvester
    {
        private const int BatchSize = 100000;
        private readonly IDarwinCoreArchiveVerbatimRepository _dwcArchiveVerbatimRepository;
        private readonly IDwcArchiveReader _dwcArchiveReader;
        private readonly ILogger<DwcObservationHarvester> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dwcArchiveVerbatimRepository"></param>
        /// <param name="dwcArchiveReader"></param>
        /// <param name="logger"></param>
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
            var harvestInfo = new HarvestInfo(nameof(DwcObservationVerbatim), DataSet.Dwc, DateTime.Now);
            try
            {
                _logger.LogDebug("Start storing DwC verbatim");
                await _dwcArchiveVerbatimRepository.DeleteCollectionAsync();
                await _dwcArchiveVerbatimRepository.AddCollectionAsync();

                int observationCount = 0;
                var observationBatches = _dwcArchiveReader.ReadArchiveInBatchesAsync(archivePath, BatchSize);
                await foreach (List<DwcObservationVerbatim> verbatimObservationsBatch in observationBatches)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    observationCount += verbatimObservationsBatch.Count;
                    await _dwcArchiveVerbatimRepository.AddManyAsync(verbatimObservationsBatch);
                }

                _logger.LogDebug("Finish storing DwC verbatim");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = observationCount;
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