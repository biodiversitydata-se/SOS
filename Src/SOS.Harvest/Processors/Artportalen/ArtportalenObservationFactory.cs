using NetTopologySuite.Geometries;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Enums.Weather;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using Area = SOS.Lib.Models.Processed.Observation.Area;
using Language = SOS.Lib.Models.DarwinCore.Vocabulary.Language;
using Location = SOS.Lib.Models.Processed.Observation.Location;
using Project = SOS.Lib.Models.Verbatim.Artportalen.Project;
using ProjectParameter = SOS.Lib.Models.Verbatim.Artportalen.ProjectParameter;

namespace SOS.Harvest.Processors.Artportalen
{
    public class ArtportalenObservationFactory : ObservationFactoryBase, IObservationFactory<ArtportalenObservationVerbatim>
    { 
        private readonly IDictionary<VocabularyId, IDictionary<object, int>> _vocabularyById;
        private readonly IDictionary<int, DatasetMapping> _datasetByProjectId;
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
            if (string.IsNullOrEmpty(area?.FeatureId))
            {
                return null!;
            }

            return new Area
            {
                FeatureId = area.FeatureId,
                Name = string.IsNullOrEmpty(area.Name) ? null : area.Name // Make sure name equals null if empty. To prevent empty string and null to be handled different when aggregation on the field
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
            var taxonProtectionLevel = taxon?.Attributes?.SensitivityCategory?.Id ?? 3;

            if (hiddenByProvider || protectedBySystem)
            {
                return Math.Max(3, taxonProtectionLevel);
            }

            return 1;
        }

        /// <summary>
        /// Calculate quantity used in aggregations
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <returns></returns>
        private int? CalculateQuantityAggregation(ArtportalenObservationVerbatim verbatimObservation)
        {
            if (verbatimObservation.Quantity.HasValue)
            {
                return verbatimObservation.Quantity;
            }

            if (!verbatimObservation.NotRecovered && !verbatimObservation.NotPresent)
            {
                return 1;
            }

            if (verbatimObservation.NotRecovered || verbatimObservation.NotPresent)
            {
                return 0;
            }

            return null!;
        }

