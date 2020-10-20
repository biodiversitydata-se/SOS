using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using SOS.Lib.Models.Processed.Observation;
using SOS.TestHelpers.JsonConverters;

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
            var serializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new ObjectIdConverter()}
            };

            var observation = JsonConvert.DeserializeObject<Observation>(str, serializerSettings);
            return observation;
        }
    }
}