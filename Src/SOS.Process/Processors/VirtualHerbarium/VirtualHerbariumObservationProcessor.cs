using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Processors.VirtualHerbarium.Interfaces;

namespace SOS.Process.Processors.VirtualHerbarium
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class VirtualHerbariumObservationProcessor : ObservationProcessorBase<VirtualHerbariumObservationProcessor>,
        IVirtualHerbariumObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IVirtualHerbariumObservationVerbatimRepository _virtualHerbariumObservationVerbatimRepository;
        private readonly SemaphoreSlim _semaphoreBatch;

        /// <inheritdoc />
        protected override async Task<(int publicCount, int protectedCount)> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, 
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory = new VirtualHerbariumObservationFactory(dataProvider, taxa);

            var minId = 1;
            var maxId = await _virtualHerbariumObservationVerbatimRepository.GetMaxIdAsync();
            var processBatchTasks = new List<Task<int>>();

            while (minId <= maxId)
            {
                await _semaphoreBatch.WaitAsync();

                var batchEndId = minId + WriteBatchSize - 1;
                processBatchTasks.Add(ProcessBatchAsync(dataProvider, minId, batchEndId, mode, observationFactory,
                    taxa, cancellationToken));
                minId = batchEndId + 1;
            }

            await Task.WhenAll(processBatchTasks);

            return (processBatchTasks.Sum(t => t.Result), 0);
        }

        /// <summary>
        /// Process batch
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <param name="mode"></param>
        /// <param name="observationFactory"></param>
        /// <param name="taxa"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> ProcessBatchAsync(
            DataProvider dataProvider,
            int startId,
            int endId,
            JobRunModes mode,
            VirtualHerbariumObservationFactory observationFactory,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();
                Logger.LogDebug($"Start fetching Virtual Herbarium batch ({startId}-{endId})");
                var verbatimObservationsBatch = await _virtualHerbariumObservationVerbatimRepository.GetBatchAsync(startId, endId);
                Logger.LogDebug($"Finish fetching Virtual Herbarium batch ({startId}-{endId})");

                if (!verbatimObservationsBatch?.Any() ?? true)
                {
                    return 0;
                }

                Logger.LogDebug($"Start processing Virtual Herbarium batch ({startId}-{endId})");

                var observations = new List<Observation>();
                
                foreach (var verbatimObservation in verbatimObservationsBatch)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    var processedObservation = observationFactory.CreateProcessedObservation(verbatimObservation);

                    if (processedObservation == null)
                    {
                        continue;
                    }

                    _areaHelper.AddAreaDataToProcessedObservation(processedObservation);
                    observations.Add(processedObservation);
                }

                Logger.LogDebug($"Finish processing Virtual Herbarium batch ({startId}-{endId})");

                return await ValidateAndStoreObservations(dataProvider, mode, false, observations, $"{startId}-{endId}");
            }
            catch (JobAbortedException e)
            {
                // Throw cancelation again to let function above handle it
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Process Virtual Herbarium sightings from id: {startId} to id: {endId} failed");
            }
            finally
            {
                _semaphoreBatch.Release();
            }

            return 0;
        }

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="virtualHerbariumObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        public VirtualHerbariumObservationProcessor(
            IVirtualHerbariumObservationVerbatimRepository virtualHerbariumObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ProcessConfiguration processConfiguration,
            ILogger<VirtualHerbariumObservationProcessor> logger) : 
                base(processedPublicObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, logger)
        {
            _virtualHerbariumObservationVerbatimRepository = virtualHerbariumObservationVerbatimRepository ??
                                                             throw new ArgumentNullException(
                                                                 nameof(virtualHerbariumObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));

            _semaphoreBatch = new SemaphoreSlim(processConfiguration.NoOfThreads);
        }

        public override DataProviderType Type => DataProviderType.VirtualHerbariumObservations;
    }
}