using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Process.Extensions;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Factories
{
    /// <summary>
    /// Process factory class
    /// </summary>
    public class ArtportalenProcessFactory : DataProviderProcessorBase<ArtportalenProcessFactory>, Interfaces.IArtportalenProcessFactory
    {
        private readonly IArtportalenVerbatimRepository _artportalenVerbatimRepository;
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;
        private readonly SemaphoreSlim _semaphore;

        public override DataProvider DataProvider => DataProvider.Artportalen;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenVerbatimRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processedFieldMappingRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        public ArtportalenProcessFactory(
            IArtportalenVerbatimRepository artportalenVerbatimRepository,
            IProcessedObservationRepository processedObservationRepository,
            IProcessedFieldMappingRepository processedFieldMappingRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            ProcessConfiguration processConfiguration,
            ILogger<ArtportalenProcessFactory> logger) : base(processedObservationRepository, fieldMappingResolverHelper, logger)
        {
            _artportalenVerbatimRepository = artportalenVerbatimRepository ?? throw new ArgumentNullException(nameof(artportalenVerbatimRepository));
            _processedFieldMappingRepository = processedFieldMappingRepository ?? throw new ArgumentNullException(nameof(processedFieldMappingRepository));

            if (processConfiguration == null)
            {
                throw new ArgumentNullException(nameof(processConfiguration));
            }

            _semaphore = new SemaphoreSlim(processConfiguration.NoOfThreads);
        }

        /// <summary>
        /// Process all observations
        /// </summary>
        /// <param name="taxa"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<int> ProcessObservations(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            ICollection<ProcessedObservation> sightings = new List<ProcessedObservation>();
            var allFieldMappings = await _processedFieldMappingRepository.GetFieldMappingsAsync();
            var fieldMappings = GetFieldMappingsDictionary(ExternalSystemId.Artportalen, allFieldMappings.ToArray());

            // Get min and max id from db
            (await _artportalenVerbatimRepository.GetIdSpanAsync())
                .Deconstruct(out var batchStartId, out var maxId);
            var processBatchTasks = new List<Task<int>>();

            while (batchStartId <= maxId)
            {
                await _semaphore.WaitAsync();

                var batchEndId = batchStartId + _processedFieldMappingRepository.BatchSize - 1;
                processBatchTasks.Add(ProcessBatchAsync(batchStartId, batchEndId, taxa, fieldMappings, cancellationToken));
                batchStartId = batchEndId + 1;
            }
            await Task.WhenAll(processBatchTasks);

            return processBatchTasks.Sum(t => t.Result);
        }

        /// <summary>
        /// Process a batch of data
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="endId"></param>
        /// <param name="taxa"></param>
        /// <param name="fieldMappings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<int> ProcessBatchAsync(
        int startId,
        int endId,
        IDictionary<int, ProcessedTaxon> taxa,
        IDictionary<FieldMappingFieldId, IDictionary<object, int>> fieldMappings,
        IJobCancellationToken cancellationToken)
        {
            try
            {
                cancellationToken?.ThrowIfCancellationRequested();
                Logger.LogDebug($"Start fetching Artportalen batch ({ startId }-{ endId })");
                var batch = await _artportalenVerbatimRepository.GetBatchAsync(startId, endId);
                Logger.LogDebug($"Finish fetching Artportalen batch ({ startId }-{ endId })");

                Logger.LogDebug($"Start processing Artportalen batch ({ startId }-{ endId })");
                var sightings = batch.ToProcessed(taxa, fieldMappings);
                Logger.LogDebug($"Finish processing Artportalen batch ({ startId }-{ endId })");

                Logger.LogDebug($"Start storing Artportalen batch ({ startId }-{ endId })");
                var successCount = await CommitBatchAsync(sightings);
                Logger.LogDebug($"Finish storing Artportalen batch ({ startId }-{ endId })");
                
                return successCount;
            }
            catch (JobAbortedException e)
            {
                // Throw cancelation again to let function above handle it
                throw e;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Process Artportalen sightings from id: {startId} to id: {endId} failed");
            }
            finally
            {
                _semaphore.Release();
            }

            return 0;
        }

        /// <summary>
        /// Get field mappings for Artportalen.
        /// </summary>
        /// <param name="externalSystemId"></param>
        /// <param name="allFieldMappings"></param>
        /// <returns></returns>
        private IDictionary<FieldMappingFieldId, IDictionary<object, int>> GetFieldMappingsDictionary(
            ExternalSystemId externalSystemId,
            ICollection<FieldMapping> allFieldMappings)
        {
            var dic = new Dictionary<FieldMappingFieldId, IDictionary<object, int>>();

            foreach (var fieldMapping in allFieldMappings)
            {
                var fieldMappings = fieldMapping.ExternalSystemsMapping.FirstOrDefault(m => m.Id == externalSystemId);
                if (fieldMappings != null)
                {
                    string mappingKey = GetMappingKey(fieldMapping.Id);
                    var mapping = fieldMappings.Mappings.Single(m => m.Key == mappingKey);
                    var sosIdByValue = mapping.GetIdByValueDictionary();
                    dic.Add(fieldMapping.Id, sosIdByValue);
                }
            }

            return dic;
        }

        private string GetMappingKey(FieldMappingFieldId fieldMappingFieldId)
        {
            switch (fieldMappingFieldId)
            {
                case FieldMappingFieldId.Activity:
                case FieldMappingFieldId.Gender:
                case FieldMappingFieldId.County:
                case FieldMappingFieldId.Municipality:
                case FieldMappingFieldId.Parish:
                case FieldMappingFieldId.Province:
                case FieldMappingFieldId.LifeStage:
                case FieldMappingFieldId.Substrate:
                case FieldMappingFieldId.ValidationStatus:
                case FieldMappingFieldId.Biotope:
                case FieldMappingFieldId.Institution:
                case FieldMappingFieldId.Unit:
                    return "Id";
                default:
                    throw new ArgumentException($"No mapping exist for the field: {fieldMappingFieldId}");
            }
        }
    }
}
