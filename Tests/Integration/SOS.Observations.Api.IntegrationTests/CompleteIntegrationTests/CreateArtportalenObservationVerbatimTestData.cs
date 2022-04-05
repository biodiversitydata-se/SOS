using LinqStatistics;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using SOS.TestHelpers.JsonConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.CompleteIntegrationTests
{
    [Collection(Collections.CompleteApiIntegrationTestsCollection)]
    public class CreateArtportalenObservationVerbatimTestData
    {
        private readonly CompleteApiIntegrationTestFixture _fixture;

        public CreateArtportalenObservationVerbatimTestData(CompleteApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        private bool IsObservationOk(ArtportalenObservationVerbatim obs)
        {
            if (obs.ProtectedBySystem) return false;

            return true;
        }

        [Fact]        
        public async Task CreateArtportalenVerbatimObservationTestData()
        {
            // Read observations from MongoDB
            using var cursor = await _fixture.ArtportalenVerbatimRepository.GetAllByCursorAsync();
            const int NrRowsToRead = 1000;
            int nrRowsRead = 0;
            var verbatimObservations = new List<ArtportalenObservationVerbatim>();
            while (await cursor.MoveNextAsync())
            {
                if (nrRowsRead >= NrRowsToRead) break;
                foreach (var observation in cursor.Current)
                {
                    if (nrRowsRead >= NrRowsToRead) break;
                    if (IsObservationOk(observation))
                    {
                        verbatimObservations.Add(observation);
                        nrRowsRead++;
                    }
                }
            }

            // Write observations to JSON

            // Serialize using Newtonsoft.Json.JsonConvert
            var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                Converters = new List<Newtonsoft.Json.JsonConverter> { new ObjectIdConverter() }
            };
            var strJson = Newtonsoft.Json.JsonConvert.SerializeObject(verbatimObservations, serializerSettings);
            System.IO.File.WriteAllText(@"C:\Temp\2022-04-04\ArtportalenVerbatimObservations1.json", strJson, Encoding.UTF8);
            //var observations1 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ArtportalenObservationVerbatim>>(strJson, serializerSettings);

            // Serialize using System.Text.Json.JsonSerializer
            var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            jsonSerializerOptions.Converters.Add(new GeoShapeConverter());
            jsonSerializerOptions.Converters.Add(new GeoLocationConverter());
            var strJson2 = System.Text.Json.JsonSerializer.Serialize(verbatimObservations.First(), jsonSerializerOptions);
            System.IO.File.WriteAllText(@"C:\Temp\2022-04-04\ArtportalenVerbatimObservations2.json", strJson, Encoding.UTF8);
            //var observations2 = System.Text.Json.JsonSerializer.Deserialize<List<ArtportalenObservationVerbatim>>(strJson2, jsonSerializerOptions);            
        }      
    }
}