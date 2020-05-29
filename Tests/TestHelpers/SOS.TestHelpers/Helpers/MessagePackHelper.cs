using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MessagePack;
using MessagePack.Resolvers;

namespace SOS.TestHelpers.Helpers
{
    public static class MessagePackHelper
    {
        public static T CreateFromMessagePackFile<T>(string fileName)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            var bytes = File.ReadAllBytes(filePath);
            var options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
            var objects = MessagePackSerializer.Deserialize<T>(bytes, options);
            return objects;
        }

        public static List<T> CreateListFromMessagePackFile<T>(string fileName)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            var bytes = File.ReadAllBytes(filePath);
            var options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
            var objects = MessagePackSerializer.Deserialize<List<T>>(bytes, options);
            return objects;
        }
    }
}