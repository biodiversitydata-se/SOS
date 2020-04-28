using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using Nest;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.FieldMappingValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using FieldMapping = SOS.Lib.Models.Shared.FieldMapping;
using Language = SOS.Lib.Models.DarwinCore.Vocabulary.Language;

namespace SOS.Process.Processors.DarwinCoreArchive
{
    /// <summary>
    /// DwC-A observation factory.
    /// </summary>
    public class DwcaObservationFactory
    {
        private readonly IDictionary<int, ProcessedTaxon> _taxa;
        private readonly IDictionary<FieldMappingFieldId, IDictionary<object, int>> _fieldMappings;
        private readonly IAreaHelper _areaHelper;

        public DwcaObservationFactory(
            IDictionary<int, ProcessedTaxon> taxa,
            IDictionary<FieldMappingFieldId, IDictionary<object, int>> fieldMappings,
            IAreaHelper areaHelper)
        {
            {
                _taxa = taxa ?? throw new ArgumentNullException(nameof(taxa));
                _fieldMappings = fieldMappings ?? throw new ArgumentNullException(nameof(fieldMappings));
                _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            }
        }

        public static async Task<DwcaObservationFactory> CreateAsync(
            IDictionary<int, ProcessedTaxon> taxa,
            IProcessedFieldMappingRepository processedFieldMappingRepository, 
            IAreaHelper areaHelper)
        {
            var allFieldMappings = await processedFieldMappingRepository.GetAllAsync();
            var fieldMappings = GetFieldMappingsDictionary(
                ExternalSystemId.DarwinCore, 
                allFieldMappings.ToArray(),
                convertValuesToLowercase: true);
            return new DwcaObservationFactory(taxa, fieldMappings, areaHelper);
        }

        public IEnumerable<ProcessedObservation> CreateProcessedObservations(IEnumerable<DwcObservationVerbatim> verbatimObservations)
        {
            return verbatimObservations.Select(CreateProcessedObservation);
        }

