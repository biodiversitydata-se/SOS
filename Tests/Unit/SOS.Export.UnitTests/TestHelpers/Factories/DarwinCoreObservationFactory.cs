using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using SOS.Export.UnitTests.TestHelpers.JsonConverters;
using SOS.Lib.Models.Processed.DarwinCore;

namespace SOS.Export.UnitTests.TestHelpers.Factories
{
    public static class DarwinCoreObservationFactory
    {
        public static DarwinCore<DynamicProperties> CreateDefaultObservation()
        {
            string fileName = @"Resources\DefaultTestObservation.json";
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            string str = File.ReadAllText(filePath, Encoding.UTF8);
            var serializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new ObjectIdConverter() }
            };

            var observation = JsonConvert.DeserializeObject<DarwinCore<DynamicProperties>>(str, serializerSettings);
            return observation;
        }
    }
}
