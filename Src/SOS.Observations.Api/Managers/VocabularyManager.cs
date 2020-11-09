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
    public class VocabularyManager : IVocabularyManager
    {
        private readonly ILogger<VocabularyManager> _logger;
        private readonly IVocabularyRepository _processedVocabularyRepository;
        private Dictionary<VocabularyId, Dictionary<int, string>> _nonLocalizedTranslationDictionary;
        
        /// <summary>
        ///     A dictionary with translations grouped by the following properties in order:
        ///     FieldMappingFieldId, SosId, CultureCode.
        /// </summary>
        private Dictionary<VocabularyId, Dictionary<int, Dictionary<string, string>>> _translationDictionary;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processedVocabularyRepository"></param>
        /// <param name="logger"></param>
        public VocabularyManager(
            IVocabularyRepository processedVocabularyRepository,
            ILogger<VocabularyManager> logger)
        {
            _processedVocabularyRepository = processedVocabularyRepository ??
                                               throw new ArgumentNullException(nameof(processedVocabularyRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Task.Run(InitializeAsync).Wait();
        }

        private async Task InitializeAsync()
        {
            _translationDictionary = await CreateLocalizedTranslationDictionaryAsync();
            _nonLocalizedTranslationDictionary = await CreateNonLocalizedTranslationDictionaryAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Vocabulary>> GetVocabulariesAsync()
        {
            return await _processedVocabularyRepository.GetAllAsync();
        }

        /// <inheritdoc />
        public bool TryGetTranslatedValue(VocabularyId fieldId, string cultureCode, int sosId,
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
        public bool TryGetValue(VocabularyId fieldId, int sosId, out string translatedValue)
        {
            return _nonLocalizedTranslationDictionary[fieldId].TryGetValue(sosId, out translatedValue);
        }

        private async Task<Dictionary<VocabularyId, Dictionary<int, Dictionary<string, string>>>>
            CreateLocalizedTranslationDictionaryAsync()
        {
            var dic = new Dictionary<VocabularyId, Dictionary<int, Dictionary<string, string>>>();
            var processedVocabularies = await _processedVocabularyRepository.GetAllAsync();
            foreach (var vocabularity in processedVocabularies.Where(m => m.Localized))
            {
                var fieldMappingFieldId = vocabularity.Id;
                dic.Add(fieldMappingFieldId, new Dictionary<int, Dictionary<string, string>>());
                foreach (var fieldMappingValue in vocabularity.Values)
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

        private async Task<Dictionary<VocabularyId, Dictionary<int, string>>>
            CreateNonLocalizedTranslationDictionaryAsync()
        {
            var dic = new Dictionary<VocabularyId, Dictionary<int, string>>();
            var processedVocabularies = await _processedVocabularyRepository.GetAllAsync();
            foreach (var vocabularity in processedVocabularies.Where(m => !m.Localized))
            {
                var fieldMappingFieldId = vocabularity.Id;
                dic.Add(fieldMappingFieldId, new Dictionary<int, string>());
                foreach (var fieldMappingValue in vocabularity.Values)
                {
                    dic[fieldMappingFieldId].Add(fieldMappingValue.Id, fieldMappingValue.Value);
                }
            }

            return dic;
        }
    }
}