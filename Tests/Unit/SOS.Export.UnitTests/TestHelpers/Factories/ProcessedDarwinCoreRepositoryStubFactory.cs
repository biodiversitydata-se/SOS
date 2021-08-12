using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Moq;
using Newtonsoft.Json;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.TestHelpers.JsonConverters;

namespace SOS.Export.UnitTests.TestHelpers.Factories
{
    public static class ProcessedDarwinCoreRepositoryStubFactory
    {
        public static string TenObservations = @"Resources\TenProcessedTestObservations.json";

        public static Mock<IProcessedObservationRepository> Create(Observation observation)
        {
            var stub = new Mock<IProcessedObservationRepository>();
            
            stub.SetupSequence(pdcr => pdcr.ScrollObservationsAsync(It.IsAny<SearchFilter>(), null))
                .ReturnsAsync(new ScrollResult<Observation>
                {
                    Records = new[] { observation } // return the observation the first call.
                })
                .ReturnsAsync(new ScrollResult<Observation>
                {
                    Records = Enumerable.Empty<Observation>()  // return empty the second call. new Observation[0]
                });

            stub.SetupSequence(pdcr => pdcr.ScrollObservationsAsync(It.IsAny<SearchFilter>(), null))
                .ReturnsAsync(new ScrollResult<Observation>
                {
                    Records = new[] {observation} // return the observation the first call.
                })
                .ReturnsAsync(new ScrollResult<Observation>
                    {
                        Records = Enumerable.Empty<Observation>()  // return empty the second call. new Observation[0]
                });

            return stub;
        }

        private static IEnumerable<Observation> LoadObservations(string fileName)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            var str = File.ReadAllText(filePath, Encoding.UTF8);
            var serializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new ObjectIdConverter()}
            };

            var observations = JsonConvert.DeserializeObject<List<Observation>>(str, serializerSettings);
            return observations;
        }
    }
}