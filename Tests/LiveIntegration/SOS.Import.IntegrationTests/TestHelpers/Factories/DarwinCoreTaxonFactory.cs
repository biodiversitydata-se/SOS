﻿using SOS.Lib.JsonConverters;
using SOS.Lib.Models.DarwinCore;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace SOS.Import.LiveIntegrationTests.TestHelpers.Factories
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

            var serializeOptions = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
            serializeOptions.Converters.Add(new ObjectIdConverter());

            var taxa = JsonSerializer.Deserialize<List<DarwinCoreTaxon>>(zipArchive.Entries.First().Open(), serializeOptions);
            return taxa;
        }
    }
}