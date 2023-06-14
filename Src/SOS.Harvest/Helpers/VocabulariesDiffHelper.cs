using System.Reflection;
using System.Text;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOS.Harvest.Helpers.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Harvest.Helpers
{
    public class VocabulariesDiffHelper : IVocabulariesDiffHelper
    {
        private readonly IVocabularyRepository _processedVocabularyRepository;

        public VocabulariesDiffHelper(
            IVocabularyRepository processedVocabularyRepository)
        {
            _processedVocabularyRepository = processedVocabularyRepository ??
                                               throw new ArgumentNullException(nameof(processedVocabularyRepository));
        }

        /// <summary>
        ///     Get diff between generated, and processed vocabularies.
        /// </summary>
        /// <param name="generatedVocabularies"></param>
        /// <returns></returns>
        public async Task<byte[]> CreateDiffZipFile(IEnumerable<Vocabulary> generatedVocabularies)
        {
            var jsonFilesVocabularies = GetJsonFilesVocabularies();
            var processedVocabularies = await _processedVocabularyRepository.GetAllAsync();
            generatedVocabularies = SortVocabularies(generatedVocabularies);
            jsonFilesVocabularies = SortVocabularies(jsonFilesVocabularies);
            processedVocabularies = SortVocabularies(processedVocabularies);
            var generatedVocabulariesJtoken = JToken.FromObject(generatedVocabularies);
            var jsonFilesVocabulariesJtoken = JToken.FromObject(jsonFilesVocabularies);
            var processedVocabulariesJtoken = JToken.FromObject(processedVocabularies);
            var serializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            var generatedVocabulariesJson =
                JsonConvert.SerializeObject(generatedVocabularies, Formatting.Indented, serializerSettings);
            var jsonFilesVocabulariesJson =
                JsonConvert.SerializeObject(jsonFilesVocabularies, Formatting.Indented, serializerSettings);
            var processedVocabulariesJson =
                JsonConvert.SerializeObject(processedVocabularies, Formatting.Indented, serializerSettings);
            var generatedFile = Encoding.UTF8.GetBytes(generatedVocabulariesJson);
            var jsonFilesFile = Encoding.UTF8.GetBytes(jsonFilesVocabulariesJson);
            var processedFile = Encoding.UTF8.GetBytes(processedVocabulariesJson);
            var zipFile = ZipFileHelper.CreateZipFile(new[]
            {
                (Filename: "1.GeneratedVocabularies.json", Bytes: generatedFile),
                (Filename: "2.JsonFilesVocabularies.json", Bytes: jsonFilesFile),
                (Filename: "3.ProcessedVocabularies.json", Bytes: processedFile),
                CreateVocabulariesDiffResult(generatedVocabulariesJtoken, jsonFilesVocabulariesJtoken,
                    "JsonDiffPatch - GeneratedComparedToJsonFiles"),
                CreateVocabulariesDiffResult(jsonFilesVocabulariesJtoken, processedVocabulariesJtoken,
                    "JsonDiffPatch - JsonFilesComparedToProcessed"),
                CreateVocabulariesDiffResult(generatedVocabulariesJtoken, processedVocabulariesJtoken,
                    "JsonDiffPatch - GeneratedComparedToProcessed")
            });

            return zipFile;
        }

        private List<Vocabulary> SortVocabularies(IEnumerable<Vocabulary> vocabularies)
        {
            var sortedVocabularies = new List<Vocabulary>();
            foreach (var vocabulary in vocabularies.OrderBy(vocabulary => vocabulary.Id))
            {
                vocabulary.Values = vocabulary.Values.OrderBy(m => m.Id).ToList();
                foreach (var externalSystemMapping in vocabulary.ExternalSystemsMapping ?? Enumerable.Empty<ExternalSystemMapping>())
                {
                    foreach (var mapping in externalSystemMapping?.Mappings ?? Enumerable.Empty<ExternalSystemMappingField>())
                    {
                        mapping.Values = mapping.Values.OrderBy(m => m.SosId).ToList();
                    }
                }

                vocabulary.ExternalSystemsMapping = vocabulary.ExternalSystemsMapping?.OrderBy(m => m.Id).ToList();
                sortedVocabularies.Add(vocabulary);
            }

            return sortedVocabularies;
        }

        private List<Vocabulary> GetJsonFilesVocabularies()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath!, @"Resources\Vocabularies\");
            var jsonFilesVocabularies = new List<Vocabulary>();
            foreach (var fileName in Directory.GetFiles(filePath))
            {
                var vocabulary = CreateVocabularyFromJsonFile(fileName);

                if (vocabulary == null)
                {
                    continue;
                }
                jsonFilesVocabularies.Add(vocabulary);
            }

            return jsonFilesVocabularies;
        }

        private Vocabulary? CreateVocabularyFromJsonFile(string filename)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath!, filename);
            var str = File.ReadAllText(filePath, Encoding.UTF8);
            var vocabulary = JsonConvert.DeserializeObject<Vocabulary>(str);
            ValidateVocabulary(vocabulary);
            return vocabulary;
        }

        private void ValidateVocabulary(Vocabulary? vocabulary)
        {
            if (vocabulary == null)
            {
                return;
            }

            var externalSystemMappingFields = vocabulary?.ExternalSystemsMapping?.SelectMany(mapping => mapping.Mappings);
            if (externalSystemMappingFields == null) return;

            foreach (var externalSystemMappingField in externalSystemMappingFields)
            {
                // Check if there exists duplicate synonyms.
                if (externalSystemMappingField.Values.First().Value is string)
                {
                    if (externalSystemMappingField.Values.Select(m => m.Value.ToString()?.ToLower()).HasDuplicates())
                    {
                        throw new Exception($"Duplicate mappings exist for field \"{vocabulary?.Name}\"");
                    }
                }
                else
                {
                    if (externalSystemMappingField.Values.Select(m => m.Value).HasDuplicates())
                    {
                        throw new Exception($"Duplicate mappings exist for field \"{vocabulary?.Name}\"");
                    }
                }
            }
        }

        private (string Filename, byte[] Bytes) CreateVocabulariesDiffResult(JToken left, JToken right, string fileName)
        {
            var jdp = new JsonDiffPatch();
            var diff = jdp.Diff(left, right);
            var newFilename = diff == null
                ? $"{fileName} (No changes detected).json"
                : $"{fileName} (Changes detected).json";
            var diffBytes = Encoding.UTF8.GetBytes(diff == null ? "" : diff.ToString());
            return (Filename: newFilename, Bytes: diffBytes);
        }
    }
}