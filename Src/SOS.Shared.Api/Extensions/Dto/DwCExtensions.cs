using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;

using SOS.Shared.Api.Dtos.Observation;

namespace SOS.Shared.Api.Extensions.Dto;

public static class DwCExtensions
{
    extension(Observation observation)
    {
        public DarwinCoreOccurrenceDto ToDto()
        {
            if (observation == null)
            {
                return null!;
            }

            var dto = new DarwinCoreOccurrenceDto
            {
                AccessRights = observation.AccessRights?.Value!,
                BasisOfRecord = observation.BasisOfRecord?.Value!,
                Bed = null!,
                BibliographicCitation = observation.BibliographicCitation,
                CollectionCode = observation.CollectionCode,
                CollectionID = observation.CollectionId,
                CultivarEpithet = null!,
                DataGeneralizations = null!,
                DatasetID = observation.DatasetId,
                DatasetName = observation.DatasetName,
                DegreeOfEstablishment = null!,
                Disposition = null!,
                DynamicProperties = observation.DynamicProperties,
                EarliestAgeOrLowestStage = null!,
                EarliestEonOrLowestEonothem = null!,
                EarliestEpochOrLowestSeries = null!,
                EarliestEraOrLowestErathem = null!,
                EarliestPeriodOrLowestSystem = null!,
                EstablishmentMeans = null!,
                FieldNotes = null!,
                FieldNumber = null!,
                Formation = null!,
                GenericName = null!,
                Group = null!,
                HighestBiostratigraphicZone = null!,
                IdentifiedByID = null!,
                InformationWithheld = observation.InformationWithheld,
                InstitutionCode = observation.InstitutionCode?.Value!,
                InstitutionID = observation.InstitutionId,
                Language = observation.Language,
                LatestAgeOrHighestStage = null!,
                LatestEonOrHighestEonothem = null!,
                LatestEpochOrHighestSeries = null!,
                LatestEraOrHighestErathem = null!,
                LatestPeriodOrHighestSystem = null!,
                License = observation.License,
                LithostratigraphicTerms = null!,
                LowestBiostratigraphicZone = null!,
                Member = null!,
                Modified = observation.Modified,
                NomenclaturalCode = null!,
                NomenclaturalStatus = null!,
                OwnerInstitutionCode = observation.OwnerInstitutionCode,
                Pathway = null!,
                Preparations = null!,
                RecordedByID = null!,
                References = observation.References,
                RightsHolder = observation.RightsHolder,
            };

            if (observation.Event != null)
            {
                dto.Day = observation.Event.StartDay;
                dto.EndDayOfYear = observation.Event.EndDay;
                dto.EventDate = DwcFormatter.CreateDateIntervalString(observation.Event.StartDate, observation.Event.EndDate);
                dto.EventID = observation.Event.EventId;
                dto.EventRemarks = observation.Event.EventRemarks;
                dto.EventTime = observation.Event.PlainStartTime;
                dto.Habitat = observation.Event.Habitat;
                dto.Month = observation.Event.StartMonth;
                dto.ParentEventID = observation.Event.ParentEventId;
                dto.SampleSizeUnit = observation.Event.SampleSizeUnit;
                dto.SampleSizeValue = observation.Event.SampleSizeValue;
                dto.SamplingEffort = observation.Event.SamplingEffort;
                dto.SamplingProtocol = observation.Event.SamplingProtocol;
                dto.StartDayOfYear = observation.Event.StartDay;
            }

            if (observation.Identification != null)
            {
                dto.DateIdentified = observation.Identification.DateIdentified;
                dto.IdentificationID = observation.Identification.IdentificationId;
                dto.IdentificationQualifier = observation.Identification.IdentificationQualifier;
                dto.IdentificationReferences = observation.Identification.IdentificationReferences;
                dto.IdentificationRemarks = observation.Identification.IdentificationRemarks;
                dto.IdentificationVerificationStatus = observation.Identification.VerificationStatus?.Value!;
                dto.IdentifiedBy = observation.Identification.IdentifiedBy;
            }

            if (observation.Location != null)
            {
                dto.Continent = observation.Location.Continent?.Value!;
                dto.CoordinatePrecision = observation.Location.CoordinatePrecision;
                dto.CoordinateUncertaintyInMeters = observation.Location.CoordinateUncertaintyInMeters;
                dto.Country = observation.Location.Country?.Value!;
                dto.CountryRegion = observation.Location.CountryRegion?.Name!;
                dto.CountryCode = observation.Location.CountryCode;
                dto.County = observation.Location.County?.Name!;
                dto.DecimalLatitude = observation.Location.DecimalLatitude;
                dto.DecimalLongitude = observation.Location.DecimalLongitude;
                dto.FootprintSpatialFit = observation.Location.FootprintSpatialFit;
                dto.FootprintSRS = observation.Location.FootprintSRS;
                dto.FootprintWKT = observation.Location.FootprintWKT;
                dto.GeodeticDatum = observation.Location.GeodeticDatum;
                dto.GeoreferencedBy = observation.Location.GeoreferencedBy;
                dto.GeoreferencedDate = observation.Location.GeoreferencedDate;
                dto.GeoreferenceProtocol = observation.Location.GeoreferenceProtocol;
                dto.GeoreferenceRemarks = observation.Location.GeoreferenceRemarks;
                dto.GeoreferenceSources = observation.Location.GeoreferenceSources;
                dto.GeoreferenceVerificationStatus = observation.Location.GeoreferenceVerificationStatus;
                dto.HigherGeography = observation.Location.HigherGeography;
                dto.HigherGeographyID = observation.Location.HigherGeographyId;
                dto.IslandGroup = observation.Location.IslandGroup;
                dto.Locality = observation.Location.Locality;
                dto.LocationAccordingTo = observation.Location.LocationAccordingTo;
                dto.LocationID = observation.Location.LocationId;
                dto.LocationRemarks = observation.Location.LocationRemarks;
                dto.MaximumDepthInMeters = observation.Location.MaximumDepthInMeters;
                dto.MaximumDistanceAboveSurfaceInMeters = observation.Location.MaximumDistanceAboveSurfaceInMeters;
                dto.MaximumElevationInMeters = observation.Location.MaximumElevationInMeters;
                dto.MinimumDepthInMeters = observation.Location.MinimumDepthInMeters;
                dto.MinimumDistanceAboveSurfaceInMeters = observation.Location.MinimumDistanceAboveSurfaceInMeters;
                dto.MinimumElevationInMeters = observation.Location.MinimumElevationInMeters;
                dto.Municipality = observation.Location.Municipality?.Name!;
                dto.PointRadiusSpatialFit = observation.Location.PointRadiusSpatialFit;
                dto.StateProvince = observation.Location.Province?.Name!;
                dto.VerbatimLatitude = observation.Location.VerbatimLatitude;
                dto.VerbatimLongitude = observation.Location.VerbatimLongitude;
                dto.VerbatimCoordinateSystem = observation.Location.VerbatimCoordinateSystem;
                dto.VerbatimSRS = observation.Location.VerbatimSRS;
            }

            if (observation.MaterialSample != null)
            {
                dto.MaterialSampleID = observation.MaterialSample.MaterialSampleId;
            }

            if (observation.Occurrence != null)
            {
                dto.AssociatedMedia = observation.Occurrence.AssociatedMedia;
                dto.AssociatedOccurrences = observation.Occurrence.AssociatedOccurrences;
                dto.AssociatedReferences = observation.Occurrence.AssociatedReferences;
                dto.AssociatedSequences = observation.Occurrence.AssociatedSequences;
                dto.AssociatedTaxa = observation.Occurrence.AssociatedTaxa;
                dto.Behavior = observation.Occurrence.Behavior?.Value!;
                dto.CatalogNumber = observation.Occurrence.CatalogNumber;
                dto.IndividualCount = observation.Occurrence.IndividualCount;
                dto.LifeStage = observation.Occurrence.LifeStage?.Value!;
                dto.Media = observation.Occurrence.Media;
                dto.OccurrenceID = observation.Occurrence.OccurrenceId;
                dto.OccurrenceRemarks = observation.Occurrence.OccurrenceRemarks;
                dto.OccurrenceStatus = observation.Occurrence.OccurrenceStatus?.Value!;
                dto.OrganismQuantity = observation.Occurrence.OrganismQuantity;
                dto.OrganismQuantityType = observation.Occurrence.OrganismQuantityUnit?.Value!;
                dto.OtherCatalogNumbers = observation.Occurrence.OtherCatalogNumbers;
                dto.RecordedBy = observation.Occurrence.RecordedBy;
                dto.RecordNumber = observation.Occurrence.RecordNumber;
                dto.ReproductiveCondition = observation.Occurrence.ReproductiveCondition?.Value!;
                dto.Sex = observation.Occurrence.Sex?.Value!;
            }

            if (observation.Organism != null)
            {
                dto.AssociatedOrganisms = observation.Organism.AssociatedOrganisms;
                dto.OrganismID = observation.Organism.OrganismId;
                dto.OrganismName = observation.Organism.OrganismName;
                dto.OrganismRemarks = observation.Organism.OrganismRemarks;
                dto.OrganismScope = observation.Organism.OrganismScope;
                dto.PreviousIdentifications = observation.Organism.PreviousIdentifications;
            }

            if (observation.Taxon != null)
            {
                dto.AcceptedNameUsage = observation.Taxon.AcceptedNameUsage;
                dto.AcceptedNameUsageID = observation.Taxon.AcceptedNameUsageId;
                dto.Class = observation.Taxon.Class;
                dto.Family = observation.Taxon.Family;
                dto.Genus = observation.Taxon?.Genus!;
                dto.HigherClassification = observation.Taxon.HigherClassification;
                dto.InfragenericEpithet = observation.Taxon.InfraspecificEpithet!;
                dto.InfraspecificEpithet = observation.Taxon.InfraspecificEpithet!;
                dto.Kingdom = observation.Taxon.Kingdom!;
                dto.NameAccordingTo = observation.Taxon.NameAccordingTo!;
                dto.NameAccordingToID = observation.Taxon.NameAccordingToId!;
                dto.NamePublishedIn = observation.Taxon.NamePublishedIn!;
                dto.NamePublishedInID = observation.Taxon.NamePublishedInId!;
                dto.NamePublishedInYear = observation.Taxon.NamePublishedInYear!;
                dto.Order = observation.Taxon.Order;
                dto.OriginalNameUsage = observation.Taxon.OriginalNameUsage;
                dto.OriginalNameUsageID = observation.Taxon.OriginalNameUsageId;
                dto.ParentNameUsage = observation.Taxon.ParentNameUsage;
                dto.ParentNameUsageID = observation.Taxon.ParentNameUsageId;
                dto.Phylum = observation.Taxon.Phylum;
                dto.ScientificName = observation.Taxon.ScientificName;
                dto.ScientificNameAuthorship = observation.Taxon.ScientificNameAuthorship;
                dto.ScientificNameID = observation.Taxon.ScientificNameId;
                dto.SpecificEpithet = observation.Taxon.SpecificEpithet;
                dto.VernacularName = observation.Taxon.VernacularName;
                dto.TaxonRank = observation.Taxon.TaxonRank;
                dto.TaxonomicStatus = observation.Taxon.TaxonomicStatus;
                dto.TaxonRemarks = observation.Taxon.TaxonRemarks;
            }

            return dto;
        }
    }
}