using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Moq;
using Newtonsoft.Json;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Search;
using SOS.TestHelpers.JsonConverters;

namespace SOS.Export.IntegrationTests.TestHelpers.Factories
{
    public static class ProcessedDarwinCoreRepositoryStubFactory
    {
        public static string TenObservations = @"Resources\TenProcessedTestObservations.json";

        public static Mock<IProcessedSightingRepository> Create(string fileName)
        {
            var stub = new Mock<IProcessedSightingRepository>();
            var observations = LoadObservations(fileName);
            stub
                .Setup(pdcr => pdcr.GetChunkAsync(It.IsAny<SearchFilter>(), 0, It.IsAny<int>()))
                .ReturnsAsync(observations);

            return stub;
        }
        
        public static Mock<IProcessedSightingRepository> Create(ProcessedSighting observation)
        {
            var stub = new Mock<IProcessedSightingRepository>();
            stub
                .Setup(pdcr => pdcr.GetChunkAsync(It.IsAny<SearchFilter>(), 0, It.IsAny<int>()))
                .ReturnsAsync(new[] { observation });

            return stub;
        }

        private static IEnumerable<ProcessedSighting> LoadObservations(string fileName)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            string str = File.ReadAllText(filePath, Encoding.UTF8);
            var serializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new ObjectIdConverter() }
            };

            var observations = JsonConvert.DeserializeObject<List<ProcessedSighting>>(str, serializerSettings);
            return observations;
        }
    }
}
