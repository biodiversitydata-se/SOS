using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Helpers
{
    public class AreaDiffHelper : Interfaces.IAreaDiffHelper
    {
        private readonly IAreaVerbatimRepository _areaVerbatimRepository;
        private readonly IProcessedAreaRepository _processedAreaRepository;

        public AreaDiffHelper(
            IAreaVerbatimRepository areaVerbatimRepository,
            IProcessedAreaRepository processedAreaRepository)
        {
            _areaVerbatimRepository = areaVerbatimRepository ?? throw new ArgumentNullException(nameof(_areaVerbatimRepository));
            _processedAreaRepository = processedAreaRepository ?? throw new ArgumentNullException(nameof(processedAreaRepository));
        }

        /// <summary>
        /// Get diff between generated, verbatim and processed field mappings.
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> CreateDiffZipFile(Area[] generatedAreas)
        {
            foreach (var generatedArea in generatedAreas)
            {
                generatedArea.Geometry = null;
            }

            var processedAreas = (await _processedAreaRepository.GetAreasAsync()).ToArray();
            foreach (var processedArea in processedAreas)
            {
                processedArea.Geometry = null;
            }
            var verbatimAreas = (await _areaVerbatimRepository.GetAllAsync()).ToArray();
            foreach (var verbatimArea in verbatimAreas)
            {
                verbatimArea.Geometry = null;
            }
            var generatedAreasJtoken = JToken.FromObject(generatedAreas);
            var verbatimAreasJtoken = JToken.FromObject(verbatimAreas);
            var processedAreasJtoken = JToken.FromObject(processedAreas);
            var serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var generatedFieldMappingsJson = JsonConvert.SerializeObject(generatedAreas, Formatting.Indented, serializerSettings);
            var verbatimFieldMappingsJson = JsonConvert.SerializeObject(verbatimAreas, Formatting.Indented, serializerSettings);
            var processedFieldMappingsJson = JsonConvert.SerializeObject(processedAreas, Formatting.Indented, serializerSettings);
            var generatedFile = Encoding.UTF8.GetBytes(generatedFieldMappingsJson);
            var verbatimFile = Encoding.UTF8.GetBytes(verbatimFieldMappingsJson);
            var processedFile = Encoding.UTF8.GetBytes(processedFieldMappingsJson);
            var zipFile = CreateZipFile(new[]
            {
                (Filename: "1.GeneratedAreas.json", Bytes: generatedFile),
                (Filename: "2.VerbatimAreas.json", Bytes: verbatimFile),
                (Filename: "3.ProcessedAreas.json", Bytes: processedFile),
                CreateFieldMappingDiffResult(generatedAreasJtoken, verbatimAreasJtoken, "JsonDiffPatch - GeneratedComparedToVerbatim"),
                CreateFieldMappingDiffResult(verbatimAreasJtoken, processedAreasJtoken, "JsonDiffPatch - VerbatimComparedToProcessed"),
                CreateFieldMappingDiffResult(generatedAreasJtoken, processedAreasJtoken, "JsonDiffPatch - GeneratedComparedToProcessed"),
            });

            return zipFile;
        }

        private (string Filename, byte[] Bytes) CreateFieldMappingDiffResult(JToken left, JToken right, string fileName)
        {
            JsonDiffPatch jdp = new JsonDiffPatch();
            JToken diff = jdp.Diff(left, right);
            var newFilename = diff == null ? $"{fileName} (No changes detected).json" : $"{fileName} (Changes detected).json";
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