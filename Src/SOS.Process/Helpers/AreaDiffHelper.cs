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
        /// Get diff between generated, verbatim and processed areas.
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> CreateDiffZipFile(AreaBase[] generatedAreas)
        {
            // Get verbatim areas and set Geometry to null. It takes too long time and RAM to compare coordinates using JsonDiffPatch.
            var verbatimAreas = (await _areaVerbatimRepository.GetAllAreaBaseAsync()).ToArray();

            // Get processed areas and set Geometry to null. It takes too long time and RAM to compare coordinates using JsonDiffPatch.
            var processedAreas = (await _processedAreaRepository.GetAllAreaBaseAsync()).ToArray();

            var generatedAreasJtoken = JToken.FromObject(generatedAreas);
            var verbatimAreasJtoken = JToken.FromObject(verbatimAreas);
            var processedAreasJtoken = JToken.FromObject(processedAreas);
            var serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var generatedAreasJson = JsonConvert.SerializeObject(generatedAreas, Formatting.Indented, serializerSettings);
            var verbatimAreasJson = JsonConvert.SerializeObject(verbatimAreas, Formatting.Indented, serializerSettings);
            var processedAreasJson = JsonConvert.SerializeObject(processedAreas, Formatting.Indented, serializerSettings);
            var generatedFile = Encoding.UTF8.GetBytes(generatedAreasJson);
            var verbatimFile = Encoding.UTF8.GetBytes(verbatimAreasJson);
            var processedFile = Encoding.UTF8.GetBytes(processedAreasJson);
            var zipFile = CreateZipFile(new[]
            {
                (Filename: "1.GeneratedAreas.json", Bytes: generatedFile),
                (Filename: "2.VerbatimAreas.json", Bytes: verbatimFile),
                (Filename: "3.ProcessedAreas.json", Bytes: processedFile),
                CreateDiffResult(generatedAreasJtoken, verbatimAreasJtoken, "JsonDiffPatch - GeneratedComparedToVerbatim"),
                CreateDiffResult(verbatimAreasJtoken, processedAreasJtoken, "JsonDiffPatch - VerbatimComparedToProcessed"),
                CreateDiffResult(generatedAreasJtoken, processedAreasJtoken, "JsonDiffPatch - GeneratedComparedToProcessed"),
            });

            return zipFile;
        }

        private (string Filename, byte[] Bytes) CreateDiffResult(JToken left, JToken right, string fileName)
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