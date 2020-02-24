using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Verbatim.SpeciesPortal;

namespace SOS.Process.Extensions
{
    /// <summary>
    /// Entity extensions
    /// </summary>
    public static class SpeciesPortalExtensions
    {
        /// <summary>
        /// Cast multiple sightings entities to models 
        /// </summary>
        /// <param name="verbatims"></param>
        /// <param name="taxa"></param>
        /// <param name="fieldMappings"></param>
        /// <returns></returns>
        public static ICollection<ProcessedSighting> ToProcessed(
            this IEnumerable<APSightingVerbatim> verbatims,
            IDictionary<int, ProcessedTaxon> taxa,
            IDictionary<FieldMappingFieldId, IDictionary<object, int>> fieldMappings)
        {
            return verbatims.Select(v => v.ToProcessed(taxa, fieldMappings)).ToArray();
        }

        /// <summary>
        /// Cast sighting verbatim to processed data model
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="taxa"></param>
        /// <param name="fieldMappings"></param>
        /// <returns></returns>
        public static ProcessedSighting ToProcessed(
            this APSightingVerbatim verbatim, 
            IDictionary<int, ProcessedTaxon> taxa,
            IDictionary<FieldMappingFieldId, IDictionary<object, int>> fieldMappings)
        {
            if (verbatim == null)
            {
                return null;
            }

            var taxonId = verbatim.TaxonId ?? -1;

            var hasPosition = (verbatim.Site?.XCoord ?? 0) > 0 && (verbatim.Site?.YCoord ?? 0) > 0;
            
            if (taxa.TryGetValue(taxonId, out var taxon))
            {
                taxon.IndividualId = verbatim.URL;
            }

            var obs = new ProcessedSighting(DataProvider.Artdatabanken)
            {
                AccessRights =
                    !verbatim.ProtectedBySystem &&
                    verbatim.HiddenByProvider.GetValueOrDefault(DateTime.MinValue) < DateTime.Now
                        ? AccessRights.FreeUsage
                        : AccessRights.NotForPublicUsage,
                BasisOfRecord = string.IsNullOrEmpty(verbatim.SpeciesCollection)
                    ? BasisOfRecord.HumanObservation
                    : BasisOfRecord.PreservedSpecimen,
                CollectionCode = string.IsNullOrEmpty(verbatim.SpeciesCollection)
                    ? "Artportalen"
                    : verbatim.SpeciesCollection,
                CollectionId = verbatim.CollectionID,
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProvider.Artdatabanken.ToString()}",
                DatasetName = "Artportalen",
                Event = new ProcessedEvent
                {
                    Biotope = verbatim.Bioptope,
                    BiotopeDescription = verbatim.BiotopeDescription,
                    EndDate = verbatim.EndDate?.ToUniversalTime(),
                    QuantityOfSubstrate = verbatim.QuantityOfSubstrate,
                    SamplingProtocol = GetSamplingProtocol(verbatim.Projects),
                    StartDate = verbatim.StartDate?.ToUniversalTime(),
                    Substrate = verbatim.Substrate,
                    SubstrateSpeciesDescription = verbatim.SubstrateSpeciesDescription,
                    SubstrateDescription = GetSubstrateDescription(verbatim, taxa),
                    VerbatimEndDate = verbatim.EndDate,
                    VerbatimStartDate = verbatim.StartDate
                },
                Identification = new ProcessedIdentification
                {
                    IdentifiedBy = verbatim.VerifiedBy,
                    Validated = new []{ 60, 61, 62, 63, 64, 65 }.Contains(verbatim.ValidationStatus?.Id ?? 0),
                    VerificationStatus = verbatim.ValidationStatus,
                    
                    UncertainDetermination = verbatim.UnsureDetermination 
                },
                InformationWithheld = "More information can be obtained from the Data Provider",
                Institution = verbatim.OwnerOrganization,
                IsInEconomicZoneOfSweden = hasPosition, // Species portal validate all sightings, we rely on that validation as long it has coordinates
                Language = Language.Swedish,
                Location = new ProcessedLocation
                {
                    Continent = Continent.Europe,
                    CoordinateUncertaintyInMeters = verbatim.Site?.Accuracy,
                    Country = Country.Sweden,
                    CountryCode = CountryCode.Sweden,
                    County = verbatim.Site?.County?.ToProcessed(),
                    DecimalLatitude = verbatim.Site?.Point?.Coordinates?.Latitude ?? 0,
                    DecimalLongitude = verbatim.Site?.Point?.Coordinates?.Longitude ?? 0,
                    GeodeticDatum = GeodeticDatum.Wgs84,
                    Locality = verbatim.Site?.Name,
                    Id = $"urn:lsid:artportalen.se:site:{verbatim.Site?.Id}",
                    MaximumDepthInMeters = verbatim.MaxDepth,
                    MaximumElevationInMeters = verbatim.MaxHeight,
                    MinimumDepthInMeters = verbatim.MinDepth,
                    MinimumElevationInMeters = verbatim.MinHeight,
                    Municipality = verbatim.Site?.Municipality?.ToProcessed(),
                    Parish = verbatim.Site?.Parish?.ToProcessed(),
                    Point = verbatim.Site?.Point,
                    PointWithBuffer = verbatim.Site?.PointWithBuffer,
                    Province = verbatim.Site?.Province?.ToProcessed(),
                    VerbatimLatitude = hasPosition ? verbatim.Site.YCoord : 0,
                    VerbatimLongitude = hasPosition ? verbatim.Site.XCoord : 0,
                    VerbatimCoordinateSystem = "EPSG:3857"
                },
                Modified = verbatim.EndDate ?? verbatim.ReportedDate,
                Occurrence = new ProcessedOccurrence
                {
                    Activity = verbatim.Activity,
                    ActivityId = GetSosLookupId(verbatim.Activity?.Id, fieldMappings[FieldMappingFieldId.Activity]),
                    AssociatedMedia = verbatim.HasImages
                        ? $"http://www.artportalen.se/sighting/{verbatim.Id}#SightingDetailImages"
                        : "",
                    AssociatedReferences = GetAssociatedReferences(verbatim),
                    BirdNestActivityId = GetBirdNestActivityId(verbatim, taxon), 
                    CatalogNumber = verbatim.Id.ToString(),
                    EstablishmentMeans = verbatim.Unspontaneous ? "Unspontaneous" : "Natural", // todo - "Unspontaneous" & "Natural" is not in the DwC recomended vocabulary. Get value from Dyntaxa instead?
                    Id = $"urn:lsid:artportalen.se:Sighting:{verbatim.Id}",
                    IndividualCount = verbatim.Quantity?.ToString() ?? "",
                    IsNaturalOccurrence = !verbatim.Unspontaneous,
                    IsNeverFoundObservation = verbatim.NotPresent,
                    IsNotRediscoveredObservation = verbatim.NotRecovered,
                    IsPositiveObservation = !(verbatim.NotPresent || verbatim.NotRecovered),
                    LifeStage = verbatim.Stage,
                    OrganismQuantity = verbatim.Quantity,
                    OrganismQuantityType = verbatim.Unit,
                    RecordedBy = verbatim.Observers,
                    RecordNumber = verbatim.Label,
                    Remarks = verbatim.Comment,
                    Sex = verbatim.Gender,
                    SexId = GetSosLookupId(verbatim.Gender?.Id, fieldMappings[FieldMappingFieldId.Sex]),
                    Status = verbatim.NotPresent || verbatim.NotRecovered
                        ? OccurrenceStatus.Absent
                        : OccurrenceStatus.Present,
                    URL = $"http://www.artportalen.se/sighting/{verbatim.Id}"
                },
                OwnerInstitutionCode = verbatim.OwnerOrganization?.Translate(Cultures.en_GB, Cultures.sv_SE) ?? "ArtDatabanken",
                Projects = verbatim.Projects?.ToProcessedProjects(), 
                ProtectionLevel = CalculateProtectionLevel(taxon, verbatim.HiddenByProvider, verbatim.ProtectedBySystem),
                ReportedBy = verbatim.ReportedBy, 
                ReportedDate = verbatim.ReportedDate,
                RightsHolder = verbatim.RightsHolder ?? verbatim.OwnerOrganization?.Translate(Cultures.en_GB, Cultures.sv_SE) ?? "Data saknas",
                Taxon = taxon,
                Type = "Occurrence"
            };
            
