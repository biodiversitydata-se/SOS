using System.Collections.Concurrent;
using System.Text;
using NetTopologySuite.Geometries;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using Area = SOS.Lib.Models.Processed.Observation.Area;
using DateTime = System.DateTime;
using Language = SOS.Lib.Models.DarwinCore.Vocabulary.Language;
using Location = SOS.Lib.Models.Processed.Observation.Location;
using Project = SOS.Lib.Models.Verbatim.Artportalen.Project;
using ProjectParameter = SOS.Lib.Models.Verbatim.Artportalen.ProjectParameter;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;
using Nest;

namespace SOS.Harvest.Processors.Artportalen
{
    public class ArtportalenObservationFactory : ObservationFactoryBase, IObservationFactory<ArtportalenObservationVerbatim>
    { 
        private readonly IDictionary<VocabularyId, IDictionary<object, int>> _vocabularyById;
        private readonly bool _incrementalMode;
        private readonly string _artPortalenUrl;
        private int[] _validationStatusIdIds = new[] {
            (int) ValidationStatusId.ApprovedBasedOnReportersDocumentation,
            (int) ValidationStatusId.ApprovedSpecimenCheckedByValidator,
            (int) ValidationStatusId.ApprovedBasedOnImageSoundOrVideoRecording,
            (int) ValidationStatusId.ApprovedBasedOnReportersRarityForm,
            (int) ValidationStatusId.ApprovedBasedOnDeterminatorsVerification,
            (int) ValidationStatusId.ApprovedBasedOnReportersOldRarityForm
        };

        /// Cast verbatim area to processed area
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        private static Area CastToArea(GeographicalArea area)
        {
            if (area == null)
            {
                return null;
            }

            return new Area
            {
                FeatureId = area.FeatureId,
                Name = area.Name
            };
        }

        /// <summary>
        ///     Calculate protection level
        /// </summary>
        /// <param name="taxon"></param>
        /// <param name="hiddenByProviderUntil"></param>
        /// <param name="protectedBySystem"></param>
        /// <returns></returns>
        private int CalculateProtectionLevel(Lib.Models.Processed.Observation.Taxon taxon, DateTime? hiddenByProviderUntil, bool protectedBySystem)
        {
            var hiddenByProvider = hiddenByProviderUntil.HasValue && hiddenByProviderUntil.Value >= DateTime.Now;
            var taxonProtectionLevel = taxon?.Attributes?.ProtectionLevel?.Id ?? 3;

            if (hiddenByProvider || protectedBySystem)
            {
                return Math.Max(3, taxonProtectionLevel);
            }

            return 1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="vocabularyById"></param>
        /// <param name="incrementalMode"></param>
        /// <param name="artPortalenUrl"></param>
        /// <param name="processTimeManager"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArtportalenObservationFactory(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IDictionary<VocabularyId, IDictionary<object, int>> vocabularyById,
            bool incrementalMode,
            string artPortalenUrl, 
            IProcessTimeManager processTimeManager) : base(dataProvider, taxa, processTimeManager) 
        {
            _vocabularyById = vocabularyById ?? throw new ArgumentNullException(nameof(vocabularyById));
            _incrementalMode = incrementalMode;
            _artPortalenUrl = artPortalenUrl ?? throw new ArgumentNullException(nameof(artPortalenUrl));
        }

        public static async Task<ArtportalenObservationFactory> CreateAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IVocabularyRepository processedVocabularyRepository,
            bool incrementalMode,
            string artPortalenUrl,
            IProcessTimeManager processTimeManager)
        {
            var allVocabularies = await processedVocabularyRepository.GetAllAsync();
            var processedVocabularies = GetVocabulariesDictionary(ExternalSystemId.Artportalen, allVocabularies?.ToArray());
            return new ArtportalenObservationFactory(dataProvider, taxa, processedVocabularies, incrementalMode, artPortalenUrl, processTimeManager);
        }

