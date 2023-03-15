using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Export.UnitTests.TestHelpers.Factories
{
    public static class DarwinCoreObservationFactory
    {
        public static Observation CreateDefaultObservation()
        {
            var fileName = @"Resources\DefaultTestObservation.json";
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            var str = File.ReadAllText(filePath, Encoding.UTF8);

            var serializeOptions = new JsonSerializerOptions { IgnoreNullValues = true, };
            serializeOptions.Converters.Add(new ObjectIdConverter());

            var observation = JsonSerializer.Deserialize<Observation>(str, serializeOptions);

            return observation;
        }
    }
}