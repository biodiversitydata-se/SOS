using DotNetCore.Mapping;
using FizzWare.NBuilder;
using FizzWare.NBuilder.Implementation;
using SOS.ContainerIntegrationTests.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Verbatim.DarwinCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace SOS.ContainerIntegrationTests.TestData.TestDataBuilder
{
    public static class DarwinCoreObservationBuilder
    {
        private static Bogus.Faker _faker = new Bogus.Faker();
        private static Bogus.DataSets.Lorem _lorem = new Bogus.DataSets.Lorem("sv");
        private const int ArtportalenDataSourceId = 1;

        public static List<DwcObservationVerbatim> VerbatimDarwinCoreObservationsFromJsonFile
        {
            get
            {
                if (_verbatimDarwinCoreObservationsFromJsonFile == null)
                {                    
                    string filePath = "Resources/TestDataBuilder/DarwinCoreObservations_1000.json".GetAbsoluteFilePath();
                    string str = File.ReadAllText(filePath, Encoding.UTF8);
                    var serializeOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                    serializeOptions.Converters.Add(new ObjectIdConverter());

                    _verbatimDarwinCoreObservationsFromJsonFile = JsonSerializer.Deserialize<List<DwcObservationVerbatim>>(str, serializeOptions);
                }

                return _verbatimDarwinCoreObservationsFromJsonFile;
            }
        }
        private static List<DwcObservationVerbatim> _verbatimDarwinCoreObservationsFromJsonFile;

        public static IOperable<DwcObservationVerbatim> HaveRandomValues(this IOperable<DwcObservationVerbatim> operable)
        {
            var builder = ((IDeclaration<DwcObservationVerbatim>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                //bool equalStartAndEndDate = _faker.Random.WeightedRandom(new bool[] { true, false }, new float[] { 0.9f, 0.1f }); 
                TimeSpan? obsTimeSpan = _faker.Random.Bool(0.9f) ? null : _faker.Date.Timespan(TimeSpan.FromHours(5)); // 90% probability of equal start and end date.
                DateTime startDate = _faker.Date.Past(10); // random date within 10 years
                DateTime endDate = obsTimeSpan == null ? startDate : startDate.Add(obsTimeSpan.Value);
                DateTime reportedDate = endDate.Add(_faker.Date.Timespan(TimeSpan.FromDays(5)));
                DateTime editDate = reportedDate;
                int sightingId = _faker.IndexVariable++;

                // Record level
                obs.Id = _faker.IndexVariable++;
                obs.RecordId = null;
                obs.DataProviderId = 1;
                obs.DataProviderIdentifier = null;
                obs.DwcArchiveFilename = null;
                obs.Type = null;
                obs.Modified = null;
                obs.Language = null;
                obs.License = null;
                obs.RightsHolder = _faker.Name.FullName();
                obs.AccessRights = null;
                obs.BibliographicCitation = null;
                obs.References = null;
                obs.InstitutionCode = null;
                obs.InstitutionID = null;
                obs.DatasetID = null;
                obs.DatasetName = null;
                obs.CollectionCode = null;
                obs.CollectionID = null;
                obs.OwnerInstitutionCode = null;
                obs.BasisOfRecord = null;
                obs.InformationWithheld = null;
                obs.DataGeneralizations = null;
                obs.DynamicProperties = null;
                obs.Behavior = null;
                obs.ObservationAudubonMedia = null;
                obs.ObservationExtendedMeasurementOrFacts = null;
                obs.ObservationMeasurementOrFacts = null;
                obs.ObservationMultimedia = null;

                // Occurrence
                obs.OccurrenceID = $"urn:lsid:artportalen.se:sighting:{sightingId}";
                obs.OccurrenceRemarks = null;
                obs.OccurrenceStatus = null;
                obs.CatalogNumber = sightingId.ToString();
                obs.RecordNumber = null;
                obs.RecordedBy = _faker.Name.FullName();
                obs.IndividualCount = null;
                obs.OrganismQuantity = null;
                obs.OrganismQuantityType = null;
                obs.Sex = null;
                obs.LifeStage = null;
                obs.ReproductiveCondition = null;
                obs.EstablishmentMeans = null;
                obs.GeoreferenceVerificationStatus = null;
                obs.Preparations = null;
                obs.Disposition = null;
                obs.AssociatedMedia = null;
                obs.AssociatedOccurrences = null;
                obs.AssociatedReferences = null;
                obs.AssociatedTaxa = null;
                obs.OtherCatalogNumbers = null;
                obs.AssociatedSequences = null;

                // Organism
                obs.OrganismID = null;
                obs.OrganismName = null;
                obs.OrganismRemarks = null;
                obs.OrganismScope = null;
                obs.PreviousIdentifications = null;
                obs.AssociatedOrganisms = null;

                // MaterialSample
                obs.MaterialSampleID = null;

                // Event
                obs.EventID = null;
                obs.ParentEventID = null;
                obs.FieldNotes = null;
                obs.FieldNumber = null;
                obs.EventAudubonMedia = null;
                obs.EventExtendedMeasurementOrFacts = null;
                obs.EventMeasurementOrFacts = null;
                obs.EventMultimedia = null;
                obs.Year = null;
                obs.Month = null;
                obs.Day = null;
                obs.EventDate = null;
                obs.EventTime = null;
                obs.VerbatimEventDate = null;
                obs.StartDayOfYear = null;
                obs.EndDayOfYear = null;
                obs.Habitat = null;
                obs.SampleSizeUnit = null;
                obs.SampleSizeValue = null;
                obs.SamplingEffort = null;
                obs.SamplingProtocol = null;
                obs.EventRemarks = null;

                // Location
                obs.LocationID = null;
                obs.HigherGeographyID = null;
                obs.HigherGeography = null;
                obs.Continent = null;
                obs.WaterBody = null;
                obs.Island = null;
                obs.IslandGroup = null;
                obs.CoordinatePrecision = null;
                obs.CoordinateUncertaintyInMeters = null;
                obs.Country = null;
                obs.CountryCode = null;
                obs.County = null;
                obs.StateProvince = null;
                obs.Municipality = null;
                obs.Locality = null;
                obs.VerbatimCoordinates = null;
                obs.VerbatimCoordinateSystem = null;
                obs.VerbatimDepth = null;
                obs.VerbatimElevation = null;
                obs.VerbatimLatitude = null;
                obs.VerbatimLocality = null;
                obs.VerbatimLongitude = null;
                obs.VerbatimSRS = null;
                obs.MaximumDepthInMeters = null;
                obs.MaximumDistanceAboveSurfaceInMeters = null;
                obs.MaximumElevationInMeters = null;
                obs.MinimumDepthInMeters = null;
                obs.MinimumDistanceAboveSurfaceInMeters = null;
                obs.MinimumElevationInMeters = null;
                obs.LocationAccordingTo = null;
                obs.LocationRemarks = null;
                obs.DecimalLatitude = null;
                obs.DecimalLongitude = null;
                obs.GeodeticDatum = null;
                obs.PointRadiusSpatialFit = null;
                obs.FootprintSpatialFit = null;
                obs.FootprintSRS = null;
                obs.FootprintWKT = null;
                obs.GeoreferencedBy = null;
                obs.GeoreferencedDate = null;
                obs.GeoreferenceProtocol = null;
                obs.GeoreferenceRemarks = null;
                obs.GeoreferenceSources = null;

                // GeologicalContext
                obs.GeologicalContextID = null;
                obs.EarliestAgeOrLowestStage = null;
                obs.EarliestEonOrLowestEonothem = null;
                obs.EarliestEpochOrLowestSeries = null;
                obs.EarliestEraOrLowestErathem = null;
                obs.EarliestGeochronologicalEra = null;
                obs.EarliestPeriodOrLowestSystem = null;
                obs.Formation = null;
                obs.FromLithostratigraphicUnit = null;
                obs.Group = null;
                obs.HighestBiostratigraphicZone = null;
                obs.LatestAgeOrHighestStage = null;
                obs.LatestEonOrHighestEonothem = null;
                obs.LatestEpochOrHighestSeries = null;
                obs.LatestEraOrHighestErathem = null;
                obs.LatestGeochronologicalEra = null;
                obs.LatestPeriodOrHighestSystem = null;
                obs.LithostratigraphicTerms = null;
                obs.LowestBiostratigraphicZone = null;
                obs.Member = null;
                obs.Bed = null;

                // Identification
                obs.IdentificationID = null;
                obs.IdentificationQualifier = null;
                obs.IdentificationReferences = null;
                obs.IdentificationRemarks = null;
                obs.IdentificationVerificationStatus = null;
                obs.IdentifiedBy = null;
                obs.TypeStatus = null;
                obs.DateIdentified = null;

                // Taxon
                obs.Kingdom = null;
                obs.AcceptedNameUsage = null;
                obs.AcceptedNameUsageID = null;
                obs.TaxonConceptID = null;
                obs.TaxonID = null;
                obs.TaxonomicStatus = null;
                obs.TaxonRank = null;
                obs.TaxonRemarks = null;
                obs.Class = null;
                obs.Family = null;
                obs.Genus = null;
                obs.HigherClassification = null;
                obs.InfraspecificEpithet = null;
                obs.NameAccordingTo = null;
                obs.NameAccordingToID = null;
                obs.NamePublishedIn = null;
                obs.NamePublishedInID = null;
                obs.NamePublishedInYear = null;
                obs.NomenclaturalCode = null;
                obs.NomenclaturalStatus = null;
                obs.Order = null;
                obs.OriginalNameUsage = null;
                obs.OriginalNameUsageID = null;
                obs.ParentNameUsage = null;
                obs.ParentNameUsageID = null;
                obs.Phylum = null;
                obs.ScientificName = null;
                obs.ScientificNameAuthorship = null;
                obs.ScientificNameID = null;
                obs.Species = null;
                obs.SpecificEpithet = null;
                obs.Subgenus = null;
                obs.VerbatimTaxonRank = null;
                obs.VernacularName = null;

                // UseWithIRI
                obs.ToTaxon = null;
                obs.InCollection = null;
                obs.InDataset = null;
                obs.InDescribedPlace = null;
            });

            return operable;
        }

        public static IOperable<DwcObservationVerbatim> HaveValuesFromPredefinedObservations(this IOperable<DwcObservationVerbatim> operable)
        {
            var builder = ((IDeclaration<DwcObservationVerbatim>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                var sourceObservation = Pick<DwcObservationVerbatim>.RandomItemFrom(VerbatimDarwinCoreObservationsFromJsonFile).Clone();
                int sightingId = _faker.IndexVariable++;
                string occurrenceID = $"urn:lsid:artportalen.se:sighting:{sightingId}";

                // Record level
                obs.Id = sourceObservation.Id; //_faker.IndexVariable++;
                obs.RecordId = sourceObservation.RecordId;
                obs.DataProviderId = sourceObservation.DataProviderId; // 1;
                obs.DataProviderIdentifier = sourceObservation.DataProviderIdentifier;
                obs.DwcArchiveFilename = sourceObservation.DwcArchiveFilename;
                obs.Type = sourceObservation.Type;
                obs.Modified = sourceObservation.Modified;
                obs.Language = sourceObservation.Language;
                obs.License = sourceObservation.License;
                obs.RightsHolder = sourceObservation.RightsHolder;
                obs.AccessRights = sourceObservation.AccessRights;
                obs.BibliographicCitation = sourceObservation.BibliographicCitation;
                obs.References = sourceObservation.References;
                obs.InstitutionCode = sourceObservation.InstitutionCode;
                obs.InstitutionID = sourceObservation.InstitutionID;
                obs.DatasetID = sourceObservation.DatasetID;
                obs.DatasetName = sourceObservation.DatasetName;
                obs.CollectionCode = sourceObservation.CollectionCode;
                obs.CollectionID = sourceObservation.CollectionID;
                obs.OwnerInstitutionCode = sourceObservation.OwnerInstitutionCode;
                obs.BasisOfRecord = sourceObservation.BasisOfRecord;
                obs.InformationWithheld = sourceObservation.InformationWithheld;
                obs.DataGeneralizations = sourceObservation.DataGeneralizations;
                obs.DynamicProperties = sourceObservation.DynamicProperties;
                obs.Behavior = sourceObservation.Behavior;
                obs.ObservationAudubonMedia = sourceObservation.ObservationAudubonMedia;
                obs.ObservationExtendedMeasurementOrFacts = sourceObservation.ObservationExtendedMeasurementOrFacts;
                obs.ObservationMeasurementOrFacts = sourceObservation.ObservationMeasurementOrFacts;
                obs.ObservationMultimedia = sourceObservation.ObservationMultimedia;

                // Occurrence
                obs.OccurrenceID = occurrenceID;
                obs.OccurrenceRemarks = sourceObservation.OccurrenceRemarks;
                obs.OccurrenceStatus = sourceObservation.OccurrenceStatus;
                obs.CatalogNumber = sightingId.ToString();
                obs.RecordNumber = sourceObservation.RecordNumber;
                obs.RecordedBy = sourceObservation.RecordedBy;
                obs.IndividualCount = sourceObservation.IndividualCount;
                obs.OrganismQuantity = sourceObservation.OrganismQuantity;
                obs.OrganismQuantityType = sourceObservation.OrganismQuantityType;
                obs.Sex = sourceObservation.Sex;
                obs.LifeStage = sourceObservation.LifeStage;
                obs.ReproductiveCondition = sourceObservation.ReproductiveCondition;
                obs.EstablishmentMeans = sourceObservation.EstablishmentMeans;
                obs.GeoreferenceVerificationStatus = sourceObservation.GeoreferenceVerificationStatus;
                obs.Preparations = sourceObservation.Preparations;
                obs.Disposition = sourceObservation.Disposition;
                obs.AssociatedMedia = sourceObservation.AssociatedMedia;
                obs.AssociatedOccurrences = sourceObservation.AssociatedOccurrences;
                obs.AssociatedReferences = sourceObservation.AssociatedReferences;
                obs.AssociatedTaxa = sourceObservation.AssociatedTaxa;
                obs.OtherCatalogNumbers = sourceObservation.OtherCatalogNumbers;
                obs.AssociatedSequences = sourceObservation.AssociatedSequences;

                // Organism
                obs.OrganismID = sourceObservation.OrganismID;
                obs.OrganismName = sourceObservation.OrganismName;
                obs.OrganismRemarks = sourceObservation.OrganismRemarks;
                obs.OrganismScope = sourceObservation.OrganismScope;
                obs.PreviousIdentifications = sourceObservation.PreviousIdentifications;
                obs.AssociatedOrganisms = sourceObservation.AssociatedOrganisms;

                // MaterialSample
                obs.MaterialSampleID = sourceObservation.MaterialSampleID;

                // Event
                obs.EventID = sourceObservation.EventID;
                obs.ParentEventID = sourceObservation.ParentEventID;
                obs.FieldNotes = sourceObservation.FieldNotes;
                obs.FieldNumber = sourceObservation.FieldNumber;
                obs.EventAudubonMedia = sourceObservation.EventAudubonMedia;
                obs.EventExtendedMeasurementOrFacts = sourceObservation.EventExtendedMeasurementOrFacts;
                obs.EventMeasurementOrFacts = sourceObservation.EventMeasurementOrFacts;
                obs.EventMultimedia = sourceObservation.EventMultimedia;
                obs.Year = sourceObservation.Year;
                obs.Month = sourceObservation.Month;
                obs.Day = sourceObservation.Day;
                obs.EventDate = sourceObservation.EventDate;
                obs.EventTime = sourceObservation.EventTime;
                obs.VerbatimEventDate = sourceObservation.VerbatimEventDate;
                obs.StartDayOfYear = sourceObservation.StartDayOfYear;
                obs.EndDayOfYear = sourceObservation.EndDayOfYear;
                obs.Habitat = sourceObservation.Habitat;
                obs.SampleSizeUnit = sourceObservation.SampleSizeUnit;
                obs.SampleSizeValue = sourceObservation.SampleSizeValue;
                obs.SamplingEffort = sourceObservation.SamplingEffort;
                obs.SamplingProtocol = sourceObservation.SamplingProtocol;
                obs.EventRemarks = sourceObservation.EventRemarks;

                // Location
                obs.LocationID = sourceObservation.LocationID;
                obs.HigherGeographyID = sourceObservation.HigherGeographyID;
                obs.HigherGeography = sourceObservation.HigherGeography;
                obs.Continent = sourceObservation.Continent;
                obs.WaterBody = sourceObservation.WaterBody;
                obs.Island = sourceObservation.Island;
                obs.IslandGroup = sourceObservation.IslandGroup;
                obs.CoordinatePrecision = sourceObservation.CoordinatePrecision;
                obs.CoordinateUncertaintyInMeters = sourceObservation.CoordinateUncertaintyInMeters;
                obs.Country = sourceObservation.Country;
                obs.CountryCode = sourceObservation.CountryCode;
                obs.County = sourceObservation.County;
                obs.StateProvince = sourceObservation.StateProvince;
                obs.Municipality = sourceObservation.Municipality;
                obs.Locality = sourceObservation.Locality;
                obs.VerbatimCoordinates = sourceObservation.VerbatimCoordinates;
                obs.VerbatimCoordinateSystem = sourceObservation.VerbatimCoordinateSystem;
                obs.VerbatimDepth = sourceObservation.VerbatimDepth;
                obs.VerbatimElevation = sourceObservation.VerbatimElevation;
                obs.VerbatimLatitude = sourceObservation.VerbatimLatitude;
                obs.VerbatimLocality = sourceObservation.VerbatimLocality;
                obs.VerbatimLongitude = sourceObservation.VerbatimLongitude;
                obs.VerbatimSRS = sourceObservation.VerbatimSRS;
                obs.MaximumDepthInMeters = sourceObservation.MaximumDepthInMeters;
                obs.MaximumDistanceAboveSurfaceInMeters = sourceObservation.MaximumDistanceAboveSurfaceInMeters;
                obs.MaximumElevationInMeters = sourceObservation.MaximumElevationInMeters;
                obs.MinimumDepthInMeters = sourceObservation.MinimumDepthInMeters;
                obs.MinimumDistanceAboveSurfaceInMeters = sourceObservation.MinimumDistanceAboveSurfaceInMeters;
                obs.MinimumElevationInMeters = sourceObservation.MinimumElevationInMeters;
                obs.LocationAccordingTo = sourceObservation.LocationAccordingTo;
                obs.LocationRemarks = sourceObservation.LocationRemarks;
                obs.DecimalLatitude = sourceObservation.DecimalLatitude;
                obs.DecimalLongitude = sourceObservation.DecimalLongitude;
                obs.GeodeticDatum = sourceObservation.GeodeticDatum;
                obs.PointRadiusSpatialFit = sourceObservation.PointRadiusSpatialFit;
                obs.FootprintSpatialFit = sourceObservation.FootprintSpatialFit;
                obs.FootprintSRS = sourceObservation.FootprintSRS;
                obs.FootprintWKT = sourceObservation.FootprintWKT;
                obs.GeoreferencedBy = sourceObservation.GeoreferencedBy;
                obs.GeoreferencedDate = sourceObservation.GeoreferencedDate;
                obs.GeoreferenceProtocol = sourceObservation.GeoreferenceProtocol;
                obs.GeoreferenceRemarks = sourceObservation.GeoreferenceRemarks;
                obs.GeoreferenceSources = sourceObservation.GeoreferenceSources;

                // GeologicalContext
                obs.GeologicalContextID = sourceObservation.GeologicalContextID;
                obs.EarliestAgeOrLowestStage = sourceObservation.EarliestAgeOrLowestStage;
                obs.EarliestEonOrLowestEonothem = sourceObservation.EarliestEonOrLowestEonothem;
                obs.EarliestEpochOrLowestSeries = sourceObservation.EarliestEpochOrLowestSeries;
                obs.EarliestEraOrLowestErathem = sourceObservation.EarliestEraOrLowestErathem;
                obs.EarliestGeochronologicalEra = sourceObservation.EarliestGeochronologicalEra;
                obs.EarliestPeriodOrLowestSystem = sourceObservation.EarliestPeriodOrLowestSystem;
                obs.Formation = sourceObservation.Formation;
                obs.FromLithostratigraphicUnit = sourceObservation.FromLithostratigraphicUnit;
                obs.Group = sourceObservation.Group;
                obs.HighestBiostratigraphicZone = sourceObservation.HighestBiostratigraphicZone;
                obs.LatestAgeOrHighestStage = sourceObservation.LatestAgeOrHighestStage;
                obs.LatestEonOrHighestEonothem = sourceObservation.LatestEonOrHighestEonothem;
                obs.LatestEpochOrHighestSeries = sourceObservation.LatestEpochOrHighestSeries;
                obs.LatestEraOrHighestErathem = sourceObservation.LatestEraOrHighestErathem;
                obs.LatestGeochronologicalEra = sourceObservation.LatestGeochronologicalEra;
                obs.LatestPeriodOrHighestSystem = sourceObservation.LatestPeriodOrHighestSystem;
                obs.LithostratigraphicTerms = sourceObservation.LithostratigraphicTerms;
                obs.LowestBiostratigraphicZone = sourceObservation.LowestBiostratigraphicZone;
                obs.Member = sourceObservation.Member;
                obs.Bed = sourceObservation.Bed;

                // Identification
                obs.IdentificationID = sourceObservation.IdentificationID;
                obs.IdentificationQualifier = sourceObservation.IdentificationQualifier;
                obs.IdentificationReferences = sourceObservation.IdentificationReferences;
                obs.IdentificationRemarks = sourceObservation.IdentificationRemarks;
                obs.IdentificationVerificationStatus = sourceObservation.IdentificationVerificationStatus;
                obs.IdentifiedBy = sourceObservation.IdentifiedBy;
                obs.TypeStatus = sourceObservation.TypeStatus;
                obs.DateIdentified = sourceObservation.DateIdentified;

                // Taxon
                obs.Kingdom = sourceObservation.Kingdom;
                obs.AcceptedNameUsage = sourceObservation.AcceptedNameUsage;
                obs.AcceptedNameUsageID = sourceObservation.AcceptedNameUsageID;
                obs.TaxonConceptID = sourceObservation.TaxonConceptID;
                obs.TaxonID = sourceObservation.TaxonID;
                obs.TaxonomicStatus = sourceObservation.TaxonomicStatus;
                obs.TaxonRank = sourceObservation.TaxonRank;
                obs.TaxonRemarks = sourceObservation.TaxonRemarks;
                obs.Class = sourceObservation.Class;
                obs.Family = sourceObservation.Family;
                obs.Genus = sourceObservation.Genus;
                obs.HigherClassification = sourceObservation.HigherClassification;
                obs.InfraspecificEpithet = sourceObservation.InfraspecificEpithet;
                obs.NameAccordingTo = sourceObservation.NameAccordingTo;
                obs.NameAccordingToID = sourceObservation.NameAccordingToID;
                obs.NamePublishedIn = sourceObservation.NamePublishedIn;
                obs.NamePublishedInID = sourceObservation.NamePublishedInID;
                obs.NamePublishedInYear = sourceObservation.NamePublishedInYear;
                obs.NomenclaturalCode = sourceObservation.NomenclaturalCode;
                obs.NomenclaturalStatus = sourceObservation.NomenclaturalStatus;
                obs.Order = sourceObservation.Order;
                obs.OriginalNameUsage = sourceObservation.OriginalNameUsage;
                obs.OriginalNameUsageID = sourceObservation.OriginalNameUsageID;
                obs.ParentNameUsage = sourceObservation.ParentNameUsage;
                obs.ParentNameUsageID = sourceObservation.ParentNameUsageID;
                obs.Phylum = sourceObservation.Phylum;
                obs.ScientificName = sourceObservation.ScientificName;
                obs.ScientificNameAuthorship = sourceObservation.ScientificNameAuthorship;
                obs.ScientificNameID = sourceObservation.ScientificNameID;
                obs.Species = sourceObservation.Species;
                obs.SpecificEpithet = sourceObservation.SpecificEpithet;
                obs.Subgenus = sourceObservation.Subgenus;
                obs.VerbatimTaxonRank = sourceObservation.VerbatimTaxonRank;
                obs.VernacularName = sourceObservation.VernacularName;

                // UseWithIRI
                obs.ToTaxon = sourceObservation.ToTaxon;
                obs.InCollection = sourceObservation.InCollection;
                obs.InDataset = sourceObservation.InDataset;
                obs.InDescribedPlace = sourceObservation.InDescribedPlace;
            });

            return operable;
        }

        public static IOperable<DwcObservationVerbatim> HaveSensitiveSpeciesTaxonId(this IOperable<DwcObservationVerbatim> operable)
        {
            var builder = ((IDeclaration<DwcObservationVerbatim>)operable).ObjectBuilder;
            builder.With((obs, index) =>
            {
                var sensitiveTaxonId = Pick<int>.RandomItemFrom(ProtectedSpeciesHelper.SensitiveSpeciesTaxonIds);
                obs.TaxonID = $"urn:lsid:dyntaxa:{sensitiveTaxonId}";
            });

            return operable;
        }
    }
}