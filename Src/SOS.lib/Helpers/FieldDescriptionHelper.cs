using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using SOS.Export.Models;
using SOS.Lib.Enums;

namespace SOS.Lib.Helpers
{
    // todo - move to SOS.Lib
    /// <summary>
    ///     Field descriptions.
    /// </summary>
    public static class FieldDescriptionHelper
    {
        private static readonly List<FieldDescription> AllFields;
        private static readonly Dictionary<string, FieldDescription> AllFieldsByName;
        private static readonly Dictionary<int, FieldDescription> AllFieldsById;
        private static readonly Dictionary<FieldDescriptionId, FieldDescription> AllFieldsByFieldDescriptionId;

        private static readonly FieldDescriptionId[] MandatoryDwcFields =
        {
            FieldDescriptionId.OccurrenceID,
            FieldDescriptionId.BasisOfRecord,
            FieldDescriptionId.InstitutionCode,
            FieldDescriptionId.CollectionCode,
            FieldDescriptionId.CatalogNumber,
            FieldDescriptionId.ScientificName,
            FieldDescriptionId.DecimalLongitude,
            FieldDescriptionId.DecimalLatitude,
            FieldDescriptionId.GeodeticDatum,
            FieldDescriptionId.CoordinateUncertaintyInMeters,
            FieldDescriptionId.EventDate
        };

        static FieldDescriptionHelper()
        {
            AllFields = LoadFieldDescriptions().ToList();
            AllFieldsByName = AllFields.ToDictionary(x => x.Name, x => x);
            AllFieldsById = AllFields.ToDictionary(x => x.Id, x => x);
            AllFieldsByFieldDescriptionId = AllFields.ToDictionary(x => (FieldDescriptionId)x.Id, x => x);
        }

