using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Field mapping manager.
    /// </summary>
    public class FieldMappingManager : IFieldMappingManager
    {
        private readonly ILogger<FieldMappingManager> _logger;
        private readonly IFieldMappingRepository _processedFieldMappingRepository;
        private Dictionary<FieldMappingFieldId, Dictionary<int, string>> _nonLocalizedTranslationDictionary;
        
        /// <summary>
        ///     A dictionary with translations grouped by the following properties in order:
        ///     FieldMappingFieldId, SosId, CultureCode.
        /// </summary>
        private Dictionary<FieldMappingFieldId, Dictionary<int, Dictionary<string, string>>> _translationDictionary;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processedFieldMappingRepository"></param>
        /// <param name="logger"></param>
        public FieldMappingManager(
            IFieldMappingRepository processedFieldMappingRepository,
            ILogger<FieldMappingManager> logger)
        {
            _processedFieldMappingRepository = processedFieldMappingRepository ??
                                               throw new ArgumentNullException(nameof(processedFieldMappingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Task.Run(InitializeAsync).Wait();
        }

        private async Task InitializeAsync()
        {
            _translationDictionary = await CreateLocalizedTranslationDictionaryAsync();
            _nonLocalizedTranslationDictionary = await CreateNonLocalizedTranslationDictionaryAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<FieldMapping>> GetFieldMappingsAsync()
        {
            return await _processedFieldMappingRepository.GetAllAsync();
        }

        /// <inheritdoc />
        public bool TryGetTranslatedValue(FieldMappingFieldId fieldId, string cultureCode, int sosId,
            out string translatedValue)
        {
            if (_translationDictionary[fieldId].TryGetValue(sosId, out var translationByCultureCode))
            {
                translatedValue = translationByCultureCode[cultureCode];
                return true;
            }

            translatedValue = null;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetValue(FieldMappingFieldId fieldId, int sosId, out string translatedValue)
        {
            return _nonLocalizedTranslationDictionary[fieldId].TryGetValue(sosId, out translatedValue);
        }

        private async Task<Dictionary<FieldMappingFieldId, Dictionary<int, Dictionary<string, string>>>>
            CreateLocalizedTranslationDictionaryAsync()
        {
            var dic = new Dictionary<FieldMappingFieldId, Dictionary<int, Dictionary<string, string>>>();
            var fieldMappings = await _processedFieldMappingRepository.GetAllAsync();
            foreach (var fieldMapping in fieldMappings.Where(m => m.Localized))
            {
                var fieldMappingFieldId = fieldMapping.Id;
                dic.Add(fieldMappingFieldId, new Dictionary<int, Dictionary<string, string>>());
                foreach (var fieldMappingValue in fieldMapping.Values)
                {
                    dic[fieldMappingFieldId].Add(fieldMappingValue.Id, new Dictionary<string, string>());
                    foreach (var fieldMappingTranslation in fieldMappingValue.Translations)
                    {
                        dic[fieldMappingFieldId][fieldMappingValue.Id].Add(fieldMappingTranslation.CultureCode,
                            fieldMappingTranslation.Value);
                    }
                }
            }

            return dic;
        }

        private async Task<Dictionary<FieldMappingFieldId, Dictionary<int, string>>>
            CreateNonLocalizedTranslationDictionaryAsync()
        {
            var dic = new Dictionary<FieldMappingFieldId, Dictionary<int, string>>();
            var fieldMappings = await _processedFieldMappingRepository.GetAllAsync();
            foreach (var fieldMapping in fieldMappings.Where(m => !m.Localized))
            {
                var fieldMappingFieldId = fieldMapping.Id;
                dic.Add(fieldMappingFieldId, new Dictionary<int, string>());
                foreach (var fieldMappingValue in fieldMapping.Values)
                {
                    dic[fieldMappingFieldId].Add(fieldMappingValue.Id, fieldMappingValue.Value);
                }
            }

            return dic;
        }
    }
}