        /// <summary>
        ///     Cast verbatim observations to processed data model
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(ArtportalenObservationVerbatim verbatimObservation, bool diffuseIfSupported)
        {
            try
            {
                var diffuseFactor = verbatimObservation?.Site?.DiffusionFactor ?? 0;
                var diffuse = diffuseIfSupported && diffuseFactor > 0;
                var hasPosition = (verbatimObservation.Site?.XCoord ?? 0) > 0 &&
                                  (verbatimObservation.Site?.YCoord ?? 0) > 0;
                
                // Add time to start date if it exists
                var startDate = verbatimObservation.StartDate.HasValue && verbatimObservation.StartTime.HasValue
                    ? verbatimObservation.StartDate.Value.ToLocalTime() + verbatimObservation.StartTime
                    : verbatimObservation.StartDate.Value;

                // Add time to end date if it exists
                var endDate = verbatimObservation.EndDate.HasValue && verbatimObservation.EndTime.HasValue
                    ? verbatimObservation.EndDate.Value.ToLocalTime() + verbatimObservation.EndTime
                    : verbatimObservation.EndDate;

                var taxonId = verbatimObservation.TaxonId ?? -1;
                var taxon = GetTaxon(taxonId);

                var obs = new Observation
                {
                    DiffusionStatus = diffuse ? DiffusionStatus.DiffusedByProvider : DiffusionStatus.NotDiffused
                };

                // Record level

                obs.DataProviderId = DataProvider.Id;
                obs.AccessRights = verbatimObservation.ProtectedBySystem || verbatimObservation.HiddenByProvider.GetValueOrDefault(DateTime.MinValue) > DateTime.Now
                    ? new VocabularyValue { Id = (int)AccessRightsId.NotForPublicUsage } 
                    : new VocabularyValue {Id = (int) AccessRightsId.FreeUsage};
                obs.BasisOfRecord = string.IsNullOrEmpty(verbatimObservation.SpeciesCollection)
                    ? new VocabularyValue {Id = (int) BasisOfRecordId.HumanObservation}
                    : new VocabularyValue {Id = (int) BasisOfRecordId.PreservedSpecimen};
                obs.CollectionCode = "Artportalen";
                obs.CollectionId = null;
                obs.SpeciesCollectionLabel = verbatimObservation.CollectionID.Clean(); // todo - is verbatimObservation.CollectionID always the same as verbatimObservation.SpeciesCollection?;
                obs.DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.Artportalen}";
                obs.DatasetName = "Artportalen";
                obs.InformationWithheld = null;
                obs.Language = Language.Swedish;
                obs.Modified = verbatimObservation.EditDate.ToUniversalTime();
                obs.OwnerInstitutionCode = verbatimObservation.OwnerOrganization?.Translate(Cultures.en_GB, Cultures.sv_SE) ?? "SLU Artdatabanken";
                obs.PrivateCollection = verbatimObservation.PrivateCollection;                
                obs.Projects = verbatimObservation.Projects?.Select(ArtportalenFactoryHelper.CreateProcessedProject);
                obs.ProjectsSummary = ArtportalenFactoryHelper.CreateProjectsSummary(obs.Projects);
                obs.PublicCollection = verbatimObservation.PublicCollection?.Translate(Cultures.en_GB, Cultures.sv_SE);
                obs.RightsHolder = verbatimObservation.RightsHolder ?? verbatimObservation.OwnerOrganization?.Translate(Cultures.en_GB, Cultures.sv_SE) ?? "Data saknas";
                obs.Type = null;

                // Event
                obs.Event = new Event(startDate, endDate);
                obs.Event.EventId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.Artportalen}:event:{$"{verbatimObservation.Site?.Id ?? 0}:{obs.Event.VerbatimEventDate}".GetHashCode()}";
                obs.Event.DiscoveryMethod = GetSosIdFromMetadata(verbatimObservation?.DiscoveryMethod, VocabularyId.DiscoveryMethod);
                obs.Event.SamplingProtocol = GetSamplingProtocol(verbatimObservation);
                
                // Identification
                obs.Identification = new Identification();
                obs.Identification.ConfirmedBy = verbatimObservation.ConfirmedBy?.Clean();
                obs.Identification.ConfirmedDate = verbatimObservation.ConfirmationYear?.ToString();
                obs.Identification.DateIdentified = verbatimObservation.DeterminationYear.ToString();
                obs.Identification.IdentifiedBy = verbatimObservation.DeterminedBy;
                obs.Identification.VerifiedBy = verbatimObservation.VerifiedBy?.Clean();
                obs.Identification.Verified = obs.Identification.Validated = _validationStatusIdIds.Contains(verbatimObservation.ValidationStatus?.Id ?? 0);                
                obs.Identification.UncertainIdentification = verbatimObservation.UnsureDetermination;
                obs.Identification.IdentificationRemarks = verbatimObservation.UnsureDetermination ? "Uncertain determination" : string.Empty;
               
