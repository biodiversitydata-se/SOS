﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Shared;
using SOS.Process.Extensions;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Factories
{
    /// <summary>
    /// Process factory class
    /// </summary>
    public class SpeciesPortalProcessFactory : ProcessBaseFactory<SpeciesPortalProcessFactory>, Interfaces.ISpeciesPortalProcessFactory
    {
        private readonly ISpeciesPortalVerbatimRepository _speciesPortalVerbatimRepository;
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="speciesPortalVerbatimRepository"></param>
        /// <param name="processedSightingRepository"></param>
        /// <param name="processedFieldMappingRepository"></param>
        /// <param name="logger"></param>
        public SpeciesPortalProcessFactory(
            ISpeciesPortalVerbatimRepository speciesPortalVerbatimRepository,
            IProcessedSightingRepository processedSightingRepository,
            IProcessedFieldMappingRepository processedFieldMappingRepository,
            ILogger<SpeciesPortalProcessFactory> logger) : base(processedSightingRepository, logger)
        {
            _speciesPortalVerbatimRepository = speciesPortalVerbatimRepository ?? throw new ArgumentNullException(nameof(speciesPortalVerbatimRepository));
            _processedFieldMappingRepository = processedFieldMappingRepository ?? throw new ArgumentNullException(nameof(processedFieldMappingRepository));
        }

        /// <inheritdoc />
        public async Task<RunInfo> ProcessAsync(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var runInfo = new RunInfo(DataProvider.KUL)
            {
                Start = DateTime.Now
            };

            try
            {
                var allFieldMappings = await _processedFieldMappingRepository.GetFieldMappingsAsync();
                Logger.LogDebug("Start Processing Species Portal Verbatim");
                var fieldMappings = GetFieldMappingsDictionary(VerbatimDataProviderTypeId.Artportalen, allFieldMappings.ToArray());

                Logger.LogDebug("Start deleting Species Portal data");
                if (!await ProcessRepository.DeleteProviderDataAsync(DataProvider.Artdatabanken))
                {
                    Logger.LogError("Failed to delete Species Portal data");

                    runInfo.End = DateTime.Now;
                    runInfo.Status = RunStatus.Failed;
                    return runInfo;
                }
                Logger.LogDebug("Finish deleting Species Portal data");

                Logger.LogDebug("Start getting first Species Portal batch");
                var verbatim = await _speciesPortalVerbatimRepository.GetBatchAsync(0);
                Logger.LogDebug("Finish getting first Species Portal batch");

                if (!verbatim.Any())
                {
                    Logger.LogError("No verbatim data to process");
                    runInfo.End = DateTime.Now;
                    runInfo.Status = RunStatus.Failed;
                    return runInfo;
                }

                var successCount = 0;
                var verbatimCount = 0;
                var count = verbatim.Count();
                while (count != 0)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    Logger.LogDebug("Start processing Species Portal batch");
                    var processedSightings = verbatim.ToProcessed(taxa, fieldMappings).ToArray();
                    Logger.LogDebug("Finish processing Species Portal batch");

                    Logger.LogDebug("Start adding Species Portal batch to db");
                    successCount += await ProcessRepository.AddManyAsync(processedSightings);
                    Logger.LogDebug("Finish adding Species Portal batch to db");

                    verbatimCount += count;
                    Logger.LogInformation($"Species Portal observations being processed, totalCount: {verbatimCount}");

                    Logger.LogDebug("Start getting next Species Portal batch");
                    verbatim = await _speciesPortalVerbatimRepository.GetBatchAsync(verbatim.Last().Id);
                    Logger.LogDebug("Finish getting next Species Portal batch");

                    count = verbatim.Count();
                }
                Logger.LogDebug("Finish getting Species Portal data");

                runInfo.Count = successCount;
                runInfo.End = DateTime.Now;
                runInfo.Status = RunStatus.Success;
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("Species Portal observation processing was canceled.");
                runInfo.End = DateTime.Now;
                runInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process sightings");
                runInfo.End = DateTime.Now;
                runInfo.Status = RunStatus.Failed;
            }

            return runInfo;
        }

        /// <summary>
        /// Get field mappings for Artportalen.
        /// </summary>
        /// <param name="dataProviderTypeId"></param>
        /// <param name="allFieldMappings"></param>
        /// <returns></returns>
        private IDictionary<FieldMappingFieldId, IDictionary<object, int>> GetFieldMappingsDictionary(
            VerbatimDataProviderTypeId dataProviderTypeId,
            ICollection<FieldMapping> allFieldMappings)
        {
            var dic = new Dictionary<FieldMappingFieldId, IDictionary<object, int>>();

            foreach (var fieldMapping in allFieldMappings)
            {
                var fieldMappings = fieldMapping.ExternalSystemsMapping.FirstOrDefault(m => m.Id == (int)dataProviderTypeId);
                if (fieldMappings != null)
                {
                    string mappingKey = GetMappingKey(fieldMapping.FieldMappingFieldId);
                    var mapping = fieldMappings.Mappings.Single(m => m.Key == mappingKey);
                    var sosIdByValue = mapping.GetIdByValueDictionary();
                    dic.Add((FieldMappingFieldId)fieldMapping.FieldMappingFieldId, sosIdByValue);
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
                    return "Id";
                default:
                    throw new ArgumentException($"No mapping exist for the field: {fieldMappingFieldId}");
            }
        }
    }
}
