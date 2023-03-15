using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.TestHelpers.JsonConverters;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace SOS.AutomaticIntegrationTests.DataUtils
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class CreateDarwinCoreJsonTestData
    {
        private readonly IntegrationTestFixture _fixture;
        private static Bogus.Faker _faker = new Bogus.Faker("sv");

        public CreateDarwinCoreJsonTestData(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(Skip = "Intended to run on demand when needed")]
        [Trait("Category", "DataUtil")]
        public async Task CreateDarwinCoreVerbatimObservationTestData()
        {
            // Read observations from MongoDB            
            const int NrObservationsToAdd = 1000;            
            var repository = _fixture.GetDarwinCoreArchiveVerbatimRepository(new DataProvider() { Id = 14, Identifier= "SharkZooplankton" });
            long nrItemsInCollection = await repository.CountAllDocumentsAsync();            
            using var cursor = await repository.GetAllByCursorAsync();
            int nrObservationsAdded = 0;
            var verbatimObservations = new List<DwcObservationVerbatim>();
            while (await cursor.MoveNextAsync())
            {
                if (nrObservationsAdded >= NrObservationsToAdd) break;
                foreach (var observation in cursor.Current)
                {
                    if (nrObservationsAdded >= NrObservationsToAdd) break;                    
                    if (IsObservationOk(observation))
                    {
                        CleanObservation(observation);
                        // Only add observations with emof
                         if (observation.ObservationExtendedMeasurementOrFacts != null)
                        {
                            verbatimObservations.Add(observation);
                            nrObservationsAdded++;
                        }
                    }
                }
            }

            if (nrObservationsAdded < NrObservationsToAdd)
            {
                using var cursor2 = await repository.GetAllByCursorAsync();
                while (await cursor2.MoveNextAsync())
                {
                    if (nrObservationsAdded >= NrObservationsToAdd) break;
                    foreach (var observation in cursor2.Current)
                    {
                        if (nrObservationsAdded >= NrObservationsToAdd) break;
                        if (IsObservationOk(observation))
                        {
                            CleanObservation(observation);
                            // Only add observations with multimedia
                              if (observation.ObservationMultimedia != null)
                            {
                                verbatimObservations.Add(observation);
                                nrObservationsAdded++;
                            }
                        }
                    }
                }
            }

            // Write observations to JSON
            var serializeOptions = new JsonSerializerOptions { IgnoreNullValues = true };
            serializeOptions.Converters.Add(new ObjectIdConverter());

            var strJson = JsonSerializer.Serialize(verbatimObservations.Take(1000), serializeOptions);
            System.IO.File.WriteAllText(@"C:\Temp\DarwinCoreObservations_1000.json", strJson, Encoding.UTF8);  
        }

        private bool IsObservationOk(DwcObservationVerbatim obs)
        {
            //if (obs.ProtectedBySystem) return false;
            //if (obs.HiddenByProvider.HasValue) return false;

            return true;
        }

        private void CleanObservation(DwcObservationVerbatim obs)
        {
            if (!string.IsNullOrWhiteSpace(obs.RecordedBy))
            {
                obs.RecordedBy = _faker.Name.FullName();
            }

            if (!string.IsNullOrWhiteSpace(obs.RightsHolder))
            {
                obs.RightsHolder = _faker.Name.FullName();
            }

            obs.DataProviderId = 1;
            obs.DataProviderIdentifier = "Artportalen";

            if (obs.ObservationMultimedia != null)
            {
                foreach (var row in obs.ObservationMultimedia)
                {
                    row.RightsHolder = _faker.Name.FullName();
                }
            }
        }        
    }
}