                // Location
                obs.Location = new Location();
                obs.Location.Attributes.CountyPartIdByCoordinate = verbatimObservation.Site?.CountyPartIdByCoordinate;
                obs.Location.Attributes.ExternalId = verbatimObservation.Site?.ExternalId;
                obs.Location.Attributes.ProjectId = verbatimObservation.Site?.ProjectId;
                obs.Location.Attributes.ProvincePartIdByCoordinate = verbatimObservation.Site?.ProvincePartIdByCoordinate;
                obs.Location.County = CastToArea(verbatimObservation.Site?.County);
                obs.Location.IsInEconomicZoneOfSweden = hasPosition;
                obs.Location.Locality = verbatimObservation.Site?.Name.Trim().Clean();
                obs.Location.LocationId = $"urn:lsid:artportalen.se:site:{verbatimObservation.Site?.Id}";
                obs.Location.MaximumDepthInMeters = verbatimObservation.MaxDepth;
                obs.Location.MaximumElevationInMeters = verbatimObservation.MaxHeight;
                obs.Location.MinimumDepthInMeters = verbatimObservation.MinDepth;
                obs.Location.MinimumElevationInMeters = verbatimObservation.MinHeight;
                obs.Location.Municipality = CastToArea(verbatimObservation.Site?.Municipality);
                obs.Location.Parish = CastToArea(verbatimObservation.Site?.Parish);
                obs.Location.Province = CastToArea(verbatimObservation.Site?.Province);
                obs.Location.VerbatimLocality = obs.Location.Locality;
                if (diffuse)
                {
                    AddPositionData(obs.Location, 
                        verbatimObservation.Site?.XCoord,
                        verbatimObservation.Site?.YCoord,
                        CoordinateSys.WebMercator,
                        (Point)verbatimObservation.Site?.DiffusedPoint?.ToGeometry(),
                        verbatimObservation.Site?.DiffusedPointWithBuffer,
                        verbatimObservation.Site?.Accuracy,
                        taxon?.Attributes?.DisturbanceRadius);
                }
                else
                {
                    AddPositionData(obs.Location, 
                        verbatimObservation.Site?.XCoord,
                        verbatimObservation.Site?.YCoord,
                        CoordinateSys.WebMercator,
                        (Point)verbatimObservation.Site?.Point?.ToGeometry(),
                        verbatimObservation.Site?.PointWithBuffer,
                        verbatimObservation.Site?.Accuracy,
                        taxon?.Attributes?.DisturbanceRadius);
                }

