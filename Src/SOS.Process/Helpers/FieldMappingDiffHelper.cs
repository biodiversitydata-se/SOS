using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Helpers
{
    public class FieldMappingDiffHelper : IFieldMappingDiffHelper
    {
        private readonly IFieldMappingVerbatimRepository _fieldMappingVerbatimRepository;
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;

        public FieldMappingDiffHelper(
            IFieldMappingVerbatimRepository fieldMappingVerbatimRepository,
            IProcessedFieldMappingRepository processedFieldMappingRepository)
        {
            _fieldMappingVerbatimRepository = fieldMappingVerbatimRepository ??
                                              throw new ArgumentNullException(nameof(_fieldMappingVerbatimRepository));
            _processedFieldMappingRepository = processedFieldMappingRepository ??
                                               throw new ArgumentNullException(nameof(processedFieldMappingRepository));
        }

        /// <summary>
        ///     Get diff between generated, verbatim and processed field mappings.
        /// </summary>
        /// <param name="generatedFieldMappings"></param>
        /// <returns></returns>
        public async Task<byte[]> CreateDiffZipFile(IEnumerable<FieldMapping> generatedFieldMappings)
        {
            var processedFieldMappings = await _processedFieldMappingRepository.GetAllAsync();
            var verbatimFieldMappings = await _fieldMappingVerbatimRepository.GetAllAsync();
            var generatedFieldMappingsJtoken = JToken.FromObject(generatedFieldMappings);
            var verbatimFieldMappingsJtoken = JToken.FromObject(verbatimFieldMappings);
            var processedFieldMappingsJtoken = JToken.FromObject(processedFieldMappings);
            var serializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            var generatedFieldMappingsJson =
                JsonConvert.SerializeObject(generatedFieldMappings, Formatting.Indented, serializerSettings);
            var verbatimFieldMappingsJson =
                JsonConvert.SerializeObject(verbatimFieldMappings, Formatting.Indented, serializerSettings);
            var processedFieldMappingsJson =
                JsonConvert.SerializeObject(processedFieldMappings, Formatting.Indented, serializerSettings);
            var generatedFile = Encoding.UTF8.GetBytes(generatedFieldMappingsJson);
            var verbatimFile = Encoding.UTF8.GetBytes(verbatimFieldMappingsJson);
            var processedFile = Encoding.UTF8.GetBytes(processedFieldMappingsJson);
            var zipFile = CreateZipFile(new[]
            {
                (Filename: "1.GeneratedFieldMappings.json", Bytes: generatedFile),
                (Filename: "2.VerbatimFieldMappings.json", Bytes: verbatimFile),
                (Filename: "3.ProcessedFieldMappings.json", Bytes: processedFile),
                CreateFieldMappingDiffResult(generatedFieldMappingsJtoken, verbatimFieldMappingsJtoken,
                    "JsonDiffPatch - GeneratedComparedToVerbatim"),
                CreateFieldMappingDiffResult(verbatimFieldMappingsJtoken, processedFieldMappingsJtoken,
                    "JsonDiffPatch - VerbatimComparedToProcessed"),
                CreateFieldMappingDiffResult(generatedFieldMappingsJtoken, processedFieldMappingsJtoken,
                    "JsonDiffPatch - GeneratedComparedToProcessed")
            });

            return zipFile;
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