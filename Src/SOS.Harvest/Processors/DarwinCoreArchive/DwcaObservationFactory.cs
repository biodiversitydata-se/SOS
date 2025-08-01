﻿using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Text.RegularExpressions;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Harvest.Processors.DarwinCoreArchive
{
    /// <summary>
    ///     DwC-A observation factory.
    /// </summary>
    public class DwcaObservationFactory : ObservationFactoryBase, IObservationFactory<DwcObservationVerbatim>
    {
        private int DefaultCoordinateUncertaintyInMeters = 5000;
        private readonly DataProvider _dataProvider;
        private readonly IAreaHelper _areaHelper;
        private readonly NetTopologySuite.IO.WKTReader _wktReader = new NetTopologySuite.IO.WKTReader();

        private string _englishDataproviderName;
        private string _englishOrganizationName;
        private string _englishOrganizationNameLowerCase;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="dwcaVocabularyById"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processTimeManager"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DwcaObservationFactory(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon>? taxa,
            IDictionary<VocabularyId, IDictionary<object, int>> dwcaVocabularyById,            
            IAreaHelper areaHelper,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration) : base(dataProvider, taxa, dwcaVocabularyById, processTimeManager, processConfiguration)
        {
            _dataProvider = dataProvider;
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _englishDataproviderName = dataProvider?.Names?.Translate("en-GB")!;
            _englishOrganizationName = dataProvider?.Organizations?.Translate("en-GB")!;
            _englishOrganizationNameLowerCase = _englishOrganizationName?.ToLower()!;
            if (dataProvider != null && dataProvider.CoordinateUncertaintyInMeters > 0)
            {
                DefaultCoordinateUncertaintyInMeters = dataProvider.CoordinateUncertaintyInMeters;
            }
        }

        public static async Task<DwcaObservationFactory> CreateAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IDictionary<VocabularyId, IDictionary<object, int>> dwcaVocabularyById,
            IAreaHelper areaHelper,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration)
        {            
            return new DwcaObservationFactory(dataProvider, taxa, dwcaVocabularyById, areaHelper, processTimeManager, processConfiguration);
        }

        /// <summary>
        ///  Cast verbatim observations to processed data model
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="diffuseIfSupported"></param>
        /// <returns></returns>
        public Observation? CreateProcessedObservation(DwcObservationVerbatim verbatim, bool diffuseIfSupported)
        {
            if (verbatim == null)
            {
                return null;
            }

            var accessRights = GetSosId(verbatim.AccessRights, VocabularyById[VocabularyId.AccessRights]);
            var obs = new Observation
            {
                MongoDbId = verbatim.Id,
                AccessRights = accessRights,
                DataProviderId = DataProvider.Id,
                DiffusionStatus = DiffusionStatus.NotDiffused,
            };

            //AddVerbatimObservationAsJson(obs, verbatim); // todo - this could be used to store the original verbatim observation

            // Record level
            if (verbatim.ObservationMeasurementOrFacts.HasItems())
                obs.MeasurementOrFacts = verbatim.ObservationMeasurementOrFacts?.Select(dwcMof => dwcMof.ToProcessedExtendedMeasurementOrFact()).ToArray();
            else if (verbatim.ObservationExtendedMeasurementOrFacts.HasItems())
                obs.MeasurementOrFacts = verbatim.ObservationExtendedMeasurementOrFacts?.Select(dwcMof => dwcMof.ToProcessedExtendedMeasurementOrFact()).ToArray();
            obs.BasisOfRecord = GetSosId(verbatim.BasisOfRecord,
                VocabularyById[VocabularyId.BasisOfRecord]);
            obs.BibliographicCitation = verbatim.BibliographicCitation;
            obs.CollectionCode = verbatim.CollectionCode;
            obs.CollectionId = verbatim.CollectionID;
            obs.DataGeneralizations = verbatim.DataGeneralizations;
            obs.DatasetId = verbatim.DatasetID;
            obs.DatasetName = string.IsNullOrWhiteSpace(verbatim.DatasetName) ? _englishDataproviderName : verbatim.DatasetName;
            obs.DynamicProperties = verbatim.DynamicProperties;
            obs.InformationWithheld = verbatim.InformationWithheld;
            obs.InstitutionId = verbatim.InstitutionID;
            if (!string.IsNullOrEmpty(verbatim.InstitutionCode))
            {
                obs.InstitutionCode = GetSosId(verbatim.InstitutionCode,
                    VocabularyById[VocabularyId.Institution]);
            }
            else
            {
                obs.InstitutionCode = GetSosId(_englishOrganizationName,
                    VocabularyById[VocabularyId.Institution],
                    null,
                    MappingNotFoundLogic.UseSourceValue,
                    _englishOrganizationNameLowerCase);
            }
            obs.Language = verbatim.Language;
            obs.License = verbatim.License;
            obs.Modified = DwcParser.ParseDate(verbatim.Modified)?.ToUniversalTime();
            obs.OwnerInstitutionCode = verbatim.OwnerInstitutionCode;
            obs.References = verbatim.References;
            obs.RightsHolder = verbatim.RightsHolder?.Clean();
            obs.Type = GetSosId(verbatim.Type, VocabularyById[VocabularyId.Type]);
            if (!string.IsNullOrEmpty(verbatim.DataStewardshipDatasetId))
            {
                obs.DataStewardship = new Lib.Models.Processed.DataStewardship.Common.DataStewardshipInfo
                {
                    DatasetIdentifier = verbatim.DataStewardshipDatasetId,
                    DatasetTitle = verbatim.DataStewardshipDatasetTitle
                };
            }

            // Event
            obs.Event = CreateProcessedEvent(verbatim);

            // Geological
            obs.GeologicalContext = CreateProcessedGeologicalContext(verbatim);

            // Identification
            obs.Identification = CreateProcessedIdentification(verbatim);

            // Taxon
            obs.Taxon = CreateProcessedTaxon(verbatim);

            // Location
            obs.Location = CreateProcessedLocation(verbatim);
            if (!GISExtensions.TryParseCoordinateSystem(string.IsNullOrEmpty(verbatim.GeodeticDatum) ? verbatim.VerbatimSRS : verbatim.GeodeticDatum, out var coordinateSystem))
            {
                coordinateSystem = CoordinateSys.WGS84;
            }

            var coordinateUncertaintyInMeters = verbatim.CoordinateUncertaintyInMeters?.ParseDoubleConvertToInt() ?? DefaultCoordinateUncertaintyInMeters;
            if (TryGetGeometryFromWkt(obs.Location.FootprintWKT, out NetTopologySuite.Geometries.Geometry? wktGeometry))
            {
                if (wktGeometry.GeometryType == "MultiPolygon")
                {
                    wktGeometry = wktGeometry.ConvexHull();
                }
                else if (wktGeometry.GeometryType != "Polygon")
                {
                    var sweref99TmGeom = wktGeometry.Transform(coordinateSystem, CoordinateSys.SWEREF99_TM, true);
                    NetTopologySuite.Operation.Buffer.EndCapStyle endCapStyle = sweref99TmGeom.Coordinates.Length == 2 ? NetTopologySuite.Operation.Buffer.EndCapStyle.Flat : NetTopologySuite.Operation.Buffer.EndCapStyle.Square;
                    sweref99TmGeom = sweref99TmGeom.Buffer(coordinateUncertaintyInMeters, endCapStyle);
                    wktGeometry = sweref99TmGeom.Transform(CoordinateSys.SWEREF99_TM, coordinateSystem, true);
                    if (wktGeometry.GeometryType != "Polygon")
                    {
                        wktGeometry = wktGeometry.ConvexHull();
                    }
                }

                double? decimalLongitude = verbatim.DecimalLongitude.ParseDouble() ?? verbatim.VerbatimLongitude.ParseDouble() ?? wktGeometry.Centroid.X;
                double? decimalLatitude = verbatim.DecimalLatitude.ParseDouble() ?? verbatim.VerbatimLatitude.ParseDouble() ?? wktGeometry.Centroid.Y;
   
                AddPositionData(
                    obs.Location,
                    decimalLongitude,
                    decimalLatitude,
                    coordinateSystem,
                    wktGeometry!.Centroid,
                    wktGeometry,
                    coordinateUncertaintyInMeters,
                    obs.Taxon?.Attributes?.DisturbanceRadius
                );
            }
            else
            {
                AddPositionData(
                    obs.Location,
                    verbatim.DecimalLongitude.ParseDouble() ?? verbatim.VerbatimLongitude.ParseDouble(),
                    verbatim.DecimalLatitude.ParseDouble() ?? verbatim.VerbatimLatitude.ParseDouble(),
                    coordinateSystem,
                    coordinateUncertaintyInMeters,
                    obs.Taxon?.Attributes?.DisturbanceRadius);
            }

            // MaterialSample
            obs.MaterialSample = CreateProcessedMaterialSample(verbatim);

            // Occurrence
            obs.Occurrence = CreateProcessedOccurrence(verbatim, obs.Taxon, obs.AccessRights != null ? (AccessRightsId)obs.AccessRights.Id : null);

            // Organism
            obs.Organism = CreateProcessedOrganism(verbatim);

            // Temporarily remove
            //obs.IsInEconomicZoneOfSweden = true;
            _areaHelper.AddAreaDataToProcessedLocation(obs.Location);

            if (obs.ShallBeProtected())
            {
                obs.Sensitive = true;

                if (obs.AccessRights?.Id == (int)AccessRightsId.NotForPublicUsage && obs.Occurrence.SensitivityCategory < 3 && (obs.Taxon?.Attributes?.SensitivityCategory?.Id ?? 0) < 3) {
                    obs.Occurrence.SensitivityCategory = 3;
                }
            }

            // Populate generic data
            PopulateGenericData(obs);

            obs.Occurrence.BirdNestActivityId = GetBirdNestActivityId(obs.Occurrence.Activity, obs.Taxon);
            CalculateOrganismQuantity(obs);
            return obs;
        }

        private bool TryGetGeometryFromWkt(string wkt, out NetTopologySuite.Geometries.Geometry? geometry)
        {
            geometry = null;
            if (string.IsNullOrEmpty(wkt)) return false;            
            
            try
            {                
                geometry = _wktReader.Read(wkt);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static void AddVerbatimObservationAsJson(Observation obs,
            DwcObservationVerbatim verbatim)
        {
            //obs.VerbatimObservation = JsonConvert.SerializeObject(
            //    verbatim,
            //    Formatting.Indented,
            //    new JsonSerializerSettings()
            //    {
            //        NullValueHandling = NullValueHandling.Ignore
            //    });
        }

        private ICollection<Multimedia>? CreateProcessedMultimedia(
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

        private Organism CreateProcessedOrganism(DwcObservationVerbatim verbatim)
        {
            var processedOrganism = new Organism();
            processedOrganism.OrganismId = verbatim.OrganismID;
            processedOrganism.OrganismName = verbatim.OrganismName;
            processedOrganism.OrganismScope = verbatim.OrganismScope;
            processedOrganism.AssociatedOrganisms = verbatim.AssociatedOrganisms;
            processedOrganism.PreviousIdentifications = verbatim.PreviousIdentifications;
            processedOrganism.OrganismRemarks = verbatim.OrganismRemarks;

            return processedOrganism;
        }

        private GeologicalContext CreateProcessedGeologicalContext(DwcObservationVerbatim verbatim)
        {
            var processedGeologicalContext = new GeologicalContext();
            processedGeologicalContext.Bed = verbatim.Bed;
            processedGeologicalContext.EarliestAgeOrLowestStage = verbatim.EarliestAgeOrLowestStage;
            processedGeologicalContext.EarliestEonOrLowestEonothem = verbatim.EarliestEonOrLowestEonothem;
            processedGeologicalContext.EarliestEpochOrLowestSeries = verbatim.EarliestEpochOrLowestSeries;
            processedGeologicalContext.EarliestEraOrLowestErathem = verbatim.EarliestEraOrLowestErathem;
            processedGeologicalContext.EarliestGeochronologicalEra = verbatim.EarliestGeochronologicalEra;
            processedGeologicalContext.EarliestPeriodOrLowestSystem = verbatim.EarliestPeriodOrLowestSystem;
            processedGeologicalContext.Formation = verbatim.Formation;
            processedGeologicalContext.GeologicalContextId = verbatim.GeologicalContextID;
            processedGeologicalContext.Group = verbatim.Group;
            processedGeologicalContext.HighestBiostratigraphicZone = verbatim.HighestBiostratigraphicZone;
            processedGeologicalContext.LatestAgeOrHighestStage = verbatim.LatestAgeOrHighestStage;
            processedGeologicalContext.LatestEonOrHighestEonothem = verbatim.LatestEonOrHighestEonothem;
            processedGeologicalContext.LatestEpochOrHighestSeries = verbatim.LatestEpochOrHighestSeries;
            processedGeologicalContext.LatestEraOrHighestErathem = verbatim.LatestEraOrHighestErathem;
            processedGeologicalContext.LatestGeochronologicalEra = verbatim.LatestGeochronologicalEra;
            processedGeologicalContext.LatestPeriodOrHighestSystem = verbatim.LatestPeriodOrHighestSystem;
            processedGeologicalContext.LithostratigraphicTerms = verbatim.LithostratigraphicTerms;
            processedGeologicalContext.LowestBiostratigraphicZone = verbatim.LowestBiostratigraphicZone;
            processedGeologicalContext.Member = verbatim.Member;

            return processedGeologicalContext;
        }

        private MaterialSample CreateProcessedMaterialSample(DwcObservationVerbatim verbatim)
        {
            var processedMaterialSample = new MaterialSample();
            processedMaterialSample.MaterialSampleId = verbatim.MaterialSampleID;
            return processedMaterialSample;
        }

        private Event CreateProcessedEvent(DwcObservationVerbatim verbatim)
        {
            DwcParser.TryParseEventDate(
                verbatim.EventDate,
                verbatim.Year,
                verbatim.Month,
                verbatim.Day,
                verbatim.EventTime,
                out var startDate,
                out var endDate,
                out var startTime,
                out var endTime);

            var processedEvent = new Event(startDate, startTime, endDate, endTime);
            processedEvent.EventId = verbatim.EventID;
            processedEvent.ParentEventId = verbatim.ParentEventID;
            processedEvent.EventRemarks = verbatim.EventRemarks;
            processedEvent.FieldNotes = verbatim.FieldNotes;
            processedEvent.FieldNumber = verbatim.FieldNumber;
            processedEvent.Habitat = verbatim.Habitat;
            processedEvent.SampleSizeUnit = verbatim.SampleSizeUnit;
            processedEvent.SampleSizeValue = verbatim.SampleSizeValue;
            processedEvent.SamplingEffort = verbatim.SamplingEffort;
            processedEvent.SamplingProtocol = verbatim.SamplingProtocol;
            processedEvent.VerbatimEventDate = verbatim.VerbatimEventDate?.Clean();

            processedEvent.Media = CreateProcessedMultimedia(
                verbatim.EventMultimedia,
                verbatim.EventAudubonMedia);
            if (verbatim.EventMeasurementOrFacts.HasItems())
                processedEvent.MeasurementOrFacts = verbatim.EventMeasurementOrFacts?.Select(dwcMof => dwcMof.ToProcessedExtendedMeasurementOrFact()).ToArray();
            else if (verbatim.EventExtendedMeasurementOrFacts.HasItems())
                processedEvent.MeasurementOrFacts = verbatim.EventExtendedMeasurementOrFacts?.Select(dwcMof => dwcMof.ToProcessedExtendedMeasurementOrFact()).ToArray();

            return processedEvent;
        }

        private Identification CreateProcessedIdentification(DwcObservationVerbatim verbatim)
        {
            string dateIdentifiedString = null!;
            if (DateTime.TryParse(verbatim.DateIdentified, out var dateIdentified))
            {
                dateIdentifiedString = dateIdentified.ToUniversalTime().ToString();
            }

            var processedIdentification = new Identification();
            processedIdentification.DateIdentified = dateIdentifiedString;
            processedIdentification.IdentificationId = verbatim.IdentificationID;
            processedIdentification.IdentificationQualifier = verbatim.IdentificationQualifier;
            processedIdentification.IdentificationReferences = verbatim.IdentificationReferences;
            processedIdentification.IdentificationRemarks = verbatim.IdentificationRemarks?.Clean();
            processedIdentification.VerificationStatus = GetSosId(verbatim.IdentificationVerificationStatus, VocabularyById[VocabularyId.VerificationStatus]);
            if (processedIdentification.VerificationStatus == null && _dataProvider.DefaultVerificationStatus != null)
            {
                processedIdentification.VerificationStatus = VocabularyValue.Create((int)_dataProvider.DefaultVerificationStatus);
            }
            processedIdentification.Verified = GetIsValidated(processedIdentification.VerificationStatus);
            processedIdentification.IdentifiedBy = verbatim.IdentifiedBy?.Clean();
            processedIdentification.TypeStatus = verbatim.TypeStatus;
            return processedIdentification;
        }

        private bool GetIsValidated(VocabularyValue? validationStatus)
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

        private Location CreateProcessedLocation(DwcObservationVerbatim verbatim)
        {


            var processedLocation = new Location(LocationType.Point);
            processedLocation.Continent = GetSosId(
                verbatim.Continent,
                VocabularyById[VocabularyId.Continent],
                (int)ContinentId.Europe,
                MappingNotFoundLogic.UseDefaultValue);
            processedLocation.CoordinatePrecision = verbatim.CoordinatePrecision.ParseDouble();
            processedLocation.CoordinateUncertaintyInMeters =
                verbatim.CoordinateUncertaintyInMeters?.ParseDoubleConvertToInt() ?? DefaultCoordinateUncertaintyInMeters;
            processedLocation.Country = GetSosId(
                verbatim.Country,
                VocabularyById[VocabularyId.Country],
                (int)CountryId.Sweden,
                MappingNotFoundLogic.UseDefaultValue);
            processedLocation.CountryCode = verbatim.CountryCode;
            processedLocation.FootprintSpatialFit = verbatim.FootprintSpatialFit;
            processedLocation.FootprintSRS = verbatim.FootprintSRS;
            processedLocation.FootprintWKT = verbatim.FootprintWKT;
            processedLocation.GeoreferencedBy = verbatim.GeoreferencedBy;
            processedLocation.GeoreferencedDate = verbatim.GeoreferencedDate;
            processedLocation.GeoreferenceProtocol = verbatim.GeoreferenceProtocol;
            processedLocation.GeoreferenceRemarks = verbatim.GeoreferenceRemarks;
            processedLocation.GeoreferenceSources = verbatim.GeoreferenceSources;
            processedLocation.GeoreferenceVerificationStatus = verbatim.GeoreferenceVerificationStatus;
            processedLocation.HigherGeography = verbatim.HigherGeography;
            processedLocation.HigherGeographyId = verbatim.HigherGeographyID;
            processedLocation.Island = verbatim.Island;
            processedLocation.IslandGroup = verbatim.IslandGroup;
            processedLocation.Locality = verbatim.Locality.Clean();
            processedLocation.LocationAccordingTo = verbatim.LocationAccordingTo;
            processedLocation.LocationId = verbatim.LocationID;
            processedLocation.LocationRemarks = verbatim.LocationRemarks;
            processedLocation.MaximumDepthInMeters = verbatim.MaximumDepthInMeters.ParseDouble();
            processedLocation.MaximumDistanceAboveSurfaceInMeters =
                verbatim.MaximumDistanceAboveSurfaceInMeters.ParseDouble();
            processedLocation.MaximumElevationInMeters = verbatim.MaximumElevationInMeters.ParseDouble();
            processedLocation.MinimumDepthInMeters = verbatim.MinimumDepthInMeters.ParseDouble();
            processedLocation.MinimumDistanceAboveSurfaceInMeters =
                verbatim.MinimumDistanceAboveSurfaceInMeters.ParseDouble();
            processedLocation.MinimumElevationInMeters = verbatim.MinimumElevationInMeters.ParseDouble();
            processedLocation.Attributes.VerbatimMunicipality = verbatim.Municipality;
            processedLocation.Attributes.VerbatimProvince = verbatim.StateProvince;
            processedLocation.VerbatimCoordinates = verbatim.VerbatimCoordinates;
            processedLocation.VerbatimCoordinateSystem = verbatim.VerbatimCoordinateSystem;
            processedLocation.VerbatimDepth = verbatim.VerbatimDepth;
            processedLocation.VerbatimElevation = verbatim.VerbatimElevation;
            processedLocation.VerbatimLocality = processedLocation.VerbatimLocality;
            processedLocation.WaterBody = verbatim.WaterBody;

            return processedLocation;
        }


        private Occurrence CreateProcessedOccurrence(DwcObservationVerbatim verbatim, Lib.Models.Processed.Observation.Taxon? taxon, AccessRightsId? accessRightsId)
        {
            var processedOccurrence = new Occurrence();
            processedOccurrence.AssociatedMedia = verbatim.AssociatedMedia;
            processedOccurrence.AssociatedReferences = verbatim.AssociatedReferences;
            processedOccurrence.AssociatedSequences = verbatim.AssociatedSequences;
            processedOccurrence.AssociatedTaxa = verbatim.AssociatedTaxa;
            processedOccurrence.CatalogNumber = verbatim.CatalogNumber ?? verbatim.OccurrenceID;
            processedOccurrence.Disposition = verbatim.Disposition;
            processedOccurrence.EstablishmentMeans = GetSosId(verbatim.EstablishmentMeans,
                VocabularyById[VocabularyId.EstablishmentMeans]);
            processedOccurrence.IndividualCount = verbatim.IndividualCount;
            processedOccurrence.LifeStage = GetSosId(verbatim.LifeStage, VocabularyById[VocabularyId.LifeStage]);
            processedOccurrence.Media = CreateProcessedMultimedia(
                verbatim.ObservationMultimedia,
                verbatim.ObservationAudubonMedia);
            processedOccurrence.OccurrenceId = verbatim.OccurrenceID;
            processedOccurrence.OccurrenceRemarks = verbatim.OccurrenceRemarks?.Clean();
            processedOccurrence.OccurrenceStatus = GetSosId(
                verbatim.OccurrenceStatus,
                VocabularyById[VocabularyId.OccurrenceStatus],
                (int)OccurrenceStatusId.Present);
            processedOccurrence.OrganismQuantity = verbatim.OrganismQuantity;
            processedOccurrence.OrganismQuantityUnit = GetSosId(verbatim.OrganismQuantityType, VocabularyById[VocabularyId.Unit]);
            processedOccurrence.OtherCatalogNumbers = verbatim.OtherCatalogNumbers;
            processedOccurrence.Preparations = verbatim.Preparations;
            processedOccurrence.RecordedBy = verbatim.RecordedBy?.Clean();
            processedOccurrence.RecordNumber = verbatim.RecordNumber;
            processedOccurrence.Activity = GetSosId(
                verbatim.ReproductiveCondition,
                VocabularyById[VocabularyId.Activity]);
            processedOccurrence.Sex = GetSosId(verbatim.Sex, VocabularyById[VocabularyId.Sex]);
            processedOccurrence.ReproductiveCondition = GetSosId(verbatim.ReproductiveCondition, VocabularyById!.GetValue(VocabularyId.ReproductiveCondition));
            processedOccurrence.Behavior = GetSosId(verbatim.Behavior, VocabularyById!.GetValue(VocabularyId.Behavior));
            processedOccurrence.IsNaturalOccurrence = true;
            processedOccurrence.IsNeverFoundObservation = false;
            processedOccurrence.IsNotRediscoveredObservation = false;
            processedOccurrence.IsPositiveObservation = true;
            if (processedOccurrence.OccurrenceStatus?.Id == (int)OccurrenceStatusId.Absent)
            {
                processedOccurrence.IsPositiveObservation = false;
                processedOccurrence.IsNeverFoundObservation = true;
            }

            //processedOccurrence.ProtectionLevel = CalculateProtectionLevel(taxon, accessRightsId);
            processedOccurrence.SensitivityCategory = CalculateProtectionLevel(taxon, accessRightsId);            
            return processedOccurrence;
        }

        private Lib.Models.Processed.Observation.Taxon CreateProcessedTaxon(DwcObservationVerbatim verbatim)
        {
            var parsedTaxonId = -1;

            // If dataprovider uses Dyntaxa Taxon Id, try parse TaxonId.
            if (!string.IsNullOrEmpty(verbatim.TaxonID))
            {
                // Biologg fix. They should use urn:lsid:dyntaxa prefix.
                if (DataProvider.Identifier == "Biologg")
                {
                    if (!int.TryParse(verbatim.TaxonID, out parsedTaxonId))
                    {
                        parsedTaxonId = -1;
                    }
                }

                if (verbatim.TaxonID.StartsWith("urn:lsid:dyntaxa"))
                {
                    string lastInteger = Regex.Match(verbatim.TaxonID, @"\d+", RegexOptions.RightToLeft).Value;
                    if (!int.TryParse(lastInteger, out parsedTaxonId))
                    {
                        parsedTaxonId = -1;
                    }
                }
            }
            var names = new HashSet<string> {
                verbatim.ScientificName
            };
            if (!string.IsNullOrEmpty(verbatim.Species))
            {
                names.Add(verbatim.Species);
            }            
            names.Add(RemoveAuthorFromString(verbatim.ScientificName!));            
            if (!string.IsNullOrEmpty(verbatim.Genus) && !string.IsNullOrEmpty(verbatim.SpecificEpithet))
            {
                names.Add($"{verbatim.Genus} {verbatim.SpecificEpithet}");
            }

            return GetTaxon(parsedTaxonId, names, verbatim.ScientificNameAuthorship, true, verbatim.TaxonID);
        }

        public static string RemoveAuthorFromString(string input)
        {
            if (input == null) return null;
            string withoutParentheses = Regex.Replace(input, @"\s*\([^)]*\)", "");
            string withoutCommaYear = @"\b([\wäöåÄÖÅ]+)( & [\wäöåÄÖÅ]+)?,\s*\d{4}\b";
            return Regex.Replace(withoutParentheses, withoutCommaYear, "").Trim();
        }           

        public void ValidateVerbatimData(DwcObservationVerbatim verbatim, DwcaValidationRemarksBuilder validationRemarksBuilder)
        {
            validationRemarksBuilder.NrValidatedObservations++;

            if (string.IsNullOrWhiteSpace(verbatim.CoordinateUncertaintyInMeters))
            {
                validationRemarksBuilder.NrMissingCoordinateUncertaintyInMeters++;
            }

            if (string.IsNullOrWhiteSpace(verbatim.IdentificationVerificationStatus))
            {
                validationRemarksBuilder.NrMissingIdentificationVerificationStatus++;
            }
        }

        public bool IsVerbatimObservationDiffusedByProvider(DwcObservationVerbatim verbatim)
        {
            return false;
        }
    }
}