                // Occurrence
                obs.Occurrence = new Occurrence();
                obs.Occurrence.AssociatedMedia = verbatimObservation.HasImages
                    ? $"https://www.artportalen.se/Image/{verbatimObservation.FirstImageId}"
                    : "";
                obs.Occurrence.AssociatedReferences = GetAssociatedReferences(verbatimObservation);
                obs.Occurrence.Biotope = GetSosIdFromMetadata(verbatimObservation?.Biotope, VocabularyId.Biotope);
                obs.Occurrence.BiotopeDescription = verbatimObservation.BiotopeDescription?.Clean();
                obs.Occurrence.BirdNestActivityId = GetBirdNestActivityId(verbatimObservation, taxon);
                obs.Occurrence.CatalogNumber = verbatimObservation.SightingId.ToString();
                obs.Occurrence.CatalogId = verbatimObservation.SightingId;
                obs.Occurrence.OccurrenceId = GetOccurenceId(verbatimObservation.SightingId);
                obs.Occurrence.IndividualCount = verbatimObservation.Quantity?.ToString() ?? "";
                obs.Occurrence.IsNaturalOccurrence = !verbatimObservation.Unspontaneous;
                obs.Occurrence.IsNeverFoundObservation = verbatimObservation.NotPresent;
                obs.Occurrence.IsNotRediscoveredObservation = verbatimObservation.NotRecovered;
                obs.Occurrence.IsPositiveObservation = !(verbatimObservation.NotPresent || verbatimObservation.NotRecovered);
                obs.Occurrence.OrganismQuantityInt = verbatimObservation.Quantity;
                obs.Occurrence.OrganismQuantity = verbatimObservation.Quantity.ToString();
                obs.Occurrence.ProtectionLevel = CalculateProtectionLevel(taxon, verbatimObservation.HiddenByProvider, verbatimObservation.ProtectedBySystem);
                obs.Occurrence.SensitivityCategory = CalculateProtectionLevel(taxon, verbatimObservation.HiddenByProvider, verbatimObservation.ProtectedBySystem);
                obs.Occurrence.ReportedBy = verbatimObservation.ReportedBy;
                obs.Occurrence.ReportedDate = verbatimObservation.ReportedDate?.ToUniversalTime();
                obs.Occurrence.RecordedBy = verbatimObservation.Observers;
                obs.Occurrence.RecordNumber = verbatimObservation.Label.Clean();
                obs.Occurrence.OccurrenceRemarks = verbatimObservation.Comment?.Clean();
                obs.Occurrence.OccurrenceStatus = verbatimObservation.NotPresent || verbatimObservation.NotRecovered
                    ? new VocabularyValue {Id = (int) OccurrenceStatusId.Absent}
                    : new VocabularyValue {Id = (int) OccurrenceStatusId.Present};

                Lib.Models.Processed.Observation.Taxon substrateTaxon = null!;
                if (verbatimObservation.SubstrateSpeciesId.HasValue)
                {
                    Taxa.TryGetValue(verbatimObservation.SubstrateSpeciesId.Value, out substrateTaxon);
                }

                obs.Occurrence.Substrate = new Substrate
                {
                    Description = GetSubstrateDescription(verbatimObservation, substrateTaxon)?.Clean(),
                    Id = verbatimObservation?.Substrate?.Id,
                    Name = GetSosIdFromMetadata(verbatimObservation?.Substrate, VocabularyId.Substrate),
                    Quantity = verbatimObservation.QuantityOfSubstrate,
                    SpeciesDescription = verbatimObservation.SubstrateSpeciesDescription.Clean(),
                    SpeciesId = verbatimObservation.SubstrateSpeciesId,
                    SpeciesScientificName = substrateTaxon?.ScientificName,
                    SpeciesVernacularName = substrateTaxon?.VernacularName,
                    SubstrateDescription = verbatimObservation.SubstrateDescription?.Clean()
                };
                
                obs.Occurrence.Url = $"{_artPortalenUrl}/sighting/{verbatimObservation.SightingId}";
                obs.Occurrence.Length = verbatimObservation.Length;
                obs.Occurrence.Weight = verbatimObservation.Weight;

                if (verbatimObservation.Media?.Any() ?? false)
                {
                    obs.Occurrence.Media = verbatimObservation.Media.Select(m => new Multimedia
                    {
                        Comments = m.Comments?.Select(c => new MultimediaComment
                        {
                            Comment = c.Comment,
                            CommentBy = c.CommentBy,
                            Created = c.CommentCreated
                        }),
                        Created = m.UploadDateTime?.ToShortDateString(),
                        Format = (m.FileUri?.LastIndexOf('.') ?? -1) > 0 ? m.FileUri.Substring(m.FileUri.LastIndexOf('.')): string.Empty,
                        Identifier = GetMediaUrl(m.FileUri),
                        License = string.IsNullOrEmpty(m.CopyrightText) ? "© all rights reserved" : m.CopyrightText?.Clean(),
                        References = $"{_artPortalenUrl}/Image/{m.Id}",
                        RightsHolder = m.RightsHolder,
                        Type = m.FileType
                    }).ToList();
                }

                // Taxon
                obs.Taxon = taxon;

