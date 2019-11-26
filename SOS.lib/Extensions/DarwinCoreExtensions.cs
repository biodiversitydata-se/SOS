using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SOS.Lib.Models.Processed.DarwinCore;

namespace SOS.Lib.Extensions
{
    /// <summary>
    /// Extensions for Darwin Core
    /// </summary>
    public static class DarwinCoreExtensions
    {
        /// <summary>
        /// Cast processed Darwin Core object to Darwin Core
        /// </summary>
        /// <param name="processedDarwinCore"></param>
        /// <returns></returns>
        public static DarwinCore<string> ToDarwinCore(this DarwinCore<DynamicProperties> processedDarwinCore)
        {
            if (processedDarwinCore == null)
            {
                return null;
            }

            return new DarwinCore<string>
            {
                AccessRights = processedDarwinCore.AccessRights,
                BasisOfRecord = processedDarwinCore.BasisOfRecord,
                BibliographicCitation = processedDarwinCore.BasisOfRecord,
                CollectionCode = processedDarwinCore.CollectionCode,
                CollectionID = processedDarwinCore.CollectionID,
                DataGeneralizations = processedDarwinCore.DataGeneralizations,
                DatasetID = processedDarwinCore.DatasetID,
                DatasetName = processedDarwinCore.DatasetName,
                DynamicProperties = JsonConvert.SerializeObject(processedDarwinCore.DynamicProperties),
                Event = processedDarwinCore.Event,
                GeologicalContext = processedDarwinCore.GeologicalContext,
                Identification = processedDarwinCore.Identification,
                InformationWithheld = processedDarwinCore.InformationWithheld,
                InstitutionCode = processedDarwinCore.InstitutionCode,
                InstitutionID = processedDarwinCore.InstitutionID,
                Language = processedDarwinCore.Language,
                Location = processedDarwinCore.Location,
                MeasurementOrFact = processedDarwinCore.MeasurementOrFact,
                Modified = processedDarwinCore.Modified,
                Occurrence = processedDarwinCore.Occurrence,
                OwnerInstitutionCode = processedDarwinCore.OwnerInstitutionCode,
                References = processedDarwinCore.References,
                ResourceRelationship = processedDarwinCore.ResourceRelationship,
                Rights = processedDarwinCore.Rights,
                RightsHolder = processedDarwinCore.RightsHolder,
                Taxon = processedDarwinCore.Taxon,
                Type = processedDarwinCore.Type
            };
        }

        /// <summary>
        ///  Cast processed Darwin Core objects to Darwin Core 
        /// </summary>
        /// <param name="processedDarwinCore"></param>
        /// <returns></returns>
        public static IEnumerable<DarwinCore<string>> ToDarwinCore(this IEnumerable<DarwinCore<DynamicProperties>> processedDarwinCore)
        {
            return processedDarwinCore?.Select(m => m.ToDarwinCore());
        }

        #region HasData
        /// <summary>
        /// Check if event has data
        /// </summary>
        /// <param name="dcEvent"></param>
        /// <returns></returns>
        public static bool HasData(this DarwinCoreEvent dcEvent)
        {
            return !(
                dcEvent.Day.Equals(0) &&
                dcEvent.EndDayOfYear.Equals(0) &&
                string.IsNullOrEmpty(dcEvent.EventDate) &&
                string.IsNullOrEmpty(dcEvent.EventID) &&
                string.IsNullOrEmpty(dcEvent.EventRemarks) &&
                string.IsNullOrEmpty(dcEvent.EventTime) &&
                string.IsNullOrEmpty(dcEvent.FieldNotes) &&
                string.IsNullOrEmpty(dcEvent.FieldNumber) &&
                string.IsNullOrEmpty(dcEvent.Habitat) &&
                dcEvent.Month.Equals(0) &&
                string.IsNullOrEmpty(dcEvent.ParentEventID) &&
                string.IsNullOrEmpty(dcEvent.SampleSizeUnit) &&
                string.IsNullOrEmpty(dcEvent.SampleSizeValue) &&
                string.IsNullOrEmpty(dcEvent.SamplingEffort) &&
                string.IsNullOrEmpty(dcEvent.SamplingProtocol) &&
                dcEvent.StartDayOfYear.Equals(0) &&
                string.IsNullOrEmpty(dcEvent.VerbatimEventDate) &&
                dcEvent.Year.Equals(0));
        }

