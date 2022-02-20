using Microsoft.Extensions.Logging;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.ArtportalenApiService;
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Services.Interfaces;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Managers
{
    public class ArtportalenApiManager : Interfaces.IArtportalenApiManager
    {
        private readonly IArtportalenApiService _artportalenApiService;
        //private readonly ITaxonManager _taxonManager;
        private readonly ILogger<ArtportalenApiManager> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenApiService"></param>        
        /// <param name="logger"></param>
        public ArtportalenApiManager(
            IArtportalenApiService artportalenApiService,
            //ITaxonManager taxonManager,
            ILogger<ArtportalenApiManager> logger)
        {
            _artportalenApiService = artportalenApiService ?? throw new ArgumentNullException(nameof(artportalenApiService));
            //_taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Observation> GetObservationAsync(string occurrenceId)
        {
            var sightingId = ParseSightingId(occurrenceId);
            if (sightingId == null) return null;
            return await GetObservationAsync(sightingId.Value);
        }

        private int? ParseSightingId(string occurrenceId)
        {
            if (occurrenceId == null) return null;
            if (occurrenceId.ToLower().StartsWith("urn:lsid:artportalen.se:sighting:"))
            {                
                string lastInteger = Regex.Match(occurrenceId, @"\d+", RegexOptions.RightToLeft).Value;
                if (int.TryParse(lastInteger, out var sightingId))
                {
                    return sightingId;
                }
            }

            return null;
        }

        public async Task<Observation> GetObservationAsync(int sightingId)
        {
            var sighting = await _artportalenApiService.GetSightingByIdAsync(67611684);
            var observation = await ConvertToObservationAsync(sighting);
            return observation;
        }

        private async Task<Observation> ConvertToObservationAsync(SightingOutput sighting)
        {
            // todo - create emof for project parameters, weight and length
            // todo - add project information
            // todo - convert coordinates
            // todo - Get taxon information from SOS MongoDB Taxon collection.

            /* Missing information in Artportalen API
             * Record level
             * ============
             * - OwnerOrganization
             * - SpeciesCollection
             * - CollectionID
             * - EditDate
             * - PrivateCollection
             * - PublicCollection
             * - RightsHolder
             * 
             * Occurrence
             * ==========
             * - ReportedBy (is this sighting.Owner?)
             * - ReportedDate
             * - MigrateSightingObsId
             * - MigrateSightingPortalId
             * - Label
             * - UnitId
             * 
             * Location
             * ========
             * - County FeatureId
             * - Municipality FeatureId
             * - Province FeatureId
             * - Parish FeatureId
             * 
             * Identification
             * =============
             * - ConfirmedBy
             * - ConfirmationYear
             * - DeterminationMethodId
             * - DeterminationYear
             * - DeterminedBy
             * - VerifiedBy
             * - ValidationStatusId 
             * 
             * ArtportalenInternal
             * ===================
             * - BirdValidationAreaIds;            
             * - DatasourceId;
             * - HasAnyTriggeredValidationRuleWithWarning
             * - SightingSpeciesCollectionItemId
             * - SpeciesFactsIds
             * - NoteOfInterest
             * - ParentSiteId
             * - ParentSiteName
             * - SightingTypeId
             * - SightingTypeSearchGroupId
             * - RegionalSightingStateId
             * - SightingPublishTypeIds
             * - ReportedByUserId
             * - ReportedByUserServiceUserId
             * - ReportedByUserAlias
             * - PresentationNameParishRegion
             * - ObserversInternal
             * - VerifiedByInternal
             * - SightingBarcodeURL
             * - FrequencyId
             * - ReproductionId            
            */
            var obs = new Observation();

            // todo - Get taxon information from SOS MongoDB Taxon collection.
            var taxon = new Lib.Models.Processed.Observation.Taxon();
            taxon = new Lib.Models.Processed.Observation.Taxon();
            taxon.Id = sighting.TaxonId;
            taxon.ScientificNameAuthorship = sighting.Taxon.Auctor;
            taxon.VernacularName = sighting.Taxon.Name;
            taxon.ScientificName = sighting.Taxon.ScientificName;
            taxon.Attributes = new TaxonAttributes();
            taxon.Attributes.DyntaxaTaxonId = sighting.Taxon.Id;
            taxon.Attributes.ProtectionLevel = VocabularyValue.Create(sighting.Taxon.ProtectionLevelId);
            taxon.Attributes.SensitivityCategory = VocabularyValue.Create(sighting.Taxon.ProtectionLevelId);
            obs.Taxon = taxon;

            // Record level
            obs.DataProviderId = 1;
            obs.AccessRights = sighting.ProtectedBySystem || sighting.HiddenByProvider.GetValueOrDefault(DateTime.MinValue) > DateTime.Now
                ? new VocabularyValue { Id = (int)AccessRightsId.NotForPublicUsage }
                : new VocabularyValue { Id = (int)AccessRightsId.FreeUsage };
            obs.CollectionCode = "Artportalen";
            obs.CollectionId = null;
            obs.DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.Artportalen}";
            obs.DatasetName = "Artportalen";
            obs.InformationWithheld = null;
            obs.IsInEconomicZoneOfSweden = true;
            obs.Language = Language.Swedish;
            obs.Type = null;
            //obs.MeasurementOrFacts = CreateMeasurementOrFacts(obs.Occurrence.OccurrenceId, verbatimObservation); // todo

            //obs.BasisOfRecord = string.IsNullOrEmpty(verbatimObservation.SpeciesCollection)
            //    ? new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation }
            //    : new VocabularyValue { Id = (int)BasisOfRecordId.PreservedSpecimen };
            //obs.SpeciesCollectionLabel = verbatimObservation.CollectionID; // todo - is verbatimObservation.CollectionID always the same as verbatimObservation.SpeciesCollection?;
            //obs.Modified = verbatimObservation.EditDate.ToUniversalTime();
            //obs.OwnerInstitutionCode = verbatimObservation.OwnerOrganization?.Translate(Cultures.en_GB, Cultures.sv_SE) ?? "SLU Artdatabanken";
            //obs.PrivateCollection = verbatimObservation.PrivateCollection;
            //obs.Projects = verbatimObservation.Projects?.Select(CreateProcessedProject);
            //obs.PublicCollection = verbatimObservation.PublicCollection?.Translate(Cultures.en_GB, Cultures.sv_SE);
            //obs.RightsHolder = verbatimObservation.RightsHolder ?? verbatimObservation.OwnerOrganization?.Translate(Cultures.en_GB, Cultures.sv_SE) ?? "Data saknas";
            //obs.InstitutionCode = GetSosIdFromMetadata(verbatimObservation?.OwnerOrganization, VocabularyId.Institution);
            //obs.InstitutionId = verbatimObservation?.OwnerOrganization == null
            //    ? null
            //    : $"urn:lsid:artdata.slu.se:organization:{verbatimObservation.OwnerOrganization.Id}";
            


            // Occurrence
            obs.Occurrence = new Occurrence();
            obs.Occurrence.Activity = VocabularyValue.Create(sighting.ActivityId);
            obs.Occurrence.Biotope = VocabularyValue.Create(sighting.BiotopeId);
            obs.Occurrence.BiotopeDescription = sighting.BiotopeDescription;
            obs.Occurrence.OccurrenceId = $"urn:lsid:artportalen.se:sighting:{sighting.Id}";
            obs.Occurrence.CatalogId = sighting.Id;
            obs.Occurrence.CatalogNumber = sighting.Id.ToString();
            obs.Occurrence.Sex = VocabularyValue.Create(sighting.GenderId);

            if (sighting.HasImages)
            {
                var media = await _artportalenApiService.GetMediaBySightingIdAsync(sighting.Id);
                obs.Occurrence.AssociatedMedia = media?.First()?.FileUrl; // todo - should be OriginalFileUrl or FileUrl?
                obs.Occurrence.Media = media.Select(m => new Multimedia
                {
                    Created = m.Created.ToShortDateString(),
                    Format = System.IO.Path.GetExtension(m.FileUrl), // m.MediaFileTypeName,
                    Identifier = m.FileUrl, // todo - should be OriginalFileUrl or FileUrl?
                    License = "© all rights reserved",
                    References = m.FileUrl, // todo - should be OriginalFileUrl or FileUrl?
                    RightsHolder = m.UserName,
                    Type = m.MediaFileTypeName
                }).ToList();                
            }

            obs.Occurrence.Length = sighting.Length;
            obs.Occurrence.Weight = sighting.Weight;
            obs.Occurrence.IsNaturalOccurrence = !sighting.Unspontaneous;
            obs.Occurrence.IsNeverFoundObservation = sighting.NotPresent;
            obs.Occurrence.IsNotRediscoveredObservation = sighting.NotRecovered;
            obs.Occurrence.IsPositiveObservation = !(sighting.NotPresent || sighting.NotRecovered);
            
            obs.Occurrence.ProtectionLevel = CalculateProtectionLevel(taxon, sighting.HiddenByProvider, sighting.ProtectedBySystem);
            obs.Occurrence.SensitivityCategory = CalculateProtectionLevel(taxon, sighting.HiddenByProvider, sighting.ProtectedBySystem);
            obs.Occurrence.OccurrenceRemarks = sighting.PublicComment;
            obs.Occurrence.ReportedBy = sighting.Owner; // todo - is this correct?
            obs.Occurrence.BirdNestActivityId = GetBirdNestActivityId(sighting.ActivityId, taxon);
            obs.Occurrence.IndividualCount = sighting.Quantity.ToString() ?? "";
            obs.Occurrence.OrganismQuantityInt = sighting.Quantity;
            obs.Occurrence.OrganismQuantity = sighting.Quantity.ToString();
            obs.Occurrence.RecordedBy = sighting.SightingObservers;
            obs.Occurrence.OccurrenceStatus = sighting.NotPresent || sighting.NotRecovered
                ? new VocabularyValue { Id = (int)OccurrenceStatusId.Absent }
                : new VocabularyValue { Id = (int)OccurrenceStatusId.Present };

            Lib.Models.Processed.Observation.Taxon substrateTaxon = null;
            if (sighting.SubstrateSpeciesId.HasValue)
            {
                // todo - get substrate taxon
                //if (_taxonManager.TaxonTree.TreeNodeById.TryGetValue(sighting.SubstrateSpeciesId.Value, out var treeNode))
                //{
                //    substrateTaxon = treeNode.
                //}                
            }

            obs.Occurrence.Substrate = new Substrate
            {
                //Description = GetSubstrateDescription(sighting, substrateTaxon), // todo
                Id = sighting.SubstrateId,
                //Name = GetSosIdFromMetadata(sighting.SubstrateId, VocabularyId.Substrate), // todo
                Quantity = sighting.QuantityOfSubstrate,
                SpeciesDescription = sighting.SubstrateSpeciesDescription,
                SpeciesId = sighting.SubstrateSpeciesId,
                SpeciesScientificName = substrateTaxon?.ScientificName,
                SpeciesVernacularName = substrateTaxon?.VernacularName,
                SubstrateDescription = sighting.SubstrateDescription
            };

            obs.Occurrence.LifeStage = VocabularyValue.Create(sighting.StageId); // todo - use vocabulary dictionary for correct mapping.
            obs.Occurrence.Activity = VocabularyValue.Create(sighting.ActivityId); // todo - use vocabulary dictionary for correct mapping.
            //obs.Occurrence.LifeStage = GetSosIdFromMetadata(verbatimObservation?.Stage, VocabularyId.LifeStage); // todo - use vocabulary dictionary for correct mapping.
            //obs.Occurrence.ReproductiveCondition = GetSosIdFromMetadata(verbatimObservation?.Activity, VocabularyId.ReproductiveCondition, null, true); // todo - use vocabulary dictionary for correct mapping.
            //obs.Occurrence.Behavior = GetSosIdFromMetadata(verbatimObservation?.Activity, VocabularyId.Behavior, null, true); // todo - use vocabulary dictionary for correct mapping.
            //obs.Occurrence.Url = $"{_artPortalenUrl}/sighting/{sighting.Id}"; // todo - add _artportalenUrl to class
            //obs.Occurrence.ReportedDate = verbatimObservation.ReportedDate?.ToUniversalTime();
            //obs.Occurrence.AssociatedReferences = GetAssociatedReferences(verbatimObservation);
            //obs.Occurrence.RecordNumber = verbatimObservation.Label;
            obs.Occurrence.OrganismQuantityUnit = VocabularyValue.Create(sighting.Unit); // todo
            //obs.Occurrence.OrganismQuantityUnit = GetSosIdFromMetadata(
            //        verbatimObservation?.Unit,
            //        VocabularyId.Unit,
            //        (int)UnitId.Individuals);

            // Event
            obs.Event = new Event();
            obs.Event.DiscoveryMethod = VocabularyValue.Create(sighting.DiscoveryMethod); // todo - Artportalen API is missing DiscoveryMethodId
            obs.Event.StartDate = sighting.StartDate;
            obs.Event.EndDate = sighting.EndDate;
            obs.Event.PlainStartDate =  sighting.StartDate.ToLocalTime().ToString("yyyy-MM-dd");
            obs.Event.PlainEndDate = sighting.EndDate.ToLocalTime().ToString("yyyy-MM-dd");
            obs.Event.PlainStartTime = sighting.StartDate.TimeOfDay.ToString("hh\\:mm");
            obs.Event.PlainEndTime = sighting.EndDate.TimeOfDay.ToString("hh\\:mm");
            obs.Event.VerbatimEventDate = DwcFormatter.CreateDateIntervalString(sighting.StartDate.ToLocalTime(), sighting.EndDate.ToLocalTime());
            //obs.Event.SamplingProtocol = GetSamplingProtocol(verbatimObservation.Projects); // todo - implement
            obs.Event.SamplingProtocol = sighting.DiscoveryMethod;

            // Location
            obs.Location = new Location();
            obs.Location.MaximumDepthInMeters = sighting.MaxDepth;
            obs.Location.MinimumDepthInMeters = sighting.MinDepth;
            obs.Location.MaximumElevationInMeters = sighting.MaxHeight;
            obs.Location.MinimumElevationInMeters = sighting.MinHeight;
            obs.Location.County = new Area { FeatureId = null, Name = sighting.Site?.Lan };
            obs.Location.Locality = sighting.Site?.Name;
            obs.Location.LocationId = $"urn:lsid:artportalen.se:site:{sighting.Site?.Id}";
            obs.Location.Municipality = new Area { FeatureId = null, Name = sighting.Site?.Kommun };
            obs.Location.Parish = new Area { FeatureId = null, Name = sighting.Site?.Socken };
            obs.Location.Province = new Area { FeatureId = null, Name = sighting.Site?.Landskap };
            //obs.Location.Attributes.CountyPartIdByCoordinate = verbatimObservation.Site?.CountyPartIdByCoordinate;
            //obs.Location.Attributes.ProvincePartIdByCoordinate = verbatimObservation.Site?.ProvincePartIdByCoordinate;

            obs.Location.Continent = new VocabularyValue { Id = (int)ContinentId.Europe };
            obs.Location.CoordinateUncertaintyInMeters = sighting.Site.Accuracy;
            obs.Location.Country = new VocabularyValue { Id = (int)CountryId.Sweden };
            obs.Location.CountryCode = CountryCode.Sweden;
            obs.Location.GeodeticDatum = CoordinateSys.WGS84.EpsgCode();
            var wgs84Coordinate = sighting.Site.Coordinates.Single(m => m.CoordinateSystemId == 10);
            var sweref99TimCoordinate = sighting.Site.Coordinates.Single(m => m.CoordinateSystemId == 20);
            obs.Location.DecimalLongitude = wgs84Coordinate.Easting;
            obs.Location.DecimalLatitude = wgs84Coordinate.Northing;
            obs.Location.Sweref99TmX = sweref99TimCoordinate.Easting;
            obs.Location.Sweref99TmY = sweref99TimCoordinate.Northing;
            obs.Location.VerbatimSRS = "EPSG:3006";
            obs.Location.VerbatimLongitude = sweref99TimCoordinate.Easting.ToString(CultureInfo.InvariantCulture);
            obs.Location.VerbatimLatitude = sweref99TimCoordinate.Northing.ToString(CultureInfo.InvariantCulture);

            // Identification
            obs.Identification = new Identification();
            obs.Identification.UncertainIdentification = sighting.UnsureDetermination;
            obs.Identification.IdentificationRemarks = sighting.UnsureDetermination ? "Uncertain determination" : string.Empty;

            //obs.Identification.DeterminationMethod = GetSosIdFromMetadata(verbatimObservation?.DeterminationMethod, VocabularyId.DeterminationMethod);
            //obs.Identification.ValidationStatus = GetSosIdFromMetadata(verbatimObservation?.ValidationStatus, VocabularyId.VerificationStatus);
            //obs.Identification.VerificationStatus = GetSosIdFromMetadata(verbatimObservation?.ValidationStatus, VocabularyId.VerificationStatus);
            //obs.Identification.ConfirmedBy = verbatimObservation.ConfirmedBy;
            //obs.Identification.ConfirmedDate = verbatimObservation.ConfirmationYear?.ToString();
            //obs.Identification.DateIdentified = verbatimObservation.DeterminationYear.ToString();
            //obs.Identification.IdentifiedBy = verbatimObservation.DeterminedBy;
            //obs.Identification.VerifiedBy = verbatimObservation.VerifiedBy;
            //obs.Identification.Validated = new[]
            //{
            //        (int)ValidationStatusId.ApprovedBasedOnReportersDocumentation,
            //        (int)ValidationStatusId.ApprovedSpecimenCheckedByValidator,
            //        (int)ValidationStatusId.ApprovedBasedOnImageSoundOrVideoRecording,
            //        (int)ValidationStatusId.ApprovedBasedOnReportersRarityForm,
            //        (int)ValidationStatusId.ApprovedBasedOnDeterminatorsVerification,
            //        (int)ValidationStatusId.ApprovedBasedOnReportersOldRarityForm,
            //    }.Contains(verbatimObservation.ValidationStatus?.Id ?? 0);
            //obs.Identification.Verified = new[]
            //{
            //        (int)ValidationStatusId.ApprovedBasedOnReportersDocumentation,
            //        (int)ValidationStatusId.ApprovedSpecimenCheckedByValidator,
            //        (int)ValidationStatusId.ApprovedBasedOnImageSoundOrVideoRecording,
            //        (int)ValidationStatusId.ApprovedBasedOnReportersRarityForm,
            //        (int)ValidationStatusId.ApprovedBasedOnDeterminatorsVerification,
            //        (int)ValidationStatusId.ApprovedBasedOnReportersOldRarityForm,
            //    }.Contains(verbatimObservation.ValidationStatus?.Id ?? 0);

            // ArtportalenInternal
            obs.ArtportalenInternal = new ArtportalenInternal();
            obs.ArtportalenInternal.HasTriggeredValidationRules = sighting.RuleValidationMessages != null && sighting.RuleValidationMessages.Count > 0;
            obs.ArtportalenInternal.HasTriggeredVerificationRules = sighting.RuleValidationMessages != null && sighting.RuleValidationMessages.Count > 0;
            obs.ArtportalenInternal.LocationExternalId = sighting.Site.ExternalId;
            obs.ArtportalenInternal.HasUserComments = !string.IsNullOrEmpty(sighting.PublicComment);
            obs.ArtportalenInternal.SightingId = sighting.Id;
            obs.ArtportalenInternal.SpeciesGroupId = sighting.Taxon.SpeciesGroupId;
                        
            //obs.ArtportalenInternal.BirdValidationAreaIds = verbatimObservation.Site?.BirdValidationAreaIds;
            //obs.ArtportalenInternal.ConfirmationYear = verbatimObservation.ConfirmationYear;
            //obs.ArtportalenInternal.DatasourceId = verbatimObservation.DatasourceId;
            //obs.ArtportalenInternal.DeterminationYear = verbatimObservation.DeterminationYear;            
            //obs.ArtportalenInternal.HasAnyTriggeredValidationRuleWithWarning = verbatimObservation.HasAnyTriggeredValidationRuleWithWarning;
            //obs.ArtportalenInternal.HasAnyTriggeredVerificationRuleWithWarning = verbatimObservation.HasAnyTriggeredValidationRuleWithWarning;
            //obs.ArtportalenInternal.SightingSpeciesCollectionItemId = verbatimObservation.SightingSpeciesCollectionItemId;
            //obs.ArtportalenInternal.SpeciesFactsIds = verbatimObservation.SpeciesFactsIds;            
            //obs.ArtportalenInternal.NoteOfInterest = verbatimObservation.NoteOfInterest;            
            //obs.ArtportalenInternal.ParentLocationId = verbatimObservation.Site?.ParentSiteId;
            //obs.ArtportalenInternal.ParentLocality = verbatimObservation.Site?.ParentSiteName?.Trim();            
            //obs.ArtportalenInternal.SightingTypeId = verbatimObservation.SightingTypeId;
            //obs.ArtportalenInternal.SightingTypeSearchGroupId = verbatimObservation.SightingTypeSearchGroupId;            
            //obs.ArtportalenInternal.RegionalSightingStateId = verbatimObservation.RegionalSightingStateId;
            //obs.ArtportalenInternal.SightingPublishTypeIds = verbatimObservation.SightingPublishTypeIds;
            //obs.ArtportalenInternal.ReportedByUserId = verbatimObservation.ReportedByUserId;
            //obs.ArtportalenInternal.ReportedByUserServiceUserId = verbatimObservation.ReportedByUserServiceUserId;
            //obs.ArtportalenInternal.ReportedByUserAlias = verbatimObservation.ReportedByUserAlias;
            //obs.ArtportalenInternal.LocationPresentationNameParishRegion = verbatimObservation.Site?.PresentationNameParishRegion;
            //obs.ArtportalenInternal.OccurrenceRecordedByInternal = verbatimObservation.ObserversInternal;
            //obs.ArtportalenInternal.OccurrenceVerifiedByInternal = verbatimObservation.VerifiedByInternal;
            //obs.ArtportalenInternal.IncrementalHarvested = _incrementalMode;
            //obs.ArtportalenInternal.SightingBarcodeURL = verbatimObservation.URL;
            //obs.ArtportalenInternal.SecondHandInformation =
            //    (obs.Occurrence.RecordedBy?.StartsWith("Via", StringComparison.CurrentCultureIgnoreCase) ?? false) &&
            //    (verbatimObservation.ObserversInternal?.Any(oi => oi.Id == verbatimObservation.ReportedByUserId) ?? false);
            //obs.ArtportalenInternal.TriggeredObservationRuleFrequencyId = verbatimObservation.FrequencyId;
            //obs.ArtportalenInternal.TriggeredObservationRuleReproductionId = verbatimObservation.ReproductionId;


            return obs;
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
        ///     Get bird nest activity id
        /// </summary>
        /// <param name="activityId"></param>
        /// <param name="taxon"></param>
        /// <returns></returns>
        public int GetBirdNestActivityId(int? activityId, Lib.Models.Processed.Observation.Taxon taxon)
        {            
            if (taxon.Attributes?.OrganismGroup?.StartsWith("fåg", StringComparison.CurrentCultureIgnoreCase) ?? false)
            {
                return activityId.GetValueOrDefault(0) == 0 ? 1000000 : activityId.Value;
            }

            return 0;
        }

    }
}