        private Weather TryGetWeather(DiaryEntry diaryEntry)
        {
            return diaryEntry == null ? null! : new Weather
            {
                AirTemperature = new Measuring
                {
                    Unit = Unit.GraderCelsius,
                    Value = diaryEntry.Temperature
                },
                Cloudiness = diaryEntry.CloudinessId.HasValue ? diaryEntry.CloudinessId.Value switch
                {
                    1 => Cloudiness.Clear0Of8,
                    2 => Cloudiness.AlmostClear1To2Av8,
                    3 => Cloudiness.PartlyClear3To5Av8,
                    4 => Cloudiness.Cloudy6To7Of8,
                    5 => Cloudiness.Overcast8Of8,
                    6 => Cloudiness.EverChanging0Till8Av8,
                    _ => null
                } : null,
                Precipitation = diaryEntry.PrecipitationId.HasValue ? diaryEntry.PrecipitationId.Value switch
                {
                    1 => Precipitation.DryWeather,
                    2 => Precipitation.LightRain,
                    3 => Precipitation.ModerateRain,
                    4 => Precipitation.HeavyRain,
                    5 => Precipitation.Showers,
                    6 => Precipitation.LightSnowfall,
                    7 => Precipitation.ModerateSnowfall,
                    8 => Precipitation.HeavySnowfall,
                    9 => Precipitation.Snowflurries,
                    10 => Precipitation.HailShowers,
                    _ => null
                } : null,
                SnowCover = diaryEntry.SnowcoverId.HasValue ? diaryEntry.SnowcoverId.Value switch
                {
                    1 => SnowCover.SnowFreeGround,
                    2 => SnowCover.ThinOrPartialSnowCoveredGround,
                    3 or 4 or 5 or 6 or 7 or 8 or 9 => SnowCover.SnowCoveredGround,
                    _ => null
                } : null,
                Visibility = diaryEntry.VisibilityId.HasValue ? diaryEntry.VisibilityId.Value switch
                {
                    1 => Visibility.VeryGood20Km,
                    2 => Visibility.Good10To20Km,
                    3 => Visibility.Moderate4To10Km,
                    4 => Visibility.Haze1To4Km,
                    5 => Visibility.Fog1Km,
                    _ => null
                } : null,
                WindDirection = diaryEntry.WindId.HasValue ? diaryEntry.WindId.Value switch
                {
                    1 => CompassDirection.North,
                    2 => CompassDirection.Northeast,
                    3 => CompassDirection.East,
                    4 => CompassDirection.Southeast,
                    5 => CompassDirection.South,
                    6 => CompassDirection.Southwest,
                    7 => CompassDirection.West,
                    8 => CompassDirection.Northwest,
                    _ => null
                } : null,
                WindStrength = diaryEntry.WindStrengthId.HasValue ? diaryEntry.WindStrengthId.Value switch
                {
                    1 => WindStrength.Calm1Ms,
                    2 => WindStrength.LightBreezeUpTo3Ms,
                    3 => WindStrength.ModerateBreeze4To7Ms,
                    4 => WindStrength.FreshBreeze8Till13Ms,
                    5 => WindStrength.NearGale14To19Ms,
                    6 => WindStrength.StrongGale20To24Ms,
                    7 => WindStrength.Storm25Till32Ms,
                    8 => WindStrength.Hurricane33Ms,
                    _ => null
                } : null
            };
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
        /// <param name="processConfiguration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArtportalenObservationFactory(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IDictionary<VocabularyId, IDictionary<object, int>> vocabularyById,
            IDictionary<int, DatasetMapping> datasetByProjectId,
            bool incrementalMode,
            string artPortalenUrl, 
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration) : base(dataProvider, taxa, processTimeManager, processConfiguration) 
        {
            _vocabularyById = vocabularyById ?? throw new ArgumentNullException(nameof(vocabularyById));
            _incrementalMode = incrementalMode;
            _artPortalenUrl = artPortalenUrl ?? throw new ArgumentNullException(nameof(artPortalenUrl));
            _datasetByProjectId = datasetByProjectId == null ? new Dictionary<int, DatasetMapping>() : datasetByProjectId;
        }

        public static async Task<ArtportalenObservationFactory> CreateAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IVocabularyRepository processedVocabularyRepository,
            IArtportalenDatasetMetadataRepository datasetRepository,
            bool incrementalMode,
            string artPortalenUrl,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration)
        {
            var allVocabularies = await processedVocabularyRepository.GetAllAsync();
            var allDatasets = await datasetRepository.GetAllAsync();
            var processedVocabularies = GetVocabulariesDictionary(ExternalSystemId.Artportalen, allVocabularies?.ToArray());
            var datasetByProjectId = CreateDatasetMappingDictionary(allDatasets);
            return new ArtportalenObservationFactory(dataProvider, taxa, processedVocabularies, datasetByProjectId, incrementalMode, artPortalenUrl, processTimeManager, processConfiguration);
        }

        private static List<DatasetMapping> GetDatasetMappings(List<ArtportalenDatasetMetadata> datasets)
        {
            List<DatasetMapping> mappings = new List<DatasetMapping>();
            foreach (var dataset in datasets)
            {
                mappings.Add(new DatasetMapping()
                {
                    DatasetIdentifier = dataset.Identifier,
                    ProjectIdsSet = dataset.Projects.Where(m => m.ApProjectId.HasValue).Select(m => m.ApProjectId.Value).ToHashSet(),
                });
            }

            return mappings;
        }

