using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Sighting;
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
    public class SpeciesPortalProcessFactory : DataProviderProcessorBase<SpeciesPortalProcessFactory>, Interfaces.ISpeciesPortalProcessFactory
    {
        private readonly ISpeciesPortalVerbatimRepository _speciesPortalVerbatimRepository;
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;
        public override DataProvider DataProvider => DataProvider.SpeciesPortal;

        /// <summary>
        /// Constructorro
        /// </summary>
        /// <param name="speciesPortalVerbatimRepository"></param>
        /// <param name="processedSightingRepository"></param>
        /// <param name="processedFieldMappingRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="logger"></param>
        public SpeciesPortalProcessFactory(
            ISpeciesPortalVerbatimRepository speciesPortalVerbatimRepository,
            IProcessedSightingRepository processedSightingRepository,
            IProcessedFieldMappingRepository processedFieldMappingRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            ILogger<SpeciesPortalProcessFactory> logger) : base(processedSightingRepository, fieldMappingResolverHelper, logger)
        {
            _speciesPortalVerbatimRepository = speciesPortalVerbatimRepository ?? throw new ArgumentNullException(nameof(speciesPortalVerbatimRepository));
            _processedFieldMappingRepository = processedFieldMappingRepository ?? throw new ArgumentNullException(nameof(processedFieldMappingRepository));
        }

        protected override async Task<int> ProcessObservations(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            ICollection<ProcessedSighting> sightings = new List<ProcessedSighting>();
            var allFieldMappings = await _processedFieldMappingRepository.GetFieldMappingsAsync();
            var fieldMappings = GetFieldMappingsDictionary(ExternalSystemId.Artportalen, allFieldMappings.ToArray());

            // Process and commit in batches.
            using var cursor = await _speciesPortalVerbatimRepository.GetAllAsync();
            await cursor.ForEachAsync(c =>
            {
                sightings.Add(c.ToProcessed(taxa, fieldMappings));
                if (IsBatchFilledToLimit(sightings.Count))
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    verbatimCount += CommitBatch(sightings);
                    Logger.LogDebug($"Species Portal Sightings processed: {verbatimCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (sightings.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                verbatimCount += CommitBatch(sightings);
                Logger.LogDebug($"Species Portal Sightings processed: {verbatimCount}");
            }

            return verbatimCount;
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
                case FieldMappingFieldId.Organization:
                case FieldMappingFieldId.Unit:
                    return "Id";
                default:
                    throw new ArgumentException($"No mapping exist for the field: {fieldMappingFieldId}");
            }
        }
    }
}
