using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using MongoDB.Bson.Serialization.Conventions;
using SOS.Import.DarwinCore;
using SOS.Import.DarwinCore.Interfaces;
using SOS.Import.Repositories.Destination.DarwinCoreArchive.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Shared;
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
        private readonly IDarwinCoreArchiveEventRepository _dwcArchiveEventRepository;
        private readonly IDwcArchiveReader _dwcArchiveReader;
        private readonly ILogger<DwcObservationHarvester> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dwcArchiveVerbatimRepository"></param>
        /// <param name="dwcArchiveEventRepository"></param>
        /// <param name="dwcArchiveReader"></param>
        /// <param name="logger"></param>
        public DwcObservationHarvester(
            IDarwinCoreArchiveVerbatimRepository dwcArchiveVerbatimRepository,
            IDarwinCoreArchiveEventRepository dwcArchiveEventRepository,
            IDwcArchiveReader dwcArchiveReader,
            ILogger<DwcObservationHarvester> logger)
        {
            _dwcArchiveVerbatimRepository = dwcArchiveVerbatimRepository ?? throw new ArgumentNullException(nameof(dwcArchiveVerbatimRepository));
            _dwcArchiveEventRepository = dwcArchiveEventRepository ?? throw new ArgumentNullException(nameof(dwcArchiveEventRepository));
            _dwcArchiveReader = dwcArchiveReader ?? throw new ArgumentNullException(nameof(dwcArchiveReader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Harvest DwC Archive observations
        /// </summary>
        /// <returns></returns>
        public async Task<HarvestInfo> HarvestObservationsAsync(
            string archivePath,
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken)
        {
            string harvestInfoId = HarvestInfo.GetIdFromDataProvider(dataProvider);
            var harvestInfo = new HarvestInfo(harvestInfoId, DataProviderType.DwcA, DateTime.Now);

            try
            {
                ConventionRegistry.Register("IgnoreIfNull",
                    new ConventionPack {new IgnoreIfNullConvention(true)},
                    t => true);

                _logger.LogDebug("Start storing DwC-A observations");
                await _dwcArchiveVerbatimRepository.ClearTempHarvestCollection(dataProvider);
                int observationCount = 0;
                using var archiveReader = new ArchiveReader(archivePath);
                var observationBatches =
                    _dwcArchiveReader.ReadArchiveInBatchesAsync(archiveReader, dataProvider, BatchSize);
                await foreach (List<DwcObservationVerbatim> verbatimObservationsBatch in observationBatches)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    observationCount += verbatimObservationsBatch.Count;
                    await _dwcArchiveVerbatimRepository.AddManyToTempHarvestAsync(verbatimObservationsBatch, dataProvider);
                }

                await _dwcArchiveVerbatimRepository.RenameTempHarvestCollection(dataProvider);
                _logger.LogDebug("Finish storing DwC-A observations");

                // Read and store events
                //if (archiveReader.IsSamplingEventCore) // todo - Use this when we have functionality to process event data.
                //{
                //    await HarvestEventData(datasetInfo, cancellationToken, archiveReader);
                //}

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
            finally
            {
                await _dwcArchiveVerbatimRepository.DeleteTempHarvestCollectionIfExists(dataProvider);
            }

            return harvestInfo;
        }

        private async Task HarvestEventData(IIdIdentifierTuple idIdentifierTuple,
            ArchiveReader archiveReader,
            IJobCancellationToken cancellationToken)
        {
            _logger.LogDebug("Start storing DwC-A events");
            await _dwcArchiveEventRepository.ClearTempHarvestCollection(idIdentifierTuple);
            var eventBatches = _dwcArchiveReader.ReadSamplingEventArchiveInBatchesAsDwcEventAsync(archiveReader, idIdentifierTuple, BatchSize);
            await foreach (List<DwcEvent> eventBatch in eventBatches)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                await _dwcArchiveEventRepository.AddManyToTempHarvestAsync(eventBatch, idIdentifierTuple);
            }

            await _dwcArchiveEventRepository.RenameTempHarvestCollection(idIdentifierTuple);
            _logger.LogDebug("Finish storing DwC-A events");
        }

        /// <summary>
        /// Harvest DwC Archive observations
        /// </summary>
        /// <returns></returns>
        public async Task<HarvestInfo> HarvestMultipleDwcaFilesAsync(
            string[] filePaths,
            bool emptyCollectionsBeforeHarvest,
            IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(nameof(DwcObservationVerbatim), DataProviderType.DwcA, DateTime.Now);
            string latestFilePath = "";
            try
            {
                string occurrenceCollectionName = nameof(DwcObservationVerbatim);
                string eventCollectionName = nameof(DwcEvent);
                ConventionRegistry.Register("IgnoreIfNull",
                    new ConventionPack {new IgnoreIfNullConvention(true)},
                    t => true);

                _logger.LogDebug("Start storing DwC-A observations");
                if (emptyCollectionsBeforeHarvest)
                {
                    await _dwcArchiveVerbatimRepository.DeleteCollectionAsync(occurrenceCollectionName);
                    await _dwcArchiveVerbatimRepository.AddCollectionAsync(occurrenceCollectionName);
                    await _dwcArchiveVerbatimRepository.DeleteCollectionAsync(eventCollectionName);
                    await _dwcArchiveVerbatimRepository.AddCollectionAsync(eventCollectionName);
                }

                int observationCount = 0;
                foreach (var filePath in filePaths)
                {
                    latestFilePath = filePath;
                    var datasetInfo = new IdIdentifierTuple
                    {
                        Identifier = Path.GetFileNameWithoutExtension(filePath),
                        Id = -1 // Unknown
                    };

                    using var archiveReader = new ArchiveReader(filePath);
                    var observationBatches =
                        _dwcArchiveReader.ReadArchiveInBatchesAsync(archiveReader, datasetInfo, BatchSize);
                    await foreach (List<DwcObservationVerbatim> verbatimObservationsBatch in observationBatches)
                    {
                        cancellationToken?.ThrowIfCancellationRequested();
                        observationCount += verbatimObservationsBatch.Count;
                        await _dwcArchiveVerbatimRepository.AddManyAsync(verbatimObservationsBatch, occurrenceCollectionName);
                    }

                    // Read and store events
                    if (archiveReader.IsSamplingEventCore)
                    {
                        _logger.LogDebug("Start storing DwC-A events");
                        var eventBatches =
                            _dwcArchiveReader.ReadSamplingEventArchiveInBatchesAsDwcEventAsync(archiveReader, datasetInfo,
                                BatchSize);
                        await foreach (List<DwcEvent> eventBatch in eventBatches)
                        {
                            cancellationToken?.ThrowIfCancellationRequested();
                            await _dwcArchiveEventRepository.AddManyAsync(eventBatch, eventCollectionName);
                        }

                    }
                }

                _logger.LogDebug("Finish storing DwC-A observations");
                _logger.LogDebug("Finish storing DwC-A events");

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
                _logger.LogError(e, $"Failed harvest of DwC Archive '{latestFilePath}'");
                harvestInfo.Status = RunStatus.Failed;
            }
            return harvestInfo;
        }
    }
}