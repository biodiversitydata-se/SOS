using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Moq;
using Newtonsoft.Json;
using SOS.Export.Repositories.Interfaces;
using SOS.Export.UnitTests.TestHelpers.JsonConverters;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Search;

namespace SOS.Export.UnitTests.TestHelpers.Factories
{
    public static class ProcessedDarwinCoreRepositoryStubFactory
    {
        public static string TenObservations = @"Resources\TenProcessedTestObservations.json";

        public static Mock<IProcessedDarwinCoreRepository> Create(string fileName)
        {
            var stub = new Mock<IProcessedDarwinCoreRepository>();
            var observations = LoadObservations(fileName);
            stub
                .Setup(pdcr => pdcr.GetChunkAsync(It.IsAny<AdvancedFilter>(), 0, It.IsAny<int>()))
                .ReturnsAsync(observations);

            return stub;
        }
        
        public static Mock<IProcessedDarwinCoreRepository> Create(DarwinCore<DynamicProperties> observation)
        {
            var stub = new Mock<IProcessedDarwinCoreRepository>();
            stub
                .Setup(pdcr => pdcr.GetChunkAsync(It.IsAny<AdvancedFilter>(), 0, It.IsAny<int>()))
                .ReturnsAsync(new[] { observation });

            return stub;
        }

        private static IEnumerable<DarwinCore<DynamicProperties>> LoadObservations(string fileName)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            string str = File.ReadAllText(filePath, Encoding.UTF8);
            var serializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new ObjectIdConverter() }
            };

            var observations = JsonConvert.DeserializeObject<List<DarwinCore<DynamicProperties>>>(str, serializerSettings);
            return observations;
        }
    }
}
