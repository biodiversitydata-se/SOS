using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Vocabulary manager.
    /// </summary>
    public class VocabularyManager : IVocabularyManager
    {
        private readonly ILogger<VocabularyManager> _logger;
        private readonly ICache<VocabularyId, Vocabulary> _vocabularyCache;
        private readonly ICache<int, ProjectInfo> _projectCache;
        private Dictionary<VocabularyId, Dictionary<int, string>> _nonLocalizedTranslationDictionary;
        
        /// <summary>
        ///     A dictionary with translations grouped by the following properties in order:
        ///     VocabularyId, SosId, CultureCode.
        /// </summary>
        private Dictionary<VocabularyId, Dictionary<int, Dictionary<string, string>>> _translationDictionary;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="vocabularyCache"></param>
        /// <param name="projectCache"></param>
        /// <param name="logger"></param>
        public VocabularyManager(
            ICache<VocabularyId, Vocabulary> vocabularyCache,
            ICache<int, ProjectInfo> projectCache,
            ILogger<VocabularyManager> logger)
        {
            _vocabularyCache = vocabularyCache ??
                               throw new ArgumentNullException(nameof(vocabularyCache));
            _projectCache = projectCache ?? throw new ArgumentNullException(nameof(projectCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Task.Run(InitializeAsync).Wait();
        }

        private async Task InitializeAsync()
        {
            _translationDictionary = await CreateLocalizedTranslationDictionaryAsync();
            _nonLocalizedTranslationDictionary = await CreateNonLocalizedTranslationDictionaryAsync();
        }

        public async Task<IEnumerable<ProjectInfo>> GetProjectsAsync()
        {
            return await _projectCache.GetAllAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Vocabulary>> GetVocabulariesAsync()
        {
            return await _vocabularyCache.GetAllAsync();
        }

        /// <inheritdoc />
        public bool TryGetTranslatedValue(VocabularyId vocabularyId, string cultureCode, int sosId,
            out string translatedValue)
        {
            if (_translationDictionary[vocabularyId].TryGetValue(sosId, out var translationByCultureCode))
            {
                translatedValue = translationByCultureCode[cultureCode];
                return true;
            }

            translatedValue = null;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetValue(VocabularyId vocabularyId, int sosId, out string translatedValue)
        {
            return _nonLocalizedTranslationDictionary[vocabularyId].TryGetValue(sosId, out translatedValue);
        }

        private async Task<Dictionary<VocabularyId, Dictionary<int, Dictionary<string, string>>>>
            CreateLocalizedTranslationDictionaryAsync()
        {
            var dic = new Dictionary<VocabularyId, Dictionary<int, Dictionary<string, string>>>();
            var processedVocabularies = await _vocabularyCache.GetAllAsync();
            foreach (var vocabularity in processedVocabularies.Where(m => m.Localized))
            {
                var vocabularyId = vocabularity.Id;
                dic.Add(vocabularyId, new Dictionary<int, Dictionary<string, string>>());
                foreach (var vocabularyValue in vocabularity.Values)
                {
                    dic[vocabularyId].Add(vocabularyValue.Id, new Dictionary<string, string>());
                    foreach (var translation in vocabularyValue.Translations)
                    {
                        dic[vocabularyId][vocabularyValue.Id].Add(translation.CultureCode,
                            translation.Value);
                    }
                }
            }

            return dic;
        }

        private async Task<Dictionary<VocabularyId, Dictionary<int, string>>>
            CreateNonLocalizedTranslationDictionaryAsync()
        {
            var dic = new Dictionary<VocabularyId, Dictionary<int, string>>();
            var processedVocabularies = await _vocabularyCache.GetAllAsync();
            foreach (var vocabularity in processedVocabularies.Where(m => !m.Localized))
            {
                var vocabularyId = vocabularity.Id;
                dic.Add(vocabularyId, new Dictionary<int, string>());
                foreach (var vocabularyValue in vocabularity.Values)
                {
                    dic[vocabularyId].Add(vocabularyValue.Id, vocabularyValue.Value);
                }
            }

            return dic;
        }

        public async Task<byte[]> GetVocabulariesZipFileAsync(IEnumerable<VocabularyId> vocabularyIds)
        {
            var vocabularies = await GetVocabulariesAsync();
            var vocabularyIdsSet = new HashSet<VocabularyId>(vocabularyIds);
            var zipFile = CreateZipFile(vocabularies.Where(m => vocabularyIdsSet.Contains(m.Id)));
            return zipFile;
        }

        private byte[] CreateZipFile(IEnumerable<Vocabulary> vocabularies)
        {
            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var vocabulary in vocabularies)
                {
                    var bytes = SerializeToJsonArray(vocabulary,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }, Formatting.Indented);
                    var zipEntry = archive.CreateEntry($"{vocabulary.Id}.json", CompressionLevel.Optimal);
                    using var zipStream = zipEntry.Open();
                    zipStream.Write(bytes, 0, bytes.Length);
                }
            }

            return ms.ToArray();
        }

        private byte[] SerializeToJsonArray(object value, JsonSerializerSettings jsonSerializerSettings,
            Formatting formatting)
        {
            var result = JsonConvert.SerializeObject(value, formatting, jsonSerializerSettings);
            return Encoding.UTF8.GetBytes(result);
        }
    }
}