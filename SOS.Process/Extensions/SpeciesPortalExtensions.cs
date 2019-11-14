using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Verbatim.SpeciesPortal;

namespace SOS.Process.Extensions
{
    /// <summary>
    /// Entity extensions
    /// </summary>
    public static class SpeciesPortalExtensions
    {
        /// <summary>
        /// Cast sighting verbatim to Darwin Core
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static DarwinCore<DynamicProperties> ToDarwinCore(this APSightingVerbatim verbatim, IDictionary<int, DarwinCoreTaxon> taxa)
        {
            if (verbatim == null)
            {
                return null;
            }

            var taxonId = verbatim.TaxonId ?? -1;

            var googleMercatorPoint = new Point(verbatim.Site?.XCoord ?? 0, verbatim.Site?.YCoord ?? 0);
            var wgs84Point = googleMercatorPoint.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84);

            taxa.TryGetValue(taxonId, out var taxon);

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
            
            return new DarwinCore<DynamicProperties>()
            {
                AccessRights = !verbatim.ProtectedBySystem && verbatim.HiddenByProvider.GetValueOrDefault(DateTime.MinValue) < DateTime.Now ? "Free usage" : "Not for public usage",
                BasisOfRecord = string.IsNullOrEmpty(verbatim.SpeciesCollection) ? "Human observation" : "Preserved specimen", 
                CollectionCode = string.IsNullOrEmpty(verbatim.SpeciesCollection) ? "Artportalen" : verbatim.SpeciesCollection,
                CollectionID = verbatim.CollectionID,
                DatasetID = "urn:lsid:swedishlifewatch.se:dataprovider:1",
                DatasetName = "Artportalen",
                DynamicProperties = new DynamicProperties
                {
                    ActivityId = verbatim.Activity?.Id,
                    BirdNestActivityId = GetBirdNestActivityId(verbatim, taxon),
                    CoordinateX = googleMercatorPoint.Coordinate?.X ?? 0,
                    CoordinateY = googleMercatorPoint.Coordinate?.Y ?? 0,
                    DyntaxaTaxonID = verbatim.TaxonId,
                    IndividualID = verbatim.URL,
                    IsNaturalOccurrence = !verbatim.Unspontaneous,
                    IsNeverFoundObservation = verbatim.NotPresent,
                    IsNotRediscoveredObservation = verbatim.NotRecovered,
                    IsPositiveObservation = !(verbatim.NotPresent || verbatim.NotRecovered),
                    OccurrenceURL = $"http://www.artportalen.se/sighting/{verbatim.Id}",
                    Parish = verbatim.Site?.Parish?.Name ?? string.Empty,
                    Project = verbatim.Project == null ? null : new DarwinCoreProject
                    {
                        IsPublic = verbatim.Project?.IsPublic ?? false,
                        ProjectCategory = verbatim.Project?.Category,
                        ProjectDescription = verbatim.Project?.Description,
                        ProjectEndDate = verbatim.Project?.EndDate.HasValue ?? false ? verbatim.Project.EndDate.Value.ToString("yyyy-MM-dd hh:mm") : null,
                        ProjectID = verbatim.Project?.Id.ToString(),
                        ProjectName = verbatim.Project?.Name,
                        ProjectOwner = verbatim.Project ?.Owner,
                        ProjectStartDate = verbatim.Project?.StartDate.HasValue ?? false ? verbatim.Project.StartDate.Value.ToString("yyyy-MM-dd hh:mm") : null,
                        SurveyMethod = verbatim.Project?.SurveyMethod,
                    },
                    ProtectionLevel = CalculateProtectionLevel(taxon, verbatim.HiddenByProvider, verbatim.ProtectedBySystem),
                    ReportedBy = verbatim.ReportedBy,
                    ReportedDate = verbatim.ReportedDate,
                    Substrate = GetSubstrateDescription(verbatim, taxa),
                    UncertainDetermination = verbatim.UnsureDetermination
                },
                Event = new DarwinCoreEvent
                {
                    Day = verbatim.EndDate?.Day ?? verbatim.StartDate?.Day ?? 0,
                    EndDayOfYear = verbatim.EndDate?.DayOfYear ?? 0,
                    EventDate = $"{(verbatim.StartDate.HasValue ? verbatim.StartDate.Value.ToString("O") : "")}{(verbatim.StartDate.HasValue && verbatim.EndDate.HasValue ? "-" : "")}{(verbatim.EndDate.HasValue ? verbatim.EndDate.Value.ToString("O") : "")}",
                    EventTime = $"{(verbatim.StartTime.HasValue ? verbatim.StartTime.Value.ToString("hh:mm") : "")}+1{(verbatim.StartTime.HasValue && verbatim.EndTime.HasValue ? "/" : "")}{(verbatim.EndTime.HasValue ? verbatim.EndTime.Value.ToString("hh:mm") : "")}+1",
                    Habitat = (verbatim.Bioptope != null ? $"{verbatim.Bioptope.Name}{(string.IsNullOrEmpty(verbatim.BiotopeDescription) ? "" : " # ")}{verbatim.BiotopeDescription}" : verbatim.BiotopeDescription).Substring(0, 255),
                    Month = verbatim.EndDate?.Month ?? verbatim.StartDate?.Month ?? 0,
                    SamplingProtocol = verbatim.Project?.SurveyMethod ?? verbatim.Project?.SurveyMethodUrl,
                    StartDayOfYear = verbatim.StartDate?.DayOfYear ?? 0,
                    VerbatimEventDate = $"{(verbatim.StartDate.HasValue ? verbatim.StartDate.Value.ToString("yyyy-MM-dd hh:mm") : "")}{(verbatim.StartDate.HasValue && verbatim.EndDate.HasValue ? "-" : "")}{(verbatim.EndDate.HasValue ? verbatim.EndDate.Value.ToString("yyyy-MM-dd hh:mm") : "")}",
                    Year = verbatim.EndDate?.Year ?? verbatim.StartDate?.Year ?? 0
                },
                Identification = new DarwinCoreIdentification
                {
                    IdentificationVerificationStatus = verbatim.ValidationStatus?.Name,
                    IdentifiedBy = verbatim.VerifiedBy
                },
                InformationWithheld = "More information can be obtained from the Data Provider",
                InstitutionCode = verbatim.OwnerOrganization?.Name ?? "ArtDatabanken",
                InstitutionID = verbatim.ControlingOrganisationId.HasValue ? $"urn:lsid:artdata.slu.se:organization:{verbatim.ControlingOrganisationId}" : null,
                Language = "Swedish",
                Location = new DarwinCoreLocation
                {
                    Continent = "Europa",
                    CoordinateUncertaintyInMeters = verbatim.Site?.Accuracy.ToString(),
                    Country = "Sweden",
                    CountryCode = "SE",
                    County = verbatim.Site?.County?.Name ?? string.Empty,
                    DecimalLatitude = wgs84Point.Coordinate?.X ?? 0,
                    DecimalLongitude = wgs84Point.Coordinate?.Y ?? 0,
                    GeodeticDatum = "EPSG:4326",
                    Locality = verbatim.Site?.Name,
                    LocationID = $"urn:lsid:artportalen.se:site:{verbatim.Site?.Id}",
                    MaximumDepthInMeters = verbatim.MaxDepth?.ToString(),
                    MaximumElevationInMeters = verbatim.MaxHeight?.ToString(),
                    MinimumDepthInMeters = verbatim.MinDepth?.ToString(),
                    MinimumElevationInMeters = verbatim.MinHeight?.ToString(),
                    Municipality = verbatim.Site?.Municipality?.Name ?? string.Empty,
                    StateProvince = verbatim.Site?.Province?.Name ?? string.Empty
                },
                Modified = verbatim.EndDate ?? verbatim.ReportedDate,
                Occurrence = new DarwinCoreOccurrence
                {
                    AssociatedMedia = verbatim.HasImages ? $"http://www.artportalen.se/sighting/{verbatim.Id}#SightingDetailImages" : "",
                    AssociatedReferences = associatedReferences,
                    Behavior = verbatim.Activity?.Name, 
                    CatalogNumber = verbatim.Id.ToString(),
                    EstablishmentMeans = verbatim.Unspontaneous ? "Unspontaneous" : "Natural",
                    IndividualCount = verbatim.Quantity.HasValue ? verbatim.Quantity.Value.ToString() : "",
                    LifeStage = verbatim.Stage?.Name,
                    OccurrenceID = $"urn:lsid:artportalen.se:Sighting:{verbatim.Id}",
                    OccurrenceRemarks = verbatim.Comment,
                    OccurrenceStatus = verbatim.NotPresent || verbatim.NotRecovered ? "Absent" : "Present",
                    OrganismQuantity = verbatim.Quantity.HasValue ? verbatim.Quantity.Value.ToString() : "",
                    OrganismQuantityType = verbatim.Quantity.HasValue ? verbatim.Unit?.Name ?? "Individuals" : null,
                    RecordedBy = verbatim.Observers,    
                    RecordNumber = verbatim.Label,
                    ReproductiveCondition = verbatim.Activity?.Category?.Name,
                    Sex = verbatim.Gender?.Name
                },
                OwnerInstitutionCode = verbatim.OwnerOrganization?.Name ?? "ArtDatabanken",
                RightsHolder = verbatim.RightsHolder ?? verbatim.OwnerOrganization?.Name ?? "Data saknas",
                Taxon = taxon,
                Type = "Occurrence"
            };
        }

