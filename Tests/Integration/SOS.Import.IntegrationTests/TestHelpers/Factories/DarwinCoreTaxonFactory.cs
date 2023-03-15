using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using SOS.Lib.Models.DarwinCore;
using SOS.TestHelpers.JsonConverters;

namespace SOS.Import.IntegrationTests.TestHelpers.Factories
{
    public static class DarwinCoreTaxonFactory
    {
        public static IEnumerable<DarwinCoreTaxon> CreateFromFile(string fileName)
        {
            var taxa = LoadTaxa(fileName);
            return taxa;
        }

        private static IEnumerable<DarwinCoreTaxon> LoadTaxa(string fileName)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            using Stream zipStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read, false);
            
            var serializeOptions = new JsonSerializerOptions { IgnoreNullValues = true,  };
            serializeOptions.Converters.Add(new ObjectIdConverter());

            var taxa = JsonSerializer.Deserialize<List<DarwinCoreTaxon>>(zipArchive.Entries.First().Open(), serializeOptions);
            return taxa;
        }
    }
}