        private static List<FieldDescription> LoadFieldDescriptions()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, @"Resources\FieldDescriptions.json");
            using var fs = FileSystemHelper.WaitForFileAndThenOpenIt(filePath);
            var fields = JsonSerializer.DeserializeAsync<List<FieldDescription>>(fs).Result;
            return fields;
        }

        public static IEnumerable<FieldDescription> GetDefaultDwcExportFieldDescriptions()
        {
            var fieldIds = GetDefaultDwcExportFieldDescriptionIds();
            return GetFieldDescriptions(fieldIds);
        }

        public static IEnumerable<FieldDescriptionId> GetDefaultDwcExportFieldDescriptionIds()
        {
            return AllFields
                .Where(x => x.IncludedByDefaultInDwcExport)
                .Select(x => (FieldDescriptionId)x.Id)
                .AddMandatoryFieldDescriptionIdsFirst();
        }

        public static IEnumerable<FieldDescription> GetAllFieldDescriptions()
        {
            return AllFields;
        }

        public static IEnumerable<FieldDescription> AddMandatoryFieldDescriptions(
            IEnumerable<FieldDescriptionId> fieldDescriptions)
        {
            var fieldIds = AddMandatoryFieldDescriptionIds(fieldDescriptions);
            return AllFieldsByFieldDescriptionId
                .Where(x => fieldIds.Contains(x.Key))
                .Select(x => x.Value);
        }

        public static IEnumerable<FieldDescription> GetFieldDescriptions(
            IEnumerable<FieldDescriptionId> fieldDescriptionIds)
        {
            var fieldDescriptions = new List<FieldDescription>();
            foreach (var fieldDescriptionId in fieldDescriptionIds)
            {
                fieldDescriptions.Add(AllFieldsByFieldDescriptionId[fieldDescriptionId]);
            }

            return fieldDescriptions;
        }

        public static IEnumerable<FieldDescriptionId> AddMandatoryFieldDescriptionIds(
            IEnumerable<FieldDescriptionId> fieldDescriptions)
        {
            return MandatoryDwcFields.Union(fieldDescriptions);
        }

        public static IEnumerable<FieldDescriptionId> AddMandatoryFieldDescriptionIdsFirst(
            this IEnumerable<FieldDescriptionId> fieldDescriptions)
        {
            return MandatoryDwcFields.Union(fieldDescriptions);
        }

        public static FieldDescription GetFieldDescription(FieldDescriptionId fieldDescriptionId)
        {
            return AllFieldsByFieldDescriptionId[fieldDescriptionId];
        }

        public static IEnumerable<FieldDescriptionId> GetMissingFieldDescriptionIds(
            IEnumerable<FieldDescriptionId> fieldDescriptionIds)
        {
            return AllFields.Select(x => (FieldDescriptionId)x.Id).Except(fieldDescriptionIds);
        }

        /// <summary>
        /// Create a bool array that holds data about whether a field should be written to a DwC-A occurrence CSV file.
        /// </summary>
        /// <param name="fieldDescriptions"></param>
        /// <remarks>A bool array is much faster to use than using a Dictionary of {FieldDescriptionId, bool}</remarks>
        /// <returns></returns>
        public static bool[] CreateWriteFieldsArray(IEnumerable<FieldDescription> fieldDescriptions)
        {
            var fieldDescriptionIdsSet = new HashSet<FieldDescriptionId>();
            foreach (var fieldDescription in fieldDescriptions)
            {
                fieldDescriptionIdsSet.Add((FieldDescriptionId)fieldDescription.Id);
            }

            bool[] writeField = new bool[AllFields.Max(f => f.Id) + 1];
            foreach (var field in AllFields.OrderBy(m => m.Id))
            {
                if (fieldDescriptionIdsSet.Contains(field.FieldDescriptionId))
                {
                    writeField[field.Id] = true;
                }
            }

            return writeField;
        }

        /// <summary>
        /// Get a subset of important DwC fields used for testing purpose. It's easier to debug data and the exports are faster.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<FieldDescription> GetDwcFieldDescriptionsForTestingPurpose()
        {
            return GetFieldDescriptions(DwcFieldDescriptionForTestingPurpose);
        }

        private static readonly FieldDescriptionId[] DwcFieldDescriptionForTestingPurpose = {
            FieldDescriptionId.OccurrenceID,
            FieldDescriptionId.BasisOfRecord,
            FieldDescriptionId.ScientificName,
            FieldDescriptionId.Kingdom,
            FieldDescriptionId.DecimalLongitude,
            FieldDescriptionId.DecimalLatitude,
            FieldDescriptionId.CoordinateUncertaintyInMeters,
            FieldDescriptionId.GeodeticDatum,
            FieldDescriptionId.EventDate,
            FieldDescriptionId.EventTime,
            FieldDescriptionId.CollectionCode,
            FieldDescriptionId.DatasetName,
            FieldDescriptionId.RecordedBy,
            FieldDescriptionId.LifeStage,
            FieldDescriptionId.IdentificationVerificationStatus,
            FieldDescriptionId.SamplingProtocol,
            FieldDescriptionId.Country,
            FieldDescriptionId.InstitutionCode,
            FieldDescriptionId.CatalogNumber
        };

        public static IEnumerable<FieldDescription> GetAllDwcOccurrenceCoreFieldDescriptions()
        {
            return GetFieldDescriptions(AllDwcOccurrenceCoreFieldDescriptions);
        }

        public static IEnumerable<FieldDescription> GetAllEventDwcOccurrenceCoreFieldDescriptions()
        {
            var fieldDescriptions = GetFieldDescriptions(AllDwcOccurrenceCoreFieldDescriptions).ToList();
            var eventIdFieldDescription =
                fieldDescriptions.First(m => m.FieldDescriptionId == FieldDescriptionId.EventID);
            fieldDescriptions.Remove(eventIdFieldDescription);
            fieldDescriptions.Insert(0, eventIdFieldDescription);
            return fieldDescriptions;
        }

        private static readonly FieldDescriptionId[] AllDwcOccurrenceCoreFieldDescriptions =
        {
            FieldDescriptionId.OccurrenceID,
            FieldDescriptionId.BasisOfRecord,
            FieldDescriptionId.BibliographicCitation,
            FieldDescriptionId.CollectionCode,
            FieldDescriptionId.CollectionID,
            FieldDescriptionId.DataGeneralizations,
            FieldDescriptionId.DatasetID,
            FieldDescriptionId.DatasetName,
            FieldDescriptionId.DynamicProperties,
            FieldDescriptionId.InformationWithheld,
            FieldDescriptionId.InstitutionCode,
            FieldDescriptionId.InstitutionID,
            FieldDescriptionId.Language,
            FieldDescriptionId.License,
            FieldDescriptionId.Modified,
            FieldDescriptionId.OwnerInstitutionCode,
            FieldDescriptionId.References,
            FieldDescriptionId.RightsHolder,
            FieldDescriptionId.Type,
            FieldDescriptionId.Day,
            FieldDescriptionId.EndDayOfYear,
            FieldDescriptionId.EventDate,
            FieldDescriptionId.EventID,
            FieldDescriptionId.EventRemarks,
            FieldDescriptionId.EventTime,
            FieldDescriptionId.FieldNotes,
            FieldDescriptionId.FieldNumber,
            FieldDescriptionId.Habitat,
            FieldDescriptionId.Month,
            FieldDescriptionId.ParentEventID,
            FieldDescriptionId.SampleSizeValue,
            FieldDescriptionId.SampleSizeUnit,
            FieldDescriptionId.SamplingEffort,
            FieldDescriptionId.SamplingProtocol,
            FieldDescriptionId.StartDayOfYear,
            FieldDescriptionId.VerbatimEventDate,
            FieldDescriptionId.Year,
            FieldDescriptionId.DateIdentified,
            FieldDescriptionId.IdentificationID,
            FieldDescriptionId.IdentificationQualifier,
            FieldDescriptionId.IdentificationReferences,
            FieldDescriptionId.IdentificationRemarks,
            FieldDescriptionId.IdentificationVerificationStatus,
            FieldDescriptionId.IdentifiedBy,
            FieldDescriptionId.TypeStatus,
            FieldDescriptionId.Continent,
            FieldDescriptionId.CoordinatePrecision,
            FieldDescriptionId.CoordinateUncertaintyInMeters,
            FieldDescriptionId.Country,
            FieldDescriptionId.CountryCode,
            FieldDescriptionId.County,
            FieldDescriptionId.DecimalLatitude,
            FieldDescriptionId.DecimalLongitude,
            FieldDescriptionId.FootprintSpatialFit,
            FieldDescriptionId.FootprintSRS,
            FieldDescriptionId.FootprintWKT,
            FieldDescriptionId.GeodeticDatum,
            FieldDescriptionId.GeoreferencedBy,
            FieldDescriptionId.GeoreferencedDate,
            FieldDescriptionId.GeoreferenceProtocol,
            FieldDescriptionId.GeoreferenceRemarks,
            FieldDescriptionId.GeoreferenceSources,
            FieldDescriptionId.GeoreferenceVerificationStatus,
            FieldDescriptionId.HigherGeography,
            FieldDescriptionId.HigherGeographyID,
            FieldDescriptionId.Island,
            FieldDescriptionId.IslandGroup,
            FieldDescriptionId.Locality,
            FieldDescriptionId.LocationAccordingTo,
            FieldDescriptionId.LocationID,
            FieldDescriptionId.LocationRemarks,
            FieldDescriptionId.MaximumDepthInMeters,
            FieldDescriptionId.MaximumDistanceAboveSurfaceInMeters,
            FieldDescriptionId.MaximumElevationInMeters,
            FieldDescriptionId.MinimumDepthInMeters,
            FieldDescriptionId.MinimumDistanceAboveSurfaceInMeters,
            FieldDescriptionId.MinimumElevationInMeters,
            FieldDescriptionId.Municipality,
            FieldDescriptionId.PointRadiusSpatialFit,
            FieldDescriptionId.StateProvince,
            FieldDescriptionId.WaterBody,
            FieldDescriptionId.VerbatimCoordinates,
            FieldDescriptionId.VerbatimCoordinateSystem,
            FieldDescriptionId.VerbatimDepth,
            FieldDescriptionId.VerbatimElevation,
            FieldDescriptionId.VerbatimLatitude,
            FieldDescriptionId.VerbatimLocality,
            FieldDescriptionId.VerbatimLongitude,
            FieldDescriptionId.VerbatimSRS,
            FieldDescriptionId.AssociatedMedia,
            FieldDescriptionId.AssociatedReferences,
            FieldDescriptionId.AssociatedSequences,
            FieldDescriptionId.AssociatedTaxa,
            FieldDescriptionId.Behavior,
            FieldDescriptionId.CatalogNumber,
            FieldDescriptionId.Disposition,
            FieldDescriptionId.EstablishmentMeans,
            FieldDescriptionId.IndividualCount,
            FieldDescriptionId.LifeStage,
            FieldDescriptionId.AccessRights,
            FieldDescriptionId.OccurrenceRemarks,
            FieldDescriptionId.OccurrenceStatus,
            FieldDescriptionId.OrganismQuantity,
            FieldDescriptionId.OrganismQuantityType,
            FieldDescriptionId.OtherCatalogNumbers,
            FieldDescriptionId.Preparations,
            FieldDescriptionId.RecordedBy,
            FieldDescriptionId.RecordNumber,
            FieldDescriptionId.ReproductiveCondition,
            FieldDescriptionId.Sex,
            FieldDescriptionId.AcceptedNameUsage,
            FieldDescriptionId.AcceptedNameUsageID,
            FieldDescriptionId.Class,
            FieldDescriptionId.Family,
            FieldDescriptionId.Genus,
            FieldDescriptionId.HigherClassification,
            FieldDescriptionId.InfraspecificEpithet,
            FieldDescriptionId.Kingdom,
            FieldDescriptionId.NameAccordingTo,
            FieldDescriptionId.NameAccordingToID,
            FieldDescriptionId.NamePublishedIn,
            FieldDescriptionId.NamePublishedInID,
            FieldDescriptionId.NamePublishedInYear,
            FieldDescriptionId.NomenclaturalCode,
            FieldDescriptionId.NomenclaturalStatus,
            FieldDescriptionId.Order,
            FieldDescriptionId.OriginalNameUsage,
            FieldDescriptionId.OriginalNameUsageID,
            FieldDescriptionId.ParentNameUsage,
            FieldDescriptionId.ParentNameUsageID,
            FieldDescriptionId.Phylum,
            FieldDescriptionId.ScientificName,
            FieldDescriptionId.ScientificNameAuthorship,
            FieldDescriptionId.ScientificNameID,
            FieldDescriptionId.SpecificEpithet,
            FieldDescriptionId.Subgenus,
            FieldDescriptionId.TaxonConceptID,
            FieldDescriptionId.TaxonID,
            FieldDescriptionId.TaxonomicStatus,
            FieldDescriptionId.TaxonRank,
            FieldDescriptionId.TaxonRemarks,
            FieldDescriptionId.VerbatimTaxonRank,
            FieldDescriptionId.VernacularName,
            FieldDescriptionId.Bed,
            FieldDescriptionId.EarliestAgeOrLowestStage,
            FieldDescriptionId.EarliestEonOrLowestEonothem,
            FieldDescriptionId.EarliestEpochOrLowestSeries,
            FieldDescriptionId.EarliestEraOrLowestErathem,
            FieldDescriptionId.EarliestPeriodOrLowestSystem,
            FieldDescriptionId.Formation,
            FieldDescriptionId.GeologicalContextID,
            FieldDescriptionId.Group,
            FieldDescriptionId.HighestBiostratigraphicZone,
            FieldDescriptionId.LatestAgeOrHighestStage,
            FieldDescriptionId.LatestEonOrHighestEonothem,
            FieldDescriptionId.LatestEpochOrHighestSeries,
            FieldDescriptionId.LatestEraOrHighestErathem,
            FieldDescriptionId.LatestPeriodOrHighestSystem,
            FieldDescriptionId.LithostratigraphicTerms,
            FieldDescriptionId.LowestBiostratigraphicZone,
            FieldDescriptionId.Member,
            FieldDescriptionId.MaterialSampleID
        };

        public static IEnumerable<FieldDescription> GetAllDwcEventCoreFieldDescriptions()
        {
            return GetFieldDescriptions(AllDwcEventCoreFieldDescriptions);
        }

        private static readonly FieldDescriptionId[] AllDwcEventCoreFieldDescriptions =
        {
            FieldDescriptionId.EventID
            ,FieldDescriptionId.ParentEventID
            ,FieldDescriptionId.EventDate
            ,FieldDescriptionId.VerbatimEventDate
            ,FieldDescriptionId.EventTime
            ,FieldDescriptionId.EventRemarks
            ,FieldDescriptionId.FieldNotes
            ,FieldDescriptionId.FieldNumber
            ,FieldDescriptionId.Habitat
            ,FieldDescriptionId.SampleSizeValue
            ,FieldDescriptionId.SampleSizeUnit
            ,FieldDescriptionId.SamplingEffort
            ,FieldDescriptionId.SamplingProtocol
            ,FieldDescriptionId.Day
            ,FieldDescriptionId.Month
            ,FieldDescriptionId.Year
            ,FieldDescriptionId.EndDayOfYear
            ,FieldDescriptionId.StartDayOfYear
            ,FieldDescriptionId.BibliographicCitation
            ,FieldDescriptionId.DataGeneralizations
            ,FieldDescriptionId.DatasetID
            ,FieldDescriptionId.DatasetName
            ,FieldDescriptionId.DynamicProperties
            ,FieldDescriptionId.InformationWithheld
            ,FieldDescriptionId.InstitutionCode
            ,FieldDescriptionId.InstitutionID
            ,FieldDescriptionId.Language
            ,FieldDescriptionId.License
            ,FieldDescriptionId.Modified
            ,FieldDescriptionId.OwnerInstitutionCode
            ,FieldDescriptionId.References
            ,FieldDescriptionId.RightsHolder
            ,FieldDescriptionId.Type
            ,FieldDescriptionId.CoordinatePrecision
            ,FieldDescriptionId.CoordinateUncertaintyInMeters
            ,FieldDescriptionId.Country
            ,FieldDescriptionId.CountryCode
            ,FieldDescriptionId.County
            ,FieldDescriptionId.DecimalLatitude
            ,FieldDescriptionId.DecimalLongitude
            ,FieldDescriptionId.FootprintSpatialFit
            ,FieldDescriptionId.FootprintSRS
            ,FieldDescriptionId.FootprintWKT
            ,FieldDescriptionId.GeodeticDatum
            ,FieldDescriptionId.GeoreferencedBy
            ,FieldDescriptionId.GeoreferencedDate
            ,FieldDescriptionId.GeoreferenceProtocol
            ,FieldDescriptionId.GeoreferenceRemarks
            ,FieldDescriptionId.GeoreferenceSources
            ,FieldDescriptionId.GeoreferenceVerificationStatus
            ,FieldDescriptionId.HigherGeography
            ,FieldDescriptionId.HigherGeographyID
            ,FieldDescriptionId.Continent
            ,FieldDescriptionId.Island
            ,FieldDescriptionId.IslandGroup
            ,FieldDescriptionId.Locality
            ,FieldDescriptionId.LocationAccordingTo
            ,FieldDescriptionId.LocationID
            ,FieldDescriptionId.LocationRemarks
            ,FieldDescriptionId.MaximumDepthInMeters
            ,FieldDescriptionId.MaximumDistanceAboveSurfaceInMeters
            ,FieldDescriptionId.MaximumElevationInMeters
            ,FieldDescriptionId.MinimumDepthInMeters
            ,FieldDescriptionId.MinimumDistanceAboveSurfaceInMeters
            ,FieldDescriptionId.MinimumElevationInMeters
            ,FieldDescriptionId.Municipality
            ,FieldDescriptionId.PointRadiusSpatialFit
            ,FieldDescriptionId.StateProvince
            ,FieldDescriptionId.WaterBody
            ,FieldDescriptionId.VerbatimCoordinates
            ,FieldDescriptionId.VerbatimCoordinateSystem
            ,FieldDescriptionId.VerbatimDepth
            ,FieldDescriptionId.VerbatimElevation
            ,FieldDescriptionId.VerbatimLatitude
            ,FieldDescriptionId.VerbatimLocality
            ,FieldDescriptionId.VerbatimLongitude
            ,FieldDescriptionId.VerbatimSRS
            ,FieldDescriptionId.Bed
            ,FieldDescriptionId.EarliestAgeOrLowestStage
            ,FieldDescriptionId.EarliestEonOrLowestEonothem
            ,FieldDescriptionId.EarliestEpochOrLowestSeries
            ,FieldDescriptionId.EarliestEraOrLowestErathem
            ,FieldDescriptionId.EarliestPeriodOrLowestSystem
            ,FieldDescriptionId.Formation
            ,FieldDescriptionId.GeologicalContextID
            ,FieldDescriptionId.Group
            ,FieldDescriptionId.HighestBiostratigraphicZone
            ,FieldDescriptionId.LatestAgeOrHighestStage
            ,FieldDescriptionId.LatestEonOrHighestEonothem
            ,FieldDescriptionId.LatestEpochOrHighestSeries
            ,FieldDescriptionId.LatestEraOrHighestErathem
            ,FieldDescriptionId.LatestPeriodOrHighestSystem
            ,FieldDescriptionId.LithostratigraphicTerms
            ,FieldDescriptionId.LowestBiostratigraphicZone
            ,FieldDescriptionId.Member
        };

        public static IEnumerable<FieldDescription> GetAllDwcEventCoreOccurrenceFieldDescriptions()
        {
            return GetFieldDescriptions(AllDwcEventCoreOccurrenceFieldDescriptions);
        }

        private static readonly FieldDescriptionId[] AllDwcEventCoreOccurrenceFieldDescriptions =
        {
            FieldDescriptionId.EventID,
            FieldDescriptionId.OccurrenceID,
            FieldDescriptionId.BasisOfRecord,
            FieldDescriptionId.BibliographicCitation,
            FieldDescriptionId.CollectionCode,
            FieldDescriptionId.CollectionID,
            FieldDescriptionId.DataGeneralizations,
            FieldDescriptionId.DatasetID,
            FieldDescriptionId.DatasetName,
            FieldDescriptionId.DynamicProperties,
            FieldDescriptionId.InformationWithheld,
            FieldDescriptionId.InstitutionCode,
            FieldDescriptionId.InstitutionID,
            FieldDescriptionId.Language,
            FieldDescriptionId.License,
            FieldDescriptionId.Modified,
            FieldDescriptionId.OwnerInstitutionCode,
            FieldDescriptionId.References,
            FieldDescriptionId.RightsHolder,
            FieldDescriptionId.Type,
            FieldDescriptionId.DateIdentified,
            FieldDescriptionId.IdentificationID,
            FieldDescriptionId.IdentificationQualifier,
            FieldDescriptionId.IdentificationReferences,
            FieldDescriptionId.IdentificationRemarks,
            FieldDescriptionId.IdentificationVerificationStatus,
            FieldDescriptionId.IdentifiedBy,
            FieldDescriptionId.TypeStatus,
            FieldDescriptionId.AssociatedMedia,
            FieldDescriptionId.AssociatedReferences,
            FieldDescriptionId.AssociatedSequences,
            FieldDescriptionId.AssociatedTaxa,
            FieldDescriptionId.Behavior,
            FieldDescriptionId.CatalogNumber,
            FieldDescriptionId.Disposition,
            FieldDescriptionId.EstablishmentMeans,
            FieldDescriptionId.IndividualCount,
            FieldDescriptionId.LifeStage,
            FieldDescriptionId.AccessRights,
            FieldDescriptionId.OccurrenceRemarks,
            FieldDescriptionId.OccurrenceStatus,
            FieldDescriptionId.OrganismQuantity,
            FieldDescriptionId.OrganismQuantityType,
            FieldDescriptionId.OtherCatalogNumbers,
            FieldDescriptionId.Preparations,
            FieldDescriptionId.RecordedBy,
            FieldDescriptionId.RecordNumber,
            FieldDescriptionId.ReproductiveCondition,
            FieldDescriptionId.Sex,
            FieldDescriptionId.AcceptedNameUsage,
            FieldDescriptionId.AcceptedNameUsageID,
            FieldDescriptionId.Class,
            FieldDescriptionId.Family,
            FieldDescriptionId.Genus,
            FieldDescriptionId.HigherClassification,
            FieldDescriptionId.InfraspecificEpithet,
            FieldDescriptionId.Kingdom,
            FieldDescriptionId.NameAccordingTo,
            FieldDescriptionId.NameAccordingToID,
            FieldDescriptionId.NamePublishedIn,
            FieldDescriptionId.NamePublishedInID,
            FieldDescriptionId.NamePublishedInYear,
            FieldDescriptionId.NomenclaturalCode,
            FieldDescriptionId.NomenclaturalStatus,
            FieldDescriptionId.Order,
            FieldDescriptionId.OriginalNameUsage,
            FieldDescriptionId.OriginalNameUsageID,
            FieldDescriptionId.ParentNameUsage,
            FieldDescriptionId.ParentNameUsageID,
            FieldDescriptionId.Phylum,
            FieldDescriptionId.ScientificName,
            FieldDescriptionId.ScientificNameAuthorship,
            FieldDescriptionId.ScientificNameID,
            FieldDescriptionId.SpecificEpithet,
            FieldDescriptionId.Subgenus,
            FieldDescriptionId.TaxonConceptID,
            FieldDescriptionId.TaxonID,
            FieldDescriptionId.TaxonomicStatus,
            FieldDescriptionId.TaxonRank,
            FieldDescriptionId.TaxonRemarks,
            FieldDescriptionId.VerbatimTaxonRank,
            FieldDescriptionId.VernacularName,
            FieldDescriptionId.MaterialSampleID
        };
    }
}