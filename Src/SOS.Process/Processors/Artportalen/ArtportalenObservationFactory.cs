using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.FieldMappingValues;
using SOS.Lib.Extensions;
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Processors.Artportalen
{
    public class ArtportalenObservationFactory
    {
        private readonly IDictionary<int, ProcessedTaxon> _taxa;
        private readonly IDictionary<FieldMappingFieldId, IDictionary<object, int>> _fieldMappings;

        public ArtportalenObservationFactory(
            IDictionary<int, ProcessedTaxon> taxa,
            IDictionary<FieldMappingFieldId, IDictionary<object, int>> fieldMappings)
        {
            {
                _taxa = taxa ?? throw new ArgumentNullException(nameof(taxa));
                _fieldMappings = fieldMappings ?? throw new ArgumentNullException(nameof(fieldMappings));
            }
        }

        public static async Task<ArtportalenObservationFactory> CreateAsync(
            IDictionary<int, ProcessedTaxon> taxa,
            IProcessedFieldMappingRepository processedFieldMappingRepository)
        {
            var allFieldMappings = await processedFieldMappingRepository.GetAllAsync();
            var fieldMappings = GetFieldMappingsDictionary(ExternalSystemId.Artportalen, allFieldMappings.ToArray());
            return new ArtportalenObservationFactory(taxa, fieldMappings);
        }

        public IEnumerable<ProcessedObservation> CreateProcessedObservations(IEnumerable<ArtportalenVerbatimObservation> verbatimObservations)
        {
            return verbatimObservations.Select(CreateProcessedObservation);
        }

        /// <summary>
        /// Cast verbatim observations to processed data model
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

            var hasPosition = (verbatimObservation.Site?.XCoord ?? 0) > 0 && (verbatimObservation.Site?.YCoord ?? 0) > 0;

            if (_taxa.TryGetValue(taxonId, out var taxon))
            {
                taxon.IndividualId = verbatimObservation.URL;
            }

            var obs = new ProcessedObservation(ObservationProvider.Artportalen)
            {
                AccessRightsId =
                !verbatimObservation.ProtectedBySystem && verbatimObservation.HiddenByProvider.HasValue &&
                verbatimObservation.HiddenByProvider.GetValueOrDefault(DateTime.MinValue) < DateTime.Now
                    ? new ProcessedFieldMapValue { Id = (int)AccessRightsId.FreeUsage }
                    : new ProcessedFieldMapValue { Id = (int)AccessRightsId.NotForPublicUsage },
                BasisOfRecordId = string.IsNullOrEmpty(verbatimObservation.SpeciesCollection)
                ? new ProcessedFieldMapValue { Id = (int)BasisOfRecordId.HumanObservation }
                : new ProcessedFieldMapValue { Id = (int)BasisOfRecordId.PreservedSpecimen },
                CollectionCode = string.IsNullOrEmpty(verbatimObservation.SpeciesCollection)
                ? "Artportalen"
                : verbatimObservation.SpeciesCollection,
                CollectionId = verbatimObservation.CollectionID,
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{ObservationProvider.Artportalen.ToString()}",
                DatasetName = "Artportalen",
                Event = new ProcessedEvent
                {
                    BiotopeDescription = verbatimObservation.BiotopeDescription,
                    EndDate = verbatimObservation.EndDate?.ToUniversalTime(),
                    QuantityOfSubstrate = verbatimObservation.QuantityOfSubstrate,
                    SamplingProtocol = GetSamplingProtocol(verbatimObservation.Projects),
                    StartDate = verbatimObservation.StartDate?.ToUniversalTime(),
                    SubstrateSpeciesDescription = verbatimObservation.SubstrateSpeciesDescription,
                    SubstrateDescription = GetSubstrateDescription(verbatimObservation, _taxa),
                    VerbatimEndDate = verbatimObservation.EndDate,
                    VerbatimStartDate = verbatimObservation.StartDate
                },
                Identification = new ProcessedIdentification
                {
                    IdentifiedBy = verbatimObservation.VerifiedBy,
                    Validated = new[] { 60, 61, 62, 63, 64, 65 }.Contains(verbatimObservation.ValidationStatus?.Id ?? 0),
                    UncertainDetermination = verbatimObservation.UnsureDetermination
                },
                InformationWithheld = null,
                IsInEconomicZoneOfSweden = hasPosition, // Artportalen validate all sightings, we rely on that validation as long it has coordinates
                Language = Language.Swedish,
                Location = new ProcessedLocation
                {
                    ContinentId = new ProcessedFieldMapValue { Id = (int)ContinentId.Europe },
                    CoordinateUncertaintyInMeters = verbatimObservation.Site?.Accuracy,
                    CountryId = new ProcessedFieldMapValue { Id = (int)CountryId.Sweden },
                    CountryCode = CountryCode.Sweden,
                    DecimalLatitude = verbatimObservation.Site?.Point?.Coordinates?.Latitude ?? 0,
                    DecimalLongitude = verbatimObservation.Site?.Point?.Coordinates?.Longitude ?? 0,
                    GeodeticDatum = GeodeticDatum.Wgs84,
                    Locality = verbatimObservation.Site?.Name,
                    Id = $"urn:lsid:artportalen.se:site:{verbatimObservation.Site?.Id}",
                    MaximumDepthInMeters = verbatimObservation.MaxDepth,
                    MaximumElevationInMeters = verbatimObservation.MaxHeight,
                    MinimumDepthInMeters = verbatimObservation.MinDepth,
                    MinimumElevationInMeters = verbatimObservation.MinHeight,
                    Point = verbatimObservation.Site?.Point,
                    PointWithBuffer = verbatimObservation.Site?.PointWithBuffer,
                    VerbatimLatitude = hasPosition ? verbatimObservation.Site.YCoord : 0,
                    VerbatimLongitude = hasPosition ? verbatimObservation.Site.XCoord : 0,
                    VerbatimCoordinateSystem = "EPSG:3857"
                },
                Modified = verbatimObservation.EndDate ?? verbatimObservation.ReportedDate,
                Occurrence = new ProcessedOccurrence
                {
                    AssociatedMedia = verbatimObservation.HasImages
                    ? $"http://www.artportalen.se/sighting/{verbatimObservation.Id}#SightingDetailImages"
                    : "",
                    AssociatedReferences = GetAssociatedReferences(verbatimObservation),
                    BirdNestActivityId = GetBirdNestActivityId(verbatimObservation, taxon),
                    CatalogNumber = verbatimObservation.Id.ToString(),
                    //EstablishmentMeansId = verbatim.Unspontaneous ? "Unspontaneous" : "Natural", // todo - "Unspontaneous" & "Natural" is not in the DwC recomended vocabulary. Get value from Dyntaxa instead?
                    Id = $"urn:lsid:artportalen.se:Sighting:{verbatimObservation.Id}",
                    IndividualCount = verbatimObservation.Quantity?.ToString() ?? "",
                    IsNaturalOccurrence = !verbatimObservation.Unspontaneous,
                    IsNeverFoundObservation = verbatimObservation.NotPresent,
                    IsNotRediscoveredObservation = verbatimObservation.NotRecovered,
                    IsPositiveObservation = !(verbatimObservation.NotPresent || verbatimObservation.NotRecovered),
                    OrganismQuantity = verbatimObservation.Quantity,
                    RecordedBy = verbatimObservation.Observers,
                    RecordNumber = verbatimObservation.Label,
                    Remarks = verbatimObservation.Comment,
                    OccurrenceStatusId = verbatimObservation.NotPresent || verbatimObservation.NotRecovered
                    ? new ProcessedFieldMapValue { Id = (int)OccurrenceStatusId.Absent }
                    : new ProcessedFieldMapValue { Id = (int)OccurrenceStatusId.Present },
                    URL = $"http://www.artportalen.se/sighting/{verbatimObservation.Id}"
                },
                OwnerInstitutionCode = verbatimObservation.OwnerOrganization?.Translate(Cultures.en_GB, Cultures.sv_SE) ?? "Artdatabanken",
                Projects = verbatimObservation.Projects?.Select(CreateProcessedProject),
                ProtectionLevel = CalculateProtectionLevel(taxon, verbatimObservation.HiddenByProvider, verbatimObservation.ProtectedBySystem),
                ReportedBy = verbatimObservation.ReportedBy,
                ReportedDate = verbatimObservation.ReportedDate,
                RightsHolder = verbatimObservation.RightsHolder ?? verbatimObservation.OwnerOrganization?.Translate(Cultures.en_GB, Cultures.sv_SE) ?? "Data saknas",
                Taxon = taxon,
                TypeId = null
            };

            // Get field mapping values
            obs.Occurrence.GenderId = GetSosId(verbatimObservation.Gender?.Id, _fieldMappings[FieldMappingFieldId.Gender]);
            obs.Occurrence.ActivityId = GetSosId(verbatimObservation.Activity?.Id, _fieldMappings[FieldMappingFieldId.Activity]);
            obs.Location.CountyId = GetSosId(verbatimObservation.Site?.County?.Id, _fieldMappings[FieldMappingFieldId.County]);
            obs.Location.MunicipalityId = GetSosId(verbatimObservation.Site?.Municipality?.Id, _fieldMappings[FieldMappingFieldId.Municipality]);
            obs.Location.ProvinceId = GetSosId(verbatimObservation.Site?.Province?.Id, _fieldMappings[FieldMappingFieldId.Province]);
            obs.Location.ParishId = GetSosId(verbatimObservation.Site?.Parish?.Id, _fieldMappings[FieldMappingFieldId.Parish]);
            obs.Event.BiotopeId = GetSosId(verbatimObservation?.Bioptope?.Id, _fieldMappings[FieldMappingFieldId.Biotope]);
            obs.Event.SubstrateId = GetSosId(verbatimObservation?.Bioptope?.Id, _fieldMappings[FieldMappingFieldId.Substrate]);
            obs.Identification.ValidationStatusId = GetSosId(verbatimObservation?.ValidationStatus?.Id, _fieldMappings[FieldMappingFieldId.ValidationStatus]);
            obs.Occurrence.LifeStageId = GetSosId(verbatimObservation?.Stage?.Id, _fieldMappings[FieldMappingFieldId.LifeStage]);
            obs.InstitutionId = GetSosId(verbatimObservation?.OwnerOrganization?.Id, _fieldMappings[FieldMappingFieldId.Institution]);
            obs.Occurrence.OrganismQuantityUnitId = GetSosId(verbatimObservation?.Unit?.Id, _fieldMappings[FieldMappingFieldId.Unit]);
            return obs;
        }

        /// <summary>
        /// Get SOS internal Id for the id specific for the data provider.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sosIdByProviderValue"></param>
        /// <returns></returns>
        private static ProcessedFieldMapValue GetSosId(int? val, IDictionary<object, int> sosIdByProviderValue)
        {
            if (!val.HasValue || sosIdByProviderValue == null) return null;

            if (sosIdByProviderValue.TryGetValue(val.Value, out var sosId))
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
                    string mappingKey = GetMappingKey(fieldMapping.Id);
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
                case FieldMappingFieldId.Unit:
                    return "Id";
                default:
                    throw new ArgumentException($"No mapping exist for the field: {fieldMappingFieldId}");
            }
        }
    }
}
