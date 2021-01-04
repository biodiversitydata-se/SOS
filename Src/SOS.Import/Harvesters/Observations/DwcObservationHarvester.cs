using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using DwC_A;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.DarwinCore.Interfaces;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Import.Harvesters.Observations
{
    /// <summary>
    ///     DwC-A observation harvester.
    /// </summary>
    public class DwcObservationHarvester : IDwcObservationHarvester
    {
        private const int BatchSize = 100000;
        private readonly IDarwinCoreArchiveEventRepository _dwcArchiveEventRepository;
        private readonly IDwcArchiveReader _dwcArchiveReader;
        private readonly IDarwinCoreArchiveVerbatimRepository _dwcArchiveVerbatimRepository;
        private readonly DwcaConfiguration _dwcaConfiguration;
        private readonly ILogger<DwcObservationHarvester> _logger;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="dwcArchiveVerbatimRepository"></param>
        /// <param name="dwcArchiveEventRepository"></param>
        /// <param name="dwcArchiveReader"></param>
        /// <param name="dwcaConfiguration"></param>
        /// <param name="logger"></param>
        public DwcObservationHarvester(
            IDarwinCoreArchiveVerbatimRepository dwcArchiveVerbatimRepository,
            IDarwinCoreArchiveEventRepository dwcArchiveEventRepository,
            IDwcArchiveReader dwcArchiveReader,
            DwcaConfiguration dwcaConfiguration,
            ILogger<DwcObservationHarvester> logger)
        {
            _dwcArchiveVerbatimRepository = dwcArchiveVerbatimRepository ??
                                            throw new ArgumentNullException(nameof(dwcArchiveVerbatimRepository));
            _dwcArchiveEventRepository = dwcArchiveEventRepository ??
                                         throw new ArgumentNullException(nameof(dwcArchiveEventRepository));
            _dwcArchiveReader = dwcArchiveReader ?? throw new ArgumentNullException(nameof(dwcArchiveReader));
            _dwcaConfiguration = dwcaConfiguration ?? throw new ArgumentNullException(nameof(dwcaConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get EML XML document.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <returns></returns>
        public XDocument GetEmlXmlDocument(string archivePath)
        {
            using var archiveReader = new ArchiveReader(archivePath, _dwcaConfiguration.ImportPath);
            var emlFile = archiveReader.GetEmlXmlDocument();
            return emlFile;
        }

        /// <summary>
        ///     Harvest DwC Archive observations
        /// </summary>
        /// <returns></returns>
        public async Task<HarvestInfo> HarvestObservationsAsync(
            string archivePath,
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(DateTime.Now);

            try
            {
                _logger.LogDebug("Start storing DwC-A observations");
                await _dwcArchiveVerbatimRepository.ClearTempHarvestCollection(dataProvider);
                var observationCount = 0;
                using var archiveReader = new ArchiveReader(archivePath, _dwcaConfiguration.ImportPath);
                var observationBatches =
                    _dwcArchiveReader.ReadArchiveInBatchesAsync(archiveReader, dataProvider, BatchSize);
                await foreach (var verbatimObservationsBatch in observationBatches)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    observationCount += verbatimObservationsBatch.Count;
                    await _dwcArchiveVerbatimRepository.AddManyToTempHarvestAsync(verbatimObservationsBatch,
                        dataProvider);
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

        /// <summary>
        ///     Harvest DwC Archive observations
        /// </summary>
        /// <returns></returns>
        public async Task<HarvestInfo> HarvestMultipleDwcaFilesAsync(
            string[] filePaths,
            bool emptyCollectionsBeforeHarvest,
            IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(DateTime.Now);
            var latestFilePath = "";
            try
            {
                var occurrenceCollectionName = nameof(DwcObservationVerbatim);
                var eventCollectionName = nameof(DwcEvent);
                _logger.LogDebug("Start storing DwC-A observations");
                if (emptyCollectionsBeforeHarvest)
                {
                    await _dwcArchiveVerbatimRepository.DeleteCollectionAsync(occurrenceCollectionName);
                    await _dwcArchiveVerbatimRepository.AddCollectionAsync(occurrenceCollectionName);
                    await _dwcArchiveVerbatimRepository.DeleteCollectionAsync(eventCollectionName);
                    await _dwcArchiveVerbatimRepository.AddCollectionAsync(eventCollectionName);
                }

                var observationCount = 0;
                foreach (var filePath in filePaths)
                {
                    latestFilePath = filePath;
                    var datasetInfo = new IdIdentifierTuple
                    {
                        Identifier = Path.GetFileNameWithoutExtension(filePath),
                        Id = -1 // Unknown
                    };

                    using var archiveReader = new ArchiveReader(filePath, _dwcaConfiguration.ImportPath);
                    var observationBatches =
                        _dwcArchiveReader.ReadArchiveInBatchesAsync(archiveReader, datasetInfo, BatchSize);
                    await foreach (var verbatimObservationsBatch in observationBatches)
                    {
                        cancellationToken?.ThrowIfCancellationRequested();
                        observationCount += verbatimObservationsBatch.Count;
                        await _dwcArchiveVerbatimRepository.AddManyAsync(verbatimObservationsBatch,
                            occurrenceCollectionName);
                    }

                    // Read and store events
                    if (archiveReader.IsSamplingEventCore)
                    {
                        _logger.LogDebug("Start storing DwC-A events");
                        var eventBatches =
                            _dwcArchiveReader.ReadSamplingEventArchiveInBatchesAsDwcEventAsync(archiveReader,
                                datasetInfo,
                                BatchSize);
                        await foreach (var eventBatch in eventBatches)
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

        private async Task HarvestEventData(IIdIdentifierTuple idIdentifierTuple,
            ArchiveReader archiveReader,
            IJobCancellationToken cancellationToken)
        {
            _logger.LogDebug("Start storing DwC-A events");
            await _dwcArchiveEventRepository.ClearTempHarvestCollection(idIdentifierTuple);
            var eventBatches =
                _dwcArchiveReader.ReadSamplingEventArchiveInBatchesAsDwcEventAsync(archiveReader, idIdentifierTuple,
                    BatchSize);
            await foreach (var eventBatch in eventBatches)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                await _dwcArchiveEventRepository.AddManyToTempHarvestAsync(eventBatch, idIdentifierTuple);
            }

            await _dwcArchiveEventRepository.RenameTempHarvestCollection(idIdentifierTuple);
            _logger.LogDebug("Finish storing DwC-A events");
        }
    }
}