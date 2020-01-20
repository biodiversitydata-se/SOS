using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using MessagePack;
using MessagePack.Resolvers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.TestHelpers.JsonConverters;

namespace SOS.Lib.UnitTests.TestHelpers.Factories
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
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            using Stream zipStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read, false);
            using StreamReader sr = new StreamReader(zipArchive.Entries.First().Open());
            using JsonReader reader = new JsonTextReader(sr);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new ObjectIdConverter());
            serializer.ContractResolver = new IgnoreJsonAttributesResolver();
            var taxa = serializer.Deserialize<List<DarwinCoreTaxon>>(reader);
            return taxa;
        }

        public static IEnumerable<DarwinCoreTaxon> CreateFromMessagePackFile(string fileName)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            var bytes = File.ReadAllBytes(filePath);
            var options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
            var taxa = MessagePackSerializer.Deserialize<List<DarwinCoreTaxon>>(bytes, options);
            return taxa;
        }

        /// <summary>
        /// Class that is used to ignore the JsonIgnore attribute, so that those properties will be included.
        /// </summary>
        private class IgnoreJsonAttributesResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);
                foreach (var prop in props)
                {
                    prop.Ignored = false;   // Ignore [JsonIgnore]
                    prop.Converter = null;  // Ignore [JsonConverter]
                    prop.PropertyName = prop.UnderlyingName;  // restore original property name
                }
                return props;
            }
        }
    }
}