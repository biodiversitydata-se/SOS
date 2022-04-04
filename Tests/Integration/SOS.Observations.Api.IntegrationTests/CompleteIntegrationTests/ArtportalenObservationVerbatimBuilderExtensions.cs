using FizzWare.NBuilder;
using FizzWare.NBuilder.Implementation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Observations.Api.IntegrationTests.CompleteIntegrationTests
{
    public static class ArtportalenObservationVerbatimBuilderExtensions
    {
        private static Bogus.Faker _faker = new Bogus.Faker();
        private static Bogus.DataSets.Lorem _lorem = new Bogus.DataSets.Lorem("sv");
        private const int ArtportalenDataSourceId = 1;        
        private static (int Value, float Probability)[] _verifiersProbability = new[] {
            (0, 0.50f), // 50% probability of getting zero verifiers
            (1, 0.20f),
            (2, 0.10f),
            (3, 0.05f),
            (4, 0.05f),
            (5, 0.05f),
            (6, 0.05f)
        };

        public static List<ArtportalenObservationVerbatim> VerbatimFromJsonNewtonsoft
        {
            get
            {
                if (_verbatimFromJsonNewtonsoft == null)
                {
                    string str = System.IO.File.ReadAllText(@"C:\TEMP\2022-04-04\ArtportalenVerbatimObservations1.json", Encoding.UTF8);
                    var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings
                    {
                        Converters = new List<Newtonsoft.Json.JsonConverter> { new TestHelpers.JsonConverters.ObjectIdConverter() }
                    };

                    _verbatimFromJsonNewtonsoft = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ArtportalenObservationVerbatim>>(str, serializerSettings);
                }

                return _verbatimFromJsonNewtonsoft;
            }        
        }
        private static List<ArtportalenObservationVerbatim> _verbatimFromJsonNewtonsoft;

        public static List<ArtportalenObservationVerbatim> VerbatimFromJson
        {
            get
            {
                if (_verbatimFromJsonNewtonsoft == null)
                {
                    string str = System.IO.File.ReadAllText(@"C:\TEMP\2022-04-04\ArtportalenVerbatimObservations2.json", Encoding.UTF8);
                    var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                    jsonSerializerOptions.Converters.Add(new Lib.JsonConverters.GeoShapeConverter());
                    jsonSerializerOptions.Converters.Add(new Lib.JsonConverters.GeoLocationConverter());

                    _verbatimFromJson = System.Text.Json.JsonSerializer.Deserialize<List<ArtportalenObservationVerbatim>>(str, jsonSerializerOptions);
                }

                return _verbatimFromJson;
            }
        }
        private static List<ArtportalenObservationVerbatim> _verbatimFromJson;
        
        private static UserInternal GetRandomUserInternal()
        {            
            return new UserInternal
            {
                Id = _faker.IndexVariable++,
                UserServiceUserId = _faker.IndexVariable++,
                UserAlias = _faker.Name.FirstName(),
                Discover = _faker.Random.Bool(),
                ViewAccess = _faker.Random.Bool(0.9f),
            };
        }      

        private static List<UserInternal> GetRandomUserInternals((int Value, float Probability)[] probabilities)
        {
            int[] values = probabilities.Select(m => m.Value).ToArray();
            float[] weights = probabilities.Select(m => m.Probability).ToArray();
            int nrVerifiers = _faker.Random.WeightedRandom(values, weights);
            List<UserInternal> userInternals = null;
            if (nrVerifiers > 0)
            {
                userInternals = new List<UserInternal>();
                for (int i = 0; i < nrVerifiers; i++)
                {
                    userInternals.Add(GetRandomUserInternal());
                }
            }

            return userInternals;
        }

        private static Site GetTestSite()
        {
            var site = new Site
            {
                Id = 6502743,
                XCoord = 1683599,
                YCoord = 8142284,
                Accuracy = 25,
                County = new GeographicalArea
                {
                    FeatureId = "18",
                    Name = "Örebro"
                },
                CountyPartIdByCoordinate = "18",
                BirdValidationAreaIds = new string[] { "100", "13" },
                Municipality = new GeographicalArea
                {
                    FeatureId = "1882",
                    Name = "Asersund"
                },
                Name = "Isåsen, Nrk",
                Parish = new GeographicalArea
                {
                    FeatureId = "2247",
                    Name = "Lerbäck"
                },
                PresentationNameParishRegion = "Isåsen, Lerbäck[[Shared_ParishName_Abbrevation]], Nrk",
                Point = new GeoJsonGeometry
                {
                    Type = "Point",
                    Coordinates = new System.Collections.ArrayList { 15.124, 58.8233 }
                },
                PointWithBuffer = new GeoJsonGeometry
                {
                    Type = "Polygon",
                    Coordinates = new System.Collections.ArrayList { new double[][] { // todo - generate automatic from coordinates and accuracy.
                            new double[] { 15.124460958527196, 58.82325882868679 },
                            new double[] {15.124460958527196, 58.82325882868679},
                            new double[] {15.124452622831287, 58.823302641640886},
                            new double[] {15.124427936079675, 58.82334477088773},
                            new double[] {15.124387846970382, 58.82338359742399},
                            new double[] {15.124333896105398, 58.82341762916765},
                            new double[] {15.124268156786204, 58.823445558297884},
                            new double[] {15.124193155337974, 58.82346631151389},
                            new double[] {15.124111774024298, 58.8234790912812},
                            new double[] {15.124027140283424, 58.823483406480534},
                            new double[] {15.12394250654255, 58.8234790912812},
                            new double[] {15.123861125228874, 58.82346631151389},
                            new double[] {15.123786123780643, 58.823445558297884},
                            new double[] {15.12372038446145, 58.82341762916765},
                            new double[] {15.123666433596465, 58.82338359742399},
                            new double[] {15.123626344487173, 58.82334477088773},
                            new double[] {15.123601657735561, 58.823302641640886},
                            new double[] {15.123593322039651, 58.82325882868679},
                            new double[] {15.123601657735561, 58.82321501573269},
                            new double[] {15.123626344487173, 58.82317288648585},
                            new double[] {15.123666433596465, 58.82313405994959},
                            new double[] {15.12372038446145, 58.82310002820593},
                            new double[] {15.123786123780643, 58.823072099075695},
                            new double[] {15.123861125228874, 58.82305134585969},
                            new double[] {15.12394250654255, 58.82303856609238},
                            new double[] {15.124027140283424, 58.823034250893045},
                            new double[] {15.124111774024298, 58.82303856609238},
                            new double[] {15.124193155337974, 58.82305134585969},
                            new double[] {15.124268156786204, 58.823072099075695},
                            new double[] {15.124333896105398, 58.82310002820593},
                            new double[] {15.124387846970382, 58.82313405994959},
                            new double[] {15.124427936079675, 58.82317288648585},
                            new double[] {15.124452622831287, 58.82321501573269},
                            new double[] {15.124460958527196, 58.82325882868679}}
                        }
                },
                Province = new GeographicalArea
                {
                    FeatureId = "10",
                    Name = "Närke"
                },
                ProvincePartIdByCoordinate = "10",
                VerbatimCoordinateSystem = Lib.Enums.CoordinateSys.WebMercator
            };

            return site;
        }       

        public static IOperable<ArtportalenObservationVerbatim> HaveRandomValidValues(this IOperable<ArtportalenObservationVerbatim> operable)
        {
            //bool equalStartAndEndDate = _faker.Random.WeightedRandom(new bool[] { true, false }, new float[] { 0.9f, 0.1f }); 
            TimeSpan? obsTimeSpan = _faker.Random.Bool(0.9f) ? null : _faker.Date.Timespan(TimeSpan.FromHours(5)); // 90% probability of equal start and end date.
            DateTime startDate = _faker.Date.Past(10); // random date within 10 years
            DateTime endDate = obsTimeSpan == null ? startDate : startDate.Add(obsTimeSpan.Value);
            DateTime reportedDate = endDate.Add(_faker.Date.Timespan(TimeSpan.FromDays(5)));
            DateTime editDate = reportedDate;
            List<UserInternal> verifiedByInternal = GetRandomUserInternals(_verifiersProbability);
            string verifiedBy = verifiedByInternal == null ? null : string.Join(", ", verifiedByInternal.Select(m => m.UserAlias));
            List<UserInternal> observersInternal = GetRandomUserInternals(_verifiersProbability);
            string observers = observersInternal == null ? null : string.Join(", ", observersInternal.Select(m => m.UserAlias));
            UserInternal reportedByInternal = GetRandomUserInternal();

            var builder = ((IDeclaration<ArtportalenObservationVerbatim>)operable).ObjectBuilder;
            builder.With(x => x.Id = _faker.IndexVariable++);
            builder.With(x => x.DatasourceId = ArtportalenDataSourceId);
            builder.With(x => x.DiscoveryMethod = new Metadata(0)); // todo
            builder.With(x => x.DeterminationMethod = new Metadata(0)); // todo
            builder.With(x => x.EditDate = editDate);
            builder.With(x => x.ReportedDate = reportedDate);
            builder.With(x => x.StartDate = startDate);
            builder.With(x => x.EndDate = endDate);
            builder.With(x => x.Gender = new Metadata(0));
            builder.With(x => x.HasImages = false); // todo
            builder.With(x => x.FirstImageId = 0); // todo
            builder.With(x => x.HasTriggeredValidationRules = _faker.Random.Bool()); // todo
            builder.With(x => x.HasAnyTriggeredValidationRuleWithWarning = _faker.Random.Bool()); // todo
            builder.With(x => x.NoteOfInterest = _faker.Random.Bool()); // todo
            builder.With(x => x.HasUserComments = _faker.Random.Bool()); // todo
            builder.With(x => x.NotPresent = false); // _faker.Random.Bool()); // todo
            builder.With(x => x.NotRecovered = false); // _faker.Random.Bool()); // todo
            builder.With(x => x.ProtectedBySystem = false); // _faker.Random.Bool()); // todo
            builder.With(x => x.RightsHolder = _faker.Name.FullName());
            builder.With(x => x.VerifiedByInternal = verifiedByInternal);
            builder.With(x => x.VerifiedBy = verifiedBy);
            builder.With(x => x.ObserversInternal = observersInternal);
            builder.With(x => x.Observers = observers);
            builder.With(x => x.Site = GetTestSite()); // todo
            builder.With(x => x.SpeciesGroupId = 1); // todo
            builder.With(x => x.Stage = new Metadata(0)); // todo
            builder.With(x => x.TaxonId = 231326); // todo
            builder.With(x => x.Unit = new Metadata(0)); // todo
            builder.With(x => x.Unspontaneous = false); // todo?
            builder.With(x => x.UnsureDetermination = false); // todo?
            builder.With(x => x.ValidationStatus = new Metadata(10)); // todo
            builder.With(x => x.ReportedBy = reportedByInternal.UserAlias); // todo
            builder.With(x => x.ReportedByUserId = reportedByInternal.Id);
            builder.With(x => x.ReportedByUserServiceUserId = reportedByInternal.UserServiceUserId);
            builder.With(x => x.ReportedByUserAlias = reportedByInternal.UserAlias);
            builder.With(x => x.DeterminedBy = _faker.Name.FullName());
            builder.With(x => x.SightingTypeSearchGroupId = 1); // todo
            builder.With(x => x.SpeciesFactsIds = new List<int> { 4, 1938 }); // todo

            // todo - add values for more properties
            //Activity
            //Biotope
            //BiotopeDescription
            //CollectionID
            //Comment
            //ConfirmationYear
            //ConfirmedBy
            //DatasourceId
            //DeterminationYear
            //StartTime
            //EndTime
            //FrequencyId
            //HasUserComments
            //HiddenByProvider
            //Label
            //Length
            //MaxDepth
            //MaxHeight
            //Media
            //MigrateSightingObsId
            //MigrateSightingPortalId
            //MinDepth
            //MinHeight
            //OwnerOrganization
            //PrivateCollection
            //Projects
            //PublicCollection
            //Quantity
            //QuantityOfSubstrates
            //RegionalSightingStateId
            //ReproductionId
            //SightingPublishTypeIds
            //SightingSpeciesCollectionItemId
            //SightingTypeId
            //SightingTypeSearchGroupId
            //SpeciesCollection
            //SpeciesGroupId
            //StartTime
            //Substrate
            //SubstrateDescription
            //SubstrateSpeciesId
            //VerifiedBy
            //VerifiedByInternal
            //Weight

            return operable;
        }
    }
}
