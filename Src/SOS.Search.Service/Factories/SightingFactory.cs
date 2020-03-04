using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Search;
using SOS.Search.Service.Enum;
using SOS.Search.Service.Factories.Interfaces;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class SightingFactory : Interfaces.ISightingFactory
    {
        private readonly IProcessedSightingRepository _processedSightingRepository;
        private readonly IFieldMappingFactory _fieldMappingFactory;
        private readonly ILogger<SightingFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedSightingRepository"></param>
        /// <param name="fieldMappingFactory"></param>
        /// <param name="logger"></param>
        public SightingFactory(
            IProcessedSightingRepository processedSightingRepository,
            IFieldMappingFactory fieldMappingFactory,
            ILogger<SightingFactory> logger)
        {
            _processedSightingRepository = processedSightingRepository ?? throw new ArgumentNullException(nameof(processedSightingRepository));
            _fieldMappingFactory = fieldMappingFactory ?? throw new ArgumentNullException(nameof(fieldMappingFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take)
        {
            try
            {
                var processedSightings = (await _processedSightingRepository.GetChunkAsync(filter, skip, take)).ToArray();
                ProcessLocalizedFieldMappings(filter, processedSightings);
                ProcessNonLocalizedFieldMappings(filter, processedSightings);
                return processedSightings;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get chunk of sightings");
                return null;
            }
        }

        private void ProcessNonLocalizedFieldMappings(SearchFilter filter, IEnumerable<object> processedSightings)
        {
            if (!filter.TranslateFieldMappedValues) return;

            if (filter.OutputFields == null || !filter.OutputFields.Any()) // ProcessedSighting objects is returned wen OutputFields is not used.
            {
                var sightings = processedSightings.Cast<ProcessedSighting>();
                foreach (var sighting in sightings)
                {
                    TranslateValue(sighting.Location?.CountyId, FieldMappingFieldId.County);
                    TranslateValue(sighting.Location?.MunicipalityId, FieldMappingFieldId.Municipality);
                    TranslateValue(sighting.Location?.ProvinceId, FieldMappingFieldId.Province);
                    TranslateValue(sighting.Location?.ParishId, FieldMappingFieldId.Parish);
                }
            }
            else // dynamic objects is returned when OutputFields is used
            {
                foreach (var sighting in processedSightings)
                {
                    if (sighting is IDictionary<string, object> obs && obs.ContainsKey("Occurrence"))
                    {
                        var location = obs["Location"] as IDictionary<string, object>;
                        TranslateValue(location, FieldMappingFieldId.County, nameof(ProcessedSighting.Location.CountyId));
                        TranslateValue(location, FieldMappingFieldId.Municipality, nameof(ProcessedSighting.Location.MunicipalityId));
                        TranslateValue(location, FieldMappingFieldId.Province, nameof(ProcessedSighting.Location.ProvinceId));
                        TranslateValue(location, FieldMappingFieldId.Parish, nameof(ProcessedSighting.Location.ParishId));
                    }
                }
            }
        }

        private void ProcessLocalizedFieldMappings(SearchFilter filter, IEnumerable<dynamic> processedSightings)
        {
            if (!filter.TranslateFieldMappedValues) return;
            string cultureCode = filter.TranslationCultureCode;
            if (filter.OutputFields == null || !filter.OutputFields.Any()) // ProcessedSighting objects is returned wen OutputFields is not used.
            {
                var sightings = processedSightings.Cast<ProcessedSighting>();
                ProcessFieldMappedReturnValues(sightings, cultureCode);
            }
            else // dynamic objects is returned when OutputFields is used
            {
                ProcessFieldMappedReturnValues(processedSightings, cultureCode);
            }
        }

        private void ProcessFieldMappedReturnValues(
            IEnumerable<ProcessedSighting> processedSightings,
            string cultureCode)
        {
            foreach (var sighting in processedSightings)
            {
                TranslateLocalizedValue(sighting.Occurrence?.ActivityId, FieldMappingFieldId.Activity, cultureCode);
                TranslateLocalizedValue(sighting.Occurrence?.GenderId, FieldMappingFieldId.Gender, cultureCode);
            }
        }


        private void ProcessFieldMappedReturnValues(
            IEnumerable<dynamic> processedSightings,
            string cultureCode)
        {
            try
            {
                foreach (var sighting in processedSightings)
                {
                    if (sighting is IDictionary<string, object> obs && obs.ContainsKey("Occurrence"))
                    {
                        var occurrence = obs["Occurrence"] as IDictionary<string, object>;
                        TranslateLocalizedValue(occurrence, FieldMappingFieldId.Activity, nameof(ProcessedSighting.Occurrence.ActivityId), cultureCode);
                        TranslateLocalizedValue(occurrence, FieldMappingFieldId.Gender, nameof(ProcessedSighting.Occurrence.GenderId), cultureCode);

                        var location = obs["Location"] as IDictionary<string, object>;
                        TranslateValue(location, FieldMappingFieldId.County, nameof(ProcessedSighting.Location.CountyId));
                        TranslateValue(location, FieldMappingFieldId.Municipality, nameof(ProcessedSighting.Location.MunicipalityId));
                        TranslateValue(location, FieldMappingFieldId.Province, nameof(ProcessedSighting.Location.ProvinceId));
                        TranslateValue(location, FieldMappingFieldId.Parish, nameof(ProcessedSighting.Location.ParishId));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void TranslateValue(
            IDictionary<string, object> sightingNode, 
            FieldMappingFieldId fieldMappingFieldId, 
            string fieldName)
        {
            if (sightingNode == null) return;

            if (sightingNode.ContainsKey(fieldName))
            {
                if (sightingNode[fieldName] is IDictionary<string, object> fieldNode && fieldNode.ContainsKey("Value") && fieldNode.ContainsKey("_id"))
                {
                    int id = (int)fieldNode["_id"];
                    if (id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId && _fieldMappingFactory.TryGetValue(fieldMappingFieldId, id, out var translatedValue))
                    {
                        fieldNode["Value"] = translatedValue;
                    }
                }
            }
        }
        private void TranslateValue(ProcessedFieldMapValue fieldMapValue, FieldMappingFieldId fieldMappingFieldId)
        {
            if (fieldMapValue == null) return;
            if (fieldMapValue.Id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId
                && _fieldMappingFactory.TryGetValue(fieldMappingFieldId, fieldMapValue.Id, out var translatedValue))
            {
                fieldMapValue.Value = translatedValue;
            }
        }

        private void TranslateLocalizedValue(
            ProcessedFieldMapValue fieldMapValue,
            FieldMappingFieldId fieldMappingFieldId,
            string cultureCode)
        {
            if (fieldMapValue == null) return;

            if (fieldMapValue.Id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId
                && _fieldMappingFactory.TryGetTranslatedValue(fieldMappingFieldId, cultureCode, fieldMapValue.Id, out var translatedValue))
            {
                fieldMapValue.Value = translatedValue;
            }
        }

        private void TranslateLocalizedValue(
            IDictionary<string, object> sightingNode, 
            FieldMappingFieldId fieldMappingFieldId, 
            string fieldName,
            string cultureCode)
        {
            if (sightingNode == null) return;

            if (sightingNode.ContainsKey(fieldName))
            {
                if (sightingNode[fieldName] is IDictionary<string, object> fieldNode && fieldNode.ContainsKey("Value") && fieldNode.ContainsKey("_id"))
                {
                    int id = (int)fieldNode["_id"];
                    if (id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId && _fieldMappingFactory.TryGetTranslatedValue(fieldMappingFieldId, cultureCode, id, out var translatedValue))
                    {
                        fieldNode["Value"] = translatedValue;
                    }
                }
            }
        }
    }
}