        private static Dictionary<int, DatasetMapping> CreateDatasetMappingDictionary(List<ArtportalenDatasetMetadata> datasets)
        {
            var dictionary = new Dictionary<int, DatasetMapping>();
            if (datasets == null || !datasets.Any()) return dictionary;

            var mappings = GetDatasetMappings(datasets);
            foreach (var mapping in mappings)
            {
                foreach (var projectId in mapping.ProjectIdsSet)
                {
                    dictionary.TryAdd(projectId, mapping);
                }
            }

            return dictionary;
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
                var diffuseFactor = verbatimObservation.Site?.DiffusionId ?? 0;
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
                var eventId = $"{verbatimObservation.Site?.Id ?? 0}:{$"{(startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "")}{(endDate.HasValue ? $"-{endDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}" : "")}"}:{(obs.Projects?.Any() ?? false ? string.Join(',', obs.Projects.Select(p => p.Id)) : "N/A")}".ToHash();
                obs.Event.EventId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.Artportalen}:event:{eventId}";
                obs.Event.DiscoveryMethod = GetSosIdFromMetadata(verbatimObservation.DiscoveryMethod, VocabularyId.DiscoveryMethod);
                obs.Event.SamplingProtocol = GetSamplingProtocol(verbatimObservation);
                obs.Event.Weather = TryGetWeather(verbatimObservation.DiaryEntry);

                // Identification
                obs.Identification = new Identification();
                obs.Identification.ConfirmedBy = verbatimObservation.ConfirmedBy?.Clean();
                obs.Identification.ConfirmedDate = verbatimObservation.ConfirmationYear?.ToString();
                obs.Identification.DateIdentified = verbatimObservation.DeterminationYear.ToString();
                obs.Identification.IdentifiedBy = verbatimObservation.DeterminedBy;
                obs.Identification.VerifiedBy = verbatimObservation.VerifiedBy?.Clean();
                obs.Identification.Verified = _validationStatusIdIds.Contains(verbatimObservation.ValidationStatus?.Id ?? 0);                
                obs.Identification.UncertainIdentification = verbatimObservation.UnsureDetermination;
                obs.Identification.IdentificationRemarks = verbatimObservation.UnsureDetermination ? "Uncertain determination" : string.Empty;
              
                // Location
                obs.Location = new Location(LocationType.Unknown);
                obs.Location.IsInEconomicZoneOfSweden = hasPosition;
                obs.Location.MaximumDepthInMeters = verbatimObservation.MaxDepth;
                obs.Location.MaximumElevationInMeters = verbatimObservation.MaxHeight;
                obs.Location.MinimumDepthInMeters = verbatimObservation.MinDepth;
                obs.Location.MinimumElevationInMeters = verbatimObservation.MinHeight;                

                if (verbatimObservation.Site != null)
                {
                    var site = verbatimObservation.Site;

                    obs.Location.Attributes.CountyPartIdByCoordinate = site.CountyPartIdByCoordinate;
                    obs.Location.Attributes.ExternalId = site.ExternalId;
                    obs.Location.Attributes.IsPrivate = site.IsPrivate;
                    obs.Location.Attributes.ProjectId = site.ProjectId;
                    obs.Location.Attributes.ProvincePartIdByCoordinate = site.ProvincePartIdByCoordinate;
                    obs.Location.CountryRegion = CastToArea(site.CountryRegion!);
                    obs.Location.County = CastToArea(site.County!);
                    obs.Location.Locality = site.Name.Trim().Clean();
                    obs.Location.LocationId = $"urn:lsid:artportalen.se:site:{site.Id}";
                    obs.Location.Municipality = CastToArea(site.Municipality!);
                    obs.Location.Parish = CastToArea(site.Parish!);
                    obs.Location.Province = CastToArea(site.Province!);
                    obs.Location.Type = site.HasGeometry ? LocationType.Polygon : LocationType.Point;

                    AddPositionData(
                        obs.Location,
                        site.XCoord,
                        site.YCoord,
                        CoordinateSys.WebMercator,
                        (Point) (diffuse ? site.DiffusedPoint : site.Point)?.ToGeometry()!,
                        diffuse ? site.DiffusedPointWithBuffer! : site.PointWithBuffer!,
                        site.Accuracy,
                        taxon?.Attributes?.DisturbanceRadius
                    );
                }
                obs.Location.VerbatimLocality = obs.Location.Locality;