        /// <summary>
        /// Check if geological Context has data
        /// </summary>
        /// <param name="geologicalContext"></param>
        /// <returns></returns>
        public static bool HasData(this DarwinCoreGeologicalContext geologicalContext)
        {
            return !(
                string.IsNullOrEmpty(geologicalContext.Bed) &&
                string.IsNullOrEmpty(geologicalContext.EarliestAgeOrLowestStage) &&
                string.IsNullOrEmpty(geologicalContext.EarliestEonOrLowestEonothem) &&
                string.IsNullOrEmpty(geologicalContext.EarliestEpochOrLowestSeries) &&
                string.IsNullOrEmpty(geologicalContext.EarliestEraOrLowestErathem) &&
                string.IsNullOrEmpty(geologicalContext.EarliestPeriodOrLowestSystem) &&
                string.IsNullOrEmpty(geologicalContext.Formation) &&
                string.IsNullOrEmpty(geologicalContext.GeologicalContextID) &&
                string.IsNullOrEmpty(geologicalContext.Group) &&
                string.IsNullOrEmpty(geologicalContext.HighestBiostratigraphicZone) &&
                string.IsNullOrEmpty(geologicalContext.LatestEraOrHighestErathem) &&
                string.IsNullOrEmpty(geologicalContext.LatestAgeOrHighestStage) &&
                string.IsNullOrEmpty(geologicalContext.LatestEonOrHighestEonothem) &&
                string.IsNullOrEmpty(geologicalContext.LatestEpochOrHighestSeries) &&
                string.IsNullOrEmpty(geologicalContext.LatestPeriodOrHighestSystem) &&
                string.IsNullOrEmpty(geologicalContext.LithostratigraphicTerms) &&
                string.IsNullOrEmpty(geologicalContext.LowestBiostratigraphicZone) &&
                string.IsNullOrEmpty(geologicalContext.Member));
        }

        /// <summary>
        /// Check if identification has data
        /// </summary>
        /// <param name="identification"></param>
        /// <returns></returns>
        public static bool HasData(this DarwinCoreIdentification identification)
        {
            return !(
                string.IsNullOrEmpty(identification.DateIdentified) &&
                string.IsNullOrEmpty(identification.IdentificationID) &&
                string.IsNullOrEmpty(identification.IdentificationQualifier) &&
                string.IsNullOrEmpty(identification.IdentificationReferences) &&
                string.IsNullOrEmpty(identification.IdentificationRemarks) &&
                string.IsNullOrEmpty(identification.IdentificationVerificationStatus) &&
                string.IsNullOrEmpty(identification.IdentifiedBy) &&
                string.IsNullOrEmpty(identification.TypeStatus));
        }

        /// <summary>
        /// Check if location has data
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static bool HasData(this DarwinCoreLocation location)
        {
            return !(
            string.IsNullOrEmpty(location.Continent) &&
            string.IsNullOrEmpty(location.CoordinatePrecision) &&
            string.IsNullOrEmpty(location.CoordinateUncertaintyInMeters) &&
            string.IsNullOrEmpty(location.County) &&
            string.IsNullOrEmpty(location.Country) &&
            string.IsNullOrEmpty(location.CountryCode) &&
            location.DecimalLatitude.Equals(0) &&
            location.DecimalLongitude.Equals(0) &&
            string.IsNullOrEmpty(location.FootprintSRS) &&
            string.IsNullOrEmpty(location.FootprintSpatialFit) &&
            string.IsNullOrEmpty(location.FootprintWKT) &&
            string.IsNullOrEmpty(location.GeodeticDatum) &&
            string.IsNullOrEmpty(location.GeoreferenceProtocol) &&
            string.IsNullOrEmpty(location.GeoreferenceRemarks) &&
            string.IsNullOrEmpty(location.GeoreferenceSources) &&
            string.IsNullOrEmpty(location.GeoreferenceVerificationStatus) &&
            string.IsNullOrEmpty(location.GeoreferencedBy) &&
            string.IsNullOrEmpty(location.HigherGeography) &&
            string.IsNullOrEmpty(location.HigherGeographyID) &&
            string.IsNullOrEmpty(location.GeoreferencedDate) &&
            string.IsNullOrEmpty(location.Island) &&
            string.IsNullOrEmpty(location.IslandGroup) &&
            string.IsNullOrEmpty(location.Locality) &&
            string.IsNullOrEmpty(location.LocationAccordingTo) &&
            string.IsNullOrEmpty(location.LocationID) &&
            string.IsNullOrEmpty(location.LocationRemarks) &&
            string.IsNullOrEmpty(location.MaximumDepthInMeters) &&
            string.IsNullOrEmpty(location.MaximumDistanceAboveSurfaceInMeters) &&
            string.IsNullOrEmpty(location.MaximumElevationInMeters) &&
            string.IsNullOrEmpty(location.MinimumDepthInMeters) &&
            string.IsNullOrEmpty(location.MinimumDistanceAboveSurfaceInMeters) &&
            string.IsNullOrEmpty(location.MinimumElevationInMeters) &&
            string.IsNullOrEmpty(location.Municipality) &&
            string.IsNullOrEmpty(location.PointRadiusSpatialFit) &&
            string.IsNullOrEmpty(location.StateProvince) &&
            string.IsNullOrEmpty(location.VerbatimCoordinateSystem) &&
            string.IsNullOrEmpty(location.VerbatimCoordinates) &&
            string.IsNullOrEmpty(location.VerbatimDepth) &&
            string.IsNullOrEmpty(location.VerbatimElevation) &&
            string.IsNullOrEmpty(location.VerbatimLatitude) &&
            string.IsNullOrEmpty(location.VerbatimLocality) &&
            string.IsNullOrEmpty(location.VerbatimLongitude) &&
            string.IsNullOrEmpty(location.VerbatimSRS) &&
            string.IsNullOrEmpty(location.WaterBody));
        }

