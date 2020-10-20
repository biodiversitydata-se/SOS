using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Managers.Interfaces;

namespace SOS.Process.Processors
{
    public abstract class ObservationProcessorBase<TEntity>
    {
        protected readonly IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator;
        protected readonly IFieldMappingResolverHelper FieldMappingResolverHelper;
        protected readonly ILogger<TEntity> Logger;
        protected readonly IProcessedObservationRepository ProcessRepository;
        protected readonly IValidationManager ValidationManager;

        protected ObservationProcessorBase(IProcessedObservationRepository processedObservationRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ILogger<TEntity> logger)
        {
            ProcessRepository = processedObservationRepository ??
                                throw new ArgumentNullException(nameof(processedObservationRepository));
            FieldMappingResolverHelper = fieldMappingResolverHelper ??
                                         throw new ArgumentNullException(nameof(fieldMappingResolverHelper));
            this.dwcArchiveFileWriterCoordinator = dwcArchiveFileWriterCoordinator ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriterCoordinator));
            ValidationManager = validationManager ?? throw new ArgumentNullException(nameof(validationManager));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public abstract DataProviderType Type { get; }

        /// <summary>
        /// Process observations
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<ProcessingStatus> ProcessAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            Logger.LogInformation($"Start Processing {dataProvider} verbatim observations");
            var startTime = DateTime.Now;
            try
            {
                if (mode == JobRunModes.Full)
                {
                    Logger.LogDebug($"Start deleting {dataProvider} data");
                    if (!await ProcessRepository.DeleteProviderDataAsync(dataProvider))
                    {
                        Logger.LogError($"Failed to delete {dataProvider} data");
                        return ProcessingStatus.Failed(dataProvider.Identifier, Type, startTime, DateTime.Now);
                    }

                    Logger.LogDebug($"Finish deleting {dataProvider} data");
                }
                
                Logger.LogDebug($"Start processing {dataProvider} data");
                var verbatimCount = await ProcessObservations(dataProvider, taxa, mode, cancellationToken);
                Logger.LogInformation($"Finish processing {dataProvider} data.");

                return ProcessingStatus.Success(dataProvider.Identifier, Type, startTime, DateTime.Now, verbatimCount);
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation($"{dataProvider} observation processing was canceled.");
                return ProcessingStatus.Cancelled(dataProvider.Identifier, Type, startTime, DateTime.Now);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to process {dataProvider} sightings");
                return ProcessingStatus.Failed(dataProvider.Identifier, Type, startTime, DateTime.Now);
            }
        }

        protected abstract Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken);


        protected async Task<int> CommitBatchAsync(
            DataProvider dataProvider,
            ICollection<Observation> processedObservations)
        {
            try
            {
                if (FieldMappingResolverHelper.Configuration.ResolveValues)
                {
                    // used for testing purpose for easier debugging of field mapped data.
                    FieldMappingResolverHelper
                        .ResolveFieldMappedValues(
                            processedObservations); 
                }

                return await ProcessRepository.AddManyAsync(processedObservations);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to commit batch for {dataProvider}");
                return 0;
            }
           
        }

        /// <summary>
        /// Resolve field mapping values and then write the observations to DwC-A CSV files.
        /// </summary>
        /// <param name="processedObservations"></param>
        /// <param name="dataProvider"></param>
        /// <param name="batchId"></param>
        /// <returns></returns>
        protected async Task<bool> WriteObservationsToDwcaCsvFiles(
            IEnumerable<Observation> processedObservations,
            DataProvider dataProvider,
            string batchId = "")
        {
            FieldMappingResolverHelper.ResolveFieldMappedValues(processedObservations, Cultures.en_GB);
            return await dwcArchiveFileWriterCoordinator.WriteObservations(processedObservations, dataProvider, batchId);
        }

        protected async Task<bool> DeleteProviderBatchAsync(
            DataProvider dataProvider,
            ICollection<int> verbatimIds)
        {
            try
            {
                return await ProcessRepository.DeleteProviderBatchAsync(dataProvider, verbatimIds);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to commit batch for {dataProvider}");
                return false;
            }

        }

        protected bool IsBatchFilledToLimit(int count)
        {
            return count % ProcessRepository.BatchSize == 0;
        }
    }
}