using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nest;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.FieldMappingValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Process.Repositories.Destination.Interfaces;
using FieldMapping = SOS.Lib.Models.Shared.FieldMapping;
using Language = SOS.Lib.Models.DarwinCore.Vocabulary.Language;

namespace SOS.Process.Processors.Artportalen
{
    public class ArtportalenObservationFactory
    {
        private readonly DataProvider _dataProvider;
        private readonly IDictionary<FieldMappingFieldId, IDictionary<object, int>> _fieldMappings;
        private readonly IDictionary<int, ProcessedTaxon> _taxa;

        public ArtportalenObservationFactory(
            DataProvider dataProvider,
            IDictionary<int, ProcessedTaxon> taxa,
            IDictionary<FieldMappingFieldId, IDictionary<object, int>> fieldMappings)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            {
                _taxa = taxa ?? throw new ArgumentNullException(nameof(taxa));
                _fieldMappings = fieldMappings ?? throw new ArgumentNullException(nameof(fieldMappings));
            }
        }

        public static async Task<ArtportalenObservationFactory> CreateAsync(
            DataProvider dataProvider,
            IDictionary<int, ProcessedTaxon> taxa,
            IProcessedFieldMappingRepository processedFieldMappingRepository)
        {
            var allFieldMappings = await processedFieldMappingRepository.GetAllAsync();
            var fieldMappings = GetFieldMappingsDictionary(ExternalSystemId.Artportalen, allFieldMappings.ToArray());
            return new ArtportalenObservationFactory(dataProvider, taxa, fieldMappings);
        }

        public IEnumerable<ProcessedObservation> CreateProcessedObservations(
            IEnumerable<ArtportalenVerbatimObservation> verbatimObservations)
        {
            return verbatimObservations.Select(CreateProcessedObservation);
        }

