using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Processors.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Processors.DarwinCoreArchive
{
    /// <summary>
    ///     DwC-A observation processor.
    /// </summary>
    public class DwcaObservationProcessor : ObservationProcessorBase<DwcaObservationProcessor>,
        IDwcaObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IDwcaVerbatimRepository _dwcaVerbatimRepository;
        private readonly ProcessConfiguration _processConfiguration;
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dwcaVerbatimRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processedFieldMappingRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="logger"></param>
        public DwcaObservationProcessor(IDwcaVerbatimRepository dwcaVerbatimRepository,
            IProcessedObservationRepository processedObservationRepository,
            IProcessedFieldMappingRepository processedFieldMappingRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            IAreaHelper areaHelper,
            ProcessConfiguration processConfiguration,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            ILogger<DwcaObservationProcessor> logger) : base(processedObservationRepository, fieldMappingResolverHelper, dwcArchiveFileWriterCoordinator, logger)
        {
            _dwcaVerbatimRepository =
                dwcaVerbatimRepository ?? throw new ArgumentNullException(nameof(dwcaVerbatimRepository));
            _processedFieldMappingRepository = processedFieldMappingRepository ??
                                               throw new ArgumentNullException(nameof(processedFieldMappingRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _processConfiguration =
                processConfiguration ?? throw new ArgumentNullException(nameof(processConfiguration));

            if (processConfiguration == null)
            {
                throw new ArgumentNullException(nameof(processConfiguration));
            }
        }

        public override DataProviderType Type => DataProviderType.DwcA;

        public async Task<bool> DoesVerbatimDataExist()
        {
            var collectionExist = await _dwcaVerbatimRepository.CheckIfCollectionExistsAsync();
            return collectionExist;
        }

        public override async Task<ProcessingStatus> ProcessAsync(
            DataProvider dataProvider,
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            Logger.LogInformation($"Start Processing {dataProvider} verbatim observations");
            var startTime = DateTime.Now;
            try
            {
                var dataExists = await _dwcaVerbatimRepository.CheckIfCollectionExistsAsync(dataProvider.Id, dataProvider.Identifier);
                if (!dataExists)
                {
                    Logger.LogInformation($"Processing {dataProvider} failed because no harvested data existed.");
                    return ProcessingStatus.Failed(dataProvider.Identifier, dataProvider.Type, startTime, DateTime.Now);
                }

                Logger.LogDebug($"Start deleting {dataProvider} data");
                if (!await ProcessRepository.DeleteProviderDataAsync(dataProvider))
                {
                    Logger.LogError($"Failed to delete {dataProvider} data");
                    return ProcessingStatus.Failed(dataProvider.Identifier, dataProvider.Type, startTime, DateTime.Now);
                }

                Logger.LogDebug($"Finish deleting {dataProvider} data");

                Logger.LogDebug($"Start processing {dataProvider} data");
                var verbatimCount = await ProcessObservationsSequential(
                    dataProvider,
                    taxa,
                    cancellationToken);
                
                Logger.LogInformation($"Finish processing {dataProvider} data.");
                return ProcessingStatus.Success(dataProvider.Identifier, dataProvider.Type, startTime, DateTime.Now, verbatimCount);
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation($"{dataProvider} observation processing was canceled.");
                return ProcessingStatus.Cancelled(dataProvider.Identifier, dataProvider.Type, startTime, DateTime.Now);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to process {dataProvider} sightings");
                return ProcessingStatus.Failed(dataProvider.Identifier, dataProvider.Type, startTime, DateTime.Now);
            }
        }

        /// <summary>
        ///     Process all observations
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task<int> ProcessObservationsSequential(
            DataProvider dataProvider,
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            var observationFactory = await DwcaObservationFactory.CreateAsync(
                dataProvider,
                taxa,
                _processedFieldMappingRepository,
                _areaHelper);
            ICollection<ProcessedObservation> sightings = new List<ProcessedObservation>();
            using var cursor = await _dwcaVerbatimRepository.GetAllByCursorAsync(dataProvider.Id, dataProvider.Identifier);
            int counter = 0;
            // Process and commit in batches.
            await cursor.ForEachAsync(async verbatimObservation =>
            {
                var processedObservation = observationFactory.CreateProcessedObservation(verbatimObservation);
                processedObservation.DataProviderId = dataProvider.Id;
                sightings.Add(processedObservation);
                if (IsBatchFilledToLimit(sightings.Count))
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    verbatimCount += await CommitBatchAsync(dataProvider, sightings);
                    //var csvResult = await dwcArchiveFileWriterCoordinator.WriteObservations(sightings, dataProvider);
                    var csvResult = await dwcArchiveFileWriterCoordinator.WriteObservations(sightings, dataProvider, counter++.ToString());
                    sightings.Clear();
                    Logger.LogDebug($"DwC-A sightings processed: {verbatimCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (sightings.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                verbatimCount += await CommitBatchAsync(dataProvider, sightings);
                //var csvResult = await dwcArchiveFileWriterCoordinator.WriteObservations(sightings, dataProvider);
                var csvResult = await dwcArchiveFileWriterCoordinator.WriteObservations(sightings, dataProvider, counter.ToString());
                Logger.LogDebug($"DwC-A sightings processed: {verbatimCount}");
            }

            return verbatimCount;
        }
    }
}