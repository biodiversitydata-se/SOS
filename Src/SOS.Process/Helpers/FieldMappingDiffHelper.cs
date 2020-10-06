using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Process.Helpers.Interfaces;

namespace SOS.Process.Helpers
{
    public class FieldMappingDiffHelper : IFieldMappingDiffHelper
    {
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;

        public FieldMappingDiffHelper(
            IProcessedFieldMappingRepository processedFieldMappingRepository)
        {
            _processedFieldMappingRepository = processedFieldMappingRepository ??
                                               throw new ArgumentNullException(nameof(processedFieldMappingRepository));
        }

        /// <summary>
        ///     Get diff between generated, and processed field mappings.
        /// </summary>
        /// <param name="generatedFieldMappings"></param>
        /// <returns></returns>
        public async Task<byte[]> CreateDiffZipFile(IEnumerable<FieldMapping> generatedFieldMappings)
        {
            var jsonFilesFieldMappings = GetJsonFilesFieldMappings();
            var processedFieldMappings = await _processedFieldMappingRepository.GetAllAsync();
            var generatedFieldMappingsJtoken = JToken.FromObject(generatedFieldMappings);
            var jsonFilesFieldMappingsJtoken = JToken.FromObject(jsonFilesFieldMappings);
            var processedFieldMappingsJtoken = JToken.FromObject(processedFieldMappings);
            var serializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            var generatedFieldMappingsJson =
                JsonConvert.SerializeObject(generatedFieldMappings, Formatting.Indented, serializerSettings);
            var jsonFilesFieldMappingsJson =
                JsonConvert.SerializeObject(jsonFilesFieldMappings, Formatting.Indented, serializerSettings);
            var processedFieldMappingsJson =
                JsonConvert.SerializeObject(processedFieldMappings, Formatting.Indented, serializerSettings);
            var generatedFile = Encoding.UTF8.GetBytes(generatedFieldMappingsJson);
            var jsonFilesFile = Encoding.UTF8.GetBytes(jsonFilesFieldMappingsJson);
            var processedFile = Encoding.UTF8.GetBytes(processedFieldMappingsJson);
            var zipFile = CreateZipFile(new[]
            {
                (Filename: "1.GeneratedFieldMappings.json", Bytes: generatedFile),
                (Filename: "2.JsonFilesFieldMappings.json", Bytes: jsonFilesFile),
                (Filename: "3.ProcessedFieldMappings.json", Bytes: processedFile),
                CreateFieldMappingDiffResult(generatedFieldMappingsJtoken, jsonFilesFieldMappingsJtoken,
                    "JsonDiffPatch - GeneratedComparedToJsonFiles"),
                CreateFieldMappingDiffResult(jsonFilesFieldMappingsJtoken, processedFieldMappingsJtoken,
                    "JsonDiffPatch - JsonFilesComparedToProcessed"),
                CreateFieldMappingDiffResult(generatedFieldMappingsJtoken, processedFieldMappingsJtoken,
                    "JsonDiffPatch - GeneratedComparedToProcessed")
            });

            return zipFile;
        }

        private List<FieldMapping> GetJsonFilesFieldMappings()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, @"Resources\FieldMappings\");
            var jsonFilesFieldMappings = new List<FieldMapping>();
            foreach (var fileName in Directory.GetFiles(filePath))
            {
                var fieldMapping = CreateFieldMappingFromJsonFile(fileName);
                jsonFilesFieldMappings.Add(fieldMapping);
            }

            return jsonFilesFieldMappings;
        }

        private FieldMapping CreateFieldMappingFromJsonFile(string filename)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, filename);
            var str = File.ReadAllText(filePath, Encoding.UTF8);
            var fieldMapping = JsonConvert.DeserializeObject<FieldMapping>(str);
            ValidateFieldMapping(fieldMapping);
            return fieldMapping;
        }

        private void ValidateFieldMapping(FieldMapping fieldMapping)
        {
            var externalSystemMappingFields = fieldMapping?.ExternalSystemsMapping?.SelectMany(mapping => mapping.Mappings);
            if (externalSystemMappingFields == null) return;

            foreach (var externalSystemMappingField in externalSystemMappingFields)
            {
                // Check if there exists duplicate synonyms.
                if (externalSystemMappingField.Values.First().Value is string)
                {
                    if (externalSystemMappingField.Values.Select(m => m.Value.ToString()?.ToLower()).HasDuplicates())
                    {
                        throw new Exception($"Duplicate mappings exist for field \"{fieldMapping.Name}\"");
                    }
                }
                else
                {
                    if (externalSystemMappingField.Values.Select(m => m.Value).HasDuplicates())
                    {
                        throw new Exception($"Duplicate mappings exist for field \"{fieldMapping.Name}\"");
                    }
                }
            }
        }

        private (string Filename, byte[] Bytes) CreateFieldMappingDiffResult(JToken left, JToken right, string fileName)
        {
            var jdp = new JsonDiffPatch();
            var diff = jdp.Diff(left, right);
            var newFilename = diff == null
                ? $"{fileName} (No changes detected).json"
                : $"{fileName} (Changes detected).json";
            var diffBytes = Encoding.UTF8.GetBytes(diff == null ? "" : diff.ToString());
            return (Filename: newFilename, Bytes: diffBytes);
        }

        private byte[] CreateZipFile(IEnumerable<(string Filename, byte[] Bytes)> files)
        {
            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    var zipEntry = archive.CreateEntry(file.Filename, CompressionLevel.Optimal);
                    using var zipStream = zipEntry.Open();
                    zipStream.Write(file.Bytes, 0, file.Bytes.Length);
                }
            }

            return ms.ToArray();
        }
    }
}