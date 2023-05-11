using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace SOS.AutomaticIntegrationTests.DataUtils
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class CreateArtportalenJsonTestData
    {
        private readonly IntegrationTestFixture _fixture;
        private static Bogus.Faker _faker = new Bogus.Faker("sv");

        public CreateArtportalenJsonTestData(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(Skip = "Intended to run on demand when needed")]
        [Trait("Category", "DataUtil")]
        public async Task CreateArtportalenVerbatimObservationTestData()
        {
            // Read observations from MongoDB            
            const int NrObservationsToAdd = 10000;
            long nrItemsInCollection = await _fixture.ArtportalenVerbatimRepository.CountAllDocumentsAsync();
            int skipInterval = (int)nrItemsInCollection / NrObservationsToAdd - 50; // -50 because some observations are protected
            using var cursor = await _fixture.ArtportalenVerbatimRepository.GetAllByCursorAsync();
            int skipCounter = skipInterval;
            int nrObservationsAdded = 0;
            var verbatimObservations = new List<ArtportalenObservationVerbatim>();
            while (await cursor.MoveNextAsync())
            {
                if (nrObservationsAdded >= NrObservationsToAdd) break;
                foreach (var observation in cursor.Current)
                {
                    if (nrObservationsAdded >= NrObservationsToAdd) break;
                    skipCounter--;
                    if (IsObservationOk(observation))
                    {
                        if (skipCounter <= 0)
                        {
                            CleanObservation(observation);
                            verbatimObservations.Add(observation);
                            nrObservationsAdded++;
                            skipCounter = skipInterval;
                        }
                    }
                }
            }

            // Write observations to JSON

            var serializeOptions = new JsonSerializerOptions { IgnoreNullValues = true };
            serializeOptions.Converters.Add(new ObjectIdConverter());
            serializeOptions.Converters.Add(new JsonStringEnumConverter());

            var strJson = JsonSerializer.Serialize(verbatimObservations, serializeOptions);
            System.IO.File.WriteAllText(@"C:\Temp\ArtportalenVerbatimObservations10000_1.json", strJson, Encoding.UTF8);
            System.IO.File.WriteAllText(@"C:\Temp\ArtportalenVerbatimObservations1000_1.json", JsonSerializer.Serialize(verbatimObservations.Take(1000), serializeOptions), Encoding.UTF8);
            System.IO.File.WriteAllText(@"C:\Temp\ArtportalenVerbatimObservations5000_1.json", JsonSerializer.Serialize(verbatimObservations.Take(5000), serializeOptions), Encoding.UTF8);
            //var observations1 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ArtportalenObservationVerbatim>>(strJson, serializerSettings);

            // Serialize using System.Text.Json.JsonSerializer
            var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            jsonSerializerOptions.Converters.Add(new GeoShapeConverter());
            jsonSerializerOptions.Converters.Add(new GeoLocationConverter());
            var strJson2 = System.Text.Json.JsonSerializer.Serialize(verbatimObservations, jsonSerializerOptions);
            System.IO.File.WriteAllText(@"C:\Temp\ArtportalenVerbatimObservations10000_2.json", strJson2, Encoding.UTF8);
            System.IO.File.WriteAllText(@"C:\Temp\ArtportalenVerbatimObservations1000_2.json", JsonSerializer.Serialize(verbatimObservations.Take(1000), serializeOptions), Encoding.UTF8);
            System.IO.File.WriteAllText(@"C:\Temp\ArtportalenVerbatimObservations5000_2.json", JsonSerializer.Serialize(verbatimObservations.Take(5000), serializeOptions), Encoding.UTF8);
            //var observations2 = System.Text.Json.JsonSerializer.Deserialize<List<ArtportalenObservationVerbatim>>(strJson2, jsonSerializerOptions);            
        }

        [Fact(Skip = "Intended to run on demand when needed")]        
        [Trait("Category", "DataUtil")]
        public async Task CreateArtportalenVerbatimChecklistTestData()
        {
            // Read observations from MongoDB            
            const int NrChecklistsToAdd = 1000;
            long nrItemsInCollection = await _fixture.ArtportalenChecklistVerbatimRepository.CountAllDocumentsAsync();
            int skipInterval = (int)nrItemsInCollection / NrChecklistsToAdd;
            using var cursor = await _fixture.ArtportalenChecklistVerbatimRepository.GetAllByCursorAsync();
            int skipCounter = skipInterval;
            int nrChecklistsAdded = 0;
            var verbatimChecklists = new List<ArtportalenChecklistVerbatim>();
            while (await cursor.MoveNextAsync())
            {
                if (nrChecklistsAdded >= NrChecklistsToAdd) break;
                foreach (var checklist in cursor.Current)
                {
                    if (nrChecklistsAdded >= NrChecklistsToAdd) break;
                    skipCounter--;                    
                    if (skipCounter <= 0)
                    {
                        //CleanObservation(observation);
                        verbatimChecklists.Add(checklist);
                        nrChecklistsAdded++;
                        skipCounter = skipInterval;
                    }                    
                }
            }

            // Write observations to JSON

            var serializeOptions = new JsonSerializerOptions { IgnoreNullValues = true, PropertyNameCaseInsensitive = true };
            serializeOptions.Converters.Add(new ObjectIdConverter());
            serializeOptions.Converters.Add(new JsonStringEnumConverter());
            serializeOptions.Converters.Add(new GeoShapeConverter());
            serializeOptions.Converters.Add(new GeoLocationConverter());

            var strJson = JsonSerializer.Serialize(verbatimChecklists, serializeOptions);
            System.IO.File.WriteAllText(@"C:\Temp\ArtportalenVerbatimChecklists1000.json", strJson, Encoding.UTF8);

            var strJson2 = JsonSerializer.Serialize(verbatimChecklists, serializeOptions);
            System.IO.File.WriteAllText(@"C:\Temp\ArtportalenVerbatimChecklists1000_2.json", strJson2, Encoding.UTF8);            
        }


        private bool IsObservationOk(ArtportalenObservationVerbatim obs)
        {
            if (obs.ProtectedBySystem) return false;
            if (obs.HiddenByProvider.HasValue) return false;

            return true;
        }

        private void CleanObservation(ArtportalenObservationVerbatim obs)
        {
            if (!string.IsNullOrWhiteSpace(obs.DeterminedBy))
            {
                obs.DeterminedBy = _faker.Name.FullName();
            }

            if (!string.IsNullOrWhiteSpace(obs.ConfirmedBy))
            {
                obs.ConfirmedBy = _faker.Name.FullName();
            }

            if (!string.IsNullOrWhiteSpace(obs.RightsHolder))
            {
                obs.RightsHolder = _faker.Name.FullName();
            }

            CleanUserInternals(obs.ObserversInternal);

            if (!string.IsNullOrWhiteSpace(obs.Observers))
            {
                int nrObservers = obs.ObserversInternal == null ? 0 : obs.ObserversInternal.Count();
                string[] observers = new string[nrObservers];
                for (int i = 0; i < nrObservers; i++)
                {
                    observers[i] = _faker.Name.FullName();
                }

                obs.Observers = string.Join(", ", observers);
            }

            if (!string.IsNullOrWhiteSpace(obs.ReportedBy))
            {
                obs.ReportedBy = _faker.Name.FullName();
            }

            if (!string.IsNullOrWhiteSpace(obs.ReportedByUserAlias))
            {
                obs.ReportedByUserAlias = GetUserAlias();
            }

            obs.ReportedByUserId = _faker.Random.Int(1, 100000);
            obs.ReportedByUserServiceUserId = _faker.Random.Int(1, 100000);

            if (!string.IsNullOrWhiteSpace(obs.VerifiedBy))
            {
                obs.VerifiedBy = _faker.Name.FullName();
            }

            CleanUserInternals(obs.VerifiedByInternal);
        }

        private void CleanUserInternals(IEnumerable<UserInternal> users)
        {
            if (users != null && users.Count() > 0)
            {
                foreach (var observer in users)
                {
                    observer.Id = _faker.Random.Int(1, 100000);
                    observer.UserAlias = GetUserAlias();
                    observer.UserServiceUserId = _faker.Random.Int(1, 100000);
                }
            }
        }

        private string GetUserAlias()
        {
            return _faker.Hacker.Adjective() + "_" + _faker.Name.FirstName();
        }
    }
}