        /// <summary>
        /// Cast multiple sightings entities to models 
        /// </summary>
        /// <param name="verbatims"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static IEnumerable<DarwinCore<DynamicProperties>> ToDarwinCore(this IEnumerable<APSightingVerbatim> verbatims, IDictionary<int, DarwinCoreTaxon> taxa)
        {
            return verbatims.Select(v => v.ToDarwinCore(taxa));
        }

        /// <summary>
        /// Calculate protection level
        /// </summary>
        /// <param name="taxon"></param>
        /// <param name="hiddenByProvider"></param>
        /// <param name="protectedBySystem"></param>
        /// <returns></returns>
        private static int CalculateProtectionLevel(DarwinCoreTaxon taxon, DateTime? hiddenByProvider, bool protectedBySystem)
        {
            if (taxon == null || string.IsNullOrEmpty(taxon.DynamicProperties.ProtectionLevel))
            {
                return 1;
            }

            var regex = new Regex(@"^\d");
            var protectionLevel = int.Parse(regex.Match(taxon.DynamicProperties.ProtectionLevel).Value);

            if (protectionLevel <= 3 && hiddenByProvider.HasValue && hiddenByProvider.Value >= DateTime.Now)
            {
                return 3;
            }
            if ((protectionLevel > 3 && hiddenByProvider.HasValue && hiddenByProvider.Value >= DateTime.Now) || protectedBySystem)
            {
                return protectionLevel;
            }

            return 1;
        }

