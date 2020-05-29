using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
            using var sr = new StreamReader(zipArchive.Entries.First().Open());
            using JsonReader reader = new JsonTextReader(sr);
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new ObjectIdConverter());
            serializer.ContractResolver = new IgnoreJsonAttributesResolver();
            var taxa = serializer.Deserialize<List<DarwinCoreTaxon>>(reader);
            return taxa;
        }

        /// <summary>
        ///     Class that is used to ignore the JsonIgnore attribute, so that those properties will be included.
        /// </summary>
        private class IgnoreJsonAttributesResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var props = base.CreateProperties(type, memberSerialization);
                foreach (var prop in props)
                {
                    prop.Ignored = false; // Ignore [JsonIgnore]
                    prop.Converter = null; // Ignore [JsonConverter]
                    prop.PropertyName = prop.UnderlyingName; // restore original property name
                }

                return props;
            }
        }
    }
}