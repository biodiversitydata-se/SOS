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
using SOS.Lib.Enums.FieldMappingValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Resource.Interfaces;
using FieldMapping = SOS.Lib.Models.Shared.FieldMapping;

namespace SOS.Process.Processors.DarwinCoreArchive
{
    /// <summary>
    ///     DwC-A observation factory.
    /// </summary>
    public class DwcaObservationFactory
    {
        private const int DefaultCoordinateUncertaintyInMeters = 10000;
        private readonly IAreaHelper _areaHelper;
        private readonly DataProvider _dataProvider;
        private readonly IDictionary<FieldMappingFieldId, IDictionary<object, int>> _fieldMappings;
        private readonly HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon> _taxonByScientificName;
        private readonly HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon> _taxonBySynonymeName;
        private readonly IDictionary<int, Lib.Models.Processed.Observation.Taxon> _taxonByTaxonId;

        private readonly List<string> errors = new List<string>();

        public DwcaObservationFactory(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IDictionary<FieldMappingFieldId, IDictionary<object, int>> fieldMappings,
            IAreaHelper areaHelper)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _taxonByTaxonId = taxa ?? throw new ArgumentNullException(nameof(taxa));
            _fieldMappings = fieldMappings ?? throw new ArgumentNullException(nameof(fieldMappings));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));

            _taxonByScientificName = new HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon>();
            _taxonBySynonymeName = new HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon>();
            foreach (var processedTaxon in _taxonByTaxonId.Values)
            {
                _taxonByScientificName.Add(processedTaxon.ScientificName.ToLower(), processedTaxon);
                if (processedTaxon.Synonyms != null)
                {
                    foreach (var synonyme in processedTaxon.Synonyms)
                    {
                        _taxonBySynonymeName.Add(synonyme.Name.ToLower(), processedTaxon);
                    }
                }
            }
        }

        public static async Task<DwcaObservationFactory> CreateAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IFieldMappingRepository processedFieldMappingRepository,
            IAreaHelper areaHelper)
        {
            var allFieldMappings = await processedFieldMappingRepository.GetAllAsync();
            var fieldMappings = GetFieldMappingsDictionary(
                ExternalSystemId.DarwinCore,
                allFieldMappings.ToArray(),
                true);
            return new DwcaObservationFactory(dataProvider, taxa, fieldMappings, areaHelper);
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
            obs.Media = CreateProcessedMultimedia(
                verbatimObservation.ObservationMultimedia,
                verbatimObservation.ObservationAudubonMedia);
            if (verbatimObservation.ObservationMeasurementOrFacts.HasItems())
                obs.MeasurementOrFacts = verbatimObservation.ObservationMeasurementOrFacts?.Select(dwcMof => dwcMof.ToProcessedExtendedMeasurementOrFact()).ToArray();
            else if (verbatimObservation.ObservationExtendedMeasurementOrFacts.HasItems())
                obs.MeasurementOrFacts = verbatimObservation.ObservationExtendedMeasurementOrFacts?.Select(dwcMof => dwcMof.ToProcessedExtendedMeasurementOrFact()).ToArray();
            obs.AccessRights = GetSosId(verbatimObservation.AccessRights,
                _fieldMappings[FieldMappingFieldId.AccessRights]);
            obs.BasisOfRecord = GetSosId(verbatimObservation.BasisOfRecord,
                _fieldMappings[FieldMappingFieldId.BasisOfRecord]);
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
                _fieldMappings[FieldMappingFieldId.Institution]); // todo - Create DarwinCore field mapping.
            obs.Language = verbatimObservation.Language;
            obs.License = verbatimObservation.License;
            obs.Modified = DwcParser.ParseDate(verbatimObservation.Modified)?.ToUniversalTime();
            obs.OwnerInstitutionCode = verbatimObservation.OwnerInstitutionCode;
            obs.References = verbatimObservation.References;
            obs.RightsHolder = verbatimObservation.RightsHolder;
            obs.Type = GetSosId(verbatimObservation.Type,
                _fieldMappings[FieldMappingFieldId.Type]); // todo - Create DarwinCore field mapping.

            // todo - handle the following fields?
            // obs.Projects = verbatimObservation.Projects?.Select(CreateProcessedProject);
            // obs.ProtectionLevel = CalculateProtectionLevel(taxon, verbatimObservation.HiddenByProvider, verbatimObservation.ProtectedBySystem);
            // obs.ReportedBy = verbatimObservation.ReportedBy;
            // obs.ReportedDate = verbatimObservation.ReportedDate;

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

            // Code from ArtportalenObservationFactory
            // Taxon
            // ========
            // var taxonId = verbatimObservation.TaxonId ?? -1;
            // if (_taxa.TryGetValue(taxonId, out var taxon))
            // {
            //     taxon.IndividualId = verbatimObservation.URL;
            // }
            //
            // Location
            //================
            // var hasPosition = (verbatimObservation.Site?.XCoord ?? 0) > 0 && (verbatimObservation.Site?.YCoord ?? 0) > 0;
            //    IsInEconomicZoneOfSweden = hasPosition, // Artportalen validate all sightings, we rely on that validation as long it has coordinates
            //        Point = (PointGeoShape)verbatimObservation.Site?.Point?.ToGeoShape(),
            //        PointLocation = verbatimObservation.Site?.Point?.ToGeoLocation(),
            //        PointWithBuffer = (PolygonGeoShape)verbatimObservation.Site?.PointWithBuffer?.ToGeoShape(),
            //
            // Occurrence
            //================
            // BirdNestActivityId = GetBirdNestActivityId(verbatimObservation, taxon),
            // IsNaturalOccurrence = !verbatimObservation.Unspontaneous,
            // IsNeverFoundObservation = verbatimObservation.NotPresent,
            // IsNotRediscoveredObservation = verbatimObservation.NotRecovered,
            // IsPositiveObservation = !(verbatimObservation.NotPresent || verbatimObservation.NotRecovered),
            // URL = $"http://www.artportalen.se/sighting/{verbatimObservation.Id}"
            //
            // Record level & other
            //=======================
            // Projects = verbatimObservation.Projects?.Select(CreateProcessedProject),
            // ProtectionLevel = CalculateProtectionLevel(taxon, verbatimObservation.HiddenByProvider, verbatimObservation.ProtectedBySystem),
            // ReportedBy = verbatimObservation.ReportedBy,
            // ReportedDate = verbatimObservation.ReportedDate,
            //
            // Event
            //=====================
            // obs.Event.Biotope = GetSosId(verbatimObservation?.Bioptope?.Id, _fieldMappings[FieldMappingFieldId.Biotope]);
            // obs.Event.Substrate = GetSosId(verbatimObservation?.Bioptope?.Id, _fieldMappings[FieldMappingFieldId.Substrate]);

            //return obs;
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


            // todo - Can we map the following fields?
            // processedEvent.Biotope = GetSosId(verbatimObservation?.Bioptope?.Id, _fieldMappings[FieldMappingFieldId.Biotope]);
            // processedEvent.Substrate = GetSosId(verbatimObservation?.Bioptope?.Id, _fieldMappings[FieldMappingFieldId.Substrate]);

            return processedEvent;
        }

        private Identification CreateProcessedIdentification(DwcObservationVerbatim verbatimObservation)
        {
            var processedIdentification = new Identification();
            processedIdentification.DateIdentified = verbatimObservation.DateIdentified?.ParseDateTime()?.ToUniversalTime();
            processedIdentification.IdentificationId = verbatimObservation.IdentificationID;
            processedIdentification.IdentificationQualifier = verbatimObservation.IdentificationQualifier;
            processedIdentification.IdentificationReferences = verbatimObservation.IdentificationReferences;
            processedIdentification.IdentificationRemarks = verbatimObservation.IdentificationRemarks;
            processedIdentification.ValidationStatus = GetSosId(verbatimObservation.IdentificationVerificationStatus, _fieldMappings[FieldMappingFieldId.ValidationStatus]);
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
                _fieldMappings[FieldMappingFieldId.Continent],
                (int) ContinentId.Europe,
                MappingNotFoundLogic.UseDefaultValue);
            processedLocation.CoordinatePrecision = verbatimObservation.CoordinatePrecision.ParseDouble();
            processedLocation.CoordinateUncertaintyInMeters =
                verbatimObservation.CoordinateUncertaintyInMeters?.ParseInt() ?? DefaultCoordinateUncertaintyInMeters;
            processedLocation.Country = GetSosId(
                verbatimObservation.Country,
                _fieldMappings[FieldMappingFieldId.Country],
                (int) CountryId.Sweden,
                MappingNotFoundLogic.UseDefaultValue);
            processedLocation.CountryCode = verbatimObservation.CountryCode;
            //processedLocation.County = GetSosId(verbatimObservation.County, _fieldMappings[FieldMappingFieldId.County]); // Mapped by AreaHelper
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
            //processedLocation.Municipality = GetSosId(verbatimObservation.Municipality, _fieldMappings[FieldMappingFieldId.Municipality]); // Mapped by AreaHelper
            processedLocation.VerbatimMunicipality = verbatimObservation.Municipality;
            //processedLocation.Province = GetSosId(verbatimObservation.StateProvince, _fieldMappings[FieldMappingFieldId.Province]); // Mapped by AreaHelper
            processedLocation.VerbatimProvince = verbatimObservation.StateProvince;
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


        private double? ParseDouble(string strValue, string fieldName)
        {
            var result = strValue.ParseDouble();
            if (!result.HasValue && strValue.HasValue())
            {
                var errorText = $"The field {fieldName} with a value of [{strValue}] is not a valid {typeof(double)}";
                errors.Add(errorText);
            }

            return result;
        }

        private Occurrence CreateProcessedOccurrence(DwcObservationVerbatim verbatimObservation)
        {
            var processedOccurrence = new Occurrence();
            processedOccurrence.AssociatedMedia = verbatimObservation.AssociatedMedia;
            processedOccurrence.AssociatedReferences = verbatimObservation.AssociatedReferences;
            processedOccurrence.AssociatedSequences = verbatimObservation.AssociatedSequences;
            processedOccurrence.AssociatedTaxa = verbatimObservation.AssociatedTaxa;
            processedOccurrence.Behavior = verbatimObservation.Behavior;
            processedOccurrence.CatalogNumber = verbatimObservation.CatalogNumber ?? verbatimObservation.OccurrenceID;
            processedOccurrence.Disposition = verbatimObservation.Disposition;
            processedOccurrence.EstablishmentMeans = GetSosId(verbatimObservation.EstablishmentMeans,
                _fieldMappings[FieldMappingFieldId.EstablishmentMeans]);
            processedOccurrence.IndividualCount = verbatimObservation.IndividualCount;
            processedOccurrence.LifeStage = GetSosId(verbatimObservation.LifeStage,
                _fieldMappings[FieldMappingFieldId.LifeStage]); // todo - create DarwinCore field mapping for FieldMappingFieldId.LifeStage.
            processedOccurrence.OccurrenceId = verbatimObservation.OccurrenceID;
            processedOccurrence.OccurrenceRemarks = verbatimObservation.OccurrenceRemarks;
            processedOccurrence.OccurrenceStatus = GetSosId(
                verbatimObservation.OccurrenceStatus,
                _fieldMappings[FieldMappingFieldId.OccurrenceStatus],
                (int)OccurrenceStatusId.Present);
            processedOccurrence.OrganismQuantity = verbatimObservation.OrganismQuantity;
            processedOccurrence.OrganismQuantityUnit = GetSosId(verbatimObservation.OrganismQuantityType,
                _fieldMappings[FieldMappingFieldId.Unit]); // todo - create DarwinCore field mapping for FieldMappingFieldId.OrganismQuantityUnit.
            processedOccurrence.OtherCatalogNumbers = verbatimObservation.OtherCatalogNumbers;
            processedOccurrence.Preparations = verbatimObservation.Preparations;
            processedOccurrence.RecordedBy = verbatimObservation.RecordedBy;
            processedOccurrence.RecordNumber = verbatimObservation.RecordNumber;
            processedOccurrence.Activity = GetSosId(verbatimObservation.ReproductiveCondition,
                _fieldMappings[FieldMappingFieldId.Activity]); // todo - create DarwinCore field mapping for FieldMappingFieldId.Activity.

            processedOccurrence.Gender = GetSosId(verbatimObservation.Sex, _fieldMappings[FieldMappingFieldId.Gender]);
            processedOccurrence.IsNaturalOccurrence = true;
            processedOccurrence.IsNeverFoundObservation =
                false; // todo - Add the following logic? dyntaxaTaxonId == 0; // Set to False if DyntaxaTaxonId from provider is greater than 0 and True if DyntaxaTaxonId is 0.
            processedOccurrence.IsNotRediscoveredObservation = false;
            processedOccurrence.IsPositiveObservation =
                true; // todo - Add the following logic? dyntaxaTaxonId != 0; // Set to True if DyntaxaTaxonId from provider is greater than 0 and False if DyntaxaTaxonId is 0.
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

            if (_taxonBySynonymeName.TryGetValues(scientificName?.ToLower(), out var synonymeResult))
            {
                if (synonymeResult.Count == 1)
                {
                    taxon = synonymeResult.First();
                }
            }

            return taxon;
        }

        //private ProcessedFieldMapValue GetSosId(string val,
        //    IDictionary<object, int> sosIdByValue,
        //    int? defaultValueIfNull = null,
        //    int? defaultValueIfNoMappingFound = null)
        //{
        //    if (string.IsNullOrWhiteSpace(val) || sosIdByValue == null)
        //    {
        //        return defaultValueIfNull.HasValue ? new ProcessedFieldMapValue { Id = defaultValueIfNull.Value } : null;
        //    }

        //    string lookupVal = val.ToLower();
        //    if (sosIdByValue.TryGetValue(lookupVal, out var sosId))
        //    {
        //        return new ProcessedFieldMapValue { Id = sosId };
        //    }

        //    if (defaultValueIfNoMappingFound.HasValue)
        //    {
        //        return new ProcessedFieldMapValue {Id = defaultValueIfNoMappingFound.Value};
        //    }

        //    return new ProcessedFieldMapValue { Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId, Value = val };
        //}

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
                {Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId, Value = val};
        }


        /// <summary>
        ///     Get SOS internal Id for the id specific for the data provider.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sosIdByValue"></param>
        /// <returns></returns>
        private static VocabularyValue GetSosId(int? val, IDictionary<object, int> sosIdByValue)
        {
            if (!val.HasValue || sosIdByValue == null) return null;

            if (sosIdByValue.TryGetValue(val.Value, out var sosId))
            {
                return new VocabularyValue {Id = sosId};
            }

            return new VocabularyValue
                {Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId, Value = val.ToString()};
        }

        private Lib.Models.Processed.Observation.Project CreateProcessedProject(Lib.Models.Verbatim.Artportalen.Project project)
        {
            if (project == null) return null;

            return new Lib.Models.Processed.Observation.Project
            {
                IsPublic = project.IsPublic,
                Category = project.Category,
                Description = project.Description,
                EndDate = project.EndDate?.ToUniversalTime(),
                Id = project.Id,
                Name = project.Name,
                Owner = project.Owner,
                StartDate = project.StartDate?.ToUniversalTime(),
                SurveyMethod = project.SurveyMethod,
                SurveyMethodUrl = project.SurveyMethodUrl,
                ProjectParameters = project.ProjectParameters?.Select(this.CreateProcessedProjectParameter)
            };
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

        private string GetSamplingProtocol(IEnumerable<Lib.Models.Verbatim.Artportalen.Project> projects)
        {
            if (!projects?.Any() ?? true) return null;

            var project = projects.First();

            if (projects.Count() == 1)
            {
                return project?.SurveyMethod ?? project?.SurveyMethodUrl;
            }

            var firstSurveyMethod = project.SurveyMethod;
            if (firstSurveyMethod != null && projects.All(p => p.SurveyMethod == firstSurveyMethod))
            {
                return firstSurveyMethod;
            }

            var firstSurveyMethodUrl = project.SurveyMethodUrl;
            if (firstSurveyMethodUrl != null && projects.All(p => p.SurveyMethod == firstSurveyMethodUrl))
            {
                return firstSurveyMethodUrl;
            }

            return null;
        }

        /// <summary>
        ///     Calculate protection level
        /// </summary>
        /// <param name="taxon"></param>
        /// <param name="hiddenByProvider"></param>
        /// <param name="protectedBySystem"></param>
        /// <returns></returns>
        private int CalculateProtectionLevel(Lib.Models.Processed.Observation.Taxon taxon, DateTime? hiddenByProvider, bool protectedBySystem)
        {
            if (string.IsNullOrEmpty(taxon?.ProtectionLevel))
            {
                return 1;
            }

            var regex = new Regex(@"^\d");

            if (int.TryParse(regex.Match(taxon.ProtectionLevel).Value, out var protectionLevel))
            {
                if (protectionLevel <= 3 && hiddenByProvider.HasValue && hiddenByProvider.Value >= DateTime.Now)
                {
                    return 3;
                }

                if (protectionLevel > 3 && hiddenByProvider.HasValue && hiddenByProvider.Value >= DateTime.Now ||
                    protectedBySystem)
                {
                    return protectionLevel;
                }
            }

            return 1;
        }

        /// <summary>
        ///     Build the substrate description string
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        private string GetSubstrateDescription(ArtportalenObservationVerbatim verbatimObservation,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa)
        {
            if (verbatimObservation == null)
            {
                return null;
            }

            var substrateDescription = new StringBuilder();

            if (verbatimObservation.QuantityOfSubstrate.HasValue)
            {
                substrateDescription.Append($"{verbatimObservation.QuantityOfSubstrate.Value} substratenheter");
            }

            if (verbatimObservation.Substrate != null)
            {
                substrateDescription.Append(
                    $"{(substrateDescription.Length == 0 ? "" : " # ")}{verbatimObservation.Substrate.Translate(Cultures.en_GB)}");
            }

            if (!string.IsNullOrEmpty(verbatimObservation.SubstrateDescription))
            {
                substrateDescription.Append(
                    $"{(substrateDescription.Length == 0 ? "" : " # ")}{verbatimObservation.SubstrateDescription}");
            }

            if (verbatimObservation.SubstrateSpeciesId.HasValue &&
                taxa != null &&
                taxa.TryGetValue(verbatimObservation.SubstrateSpeciesId.Value, out var taxon))
            {
                substrateDescription.Append($"{(substrateDescription.Length == 0 ? "" : " # ")}{taxon.ScientificName}");
            }

            if (!string.IsNullOrEmpty(verbatimObservation.SubstrateSpeciesDescription))
            {
                substrateDescription.Append(
                    $"{(substrateDescription.Length == 0 ? "" : " # ")}{verbatimObservation.SubstrateSpeciesDescription}");
            }

            var res = substrateDescription.Length > 0 ? substrateDescription.ToString().WithMaxLength(255) : null;
            return res;
        }

        /// <summary>
        ///     Get bird nest activity id
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <param name="taxon"></param>
        /// <returns></returns>
        public int? GetBirdNestActivityId(ArtportalenObservationVerbatim verbatimObservation, Lib.Models.Processed.Observation.Taxon taxon)
        {
            if (verbatimObservation == null || taxon == null)
            {
                return null;
            }

            if (taxon.OrganismGroup?.StartsWith("fåg", StringComparison.CurrentCultureIgnoreCase) ?? false)
            {
                return (verbatimObservation.Activity?.Id ?? 0) == 0 ? 1000000 : verbatimObservation.Activity.Id;
            }

            return 0;
        }

        /// <summary>
        ///     Get associated references
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <returns></returns>
        private string GetAssociatedReferences(ArtportalenObservationVerbatim verbatimObservation)
        {
            if (!verbatimObservation?.MigrateSightingObsId.HasValue ?? true)
            {
                return null;
            }

            string associatedReferences = null;
            switch (verbatimObservation.MigrateSightingPortalId ?? 0)
            {
                case 1:
                    associatedReferences =
                        $"urn:lsid:artportalen.se:Sighting:Bird.{verbatimObservation.MigrateSightingObsId.Value}";
                    break;
                case 2:
                    associatedReferences =
                        $"urn:lsid:artportalen.se:Sighting:PlantAndMushroom.{verbatimObservation.MigrateSightingObsId.Value}";
                    break;
                case 6:
                    associatedReferences =
                        $"urn:lsid:artportalen.se:Sighting:Vertebrate.{verbatimObservation.MigrateSightingObsId.Value}";
                    break;
                case 7:
                    associatedReferences =
                        $"urn:lsid:artportalen.se:Sighting:Bugs.{verbatimObservation.MigrateSightingObsId.Value}";
                    break;
                case 8:
                    associatedReferences =
                        $"urn:lsid:artportalen.se:Sighting:Fish.{verbatimObservation.MigrateSightingObsId.Value}";
                    break;
                case 9:
                    associatedReferences =
                        $"urn:lsid:artportalen.se:Sighting:MarineInvertebrates.{verbatimObservation.MigrateSightingObsId.Value}";
                    break;
            }

            return associatedReferences;
        }

        /// <summary>
        ///     Get field mappings for Artportalen.
        /// </summary>
        /// <param name="externalSystemId"></param>
        /// <param name="allFieldMappings"></param>
        /// <param name="convertValuesToLowercase"></param>
        /// <returns></returns>
        public static IDictionary<FieldMappingFieldId, IDictionary<object, int>> GetFieldMappingsDictionary(
            ExternalSystemId externalSystemId,
            ICollection<FieldMapping> allFieldMappings,
            bool convertValuesToLowercase)
        {
            var dic = new Dictionary<FieldMappingFieldId, IDictionary<object, int>>();

            foreach (var fieldMapping in allFieldMappings)
            {
                var fieldMappings = fieldMapping.ExternalSystemsMapping.FirstOrDefault(m => m.Id == externalSystemId);
                if (fieldMappings != null)
                {
                    var mapping = fieldMappings.Mappings.Single();
                    var sosIdByValue = mapping.GetIdByValueDictionary(convertValuesToLowercase);
                    dic.Add(fieldMapping.Id, sosIdByValue);
                }
            }

            // Add missing entries. Initialize with empty dictionary.
            foreach (FieldMappingFieldId fieldMappingFieldId in (FieldMappingFieldId[])Enum.GetValues(typeof(FieldMappingFieldId)))
            {
                if (!dic.ContainsKey(fieldMappingFieldId))
                {
                    dic.Add(fieldMappingFieldId, new Dictionary<object, int>());
                }
            }

            return dic;
        }

        private static string GetMappingKey(FieldMappingFieldId fieldMappingFieldId)
        {
            switch (fieldMappingFieldId)
            {
                case FieldMappingFieldId.Gender:
                    return "sex";
                case FieldMappingFieldId.County:
                    return "county";
                case FieldMappingFieldId.Municipality:
                    return "municipality";
                case FieldMappingFieldId.Province:
                    return "stateProvince";
                case FieldMappingFieldId.BasisOfRecord:
                    return "basisOfRecord";
                case FieldMappingFieldId.Continent:
                    return "continent";
                case FieldMappingFieldId.EstablishmentMeans:
                    return "establishmentMeans";
                case FieldMappingFieldId.OccurrenceStatus:
                    return "occurrenceStatus";
                case FieldMappingFieldId.AccessRights:
                    return "accessRights";
                case FieldMappingFieldId.Country:
                    return "country";
                case FieldMappingFieldId.Type:
                    return "type";
                default:
                    throw new ArgumentException($"No mapping exist for the field: {fieldMappingFieldId}");
            }
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