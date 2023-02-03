using System.Text.RegularExpressions;
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
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;
using SOS.Lib.Configuration.Process;

namespace SOS.Harvest.Processors.DarwinCoreArchive
{
    /// <summary>
    ///     DwC-A observation factory.
    /// </summary>
    public class DwcaObservationFactory : ObservationFactoryBase, IObservationFactory<DwcObservationVerbatim>
    {
        private const int DefaultCoordinateUncertaintyInMeters = 5000;
        private readonly IAreaHelper _areaHelper;
        private readonly IDictionary<VocabularyId, IDictionary<object, int>> _vocabularyById;
        private readonly HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon> _taxonByScientificName;
        private readonly HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon> _taxonByScientificNameAuthor;
        private readonly HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon> _taxonBySynonymName;
        private readonly HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon> _taxonBySynonymNameAuthor;
        private string _englishDataproviderName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="vocabularyById"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processTimeManager"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DwcaObservationFactory(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IDictionary<VocabularyId, IDictionary<object, int>> vocabularyById,
            IAreaHelper areaHelper,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration) : base(dataProvider, taxa, processTimeManager, processConfiguration)
        {
            _vocabularyById = vocabularyById ?? throw new ArgumentNullException(nameof(vocabularyById));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _taxonByScientificName = new HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon>();
            _taxonByScientificNameAuthor = new HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon>();
            _taxonBySynonymName = new HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon>();
            _taxonBySynonymNameAuthor = new HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon>();
            foreach (var processedTaxon in taxa.Values)
            {
                _taxonByScientificName.Add(processedTaxon.ScientificName.ToLower(), processedTaxon);
                _taxonByScientificNameAuthor.Add(processedTaxon.ScientificName.ToLower() + " " + processedTaxon.ScientificNameAuthorship.ToLower(), processedTaxon);
                if (processedTaxon.Attributes?.Synonyms != null)
                {
                    foreach (var synonyme in processedTaxon.Attributes.Synonyms)
                    {
                        _taxonBySynonymName.Add(synonyme.Name.ToLower(), processedTaxon);
                        _taxonBySynonymNameAuthor.Add(synonyme.Name.ToLower() + " " + synonyme.Author.ToLower(), processedTaxon);
                    }
                }
            }

            _englishDataproviderName = dataProvider?.Names?.Translate("en-GB");
        }

