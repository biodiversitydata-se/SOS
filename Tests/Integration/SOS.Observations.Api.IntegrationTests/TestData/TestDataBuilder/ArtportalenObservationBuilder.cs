﻿using FizzWare.NBuilder;
using FizzWare.NBuilder.Implementation;
using MongoDB.Driver.GeoJsonObjectModel;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums.Artportalen;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Extensions;

namespace SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder
{
    public static class ArtportalenObservationBuilder
    {
        private static Bogus.Faker _faker = new Bogus.Faker();
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

        public static List<ArtportalenObservationVerbatim>? VerbatimArtportalenObservationsFromJsonFile(bool sensitive)
        {
            if (_verbatimArtportalenObservationsFromJsonFile == null || !sensitive.Equals(_sensitiveLoaded))
            {
                string filePath = $"Resources/TestDataBuilder/{(sensitive ? "ArtportalenVerbatimProtectedObservations_1000" : "ArtportalenVerbatimObservations_1000")}.json".GetAbsoluteFilePath();
                string str = File.ReadAllText(filePath, Encoding.UTF8);
                var serializeOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNameCaseInsensitive = true, IncludeFields = false };
                serializeOptions.Converters.Add(new ObjectIdConverter());
                serializeOptions.Converters.Add(new JsonStringEnumConverter());
                serializeOptions.Converters.Add(new GeoJsonConverter());

                _verbatimArtportalenObservationsFromJsonFile = JsonSerializer.Deserialize<List<ArtportalenObservationVerbatim>>(str, serializeOptions);

                // _verbatimArtportalenObservationsFromJsonFile = JsonConvert.DeserializeObject<List<ArtportalenObservationVerbatim>>(str, serializerSettings);
                _sensitiveLoaded = sensitive;
            }

            return _verbatimArtportalenObservationsFromJsonFile;

        }

        private static List<ArtportalenObservationVerbatim>? _verbatimArtportalenObservationsFromJsonFile;
        private static bool _sensitiveLoaded;

        private static UserInternal GetRandomUserInternal()
        {
            return new UserInternal
            {
                Id = _faker.IndexVariable++,
                PersonId = _faker.IndexVariable++,
                UserServiceUserId = _faker.IndexVariable++,
                UserAlias = _faker.Name.FirstName(),
                Discover = _faker.Random.Bool(),
                ViewAccess = _faker.Random.Bool(0.9f),
            };
        }

