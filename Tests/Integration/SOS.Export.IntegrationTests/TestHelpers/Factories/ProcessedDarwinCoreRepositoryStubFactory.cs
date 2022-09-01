using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Moq;
using Newtonsoft.Json;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.TestHelpers.JsonConverters;

namespace SOS.Export.IntegrationTests.TestHelpers.Factories
{
    public static class ProcessedDarwinCoreRepositoryStubFactory
    {
        public static string TenObservations = @"Resources\TenProcessedTestObservations.json";

        public static Mock<IProcessedObservationCoreRepository> Create(string fileName)
        {
            var stub = new Mock<IProcessedObservationCoreRepository>();
            var observations = LoadObservations(fileName);
            stub
                .Setup(pdcr => pdcr.GetObservationsBySearchAfterAsync<Observation>(It.IsAny<SearchFilter>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>()))
                .ReturnsAsync(observations);

            return stub;
        }

        private static SearchAfterResult<Observation> LoadObservations(string fileName)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            var str = File.ReadAllText(filePath, Encoding.UTF8);
            var serializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new ObjectIdConverter()}
            };

            var observations = JsonConvert.DeserializeObject<List<Observation>>(str, serializerSettings);
            return new SearchAfterResult<Observation> {Records = observations};
        }
    }
}