        public static async Task<DwcaObservationFactory> CreateAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IVocabularyRepository processedVocabularyRepository,
            IAreaHelper areaHelper,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration)
        {
            var vocabularies = await processedVocabularyRepository.GetAllAsync();
            var vocabularyById = GetVocabulariesDictionary(
                ExternalSystemId.DarwinCore,
                vocabularies.ToArray(),
                true);
            return new DwcaObservationFactory(dataProvider, taxa, vocabularyById, areaHelper, processTimeManager, processConfiguration);
        }

        /// <summary>
        ///  Cast verbatim observations to processed data model
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="diffuseIfSupported"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(DwcObservationVerbatim verbatim, bool diffuseIfSupported)
        {
            if (verbatim == null)
            {
                return null;
            }

            var accessRights = GetSosId(verbatim.AccessRights, _vocabularyById[VocabularyId.AccessRights]);
            var obs = new Observation
            {
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
                _vocabularyById[VocabularyId.BasisOfRecord]);
            obs.BibliographicCitation = verbatim.BibliographicCitation;
            obs.CollectionCode = verbatim.CollectionCode;
            obs.CollectionId = verbatim.CollectionID;
            obs.DataGeneralizations = verbatim.DataGeneralizations;
            obs.DatasetId = verbatim.DatasetID;
            obs.DatasetName = string.IsNullOrWhiteSpace(verbatim.DatasetName) ? _englishDataproviderName : verbatim.DatasetName;
            obs.DynamicProperties = verbatim.DynamicProperties;
            obs.InformationWithheld = verbatim.InformationWithheld;
            obs.InstitutionId = verbatim.InstitutionID;
            obs.InstitutionCode = GetSosId(verbatim.InstitutionCode,
                _vocabularyById[VocabularyId.Institution]);
            obs.Language = verbatim.Language;
            obs.License = verbatim.License;
            obs.Modified = DwcParser.ParseDate(verbatim.Modified)?.ToUniversalTime();
            obs.OwnerInstitutionCode = verbatim.OwnerInstitutionCode;
            obs.References = verbatim.References;
            obs.RightsHolder = verbatim.RightsHolder;
            obs.Type = GetSosId(verbatim.Type,
                _vocabularyById[VocabularyId.Type]);
            obs.DataStewardshipDatasetId = verbatim.DataStewardshipDatasetId;

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
            AddPositionData(obs.Location,
                verbatim.DecimalLongitude.ParseDouble() ?? verbatim.VerbatimLongitude.ParseDouble(),
                verbatim.DecimalLatitude.ParseDouble() ?? verbatim.VerbatimLatitude.ParseDouble(),
                coordinateSystem,
                verbatim.CoordinateUncertaintyInMeters?.ParseDoubleConvertToInt() ?? DefaultCoordinateUncertaintyInMeters,
                obs.Taxon?.Attributes?.DisturbanceRadius);

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
            }

            // Populate generic data
            PopulateGenericData(obs);

            return obs;
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
            processedEvent.VerbatimEventDate = verbatim.VerbatimEventDate;

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
            string dateIdentifiedString = null;
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
            processedIdentification.VerificationStatus = GetSosId(verbatim.IdentificationVerificationStatus, _vocabularyById[VocabularyId.VerificationStatus]);
            processedIdentification.Verified = GetIsValidated(processedIdentification.VerificationStatus);
            processedIdentification.IdentifiedBy = verbatim.IdentifiedBy?.Clean();
            processedIdentification.TypeStatus = verbatim.TypeStatus;
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

        private Location CreateProcessedLocation(DwcObservationVerbatim verbatim)
        {
           
           
            var processedLocation = new Location(LocationType.Point);
            processedLocation.Continent = GetSosId(
                verbatim.Continent,
                _vocabularyById[VocabularyId.Continent],
                (int) ContinentId.Europe,
                MappingNotFoundLogic.UseDefaultValue);
            processedLocation.CoordinatePrecision = verbatim.CoordinatePrecision.ParseDouble();
            processedLocation.CoordinateUncertaintyInMeters =
                verbatim.CoordinateUncertaintyInMeters?.ParseDoubleConvertToInt() ?? DefaultCoordinateUncertaintyInMeters;
            processedLocation.Country = GetSosId(
                verbatim.Country,
                _vocabularyById[VocabularyId.Country],
                (int) CountryId.Sweden,
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
            processedLocation.VerbatimLocality = processedLocation.Locality;
            processedLocation.WaterBody = verbatim.WaterBody;
            
            return processedLocation;
        }


        private Occurrence CreateProcessedOccurrence(DwcObservationVerbatim verbatim, Lib.Models.Processed.Observation.Taxon taxon, AccessRightsId? accessRightsId)
        {
            var processedOccurrence = new Occurrence();
            processedOccurrence.BirdNestActivityId = taxon?.IsBird() ?? false ? 1000000 : 0;
            processedOccurrence.AssociatedMedia = verbatim.AssociatedMedia;
            processedOccurrence.AssociatedReferences = verbatim.AssociatedReferences;
            processedOccurrence.AssociatedSequences = verbatim.AssociatedSequences;
            processedOccurrence.AssociatedTaxa = verbatim.AssociatedTaxa;
            processedOccurrence.CatalogNumber = verbatim.CatalogNumber ?? verbatim.OccurrenceID;
            processedOccurrence.Disposition = verbatim.Disposition;
            processedOccurrence.EstablishmentMeans = GetSosId(verbatim.EstablishmentMeans,
                _vocabularyById[VocabularyId.EstablishmentMeans]);
            processedOccurrence.IndividualCount = verbatim.IndividualCount;
            processedOccurrence.LifeStage = GetSosId(verbatim.LifeStage, _vocabularyById[VocabularyId.LifeStage]);
            processedOccurrence.Media = CreateProcessedMultimedia(
                verbatim.ObservationMultimedia,
                verbatim.ObservationAudubonMedia);
            processedOccurrence.OccurrenceId = verbatim.OccurrenceID;
            processedOccurrence.OccurrenceRemarks = verbatim.OccurrenceRemarks?.Clean();
            processedOccurrence.OccurrenceStatus = GetSosId(
                verbatim.OccurrenceStatus,
                _vocabularyById[VocabularyId.OccurrenceStatus],
                (int)OccurrenceStatusId.Present);
            processedOccurrence.OrganismQuantity = verbatim.OrganismQuantity;
            if (int.TryParse(verbatim.OrganismQuantity, out var quantity))
            {
                processedOccurrence.OrganismQuantityAggregation = quantity;
                processedOccurrence.OrganismQuantityInt = quantity;
            }
            processedOccurrence.OrganismQuantityUnit = GetSosId(verbatim.OrganismQuantityType, _vocabularyById[VocabularyId.Unit]);
            processedOccurrence.OtherCatalogNumbers = verbatim.OtherCatalogNumbers;
            processedOccurrence.Preparations = verbatim.Preparations;
            processedOccurrence.RecordedBy = verbatim.RecordedBy;
            processedOccurrence.RecordNumber = verbatim.RecordNumber;
            processedOccurrence.Activity = GetSosId(
                verbatim.ReproductiveCondition,
                _vocabularyById[VocabularyId.Activity]);
            processedOccurrence.Sex = GetSosId(verbatim.Sex, _vocabularyById[VocabularyId.Sex]);
            processedOccurrence.ReproductiveCondition = GetSosId(verbatim.ReproductiveCondition, _vocabularyById.GetValue(VocabularyId.ReproductiveCondition));
            processedOccurrence.Behavior = GetSosId(verbatim.Behavior, _vocabularyById.GetValue(VocabularyId.Behavior));
            processedOccurrence.IsNaturalOccurrence = true;
            processedOccurrence.IsNeverFoundObservation = false;
            processedOccurrence.IsNotRediscoveredObservation = false;
            processedOccurrence.IsPositiveObservation = true; 
            if (processedOccurrence.OccurrenceStatus?.Id == (int) OccurrenceStatusId.Absent)
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
            // If dataprovider uses Dyntaxa Taxon Id, try parse TaxonId.
            if (!string.IsNullOrEmpty(verbatim.TaxonID))
            {
                var parsedTaxonId = -1;

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
                    if(!int.TryParse(lastInteger, out parsedTaxonId))
                    {
                        parsedTaxonId = -1;
                    }
                }                

                if (parsedTaxonId != -1)
                {
                    var taxon = GetTaxon(parsedTaxonId);

                    if ((taxon?.Id ?? -1) != -1)
                    {
                        return taxon;
                    }
                }
            }

            // Get by scientific name
            if (_taxonByScientificName.TryGetValues(verbatim.ScientificName?.ToLower(), out var taxa))
            {
                if (taxa.Count == 1)
                {
                    return taxa.First();
                }
            }

            // Get by scientific name + author
            if (_taxonByScientificNameAuthor.TryGetValues(verbatim.ScientificName?.ToLower(), out taxa))
            {
                if (taxa.Count == 1)
                {
                    return taxa.First();
                }
            }

            // Get by species
            if (_taxonByScientificName.TryGetValues(verbatim.Species?.ToLower(), out taxa))
            {
                if (taxa.Count == 1)
                {
                    return taxa.First();
                }
            }

            // Get by synonyme
            if (_taxonBySynonymName.TryGetValues(verbatim.ScientificName?.ToLower(), out taxa))
            {
                if (taxa.Count == 1)
                {
                    return taxa.First();
                }
            }

            // Get by synonyme + author
            if (_taxonBySynonymNameAuthor.TryGetValues(verbatim.ScientificName?.ToLower(), out taxa))
            {
                if (taxa.Count == 1)
                {
                    return taxa.First();
                }
            }

            // Get by species (synonyme)
            if (_taxonBySynonymName.TryGetValues(verbatim.Species?.ToLower(), out taxa))
            {
                if (taxa.Count == 1)
                {
                    return taxa.First();
                }
            }

            return new Lib.Models.Processed.Observation.Taxon { Id = -1, VerbatimId = verbatim.TaxonID };
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
    }
}