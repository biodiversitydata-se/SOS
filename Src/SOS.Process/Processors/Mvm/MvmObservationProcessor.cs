using System;
using System.Collections.Generic;
using System.Linq;
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
using SOS.Process.Processors.Mvm.Interfaces;

namespace SOS.Process.Processors.Mvm
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class MvmObservationProcessor : ObservationProcessorBase<MvmObservationProcessor>, IMvmObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IMvmObservationVerbatimRepository _mvmObservationVerbatimRepository;

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
            MvmObservationFactory observationFactory,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();
                Logger.LogDebug($"Start fetching {dataProvider.Identifier} batch ({startId}-{endId})");
                var verbatimObservationsBatch = await _mvmObservationVerbatimRepository.GetBatchAsync(startId, endId);
                Logger.LogDebug($"Finish fetching {dataProvider.Identifier} batch ({startId}-{endId})");

                if (!verbatimObservationsBatch?.Any() ?? true)
                {
                    return 0;
                }

                Logger.LogDebug($"Start processing {dataProvider.Identifier} batch ({startId}-{endId})");

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

                Logger.LogDebug($"Finish processing {dataProvider.Identifier} batch ({startId}-{endId})");

                return await ValidateAndStoreObservations(dataProvider, mode, false, observations, $"{startId}-{endId}");
            }
            catch (JobAbortedException e)
            {
                // Throw cancelation again to let function above handle it
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Process {dataProvider.Identifier} sightings from id: {startId} to id: {endId} failed");
            }
            finally
            {
                SemaphoreBatch.Release();
            }

            return 0;
        }

        /// <inheritdoc />
        protected override async Task<(int publicCount, int protectedCount)> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory = new MvmObservationFactory(dataProvider, taxa);

            var minId = 1;
            var maxId = await _mvmObservationVerbatimRepository.GetMaxIdAsync();
            var processBatchTasks = new List<Task<int>>();

            while (minId <= maxId)
            {
                await SemaphoreBatch.WaitAsync();

                var batchEndId = minId + WriteBatchSize - 1;
                processBatchTasks.Add(ProcessBatchAsync(dataProvider, minId, batchEndId, mode, observationFactory,
                    taxa, cancellationToken));
                minId = batchEndId + 1;
            }

            await Task.WhenAll(processBatchTasks);

            return (processBatchTasks.Sum(t => t.Result), 0);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mvmObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        public MvmObservationProcessor(IMvmObservationVerbatimRepository mvmObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ProcessConfiguration processConfiguration,
            ILogger<MvmObservationProcessor> logger) :
            base(processedPublicObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, processConfiguration, logger)
        {
            _mvmObservationVerbatimRepository = mvmObservationVerbatimRepository ??
                                                throw new ArgumentNullException(
                                                    nameof(mvmObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.MvmObservations;
    }
}