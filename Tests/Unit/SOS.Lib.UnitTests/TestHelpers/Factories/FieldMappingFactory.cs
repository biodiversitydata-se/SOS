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
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Shared;
using SOS.TestHelpers.JsonConverters;

namespace SOS.Lib.UnitTests.TestHelpers.Factories
{
    public static class FieldMappingFactory
    {
        public static IEnumerable<T> CreateFromMessagePackFile<T>(string fileName)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            var bytes = File.ReadAllBytes(filePath);
            var options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
            var objects = MessagePackSerializer.Deserialize<List<T>>(bytes, options);
            return objects;
        }
    }
}