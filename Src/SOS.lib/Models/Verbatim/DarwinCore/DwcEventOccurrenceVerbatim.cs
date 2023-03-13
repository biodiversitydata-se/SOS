using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;

namespace SOS.Lib.Models.Verbatim.DarwinCore
{
    /// <summary>
    /// Event object including all observations associated with this event.
    /// There could be a problem to store this object in MongoDB due to the document size limit.
    /// </summary>
    public class DwcEventOccurrenceVerbatim : DwcEventVerbatim
    {
        /// <summary>
        ///     Darwin Core term name: basisOfRecord.
        ///     The specific nature of the data record -
        ///     a subtype of the dcterms:type.
        ///     Recommended best practice is to use a controlled
        ///     vocabulary such as the Darwin Core Type Vocabulary
        ///     (http://rs.tdwg.org/dwc/terms/type-vocabulary/index.htm).
        ///     In Species Gateway this property has the value
        ///     HumanObservation.
        /// </summary>
        public string BasisOfRecord { get; set; }

        /// <summary>
        ///     Darwin Core term name: identificationVerificationStatus.
        ///     A categorical indicator of the extent to which the taxonomic
        ///     identification has been verified to be correct.
        ///     Recommended best practice is to use a controlled vocabulary
        ///     such as that used in HISPID/ABCD.
        /// </summary>
        public string IdentificationVerificationStatus { get; set; }

        /// <summary>
        ///     Observations linked to the event.
        /// </summary>
        public ICollection<DwcObservationVerbatim> Observations { get; set; }

        /// <summary>
        ///     Darwin Core term name: recordedBy.
        ///     A list (concatenated and separated) of names of people,
        ///     groups, or organizations responsible for recording the
        ///     original Occurrence. The primary collector or observer,
        ///     especially one who applies a personal identifier
        ///     (recordNumber), should be listed first.
        /// </summary>
        public string RecordedBy { get; set; }

        /// <summary>
        /// Time spent to find taxa
        /// </summary>
        public string SamplingEffortTime { get; set; }

        /// <summary>
        /// List of taxon
        /// </summary>
        public ICollection<DwcTaxon> Taxa { get; set; }

        /// <summary>
        /// List of not found taxa
        /// </summary>
        public ICollection<DwcTaxon> NotFoundTaxa { get; set; }

        /// <summary>
        /// List of not found taxon ids
        /// </summary>
        public ICollection<string> NotFoundTaxonIds { get; set; }

        /// <summary>
        /// Data stewardship dataset Id.
        /// </summary>
        public string DataStewardshipDatasetId { get; set; }

        public List<DwcObservationVerbatim> CreateAbsentObservations()
        {
            if (NotFoundTaxa == null) return new List<DwcObservationVerbatim>();
            return NotFoundTaxa.Select(CreateAbsentObservation).ToList();
        }