                // ArtportalenInternal
                obs.ArtportalenInternal = new ArtportalenInternal();
                obs.ArtportalenInternal.BirdValidationAreaIds = verbatimObservation.Site?.BirdValidationAreaIds;
                obs.ArtportalenInternal.ChecklistId = verbatimObservation.ChecklistId;
                obs.ArtportalenInternal.ConfirmationYear = verbatimObservation.ConfirmationYear;
                obs.ArtportalenInternal.DatasourceId = verbatimObservation.DatasourceId;
                obs.ArtportalenInternal.DeterminationYear = verbatimObservation.DeterminationYear;
                obs.ArtportalenInternal.FieldDiaryGroupId = verbatimObservation.FieldDiaryGroupId;
                obs.ArtportalenInternal.HasTriggeredValidationRules = verbatimObservation.HasTriggeredValidationRules;
                obs.ArtportalenInternal.HasTriggeredVerificationRules = verbatimObservation.HasTriggeredValidationRules;
                obs.ArtportalenInternal.HasAnyTriggeredValidationRuleWithWarning = verbatimObservation.HasAnyTriggeredValidationRuleWithWarning;
                obs.ArtportalenInternal.HasAnyTriggeredVerificationRuleWithWarning = verbatimObservation.HasAnyTriggeredValidationRuleWithWarning;
                obs.ArtportalenInternal.SightingSpeciesCollectionItemId = verbatimObservation.SightingSpeciesCollectionItemId;
                obs.ArtportalenInternal.SpeciesFactsIds = verbatimObservation.SpeciesFactsIds;
                obs.ArtportalenInternal.LocationExternalId = verbatimObservation.Site?.ExternalId;
                obs.ArtportalenInternal.NoteOfInterest = verbatimObservation.NoteOfInterest;
                obs.ArtportalenInternal.HasUserComments = verbatimObservation.HasUserComments;
                obs.ArtportalenInternal.ParentLocationId = verbatimObservation.Site?.ParentSiteId;
                obs.ArtportalenInternal.ParentLocality = verbatimObservation.Site?.ParentSiteName?.Trim();
                obs.ArtportalenInternal.SightingId = verbatimObservation.SightingId;
                obs.ArtportalenInternal.SightingTypeId = verbatimObservation.SightingTypeId;
                obs.ArtportalenInternal.SightingTypeSearchGroupId = verbatimObservation.SightingTypeSearchGroupId;
                obs.ArtportalenInternal.SpeciesGroupId = verbatimObservation.SpeciesGroupId;
                obs.ArtportalenInternal.RegionalSightingStateId = verbatimObservation.RegionalSightingStateId;
                obs.ArtportalenInternal.SightingPublishTypeIds = verbatimObservation.SightingPublishTypeIds;
                obs.ArtportalenInternal.ReportedByUserId = verbatimObservation.ReportedByUserId;
                obs.ArtportalenInternal.ReportedByUserServiceUserId = verbatimObservation.ReportedByUserServiceUserId;
                obs.ArtportalenInternal.ReportedByUserAlias = verbatimObservation.ReportedByUserAlias;
                obs.ArtportalenInternal.LocationPresentationNameParishRegion = verbatimObservation.Site?.PresentationNameParishRegion?.Clean();
                obs.ArtportalenInternal.OccurrenceRecordedByInternal = verbatimObservation.ObserversInternal;
                obs.ArtportalenInternal.OccurrenceVerifiedByInternal = verbatimObservation.VerifiedByInternal;
                obs.ArtportalenInternal.IncrementalHarvested = _incrementalMode;
                obs.ArtportalenInternal.SightingBarcodeURL = verbatimObservation.SightingBarcodeURL;
                obs.ArtportalenInternal.SecondHandInformation =
                    (obs.Occurrence.RecordedBy?.StartsWith("Via", StringComparison.CurrentCultureIgnoreCase) ?? false) &&
                    ((verbatimObservation.ObserversInternal == null || verbatimObservation.ObserversInternal.Count() == 0) ||
                    (verbatimObservation.ObserversInternal?.Any(oi => oi.Id == verbatimObservation.ReportedByUserId) ?? false));
                obs.ArtportalenInternal.TriggeredObservationRuleFrequencyId = verbatimObservation.FrequencyId;
                obs.ArtportalenInternal.TriggeredObservationRuleReproductionId = verbatimObservation.ReproductionId;

                // Set dependent properties
                var biotope = obs.Occurrence.Biotope?.Value;
                obs.Event.Habitat = ((biotope != null
                    ? $"{biotope}{(string.IsNullOrEmpty(obs.Occurrence.BiotopeDescription) ? "" : " # ")}{obs.Occurrence.BiotopeDescription}"
                    : obs.Occurrence.BiotopeDescription).WithMaxLength(255))?.Clean();