        /// <summary>
        /// Cast verbatim observations to processed data model
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <returns></returns>
        public ProcessedObservation CreateProcessedObservation(DwcObservationVerbatim verbatimObservation)
        {
            if (verbatimObservation == null)
            {
                return null;
            }

            var obs = new ProcessedObservation(ObservationProvider.Dwca);

            // Other
            //obs.Id = verbatimObservation.Id;
            // todo - handle properties below.
            //obs.DataProviderId = verbatimObservation.DataProviderId;
            //obs.DataProviderIdentifier = verbatimObservation.DataProviderIdentifier;
            //obs.ObservationAudubonMedia = verbatimObservation.ObservationAudubonMedia;
            //obs.EventAudubonMedia = verbatimObservation.EventAudubonMedia;
            //obs.ObservationMultimedia = verbatimObservation.ObservationMultimedia;
            //obs.EventMultimedia = verbatimObservation.EventMultimedia;
            //obs.ObservationMeasurementOrFacts = verbatimObservation.ObservationMeasurementOrFacts;
            //obs.EventMeasurementOrFacts = verbatimObservation.EventMeasurementOrFacts;
            //obs.ObservationExtendedMeasurementOrFacts = verbatimObservation.ObservationExtendedMeasurementOrFacts;
            //obs.EventExtendedMeasurementOrFacts = verbatimObservation.EventExtendedMeasurementOrFacts;

            // Record level
            obs.AccessRights = GetSosId(verbatimObservation.AccessRights, _fieldMappings[FieldMappingFieldId.AccessRights]);
            obs.BasisOfRecord = GetSosId(verbatimObservation.BasisOfRecord, _fieldMappings[FieldMappingFieldId.BasisOfRecord]);
            obs.BibliographicCitation = verbatimObservation.BibliographicCitation;
            obs.CollectionCode = verbatimObservation.CollectionCode;
            obs.CollectionId = verbatimObservation.CollectionID;
            obs.DataGeneralizations = verbatimObservation.DataGeneralizations;
            obs.DatasetId = verbatimObservation.DatasetID;
            obs.DatasetName = verbatimObservation.DatasetName;
            obs.DynamicProperties = verbatimObservation.DynamicProperties;
            obs.InformationWithheld = verbatimObservation.InformationWithheld;
            obs.InstitutionCode = verbatimObservation.InstitutionCode;
            obs.InstitutionId = ProcessedFieldMapValue.Create(verbatimObservation.InstitutionID); // todo - Create DarwinCore field mapping.
            obs.Language = verbatimObservation.Language;
            obs.License = verbatimObservation.License;
            obs.Modified = DwcParser.ParseDate(verbatimObservation.Modified);
            obs.OwnerInstitutionCode = verbatimObservation.OwnerInstitutionCode;
            obs.References = verbatimObservation.References;
            obs.RightsHolder = verbatimObservation.RightsHolder;
            obs.Type = ProcessedFieldMapValue.Create(verbatimObservation.Type); // todo - Create DarwinCore field mapping.
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

        private ProcessedOrganism CreateProcessedOrganism(DwcObservationVerbatim verbatimObservation)
        {
            var processedOrganism = new ProcessedOrganism();
            processedOrganism.OrganismId = verbatimObservation.OrganismID;
            processedOrganism.OrganismName = verbatimObservation.OrganismName;
            processedOrganism.OrganismScope = verbatimObservation.OrganismScope;
            processedOrganism.AssociatedOccurrences = verbatimObservation.AssociatedOccurrences;
            processedOrganism.AssociatedOrganisms = verbatimObservation.AssociatedOrganisms;
            processedOrganism.PreviousIdentifications = verbatimObservation.PreviousIdentifications;
            processedOrganism.OrganismRemarks = verbatimObservation.OrganismRemarks;

            return processedOrganism;
        }

        private ProcessedGeologicalContext CreateProcessedGeologicalContext(DwcObservationVerbatim verbatimObservation)
        {
            var processedGeologicalContext = new ProcessedGeologicalContext();
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

        private ProcessedMaterialSample CreateProcessedMaterialSample(DwcObservationVerbatim verbatimObservation)
        {
            var processedMaterialSample = new ProcessedMaterialSample();
            processedMaterialSample.MaterialSampleId = verbatimObservation.MaterialSampleID;
            return processedMaterialSample;
        }

        private ProcessedEvent CreateProcessedEvent(DwcObservationVerbatim verbatimObservation)
        {
            var processedEvent = new ProcessedEvent();
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
                out DateTime? startDate,
                out DateTime? endDate);

            processedEvent.StartDate = startDate;
            processedEvent.EndDate = endDate;

            // todo - Can we map the following fields?
            // processedEvent.Biotope = GetSosId(verbatimObservation?.Bioptope?.Id, _fieldMappings[FieldMappingFieldId.Biotope]);
            // processedEvent.Substrate = GetSosId(verbatimObservation?.Bioptope?.Id, _fieldMappings[FieldMappingFieldId.Substrate]);

            return processedEvent;
        }

        private ProcessedIdentification CreateProcessedIdentification(DwcObservationVerbatim verbatimObservation)
        {
            var processedIdentification = new ProcessedIdentification();
            processedIdentification.DateIdentified = verbatimObservation.DateIdentified?.ParseDateTime();
            processedIdentification.IdentificationId = verbatimObservation.IdentificationID;
            processedIdentification.IdentificationQualifier = verbatimObservation.IdentificationQualifier;
            processedIdentification.IdentificationReferences = verbatimObservation.IdentificationReferences;
            processedIdentification.IdentificationRemarks = verbatimObservation.IdentificationRemarks;
            //processedIdentification.ValidationStatusId = GetSosId(verbatimObservation.IdentificationVerificationStatus, _fieldMappings[FieldMappingFieldId.ValidationStatus]); // todo - Create DarwinCore field mapping.
            //processedIdentification.Validated = ? // todo -
            //processedIdentification.UncertainDetermination = ? // todo - 
            processedIdentification.IdentifiedBy = verbatimObservation.IdentifiedBy;
            processedIdentification.TypeStatus = verbatimObservation.TypeStatus;
            return processedIdentification;
        }

        private ProcessedLocation CreateProcessedLocation(DwcObservationVerbatim verbatimObservation)
        {
            var processedLocation = new ProcessedLocation();
            processedLocation.Continent = GetSosId(
                verbatimObservation.Continent, 
                _fieldMappings[FieldMappingFieldId.Continent],
                defaultValue: (int)ContinentId.Europe, 
                mappingNotFoundLogic: MappingNotFoundLogic.UseDefaultValue);
            processedLocation.CoordinatePrecision = verbatimObservation.CoordinatePrecision.ParseDouble();
            processedLocation.CoordinateUncertaintyInMeters = verbatimObservation.CoordinateUncertaintyInMeters?.ParseInt();
            processedLocation.Country = GetSosId(
                verbatimObservation.Country, 
                _fieldMappings[FieldMappingFieldId.Country], 
                defaultValue: (int)CountryId.Sweden,
                mappingNotFoundLogic: MappingNotFoundLogic.UseDefaultValue);
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
            processedLocation.MaximumDistanceAboveSurfaceInMeters = verbatimObservation.MaximumDistanceAboveSurfaceInMeters.ParseDouble();
            processedLocation.MaximumElevationInMeters = verbatimObservation.MaximumElevationInMeters.ParseDouble();
            processedLocation.MinimumDepthInMeters = verbatimObservation.MinimumDepthInMeters.ParseDouble();
            processedLocation.MinimumDistanceAboveSurfaceInMeters = verbatimObservation.MinimumDistanceAboveSurfaceInMeters.ParseDouble();
            processedLocation.MinimumElevationInMeters = verbatimObservation.MinimumElevationInMeters.ParseDouble();
            //processedLocation.Municipality = GetSosId(verbatimObservation.Municipality, _fieldMappings[FieldMappingFieldId.Municipality]); // Mapped by AreaHelper
            processedLocation.VerbatimMunicipality = verbatimObservation.Municipality;
            //processedLocation.Province = GetSosId(verbatimObservation.StateProvince, _fieldMappings[FieldMappingFieldId.Province]); // Mapped by AreaHelper
            processedLocation.VerbatimProvince = verbatimObservation.StateProvince;
            processedLocation.VerbatimCoordinates = verbatimObservation.VerbatimCoordinates;
            processedLocation.VerbatimCoordinateSystem = verbatimObservation.VerbatimCoordinateSystem;
            processedLocation.VerbatimDepth = verbatimObservation.VerbatimDepth;
            processedLocation.VerbatimElevation = verbatimObservation.VerbatimElevation;
            processedLocation.VerbatimLatitude = verbatimObservation.VerbatimLatitude.ParseDouble();
            processedLocation.VerbatimLocality = verbatimObservation.VerbatimLocality;
            processedLocation.VerbatimLongitude = verbatimObservation.VerbatimLongitude.ParseDouble();
            processedLocation.VerbatimSRS = verbatimObservation.VerbatimSRS;
            processedLocation.WaterBody = verbatimObservation.WaterBody;

            // todo - do we want to save verbatim coordinates in this way?
            if (!processedLocation.VerbatimLatitude.HasValue &&
                !processedLocation.VerbatimLongitude.HasValue &&
                string.IsNullOrWhiteSpace(processedLocation.VerbatimSRS))
            {
                processedLocation.VerbatimLatitude = processedLocation.DecimalLatitude;
                processedLocation.VerbatimLongitude = processedLocation.DecimalLongitude;
                processedLocation.VerbatimSRS = processedLocation.GeodeticDatum;
            }

            // todo - handle conversion of coordinates from different coordinate systems (GeodeticDatum).
            //Point = (PointGeoShape)wgs84Point?.ToGeoShape(),
            //PointLocation = wgs84Point?.ToGeoLocation(),
            //PointWithBuffer = (PolygonGeoShape)wgs84Point?.ToCircle(verbatim.CoordinateUncertaintyInMeters)?.ToGeoShape(),
            return processedLocation;
        }

        private double? ParseDouble(string strValue, string fieldName)
        {
            double? result = strValue.ParseDouble();
            if (!result.HasValue && strValue.HasValue())
            {
                var errorText = $"The field {fieldName} with a value of [{strValue}] is not a valid {typeof(double)}";
                errors.Add(errorText);
            }

            return result;
        }

        private List<string> errors = new List<string>();
        private ProcessedOccurrence CreateProcessedOccurrence(DwcObservationVerbatim verbatimObservation)
        {
            var processedOccurrence = new ProcessedOccurrence();
            processedOccurrence.AssociatedMedia = verbatimObservation.AssociatedMedia;
            processedOccurrence.AssociatedReferences = verbatimObservation.AssociatedReferences;
            processedOccurrence.AssociatedSequences = verbatimObservation.AssociatedSequences;
            processedOccurrence.AssociatedTaxa = verbatimObservation.AssociatedTaxa;
            processedOccurrence.Behavior = verbatimObservation.Behavior;
            processedOccurrence.CatalogNumber = verbatimObservation.CatalogNumber;
            processedOccurrence.Disposition = verbatimObservation.Disposition;
            processedOccurrence.EstablishmentMeans = GetSosId(verbatimObservation.EstablishmentMeans, _fieldMappings[FieldMappingFieldId.EstablishmentMeans]);
            processedOccurrence.IndividualCount = verbatimObservation.IndividualCount; 
            processedOccurrence.LifeStage = ProcessedFieldMapValue.Create(verbatimObservation.LifeStage); // todo - create DarwinCore field mapping for FieldMappingFieldId.LifeStage.
            processedOccurrence.OccurrenceId = verbatimObservation.OccurrenceID;
            processedOccurrence.OccurrenceRemarks = verbatimObservation.OccurrenceRemarks;
            processedOccurrence.OccurrenceStatus = GetSosId(verbatimObservation.OccurrenceStatus, _fieldMappings[FieldMappingFieldId.OccurrenceStatus]);
            processedOccurrence.OrganismQuantity = verbatimObservation.OrganismQuantity;
            processedOccurrence.OrganismQuantityUnit = ProcessedFieldMapValue.Create(verbatimObservation.OrganismQuantityType); // todo - create DarwinCore field mapping for FieldMappingFieldId.OrganismQuantityUnit.
            processedOccurrence.OtherCatalogNumbers = verbatimObservation.OtherCatalogNumbers;
            processedOccurrence.Preparations = verbatimObservation.Preparations;
            processedOccurrence.RecordedBy = verbatimObservation.RecordedBy;
            processedOccurrence.RecordNumber = verbatimObservation.RecordNumber;
            processedOccurrence.Activity = ProcessedFieldMapValue.Create(verbatimObservation.ReproductiveCondition); // todo - create DarwinCore field mapping for FieldMappingFieldId.Activity.
            processedOccurrence.Gender = GetSosId(verbatimObservation.Sex, _fieldMappings[FieldMappingFieldId.Gender]);

            // todo - handle the following fields:
            // processedOccurrence.BirdNestActivityId = GetBirdNestActivityId(verbatimObservation, taxon),
            // processedOccurrence.IsNaturalOccurrence = !verbatimObservation.Unspontaneous,
            // processedOccurrence.IsNeverFoundObservation = verbatimObservation.NotPresent,
            // processedOccurrence.IsNotRediscoveredObservation = verbatimObservation.NotRecovered,
            // processedOccurrence.IsPositiveObservation = !(verbatimObservation.NotPresent || verbatimObservation.NotRecovered),
            // processedOccurrence.URL = $"http://www.artportalen.se/sighting/{verbatimObservation.Id}"

            return processedOccurrence;
        }

        private ProcessedTaxon CreateProcessedTaxon(DwcObservationVerbatim verbatimObservation)
        {
            ProcessedTaxon processedTaxon = new ProcessedTaxon();
            processedTaxon.AcceptedNameUsage = verbatimObservation.AcceptedNameUsage;
            processedTaxon.AcceptedNameUsageID = verbatimObservation.AcceptedNameUsageID;
            processedTaxon.Class = verbatimObservation.Class;
            processedTaxon.Family = verbatimObservation.Family;
            processedTaxon.Genus = verbatimObservation.Genus;
            processedTaxon.HigherClassification = verbatimObservation.HigherClassification;
            processedTaxon.InfraspecificEpithet = verbatimObservation.InfraspecificEpithet;
            processedTaxon.Kingdom = verbatimObservation.Kingdom;
            processedTaxon.NameAccordingTo = verbatimObservation.NameAccordingTo;
            processedTaxon.NameAccordingToID = verbatimObservation.NameAccordingToID;
            processedTaxon.NamePublishedIn = verbatimObservation.NamePublishedIn;
            processedTaxon.NamePublishedInId = verbatimObservation.NamePublishedInID;
            processedTaxon.NamePublishedInYear = verbatimObservation.NamePublishedInYear;
            processedTaxon.NomenclaturalCode = verbatimObservation.NomenclaturalCode;
            processedTaxon.NomenclaturalStatus = verbatimObservation.NomenclaturalStatus;
            processedTaxon.Order = verbatimObservation.Order;
            processedTaxon.OriginalNameUsage = verbatimObservation.OriginalNameUsage;
            processedTaxon.OriginalNameUsageId = verbatimObservation.OriginalNameUsageID;
            processedTaxon.ParentNameUsage = verbatimObservation.ParentNameUsage;
            processedTaxon.ParentNameUsageId = verbatimObservation.ParentNameUsageID;
            processedTaxon.Phylum = verbatimObservation.Phylum;
            processedTaxon.ScientificName = verbatimObservation.ScientificName;
            processedTaxon.ScientificNameAuthorship = verbatimObservation.ScientificNameAuthorship;
            processedTaxon.ScientificNameId = verbatimObservation.ScientificNameID;
            processedTaxon.SpecificEpithet = verbatimObservation.SpecificEpithet;
            processedTaxon.Subgenus = verbatimObservation.Subgenus;
            processedTaxon.TaxonConceptId = verbatimObservation.TaxonConceptID;
            processedTaxon.TaxonId = verbatimObservation.TaxonID;
            processedTaxon.TaxonomicStatus = verbatimObservation.TaxonomicStatus;
            processedTaxon.TaxonRank = verbatimObservation.TaxonRank;
            processedTaxon.TaxonRemarks = verbatimObservation.TaxonRemarks;
            processedTaxon.VerbatimTaxonRank = verbatimObservation.VerbatimTaxonRank;
            processedTaxon.VernacularName = verbatimObservation.VernacularName;

            return processedTaxon;
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

        private ProcessedFieldMapValue GetSosId(string val,
            IDictionary<object, int> sosIdByValue,
            int? defaultValue = null,
            MappingNotFoundLogic mappingNotFoundLogic = MappingNotFoundLogic.UseSourceValue)
        {
            if (string.IsNullOrWhiteSpace(val) || sosIdByValue == null)
            {
                return defaultValue.HasValue ? new ProcessedFieldMapValue { Id = defaultValue.Value } : null;
            }

            string lookupVal = val.ToLower();
            if (sosIdByValue.TryGetValue(lookupVal, out var sosId))
            {
                return new ProcessedFieldMapValue { Id = sosId };
            }

            if (mappingNotFoundLogic == MappingNotFoundLogic.UseDefaultValue && defaultValue.HasValue)
            {
                return new ProcessedFieldMapValue { Id = defaultValue.Value };
            }

            return new ProcessedFieldMapValue { Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId, Value = val };
        }

        

        /// <summary>
        /// Get SOS internal Id for the id specific for the data provider.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sosIdByValue"></param>
        /// <returns></returns>
        private static ProcessedFieldMapValue GetSosId(int? val, IDictionary<object, int> sosIdByValue)
        {
            if (!val.HasValue || sosIdByValue == null) return null;

            if (sosIdByValue.TryGetValue(val.Value, out var sosId))
            {
                return new ProcessedFieldMapValue { Id = sosId };
            }

            return new ProcessedFieldMapValue { Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId, Value = val.ToString() };

        }

        private ProcessedProject CreateProcessedProject(Project project)
        {
            if (project == null) return null;

            return new ProcessedProject
            {
                IsPublic = project.IsPublic,
                Category = project.Category,
                Description = project.Description,
                EndDate = project.EndDate,
                Id = project.Id.ToString(),
                Name = project.Name,
                Owner = project.Owner,
                StartDate = project.StartDate,
                SurveyMethod = project.SurveyMethod,
                SurveyMethodUrl = project.SurveyMethodUrl,
                ProjectParameters = project.ProjectParameters?.Select(CreateProcessedProjectParameter)
            };
        }

        private ProcessedProjectParameter CreateProcessedProjectParameter(ProjectParameter projectParameter)
        {
            if (projectParameter == null)
            {
                return null;
            }

            return new ProcessedProjectParameter
            {
                Value = projectParameter.Value,
                DataType = projectParameter.DataType,
                Description = projectParameter.Description,
                Name = projectParameter.Name,
                Id = projectParameter.Id,
                Unit = projectParameter.Unit
            };
        }

        private string GetSamplingProtocol(IEnumerable<Project> projects)
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
        /// Calculate protection level
        /// </summary>
        /// <param name="taxon"></param>
        /// <param name="hiddenByProvider"></param>
        /// <param name="protectedBySystem"></param>
        /// <returns></returns>
        private int CalculateProtectionLevel(ProcessedTaxon taxon, DateTime? hiddenByProvider, bool protectedBySystem)
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
                if ((protectionLevel > 3 && hiddenByProvider.HasValue && hiddenByProvider.Value >= DateTime.Now) || protectedBySystem)
                {
                    return protectionLevel;
                }
            }

            return 1;
        }

