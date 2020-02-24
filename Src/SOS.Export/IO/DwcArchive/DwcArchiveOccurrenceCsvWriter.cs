using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Mappings;
using SOS.Export.Models;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Search;

namespace SOS.Export.IO.DwcArchive
{
    public class DwcArchiveOccurrenceCsvWriter : Interfaces.IDwcArchiveOccurrenceCsvWriter
    {
        private readonly ILogger<DwcArchiveOccurrenceCsvWriter> _logger;
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;

        public DwcArchiveOccurrenceCsvWriter(
            IProcessedFieldMappingRepository processedFieldMappingRepository,
            ILogger<DwcArchiveOccurrenceCsvWriter> logger)
        {
            _processedFieldMappingRepository = processedFieldMappingRepository ?? throw new ArgumentNullException(nameof(processedFieldMappingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> CreateOccurrenceCsvFileAsync(
            AdvancedFilter filter,
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedSightingRepository processedSightingRepository,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var skip = 0;
                const int take = 1000000;
                var darwinCoreMap = new DarwinCoreDynamicMap(fieldDescriptions);
                var fieldMappings = await _processedFieldMappingRepository.GetFieldMappingsAsync();
                var valueMappingDictionaries = fieldMappings.ToDictionary(m => m.Id, m => m.CreateValueDictionary());
                var processedSightings = (await processedSightingRepository.GetChunkAsync(filter, skip, take)).ToArray();

                while (processedSightings.Any())
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    foreach (var processedSighting in processedSightings)
                    {
                        ResolveValue(processedSighting?.Location?.CountyId, valueMappingDictionaries[FieldMappingFieldId.County]);
                        ResolveValue(processedSighting?.Location?.MunicipalityId, valueMappingDictionaries[FieldMappingFieldId.Municipality]);
                        ResolveValue(processedSighting?.Location?.ParishId, valueMappingDictionaries[FieldMappingFieldId.Parish]);
                        ResolveValue(processedSighting?.Location?.ProvinceId, valueMappingDictionaries[FieldMappingFieldId.Province]);
                    }

                    await WriteOccurrenceCsvAsync(stream, processedSightings.ToDarwinCore(), darwinCoreMap);
                    skip += take;
                    processedSightings = (await processedSightingRepository.GetChunkAsync(filter, skip, take)).ToArray();
                }

                return true;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation($"{nameof(CreateOccurrenceCsvFileAsync)} was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create occurrence CSV file.");
                return false;
            }
        }

        private void ResolveValue(
            ProcessedFieldMapValue fieldMapValue,
            Dictionary<int, string> valueById)
        {
            if (fieldMapValue == null) return;
            if (fieldMapValue.Id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId
                && valueById.TryGetValue(fieldMapValue.Id, out var translatedValue))
            {
                fieldMapValue.Value = translatedValue;
            }
        }
        private async Task WriteOccurrenceCsvAsync<T>(Stream stream, IEnumerable<T> records, ClassMap<T> map)
        {
            if (!records?.Any() ?? true)
            {
                return;
            }

            await using var streamWriter = new StreamWriter(stream, null, -1, false);
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = "\t", // tab
                Encoding = System.Text.Encoding.UTF8
            };
            using var csv = new CsvWriter(streamWriter, csvConfig);

            csv.Configuration.RegisterClassMap(map);
            csv.WriteRecords(records);
        }
    }
}
