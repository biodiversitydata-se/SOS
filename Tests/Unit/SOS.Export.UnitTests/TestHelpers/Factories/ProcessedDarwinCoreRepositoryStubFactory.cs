using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Moq;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.TestHelpers.JsonConverters;

namespace SOS.Export.UnitTests.TestHelpers.Factories
{
    public static class ProcessedDarwinCoreRepositoryStubFactory
    {
        public static string TenObservations = @"Resources\TenProcessedTestObservations.json";

        public static Mock<IProcessedObservationCoreRepository> Create(Observation observation)
        {
            var stub = new Mock<IProcessedObservationCoreRepository>();
            
            stub.SetupSequence(pdcr => pdcr.GetObservationsBySearchAfterAsync<Observation>(It.IsAny<SearchFilter>(), null, null))
                .ReturnsAsync(new SearchAfterResult<Observation>
                {
                    Records = new[] { observation } // return the observation the first call.
                })
                .ReturnsAsync(new SearchAfterResult<Observation>
                {
                    Records = Enumerable.Empty<Observation>()  // return empty the second call. new Observation[0]
                });

            stub.SetupSequence(pdcr => pdcr.GetObservationsBySearchAfterAsync<Observation>(It.IsAny<SearchFilter>(), It.IsAny<string>(), It.IsAny<IEnumerable<object>>()))
                .ReturnsAsync(new SearchAfterResult<Observation>
                {
                    Records = new[] {observation} // return the observation the first call.
                })
                .ReturnsAsync(new SearchAfterResult<Observation>
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

            var serializeOptions = new JsonSerializerOptions { IgnoreNullValues = true, };
            serializeOptions.Converters.Add(new ObjectIdConverter());

            var observations = JsonSerializer.Deserialize<List<Observation>>(str, serializeOptions);

            return observations;
        }
    }
}