                // Get vocabulary mapped values
                obs.Occurrence.Sex = GetSosIdFromMetadata(verbatimObservation?.Gender, VocabularyId.Sex);
                obs.Occurrence.Activity = GetSosIdFromMetadata(verbatimObservation?.Activity, VocabularyId.Activity);
                
                obs.Identification.ValidationStatus = GetSosIdFromMetadata(verbatimObservation?.ValidationStatus, VocabularyId.VerificationStatus);
                obs.Identification.VerificationStatus = GetSosIdFromMetadata(verbatimObservation?.ValidationStatus, VocabularyId.VerificationStatus);
                obs.Occurrence.LifeStage = GetSosIdFromMetadata(verbatimObservation?.Stage, VocabularyId.LifeStage);
                obs.Occurrence.ReproductiveCondition = GetSosIdFromMetadata(verbatimObservation?.Activity, VocabularyId.ReproductiveCondition, null, true);
                obs.Occurrence.Behavior = GetSosIdFromMetadata(verbatimObservation?.Activity, VocabularyId.Behavior, null, true);
                obs.InstitutionCode = GetSosIdFromMetadata(verbatimObservation?.OwnerOrganization, VocabularyId.Institution);
                obs.InstitutionId = verbatimObservation?.OwnerOrganization == null
                    ? null
                    : $"urn:lsid:artdata.slu.se:organization:{verbatimObservation.OwnerOrganization.Id}";
                obs.Occurrence.OrganismQuantityUnit = GetSosIdFromMetadata(
                    verbatimObservation?.Unit, 
                    VocabularyId.Unit,
                    (int) UnitId.Individuals);
                obs.Identification.DeterminationMethod = GetSosIdFromMetadata(verbatimObservation?.DeterminationMethod, VocabularyId.DeterminationMethod);
                obs.MeasurementOrFacts = CreateMeasurementOrFacts(obs.Occurrence.OccurrenceId, verbatimObservation);

                // Populate generic data
                PopulateGenericData(obs);

                if (obs.ShallBeProtected())
                {
                    obs.Sensitive = true;
                    obs.Protected = true;
                }