                // Occurrence
                obs.Occurrence = new Occurrence();
                obs.Occurrence.AssociatedMedia = verbatimObservation.HasImages && verbatimObservation.FirstImageId > 0
                    ? $"https://www.artportalen.se/Image/{verbatimObservation.FirstImageId}" //verbatimObservation.Media?.FirstOrDefault(m => m.FileUri?.StartsWith("http", StringComparison.CurrentCultureIgnoreCase) ?? true).FileUri
                    : "";
                obs.Occurrence.AssociatedReferences = GetAssociatedReferences(verbatimObservation);
                obs.Occurrence.Biotope = GetSosIdFromMetadata(verbatimObservation.Biotope!, VocabularyId.Biotope);
                obs.Occurrence.BiotopeDescription = verbatimObservation.BiotopeDescription?.Clean();
                obs.Occurrence.BirdNestActivityId = GetBirdNestActivityId(verbatimObservation!, taxon!);
                obs.Occurrence.CatalogNumber = verbatimObservation.SightingId.ToString();
                obs.Occurrence.CatalogId = verbatimObservation.SightingId;
                obs.Occurrence.OccurrenceId = GetOccurenceId(verbatimObservation.SightingId);
                obs.Occurrence.IndividualCount = verbatimObservation.Quantity?.ToString() ?? "";
                obs.Occurrence.IsNaturalOccurrence = !verbatimObservation.Unspontaneous;
                obs.Occurrence.IsNeverFoundObservation = verbatimObservation.NotPresent;
                obs.Occurrence.IsNotRediscoveredObservation = verbatimObservation.NotRecovered;
                obs.Occurrence.IsPositiveObservation = !(verbatimObservation.NotPresent || verbatimObservation.NotRecovered);
                obs.Occurrence.OrganismQuantityAggregation = CalculateQuantityAggregation(verbatimObservation); 
                obs.Occurrence.OrganismQuantityInt = verbatimObservation.Quantity;
                obs.Occurrence.OrganismQuantity = verbatimObservation.Quantity.ToString();
                //obs.Occurrence.ProtectionLevel = CalculateProtectionLevel(taxon, verbatimObservation.HiddenByProvider, verbatimObservation.ProtectedBySystem);
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
                    Description = GetSubstrateDescription(verbatimObservation, substrateTaxon!)?.Clean(),
                    Id = verbatimObservation.Substrate?.Id,
                    Name = GetSosIdFromMetadata(verbatimObservation.Substrate!, VocabularyId.Substrate),
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
                            Comment = c.Comment?.Clean(),
                            CommentBy = c.CommentBy?.Clean(),
                            Created = c.CommentCreated
                        }),
                        Created = m.UploadDateTime?.ToShortDateString(),
                        Format = (m.FileUri?.LastIndexOf('.') ?? -1) > 0 ? m.FileUri.Substring(m.FileUri.LastIndexOf('.'))?.Clean(): string.Empty,
                        Identifier = GetMediaUrl(m.FileUri)?.Clean(),
                        License = string.IsNullOrEmpty(m.CopyrightText) ? "© all rights reserved" : m.CopyrightText?.Clean(),
                        References = $"{_artPortalenUrl}/Image/{m.Id}",
                        RightsHolder = m.RightsHolder?.Clean(),
                        Type = m.FileType
                    }).ToList();
                }

                // Taxon
                obs.Taxon = taxon;

                // ArtportalenInternal
                obs.ArtportalenInternal = new ArtportalenInternal();
                obs.ArtportalenInternal.ActivityCategoryId = verbatimObservation.Activity?.Category?.Id;
                obs.ArtportalenInternal.BirdValidationAreaIds = verbatimObservation.Site?.BirdValidationAreaIds;
                obs.ArtportalenInternal.ChecklistId = verbatimObservation.ChecklistId;
                obs.ArtportalenInternal.ConfirmationYear = verbatimObservation.ConfirmationYear;
                obs.ArtportalenInternal.DatasourceId = verbatimObservation.DatasourceId;
                obs.ArtportalenInternal.DeterminationYear = verbatimObservation.DeterminationYear;
                obs.ArtportalenInternal.DiffusionId = verbatimObservation.Site?.DiffusionId ?? 0;
                obs.ArtportalenInternal.FieldDiaryGroupId = verbatimObservation.FieldDiaryGroupId;
                obs.ArtportalenInternal.HasTriggeredVerificationRules = verbatimObservation.HasTriggeredValidationRules;
                obs.ArtportalenInternal.HasAnyTriggeredVerificationRuleWithWarning = verbatimObservation.HasAnyTriggeredValidationRuleWithWarning;
                obs.ArtportalenInternal.SightingSpeciesCollectionItemId = verbatimObservation.SightingSpeciesCollectionItemId;
                obs.ArtportalenInternal.SpeciesFactsIds = verbatimObservation.SpeciesFactsIds;
              // obs.ArtportalenInternal.LocationExternalId = verbatimObservation.Site?.ExternalId;
                obs.ArtportalenInternal.NoteOfInterest = verbatimObservation.NoteOfInterest;
                obs.ArtportalenInternal.HasUserComments = verbatimObservation.HasUserComments;
                obs.ArtportalenInternal.ParentLocationId = verbatimObservation.Site?.ParentSiteId;
                obs.ArtportalenInternal.ParentLocality = verbatimObservation.Site?.ParentSiteName?.Trim();
                obs.ArtportalenInternal.SightingId = verbatimObservation.SightingId;
                obs.ArtportalenInternal.SightingTypeId = verbatimObservation.SightingTypeId;
                obs.ArtportalenInternal.SightingTypeSearchGroupId = verbatimObservation.SightingTypeSearchGroupId;
               /* obs.ArtportalenInternal.SpeciesGroupId = verbatimObservation.SpeciesGroupId;
                obs.ArtportalenInternal.RegionalSightingStateId = verbatimObservation.RegionalSightingStateId;*/
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
                obs.ArtportalenInternal.Summary = verbatimObservation.Summary;
                obs.ArtportalenInternal.TriggeredObservationRuleFrequencyId = verbatimObservation.TriggeredObservationRuleFrequencyId;
                obs.ArtportalenInternal.TriggeredObservationRuleReproductionId = verbatimObservation.TriggeredObservationRuleReproductionId;
                obs.ArtportalenInternal.TriggeredObservationRuleUnspontaneous = verbatimObservation.TriggeredObservationRuleUnspontaneous;

                var eventMonths = new HashSet<int>();
                if (startDate.HasValue)
                {
                    var fromDate = startDate.Value.ToLocalTime();
                    eventMonths.Add(fromDate.Month);

                    if (endDate.HasValue)
                    {
                        var toDate = endDate.Value.ToLocalTime();
                        // Create a new date the 1 first day in month after start month since we allready have added start month
                        var currentDate = new DateTime(fromDate.Year, fromDate.Month, 1).AddMonths(1);
                        while (currentDate <= toDate)
                        {
                            eventMonths.Add(currentDate.Month);
                            currentDate = currentDate.AddMonths(1);
                        }
                    }
                }
                obs.ArtportalenInternal.EventMonths = eventMonths;

                // Set dependent properties
                var biotope = obs.Occurrence.Biotope?.Value;
                obs.Event.Habitat = ((biotope != null
                    ? $"{biotope}{(string.IsNullOrEmpty(obs.Occurrence.BiotopeDescription) ? "" : " # ")}{obs.Occurrence.BiotopeDescription}"
                    : obs.Occurrence.BiotopeDescription).WithMaxLength(255))?.Clean();

                // Get vocabulary mapped values
                obs.Occurrence.Sex = GetSosIdFromMetadata(verbatimObservation.Gender!, VocabularyId.Sex);
                obs.Occurrence.Activity = GetSosIdFromMetadata(verbatimObservation.Activity!, VocabularyId.Activity);
               
                obs.Identification.VerificationStatus = GetSosIdFromMetadata(verbatimObservation.ValidationStatus!, VocabularyId.VerificationStatus);
                obs.Occurrence.LifeStage = GetSosIdFromMetadata(verbatimObservation.Stage!, VocabularyId.LifeStage);
                obs.Occurrence.ReproductiveCondition = GetSosIdFromMetadata(verbatimObservation.Activity!, VocabularyId.ReproductiveCondition, null, true);
                obs.Occurrence.Behavior = GetSosIdFromMetadata(verbatimObservation.Activity!, VocabularyId.Behavior, null, true);
                obs.InstitutionCode = GetSosIdFromMetadata(verbatimObservation.OwnerOrganization!, VocabularyId.Institution);
                obs.InstitutionId = verbatimObservation.OwnerOrganization == null
                    ? null
                    : $"urn:lsid:artdata.slu.se:organization:{verbatimObservation.OwnerOrganization.Id}";
                obs.Occurrence.OrganismQuantityUnit = GetSosIdFromMetadata(
                    verbatimObservation.Unit, 
                    VocabularyId.Unit,
                    (int) UnitId.Individuals);
                obs.Identification.DeterminationMethod = GetSosIdFromMetadata(verbatimObservation.DeterminationMethod!, VocabularyId.DeterminationMethod);
                obs.MeasurementOrFacts = CreateMeasurementOrFacts(obs.Occurrence.OccurrenceId, verbatimObservation);

                // Populate generic data
                PopulateGenericData(obs);
                
                if (ProcessConfiguration.ProcessDataset)
                {
                    obs.DataStewardshipDatasetId = GetDataStewardshipDatasetId(obs);
                }

                if (obs.ShallBeProtected())
                {
                    obs.Sensitive = true;
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
            Metadata<int> metadata,
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
            return res!;
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
                return (verbatimObservation.Activity?.Id ?? 0) == 0 ? 1000000 : verbatimObservation.Activity!.Id;
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
                return null!;
            }

            string associatedReferences = null!;
            switch (verbatimObservation!.MigrateSightingPortalId ?? 0)
            {
                case 1:
                    associatedReferences =
                        $"urn:lsid:artportalen.se:Sighting:Bird.{verbatimObservation!.MigrateSightingObsId}";
                    break;
                case 2:
                    associatedReferences =
                        $"urn:lsid:artportalen.se:Sighting:PlantAndMushroom.{verbatimObservation!.MigrateSightingObsId}";
                    break;
                case 6:
                    associatedReferences =
                        $"urn:lsid:artportalen.se:Sighting:Vertebrate.{verbatimObservation!.MigrateSightingObsId}";
                    break;
                case 7:
                    associatedReferences =
                        $"urn:lsid:artportalen.se:Sighting:Bugs.{verbatimObservation!.MigrateSightingObsId}";
                    break;
                case 8:
                    associatedReferences =
                        $"urn:lsid:artportalen.se:Sighting:Fish.{verbatimObservation!.MigrateSightingObsId}";
                    break;
                case 9:
                    associatedReferences =
                        $"urn:lsid:artportalen.se:Sighting:MarineInvertebrates.{verbatimObservation!.MigrateSightingObsId}";
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


        protected string? GetDataStewardshipDatasetId(Observation observation)
        {            
            if (observation.Projects == null || observation.Projects.Count() == 0) return null;

            DatasetMapping? datasetMapping = null;
            foreach (var project in observation.Projects)
            {
                if (_datasetByProjectId.TryGetValue(project.Id, out datasetMapping))
                {
                    break;
                }
            }
            if (datasetMapping == null) return null;
            return datasetMapping.DatasetIdentifier;            
        }        

        public class DatasetMapping
        {
            public string DatasetIdentifier { get; set; }
            public HashSet<int> ProjectIdsSet { get; set; }
        }
    }
}