        /// <summary>
        ///     Cast verbatim observations to processed data model
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <returns></returns>
        public ProcessedObservation CreateProcessedObservation(ArtportalenVerbatimObservation verbatimObservation)
        {
            if (verbatimObservation == null)
            {
                return null;
            }

            var taxonId = verbatimObservation.TaxonId ?? -1;

            var hasPosition = (verbatimObservation.Site?.XCoord ?? 0) > 0 &&
                              (verbatimObservation.Site?.YCoord ?? 0) > 0;
            var point = (PointGeoShape) verbatimObservation.Site?.Point?.ToGeoShape();

            if (_taxa.TryGetValue(taxonId, out var taxon))
            {
                taxon.IndividualId = verbatimObservation.URL;
            }

            // Add time to start date if it exists
            var startDate = verbatimObservation.StartDate.HasValue && verbatimObservation.StartTime.HasValue
                ? verbatimObservation.StartDate.Value.ToLocalTime() + verbatimObservation.StartTime
                : verbatimObservation.StartDate.Value;

            // Add time to end date if it exists
            var endDate = verbatimObservation.EndDate.HasValue && verbatimObservation.EndTime.HasValue
                ? verbatimObservation.EndDate.Value.ToLocalTime() + verbatimObservation.EndTime
                : verbatimObservation.EndDate;

            var obs = new ProcessedObservation
            {
                DataProviderId = _dataProvider.Id,
                AccessRights =
                    !verbatimObservation.ProtectedBySystem && verbatimObservation.HiddenByProvider.HasValue &&
                    verbatimObservation.HiddenByProvider.GetValueOrDefault(DateTime.MinValue) < DateTime.Now
                        ? new ProcessedFieldMapValue {Id = (int) AccessRightsId.FreeUsage}
                        : new ProcessedFieldMapValue {Id = (int) AccessRightsId.NotForPublicUsage},
                BasisOfRecord = string.IsNullOrEmpty(verbatimObservation.SpeciesCollection)
                    ? new ProcessedFieldMapValue {Id = (int) BasisOfRecordId.HumanObservation}
                    : new ProcessedFieldMapValue {Id = (int) BasisOfRecordId.PreservedSpecimen},
                CollectionCode = string.IsNullOrEmpty(verbatimObservation.SpeciesCollection)
                    ? "Artportalen"
                    : verbatimObservation.SpeciesCollection,
                CollectionId = verbatimObservation.CollectionID,
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.Artportalen}",
                DatasetName = "Artportalen",
                Event = new ProcessedEvent
                {
                    BiotopeDescription = verbatimObservation.BiotopeDescription,
                    EndDate = endDate?.ToUniversalTime(),
                    QuantityOfSubstrate = verbatimObservation.QuantityOfSubstrate,
                    SamplingProtocol = GetSamplingProtocol(verbatimObservation.Projects),
                    StartDate = startDate?.ToUniversalTime(),
                    SubstrateSpeciesDescription = verbatimObservation.SubstrateSpeciesDescription,
                    SubstrateDescription = GetSubstrateDescription(verbatimObservation, _taxa),
                    VerbatimEventDate = DwcFormatter.CreateDateIntervalString(startDate, endDate)
                },
                Identification = new ProcessedIdentification
                {
                    IdentifiedBy = verbatimObservation.VerifiedBy,
                    IdentifiedByInternal = verbatimObservation.VerifiedByInternal,
                    Validated = new[] {60, 61, 62, 63, 64, 65}.Contains(verbatimObservation.ValidationStatus?.Id ?? 0),
                    UncertainDetermination = verbatimObservation.UnsureDetermination
                },
                InformationWithheld = null,
                IsInEconomicZoneOfSweden =
                    hasPosition, // Artportalen validate all sightings, we rely on that validation as long it has coordinates
                Language = Language.Swedish,
                Location = new ProcessedLocation
                {
                    Continent = new ProcessedFieldMapValue {Id = (int) ContinentId.Europe},
                    CoordinateUncertaintyInMeters = verbatimObservation.Site?.Accuracy,
                    Country = new ProcessedFieldMapValue {Id = (int) CountryId.Sweden},
                    CountryCode = CountryCode.Sweden,
                    DecimalLatitude = point?.Coordinates?.Latitude ?? 0,
                    DecimalLongitude = point?.Coordinates?.Longitude ?? 0,
                    GeodeticDatum = GeodeticDatum.Wgs84,
                    Locality = verbatimObservation.Site?.Name.Trim(),
                    LocationId = $"urn:lsid:artportalen.se:site:{verbatimObservation.Site?.Id}",
                    MaximumDepthInMeters = verbatimObservation.MaxDepth,
                    MaximumElevationInMeters = verbatimObservation.MaxHeight,
                    MinimumDepthInMeters = verbatimObservation.MinDepth,
                    MinimumElevationInMeters = verbatimObservation.MinHeight,
                    Point = point,
                    PointLocation = verbatimObservation.Site?.Point?.ToGeoLocation(),
                    PointWithBuffer = (PolygonGeoShape) verbatimObservation.Site?.PointWithBuffer.ToGeoShape(),
                    VerbatimLatitude = hasPosition ? verbatimObservation.Site.YCoord : 0,
                    VerbatimLongitude = hasPosition ? verbatimObservation.Site.XCoord : 0,
                    VerbatimCoordinateSystem = "EPSG:3857",
                    ParentLocationId = verbatimObservation.Site?.ParentSiteId
                },
                Modified = endDate ?? verbatimObservation.ReportedDate,
                Occurrence = new ProcessedOccurrence
                {
                    AssociatedMedia = verbatimObservation.HasImages
                        ? $"http://www.artportalen.se/sighting/{verbatimObservation.Id}#SightingDetailImages"
                        : "",
                    AssociatedReferences = GetAssociatedReferences(verbatimObservation),
                    BirdNestActivityId = GetBirdNestActivityId(verbatimObservation, taxon),
                    CatalogNumber = verbatimObservation.Id.ToString(),
                    //EstablishmentMeansId = verbatim.Unspontaneous ? "Unspontaneous" : "Natural", // todo - "Unspontaneous" & "Natural" is not in the DwC recomended vocabulary. Get value from Dyntaxa instead?
                    OccurrenceId = $"urn:lsid:artportalen.se:Sighting:{verbatimObservation.Id}",
                    IndividualCount = verbatimObservation.Quantity?.ToString() ?? "",
                    IsNaturalOccurrence = !verbatimObservation.Unspontaneous,
                    IsNeverFoundObservation = verbatimObservation.NotPresent,
                    IsNotRediscoveredObservation = verbatimObservation.NotRecovered,
                    IsPositiveObservation = !(verbatimObservation.NotPresent || verbatimObservation.NotRecovered),
                    OrganismQuantityInt = verbatimObservation.Quantity,
                    OrganismQuantity = verbatimObservation.Quantity.ToString(),
                    RecordedBy = verbatimObservation.Observers,
                    RecordedByInternal = verbatimObservation.ObserversInternal,
                    RecordNumber = verbatimObservation.Label,
                    OccurrenceRemarks = verbatimObservation.Comment,
                    OccurrenceStatus = verbatimObservation.NotPresent || verbatimObservation.NotRecovered
                        ? new ProcessedFieldMapValue {Id = (int) OccurrenceStatusId.Absent}
                        : new ProcessedFieldMapValue {Id = (int) OccurrenceStatusId.Present},
                    URL = $"http://www.artportalen.se/sighting/{verbatimObservation.Id}"
                },
                OwnerInstitutionCode =
                    verbatimObservation.OwnerOrganization?.Translate(Cultures.en_GB, Cultures.sv_SE) ?? "Artdatabanken",
                Projects = verbatimObservation.Projects?.Select(CreateProcessedProject),
                ProtectionLevel = CalculateProtectionLevel(taxon, verbatimObservation.HiddenByProvider,
                    verbatimObservation.ProtectedBySystem),
                ReportedBy = verbatimObservation.ReportedBy,
                ReportedByUserId = verbatimObservation.ReportedByUserId,
                ReportedByUserAlias = verbatimObservation.ReportedByUserAlias,
                ReportedDate = verbatimObservation.ReportedDate,
                RightsHolder = verbatimObservation.RightsHolder ??
                               verbatimObservation.OwnerOrganization?.Translate(Cultures.en_GB, Cultures.sv_SE) ??
                               "Data saknas",
                Taxon = taxon,
                Type = null
            };

