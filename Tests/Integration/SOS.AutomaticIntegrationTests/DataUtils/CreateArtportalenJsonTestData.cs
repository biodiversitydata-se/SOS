using LinqStatistics;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.TestHelpers.JsonConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            // Serialize using Newtonsoft.Json.JsonConvert
            var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                Converters = new List<Newtonsoft.Json.JsonConverter> { new ObjectIdConverter() }
            };
            var strJson = Newtonsoft.Json.JsonConvert.SerializeObject(verbatimObservations, serializerSettings);
            System.IO.File.WriteAllText(@"C:\Temp\ArtportalenVerbatimObservations10000_1.json", strJson, Encoding.UTF8);
            System.IO.File.WriteAllText(@"C:\Temp\ArtportalenVerbatimObservations1000_1.json", Newtonsoft.Json.JsonConvert.SerializeObject(verbatimObservations.Take(1000), serializerSettings), Encoding.UTF8);
            System.IO.File.WriteAllText(@"C:\Temp\ArtportalenVerbatimObservations5000_1.json", Newtonsoft.Json.JsonConvert.SerializeObject(verbatimObservations.Take(5000), serializerSettings), Encoding.UTF8);
            //var observations1 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ArtportalenObservationVerbatim>>(strJson, serializerSettings);

            // Serialize using System.Text.Json.JsonSerializer
            var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            jsonSerializerOptions.Converters.Add(new GeoShapeConverter());
            jsonSerializerOptions.Converters.Add(new GeoLocationConverter());
            var strJson2 = System.Text.Json.JsonSerializer.Serialize(verbatimObservations, jsonSerializerOptions);
            System.IO.File.WriteAllText(@"C:\Temp\ArtportalenVerbatimObservations10000_2.json", strJson2, Encoding.UTF8);
            System.IO.File.WriteAllText(@"C:\Temp\ArtportalenVerbatimObservations1000_2.json", System.Text.Json.JsonSerializer.Serialize(verbatimObservations.Take(1000), jsonSerializerOptions), Encoding.UTF8);
            System.IO.File.WriteAllText(@"C:\Temp\ArtportalenVerbatimObservations5000_2.json", System.Text.Json.JsonSerializer.Serialize(verbatimObservations.Take(5000), jsonSerializerOptions), Encoding.UTF8);
            //var observations2 = System.Text.Json.JsonSerializer.Deserialize<List<ArtportalenObservationVerbatim>>(strJson2, jsonSerializerOptions);            
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