        /// <summary>
        /// Check if material Sample has data
        /// </summary>
        /// <param name="materialSample"></param>
        /// <returns></returns>
        public static bool HasData(this DarwinCoreMaterialSample materialSample)
        {
            return !string.IsNullOrEmpty(materialSample.MaterialSampleID);
        }

        /// <summary>
        /// Check if measurement Or Fact has data
        /// </summary>
        /// <param name="measurementOrFact"></param>
        /// <returns></returns>
        public static bool HasData(this DarwinCoreMeasurementOrFact measurementOrFact)
        {
            return !(
                string.IsNullOrEmpty(measurementOrFact.MeasurementAccuracy) &&
                string.IsNullOrEmpty(measurementOrFact.MeasurementDeterminedBy) &&
                string.IsNullOrEmpty(measurementOrFact.MeasurementDeterminedDate) &&
                string.IsNullOrEmpty(measurementOrFact.MeasurementID) &&
                string.IsNullOrEmpty(measurementOrFact.MeasurementMethod) &&
                string.IsNullOrEmpty(measurementOrFact.MeasurementRemarks) &&
                string.IsNullOrEmpty(measurementOrFact.MeasurementType) &&
                string.IsNullOrEmpty(measurementOrFact.MeasurementUnit) &&
                string.IsNullOrEmpty(measurementOrFact.MeasurementValue));
        }

        /// <summary>
        /// Check if occurrence has data
        /// </summary>
        /// <param name="occurrence"></param>
        /// <returns></returns>
        public static bool HasData(this DarwinCoreOccurrence occurrence)
        {
            return !(
                string.IsNullOrEmpty(occurrence.AssociatedMedia) &&
                string.IsNullOrEmpty(occurrence.AssociatedOccurrences) &&
                string.IsNullOrEmpty(occurrence.AssociatedReferences) &&
                string.IsNullOrEmpty(occurrence.AssociatedSequences) &&
                string.IsNullOrEmpty(occurrence.AssociatedTaxa) &&
                string.IsNullOrEmpty(occurrence.Behavior) &&
                string.IsNullOrEmpty(occurrence.CatalogNumber) &&
                string.IsNullOrEmpty(occurrence.Disposition) &&
                string.IsNullOrEmpty(occurrence.EstablishmentMeans) &&
                string.IsNullOrEmpty(occurrence.IndividualCount) &&
                string.IsNullOrEmpty(occurrence.IndividualID) &&
                string.IsNullOrEmpty(occurrence.LifeStage) &&
                string.IsNullOrEmpty(occurrence.OccurrenceID) &&
                string.IsNullOrEmpty(occurrence.OccurrenceID) &&
                string.IsNullOrEmpty(occurrence.OccurrenceRemarks) &&
                string.IsNullOrEmpty(occurrence.OccurrenceStatus) &&
                string.IsNullOrEmpty(occurrence.OrganismQuantity) &&
                string.IsNullOrEmpty(occurrence.OrganismQuantityType) &&
                string.IsNullOrEmpty(occurrence.OtherCatalogNumbers) &&
                string.IsNullOrEmpty(occurrence.Preparations) &&
                string.IsNullOrEmpty(occurrence.PreviousIdentifications) &&
                string.IsNullOrEmpty(occurrence.RecordNumber) &&
                string.IsNullOrEmpty(occurrence.RecordedBy) &&
                string.IsNullOrEmpty(occurrence.ReproductiveCondition) &&
                string.IsNullOrEmpty(occurrence.Sex));
        }