            // Set dependent properties
            var biotope = obs.Event.Biotope?.Value;
            obs.Event.Habitat = (biotope != null
                ? $"{biotope}{(string.IsNullOrEmpty(obs.Event.BiotopeDescription) ? "" : " # ")}{obs.Event.BiotopeDescription}"
                : obs.Event.BiotopeDescription).WithMaxLength(255);

            // Get field mapping values
            obs.Occurrence.Gender =
                GetSosId(verbatimObservation.Gender?.Id, _fieldMappings[FieldMappingFieldId.Gender]);
            obs.Occurrence.Activity =
                GetSosId(verbatimObservation.Activity?.Id, _fieldMappings[FieldMappingFieldId.Activity]);
            obs.Location.County = GetSosId(verbatimObservation.Site?.County?.Id,
                _fieldMappings[FieldMappingFieldId.County]);
            obs.Location.Municipality = GetSosId(verbatimObservation.Site?.Municipality?.Id,
                _fieldMappings[FieldMappingFieldId.Municipality]);
            obs.Location.Province = GetSosId(verbatimObservation.Site?.Province?.Id,
                _fieldMappings[FieldMappingFieldId.Province]);
            obs.Location.Parish = GetSosId(verbatimObservation.Site?.Parish?.Id,
                _fieldMappings[FieldMappingFieldId.Parish]);
            obs.Event.Biotope =
                GetSosId(verbatimObservation?.Bioptope?.Id, _fieldMappings[FieldMappingFieldId.Biotope]);
            obs.Event.Substrate = GetSosId(verbatimObservation?.Bioptope?.Id,
                _fieldMappings[FieldMappingFieldId.Substrate]);
            obs.Identification.ValidationStatus = GetSosId(verbatimObservation?.ValidationStatus?.Id,
                _fieldMappings[FieldMappingFieldId.ValidationStatus]);
            obs.Occurrence.LifeStage =
                GetSosId(verbatimObservation?.Stage?.Id, _fieldMappings[FieldMappingFieldId.LifeStage]);
            obs.InstitutionId = GetSosId(verbatimObservation?.OwnerOrganization?.Id,
                _fieldMappings[FieldMappingFieldId.Institution]);
            obs.Occurrence.OrganismQuantityUnit = GetSosId(verbatimObservation?.Unit?.Id,
                _fieldMappings[FieldMappingFieldId.Unit],
                (int) UnitId
                    .Individuals); // todo - if verbatimObservation.Unit is null, should the value be set to "Individuals"? This is how it works in SSOS.
            return obs;
        }

        /// <summary>
        ///     Get SOS internal Id for the id specific for the data provider.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sosIdByProviderValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static ProcessedFieldMapValue GetSosId(int? val, IDictionary<object, int> sosIdByProviderValue,
            int? defaultValue = null)
        {
            if (!val.HasValue || sosIdByProviderValue == null) return null;

            if (sosIdByProviderValue.TryGetValue(val.Value, out var sosId))
            {
                return new ProcessedFieldMapValue {Id = sosId};
            }

            if (defaultValue.HasValue)
            {
                return new ProcessedFieldMapValue {Id = defaultValue.Value};
            }

            return new ProcessedFieldMapValue
                {Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId, Value = val.ToString()};
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
        ///     Calculate protection level
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
        private string GetSubstrateDescription(ArtportalenVerbatimObservation verbatimObservation,
            IDictionary<int, ProcessedTaxon> taxa)
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
        ///     Get associated references
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
        private static IDictionary<FieldMappingFieldId, IDictionary<object, int>> GetFieldMappingsDictionary(
            ExternalSystemId externalSystemId,
            ICollection<FieldMapping> allFieldMappings)
        {
            var dic = new Dictionary<FieldMappingFieldId, IDictionary<object, int>>();

            foreach (var fieldMapping in allFieldMappings)
            {
                var fieldMappings = fieldMapping.ExternalSystemsMapping.FirstOrDefault(m => m.Id == externalSystemId);
                if (fieldMappings != null)
                {
                    var mappingKey = GetMappingKey(fieldMapping.Id);
                    var mapping = fieldMappings.Mappings.Single(m => m.Key == mappingKey);
                    var sosIdByValue = mapping.GetIdByValueDictionary();
                    dic.Add(fieldMapping.Id, sosIdByValue);
                }
            }

            return dic;
        }

        private static string GetMappingKey(FieldMappingFieldId fieldMappingFieldId)
        {
            switch (fieldMappingFieldId)
            {
                case FieldMappingFieldId.Activity:
                case FieldMappingFieldId.Gender:
                case FieldMappingFieldId.County:
                case FieldMappingFieldId.Municipality:
                case FieldMappingFieldId.Parish:
                case FieldMappingFieldId.Province:
                case FieldMappingFieldId.LifeStage:
                case FieldMappingFieldId.Substrate:
                case FieldMappingFieldId.ValidationStatus:
                case FieldMappingFieldId.Biotope:
                case FieldMappingFieldId.Institution:
                case FieldMappingFieldId.AreaType:
                case FieldMappingFieldId.Unit:
                    return "Id";
                default:
                    throw new ArgumentException($"No mapping exist for the field: {fieldMappingFieldId}");
            }
        }
    }
}