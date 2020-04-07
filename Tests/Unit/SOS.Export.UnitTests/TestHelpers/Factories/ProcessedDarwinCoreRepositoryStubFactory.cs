using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Moq;
using Newtonsoft.Json;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.TestHelpers.JsonConverters;

namespace SOS.Export.UnitTests.TestHelpers.Factories
{
    public static class ProcessedDarwinCoreRepositoryStubFactory
    {
        public static string TenObservations = @"Resources\TenProcessedTestObservations.json";

        public static Mock<IProcessedObservationRepository> Create(string fileName)
        {
            var stub = new Mock<IProcessedObservationRepository>();
            var observations = new ScrollResult<ProcessedObservation>
            {
                Records = LoadObservations(fileName)
            };
            stub
                .Setup(pdcr => pdcr.ScrollAsync(It.IsAny<SearchFilter>(), null))
                .ReturnsAsync(observations);

            return stub;
        }
        
        public static Mock<IProcessedObservationRepository> Create(ProcessedObservation observation)
        {
            var stub = new Mock<IProcessedObservationRepository>();
            stub
                .Setup(pdcr => pdcr.ScrollAsync(It.IsAny<SearchFilter>(), null))
                .ReturnsAsync(new ScrollResult<ProcessedObservation>
                {
                    Records = new[] { observation }
                });

            return stub;
        }

        private static IEnumerable<ProcessedObservation> LoadObservations(string fileName)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            string str = File.ReadAllText(filePath, Encoding.UTF8);
            var serializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new ObjectIdConverter() }
            };

            var observations = JsonConvert.DeserializeObject<List<ProcessedObservation>>(str, serializerSettings);
            return observations;
        }
    }
}