        /// <summary>
        /// Build the substrate description string
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        private static string GetSubstrateDescription(APSightingVerbatim verbatim, IDictionary<int, DarwinCoreTaxon> taxa)
        {
            var substrateDescription = new StringBuilder();

            if (verbatim.QuantityOfSubstrate.HasValue)
            {
                substrateDescription.Append($"{verbatim.QuantityOfSubstrate.Value} substratenheter");
            }

            if (verbatim.Substrate != null)
            {
                substrateDescription.Append($"{(substrateDescription.Length == 0 ? "" : " # ")}{verbatim.Substrate.Name}");
            }

            if (!string.IsNullOrEmpty(verbatim.SubstrateDescription))
            {
                substrateDescription.Append($"{(substrateDescription.Length == 0 ? "" : " # ")}{verbatim.SubstrateDescription}");
            }
            
            if (verbatim.SubstrateSpeciesId != null &&
                taxa != null &&
                taxa.TryGetValue(verbatim.SubstrateSpeciesId.Value, out var taxon))
            {
                substrateDescription.Append($"{(substrateDescription.Length == 0 ? "" : " # ")}{taxon.ScientificName}");
            }

            if (!string.IsNullOrEmpty(verbatim.SubstrateSpeciesDescription))
            {
                substrateDescription.Append($"{(substrateDescription.Length == 0 ? "" : " # ")}{verbatim.SubstrateSpeciesDescription}");
            }

            return substrateDescription.ToString(0, 255);
        }

        /// <summary>
        /// Get bird nest activity id
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="taxon"></param>
        /// <returns></returns>
        public static int? GetBirdNestActivityId(APSightingVerbatim verbatim, DarwinCoreTaxon taxon)
        {
            if (taxon == null)
            {
                return null;
            }

            if (taxon.DynamicProperties?.OrganismGroup?.StartsWith("fåg", StringComparison.CurrentCultureIgnoreCase) ?? false)
            {
                return (verbatim.Activity?.Id ?? 0) == 0 ? 1000000 : verbatim.Activity.Id;
            }

            return 0;
        }
    }
}