                return obs;
            }
            catch (Exception e)
            {
                throw new Exception($"Error when processing Artportalen verbatim observation with Id={verbatimObservation.Id}, SightingId={verbatimObservation.SightingId}", e);
            }
        }

        private List<ExtendedMeasurementOrFact> CreateMeasurementOrFacts(string occurrenceId, ArtportalenObservationVerbatim verbatimObservation)
        {
            IEnumerable<Project> projects = verbatimObservation.Projects;
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
                    var emofRecord = CreateEmofRecordFromProjectParameter(occurrenceId, project, projectParameter);
                    emofCollection.Add(emofRecord);
                }
            }

            if (verbatimObservation.Length.HasValue)
            {
                var emof = new ExtendedMeasurementOrFact();
                emof.OccurrenceID = occurrenceId;
                emof.MeasurementType = "Length";
                emof.MeasurementValue = verbatimObservation.Length.ToString();
                emof.MeasurementUnit = "cm";
                emofCollection.Add(emof);
            }

            if (verbatimObservation.Weight.HasValue)
            {
                var emof = new ExtendedMeasurementOrFact();
                emof.OccurrenceID = occurrenceId;
                emof.MeasurementType = "Weight";
                emof.MeasurementValue = verbatimObservation.Weight.ToString();
                emof.MeasurementUnit = "gram";
                emofCollection.Add(emof);
            }

            if (!emofCollection.Any()) return null;
            return emofCollection;
        }

        private ExtendedMeasurementOrFact CreateEmofRecordFromProjectParameter(string occurrenceId, Project project,
            ProjectParameter projectParameter)
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
            return emof;
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
        /// Get media url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetMediaUrl(string url)
        {
            if (url?.StartsWith("http", StringComparison.CurrentCultureIgnoreCase) ?? true)
            {
                return url;
            }

            return $"{_artPortalenUrl}{(url.StartsWith('/') ? string.Empty : "/")}{url.Replace("//", "/")}";
        }

        /// <summary>
        ///     Get SOS internal Id for the id specific for the data provider.
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="vocabularyId"></param>
        /// <param name="defaultId"></param>
        /// <param name="setValueToNullIfNoMappingFound"></param>
        /// <returns></returns>
        public VocabularyValue GetSosIdFromMetadata(
            Metadata metadata,
            VocabularyId vocabularyId,
            int? defaultId = null,
            bool setValueToNullIfNoMappingFound = false)
        {
            IDictionary<object, int> sosIdByProviderValue = _vocabularyById.GetValue(vocabularyId);
            int? val = metadata?.Id;
            if (!val.HasValue || sosIdByProviderValue == null) return null;

            if (sosIdByProviderValue.TryGetValue(val.Value, out var sosId))
            {
                return new VocabularyValue { Id = sosId };
            }

            if (setValueToNullIfNoMappingFound)
            {
                return null;
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


        private string GetSamplingProtocol(ArtportalenObservationVerbatim verbatimObservation)
        {
            if ((verbatimObservation.DiscoveryMethod?.Id ?? 0) > 0)
            {
                return verbatimObservation.DiscoveryMethod.Translate(Cultures.en_GB, Cultures.sv_SE);
            }
            
            if (!verbatimObservation.Projects?.Any() ?? true) return null!;

            var project = verbatimObservation.Projects!.First();

            if (verbatimObservation.Projects!.Count() == 1)
            {
                return project?.SurveyMethod ?? project?.SurveyMethodUrl!;
            }

            var firstSurveyMethod = project.SurveyMethod;
            if (firstSurveyMethod != null && verbatimObservation.Projects!.All(p => p.SurveyMethod == firstSurveyMethod))
            {
                return firstSurveyMethod;
            }

            var firstSurveyMethodUrl = project.SurveyMethodUrl;
            if (firstSurveyMethodUrl != null && verbatimObservation.Projects!.All(p => p.SurveyMethod == firstSurveyMethodUrl))
            {
                return firstSurveyMethodUrl;
            }

            return null!;
        }

        /// <summary>
        ///     Build the substrate description string
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <param name="substrateTaxon"></param>
        /// <returns></returns>
        private string GetSubstrateDescription(ArtportalenObservationVerbatim verbatimObservation,
            Lib.Models.Processed.Observation.Taxon substrateTaxon)
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

            if (substrateTaxon != null)
            {
                substrateDescription.Append($"{(substrateDescription.Length == 0 ? "" : " # ")}{substrateTaxon.ScientificName}");
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
        public int GetBirdNestActivityId(ArtportalenObservationVerbatim verbatimObservation, Lib.Models.Processed.Observation.Taxon taxon)
        {
            if (verbatimObservation == null || taxon == null)
            {
                return 0;
            }

            if (taxon.Attributes?.OrganismGroup?.StartsWith("fåg", StringComparison.CurrentCultureIgnoreCase) ?? false)
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
        ///     Get vocabulary mappings for Artportalen.
        /// </summary>
        /// <param name="externalSystemId"></param>
        /// <param name="allVocabularies"></param>
        /// <returns></returns>
        public static IDictionary<VocabularyId, IDictionary<object, int>> GetVocabulariesDictionary(
            ExternalSystemId externalSystemId,
            ICollection<Vocabulary> allVocabularies)
        {
            var dic = new ConcurrentDictionary<VocabularyId, IDictionary<object, int>>();

            if (allVocabularies == null)
            {
                return dic;
            }

            foreach (var vocabulary in allVocabularies)
            {
                var processedVocabularies = vocabulary.ExternalSystemsMapping.FirstOrDefault(m => m.Id == externalSystemId);
                if (processedVocabularies != null)
                {
                    var mappingKey = "Id";
                    var mapping = processedVocabularies.Mappings.Single(m => m.Key == mappingKey);
                    var sosIdByValue = mapping.GetIdByValueDictionary();
                    dic.TryAdd(vocabulary.Id, sosIdByValue);
                }
            }

            return dic;
        }

        /// <summary>
        /// Get occurence id 
        /// </summary>
        /// <param name="sightingId"></param>
        /// <returns></returns>
        public static string GetOccurenceId(int sightingId) => $"urn:lsid:artportalen.se:sighting:{sightingId}";
    }
}