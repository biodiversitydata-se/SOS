using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Models.Verbatim.INaturalist.Service;
using System.Text.RegularExpressions;
using Location = SOS.Lib.Models.Processed.Observation.Location;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Harvest.Processors.iNaturalist
{
    public class iNaturalistObservationFactory : ObservationFactoryBase, IObservationFactory<iNaturalistVerbatimObservation>
    {
        private int DefaultCoordinateUncertaintyInMeters = 5000;
        private readonly IAreaHelper _areaHelper;
        private IDictionary<string, (double longitude, double latitude, int precision)>? _communities;
        private string _englishOrganizationName;
        private string _englishOrganizationNameLowerCase;
        private VocabularyValue? _institutionCodeVocabularyValue;
        private ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processTimeManager"></param>
        /// <param name="processConfiguration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public iNaturalistObservationFactory(DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon>? taxa,
            IDictionary<VocabularyId, IDictionary<object, int>> dwcaVocabularyById,
            IAreaHelper areaHelper,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ILogger logger) : base(dataProvider, taxa, dwcaVocabularyById, processTimeManager, processConfiguration)
        {
            _logger = logger;
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _englishOrganizationName = dataProvider?.Organizations?.Translate("en-GB")!;
            _englishOrganizationNameLowerCase = _englishOrganizationName?.ToLower()!;
            if (VocabularyById != null && VocabularyById.ContainsKey(VocabularyId.Institution))
            {
                _institutionCodeVocabularyValue = GetSosId(_englishOrganizationName,
                    VocabularyById[VocabularyId.Institution],
                    null,
                    MappingNotFoundLogic.UseSourceValue,
                    _englishOrganizationNameLowerCase);
            }
            if (dataProvider != null && dataProvider.CoordinateUncertaintyInMeters > 0)
            {
                DefaultCoordinateUncertaintyInMeters = dataProvider.CoordinateUncertaintyInMeters;
            }            
        }

        public async Task InitializeAsync()
        {
            //_communities = await GetCommunitiesAsync();
        }

        /// <summary>
        /// Cast verbatim observations to processed data model
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="diffuseIfSupported"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(iNaturalistVerbatimObservation verbatim, bool diffuseIfSupported)
        {
            if (verbatim == null)
            {
                return null;
            }

            var accessRights = VocabularyValue.Create((int)AccessRightsId.FreeUsage);
            var obs = new Observation
            {
                AccessRights = accessRights,
                DataProviderId = DataProvider.Id,
                DiffusionStatus = DiffusionStatus.NotDiffused,
            };

            // Record level
            obs.License = verbatim.License_code?.Clean();
            obs.BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation };
            obs.DatasetName = "iNaturalist";
            obs.Modified = verbatim.Updated_at != null ? verbatim.Updated_at!.Value.DateTime.ToUniversalTime() : verbatim.Created_at!.Value.DateTime.ToUniversalTime();
            obs.InstitutionCode = _institutionCodeVocabularyValue;
            obs.OwnerInstitutionCode = "iNaturalist";            

            // Event
            obs.Event = CreateProcessedEvent(verbatim);

            // Identification
            obs.Identification = CreateProcessedIdentification(verbatim);

            // Taxon
            obs.Taxon = CreateProcessedTaxon(verbatim);

            // Location
            obs.Location = CreateProcessedLocation(verbatim);
            var verbatimLongitude = verbatim.Geojson.Coordinates.First();
            var verbatimLatitude = verbatim.Geojson.Coordinates.Last();

            var coordinateUncertaintyInMeters = verbatim.PublicPositionalAccuracy ?? DefaultCoordinateUncertaintyInMeters;
            AddPositionData(
                obs.Location,
                verbatimLongitude,
                verbatimLatitude,
                CoordinateSys.WGS84,
                coordinateUncertaintyInMeters,
                obs.Taxon?.Attributes?.DisturbanceRadius);
            _areaHelper.AddAreaDataToProcessedLocation(obs.Location);

            // Occurrence
            obs.Occurrence = CreateProcessedOccurrence(verbatim, obs.Taxon, obs.AccessRights != null ? (AccessRightsId)obs.AccessRights.Id : null);

            if (obs.ShallBeProtected())
            {
                obs.Sensitive = true;

                if (obs.AccessRights?.Id == (int)AccessRightsId.NotForPublicUsage && obs.Occurrence.SensitivityCategory < 3 && (obs.Taxon?.Attributes?.SensitivityCategory?.Id ?? 0) < 3)
                {
                    obs.Occurrence.SensitivityCategory = 3;
                }
            }

            // Populate generic data
            PopulateGenericData(obs);
            obs.Occurrence.BirdNestActivityId = GetBirdNestActivityId(obs.Occurrence.Activity, obs.Taxon);
            CalculateOrganismQuantity(obs);            
            //obs.AccessRights = GetAccessRightsFromSensitivityCategory(obs.Occurrence.SensitivityCategory);
            return obs;
        }

        /// <summary>
        ///     Gets the occurrence status. Set to Present if DyntaxaTaxonId from provider is greater than 0 and Absent if
        ///     DyntaxaTaxonId is 0
        /// </summary>
        private VocabularyValue GetOccurrenceStatusId(int dyntaxaTaxonId)
        {
            if (dyntaxaTaxonId == 0)
            {
                return new VocabularyValue { Id = (int)OccurrenceStatusId.Absent };
            }

            return new VocabularyValue { Id = (int)OccurrenceStatusId.Present };
        }

        /// <summary>
        ///     Set to False if DyntaxaTaxonId from provider is greater than 0 and True if DyntaxaTaxonId is 0.
        /// </summary>
        private bool GetIsNeverFoundObservation(int dyntaxaTaxonId)
        {
            return dyntaxaTaxonId == 0;
        }

        /// <summary>
        ///     Set to True if DyntaxaTaxonId from provider is greater than 0 and False if DyntaxaTaxonId is 0.
        /// </summary>
        private bool GetIsPositiveObservation(int dyntaxaTaxonId)
        {
            return dyntaxaTaxonId != 0;
        }

        public bool IsVerbatimObservationDiffusedByProvider(iNaturalistVerbatimObservation verbatim)
        {
            return false;
        }

        private ICollection<Multimedia>? CreateProcessedMultimedia(iNaturalistVerbatimObservation verbatim)            
        {
            if (verbatim.Photos == null || !verbatim.Photos.Any())
            {
                return null;
            }

            return verbatim.Photos.Select(photo => new Multimedia
            {
                Identifier = photo.Url,                
                Type = "StillImage",
                Format = "image/jpeg",
                Created = verbatim.Time_observed_at != null ? verbatim.Time_observed_at!.Value.DateTime.ToUniversalTime().ToString() :
                    verbatim.Observed_on != null ? verbatim.Observed_on!.Value.DateTime.ToUniversalTime().ToString() :
                    verbatim.Created_at!.Value.DateTime.ToUniversalTime().ToString(),
                Creator = verbatim.User.Name ?? verbatim.User.Login,
                License = photo.License_code ?? photo.Attribution,
                RightsHolder = verbatim.User.Name ?? verbatim.User.Login
            }).ToList();
        }

        private Event CreateProcessedEvent(iNaturalistVerbatimObservation verbatim)
        {
            //var observationDate = GetObservationDate(
            //    verbatim.Observed_on != null ? verbatim.Observed_on.Value.DateTime : verbatim.Created_at!.Value.DateTime,
            //    verbatim.Time_observed_at != null ? verbatim.Time_observed_at.Value.TimeOfDay : null);
            var processedEvent = new Event(verbatim.Time_observed_at != null ? verbatim.Time_observed_at.Value.DateTime : 
                verbatim.Observed_on != null ? verbatim.Observed_on.Value.DateTime : verbatim.Created_at!.Value.DateTime,
                verbatim.Time_observed_at != null ? verbatim.Time_observed_at.Value.TimeOfDay : null);
            return processedEvent;
        }

        private DateTime? GetObservationDate(
            DateTime? startDate,
            TimeSpan? startTime)
        {
            DateTime? startDateResult;

            // Add time to start date if it exists
            if (startDate.HasValue && startTime.HasValue)
            {
                startDateResult = startDate.Value.ToLocalTime().Date + startTime;
            }
            else if (startDate.HasValue && !startTime.HasValue)
            {
                startDateResult = startDate.Value.ToLocalTime().Date;
            }
            else
            {
                startDateResult = null;
            }
         
            if (startDateResult.HasValue && startDateResult.Value.Kind == DateTimeKind.Unspecified)
            {
                startDateResult = DateTime.SpecifyKind(startDateResult.Value, DateTimeKind.Local);
            }

            return startDateResult;
        }

        private Lib.Models.Processed.Observation.Identification CreateProcessedIdentification(iNaturalistVerbatimObservation verbatim)
        {
            var processedIdentification = new Lib.Models.Processed.Observation.Identification();                        
            if (verbatim.Identifications_count > 0 && verbatim.Identifications_most_agree == true && verbatim.Non_owner_ids != null)
            {
                processedIdentification.IdentifiedBy = string.Join(", ", verbatim.Non_owner_ids.Select(m => m.User.Name != null ?  m.User.Name.Clean() : m.User.Login.Clean()));
                processedIdentification.DateIdentified = verbatim.Non_owner_ids.Last().Created_at!.Value.DateTime.ToUniversalTime().ToString();
            }
            //processedIdentification.Verified = verbatim.Quality_grade == "research";

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

        private Location CreateProcessedLocation(iNaturalistVerbatimObservation verbatim)
        {
            var processedLocation = new Location(LocationType.Point);
            processedLocation.Locality = verbatim.Place_guess?.Clean();
            return processedLocation;
        }


        private Occurrence CreateProcessedOccurrence(iNaturalistVerbatimObservation verbatim, Lib.Models.Processed.Observation.Taxon? taxon, AccessRightsId? accessRightsId)
        {
            var processedOccurrence = new Occurrence();
            processedOccurrence.CatalogNumber = verbatim.ObservationId.ToString();
            processedOccurrence.OccurrenceId = $"https://www.inaturalist.org/observations/{verbatim.ObservationId}";
            processedOccurrence.ReportedDate = verbatim.Created_at!.Value.DateTime.ToUniversalTime();
            processedOccurrence.OccurrenceRemarks = verbatim.Description?.Clean();
            processedOccurrence.Url = verbatim.Uri;
            processedOccurrence.RecordedBy = processedOccurrence.ReportedBy = verbatim.User.Name?.Clean() ?? verbatim.User.Login?.Clean();            
            processedOccurrence.Media = CreateProcessedMultimedia(verbatim);
            processedOccurrence.OccurrenceStatus = VocabularyValue.Create((int)OccurrenceStatusId.Present);
            processedOccurrence.IsNaturalOccurrence = true;
            processedOccurrence.IsNeverFoundObservation = false;
            processedOccurrence.IsNotRediscoveredObservation = false;
            processedOccurrence.IsPositiveObservation = true;
            processedOccurrence.SensitivityCategory = CalculateProtectionLevel(taxon, accessRightsId);
            return processedOccurrence;
        }

        private Lib.Models.Processed.Observation.Taxon CreateProcessedTaxon(iNaturalistVerbatimObservation verbatim)
        {
            if (verbatim.Taxon == null || string.IsNullOrEmpty(verbatim.Taxon.Name))
            {
                return new Lib.Models.Processed.Observation.Taxon
                {
                    Id = -1
                };
            }

            var parsedTaxonId = -1;            
            var names = new HashSet<string>
            {
                verbatim.Taxon.Name,
                RemoveAuthorFromString(verbatim.Taxon.Name!)
            };                        

            return GetTaxon(parsedTaxonId, names, null, true, null, verbatim?.Species_guess);
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
    }
}