//#define INCLUDE_DIFFUSED_OBSERVATIONS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nest;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Resource.Interfaces;
using Language = SOS.Lib.Models.DarwinCore.Vocabulary.Language;
using Project = SOS.Lib.Models.Verbatim.Artportalen.Project;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Process.Processors.Artportalen
{
    public class ArtportalenObservationFactory
    {
        private readonly DataProvider _dataProvider;
        private readonly IDictionary<VocabularyId, IDictionary<object, int>> _vocabularyById;
        private readonly IDictionary<int, Lib.Models.Processed.Observation.Taxon> _taxa;
        private readonly bool _incrementalMode;

        public ArtportalenObservationFactory(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IDictionary<VocabularyId, IDictionary<object, int>> vocabularyById,
            bool incrementalMode)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            {
                _taxa = taxa ?? throw new ArgumentNullException(nameof(taxa));
                _vocabularyById = vocabularyById ?? throw new ArgumentNullException(nameof(vocabularyById));
            }

            _incrementalMode = incrementalMode;
        }

        public static async Task<ArtportalenObservationFactory> CreateAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IVocabularyRepository processedVocabularyRepository,
            bool incrementalMode)
        {
            var allFieldMappings = await processedVocabularyRepository.GetAllAsync();
            var processedVocabularies = GetVocabulariesDictionary(ExternalSystemId.Artportalen, allFieldMappings.ToArray());
            return new ArtportalenObservationFactory(dataProvider, taxa, processedVocabularies, incrementalMode);
        }

        public ICollection<Observation> CreateProcessedObservations(
            IEnumerable<ArtportalenObservationVerbatim> verbatimObservations)
        {
            return verbatimObservations.Select(CreateProcessedObservation).Where(p=>p!=null).ToArray();
        }

        /// <summary>
        ///     Cast verbatim observations to processed data model
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(ArtportalenObservationVerbatim verbatimObservation)
        {
            try
            {
                if (verbatimObservation == null)
                {
                    return null;
                }

                var taxonId = verbatimObservation.TaxonId ?? -1;
                if (_taxa.TryGetValue(taxonId, out var taxon))
                {
                    taxon.IndividualId = verbatimObservation.URL;
                }

                if (ShouldBeDiffused(verbatimObservation, taxon))
                {
                    return null;
#if INCLUDE_DIFFUSED_OBSERVATIONS
                    //If it is a protected sighting it should not be possible to find it in the current month
                    if((verbatimObservation?.StartDate.Value.Year == DateTime.Now.Year || verbatimObservation?.EndDate.Value.Year == DateTime.Now.Year) &&
                        (verbatimObservation?.StartDate.Value.Month == DateTime.Now.Month || verbatimObservation?.EndDate.Value.Month == DateTime.Now.Month))
                    {
                        return null;
                    }
                    //Diffuse the observation depending on the protectionlevel                
                    verbatimObservation = DiffuseObservation(verbatimObservation, taxon);
#endif
                }

                var hasPosition = (verbatimObservation.Site?.XCoord ?? 0) > 0 &&
                                  (verbatimObservation.Site?.YCoord ?? 0) > 0;
                var point = (PointGeoShape) verbatimObservation.Site?.Point?.ToGeoShape();                                              

                // Add time to start date if it exists
                var startDate = verbatimObservation.StartDate.HasValue && verbatimObservation.StartTime.HasValue
                    ? verbatimObservation.StartDate.Value.ToLocalTime() + verbatimObservation.StartTime
                    : verbatimObservation.StartDate.Value;

                // Add time to end date if it exists
                var endDate = verbatimObservation.EndDate.HasValue && verbatimObservation.EndTime.HasValue
                    ? verbatimObservation.EndDate.Value.ToLocalTime() + verbatimObservation.EndTime
                    : verbatimObservation.EndDate;

                var obs = new Observation();

                // Record level
                obs.DataProviderId = _dataProvider.Id;
                obs.AccessRights = !verbatimObservation.ProtectedBySystem && verbatimObservation.HiddenByProvider.HasValue &&
                                   verbatimObservation.HiddenByProvider.GetValueOrDefault(DateTime.MinValue) < DateTime.Now
                    ? new VocabularyValue {Id = (int) AccessRightsId.FreeUsage}
                    : new VocabularyValue {Id = (int) AccessRightsId.NotForPublicUsage};
                obs.BasisOfRecord = string.IsNullOrEmpty(verbatimObservation.SpeciesCollection)
                    ? new VocabularyValue {Id = (int) BasisOfRecordId.HumanObservation}
                    : new VocabularyValue {Id = (int) BasisOfRecordId.PreservedSpecimen};
                obs.CollectionCode = string.IsNullOrEmpty(verbatimObservation.SpeciesCollection)
                    ? "Artportalen"
                    : verbatimObservation.SpeciesCollection;
                obs.CollectionId = verbatimObservation.CollectionID;
                obs.DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.Artportalen}";
                obs.DatasetName = "Artportalen";
                obs.InformationWithheld = null;
                obs.IsInEconomicZoneOfSweden = hasPosition;
                obs.Language = Language.Swedish;
                obs.Modified = verbatimObservation.EditDate.ToUniversalTime();
                obs.Type = null;
                obs.OwnerInstitutionCode = verbatimObservation.OwnerOrganization?.Translate(Cultures.en_GB, Cultures.sv_SE) ?? "Artdatabanken";
                obs.ProtectionLevel = CalculateProtectionLevel(taxon, verbatimObservation.HiddenByProvider, verbatimObservation.ProtectedBySystem);
                obs.ReportedBy = verbatimObservation.ReportedBy;
                obs.ReportedDate = verbatimObservation.ReportedDate?.ToUniversalTime();
                obs.RightsHolder = verbatimObservation.RightsHolder ?? verbatimObservation.OwnerOrganization?.Translate(Cultures.en_GB, Cultures.sv_SE) ?? "Data saknas";

                // Event
                obs.Event = new Event();
                obs.Event.Biotope = GetSosIdFromMetadata(verbatimObservation?.Biotope, _vocabularyById[VocabularyId.Biotope]);
                obs.Event.BiotopeDescription = verbatimObservation.BiotopeDescription;
                obs.Event.EndDate = endDate?.ToUniversalTime();
                obs.Event.QuantityOfSubstrate = verbatimObservation.QuantityOfSubstrate;
                obs.Event.SamplingProtocol = GetSamplingProtocol(verbatimObservation.Projects);
                obs.Event.StartDate = startDate?.ToUniversalTime();
                obs.Event.SubstrateSpeciesDescription = verbatimObservation.SubstrateSpeciesDescription;
                obs.Event.SubstrateDescription = GetSubstrateDescription(verbatimObservation, _taxa);
                obs.Event.VerbatimEventDate = DwcFormatter.CreateDateIntervalString(startDate, endDate);

                if (verbatimObservation.SubstrateSpeciesId.HasValue && _taxa != null && _taxa.TryGetValue(verbatimObservation.SubstrateSpeciesId.Value, out var substratTaxon))
                {
                    obs.Event.SubstrateSpeciesId = verbatimObservation.SubstrateSpeciesId.Value;
                    obs.Event.SubstrateSpeciesVernacularName = substratTaxon.VernacularName;
                    obs.Event.SubstrateSpeciesScientificName = substratTaxon.ScientificName;
                }

                obs.Event.Substrate = GetSosIdFromMetadata(verbatimObservation?.Substrate, _vocabularyById[VocabularyId.Substrate]);

                // Identification
                obs.Identification = new Identification();
                obs.Identification.IdentifiedBy = verbatimObservation.VerifiedBy;
                obs.Identification.Validated = new[] {60, 61, 62, 63, 64, 65}.Contains(verbatimObservation.ValidationStatus?.Id ?? 0);
                obs.Identification.UncertainDetermination = verbatimObservation.UnsureDetermination;

                // Location
                obs.Location = new Location();
                obs.Location.Continent = new VocabularyValue {Id = (int) ContinentId.Europe};
                obs.Location.CoordinateUncertaintyInMeters = verbatimObservation.Site?.Accuracy;
                obs.Location.Country = new VocabularyValue {Id = (int) CountryId.Sweden};
                obs.Location.CountryCode = CountryCode.Sweden;
                obs.Location.County = GetSosId(verbatimObservation.Site?.County?.Id, _vocabularyById[VocabularyId.County], null, verbatimObservation.Site?.County?.Name);
                obs.Location.DecimalLatitude = point?.Coordinates?.Latitude ?? 0;
                obs.Location.DecimalLongitude = point?.Coordinates?.Longitude ?? 0;
                obs.Location.GeodeticDatum = GeodeticDatum.Wgs84;
                obs.Location.Locality = verbatimObservation.Site?.Name.Trim();
                obs.Location.LocationId = $"urn:lsid:artportalen.se:site:{verbatimObservation.Site?.Id}";
                obs.Location.MaximumDepthInMeters = verbatimObservation.MaxDepth;
                obs.Location.MaximumElevationInMeters = verbatimObservation.MaxHeight;
                obs.Location.MinimumDepthInMeters = verbatimObservation.MinDepth;
                obs.Location.MinimumElevationInMeters = verbatimObservation.MinHeight;
                obs.Location.Municipality = GetSosId(verbatimObservation.Site?.Municipality?.Id, _vocabularyById[VocabularyId.Municipality], null, verbatimObservation.Site?.Municipality?.Name);
                obs.Location.ParentLocationId = verbatimObservation.Site?.ParentSiteId;
                obs.Location.ParentLocality = verbatimObservation.Site?.ParentSiteName?.Trim();
                obs.Location.Parish = GetSosId(verbatimObservation.Site?.Parish?.Id, _vocabularyById[VocabularyId.Parish], null, verbatimObservation.Site?.Parish?.Name);
                obs.Location.Point = point;
                obs.Location.PointLocation = verbatimObservation.Site?.Point?.ToGeoLocation();
                obs.Location.PointWithBuffer = (PolygonGeoShape) verbatimObservation.Site?.PointWithBuffer.ToGeoShape();
                obs.Location.Province = GetSosId(verbatimObservation.Site?.Province?.Id, _vocabularyById[VocabularyId.Province], null, verbatimObservation.Site?.Province?.Name);
                obs.Location.VerbatimLatitude = hasPosition ? verbatimObservation.Site.YCoord.ToString() : null;
                obs.Location.VerbatimLongitude = hasPosition ? verbatimObservation.Site.XCoord.ToString() : null;
                obs.Location.VerbatimCoordinateSystem = "EPSG:3857";

                // Occurrence
                obs.Occurrence = new Occurrence();
                obs.Occurrence.AssociatedMedia = verbatimObservation.HasImages
                    ? $"http://www.artportalen.se/sighting/{verbatimObservation.SightingId}#SightingDetailImages"
                    : "";
                obs.Occurrence.AssociatedReferences = GetAssociatedReferences(verbatimObservation);
                obs.Occurrence.BirdNestActivityId = GetBirdNestActivityId(verbatimObservation, taxon);
                obs.Occurrence.CatalogNumber = verbatimObservation.SightingId.ToString();
                obs.Occurrence.OccurrenceId = $"urn:lsid:artportalen.se:Sighting:{verbatimObservation.SightingId}";
                obs.Occurrence.IndividualCount = verbatimObservation.Quantity?.ToString() ?? "";
                obs.Occurrence.IsNaturalOccurrence = !verbatimObservation.Unspontaneous;
                obs.Occurrence.IsNeverFoundObservation = verbatimObservation.NotPresent;
                obs.Occurrence.IsNotRediscoveredObservation = verbatimObservation.NotRecovered;
                obs.Occurrence.IsPositiveObservation = !(verbatimObservation.NotPresent || verbatimObservation.NotRecovered);
                obs.Occurrence.OrganismQuantityInt = verbatimObservation.Quantity;
                obs.Occurrence.OrganismQuantity = verbatimObservation.Quantity.ToString();
                obs.Occurrence.RecordedBy = verbatimObservation.Observers;
                obs.Occurrence.RecordNumber = verbatimObservation.Label;
                obs.Occurrence.OccurrenceRemarks = verbatimObservation.Comment;
                obs.Occurrence.OccurrenceStatus = verbatimObservation.NotPresent || verbatimObservation.NotRecovered
                    ? new VocabularyValue {Id = (int) OccurrenceStatusId.Absent}
                    : new VocabularyValue {Id = (int) OccurrenceStatusId.Present};
                obs.Occurrence.Url = $"http://www.artportalen.se/sighting/{verbatimObservation.SightingId}";
                obs.Occurrence.Length = verbatimObservation.Length;
                obs.Occurrence.Weight = verbatimObservation.Weight;
                obs.Occurrence.PublicCollection = verbatimObservation.PublicCollection?.Translate(Cultures.en_GB, Cultures.sv_SE);
                obs.Occurrence.ConfirmationYear = verbatimObservation.ConfirmationYear;
                obs.Occurrence.ConfirmedBy = verbatimObservation.ConfirmedBy;
                obs.Occurrence.DeterminationYear = verbatimObservation.DeterminationYear;
                obs.Occurrence.DeterminedBy = verbatimObservation.DeterminedBy;

                // Taxon
                obs.Taxon = taxon;

                // ArtportalenInternal
                obs.ArtportalenInternal = new ArtportalenInternal();
                obs.ArtportalenInternal.HasTriggeredValidationRules = verbatimObservation.HasTriggeredValidationRules;
                obs.ArtportalenInternal.HasAnyTriggeredValidationRuleWithWarning = verbatimObservation.HasAnyTriggeredValidationRuleWithWarning;
                obs.ArtportalenInternal.SightingSpeciesCollectionItemId = verbatimObservation.SightingSpeciesCollectionItemId;
                obs.ArtportalenInternal.PrivateCollection = verbatimObservation.PrivateCollection;
                obs.ArtportalenInternal.SpeciesFactsIds = verbatimObservation.SpeciesFactsIds;
                obs.ArtportalenInternal.LocationExternalId = verbatimObservation.Site?.ExternalId;
                obs.ArtportalenInternal.NoteOfInterest = verbatimObservation.NoteOfInterest;
                obs.ArtportalenInternal.SightingId = verbatimObservation.SightingId;
                obs.ArtportalenInternal.SightingTypeId = verbatimObservation.SightingTypeId;
                obs.ArtportalenInternal.SightingTypeSearchGroupId = verbatimObservation.SightingTypeSearchGroupId;
                obs.ArtportalenInternal.RegionalSightingStateId = verbatimObservation.RegionalSightingStateId;
                obs.ArtportalenInternal.SightingPublishTypeIds = verbatimObservation.SightingPublishTypeIds;
                obs.ArtportalenInternal.OccurrenceRecordedByInternal = verbatimObservation.VerifiedByInternal;
                obs.ArtportalenInternal.ReportedByUserId = verbatimObservation.ReportedByUserId;
                obs.ArtportalenInternal.ReportedByUserAlias = verbatimObservation.ReportedByUserAlias;
                obs.ArtportalenInternal.LocationPresentationNameParishRegion = verbatimObservation.Site?.PresentationNameParishRegion;
                obs.ArtportalenInternal.OccurrenceRecordedByInternal = verbatimObservation.ObserversInternal;
                obs.ArtportalenInternal.Projects = verbatimObservation.Projects?.Select(CreateProcessedProject);
                obs.ArtportalenInternal.IncrementalHarvested = _incrementalMode;

                // Set dependent properties
                var biotope = obs.Event.Biotope?.Value;
                obs.Event.Habitat = (biotope != null
                    ? $"{biotope}{(string.IsNullOrEmpty(obs.Event.BiotopeDescription) ? "" : " # ")}{obs.Event.BiotopeDescription}"
                    : obs.Event.BiotopeDescription).WithMaxLength(255);

                // Get field mapping values
                obs.Occurrence.Gender = GetSosIdFromMetadata(verbatimObservation?.Gender, _vocabularyById[VocabularyId.Gender]);
                obs.Occurrence.Activity = GetSosIdFromMetadata(verbatimObservation?.Activity, _vocabularyById[VocabularyId.Activity]);
                
                obs.Identification.ValidationStatus = GetSosIdFromMetadata(verbatimObservation?.ValidationStatus, _vocabularyById[VocabularyId.ValidationStatus]);
                obs.Occurrence.LifeStage = GetSosIdFromMetadata(verbatimObservation?.Stage, _vocabularyById[VocabularyId.LifeStage]);
                obs.InstitutionCode = GetSosIdFromMetadata(verbatimObservation?.OwnerOrganization, _vocabularyById[VocabularyId.Institution]);
                obs.InstitutionId = verbatimObservation?.OwnerOrganization == null
                    ? null
                    : $"urn:lsid:artdata.slu.se:organization:{verbatimObservation.OwnerOrganization.Id}";
                obs.Occurrence.OrganismQuantityUnit = GetSosIdFromMetadata(
                    verbatimObservation?.Unit, 
                    _vocabularyById[VocabularyId.Unit],
                    (int) UnitId.Individuals);
                obs.Occurrence.DiscoveryMethod = GetSosIdFromMetadata(verbatimObservation?.DiscoveryMethod, _vocabularyById[VocabularyId.DiscoveryMethod]);
                obs.Identification.DeterminationMethod = GetSosIdFromMetadata(verbatimObservation?.DeterminationMethod, _vocabularyById[VocabularyId.DeterminationMethod]);
                CreateMeasurementOrFactsFromProjects(obs.Occurrence.OccurrenceId, verbatimObservation.Projects);

                return obs;
            }
            catch (Exception e)
            {
                throw new Exception($"Error when processing Artportalen verbatim observation with Id={verbatimObservation.Id}, SightingId={verbatimObservation.SightingId}", e);
            }
        }

        private List<ExtendedMeasurementOrFact> CreateMeasurementOrFactsFromProjects(string occurrenceId, IEnumerable<Project> projects)
        {
            if (projects == null || !projects.Any()) return null;
            var emofCollection = new List<ExtendedMeasurementOrFact>();

            foreach (var project in projects)
            {
                if (!project?.ProjectParameters?.Any() ?? true)
                {
                    continue;
                }

                foreach (var projectParameter in project.ProjectParameters)
                {
                    var emof = new ExtendedMeasurementOrFact();
                    emof.OccurrenceID = occurrenceId;
                    emof.MeasurementID = $"{project.Id}-{projectParameter.Id}";
                    emof.MeasurementType = projectParameter.Name;
                    emof.MeasurementValue = projectParameter.Value;
                    emof.MeasurementUnit = projectParameter.Unit;
                    emof.MeasurementDeterminedDate = DwcFormatter.CreateDateIntervalString(project.StartDate, project.EndDate);
                    emof.MeasurementMethod = GetMeasurementMethodDescription(project);
                    emof.MeasurementRemarks = GetMeasurementRemarks(projectParameter, project);

                    emofCollection.Add(emof);
                }
            }

            if (!emofCollection.Any()) return null;
            return emofCollection;
        }

        private string GetMeasurementMethodDescription(Project project)
        {
            if (string.IsNullOrEmpty(project.SurveyMethod) && string.IsNullOrEmpty(project.SurveyMethodUrl))
            {
                return null;
            }

            if (string.IsNullOrEmpty(project.SurveyMethodUrl))
            {
                return project.SurveyMethod;
            }

            if (string.IsNullOrEmpty(project.SurveyMethod))
            {
                return project.SurveyMethodUrl;
            }

            return $"{project.SurveyMethod} [{project.SurveyMethodUrl}]";
        }

        private string GetMeasurementRemarks(Lib.Models.Verbatim.Artportalen.ProjectParameter projectParameter, Project project)
        {
            if (string.IsNullOrWhiteSpace(projectParameter.Description) && string.IsNullOrWhiteSpace(project.Name))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(projectParameter.Description))
            {
                return $"Artportalen project=\"{project.Name}\"";
            }

            if (projectParameter.Description.EndsWith("."))
            {
                return $"{projectParameter.Description} Artportalen project=\"{project.Name}\"";
            }
            else
            {
                return $"{projectParameter.Description}. Artportalen project=\"{project.Name}\"";
            }
        }

        /// <summary>
        /// Diffuse the observation data depending on the protectionlevel
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <param name="taxon"></param>
        /// <returns></returns>
        private ArtportalenObservationVerbatim DiffuseObservation(ArtportalenObservationVerbatim verbatimObservation, Lib.Models.Processed.Observation.Taxon taxon)
        {
            verbatimObservation.Comment = "";
            verbatimObservation.ReportedBy = "";
            verbatimObservation.ReportedByUserAlias = "";
            verbatimObservation.ReportedByUserId = -1;
            verbatimObservation.Observers = "";
            verbatimObservation.ObserversInternal = new List<UserInternal>();

            if(verbatimObservation.ReportedDate.HasValue)
                verbatimObservation.ReportedDate = new DateTime(verbatimObservation.ReportedDate.Value.Year, verbatimObservation.ReportedDate.Value.Month, 1);
            if (verbatimObservation.StartDate.HasValue)
                verbatimObservation.StartDate = new DateTime(verbatimObservation.StartDate.Value.Year, verbatimObservation.StartDate.Value.Month, 1);
            if (verbatimObservation.EndDate.HasValue)
                verbatimObservation.EndDate = new DateTime(verbatimObservation.EndDate.Value.Year, verbatimObservation.EndDate.Value.Month, 1);
            verbatimObservation.EditDate = new DateTime(verbatimObservation.EditDate.Year, verbatimObservation.EditDate.Month, 1);
            
            var diffusedPoint = DiffusePoint(verbatimObservation.Site?.Point, verbatimObservation, taxon);
            var diffusedPolygon= DiffusePolygon(verbatimObservation.Site?.PointWithBuffer, verbatimObservation, taxon);

            var newSite = new Site();
            newSite.Point = diffusedPoint;
            newSite.PointWithBuffer = diffusedPolygon;
            newSite.Name = "";
            newSite.CountryPart = verbatimObservation.Site.CountryPart;
            newSite.CountryRegion = verbatimObservation.Site.CountryRegion;
            newSite.County= verbatimObservation.Site.County;
            newSite.Municipality = verbatimObservation.Site.Municipality;
            newSite.Parish = verbatimObservation.Site.Parish;
            newSite.PresentationNameParishRegion = verbatimObservation.Site.PresentationNameParishRegion;
            newSite.ProtectedNature = verbatimObservation.Site.ProtectedNature;
            newSite.Province = verbatimObservation.Site.Province;
            newSite.SpecialProtectionArea = verbatimObservation.Site.SpecialProtectionArea;
            newSite.WaterArea = verbatimObservation.Site.WaterArea;
            newSite.Accuracy = verbatimObservation.Site.Accuracy;

            verbatimObservation.Site = newSite;

            return verbatimObservation;
        }
        private (int mod, int add) GetDiffusionValues(int protectionLevel)
        {
            if (protectionLevel == 2)
            {
               return (1000,555);
            }
            if (protectionLevel == 3)
            {
                return (5000, 2505);                
            }
            if (protectionLevel == 4)
            {
                return (25000, 12505);                
            }
            if (protectionLevel == 5)
            {
                return (50000, 25005);                
            }
            else
            {
                return (0, 0);
            }
        }
        private bool ShouldBeDiffused(ArtportalenObservationVerbatim observationVerbatim, Lib.Models.Processed.Observation.Taxon taxon)
        {
            if (string.IsNullOrEmpty(taxon?.ProtectionLevel))
            {
                return false;
            }

            if (observationVerbatim.ProtectedBySystem)
            {
                var regex = new Regex(@"^\d");

                if (int.TryParse(regex.Match(taxon.ProtectionLevel).Value, out var protectionLevel))
                {
                    if(protectionLevel > 2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Diffuse the point dependent on the sightings protection level
        /// </summary>
        /// <param name="point"></param>
        /// <param name="verbatimObservation"></param>
        /// <param name="obs"></param>
        /// <returns></returns>
        private GeoJsonGeometry DiffusePoint(GeoJsonGeometry point, ArtportalenObservationVerbatim observationVerbatim, Lib.Models.Processed.Observation.Taxon taxon)
        {
            if (string.IsNullOrEmpty(taxon?.ProtectionLevel))
            {
                return point;
            }

            var originalPoint = point;
            var diffusedPoint = point;
            if(observationVerbatim.ProtectedBySystem)
            {
                var regex = new Regex(@"^\d");
                
                if (int.TryParse(regex.Match(taxon.ProtectionLevel).Value, out var protectionLevel))
                {
                    var diffValues = GetDiffusionValues(protectionLevel);
                    var latitude = (double)originalPoint.Coordinates[1];
                    var longitude = (double)originalPoint.Coordinates[0];
                    //transform the point into the same format as Artportalen so that we can use the same diffusion as them
                    var geompoint = new NetTopologySuite.Geometries.Point(longitude, latitude);
                    var transformedPoint = geompoint.Transform(CoordinateSys.WGS84, CoordinateSys.WebMercator);
                    var diffusedUntransformedPoint = new NetTopologySuite.Geometries.Point(transformedPoint.Coordinates[0].X - transformedPoint.Coordinates[0].X % diffValues.mod + diffValues.add, transformedPoint.Coordinates[0].Y - transformedPoint.Coordinates[0].Y % diffValues.mod + diffValues.add);
                    var retransformedPoint = diffusedUntransformedPoint.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84);
                    //retransform to the correct format again
                    diffusedPoint.Coordinates = new System.Collections.ArrayList { retransformedPoint.Coordinates[0].X, retransformedPoint.Coordinates[0].Y };                        

                }
            }
            return diffusedPoint;
        }
        /// <summary>
        /// Diffuse the point dependent on the sightings protection level
        /// </summary>
        /// <param name="point"></param>
        /// <param name="verbatimObservation"></param>
        /// <param name="obs"></param>
        /// <returns></returns>
        private GeoJsonGeometry DiffusePolygon(GeoJsonGeometry polygon, ArtportalenObservationVerbatim observationVerbatim, Lib.Models.Processed.Observation.Taxon taxon)
        {
            if (string.IsNullOrEmpty(taxon?.ProtectionLevel))
            {
                return polygon;
            }

            var originalPoint = polygon;
            var diffusedPoint = polygon;
            if (observationVerbatim.ProtectedBySystem)
            {
                var regex = new Regex(@"^\d");

                if (int.TryParse(regex.Match(taxon.ProtectionLevel).Value, out var protectionLevel))
                {
                    var diffValues = GetDiffusionValues(protectionLevel);
                    var newCoordinates = new double[((double[][])originalPoint.Coordinates[0]).Length][];
                    int index = 0;
                    foreach (var point in ((double[][])originalPoint.Coordinates[0]))
                    {
                        newCoordinates[index] = new double[2];
                        var latitude = (double)point[1];
                        var longitude = (double)point[0];
                        var geompoint = new NetTopologySuite.Geometries.Point(longitude, latitude);
                        var transformedPoint = geompoint.Transform(CoordinateSys.WGS84, CoordinateSys.WebMercator);
                        var diffusedUntransformedPoint = new NetTopologySuite.Geometries.Point(transformedPoint.Coordinates[0].X - transformedPoint.Coordinates[0].X % diffValues.mod + diffValues.add, transformedPoint.Coordinates[0].Y - transformedPoint.Coordinates[0].Y % diffValues.mod + diffValues.add);
                        var retransformedPoint = diffusedUntransformedPoint.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84);
                        newCoordinates[index][0] = retransformedPoint.Coordinates[0].X;
                        newCoordinates[index][1] = retransformedPoint.Coordinates[0].Y;                        
                        index++;
                    }
                    diffusedPoint.Coordinates = new System.Collections.ArrayList() { newCoordinates };
                    diffusedPoint.Type = "Polygon";                    
                }
            }
            return diffusedPoint;
        }

        /// <summary>
        ///     Get SOS internal Id for the id specific for the data provider.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sosIdByProviderValue"></param>
        /// <param name="defaultId"></param>
        /// <param name="valueIfValNotFound"></param>
        /// <returns></returns>
        private static VocabularyValue GetSosId(
            int? val, 
            IDictionary<object, int> sosIdByProviderValue,
            int? defaultId = null,
            string valueIfValNotFound = null)
        {
            if (!val.HasValue || sosIdByProviderValue == null) return null;

            if (sosIdByProviderValue.TryGetValue(val.Value, out var sosId))
            {
                return new VocabularyValue {Id = sosId};
            }

            if (defaultId.HasValue)
            {
                return new VocabularyValue {Id = defaultId.Value};
            }

            if (!string.IsNullOrEmpty(valueIfValNotFound))
            {
                return new VocabularyValue
                    { Id = VocabularyConstants.NoMappingFoundCustomValueIsUsedId, Value = valueIfValNotFound };
            }

            return new VocabularyValue
                {Id = VocabularyConstants.NoMappingFoundCustomValueIsUsedId, Value = val.ToString()};
        }

        /// <summary>
        ///     Get SOS internal Id for the id specific for the data provider.
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="sosIdByProviderValue"></param>
        /// <param name="defaultId"></param>
        /// <returns></returns>
        private static VocabularyValue GetSosIdFromMetadata(
            Metadata metadata,
            IDictionary<object, int> sosIdByProviderValue,
            int? defaultId = null)
        {
            int? val = metadata?.Id;
            if (!val.HasValue || sosIdByProviderValue == null) return null;

            if (sosIdByProviderValue.TryGetValue(val.Value, out var sosId))
            {
                return new VocabularyValue { Id = sosId };
            }

            var metadataValue = metadata?.Translate(Cultures.en_GB, Cultures.sv_SE);
            if (metadataValue != null)
            {
                return new VocabularyValue
                    { Id = VocabularyConstants.NoMappingFoundCustomValueIsUsedId, Value = metadataValue };
            }

            if (defaultId.HasValue)
            {
                return new VocabularyValue { Id = defaultId.Value };
            }

            return new VocabularyValue
                { Id = VocabularyConstants.NoMappingFoundCustomValueIsUsedId, Value = val.ToString() };
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
                StartDate =  project.StartDate?.ToUniversalTime(),
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
        /// <returns></returns>
        private static IDictionary<VocabularyId, IDictionary<object, int>> GetVocabulariesDictionary(
            ExternalSystemId externalSystemId,
            ICollection<Vocabulary> allFieldMappings)
        {
            var dic = new Dictionary<VocabularyId, IDictionary<object, int>>();

            foreach (var vocabularity in allFieldMappings)
            {
                var processedVocabularies = vocabularity.ExternalSystemsMapping.FirstOrDefault(m => m.Id == externalSystemId);
                if (processedVocabularies != null)
                {
                    var mappingKey = GetMappingKey(vocabularity.Id);
                    var mapping = processedVocabularies.Mappings.Single(m => m.Key == mappingKey);
                    var sosIdByValue = mapping.GetIdByValueDictionary();
                    dic.Add(vocabularity.Id, sosIdByValue);
                }
            }

            return dic;
        }

        private static string GetMappingKey(VocabularyId vocabularyId)
        {
            switch (vocabularyId)
            {
                case VocabularyId.Activity:
                case VocabularyId.Gender:
                case VocabularyId.County:
                case VocabularyId.Municipality:
                case VocabularyId.Parish:
                case VocabularyId.Province:
                case VocabularyId.LifeStage:
                case VocabularyId.Substrate:
                case VocabularyId.ValidationStatus:
                case VocabularyId.Biotope:
                case VocabularyId.Institution:
                case VocabularyId.AreaType:
                case VocabularyId.Unit:
                case VocabularyId.DiscoveryMethod:
                case VocabularyId.DeterminationMethod:
                    return "Id";
                default:
                    throw new ArgumentException($"No mapping exist for the field: {vocabularyId}");
            }
        }
    }
}