            return obs;
        }

        /// <summary>
        /// Get SOS internal Id for the id specific for the data provider.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sosIdByProviderValue"></param>
        /// <returns></returns>
        private static ProcessedLookupValue GetSosLookupId(int? val, IDictionary<object, int> sosIdByProviderValue)
        {
            if (!val.HasValue) return null;

            if (sosIdByProviderValue.TryGetValue(val.Value, out int sosId))
            {
                return new ProcessedLookupValue { Id = sosId };
            }
            else
            {
                return new ProcessedLookupValue { Id = -1, Value = val.ToString() };
            }
        }

        public static ProcessedArea ToProcessed(this GeographicalArea area)
        {
            return new ProcessedArea
            {
                Id = area.Id,
                Name = area.Name
            };
        }

        private static IEnumerable<ProcessedProject> ToProcessedProjects(this IEnumerable<Project> projects)
        {
            return projects?.Select(p => p.ToProcessedProject());
        }

        private static ProcessedProject ToProcessedProject(this Project project)
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
                ProjectParameters = project.ProjectParameters?.Select(p => p.ToProcessedProjectParameter())
            };
        }

        private static ProcessedProjectParameter ToProcessedProjectParameter(this ProjectParameter projectParameter)
        {
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

        private static string GetSamplingProtocol(IEnumerable<Project> projects)
        {
            if (projects == null || !projects.Any()) return null;

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
        private static int CalculateProtectionLevel(ProcessedTaxon taxon, DateTime? hiddenByProvider, bool protectedBySystem)
        {
            if (taxon == null || string.IsNullOrEmpty(taxon.ProtectionLevel))
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
        /// <param name="verbatim"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        private static string GetSubstrateDescription(APSightingVerbatim verbatim, IDictionary<int, ProcessedTaxon> taxa)
        {
            var substrateDescription = new StringBuilder();

            if (verbatim.QuantityOfSubstrate.HasValue)
            {
                substrateDescription.Append($"{verbatim.QuantityOfSubstrate.Value} substratenheter");
            }

            if (verbatim.Substrate != null)
            {
                substrateDescription.Append($"{(substrateDescription.Length == 0 ? "" : " # ")}{verbatim.Substrate.Translate(Cultures.en_GB)}");
            }

            if (!string.IsNullOrEmpty(verbatim.SubstrateDescription))
            {
                substrateDescription.Append($"{(substrateDescription.Length == 0 ? "" : " # ")}{verbatim.SubstrateDescription}");
            }
            
            if (verbatim.SubstrateSpeciesId.HasValue &&
                taxa != null &&
                taxa.TryGetValue(verbatim.SubstrateSpeciesId.Value, out var taxon))
            {
                substrateDescription.Append($"{(substrateDescription.Length == 0 ? "" : " # ")}{taxon.ScientificName}");
            }

            if (!string.IsNullOrEmpty(verbatim.SubstrateSpeciesDescription))
            {
                substrateDescription.Append($"{(substrateDescription.Length == 0 ? "" : " # ")}{verbatim.SubstrateSpeciesDescription}");
            }

            var res = substrateDescription.Length > 0 ? substrateDescription.ToString().WithMaxLength(255) : null;
            return res;
            //return substrateDescription.Length > 0 ? substrateDescription.ToString(0, 255) : null;

            ////return substrateDescription.ToString().WithMaxLength(255);
            //string result;
            //try
            //{
            //    result = substrateDescription.ToString(0, 255);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    throw;
            //}

            //return result;
            ////return substrateDescription.ToString(0, 255);
        }

        /// <summary>
        /// Get bird nest activity id
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="taxon"></param>
        /// <returns></returns>
        public static int? GetBirdNestActivityId(APSightingVerbatim verbatim, ProcessedTaxon taxon)
        {
            if (taxon == null)
            {
                return null;
            }

            if (taxon.OrganismGroup?.StartsWith("fåg", StringComparison.CurrentCultureIgnoreCase) ?? false)
            {
                return (verbatim.Activity?.Id ?? 0) == 0 ? 1000000 : verbatim.Activity.Id;
            }

            return 0;
        }

        /// <summary>
        /// Get associated references
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        public static string GetAssociatedReferences(APSightingVerbatim verbatim)
        {
            if (!verbatim.MigrateSightingObsId.HasValue)
            {
                return null;
            }

            string associatedReferences = null;
            switch (verbatim.MigrateSightingPortalId ?? 0)
            {
                case 1:
                    associatedReferences = $"urn:lsid:artportalen.se:Sighting:Bird.{verbatim.MigrateSightingObsId.Value}";
                    break;
                case 2:
                    associatedReferences = $"urn:lsid:artportalen.se:Sighting:PlantAndMushroom.{verbatim.MigrateSightingObsId.Value}";
                    break;
                case 6:
                    associatedReferences = $"urn:lsid:artportalen.se:Sighting:Vertebrate.{verbatim.MigrateSightingObsId.Value}";
                    break;
                case 7:
                    associatedReferences = $"urn:lsid:artportalen.se:Sighting:Bugs.{verbatim.MigrateSightingObsId.Value}";
                    break;
                case 8:
                    associatedReferences = $"urn:lsid:artportalen.se:Sighting:Fish.{verbatim.MigrateSightingObsId.Value}";
                    break;
                case 9:
                    associatedReferences = $"urn:lsid:artportalen.se:Sighting:MarineInvertebrates.{verbatim.MigrateSightingObsId.Value}";
                    break;
            }

            return associatedReferences;
        }
    }
}
