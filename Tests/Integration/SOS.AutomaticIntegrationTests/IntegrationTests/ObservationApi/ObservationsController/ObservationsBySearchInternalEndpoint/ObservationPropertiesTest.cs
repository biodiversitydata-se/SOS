using System;
using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;
using System.Collections.Generic;
using SOS.Lib.Models.Shared;
using SOS.Lib.Extensions;
using Nest;
using System.Linq;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.ObservationsBySearchInternalEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class ObservationPropertiesTest
    {
        private readonly IntegrationTestFixture _fixture;
        private static Bogus.Faker _faker = new Bogus.Faker();

        public ObservationPropertiesTest(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task GetObservationsWithOccurrenceRemarks()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()                    
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var observation = new Observation
            {
                AccessRights = new VocabularyValue { Id = 0, Value = "Public" },
                ArtportalenInternal = new ArtportalenInternal
                {
                    BirdValidationAreaIds = new[] { "1", "2" },
                    ChecklistId = 1,
                    ConfirmationYear = DateTime.Now.Year,
                    DatasourceId = 1,
                    DeterminationYear = DateTime.Now.Year,
                    DiffusionId = 3,
                    EventMonths = new[] { 3,4,5 },
                    FieldDiaryGroupId = 3,
                    HasAnyTriggeredVerificationRuleWithWarning = true,
                    HasTriggeredVerificationRules = true,
                    HasUserComments = true,
                    IncludedByLocationId = 3,
                    IncrementalHarvested = true,
                    LocationPresentationNameParishRegion = "Socken",
                    NoteOfInterest = true,
                    OccurrenceRecordedByInternal = new[] {
                        new UserInternal { 
                            Discover = true,  
                            Id = 3,
                            PersonId = 3,
                            UserAlias = "Alias",
                            UserServiceUserId = 3,
                            ViewAccess = true
                        } 
                    },
                    OccurrenceVerifiedByInternal = new[] {
                        new UserInternal {
                            Discover = true,
                            Id = 3,
                            PersonId = 3,
                            UserAlias = "Alias",
                            UserServiceUserId = 3,
                            ViewAccess = true
                        }
                    },
                    ParentLocality = "Huvudlokal",
                    ParentLocationId = 3,
                    ReportedByUserAlias = "Alias",
                    ReportedByUserId = 3,
                    ReportedByUserServiceUserId = 3,
                    SecondHandInformation = true,
                    SightingBarcodeURL = "https://barcodeurl.se",
                    SightingId = 3,
                    SightingPublishTypeIds = new[] {1,2,3},
                    SightingSpeciesCollectionItemId = 3,
                    SightingTypeId = 1,
                    SightingTypeSearchGroupId = 1,
                    SpeciesFactsIds = new[] {1,2,3},
                    TriggeredObservationRuleFrequencyId = 3,
                    TriggeredObservationRuleReproductionId = 3
                },
                BasisOfRecord = new VocabularyValue { Id = 1, Value = "BOR"},
                BibliographicCitation = "BC",
                CollectionCode = "CC",
                CollectionId = "CId",
                Created = DateTime.Now,
                DataGeneralizations = "DG",
                DataProviderId = 1,
                DataQuality = new DataQuality { UniqueKey = "UK" },
                DatasetId = "DSId",
                DatasetName = "Test",
                DataStewardshipDatasetId = "DSD",
                DiffusionStatus = DiffusionStatus.NotDiffused,
                DynamicProperties = "DP",
                Event = new Event
                {
                    DiscoveryMethod = new VocabularyValue { Id = 1, Value = "DM" },
                    EndDate = DateTime.Now.AddHours(-1),
                    EndDay = DateTime.Now.Day,
                    EndMonth = DateTime.Now.Month,
                    EndYear = DateTime.Now.Year,
                    EventId = "EId",
                    EventRemarks = "Remarks",
                    FieldNotes = "Notes",
                    FieldNumber = "1000",
                    Habitat = "Habitat",
                    MeasurementOrFacts = new List<ExtendedMeasurementOrFact>{ 
                        new ExtendedMeasurementOrFact
                        {
                            MeasurementAccuracy = "MA",
                            MeasurementDeterminedBy = "MDB",
                            MeasurementDeterminedDate = "MDD",
                            MeasurementID = "MId",
                            MeasurementMethod = "MM",
                            MeasurementRemarks = "MR",
                            MeasurementType = "MT",
                            MeasurementTypeID = "MTId",
                            MeasurementUnit = "MU",
                            MeasurementUnitID = "MUId",
                            MeasurementValue = "MV",
                            MeasurementValueID = "MVId",
                            OccurrenceID = "OId"
                        } 
                    },
                    Media = new List<Multimedia> { 
                        new Multimedia {
                            Audience = "A",
                            Comments = new List<MultimediaComment>{ new MultimediaComment { Comment = "C", CommentBy = "CB", Created = DateTime.Now.AddHours(-1) } },
                            Contributor = "CR",
                            Created = "C",
                            Creator = "CRR",
                            DatasetID = "DId",
                            Description = "D",
                            Format = "F",
                            Identifier = "I",
                            License = "L",
                            Publisher = "P",
                            References = "R",
                            RightsHolder = "RH",
                            Source = "S",
                            Title = "T",
                            Type = "TY"
                        } 
                    },
                    ParentEventId = "PEId",
                    PlainEndDate = "PED",
                    PlainEndTime = "PET",
                    PlainStartDate = "PSD",
                    PlainStartTime = "PST",
                    SampleSizeUnit = "SSU",
                    SampleSizeValue = "SSV",
                    SamplingEffort = "SE",
                    SamplingProtocol = "SP",
                    StartDate = DateTime.Now.AddHours(-3),
                    StartDay = DateTime.Now.Day,
                    StartMonth = DateTime.Now.Month,
                    StartYear = DateTime.Now.Year,
                    VerbatimEventDate = "VED"
                },
                GeologicalContext = new GeologicalContext
                {
                    Bed = "B",
                    EarliestAgeOrLowestStage = "EAOLS",
                    EarliestEonOrLowestEonothem = "EEOLE",
                    EarliestEpochOrLowestSeries = "EEOLS",
                    EarliestEraOrLowestErathem = "EEOLE",
                    EarliestGeochronologicalEra = "EGE",
                    EarliestPeriodOrLowestSystem = "EPOLS",
                    Formation = "F",
                    GeologicalContextId = "GCI",
                    Group = "G",
                    HighestBiostratigraphicZone = "HBZ",
                    LatestAgeOrHighestStage = "LAOHS",
                    LatestEonOrHighestEonothem = "LEOHE",
                    LatestEpochOrHighestSeries = "LEOHS",
                    LatestEraOrHighestErathem = "LEOHE",
                    LatestGeochronologicalEra = "LGE",
                    LatestPeriodOrHighestSystem = "LPOHS",
                    LithostratigraphicTerms = "LT",
                    LowestBiostratigraphicZone = "LBZ",
                    Member = "M"
                },
                Id = "Id",
                Identification = new Identification
                {
                    ConfirmedBy = "CB",
                    ConfirmedDate = "CD",
                    DateIdentified = "DI",
                    DeterminationMethod = new VocabularyValue { Id = 1, Value = "DM" },
                    IdentificationId = "IId",
                    IdentificationQualifier = "IQ",
                    IdentificationReferences = "IR",
                    IdentificationRemarks = "IRS",
                    IdentifiedBy = "IB",
                    TypeStatus = "TS",
                    UncertainIdentification = true,
                    VerificationStatus = new VocabularyValue { Id = 1, Value = "VS" },
                    Verified = true,
                    VerifiedBy = "VB"
                },
                InformationWithheld = "IW",
                InstitutionCode = new VocabularyValue { Id = 3, Value = "IC"},
                InstitutionId = "IId",
                Language = "L",
                License = "LI",
                Location = new Location
                {
                    Attributes = new LocationAttributes
                    {
                        CountyPartIdByCoordinate = "CPIBC",
                        ExternalId = "EId",
                        ProjectId = 3,
                        ProvincePartIdByCoordinate = "PPIBC",
                        VerbatimMunicipality = "VM",
                        VerbatimProvince = "VP"
                    },
                    Continent = new VocabularyValue { Id = 1, Value = "C" },
                    CoordinatePrecision = 1.2,
                    CoordinateUncertaintyInMeters = 100,
                    Country = new VocabularyValue { Id = 2, Value = "C" },
                    CountryCode = "CC",
                    CountryRegion = new Lib.Models.Processed.Observation.Area { FeatureId = "FId", Name = "CR" },
                    County = new Lib.Models.Processed.Observation.Area { FeatureId = "FId", Name = "C" },
                    DecimalLatitude = 59.86035103929389,
                    DecimalLongitude = 17.577532793729183,
                    Etrs89X = 4745721.87522997,
                    Etrs89Y = 4107123.6189067164,
                    FootprintSpatialFit = "FSF",
                    FootprintSRS = "FS",
                    FootprintWKT = "FW",
                    GeodeticDatum = "EPSG:4326",
                    GeoreferencedBy = "GB",
                    GeoreferencedDate = "GD",
                    GeoreferenceProtocol = "GP",
                    GeoreferenceRemarks = "GR",
                    GeoreferenceSources = "GS",
                    GeoreferenceVerificationStatus = "GVS",
                    HigherGeography = "HG",
                    HigherGeographyId = "HGId",
                    IsInEconomicZoneOfSweden = false,
                    Island = "I",
                    IslandGroup = "IG",
                    Locality = "L",
                    LocationAccordingTo = "LAT",
                    LocationId = "LId",
                    LocationRemarks = "LR",
                    MaximumDepthInMeters = 0.1,
                    MaximumDistanceAboveSurfaceInMeters = 10,
                    MaximumElevationInMeters = 12,
                    MinimumDepthInMeters = 2,
                    MinimumDistanceAboveSurfaceInMeters = 3,
                    MinimumElevationInMeters = 9,
                    Municipality = new Lib.Models.Processed.Observation.Area { FeatureId = "FId", Name = "M" },
                    Parish = new Lib.Models.Processed.Observation.Area { FeatureId = "FId", Name = "P" },
                    Point = new PointGeoShape(new GeoCoordinate(59.86035103929389, 17.577532793729183)),
                    PointLocation = new GeoLocation(59.86035103929389, 17.577532793729183),
                    PointRadiusSpatialFit = "PRSF",
                    PointWithBuffer = new NetTopologySuite.Geometries.Point(17.577532793729183, 59.86035103929389).ToCircle(10).ToGeoShape() as PolygonGeoShape,
                    PointWithDisturbanceBuffer = new NetTopologySuite.Geometries.Point(17.577532793729183, 59.86035103929389).ToCircle(15).ToGeoShape() as PolygonGeoShape,
                    Province = new Lib.Models.Processed.Observation.Area { FeatureId = "FId", Name = "P" },
                    Sweref99TmX = 180,
                    Sweref99TmY = 90,
                    Type = Lib.Enums.LocationType.Point,
                    VerbatimCoordinates = "VC",
                    VerbatimCoordinateSystem = "VCS",
                    VerbatimDepth = "VD",
                    VerbatimElevation = "VE",
                    VerbatimLatitude = "VL",
                    VerbatimLocality = "VLO",
                    VerbatimLongitude = "VLT",
                    VerbatimSRS = "VS",
                    WaterBody = "WB"
                },
                MaterialSample = new MaterialSample
                {
                    MaterialSampleId = "MSId"
                },
                MeasurementOrFacts = new List<ExtendedMeasurementOrFact> { 
                    new ExtendedMeasurementOrFact
                    {
                        MeasurementAccuracy = "MA",
                        MeasurementDeterminedBy = "MDB",
                        MeasurementDeterminedDate = "MDD",
                        MeasurementID = "MId",
                        MeasurementMethod = "MM",
                        MeasurementRemarks = "MR",
                        MeasurementType = "MT",
                        MeasurementTypeID = "MTI",
                        MeasurementUnit = "MU",
                        MeasurementUnitID = "MUId",
                        MeasurementValue = "MV",
                        MeasurementValueID = "MVId",
                        OccurrenceID = "OId"
                    }
                },
                Modified = DateTime.Now,
                Occurrence = new Occurrence
                {
                    Activity = new VocabularyValue { Id = 1, Value = "A" },
                    AssociatedMedia = "AM",
                    AssociatedOccurrences = "AO",
                    AssociatedReferences = "AR",
                    AssociatedSequences = "AS",
                    AssociatedTaxa = "AT",
                    Behavior = new VocabularyValue { Id = 1, Value = "B" },
                    Biotope = new VocabularyValue { Id = 1, Value = "B"},
                    BiotopeDescription = "BD",
                    BirdNestActivityId = 1,
                    CatalogId = 2,
                    CatalogNumber = "CN",
                    Disposition = "D",
                    EstablishmentMeans = new VocabularyValue { Id = 1, Value = "EM" },
                    IndividualCount = "IC",
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = false,
                    IsNotRediscoveredObservation = true,
                    IsPositiveObservation = true,
                    Length = 12,
                    LifeStage = new VocabularyValue { Id = 1, Value = "LS" },
                    Media = new List<Multimedia> {
                        new Multimedia {
                            Audience = "A",
                            Comments = new List<MultimediaComment>{ new MultimediaComment { Comment = "C", CommentBy = "CB", Created = DateTime.Now.AddHours(-1) } },
                            Contributor = "CR",
                            Created = "C",
                            Creator = "CRR",
                            DatasetID = "DId",
                            Description = "D",
                            Format = "F",
                            Identifier = "I",
                            License = "L",
                            Publisher = "P",
                            References = "R",
                            RightsHolder = "RH",
                            Source = "S",
                            Title = "T",
                            Type = "TY"
                        }
                    },
                    OccurrenceId = "OId",
                    OccurrenceRemarks = "OR",
                    OccurrenceStatus = new VocabularyValue { Id = 1, Value = "S" },
                    OrganismQuantity = "OQ",
                    OrganismQuantityAggregation = 12,
                    OrganismQuantityInt = 1,
                    OrganismQuantityUnit = new VocabularyValue { Id = 1, Value = "OQU" },
                    OtherCatalogNumbers = "OCN",
                    Preparations = "P",
                    RecordedBy = "RB",
                    RecordNumber = "RN",
                    ReportedBy = "RB",
                    ReportedDate = DateTime.Now,
                    ReproductiveCondition = new VocabularyValue { Id = 1, Value = "RC" },
                    SensitivityCategory = 1,
                    Sex = new VocabularyValue { Id = 1, Value = "S" },
                    Substrate = new Substrate
                    {
                        Description = "D",
                        Id = 1,
                        Name = new VocabularyValue { Id = 1, Value = "N" },
                        Quantity = 3,
                        SpeciesDescription = "SD",
                        SpeciesId = 1,
                        SpeciesScientificName = "SSN",
                        SpeciesVernacularName = "SVN",
                        SubstrateDescription = "SD"
                    },
                    Url = "U",
                    Weight = 3
                },
                Organism = new Organism
                {
                    AssociatedOrganisms = "AO",
                    OrganismId = "OId",
                    OrganismName = "ON",
                    OrganismRemarks = "OR",
                    OrganismScope = "OS",
                    PreviousIdentifications = "PI"
                },
                OwnerInstitutionCode = "OIC",
                PrivateCollection = "PC",
                Projects = new[]
                {
                    new Lib.Models.Processed.Observation.Project
                    {
                        Category = "C",
                        CategorySwedish = "CS",
                        Description = "D",
                        EndDate = DateTime.Now.AddDays(1),
                        Id = 12,
                        IsPublic = true,
                        Name = "N",
                        Owner = "O",
                        ProjectParameters = new []{ new Lib.Models.Processed.Observation.ProjectParameter
                            {
                                DataType = "DT",
                                Description = "D",
                                Id = 1,
                                Name = "N",
                                Unit = "U",
                                Value = "V"
                            } 
                        },
                        ProjectURL = "PU",
                        StartDate = DateTime.Now.AddDays(-100),
                        SurveyMethod = "SM",
                        SurveyMethodUrl = "SMU"
                    }
                },
                ProjectsSummary = new ProjectsSummary
                {
                    Project1Category = "C",
                    Project1Id = 12,
                    Project1Name = "N",
                    Project1Url = "PU",
                    Project1Values = "D: V"
                },
                PublicCollection = "PC",
                References = "R",
                RightsHolder = "RH",
                Sensitive = false,
                SpeciesCollectionLabel = "SCL",
                Taxon = new Taxon
                {
                    AcceptedNameUsage = "ANU",
                    AcceptedNameUsageId = "ANUId",
                    Attributes = new TaxonAttributes
                    {
                        ActionPlan = "AP",
                        DisturbanceRadius = 1,
                        DyntaxaTaxonId = 3,
                        GbifTaxonId = 4,
                        InvasiveRiskAssessmentCategory = "IRAC",
                        IsInvasiveAccordingToEuRegulation = true,
                        IsInvasiveInSweden = true,
                        Natura2000HabitatsDirectiveArticle2 = true,
                        Natura2000HabitatsDirectiveArticle4 = true,
                        Natura2000HabitatsDirectiveArticle5 = true,
                        OrganismGroup = "OG",
                        ParentDyntaxaTaxonId = 1,
                        ProtectedByLaw = true,
                        RedlistCategory = "RLC",
                        RedlistCategoryDerived = "RLCD",
                        SensitivityCategory = new VocabularyValue { Id = 1, Value = "PL" },
                        SortOrder = 3,
                        SpeciesGroup = Lib.Enums.SpeciesGroup.Bats,
                        SwedishHistory = "SH",
                        SwedishOccurrence = "SO",
                        Synonyms = new[] { new TaxonSynonymName
                            {
                                Author = "A",
                                Name = "N",
                                NomenclaturalStatus = "NS",
                                TaxonomicStatus = "TS"
                            } 
                        },
                       TaxonCategory = new VocabularyValue { Id = 3, Value = "TC" },
                       VernacularNames = new[]
                       {
                           new TaxonVernacularName
                           {
                               CountryCode = "CC",
                               IsPreferredName = true,
                               Language = "L",
                               Name = "N",
                               ValidForSighting = true
                           }
                       }

                    },
                    BirdDirective = true,
                    Class = "C",
                    Family = "F",
                    Genus = "G",
                    HigherClassification = "HC",
                    Id = 206123,
                    InfraspecificEpithet = "IE",
                    Kingdom = "K",
                    NameAccordingTo = "NAT",
                    NameAccordingToId = "NATId",
                    NamePublishedIn = "NPI",
                    NamePublishedInId = "NPIId",
                    NamePublishedInYear = "NPIY",
                    NomenclaturalCode = "NC",
                    NomenclaturalStatus = "NS",
                    Order = "O",
                    OriginalNameUsage = "ONU",
                    OriginalNameUsageId = "ONUId",
                    ParentNameUsage = "PNU",
                    ParentNameUsageId = "PNUI",
                    Phylum = "P",
                    ScientificName = "SN",
                    ScientificNameAuthorship = "SNA",
                    ScientificNameId = "SNId",
                    SecondaryParentDyntaxaTaxonIds = new[] { 3123 },
                    SpecificEpithet = "SE",
                    Subgenus = "S",
                    TaxonConceptId = "TCId",
                    TaxonId = "206123",
                    TaxonomicStatus = "TS",
                    TaxonRank = "TR",
                    TaxonRemarks = "TR",
                    VerbatimId = "VId",
                    VerbatimTaxonRank = "VTR",
                    VernacularName = "VN"
                },
                Type = new VocabularyValue { Id = 1, Value = "T"}
            };

            await _fixture.AddObservationsToElasticsearchAsync(new[]
            {
               observation
            }, false);

            var searchFilter = new SearchFilterInternalDto
            {
                Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.All },
                Taxon = new TaxonFilterDto
                {
                    Ids = new[] { 206123 }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(1);
            var obs = result.Records.First();
            obs.AccessRights.Should().Be(observation.AccessRights);
            obs.ArtportalenInternal.ActivityCategoryId.Should().Be(observation.ArtportalenInternal.ActivityCategoryId);
            obs.ArtportalenInternal.BirdValidationAreaIds.First().Should().Be(observation.ArtportalenInternal.BirdValidationAreaIds.First());
            obs.ArtportalenInternal.ChecklistId.Should().Be(observation.ArtportalenInternal.ChecklistId);
            obs.ArtportalenInternal.ConfirmationYear.Should().Be(observation.ArtportalenInternal.ConfirmationYear);
            obs.ArtportalenInternal.DatasourceId.Should().Be(observation.ArtportalenInternal.DatasourceId);
            obs.ArtportalenInternal.DeterminationYear.Should().Be(observation.ArtportalenInternal.DeterminationYear);
            obs.ArtportalenInternal.DiffusionId.Should().Be(observation.ArtportalenInternal.DiffusionId);
            obs.ArtportalenInternal.EventMonths.First().Should().Be(observation.ArtportalenInternal.EventMonths.First());
            obs.ArtportalenInternal.FieldDiaryGroupId.Should().Be(observation.ArtportalenInternal.FieldDiaryGroupId);
            obs.ArtportalenInternal.HasAnyTriggeredVerificationRuleWithWarning.Should().Be(observation.ArtportalenInternal.HasAnyTriggeredVerificationRuleWithWarning);
            obs.ArtportalenInternal.HasTriggeredVerificationRules.Should().Be(observation.ArtportalenInternal.HasTriggeredVerificationRules);
            obs.ArtportalenInternal.HasUserComments.Should().Be(observation.ArtportalenInternal.HasUserComments);
            obs.ArtportalenInternal.IncludedByLocationId.Should().Be(observation.ArtportalenInternal.IncludedByLocationId);
            obs.ArtportalenInternal.IncrementalHarvested.Should().Be(observation.ArtportalenInternal.IncrementalHarvested);
            obs.ArtportalenInternal.LocationPresentationNameParishRegion.Should().Be(observation.ArtportalenInternal.LocationPresentationNameParishRegion);
            obs.ArtportalenInternal.NoteOfInterest.Should().Be(observation.ArtportalenInternal.NoteOfInterest);
            obs.ArtportalenInternal.OccurrenceRecordedByInternal.First().UserAlias.Should().Be(observation.ArtportalenInternal.OccurrenceRecordedByInternal.First().UserAlias);
            obs.ArtportalenInternal.OccurrenceVerifiedByInternal.First().Id.Should().Be(observation.ArtportalenInternal.OccurrenceVerifiedByInternal.First().Id);
            obs.ArtportalenInternal.ParentLocality.Should().Be(observation.ArtportalenInternal.ParentLocality);
            obs.ArtportalenInternal.ParentLocationId.Should().Be(observation.ArtportalenInternal.ParentLocationId);
            obs.ArtportalenInternal.ReportedByUserAlias.Should().Be(observation.ArtportalenInternal.ReportedByUserAlias);
            obs.ArtportalenInternal.ReportedByUserId.Should().Be(observation.ArtportalenInternal.ReportedByUserId);
            obs.ArtportalenInternal.ReportedByUserServiceUserId.Should().Be(observation.ArtportalenInternal.ReportedByUserServiceUserId);
            obs.ArtportalenInternal.SecondHandInformation.Should().Be(observation.ArtportalenInternal.SecondHandInformation);
            obs.ArtportalenInternal.SightingBarcodeURL.Should().Be(observation.ArtportalenInternal.SightingBarcodeURL);
            obs.ArtportalenInternal.SightingId.Should().Be(observation.ArtportalenInternal.SightingId);
            obs.ArtportalenInternal.SightingPublishTypeIds.First().Should().Be(observation.ArtportalenInternal.SightingPublishTypeIds.First());
            obs.ArtportalenInternal.SightingSpeciesCollectionItemId.Should().Be(observation.ArtportalenInternal.SightingSpeciesCollectionItemId);
            obs.ArtportalenInternal.SightingTypeId.Should().Be(observation.ArtportalenInternal.SightingTypeId);
            obs.ArtportalenInternal.SpeciesFactsIds.First().Should().Be(observation.ArtportalenInternal.SpeciesFactsIds.First());
            obs.ArtportalenInternal.TriggeredObservationRuleFrequencyId.Should().Be(observation.ArtportalenInternal.TriggeredObservationRuleFrequencyId);
            obs.ArtportalenInternal.TriggeredObservationRuleReproductionId.Should().Be(observation.ArtportalenInternal.TriggeredObservationRuleReproductionId);
            obs.ArtportalenInternal.TriggeredObservationRuleUnspontaneous.Should().Be(observation.ArtportalenInternal.TriggeredObservationRuleUnspontaneous);

            obs.BasisOfRecord.Should().Be(observation.BasisOfRecord);
            obs.BibliographicCitation.Should().Be(observation.BibliographicCitation);
            obs.CollectionCode.Should().Be(observation.CollectionCode);
            obs.CollectionId.Should().Be(observation.CollectionId); 
            obs.Created.Should().Be(observation.Created);
            obs.DataGeneralizations.Should().Be(observation.DataGeneralizations);
            obs.DataProviderId.Should().Be(observation.DataProviderId);
            obs.DatasetId.Should().Be(observation.DatasetId);
            obs.DatasetName.Should().Be(observation.DatasetName);
            obs.DataStewardshipDatasetId.Should().Be(observation.DataStewardshipDatasetId);
            obs.DiffusionStatus.Should().Be(observation.DiffusionStatus);
            obs.DynamicProperties.Should().Be(observation.DynamicProperties);
            
            obs.Event.DiscoveryMethod.Should().Be(observation.Event.DiscoveryMethod);
            obs.Event.EndDate.Should().Be(observation.Event.EndDate);
            obs.Event.EndDay.Should().Be(observation.Event.EndDay);
            obs.Event.EndMonth.Should().Be(observation.Event.EndMonth);
            obs.Event.EndYear.Should().Be(observation.Event.EndYear);
            obs.Event.EventId.Should().Be(observation.Event.EventId);
            obs.Event.EventRemarks.Should().Be(observation.Event.EventRemarks);
            obs.Event.FieldNotes.Should().Be(observation.Event.FieldNotes);
            obs.Event.FieldNumber.Should().Be(observation.Event.FieldNumber);
            obs.Event.Habitat.Should().Be(observation.Event.Habitat);
            
            obs.Event.MeasurementOrFacts.First().MeasurementAccuracy.Should().Be(observation.Event.MeasurementOrFacts.First().MeasurementAccuracy);
            obs.Event.MeasurementOrFacts.First().MeasurementDeterminedBy.Should().Be(observation.Event.MeasurementOrFacts.First().MeasurementDeterminedBy);
            obs.Event.MeasurementOrFacts.First().MeasurementDeterminedDate.Should().Be(observation.Event.MeasurementOrFacts.First().MeasurementDeterminedDate);
            obs.Event.MeasurementOrFacts.First().MeasurementID.Should().Be(observation.Event.MeasurementOrFacts.First().MeasurementID);
            obs.Event.MeasurementOrFacts.First().MeasurementMethod.Should().Be(observation.Event.MeasurementOrFacts.First().MeasurementMethod);
            obs.Event.MeasurementOrFacts.First().MeasurementRemarks.Should().Be(observation.Event.MeasurementOrFacts.First().MeasurementRemarks);
            obs.Event.MeasurementOrFacts.First().MeasurementType.Should().Be(observation.Event.MeasurementOrFacts.First().MeasurementType);
            obs.Event.MeasurementOrFacts.First().MeasurementTypeID.Should().Be(observation.Event.MeasurementOrFacts.First().MeasurementTypeID);
            obs.Event.MeasurementOrFacts.First().MeasurementUnit.Should().Be(observation.Event.MeasurementOrFacts.First().MeasurementUnit);
            obs.Event.MeasurementOrFacts.First().MeasurementUnitID.Should().Be(observation.Event.MeasurementOrFacts.First().MeasurementUnitID);
            obs.Event.MeasurementOrFacts.First().MeasurementValue.Should().Be(observation.Event.MeasurementOrFacts.First().MeasurementValue);
            obs.Event.MeasurementOrFacts.First().MeasurementValueID.Should().Be(observation.Event.MeasurementOrFacts.First().MeasurementValueID);
            obs.Event.MeasurementOrFacts.First().OccurrenceID.Should().Be(observation.Event.MeasurementOrFacts.First().OccurrenceID);

            obs.Event.Media.First().Audience.Should().Be(observation.Event.Media.First().Audience);
            obs.Event.Media.First().Comments.First().Comment.Should().Be(observation.Event.Media.First().Comments.First().Comment);
            obs.Event.Media.First().Comments.First().CommentBy.Should().Be(observation.Event.Media.First().Comments.First().CommentBy);
            obs.Event.Media.First().Comments.First().Created.Should().Be(observation.Event.Media.First().Comments.First().Created);
            obs.Event.Media.First().Contributor.Should().Be(observation.Event.Media.First().Contributor);
            obs.Event.Media.First().Created.Should().Be(observation.Event.Media.First().Created);
            obs.Event.Media.First().Creator.Should().Be(observation.Event.Media.First().Creator);
            obs.Event.Media.First().DatasetID.Should().Be(observation.Event.Media.First().DatasetID);
            obs.Event.Media.First().Description.Should().Be(observation.Event.Media.First().Description);
            obs.Event.Media.First().Format.Should().Be(observation.Event.Media.First().Format);
            obs.Event.Media.First().Identifier.Should().Be(observation.Event.Media.First().Identifier);
            obs.Event.Media.First().Publisher.Should().Be(observation.Event.Media.First().Publisher);
            obs.Event.Media.First().References.Should().Be(observation.Event.Media.First().References);
            obs.Event.Media.First().RightsHolder.Should().Be(observation.Event.Media.First().RightsHolder);
            obs.Event.Media.First().Source.Should().Be(observation.Event.Media.First().Source);
            obs.Event.Media.First().Type.Should().Be(observation.Event.Media.First().Type);

            obs.Event.ParentEventId.Should().Be(observation.Event.ParentEventId);
            obs.Event.PlainEndDate.Should().Be(observation.Event.PlainEndDate);
            obs.Event.PlainEndTime.Should().Be(observation.Event.PlainEndTime);
            obs.Event.PlainStartDate.Should().Be(observation.Event.PlainStartDate);
            obs.Event.PlainStartTime.Should().Be(observation.Event.PlainStartTime);
            obs.Event.SampleSizeUnit.Should().Be(observation.Event.SampleSizeUnit);
            obs.Event.SampleSizeValue.Should().Be(observation.Event.SampleSizeValue);
            obs.Event.SamplingEffort.Should().Be(observation.Event.SamplingEffort);
            obs.Event.SamplingProtocol.Should().Be(observation.Event.SamplingProtocol);
            obs.Event.StartDate.Should().Be(observation.Event.StartDate);
            obs.Event.StartDay.Should().Be(observation.Event.StartDay);
            obs.Event.StartMonth.Should().Be(observation.Event.StartMonth);
            obs.Event.StartYear.Should().Be(observation.Event.StartYear);
            obs.Event.VerbatimEventDate.Should().Be(observation.Event.VerbatimEventDate);
            
            
            obs.GeologicalContext.EarliestAgeOrLowestStage.Should().Be(observation.GeologicalContext.EarliestAgeOrLowestStage);
            obs.GeologicalContext.EarliestEonOrLowestEonothem.Should().Be(observation.GeologicalContext.EarliestEonOrLowestEonothem);
            obs.GeologicalContext.EarliestEpochOrLowestSeries.Should().Be(observation.GeologicalContext.EarliestEpochOrLowestSeries);
            obs.GeologicalContext.EarliestEraOrLowestErathem.Should().Be(observation.GeologicalContext.EarliestEraOrLowestErathem);
            obs.GeologicalContext.EarliestGeochronologicalEra.Should().Be(observation.GeologicalContext.EarliestGeochronologicalEra);
            obs.GeologicalContext.EarliestPeriodOrLowestSystem.Should().Be(observation.GeologicalContext.EarliestPeriodOrLowestSystem);
            obs.GeologicalContext.Formation.Should().Be(observation.GeologicalContext.Formation);
            obs.GeologicalContext.GeologicalContextId.Should().Be(observation.GeologicalContext.GeologicalContextId);
            obs.GeologicalContext.Group.Should().Be(observation.GeologicalContext.Group);
            obs.GeologicalContext.HighestBiostratigraphicZone.Should().Be(observation.GeologicalContext.HighestBiostratigraphicZone);
            obs.GeologicalContext.LatestAgeOrHighestStage.Should().Be(observation.GeologicalContext.LatestAgeOrHighestStage);
            obs.GeologicalContext.LatestEonOrHighestEonothem.Should().Be(observation.GeologicalContext.LatestEonOrHighestEonothem);
            obs.GeologicalContext.LatestEpochOrHighestSeries.Should().Be(observation.GeologicalContext.LatestEpochOrHighestSeries);
            obs.GeologicalContext.LatestEraOrHighestErathem.Should().Be(observation.GeologicalContext.LatestEraOrHighestErathem);
            obs.GeologicalContext.LatestGeochronologicalEra.Should().Be(observation.GeologicalContext.LatestGeochronologicalEra);
            obs.GeologicalContext.LatestPeriodOrHighestSystem.Should().Be(observation.GeologicalContext.LatestPeriodOrHighestSystem);
            obs.GeologicalContext.LithostratigraphicTerms.Should().Be(observation.GeologicalContext.LithostratigraphicTerms);
            obs.GeologicalContext.LowestBiostratigraphicZone.Should().Be(observation.GeologicalContext.LowestBiostratigraphicZone);
            obs.GeologicalContext.Member.Should().Be(observation.GeologicalContext.Member);

            obs.Id.Should().Be(observation.Id);

            obs.Identification.ConfirmedBy.Should().Be(observation.Identification.ConfirmedBy);
            obs.Identification.ConfirmedDate.Should().Be(observation.Identification.ConfirmedDate);
            obs.Identification.DateIdentified.Should().Be(observation.Identification.DateIdentified);
            obs.Identification.DeterminationMethod.Should().Be(observation.Identification.DeterminationMethod);
            obs.Identification.IdentificationId.Should().Be(observation.Identification.IdentificationId);
            obs.Identification.IdentificationQualifier.Should().Be(observation.Identification.IdentificationQualifier);
            obs.Identification.IdentificationReferences.Should().Be(observation.Identification.IdentificationReferences);
            obs.Identification.IdentificationRemarks.Should().Be(observation.Identification.IdentificationRemarks);
            obs.Identification.IdentifiedBy.Should().Be(observation.Identification.IdentifiedBy);
            obs.Identification.TypeStatus.Should().Be(observation.Identification.TypeStatus);
            obs.Identification.UncertainIdentification.Should().Be(observation.Identification.UncertainIdentification);
            obs.Identification.VerificationStatus.Should().Be(observation.Identification.VerificationStatus);
            obs.Identification.Verified.Should().Be(observation.Identification.Verified);
            obs.Identification.VerifiedBy.Should().Be(observation.Identification.VerifiedBy);

            obs.InformationWithheld.Should().Be(observation.InformationWithheld);
            obs.InstitutionCode.Should().Be(observation.InstitutionCode);
            obs.InstitutionId.Should().Be(observation.InstitutionId);
            obs.Language.Should().Be(observation.Language);
            obs.License.Should().Be(observation.License);

            obs.Location.Attributes.CountyPartIdByCoordinate.Should().Be(observation.Location.Attributes.CountyPartIdByCoordinate);
            obs.Location.Attributes.ExternalId.Should().Be(observation.Location.Attributes.ExternalId);
            obs.Location.Attributes.IsPrivate.Should().Be(observation.Location.Attributes.IsPrivate);
            obs.Location.Attributes.ProjectId.Should().Be(observation.Location.Attributes.ProjectId);
            obs.Location.Attributes.ProvincePartIdByCoordinate.Should().Be(observation.Location.Attributes.ProvincePartIdByCoordinate);
            obs.Location.Attributes.VerbatimMunicipality.Should().Be(observation.Location.Attributes.VerbatimMunicipality);
            obs.Location.Attributes.VerbatimProvince.Should().Be(observation.Location.Attributes.VerbatimProvince);

            obs.Location.Continent.Should().Be(observation.Location.Continent);
            obs.Location.CoordinatePrecision.Should().Be(observation.Location.CoordinatePrecision);
            obs.Location.CoordinateUncertaintyInMeters.Should().Be(observation.Location.CoordinateUncertaintyInMeters);
            obs.Location.Country.Should().Be(observation.Location.Country);
            obs.Location.CountryCode.Should().Be(observation.Location.CountryCode);
            obs.Location.CountryRegion.FeatureId.Should().Be(observation.Location.CountryRegion.FeatureId);
            obs.Location.CountryRegion.Name.Should().Be(observation.Location.CountryRegion.Name);
            obs.Location.County.FeatureId.Should().Be(observation.Location.County.FeatureId);
            obs.Location.County.Name.Should().Be(observation.Location.County.Name);
            obs.Location.DecimalLatitude.Should().Be(observation.Location.DecimalLatitude);
            obs.Location.DecimalLongitude.Should().Be(observation.Location.DecimalLongitude);
            obs.Location.Etrs89X.Should().Be(observation.Location.Etrs89X);
            obs.Location.Etrs89Y.Should().Be(observation.Location.Etrs89Y);
            obs.Location.FootprintSpatialFit.Should().Be(observation.Location.FootprintSpatialFit);
            obs.Location.FootprintSRS.Should().Be(observation.Location.FootprintSRS);
            obs.Location.FootprintWKT.Should().Be(observation.Location.FootprintWKT);
            obs.Location.GeodeticDatum.Should().Be(observation.Location.GeodeticDatum);
            obs.Location.GeoreferencedBy.Should().Be(observation.Location.GeoreferencedBy);
            obs.Location.GeoreferencedDate.Should().Be(observation.Location.GeoreferencedDate);
            obs.Location.GeoreferenceProtocol.Should().Be(observation.Location.GeoreferenceProtocol);
            obs.Location.GeoreferenceRemarks.Should().Be(observation.Location.GeoreferenceRemarks);
            obs.Location.GeoreferenceSources.Should().Be(observation.Location.GeoreferenceSources);
            obs.Location.GeoreferenceVerificationStatus.Should().Be(observation.Location.GeoreferenceVerificationStatus);
            obs.Location.HigherGeography.Should().Be(observation.Location.HigherGeography);
            obs.Location.HigherGeographyId.Should().Be(observation.Location.HigherGeographyId);
            obs.Location.Island.Should().Be(observation.Location.Island);
            obs.Location.IslandGroup.Should().Be(observation.Location.IslandGroup);
            obs.Location.Locality.Should().Be(observation.Location.Locality);
            obs.Location.LocationAccordingTo.Should().Be(observation.Location.LocationAccordingTo);
            obs.Location.LocationId.Should().Be(observation.Location.LocationId);
            obs.Location.LocationRemarks.Should().Be(observation.Location.LocationRemarks);
            obs.Location.MaximumDepthInMeters.Should().Be(observation.Location.MaximumDepthInMeters);
            obs.Location.MaximumDistanceAboveSurfaceInMeters.Should().Be(observation.Location.MaximumDistanceAboveSurfaceInMeters);
            obs.Location.MaximumElevationInMeters.Should().Be(observation.Location.MaximumElevationInMeters);
            obs.Location.MinimumDepthInMeters.Should().Be(observation.Location.MinimumDepthInMeters);
            obs.Location.MinimumDistanceAboveSurfaceInMeters.Should().Be(observation.Location.MinimumDistanceAboveSurfaceInMeters);
            obs.Location.MinimumElevationInMeters.Should().Be(observation.Location.MinimumElevationInMeters);
            obs.Location.Municipality.FeatureId.Should().Be(observation.Location.Municipality.FeatureId);
            obs.Location.Municipality.Name.Should().Be(observation.Location.Municipality.Name);
            obs.Location.Parish.FeatureId.Should().Be(observation.Location.Parish.FeatureId);
            obs.Location.Parish.Name.Should().Be(observation.Location.Parish.Name);
            obs.Location.PointRadiusSpatialFit.Should().Be(observation.Location.PointRadiusSpatialFit);
            obs.Location.Province.FeatureId.Should().Be(observation.Location.Province.FeatureId);
            obs.Location.Province.Name.Should().Be(observation.Location.Province.Name);
            obs.Location.Sweref99TmX.Should().Be(observation.Location.Sweref99TmX);
            obs.Location.Sweref99TmY.Should().Be(observation.Location.Sweref99TmY);
            obs.Location.VerbatimCoordinates.Should().Be(observation.Location.VerbatimCoordinates);
            obs.Location.VerbatimCoordinateSystem.Should().Be(observation.Location.VerbatimCoordinateSystem);
            obs.Location.VerbatimDepth.Should().Be(observation.Location.VerbatimDepth);
            obs.Location.VerbatimElevation.Should().Be(observation.Location.VerbatimElevation);
            obs.Location.VerbatimLatitude.Should().Be(observation.Location.VerbatimLatitude);
            obs.Location.VerbatimLocality.Should().Be(observation.Location.VerbatimLocality);
            obs.Location.VerbatimLongitude.Should().Be(observation.Location.VerbatimLongitude);
            obs.Location.VerbatimSRS.Should().Be(observation.Location.VerbatimSRS);
            obs.Location.WaterBody.Should().Be(observation.Location.WaterBody);

            obs.MaterialSample.MaterialSampleId.Should().Be(observation.MaterialSample.MaterialSampleId);

            obs.MeasurementOrFacts.First().MeasurementAccuracy.Should().Be(observation.MeasurementOrFacts.First().MeasurementAccuracy);
            obs.MeasurementOrFacts.First().MeasurementDeterminedBy.Should().Be(observation.MeasurementOrFacts.First().MeasurementDeterminedBy);
            obs.MeasurementOrFacts.First().MeasurementDeterminedDate.Should().Be(observation.MeasurementOrFacts.First().MeasurementDeterminedDate);
            obs.MeasurementOrFacts.First().MeasurementID.Should().Be(observation.MeasurementOrFacts.First().MeasurementID);
            obs.MeasurementOrFacts.First().MeasurementMethod.Should().Be(observation.MeasurementOrFacts.First().MeasurementMethod);
            obs.MeasurementOrFacts.First().MeasurementRemarks.Should().Be(observation.MeasurementOrFacts.First().MeasurementRemarks);
            obs.MeasurementOrFacts.First().MeasurementType.Should().Be(observation.MeasurementOrFacts.First().MeasurementType);
            obs.MeasurementOrFacts.First().MeasurementTypeID.Should().Be(observation.MeasurementOrFacts.First().MeasurementTypeID);
            obs.MeasurementOrFacts.First().MeasurementUnit.Should().Be(observation.MeasurementOrFacts.First().MeasurementUnit);
            obs.MeasurementOrFacts.First().MeasurementUnitID.Should().Be(observation.MeasurementOrFacts.First().MeasurementUnitID);
            obs.MeasurementOrFacts.First().MeasurementValue.Should().Be(observation.MeasurementOrFacts.First().MeasurementValue);
            obs.MeasurementOrFacts.First().MeasurementValueID.Should().Be(observation.MeasurementOrFacts.First().MeasurementValueID);
            obs.MeasurementOrFacts.First().OccurrenceID.Should().Be(observation.MeasurementOrFacts.First().OccurrenceID);

            obs.Modified.Should().Be(observation.Modified);
            obs.Occurrence.AssociatedMedia.Should().Be(observation.Occurrence.AssociatedMedia);
            obs.Occurrence.AssociatedOccurrences.Should().Be(observation.Occurrence.AssociatedOccurrences);
            obs.Occurrence.AssociatedReferences.Should().Be(observation.Occurrence.AssociatedReferences);
            obs.Occurrence.AssociatedSequences.Should().Be(observation.Occurrence.AssociatedSequences);
            obs.Occurrence.AssociatedTaxa.Should().Be(observation.Occurrence.AssociatedTaxa);
            obs.Occurrence.Behavior.Should().Be(observation.Occurrence.Behavior);
            obs.Occurrence.Biotope.Should().Be(observation.Occurrence.Biotope);
            obs.Occurrence.BiotopeDescription.Should().Be(observation.Occurrence.BiotopeDescription);
            obs.Occurrence.BirdNestActivityId.Should().Be(observation.Occurrence.BirdNestActivityId);
            obs.Occurrence.CatalogId.Should().Be(observation.Occurrence.CatalogId);
            obs.Occurrence.CatalogNumber.Should().Be(observation.Occurrence.CatalogNumber);
            obs.Occurrence.Disposition.Should().Be(observation.Occurrence.Disposition);
            obs.Occurrence.EstablishmentMeans.Should().Be(observation.Occurrence.EstablishmentMeans);
            obs.Occurrence.IndividualCount.Should().Be(observation.Occurrence.IndividualCount);
            obs.Occurrence.IsNaturalOccurrence.Should().Be(observation.Occurrence.IsNaturalOccurrence);
            obs.Occurrence.IsNeverFoundObservation.Should().Be(observation.Occurrence.IsNeverFoundObservation);
            obs.Occurrence.IsNotRediscoveredObservation.Should().Be(observation.Occurrence.IsNotRediscoveredObservation);
            obs.Occurrence.IsPositiveObservation.Should().Be(observation.Occurrence.IsPositiveObservation);
            obs.Occurrence.Length.Should().Be(observation.Occurrence.Length);
            obs.Occurrence.LifeStage.Should().Be(observation.Occurrence.LifeStage);

            obs.Occurrence.Media.First().Audience.Should().Be(observation.Occurrence.Media.First().Audience);
            obs.Occurrence.Media.First().Comments.First().Comment.Should().Be(observation.Occurrence.Media.First().Comments.First().Comment);
            obs.Occurrence.Media.First().Comments.First().CommentBy.Should().Be(observation.Occurrence.Media.First().Comments.First().CommentBy);
            obs.Occurrence.Media.First().Comments.First().Created.Should().Be(observation.Occurrence.Media.First().Comments.First().Created);
            obs.Occurrence.Media.First().Contributor.Should().Be(observation.Occurrence.Media.First().Contributor);
            obs.Occurrence.Media.First().Created.Should().Be(observation.Occurrence.Media.First().Created);
            obs.Occurrence.Media.First().Creator.Should().Be(observation.Occurrence.Media.First().Creator);
            obs.Occurrence.Media.First().DatasetID.Should().Be(observation.Occurrence.Media.First().DatasetID);
            obs.Occurrence.Media.First().Description.Should().Be(observation.Occurrence.Media.First().Description);
            obs.Occurrence.Media.First().Format.Should().Be(observation.Occurrence.Media.First().Format);
            obs.Occurrence.Media.First().Identifier.Should().Be(observation.Occurrence.Media.First().Identifier);
            obs.Occurrence.Media.First().Publisher.Should().Be(observation.Occurrence.Media.First().Publisher);
            obs.Occurrence.Media.First().References.Should().Be(observation.Occurrence.Media.First().References);
            obs.Occurrence.Media.First().RightsHolder.Should().Be(observation.Occurrence.Media.First().RightsHolder);
            obs.Occurrence.Media.First().Source.Should().Be(observation.Occurrence.Media.First().Source);
            obs.Occurrence.Media.First().Type.Should().Be(observation.Occurrence.Media.First().Type);

            obs.Occurrence.OccurrenceId.Should().Be(observation.Occurrence.OccurrenceId);
            obs.Occurrence.OccurrenceRemarks.Should().Be(observation.Occurrence.OccurrenceRemarks);
            obs.Occurrence.OccurrenceStatus.Should().Be(observation.Occurrence.OccurrenceStatus);
            obs.Occurrence.OrganismQuantity.Should().Be(observation.Occurrence.OrganismQuantity);
            obs.Occurrence.OrganismQuantityAggregation.Should().Be(observation.Occurrence.OrganismQuantityAggregation);
            obs.Occurrence.OrganismQuantityInt.Should().Be(observation.Occurrence.OrganismQuantityInt);
            obs.Occurrence.OrganismQuantityUnit.Should().Be(observation.Occurrence.OrganismQuantityUnit);
            obs.Occurrence.OtherCatalogNumbers.Should().Be(observation.Occurrence.OtherCatalogNumbers);
            obs.Occurrence.Preparations.Should().Be(observation.Occurrence.Preparations);
            obs.Occurrence.RecordedBy.Should().Be(observation.Occurrence.RecordedBy);
            obs.Occurrence.RecordNumber.Should().Be(observation.Occurrence.RecordNumber);
            obs.Occurrence.ReportedBy.Should().Be(observation.Occurrence.ReportedBy);
            obs.Occurrence.ReportedDate.Should().Be(observation.Occurrence.ReportedDate);
            obs.Occurrence.ReproductiveCondition.Should().Be(observation.Occurrence.ReproductiveCondition);
            obs.Occurrence.SensitivityCategory.Should().Be(observation.Occurrence.SensitivityCategory);
            obs.Occurrence.Sex.Should().Be(observation.Occurrence.Sex);
            obs.Occurrence.Url.Should().Be(observation.Occurrence.Url);
            obs.Occurrence.Weight.Should().Be(observation.Occurrence.Weight);

            obs.Organism.AssociatedOrganisms.Should().Be(observation.Organism.AssociatedOrganisms);
            obs.Organism.OrganismId.Should().Be(observation.Organism.OrganismId);
            obs.Organism.OrganismName.Should().Be(observation.Organism.OrganismName);
            obs.Organism.OrganismRemarks.Should().Be(observation.Organism.OrganismRemarks);
            obs.Organism.OrganismScope.Should().Be(observation.Organism.OrganismScope);
            obs.Organism.PreviousIdentifications.Should().Be(observation.Organism.PreviousIdentifications);
          
            obs.OwnerInstitutionCode.Should().Be(observation.OwnerInstitutionCode);
            obs.PrivateCollection.Should().Be(observation.PrivateCollection);
            
            obs.Projects.First().Category.Should().Be(observation.Projects.First().Category);
            obs.Projects.First().CategorySwedish.Should().Be(observation.Projects.First().CategorySwedish);
            obs.Projects.First().Description.Should().Be(observation.Projects.First().Description);
            obs.Projects.First().EndDate.Should().Be(observation.Projects.First().EndDate);
            obs.Projects.First().Id.Should().Be(observation.Projects.First().Id);
            obs.Projects.First().Name.Should().Be(observation.Projects.First().Name);
            obs.Projects.First().Owner.Should().Be(observation.Projects.First().Owner);
            obs.Projects.First().ProjectParameters.First().DataType.Should().Be(observation.Projects.First().ProjectParameters.First().DataType);
            obs.Projects.First().ProjectParameters.First().Description.Should().Be(observation.Projects.First().ProjectParameters.First().Description);
            obs.Projects.First().ProjectParameters.First().Id.Should().Be(observation.Projects.First().ProjectParameters.First().Id);
            obs.Projects.First().ProjectParameters.First().Name.Should().Be(observation.Projects.First().ProjectParameters.First().Name);
            obs.Projects.First().ProjectParameters.First().Unit.Should().Be(observation.Projects.First().ProjectParameters.First().Unit);
            obs.Projects.First().ProjectParameters.First().Value.Should().Be(observation.Projects.First().ProjectParameters.First().Value);
            obs.Projects.First().ProjectURL.Should().Be(observation.Projects.First().ProjectURL);
            obs.Projects.First().StartDate.Should().Be(observation.Projects.First().StartDate);
            obs.Projects.First().SurveyMethod.Should().Be(observation.Projects.First().SurveyMethod);
            obs.Projects.First().SurveyMethodUrl.Should().Be(observation.Projects.First().SurveyMethodUrl);
            

            obs.ProjectsSummary.Project1Category.Should().Be(observation.ProjectsSummary.Project1Category);
            obs.ProjectsSummary.Project1Id.Should().Be(observation.ProjectsSummary.Project1Id);
            obs.ProjectsSummary.Project1Name.Should().Be(observation.ProjectsSummary.Project1Name);
            obs.ProjectsSummary.Project1Url.Should().Be(observation.ProjectsSummary.Project1Url);
            obs.ProjectsSummary.Project1Values.Should().Be(observation.ProjectsSummary.Project1Values);
            obs.ProjectsSummary.Project2Category.Should().Be(observation.ProjectsSummary.Project2Category);
            obs.ProjectsSummary.Project2Id.Should().Be(observation.ProjectsSummary.Project2Id);
            obs.ProjectsSummary.Project2Name.Should().Be(observation.ProjectsSummary.Project2Name);
            obs.ProjectsSummary.Project2Url.Should().Be(observation.ProjectsSummary.Project2Url);
            obs.ProjectsSummary.Project2Values.Should().Be(observation.ProjectsSummary.Project2Values);

            obs.PublicCollection.Should().Be(observation.PublicCollection);
            obs.References.Should().Be(observation.References);
            obs.RightsHolder.Should().Be(observation.RightsHolder);
            obs.Sensitive.Should().Be(observation.Sensitive);
            obs.SpeciesCollectionLabel.Should().Be(observation.SpeciesCollectionLabel);
            
            obs.Taxon.AcceptedNameUsage.Should().Be(observation.Taxon.AcceptedNameUsage);
            obs.Taxon.AcceptedNameUsageId.Should().Be(observation.Taxon.AcceptedNameUsageId);
            obs.Taxon.Attributes.ActionPlan.Should().Be(observation.Taxon.Attributes.ActionPlan);
            obs.Taxon.Attributes.DisturbanceRadius.Should().Be(observation.Taxon.Attributes.DisturbanceRadius);
            obs.Taxon.Attributes.DyntaxaTaxonId.Should().Be(observation.Taxon.Attributes.DyntaxaTaxonId);
            obs.Taxon.Attributes.GbifTaxonId.Should().Be(observation.Taxon.Attributes.GbifTaxonId);
            obs.Taxon.Attributes.InvasiveRiskAssessmentCategory.Should().Be(observation.Taxon.Attributes.InvasiveRiskAssessmentCategory);
            obs.Taxon.Attributes.IsInvasiveAccordingToEuRegulation.Should().Be(observation.Taxon.Attributes.IsInvasiveAccordingToEuRegulation);
            obs.Taxon.Attributes.IsInvasiveInSweden.Should().Be(observation.Taxon.Attributes.IsInvasiveInSweden);
            obs.Taxon.Attributes.IsRedlisted.Should().Be(observation.Taxon.Attributes.IsRedlisted);
            obs.Taxon.Attributes.Natura2000HabitatsDirectiveArticle2.Should().Be(observation.Taxon.Attributes.Natura2000HabitatsDirectiveArticle2);
            obs.Taxon.Attributes.Natura2000HabitatsDirectiveArticle4.Should().Be(observation.Taxon.Attributes.Natura2000HabitatsDirectiveArticle4);
            obs.Taxon.Attributes.Natura2000HabitatsDirectiveArticle5.Should().Be(observation.Taxon.Attributes.Natura2000HabitatsDirectiveArticle5);
            obs.Taxon.Attributes.OrganismGroup.Should().Be(observation.Taxon.Attributes.OrganismGroup);
            obs.Taxon.Attributes.ParentDyntaxaTaxonId.Should().Be(observation.Taxon.Attributes.ParentDyntaxaTaxonId);
            obs.Taxon.Attributes.ProtectedByLaw.Should().Be(observation.Taxon.Attributes.ProtectedByLaw);
            obs.Taxon.Attributes.ProtectionLevel.Should().Be(observation.Taxon.Attributes.ProtectionLevel);
            obs.Taxon.Attributes.RedlistCategory.Should().Be(observation.Taxon.Attributes.RedlistCategory);
            obs.Taxon.Attributes.RedlistCategoryDerived.Should().Be(observation.Taxon.Attributes.RedlistCategoryDerived);
            obs.Taxon.Attributes.SensitivityCategory.Should().Be(observation.Taxon.Attributes.SensitivityCategory);
            obs.Taxon.Attributes.SortOrder.Should().Be(observation.Taxon.Attributes.SortOrder);
            obs.Taxon.Attributes.SpeciesGroup.Should().Be(observation.Taxon.Attributes.SpeciesGroup);
            obs.Taxon.Attributes.SwedishHistory.Should().Be(observation.Taxon.Attributes.SwedishHistory);
            obs.Taxon.Attributes.SwedishOccurrence.Should().Be(observation.Taxon.Attributes.SwedishOccurrence);
            obs.Taxon.Attributes.Synonyms.First().Author.Should().Be(observation.Taxon.Attributes.Synonyms.First().Author);
            obs.Taxon.Attributes.Synonyms.First().Name.Should().Be(observation.Taxon.Attributes.Synonyms.First().Name);
            obs.Taxon.Attributes.Synonyms.First().NomenclaturalStatus.Should().Be(observation.Taxon.Attributes.Synonyms.First().NomenclaturalStatus);
            obs.Taxon.Attributes.Synonyms.First().TaxonomicStatus.Should().Be(observation.Taxon.Attributes.Synonyms.First().TaxonomicStatus);
           
            obs.Taxon.BirdDirective.Should().Be(observation.Taxon.BirdDirective);
            obs.Taxon.Class.Should().Be(observation.Taxon.Class);
            obs.Taxon.Family.Should().Be(observation.Taxon.Family);
            obs.Taxon.Genus.Should().Be(observation.Taxon.Genus);
            obs.Taxon.HigherClassification.Should().Be(observation.Taxon.HigherClassification);
            obs.Taxon.Id.Should().Be(observation.Taxon.Id);
            obs.Taxon.InfraspecificEpithet.Should().Be(observation.Taxon.InfraspecificEpithet);
            obs.Taxon.Kingdom.Should().Be(observation.Taxon.Kingdom);
            obs.Taxon.NameAccordingTo.Should().Be(observation.Taxon.NameAccordingTo);
            obs.Taxon.NameAccordingToId.Should().Be(observation.Taxon.NameAccordingToId);
            obs.Taxon.NamePublishedIn.Should().Be(observation.Taxon.NamePublishedIn);
            obs.Taxon.NamePublishedInId.Should().Be(observation.Taxon.NamePublishedInId);
            obs.Taxon.NamePublishedInYear.Should().Be(observation.Taxon.NamePublishedInYear);
            obs.Taxon.NomenclaturalCode.Should().Be(observation.Taxon.NomenclaturalCode);
            obs.Taxon.NomenclaturalStatus.Should().Be(observation.Taxon.NomenclaturalStatus);
            obs.Taxon.Order.Should().Be(observation.Taxon.Order);
            obs.Taxon.OriginalNameUsage.Should().Be(observation.Taxon.OriginalNameUsage);
            obs.Taxon.OriginalNameUsageId.Should().Be(observation.Taxon.OriginalNameUsageId);
            obs.Taxon.ParentNameUsage.Should().Be(observation.Taxon.ParentNameUsage);
            obs.Taxon.ParentNameUsageId.Should().Be(observation.Taxon.ParentNameUsageId);
            obs.Taxon.Phylum.Should().Be(observation.Taxon.Phylum);
            obs.Taxon.ScientificName.Should().Be(observation.Taxon.ScientificName);
            obs.Taxon.ScientificNameAuthorship.Should().Be(observation.Taxon.ScientificNameAuthorship);
            obs.Taxon.ScientificNameId.Should().Be(observation.Taxon.ScientificNameId);
            obs.Taxon.SecondaryParentDyntaxaTaxonIds.First().Should().Be(observation.Taxon.SecondaryParentDyntaxaTaxonIds.First());
            obs.Taxon.SpecificEpithet.Should().Be(observation.Taxon.SpecificEpithet);
            obs.Taxon.Subgenus.Should().Be(observation.Taxon.Subgenus);
            obs.Taxon.TaxonConceptId.Should().Be(observation.Taxon.TaxonConceptId);
            obs.Taxon.TaxonId.Should().Be(observation.Taxon.TaxonId);
            obs.Taxon.TaxonomicStatus.Should().Be(observation.Taxon.TaxonomicStatus);
            obs.Taxon.TaxonRank.Should().Be(observation.Taxon.TaxonRank);
            obs.Taxon.TaxonRemarks.Should().Be(observation.Taxon.TaxonRemarks);
            obs.Taxon.VerbatimId.Should().Be(observation.Taxon.VerbatimId);
            obs.Taxon.VerbatimTaxonRank.Should().Be(observation.Taxon.VerbatimTaxonRank);
            obs.Taxon.VernacularName.Should().Be(observation.Taxon.VernacularName);
           
            obs.Type.Should().Be(observation.Type);
        }
    }
}