        private DwcObservationVerbatim CreateAbsentObservation(DwcTaxon taxon)
        {
            return new DwcObservationVerbatim
            {
                OccurrenceID = Guid.NewGuid().ToString(),
                EventID = EventID,
                OccurrenceStatus = "absent",
                BasisOfRecord = BasisOfRecord,
                IdentificationVerificationStatus = IdentificationVerificationStatus,
                RecordedBy = RecordedBy,
                TaxonID = taxon.TaxonID,
                ScientificName = taxon.ScientificName,
                TaxonRank = taxon.TaxonRank,
                Kingdom = taxon.Kingdom,
                DataProviderId = DataProviderId,
                DataProviderIdentifier = DataProviderIdentifier,
                DwcArchiveFilename = DwcArchiveFilename,
                DataStewardshipDatasetId = DataStewardshipDatasetId,
                DatasetID = DatasetID,
                DatasetName = DatasetName,
                EventAudubonMedia = AudubonMedia,
                EventMultimedia = Multimedia,
                EventMeasurementOrFacts = MeasurementOrFacts,
                EventExtendedMeasurementOrFacts = ExtendedMeasurementOrFacts,
                AccessRights = AccessRights,
                BibliographicCitation = BibliographicCitation,
                DataGeneralizations = DataGeneralizations,                
                DynamicProperties = DynamicProperties,
                InformationWithheld = InformationWithheld,
                InstitutionCode = InstitutionCode,
                InstitutionID = InstitutionID,
                Language = Language,
                License = License,
                Modified = Modified,
                OwnerInstitutionCode = OwnerInstitutionCode,
                References = References,
                RightsHolder = RightsHolder,
                Type = Type,
                Day = Day,
                EndDayOfYear = EndDayOfYear,
                EventDate = EventDate,                
                ParentEventID = ParentEventID,
                EventRemarks = EventRemarks,
                EventTime = EventTime,
                FieldNotes = FieldNotes,
                FieldNumber = FieldNumber,
                Habitat = Habitat,
                Month = Month,
                SampleSizeUnit = SampleSizeUnit,
                SampleSizeValue = SampleSizeValue,
                SamplingEffort = SamplingEffort,
                SamplingProtocol = SamplingProtocol,
                StartDayOfYear = StartDayOfYear,
                VerbatimEventDate = VerbatimEventDate,
                Year = Year,
                Bed = Bed,
                EarliestAgeOrLowestStage = EarliestAgeOrLowestStage,
                EarliestEonOrLowestEonothem = EarliestEonOrLowestEonothem,
                EarliestEpochOrLowestSeries = EarliestEpochOrLowestSeries,
                EarliestEraOrLowestErathem = EarliestEraOrLowestErathem,
                EarliestGeochronologicalEra = EarliestGeochronologicalEra,
                EarliestPeriodOrLowestSystem = EarliestPeriodOrLowestSystem,
                Formation = Formation,
                GeologicalContextID = GeologicalContextID,
                Group = Group,
                HighestBiostratigraphicZone = HighestBiostratigraphicZone,
                LatestAgeOrHighestStage = LatestAgeOrHighestStage,
                LatestEonOrHighestEonothem = LatestEonOrHighestEonothem,
                LatestEpochOrHighestSeries = LatestEpochOrHighestSeries,
                LatestEraOrHighestErathem = LatestEraOrHighestErathem,
                LatestGeochronologicalEra = LatestGeochronologicalEra,
                LatestPeriodOrHighestSystem = LatestPeriodOrHighestSystem,
                LithostratigraphicTerms = LithostratigraphicTerms,
                LowestBiostratigraphicZone = LowestBiostratigraphicZone,
                Member = Member,
                Continent = Continent,
                CoordinatePrecision = CoordinatePrecision,
                CoordinateUncertaintyInMeters = CoordinateUncertaintyInMeters,
                Country = Country,
                CountryCode = CountryCode,
                County = County,
                DecimalLatitude = DecimalLatitude,
                DecimalLongitude = DecimalLongitude,
                FootprintSpatialFit = FootprintSpatialFit,
                FootprintSRS = FootprintSRS,
                FootprintWKT = FootprintWKT,
                GeodeticDatum = GeodeticDatum,
                GeoreferencedBy = GeoreferencedBy,
                GeoreferencedDate = GeoreferencedDate,
                GeoreferenceProtocol = GeoreferenceProtocol,
                GeoreferenceRemarks = GeoreferenceRemarks,
                GeoreferenceSources = GeoreferenceSources,
                GeoreferenceVerificationStatus = GeoreferenceVerificationStatus,
                HigherGeography = HigherGeography,
                HigherGeographyID = HigherGeographyID,
                Island = Island,
                IslandGroup = IslandGroup,
                Locality = Locality,
                LocationAccordingTo = LocationAccordingTo,
                LocationID = LocationID,
                LocationRemarks = LocationRemarks,
                MaximumDepthInMeters = MaximumDepthInMeters,
                MaximumDistanceAboveSurfaceInMeters = MaximumDistanceAboveSurfaceInMeters,
                MaximumElevationInMeters = MaximumElevationInMeters,
                MinimumDepthInMeters = MinimumDepthInMeters,
                MinimumDistanceAboveSurfaceInMeters = MinimumDistanceAboveSurfaceInMeters,
                MinimumElevationInMeters = MinimumElevationInMeters,
                Municipality = Municipality,
                PointRadiusSpatialFit = PointRadiusSpatialFit,
                StateProvince = StateProvince,
                VerbatimCoordinates = VerbatimCoordinates,
                VerbatimCoordinateSystem = VerbatimCoordinateSystem,
                VerbatimDepth = VerbatimDepth,
                VerbatimElevation = VerbatimElevation,
                VerbatimLatitude = VerbatimLatitude,
                VerbatimLocality = VerbatimLocality,
                VerbatimLongitude = VerbatimLongitude,
                VerbatimSRS = VerbatimSRS,
                WaterBody = WaterBody,

                // SamplingEffortTime = SamplingEffortTime;
            };
        }
    }
}