        /// <summary>
        /// Build the substrate description string
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        private string GetSubstrateDescription(ArtportalenVerbatimObservation verbatimObservation, IDictionary<int, ProcessedTaxon> taxa)
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
                substrateDescription.Append($"{(substrateDescription.Length == 0 ? "" : " # ")}{verbatimObservation.Substrate.Translate(Cultures.en_GB)}");
            }

            if (!string.IsNullOrEmpty(verbatimObservation.SubstrateDescription))
            {
                substrateDescription.Append($"{(substrateDescription.Length == 0 ? "" : " # ")}{verbatimObservation.SubstrateDescription}");
            }

            if (verbatimObservation.SubstrateSpeciesId.HasValue &&
                taxa != null &&
                taxa.TryGetValue(verbatimObservation.SubstrateSpeciesId.Value, out var taxon))
            {
                substrateDescription.Append($"{(substrateDescription.Length == 0 ? "" : " # ")}{taxon.ScientificName}");
            }

            if (!string.IsNullOrEmpty(verbatimObservation.SubstrateSpeciesDescription))
            {
                substrateDescription.Append($"{(substrateDescription.Length == 0 ? "" : " # ")}{verbatimObservation.SubstrateSpeciesDescription}");
            }

            var res = substrateDescription.Length > 0 ? substrateDescription.ToString().WithMaxLength(255) : null;
            return res;
        }

        /// <summary>
        /// Get bird nest activity id
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <param name="taxon"></param>
        /// <returns></returns>
        public int? GetBirdNestActivityId(ArtportalenVerbatimObservation verbatimObservation, ProcessedTaxon taxon)
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
        /// Get associated references
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <returns></returns>
        private string GetAssociatedReferences(ArtportalenVerbatimObservation verbatimObservation)
        {
            if (!verbatimObservation?.MigrateSightingObsId.HasValue ?? true)
            {
                return null;
            }

            string associatedReferences = null;
            switch (verbatimObservation.MigrateSightingPortalId ?? 0)
            {
                case 1:
                    associatedReferences = $"urn:lsid:artportalen.se:Sighting:Bird.{verbatimObservation.MigrateSightingObsId.Value}";
                    break;
                case 2:
                    associatedReferences = $"urn:lsid:artportalen.se:Sighting:PlantAndMushroom.{verbatimObservation.MigrateSightingObsId.Value}";
                    break;
                case 6:
                    associatedReferences = $"urn:lsid:artportalen.se:Sighting:Vertebrate.{verbatimObservation.MigrateSightingObsId.Value}";
                    break;
                case 7:
                    associatedReferences = $"urn:lsid:artportalen.se:Sighting:Bugs.{verbatimObservation.MigrateSightingObsId.Value}";
                    break;
                case 8:
                    associatedReferences = $"urn:lsid:artportalen.se:Sighting:Fish.{verbatimObservation.MigrateSightingObsId.Value}";
                    break;
                case 9:
                    associatedReferences = $"urn:lsid:artportalen.se:Sighting:MarineInvertebrates.{verbatimObservation.MigrateSightingObsId.Value}";
                    break;
            }

            return associatedReferences;
        }

        /// <summary>
        /// Get field mappings for Artportalen.
        /// </summary>
        /// <param name="externalSystemId"></param>
        /// <param name="allFieldMappings"></param>
        /// <param name="convertValuesToLowercase"></param>
        /// <returns></returns>
        private static IDictionary<FieldMappingFieldId, IDictionary<object, int>> GetFieldMappingsDictionary(
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
    }
}
