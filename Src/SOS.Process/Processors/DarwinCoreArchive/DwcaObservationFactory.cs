using System;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nest;
using NetTopologySuite.Geometries;
using SOS.Lib.Constants;
using SOS.Lib.DataStructures;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Resource.Interfaces;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Process.Processors.DarwinCoreArchive
{
    /// <summary>
    ///     DwC-A observation factory.
    /// </summary>
    public class DwcaObservationFactory
    {
        private const int DefaultCoordinateUncertaintyInMeters = 5000;
        private readonly IAreaHelper _areaHelper;
        private readonly DataProvider _dataProvider;
        private readonly IDictionary<VocabularyId, IDictionary<object, int>> _vocabularyById;
        private readonly HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon> _taxonByScientificName;
        private readonly HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon> _taxonBySynonymName;
        private readonly IDictionary<int, Lib.Models.Processed.Observation.Taxon> _taxonByTaxonId;

        private readonly List<string> errors = new List<string>();

        public DwcaObservationFactory(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IDictionary<VocabularyId, IDictionary<object, int>> vocabularyById,
            IAreaHelper areaHelper)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _taxonByTaxonId = taxa ?? throw new ArgumentNullException(nameof(taxa));
            _vocabularyById = vocabularyById ?? throw new ArgumentNullException(nameof(vocabularyById));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));

            _taxonByScientificName = new HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon>();
            _taxonBySynonymName = new HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon>();
            foreach (var processedTaxon in _taxonByTaxonId.Values)
            {
                _taxonByScientificName.Add(processedTaxon.ScientificName.ToLower(), processedTaxon);
                if (processedTaxon.Attributes?.Synonyms != null)
                {
                    foreach (var synonyme in processedTaxon.Attributes.Synonyms)
                    {
                        _taxonBySynonymName.Add(synonyme.Name.ToLower(), processedTaxon);
                    }
                }
            }
        }

        public static async Task<DwcaObservationFactory> CreateAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IVocabularyRepository processedVocabularyRepository,
            IAreaHelper areaHelper)
        {
            var vocabularies = await processedVocabularyRepository.GetAllAsync();
            var vocabularyById = GetVocabulariesDictionary(
                ExternalSystemId.DarwinCore,
                vocabularies.ToArray(),
                true);
            return new DwcaObservationFactory(dataProvider, taxa, vocabularyById, areaHelper);
        }

        public IEnumerable<Observation> CreateProcessedObservations(
            IEnumerable<DwcObservationVerbatim> verbatimObservations)
        {
            return verbatimObservations.Select(CreateProcessedObservation);
        }

        /// <summary>
        ///     Cast verbatim observations to processed data model
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(DwcObservationVerbatim verbatimObservation)
        {
            if (verbatimObservation == null)
            {
                return null;
            }

            var obs = new Observation();
            obs.DataProviderId = _dataProvider.Id;
            //AddVerbatimObservationAsJson(obs, verbatimObservation); // todo - this could be used to store the original verbatim observation

            // Other
            //obs.Id = verbatimObservation.Id;
            // todo - handle properties below.
            //obs.DataProviderId = verbatimObservation.DataProviderId;
            //obs.DataProviderIdentifier = verbatimObservation.DataProviderIdentifier;

            // Record level
            if (verbatimObservation.ObservationMeasurementOrFacts.HasItems())
                obs.MeasurementOrFacts = verbatimObservation.ObservationMeasurementOrFacts?.Select(dwcMof => dwcMof.ToProcessedExtendedMeasurementOrFact()).ToArray();
            else if (verbatimObservation.ObservationExtendedMeasurementOrFacts.HasItems())
                obs.MeasurementOrFacts = verbatimObservation.ObservationExtendedMeasurementOrFacts?.Select(dwcMof => dwcMof.ToProcessedExtendedMeasurementOrFact()).ToArray();
            obs.AccessRights = GetSosId(verbatimObservation.AccessRights,
                _vocabularyById[VocabularyId.AccessRights]);
            obs.BasisOfRecord = GetSosId(verbatimObservation.BasisOfRecord,
                _vocabularyById[VocabularyId.BasisOfRecord]);
            obs.BibliographicCitation = verbatimObservation.BibliographicCitation;
            obs.CollectionCode = verbatimObservation.CollectionCode;
            obs.CollectionId = verbatimObservation.CollectionID;
            obs.DataGeneralizations = verbatimObservation.DataGeneralizations;
            obs.DatasetId = verbatimObservation.DatasetID;
            obs.DatasetName = verbatimObservation.DatasetName;
            obs.DynamicProperties = verbatimObservation.DynamicProperties;
            obs.InformationWithheld = verbatimObservation.InformationWithheld;
            obs.InstitutionId = verbatimObservation.InstitutionID;
            obs.InstitutionCode = GetSosId(verbatimObservation.InstitutionCode,
                _vocabularyById[VocabularyId.Institution]);
            obs.Language = verbatimObservation.Language;
            obs.License = verbatimObservation.License;
            obs.Modified = DwcParser.ParseDate(verbatimObservation.Modified)?.ToUniversalTime();
            obs.OwnerInstitutionCode = verbatimObservation.OwnerInstitutionCode;
            obs.References = verbatimObservation.References;
            obs.RightsHolder = verbatimObservation.RightsHolder;
            obs.Type = GetSosId(verbatimObservation.Type,
                _vocabularyById[VocabularyId.Type]);

            // Event
            obs.Event = CreateProcessedEvent(verbatimObservation);

            // Geological
            obs.GeologicalContext = CreateProcessedGeologicalContext(verbatimObservation);

            // Identification
            obs.Identification = CreateProcessedIdentification(verbatimObservation);

            // Location
            obs.Location = CreateProcessedLocation(verbatimObservation);

            // MaterialSample
            obs.MaterialSample = CreateProcessedMaterialSample(verbatimObservation);

            // Occurrence
            obs.Occurrence = CreateProcessedOccurrence(verbatimObservation);

            // Organism
            obs.Organism = CreateProcessedOrganism(verbatimObservation);

            // Taxon
            obs.Taxon = CreateProcessedTaxon(verbatimObservation);

            // Temporarily remove
            //obs.IsInEconomicZoneOfSweden = true;
            _areaHelper.AddAreaDataToProcessedObservation(obs);
            return obs;
        }

        private static void AddVerbatimObservationAsJson(Observation obs,
            DwcObservationVerbatim verbatimObservation)
        {
            //obs.VerbatimObservation = JsonConvert.SerializeObject(
            //    verbatimObservation,
            //    Formatting.Indented,
            //    new JsonSerializerSettings()
            //    {
            //        NullValueHandling = NullValueHandling.Ignore
            //    });
        }

        private ICollection<Multimedia> CreateProcessedMultimedia(
            ICollection<DwcMultimedia> verbatimMultimedia, 
            ICollection<DwcAudubonMedia> verbatimAudubonMedia)
        {
            if (verbatimMultimedia.HasItems())
            {
                return verbatimMultimedia.Select(dwcMultimedia => dwcMultimedia.ToProcessedMultimedia()).ToArray();
            }

            if (verbatimAudubonMedia.HasItems())
            {
                return verbatimAudubonMedia.Select(dwcAudubonMedia => dwcAudubonMedia.ToProcessedMultimedia()).ToArray();
            }

            return null;
        }

        private Organism CreateProcessedOrganism(DwcObservationVerbatim verbatimObservation)
        {
            var processedOrganism = new Organism();
            processedOrganism.OrganismId = verbatimObservation.OrganismID;
            processedOrganism.OrganismName = verbatimObservation.OrganismName;
            processedOrganism.OrganismScope = verbatimObservation.OrganismScope;
            processedOrganism.AssociatedOccurrences = verbatimObservation.AssociatedOccurrences;
            processedOrganism.AssociatedOrganisms = verbatimObservation.AssociatedOrganisms;
            processedOrganism.PreviousIdentifications = verbatimObservation.PreviousIdentifications;
            processedOrganism.OrganismRemarks = verbatimObservation.OrganismRemarks;

            return processedOrganism;
        }

        private GeologicalContext CreateProcessedGeologicalContext(DwcObservationVerbatim verbatimObservation)
        {
            var processedGeologicalContext = new GeologicalContext();
            processedGeologicalContext.Bed = verbatimObservation.Bed;
            processedGeologicalContext.EarliestAgeOrLowestStage = verbatimObservation.EarliestAgeOrLowestStage;
            processedGeologicalContext.EarliestEonOrLowestEonothem = verbatimObservation.EarliestEonOrLowestEonothem;
            processedGeologicalContext.EarliestEpochOrLowestSeries = verbatimObservation.EarliestEpochOrLowestSeries;
            processedGeologicalContext.EarliestEraOrLowestErathem = verbatimObservation.EarliestEraOrLowestErathem;
            processedGeologicalContext.EarliestGeochronologicalEra = verbatimObservation.EarliestGeochronologicalEra;
            processedGeologicalContext.EarliestPeriodOrLowestSystem = verbatimObservation.EarliestPeriodOrLowestSystem;
            processedGeologicalContext.Formation = verbatimObservation.Formation;
            processedGeologicalContext.GeologicalContextId = verbatimObservation.GeologicalContextID;
            processedGeologicalContext.Group = verbatimObservation.Group;
            processedGeologicalContext.HighestBiostratigraphicZone = verbatimObservation.HighestBiostratigraphicZone;
            processedGeologicalContext.LatestAgeOrHighestStage = verbatimObservation.LatestAgeOrHighestStage;
            processedGeologicalContext.LatestEonOrHighestEonothem = verbatimObservation.LatestEonOrHighestEonothem;
            processedGeologicalContext.LatestEpochOrHighestSeries = verbatimObservation.LatestEpochOrHighestSeries;
            processedGeologicalContext.LatestEraOrHighestErathem = verbatimObservation.LatestEraOrHighestErathem;
            processedGeologicalContext.LatestGeochronologicalEra = verbatimObservation.LatestGeochronologicalEra;
            processedGeologicalContext.LatestPeriodOrHighestSystem = verbatimObservation.LatestPeriodOrHighestSystem;
            processedGeologicalContext.LithostratigraphicTerms = verbatimObservation.LithostratigraphicTerms;
            processedGeologicalContext.LowestBiostratigraphicZone = verbatimObservation.LowestBiostratigraphicZone;
            processedGeologicalContext.Member = verbatimObservation.Member;

            return processedGeologicalContext;
        }

        private MaterialSample CreateProcessedMaterialSample(DwcObservationVerbatim verbatimObservation)
        {
            var processedMaterialSample = new MaterialSample();
            processedMaterialSample.MaterialSampleId = verbatimObservation.MaterialSampleID;
            return processedMaterialSample;
        }

        private Event CreateProcessedEvent(DwcObservationVerbatim verbatimObservation)
        {
            var processedEvent = new Event();
            processedEvent.EventId = verbatimObservation.EventID;
            processedEvent.ParentEventId = verbatimObservation.ParentEventID;
            processedEvent.EventRemarks = verbatimObservation.EventRemarks;
            processedEvent.FieldNotes = verbatimObservation.FieldNotes;
            processedEvent.FieldNumber = verbatimObservation.FieldNumber;
            processedEvent.Habitat = verbatimObservation.Habitat;
            processedEvent.SampleSizeUnit = verbatimObservation.SampleSizeUnit;
            processedEvent.SampleSizeValue = verbatimObservation.SampleSizeValue;
            processedEvent.SamplingEffort = verbatimObservation.SamplingEffort;
            processedEvent.SamplingProtocol = verbatimObservation.SamplingProtocol;
            processedEvent.VerbatimEventDate = verbatimObservation.VerbatimEventDate;

            DwcParser.TryParseEventDate(
                verbatimObservation.EventDate,
                verbatimObservation.Year,
                verbatimObservation.Month,
                verbatimObservation.Day,
                out var startDate,
                out var endDate);

            processedEvent.StartDate = startDate?.ToUniversalTime();
            processedEvent.EndDate = endDate?.ToUniversalTime();

            processedEvent.Media = CreateProcessedMultimedia(
                verbatimObservation.EventMultimedia,
                verbatimObservation.EventAudubonMedia);
            if (verbatimObservation.EventMeasurementOrFacts.HasItems())
                processedEvent.MeasurementOrFacts = verbatimObservation.EventMeasurementOrFacts?.Select(dwcMof => dwcMof.ToProcessedExtendedMeasurementOrFact()).ToArray();
            else if (verbatimObservation.EventExtendedMeasurementOrFacts.HasItems())
                processedEvent.MeasurementOrFacts = verbatimObservation.EventExtendedMeasurementOrFacts?.Select(dwcMof => dwcMof.ToProcessedExtendedMeasurementOrFact()).ToArray();

            return processedEvent;
        }

        private Identification CreateProcessedIdentification(DwcObservationVerbatim verbatimObservation)
        {
            string dateIdentifiedString = null;
            if (DateTime.TryParse(verbatimObservation.DateIdentified, out var dateIdentified))
            {
                dateIdentifiedString = dateIdentified.ToUniversalTime().ToString();
            }

            var processedIdentification = new Identification();
            processedIdentification.DateIdentified = dateIdentifiedString;
            processedIdentification.IdentificationId = verbatimObservation.IdentificationID;
            processedIdentification.IdentificationQualifier = verbatimObservation.IdentificationQualifier;
            processedIdentification.IdentificationReferences = verbatimObservation.IdentificationReferences;
            processedIdentification.IdentificationRemarks = verbatimObservation.IdentificationRemarks;
            processedIdentification.ValidationStatus = GetSosId(verbatimObservation.IdentificationVerificationStatus, _vocabularyById[VocabularyId.ValidationStatus]);
            processedIdentification.Validated = GetIsValidated(processedIdentification.ValidationStatus);
            //processedIdentification.UncertainDetermination = !processedIdentification.Validated; // todo - is this correct?
            processedIdentification.IdentifiedBy = verbatimObservation.IdentifiedBy;
            processedIdentification.TypeStatus = verbatimObservation.TypeStatus;
            return processedIdentification;
        }

        private bool GetIsValidated(VocabularyValue validationStatus)
        {
            if (validationStatus == null) return false;
            switch (validationStatus.Id)
            {
                case (int)ValidationStatusId.Verified:
                case (int)ValidationStatusId.ReportedByExpert:
                case (int)ValidationStatusId.ApprovedBasedOnDeterminatorsVerification:
                case (int)ValidationStatusId.ApprovedBasedOnImageSoundOrVideoRecording:
                case (int)ValidationStatusId.ApprovedBasedOnReportersDocumentation:
                case (int)ValidationStatusId.ApprovedBasedOnReportersOldRarityForm:
                case (int)ValidationStatusId.ApprovedBasedOnReportersRarityForm:
                case (int)ValidationStatusId.ApprovedSpecimenCheckedByValidator:
                    return true;
            }

            return false;
        }

        private Lib.Models.Processed.Observation.Location CreateProcessedLocation(DwcObservationVerbatim verbatimObservation)
        {
            var processedLocation = new Lib.Models.Processed.Observation.Location();
            processedLocation.Continent = GetSosId(
                verbatimObservation.Continent,
                _vocabularyById[VocabularyId.Continent],
                (int) ContinentId.Europe,
                MappingNotFoundLogic.UseDefaultValue);
            processedLocation.CoordinatePrecision = verbatimObservation.CoordinatePrecision.ParseDouble();
            processedLocation.CoordinateUncertaintyInMeters =
                verbatimObservation.CoordinateUncertaintyInMeters?.ParseDoubleConvertToInt() ?? DefaultCoordinateUncertaintyInMeters;
            processedLocation.Country = GetSosId(
                verbatimObservation.Country,
                _vocabularyById[VocabularyId.Country],
                (int) CountryId.Sweden,
                MappingNotFoundLogic.UseDefaultValue);
            processedLocation.CountryCode = verbatimObservation.CountryCode;
            processedLocation.DecimalLatitude = verbatimObservation.DecimalLatitude.ParseDouble();
            processedLocation.DecimalLongitude = verbatimObservation.DecimalLongitude.ParseDouble();
            processedLocation.FootprintSpatialFit = verbatimObservation.FootprintSpatialFit;
            processedLocation.FootprintSRS = verbatimObservation.FootprintSRS;
            processedLocation.FootprintWKT = verbatimObservation.FootprintWKT;
            processedLocation.GeodeticDatum = verbatimObservation.GeodeticDatum;
            processedLocation.GeoreferencedBy = verbatimObservation.GeoreferencedBy;
            processedLocation.GeoreferencedDate = verbatimObservation.GeoreferencedDate;
            processedLocation.GeoreferenceProtocol = verbatimObservation.GeoreferenceProtocol;
            processedLocation.GeoreferenceRemarks = verbatimObservation.GeoreferenceRemarks;
            processedLocation.GeoreferenceSources = verbatimObservation.GeoreferenceSources;
            processedLocation.GeoreferenceVerificationStatus = verbatimObservation.GeoreferenceVerificationStatus;
            processedLocation.HigherGeography = verbatimObservation.HigherGeography;
            processedLocation.HigherGeographyID = verbatimObservation.HigherGeographyID;
            processedLocation.Island = verbatimObservation.Island;
            processedLocation.IslandGroup = verbatimObservation.IslandGroup;
            processedLocation.Locality = verbatimObservation.Locality;
            processedLocation.LocationAccordingTo = verbatimObservation.LocationAccordingTo;
            processedLocation.LocationId = verbatimObservation.LocationID;
            processedLocation.LocationRemarks = verbatimObservation.LocationRemarks;
            processedLocation.MaximumDepthInMeters = verbatimObservation.MaximumDepthInMeters.ParseDouble();
            processedLocation.MaximumDistanceAboveSurfaceInMeters =
                verbatimObservation.MaximumDistanceAboveSurfaceInMeters.ParseDouble();
            processedLocation.MaximumElevationInMeters = verbatimObservation.MaximumElevationInMeters.ParseDouble();
            processedLocation.MinimumDepthInMeters = verbatimObservation.MinimumDepthInMeters.ParseDouble();
            processedLocation.MinimumDistanceAboveSurfaceInMeters =
                verbatimObservation.MinimumDistanceAboveSurfaceInMeters.ParseDouble();
            processedLocation.MinimumElevationInMeters = verbatimObservation.MinimumElevationInMeters.ParseDouble();
            processedLocation.Attributes.VerbatimMunicipality = verbatimObservation.Municipality;
            processedLocation.Attributes.VerbatimProvince = verbatimObservation.StateProvince;
            processedLocation.VerbatimCoordinates = verbatimObservation.VerbatimCoordinates;
            processedLocation.VerbatimCoordinateSystem = verbatimObservation.VerbatimCoordinateSystem;
            processedLocation.VerbatimDepth = verbatimObservation.VerbatimDepth;
            processedLocation.VerbatimElevation = verbatimObservation.VerbatimElevation;
            processedLocation.VerbatimLatitude = verbatimObservation.VerbatimLatitude;
            processedLocation.VerbatimLocality = verbatimObservation.VerbatimLocality;
            processedLocation.VerbatimLongitude = verbatimObservation.VerbatimLongitude;
            processedLocation.VerbatimSRS = verbatimObservation.VerbatimSRS;
            processedLocation.WaterBody = verbatimObservation.WaterBody;

            if (string.IsNullOrWhiteSpace(processedLocation.VerbatimLatitude) &&
                string.IsNullOrWhiteSpace(processedLocation.VerbatimLongitude) &&
                string.IsNullOrWhiteSpace(processedLocation.VerbatimSRS))
            {
                processedLocation.VerbatimLatitude = verbatimObservation.DecimalLatitude;
                processedLocation.VerbatimLongitude = verbatimObservation.DecimalLongitude;
                processedLocation.VerbatimSRS = verbatimObservation.GeodeticDatum;
            }

            if (!processedLocation.DecimalLongitude.HasValue || !processedLocation.DecimalLatitude.HasValue)
            {
                return processedLocation; // No coordinates provided
            }

            Point wgs84Point = null;
            if (string.IsNullOrWhiteSpace(processedLocation.GeodeticDatum)) // Assume WGS84 if GeodeticDatum is empty.
            {
                wgs84Point = new Point(processedLocation.DecimalLongitude.Value,
                    processedLocation.DecimalLatitude.Value);
            }
            else
            {
                var originalPoint = new Point(processedLocation.DecimalLongitude.Value,
                    processedLocation.DecimalLatitude.Value);
                if (GISExtensions.TryParseCoordinateSystem(processedLocation.GeodeticDatum, out var coordinateSystem))
                {
                    wgs84Point = (Point) originalPoint.Transform(coordinateSystem, CoordinateSys.WGS84);
                    processedLocation.DecimalLongitude = wgs84Point.X;
                    processedLocation.DecimalLatitude = wgs84Point.Y;
                }
            }

            processedLocation.GeodeticDatum = CoordinateSys.WGS84.EpsgCode();
            processedLocation.Point = (PointGeoShape) wgs84Point?.ToGeoShape();
            processedLocation.PointLocation = wgs84Point?.ToGeoLocation();
            processedLocation.PointWithBuffer =
                (PolygonGeoShape) wgs84Point?.ToCircle(processedLocation.CoordinateUncertaintyInMeters)?.ToGeoShape();
            return processedLocation;
        }


        private Occurrence CreateProcessedOccurrence(DwcObservationVerbatim verbatimObservation)
        {
            var processedOccurrence = new Occurrence();
            processedOccurrence.AssociatedMedia = verbatimObservation.AssociatedMedia;
            processedOccurrence.AssociatedReferences = verbatimObservation.AssociatedReferences;
            processedOccurrence.AssociatedSequences = verbatimObservation.AssociatedSequences;
            processedOccurrence.AssociatedTaxa = verbatimObservation.AssociatedTaxa;
            processedOccurrence.CatalogNumber = verbatimObservation.CatalogNumber ?? verbatimObservation.OccurrenceID;
            processedOccurrence.Disposition = verbatimObservation.Disposition;
            processedOccurrence.EstablishmentMeans = GetSosId(verbatimObservation.EstablishmentMeans,
                _vocabularyById[VocabularyId.EstablishmentMeans]);
            processedOccurrence.IndividualCount = verbatimObservation.IndividualCount;
            processedOccurrence.LifeStage = GetSosId(verbatimObservation.LifeStage, _vocabularyById[VocabularyId.LifeStage]);
            processedOccurrence.Media = CreateProcessedMultimedia(
                verbatimObservation.ObservationMultimedia,
                verbatimObservation.ObservationAudubonMedia);
            processedOccurrence.OccurrenceId = verbatimObservation.OccurrenceID;
            processedOccurrence.OccurrenceRemarks = verbatimObservation.OccurrenceRemarks;
            processedOccurrence.OccurrenceStatus = GetSosId(
                verbatimObservation.OccurrenceStatus,
                _vocabularyById[VocabularyId.OccurrenceStatus],
                (int)OccurrenceStatusId.Present);
            processedOccurrence.OrganismQuantity = verbatimObservation.OrganismQuantity;
            processedOccurrence.OrganismQuantityUnit = GetSosId(verbatimObservation.OrganismQuantityType, _vocabularyById[VocabularyId.Unit]);
            processedOccurrence.OtherCatalogNumbers = verbatimObservation.OtherCatalogNumbers;
            processedOccurrence.Preparations = verbatimObservation.Preparations;
            processedOccurrence.RecordedBy = verbatimObservation.RecordedBy;
            processedOccurrence.RecordNumber = verbatimObservation.RecordNumber;
            processedOccurrence.Activity = GetSosId(
                verbatimObservation.ReproductiveCondition,
                _vocabularyById[VocabularyId.Activity]);
            processedOccurrence.Sex = GetSosId(verbatimObservation.Sex, _vocabularyById[VocabularyId.Sex]);
            processedOccurrence.ReproductiveCondition = GetSosId(verbatimObservation.ReproductiveCondition, _vocabularyById.GetValue(VocabularyId.ReproductiveCondition));
            processedOccurrence.Behavior = GetSosId(verbatimObservation.Behavior, _vocabularyById.GetValue(VocabularyId.Behavior));
            processedOccurrence.IsNaturalOccurrence = true;
            processedOccurrence.IsNeverFoundObservation = false;
            processedOccurrence.IsNotRediscoveredObservation = false;
            processedOccurrence.IsPositiveObservation = true; 
            if (processedOccurrence.OccurrenceStatus?.Id == (int) OccurrenceStatusId.Absent)
            {
                processedOccurrence.IsPositiveObservation = false;
                processedOccurrence.IsNeverFoundObservation = true;
            }

            // todo - handle the following fields:
            // processedOccurrence.BirdNestActivityId = GetBirdNestActivityId(verbatimObservation, taxon),
            // processedOccurrence.URL = $"http://www.artportalen.se/sighting/{verbatimObservation.Id}"

            return processedOccurrence;
        }

        private Lib.Models.Processed.Observation.Taxon CreateProcessedTaxon(DwcObservationVerbatim verbatimObservation)
        {
            // Get all taxon values from Dyntaxa instead of the provided DarwinCore data.
            var processedTaxon = TryGetTaxonInformation(
                verbatimObservation.TaxonID,
                verbatimObservation.ScientificName,
                verbatimObservation.ScientificNameAuthorship,
                verbatimObservation.VernacularName,
                verbatimObservation.Kingdom,
                verbatimObservation.TaxonRank);

            return processedTaxon;
        }

        private Lib.Models.Processed.Observation.Taxon TryGetTaxonInformation(
            string taxonId,
            string scientificName,
            string scientificNameAuthorship,
            string vernacularName,
            string kingdom,
            string taxonRank)
        {
            Lib.Models.Processed.Observation.Taxon taxon = null;

            if (!string.IsNullOrEmpty(taxonId))
            {
                string lastInteger = Regex.Match(taxonId, @"\d+", RegexOptions.RightToLeft).Value;
                //string firstInteger = Regex.Match(taxonId, @"\d+").Value;
                if (int.TryParse(lastInteger, out int parsedTaxonId))
                {
                    _taxonByTaxonId.TryGetValue(parsedTaxonId, out taxon);
                }
                
                if (taxon != null) return taxon;
            }
            

            if (_taxonByScientificName.TryGetValues(scientificName?.ToLower(), out var result))
            {
                if (result.Count == 1)
                {
                    taxon = result.First();
                }
            }

            if (_taxonBySynonymName.TryGetValues(scientificName?.ToLower(), out var synonymeResult))
            {
                if (synonymeResult.Count == 1)
                {
                    taxon = synonymeResult.First();
                }
            }

            return taxon;
        }

        private VocabularyValue GetSosId(string val,
            IDictionary<object, int> sosIdByValue,
            int? defaultValue = null,
            MappingNotFoundLogic mappingNotFoundLogic = MappingNotFoundLogic.UseSourceValue)
        {
            if (string.IsNullOrWhiteSpace(val) || sosIdByValue == null)
            {
                return defaultValue.HasValue ? new VocabularyValue {Id = defaultValue.Value} : null;
            }

            var lookupVal = val.ToLower();
            if (sosIdByValue.TryGetValue(lookupVal, out var sosId))
            {
                return new VocabularyValue {Id = sosId};
            }

            if (mappingNotFoundLogic == MappingNotFoundLogic.UseDefaultValue && defaultValue.HasValue)
            {
                return new VocabularyValue {Id = defaultValue.Value};
            }

            return new VocabularyValue
                {Id = VocabularyConstants.NoMappingFoundCustomValueIsUsedId, Value = val};
        }


        private Lib.Models.Processed.Observation.ProjectParameter CreateProcessedProjectParameter(Lib.Models.Verbatim.Artportalen.ProjectParameter projectParameter)
        {
            if (projectParameter == null)
            {
                return null;
            }

            return new Lib.Models.Processed.Observation.ProjectParameter
            {
                Value = projectParameter.Value,
                DataType = projectParameter.DataType,
                Description = projectParameter.Description,
                Name = projectParameter.Name,
                Id = projectParameter.Id,
                Unit = projectParameter.Unit
            };
        }

        /// <summary>
        ///     Get vocabulary mappings for Artportalen.
        /// </summary>
        /// <param name="externalSystemId"></param>
        /// <param name="allVocabularies"></param>
        /// <param name="convertValuesToLowercase"></param>
        /// <returns></returns>
        public static IDictionary<VocabularyId, IDictionary<object, int>> GetVocabulariesDictionary(
            ExternalSystemId externalSystemId,
            ICollection<Vocabulary> allVocabularies,
            bool convertValuesToLowercase)
        {
            var dic = new Dictionary<VocabularyId, IDictionary<object, int>>();

            foreach (var vocabulary in allVocabularies)
            {
                var vocabularies = vocabulary.ExternalSystemsMapping.FirstOrDefault(m => m.Id == externalSystemId);
                if (vocabularies != null)
                {
                    var mapping = vocabularies.Mappings.Single();
                    var sosIdByValue = mapping.GetIdByValueDictionary(convertValuesToLowercase);
                    dic.Add(vocabulary.Id, sosIdByValue);
                }
            }

            // Add missing entries. Initialize with empty dictionary.
            foreach (VocabularyId vocabularyId in (VocabularyId[])Enum.GetValues(typeof(VocabularyId)))
            {
                if (!dic.ContainsKey(vocabularyId))
                {
                    dic.Add(vocabularyId, new Dictionary<object, int>());
                }
            }

            return dic;
        }

        private enum MappingNotFoundLogic
        {
            UseSourceValue,
            UseDefaultValue
        }

        public void ValidateVerbatimData(DwcObservationVerbatim verbatimObservation, DwcaValidationRemarksBuilder validationRemarksBuilder)
        {
            validationRemarksBuilder.NrValidatedObservations++;

            if (string.IsNullOrWhiteSpace(verbatimObservation.CoordinateUncertaintyInMeters))
            {
                validationRemarksBuilder.NrMissingCoordinateUncertaintyInMeters++;
            }

            if (string.IsNullOrWhiteSpace(verbatimObservation.IdentificationVerificationStatus))
            {
                validationRemarksBuilder.NrMissingIdentificationVerificationStatus++;
            }
        }
    }
}