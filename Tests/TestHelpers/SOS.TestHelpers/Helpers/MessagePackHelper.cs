using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using MessagePack;
using MessagePack.Resolvers;

namespace SOS.TestHelpers.Helpers
{
    public static class MessagePackHelper
    {
        public static List<T> CreateListFromMessagePackFile<T>(string fileName)
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