        private static List<UserInternal>? GetRandomUserInternals((int Value, float Probability)[] probabilities)
        {
            int[] values = probabilities.Select(m => m.Value).ToArray();
            float[] weights = probabilities.Select(m => m.Probability).ToArray();
            int nrVerifiers = _faker.Random.WeightedRandom(values, weights);
            List<UserInternal>? userInternals = null;
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
                Point = new GeoJsonPoint<GeoJson2DCoordinates>(new GeoJson2DCoordinates(15.124, 58.8233)),
                PointWithBuffer = new GeoJsonPolygon<GeoJson2DCoordinates>(
                    new GeoJsonPolygonCoordinates<GeoJson2DCoordinates>(
                        new GeoJsonLinearRingCoordinates<GeoJson2DCoordinates>(new[]
                        {
                            new GeoJson2DCoordinates(15.124460958527196, 58.82325882868679),
                            new GeoJson2DCoordinates(15.124452622831287, 58.823302641640886),
                            new GeoJson2DCoordinates(15.124427936079675, 58.82334477088773),
                            new GeoJson2DCoordinates(15.124387846970382, 58.82338359742399),
                            new GeoJson2DCoordinates(15.124333896105398, 58.82341762916765),
                            new GeoJson2DCoordinates(15.124268156786204, 58.823445558297884),
                            new GeoJson2DCoordinates(15.124193155337974, 58.82346631151389),
                            new GeoJson2DCoordinates(15.124111774024298, 58.8234790912812),
                            new GeoJson2DCoordinates(15.124027140283424, 58.823483406480534),
                            new GeoJson2DCoordinates(15.12394250654255, 58.8234790912812),
                            new GeoJson2DCoordinates(15.123861125228874, 58.82346631151389),
                            new GeoJson2DCoordinates(15.123786123780643, 58.823445558297884),
                            new GeoJson2DCoordinates(15.12372038446145, 58.82341762916765),
                            new GeoJson2DCoordinates(15.123666433596465, 58.82338359742399),
                            new GeoJson2DCoordinates(15.123626344487173, 58.82334477088773),
                            new GeoJson2DCoordinates(15.123601657735561, 58.823302641640886),
                            new GeoJson2DCoordinates(15.123593322039651, 58.82325882868679),
                            new GeoJson2DCoordinates(15.123601657735561, 58.82321501573269),
                            new GeoJson2DCoordinates(15.123626344487173, 58.82317288648585),
                            new GeoJson2DCoordinates(15.123666433596465, 58.82313405994959),
                            new GeoJson2DCoordinates(15.12372038446145, 58.82310002820593),
                            new GeoJson2DCoordinates(15.123786123780643, 58.823072099075695),
                            new GeoJson2DCoordinates(15.123861125228874, 58.82305134585969),
                            new GeoJson2DCoordinates(15.12394250654255, 58.82303856609238),
                            new GeoJson2DCoordinates(15.124027140283424, 58.823034250893045),
                            new GeoJson2DCoordinates(15.124111774024298, 58.82303856609238),
                            new GeoJson2DCoordinates(15.124193155337974, 58.82305134585969),
                            new GeoJson2DCoordinates(15.124268156786204, 58.823072099075695),
                            new GeoJson2DCoordinates(15.124333896105398, 58.82310002820593),
                            new GeoJson2DCoordinates(15.124387846970382, 58.82313405994959),
                            new GeoJson2DCoordinates(15.124427936079675, 58.82317288648585),
                            new GeoJson2DCoordinates(15.124452622831287, 58.82321501573269),
                            new GeoJson2DCoordinates(15.124460958527196, 58.82325882868679),
                        })
                    )
                ),
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

        public static IOperable<ArtportalenObservationVerbatim> HaveRandomValues(this IOperable<ArtportalenObservationVerbatim> operable)
        {
            var builder = ((IDeclaration<ArtportalenObservationVerbatim>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                //bool equalStartAndEndDate = _faker.Random.WeightedRandom(new bool[] { true, false }, new float[] { 0.9f, 0.1f }); 
                TimeSpan? obsTimeSpan = _faker.Random.Bool(0.9f) ? null : _faker.Date.Timespan(TimeSpan.FromHours(5)); // 90% probability of equal start and end date.
                DateTime startDate = _faker.Date.Past(10); // random date within 10 years
                DateTime endDate = obsTimeSpan == null ? startDate : startDate.Add(obsTimeSpan.Value);
                DateTime reportedDate = endDate.Add(_faker.Date.Timespan(TimeSpan.FromDays(5)));
                DateTime editDate = reportedDate;
                List<UserInternal>? verifiedByInternal = GetRandomUserInternals(_verifiersProbability);
                string? verifiedBy = verifiedByInternal == null ? null : string.Join(", ", verifiedByInternal.Select(m => m.UserAlias));
                List<UserInternal>? observersInternal = GetRandomUserInternals(_verifiersProbability);
                string? observers = observersInternal == null ? null : string.Join(", ", observersInternal.Select(m => m.UserAlias));
                UserInternal reportedByInternal = GetRandomUserInternal();

                obs.Id = _faker.IndexVariable++;
                obs.DatasourceId = ArtportalenDataSourceId;
                obs.DiscoveryMethod = new Metadata<int>(0); // todo
                obs.DeterminationMethod = new Metadata<int>(0); // todo
                obs.EditDate = editDate;
                obs.ReportedDate = reportedDate;
                obs.StartDate = startDate;
                obs.EndDate = endDate;
                obs.StartTime = startDate.TimeOfDay;
                obs.EndTime = endDate.TimeOfDay;
                obs.Gender = new Metadata<int>(0);
                obs.HasImages = false; // todo
                obs.FirstImageId = 0; // todo
                obs.HasTriggeredValidationRules = _faker.Random.Bool(); // todo
                obs.HasAnyTriggeredValidationRuleWithWarning = _faker.Random.Bool(); // todo
                obs.NoteOfInterest = _faker.Random.Bool(); // todo
                obs.HasUserComments = _faker.Random.Bool(); // todo
                obs.NotPresent = false; // _faker.Random.Bool(); // todo
                obs.NotRecovered = false; // _faker.Random.Bool(); // todo
                obs.ProtectedBySystem = false; // _faker.Random.Bool(); // todo
                obs.RightsHolder = _faker.Name.FullName();
                obs.VerifiedByInternal = verifiedByInternal;
                obs.VerifiedBy = verifiedBy;
                obs.ObserversInternal = observersInternal;
                obs.Observers = observers;
                obs.Site = GetTestSite(); // todo
                obs.Stage = new Metadata<int>(0); // todo
                obs.TaxonId = 231326; // todo
                obs.Unit = new Metadata<int>(0); // todo
                obs.Unspontaneous = false; // todo?
                obs.UnsureDetermination = false; // todo?
                obs.ValidationStatus = new Metadata<int>(10); // todo
                obs.ReportedBy = reportedByInternal.UserAlias; // todo
                obs.ReportedByUserId = reportedByInternal.Id;
                obs.ReportedByUserServiceUserId = reportedByInternal.UserServiceUserId;
                obs.ReportedByUserAlias = reportedByInternal.UserAlias;
                obs.DeterminedBy = _faker.Name.FullName();
                obs.SightingTypeSearchGroupId = 1; // todo
                obs.SpeciesFactsIds = new List<int> { 4, 1938 }; // todo

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
                //Weight
                //SightingBarcodeURL
                //SubstrateSpeciesDescription
            });

            return operable;
        }

        public static IOperable<ArtportalenObservationVerbatim> HaveValuesFromPredefinedObservations(this IOperable<ArtportalenObservationVerbatim> operable, bool sensitive = false)
        {
            var builder = ((IDeclaration<ArtportalenObservationVerbatim>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                var sourceObservation = Pick<ArtportalenObservationVerbatim>.RandomItemFrom(VerbatimArtportalenObservationsFromJsonFile(sensitive)).Clone();

                obs.Id = _faker.IndexVariable++;
                obs.NotPresent = false; // sourceObservation.NotPresent;
                obs.SightingTypeSearchGroupId = (int)SightingTypeSearchGroup.Ordinary; // sourceObservation.SightingTypeSearchGroupId;                
                obs.DatasourceId = ArtportalenDataSourceId;
                obs.SightingId = _faker.IndexVariable++;
                obs.DiscoveryMethod = sourceObservation.DiscoveryMethod;
                obs.DeterminationMethod = sourceObservation.DeterminationMethod;
                obs.EditDate = sourceObservation.EditDate;
                obs.ReportedDate = sourceObservation.ReportedDate;
                obs.StartDate = sourceObservation.StartDate;
                obs.EndDate = sourceObservation.EndDate;
                obs.Gender = sourceObservation.Gender;
                obs.HasImages = sourceObservation.HasImages;
                obs.FirstImageId = sourceObservation.FirstImageId;
                obs.HasTriggeredValidationRules = sourceObservation.HasTriggeredValidationRules;
                obs.HasAnyTriggeredValidationRuleWithWarning = sourceObservation.HasAnyTriggeredValidationRuleWithWarning;
                obs.NoteOfInterest = sourceObservation.NoteOfInterest;
                obs.HasUserComments = sourceObservation.HasUserComments;
                obs.NotRecovered = sourceObservation.NotRecovered;
                obs.ProtectedBySystem = sourceObservation.ProtectedBySystem;
                obs.RightsHolder = sourceObservation.RightsHolder;
                obs.VerifiedByInternal = sourceObservation.VerifiedByInternal;
                obs.VerifiedBy = sourceObservation.VerifiedBy;
                obs.ObserversInternal = sourceObservation.ObserversInternal;
                obs.Observers = sourceObservation.Observers;
                obs.Site = sourceObservation.Site;
                obs.Stage = sourceObservation.Stage;
                obs.TaxonId = sourceObservation.TaxonId;
                obs.Unit = sourceObservation.Unit;
                obs.Unspontaneous = sourceObservation.Unspontaneous;
                obs.UnsureDetermination = false;
                obs.ValidationStatus = sourceObservation.ValidationStatus;
                obs.ReportedBy = sourceObservation.ReportedBy;
                obs.ReportedByUserId = sourceObservation.ReportedByUserId;
                obs.ReportedByUserServiceUserId = sourceObservation.ReportedByUserServiceUserId;
                obs.ReportedByUserAlias = sourceObservation.ReportedByUserAlias;
                obs.DeterminedBy = sourceObservation.DeterminedBy;
                obs.SpeciesFactsIds = sourceObservation.SpeciesFactsIds;
                obs.Activity = sourceObservation.Activity;
                obs.Biotope = sourceObservation.Biotope;
                obs.BiotopeDescription = sourceObservation.BiotopeDescription;
                obs.CollectionID = sourceObservation.CollectionID;
                obs.Comment = sourceObservation.Comment;
                obs.ConfirmationYear = sourceObservation.ConfirmationYear;
                obs.ConfirmedBy = sourceObservation.ConfirmedBy;
                obs.DatasourceId = sourceObservation.DatasourceId;
                obs.DeterminationYear = sourceObservation.DeterminationYear;
                obs.StartTime = sourceObservation.StartTime;
                obs.EndTime = sourceObservation.EndTime;
                obs.HasUserComments = sourceObservation.HasUserComments;
                obs.HiddenByProvider = sourceObservation.HiddenByProvider;
                obs.Label = sourceObservation.Label;
                obs.Length = sourceObservation.Length;
                obs.MaxDepth = sourceObservation.MaxDepth;
                obs.MaxHeight = sourceObservation.MaxHeight;
                obs.Media = sourceObservation.Media;
                obs.MigrateSightingObsId = sourceObservation.MigrateSightingObsId;
                obs.MigrateSightingPortalId = sourceObservation.MigrateSightingPortalId;
                obs.MinDepth = sourceObservation.MinDepth;
                obs.MinHeight = sourceObservation.MinHeight;
                obs.OwnerOrganization = sourceObservation.OwnerOrganization;
                obs.PrivateCollection = sourceObservation.PrivateCollection;
                obs.Projects = sourceObservation.Projects;
                obs.PublicCollection = sourceObservation.PublicCollection;
                obs.Quantity = sourceObservation.Quantity;
                obs.QuantityOfSubstrate = sourceObservation.QuantityOfSubstrate;
                //obs.RegionalSightingStateId = sourceObservation.RegionalSightingStateId;
                obs.SightingPublishTypeIds = sourceObservation.SightingPublishTypeIds;
                obs.SightingSpeciesCollectionItemId = sourceObservation.SightingSpeciesCollectionItemId;
                obs.SightingTypeId = sourceObservation.SightingTypeId;
                obs.SightingTypeSearchGroupId = sourceObservation.SightingTypeSearchGroupId;
                obs.SpeciesCollection = sourceObservation.SpeciesCollection;
                obs.StartTime = sourceObservation.StartTime;
                obs.Substrate = sourceObservation.Substrate;
                obs.SubstrateDescription = sourceObservation.SubstrateDescription;
                obs.SubstrateSpeciesId = sourceObservation.SubstrateSpeciesId;
                obs.Weight = sourceObservation.Weight;
                obs.SightingBarcodeURL = sourceObservation.SightingBarcodeURL;
                obs.SubstrateSpeciesDescription = sourceObservation.SubstrateSpeciesDescription;
                obs.TriggeredObservationRuleFrequencyId = sourceObservation.TriggeredObservationRuleFrequencyId;
                obs.TriggeredObservationRuleReproductionId = sourceObservation.TriggeredObservationRuleReproductionId;
                obs.TriggeredObservationRuleUnspontaneous = sourceObservation.TriggeredObservationRuleUnspontaneous;
            });

            return operable;
        }

        public static IOperable<ArtportalenObservationVerbatim> HaveRedlistedTaxonId(this IOperable<ArtportalenObservationVerbatim> operable, string? category)
        {
            var builder = ((IDeclaration<ArtportalenObservationVerbatim>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                obs.TaxonId = Pick<int>.RandomItemFrom(ProtectedSpeciesHelper.RedlistedTaxonIdsByCategory[category ?? ""]);
            });

            return operable;
        }

        public static IOperable<ArtportalenObservationVerbatim> HaveTaxonCategoryTaxonId(this IOperable<ArtportalenObservationVerbatim> operable, int categoryId)
        {
            var builder = ((IDeclaration<ArtportalenObservationVerbatim>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                obs.TaxonId = Pick<int>.RandomItemFrom(ProtectedSpeciesHelper.TaxonByCategory[categoryId]);
            });

            return operable;
        }

        public static IOperable<ArtportalenObservationVerbatim> HaveAreaFeatureIds(this IOperable<ArtportalenObservationVerbatim> operable,
            string provinceFeatureId,
            string countyFeatureId,
            string municipalityFeatureId)
        {
            var builder = ((IDeclaration<ArtportalenObservationVerbatim>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                obs.Site.Province.FeatureId = provinceFeatureId;
                obs.Site.County.FeatureId = countyFeatureId;
                obs.Site.Municipality.FeatureId = municipalityFeatureId;
            });

            return operable;
        }

        public static IOperable<ArtportalenObservationVerbatim> HaveCoordinatesInSpan(this IOperable<ArtportalenObservationVerbatim> operable,
           double longitudeMin,
           double longitudeMax,
           double latitudeMin,
           double latitudeMax,
           int accuracy)
        {
            var builder = ((IDeclaration<ArtportalenObservationVerbatim>)operable).ObjectBuilder;
            var random = new Random();
            var precision = 1000000;
            builder.With((obs, index) =>
            {
                var longitude = (double)random.Next((int)(longitudeMin * precision), (int)(longitudeMax * precision)) / precision;
                var latitude = (double)random.Next((int)(latitudeMin * precision), (int)(latitudeMax * precision)) / precision;

                Point? wgs84Point = null;
                if (longitude > 0 && latitude > 0)
                {
                    wgs84Point = new Point(longitude, latitude);
                }

                obs.Site.Point = wgs84Point?.ToGeoJson();
                obs.Site.PointWithBuffer = wgs84Point.ToCircle(accuracy)?.ToGeoJson();
                obs.Site.Accuracy = accuracy;

            });
            return operable;
        }

        public static IOperable<ArtportalenObservationVerbatim> HaveProjectInformation(
            this IOperable<ArtportalenObservationVerbatim> operable, bool sensitive = false)
        {
            var builder = ((IDeclaration<ArtportalenObservationVerbatim>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                foreach (var verbatimObservation in VerbatimArtportalenObservationsFromJsonFile(sensitive)!)
                {
                    if (verbatimObservation.Projects != null && verbatimObservation.Projects.Any())
                    {
                        var firstProject = verbatimObservation.Projects.First();

                        if (firstProject.ProjectParameters != null && firstProject.ProjectParameters.Count > 0)
                        {
                            obs.Projects = verbatimObservation.Projects;
                            return;
                        }
                    }
                }
            });

            return operable;
        }

        public static IOperable<ArtportalenObservationVerbatim> HaveMediaInformation(
            this IOperable<ArtportalenObservationVerbatim> operable, bool sensitive = false)
        {
            var builder = ((IDeclaration<ArtportalenObservationVerbatim>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                foreach (var verbatimObservation in VerbatimArtportalenObservationsFromJsonFile(sensitive)!)
                {
                    if (verbatimObservation.Media != null && verbatimObservation.Media.Count() > 1)
                    {
                        obs.Media = verbatimObservation.Media;
                        return;
                    }
                }
            });

            return operable;
        }

        public static IOperable<ArtportalenObservationVerbatim> IsInDateSpan(this IOperable<ArtportalenObservationVerbatim> operable, DateTime spanStartDate, DateTime spanEndDate)
        {
            var randomTest = new Random();
            var startMinutes = (int)(spanEndDate - spanStartDate).TotalMinutes;

            var builder = ((IDeclaration<ArtportalenObservationVerbatim>)operable).ObjectBuilder;

            builder.With((obs, index) =>
            {
                var startDate = spanStartDate.AddMinutes(randomTest.Next(1, startMinutes));
                obs.StartDate = startDate;
                obs.StartTime = obs.StartDate.Value.TimeOfDay;
                var endMinutes = (int)(spanEndDate - startDate).TotalMinutes;
                obs.EndDate = startDate.AddMinutes(randomTest.Next(0, endMinutes));
                obs.EndTime = obs.EndDate.Value.TimeOfDay;
            });

            return operable;
        }

        public static IOperable<ArtportalenObservationVerbatim> IsInDateSpan(this IOperable<ArtportalenObservationVerbatim> operable, string strSpanStartDate, string strSpanEndDate)
        {
            var spanStartDate = DateTime.SpecifyKind(DateTime.Parse(strSpanStartDate), DateTimeKind.Local);
            var spanEndDate = DateTime.SpecifyKind(DateTime.Parse(strSpanEndDate), DateTimeKind.Local);

            return operable.IsInDateSpan(spanStartDate, spanEndDate);
        }

        public static IOperable<ArtportalenObservationVerbatim> HaveTaxonSensitivityCategory(this IOperable<ArtportalenObservationVerbatim> operable, int sensitivityCategory)
        {
            var builder = ((IDeclaration<ArtportalenObservationVerbatim>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                obs.TaxonId = Pick<int>.RandomItemFrom(ProtectedSpeciesHelper.SensitiveSpeciesByCategory[sensitivityCategory]);
                obs.ProtectedBySystem = true;
            });

            return operable;
        }

        public static IOperable<ArtportalenObservationVerbatim> HaveObserversWithId(this IOperable<ArtportalenObservationVerbatim> operable, params int[] userIds)
        {
            var builder = ((IDeclaration<ArtportalenObservationVerbatim>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                obs.ObserversInternal =
                    userIds.Select(userId => new UserInternal { Id = userId, PersonId = userId, UserServiceUserId = userId });
            });

            return operable;
        }

        public static IOperable<ArtportalenObservationVerbatim> HaveProperties(this IOperable<ArtportalenObservationVerbatim> operable,
            params ObservationProperties[] properties)
        {
            var builder = ((IDeclaration<ArtportalenObservationVerbatim>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                var item = properties[index];

                obs.ObserversInternal = new List<UserInternal> { new() { Id = item.UserId, UserServiceUserId = item.UserId } };
                obs.TaxonId = item.TaxonId;
                if (!string.IsNullOrEmpty(item.ProvinceId))
                {
                    obs.Site.Province.FeatureId = item.ProvinceId;
                }
                if (!string.IsNullOrEmpty(item.MunicipalityId))
                {
                    obs.Site.Municipality.FeatureId = item.MunicipalityId;
                }
            });

            return operable;
        }

        public static IOperable<ArtportalenObservationVerbatim> HaveProperties(this IOperable<ArtportalenObservationVerbatim> operable,
            int userId,
            params ObservationProperties[] properties)
        {
            var builder = ((IDeclaration<ArtportalenObservationVerbatim>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                var item = properties[index];

                obs.ObserversInternal = new List<UserInternal> { new() { Id = userId, UserServiceUserId = userId } };
                obs.TaxonId = item.TaxonId;

                if (!string.IsNullOrEmpty(item.ProvinceId))
                {
                    obs.Site.Province.FeatureId = item.ProvinceId;
                }
                if (!string.IsNullOrEmpty(item.MunicipalityId))
                {
                    obs.Site.Municipality.FeatureId = item.MunicipalityId;
                }
            });

            return operable;
        }

        public static IOperable<ArtportalenObservationVerbatim> HaveUniqueUserId(this IOperable<ArtportalenObservationVerbatim> operable)
        {
            var builder = ((IDeclaration<ArtportalenObservationVerbatim>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                obs.ObserversInternal = new List<UserInternal> { new() { Id = index, UserServiceUserId = index } };
            });

            return operable;
        }
    }

    public class ObservationProperties
    {
        public int UserId { get; set; }
        public int TaxonId { get; set; }
        public string? ProvinceId { get; set; }
        public string? MunicipalityId { get; set; }
    }
}