        /// <summary>
        /// Check if resourceRelationship has data
        /// </summary>
        /// <param name="resourceRelationship"></param>
        /// <returns></returns>
        public static bool HasData(this DarwinCoreResourceRelationship resourceRelationship)
        {
            return !(
                string.IsNullOrEmpty(resourceRelationship.RelatedResourceID) &&
                string.IsNullOrEmpty(resourceRelationship.RelationshipAccordingTo) &&
                string.IsNullOrEmpty(resourceRelationship.RelationshipEstablishedDate) &&
                string.IsNullOrEmpty(resourceRelationship.RelationshipOfResource) &&
                string.IsNullOrEmpty(resourceRelationship.RelationshipRemarks) &&
                string.IsNullOrEmpty(resourceRelationship.ResourceID) &&
                string.IsNullOrEmpty(resourceRelationship.ResourceRelationshipID));
        }

        /// <summary>
        /// Check if taxon has data
        /// </summary>
        /// <param name="taxon"></param>
        /// <returns></returns>
        public static bool HasData(this DarwinCoreTaxon taxon)
        {
            return !(
                string.IsNullOrEmpty(taxon.AcceptedNameUsage) &&
                string.IsNullOrEmpty(taxon.AcceptedNameUsageID) &&
                string.IsNullOrEmpty(taxon.Class) &&
                string.IsNullOrEmpty(taxon.Family) &&
                string.IsNullOrEmpty(taxon.Genus) &&
                string.IsNullOrEmpty(taxon.HigherClassification) &&
                string.IsNullOrEmpty(taxon.InfraspecificEpithet) &&
                string.IsNullOrEmpty(taxon.Kingdom) &&
                string.IsNullOrEmpty(taxon.NameAccordingTo) &&
                string.IsNullOrEmpty(taxon.NameAccordingToID) &&
                string.IsNullOrEmpty(taxon.NamePublishedIn) &&
                string.IsNullOrEmpty(taxon.NamePublishedInID) &&
                string.IsNullOrEmpty(taxon.NamePublishedInYear) &&
                string.IsNullOrEmpty(taxon.NomenclaturalCode) &&
                string.IsNullOrEmpty(taxon.NomenclaturalStatus) &&
                string.IsNullOrEmpty(taxon.Order) &&
                string.IsNullOrEmpty(taxon.OriginalNameUsage) &&
                string.IsNullOrEmpty(taxon.OriginalNameUsageID) &&
                string.IsNullOrEmpty(taxon.ParentNameUsageID) &&
                string.IsNullOrEmpty(taxon.ParentNameUsage) &&
                string.IsNullOrEmpty(taxon.Phylum) &&
                string.IsNullOrEmpty(taxon.ScientificName) &&
                string.IsNullOrEmpty(taxon.ScientificNameAuthorship) &&
                string.IsNullOrEmpty(taxon.ScientificNameID) &&
                string.IsNullOrEmpty(taxon.SpecificEpithet) &&
                string.IsNullOrEmpty(taxon.Subgenus) &&
                string.IsNullOrEmpty(taxon.TaxonConceptID) &&
                string.IsNullOrEmpty(taxon.TaxonID) &&
                string.IsNullOrEmpty(taxon.TaxonRank) &&
                string.IsNullOrEmpty(taxon.TaxonRemarks) &&
                string.IsNullOrEmpty(taxon.TaxonomicStatus) &&
                string.IsNullOrEmpty(taxon.VernacularName) &&
                string.IsNullOrEmpty(taxon.VerbatimTaxonRank));
        }

        #endregion HasData
    }
}
