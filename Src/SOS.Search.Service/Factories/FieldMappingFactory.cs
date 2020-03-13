using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Search.Service.Factories.Interfaces;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Factories
{
    /// <summary>
    /// Field mapping factory.
    /// </summary>
    public class FieldMappingFactory : IFieldMappingFactory
    {
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;
        private readonly ILogger<FieldMappingFactory> _logger;
        private Dictionary<FieldMappingFieldId, Dictionary<int, Dictionary<string, string>>> _translationDictionary;
        private Dictionary<FieldMappingFieldId, Dictionary<int, string>> _nonLocalizedTranslationDictionary;
        static readonly object InitLock = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedFieldMappingRepository"></param>
        /// <param name="logger"></param>
        public FieldMappingFactory(
            IProcessedFieldMappingRepository processedFieldMappingRepository,
            ILogger<FieldMappingFactory> logger)
        {
            _processedFieldMappingRepository = processedFieldMappingRepository ??
                                             throw new ArgumentNullException(nameof(processedFieldMappingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<Dictionary<FieldMappingFieldId, Dictionary<int, Dictionary<string, string>>>> CreateLocalizedTranslationDictionaryAsync()
        {
            var dic = new Dictionary<FieldMappingFieldId, Dictionary<int, Dictionary<string, string>>>();
            var fieldMappings = await _processedFieldMappingRepository.GetFieldMappingsAsync();
            foreach (var fieldMapping in fieldMappings.Where(m => m.Localized))
            {
                var fieldMappingFieldId = (FieldMappingFieldId) fieldMapping.Id;
                dic.Add(fieldMappingFieldId, new Dictionary<int, Dictionary<string, string>>());
                foreach (var fieldMappingValue in fieldMapping.Values)
                {
                    dic[fieldMappingFieldId].Add(fieldMappingValue.Id, new Dictionary<string, string>());
                    foreach (var fieldMappingTranslation in fieldMappingValue.Translations)
                    {
                        dic[fieldMappingFieldId][fieldMappingValue.Id].Add(fieldMappingTranslation.CultureCode, fieldMappingTranslation.Value);
                    }
                }
            }

            return dic;
        }

        private async Task<Dictionary<FieldMappingFieldId, Dictionary<int, string>>> CreateNonLocalizedTranslationDictionaryAsync()
        {
            var dic = new Dictionary<FieldMappingFieldId, Dictionary<int, string>>();
            var fieldMappings = await _processedFieldMappingRepository.GetFieldMappingsAsync();
            foreach (var fieldMapping in fieldMappings.Where(m => !m.Localized))
            {
                var fieldMappingFieldId = (FieldMappingFieldId)fieldMapping.Id;
                dic.Add(fieldMappingFieldId, new Dictionary<int, string>());
                foreach (var fieldMappingValue in fieldMapping.Values)
                {
                    dic[fieldMappingFieldId].Add(fieldMappingValue.Id, fieldMappingValue.Value);
                }
            }

            return dic;
        }

        /// <summary>
        /// A dictionary with translations grouped by the following properties in order:
        /// FieldMappingFieldId, SosId, CultureCode.
        /// </summary>
        private Dictionary<FieldMappingFieldId, Dictionary<int, Dictionary<string, string>>> TranslationDictionary
        {
            get
            {
                if (_translationDictionary == null)
                {
                    lock (InitLock)
                    {
                        if (_translationDictionary == null)
                        {
                            _translationDictionary = CreateLocalizedTranslationDictionaryAsync().Result;
                        }
                    }
                }

                return _translationDictionary;
            }
        }

        private Dictionary<FieldMappingFieldId, Dictionary<int, string>> NonLocalizedTranslationDictionary
        {
            get
            {
                if (_nonLocalizedTranslationDictionary == null)
                {
                    lock (InitLock)
                    {
                        if (_nonLocalizedTranslationDictionary == null)
                        {
                            _nonLocalizedTranslationDictionary = CreateNonLocalizedTranslationDictionaryAsync().Result;
                        }
                    }
                }

                return _nonLocalizedTranslationDictionary;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<FieldMapping>> GetFieldMappingsAsync()
        {
            return await _processedFieldMappingRepository.GetFieldMappingsAsync();
        }

        /// <inheritdoc />
        public bool TryGetTranslatedValue(FieldMappingFieldId fieldId, string cultureCode, int sosId, out string translatedValue)
        {
            if (TranslationDictionary[fieldId].TryGetValue(sosId, out Dictionary<string, string> translationByCultureCode))
            {
                translatedValue = translationByCultureCode[cultureCode];
                return true;
            }
            else
            {
                translatedValue = null;
                return false;
            }
        }

        /// <inheritdoc />
        public bool TryGetValue(FieldMappingFieldId fieldId, int sosId, out string translatedValue)
        {
            return NonLocalizedTranslationDictionary[fieldId].TryGetValue(sosId, out translatedValue);
        }
    }
}