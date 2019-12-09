using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.DarwinCore;
using  SOS.Lib.Models.Processed.DarwinCore.Vocabulary;
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
            var obs = new DarwinCore<DynamicProperties>(DataProvider.Artdatabanken)
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
                CollectionID = verbatim.CollectionID,
                DatasetID = $"urn:lsid:swedishlifewatch.se:dataprovider:{(int)DataProvider.Artdatabanken}",
                DatasetName = "Artportalen",
                DynamicProperties = new DynamicProperties
                {
                    ActivityId = verbatim.Activity?.Id,
                    BirdNestActivityId = GetBirdNestActivityId(verbatim, taxon),
                    CoordinateX = googleMercatorPoint.Coordinate.X,
                    CoordinateY = googleMercatorPoint.Coordinate.Y,
                    DyntaxaTaxonID = verbatim.TaxonId,
                    IndividualID = verbatim.URL,
                    IsNaturalOccurrence = !verbatim.Unspontaneous,
                    IsNeverFoundObservation = verbatim.NotPresent,
                    IsNotRediscoveredObservation = verbatim.NotRecovered,
                    IsPositiveObservation = !(verbatim.NotPresent || verbatim.NotRecovered),
                    OccurrenceURL = $"http://www.artportalen.se/sighting/{verbatim.Id}",
                    Parish = verbatim.Site?.Parish?.Name ?? string.Empty,
                    Project = verbatim.Project == null
                        ? null
                        : new DarwinCoreProject
                        {
                            IsPublic = verbatim.Project.IsPublic,
                            ProjectCategory = verbatim.Project.Category,
                            ProjectDescription = verbatim.Project.Description,
                            ProjectEndDate =  verbatim.Project.EndDate?.ToString("yyyy-MM-dd hh:mm"),
                            ProjectID = verbatim.Project.Id.ToString(),
                            ProjectName = verbatim.Project.Name,
                            ProjectOwner = verbatim.Project.Owner,
                            ProjectStartDate = verbatim.Project.StartDate?.ToString("yyyy-MM-dd hh:mm"),
                            SurveyMethod = verbatim.Project.SurveyMethod,
                        },
                    ProtectionLevel = CalculateProtectionLevel(taxon, verbatim.HiddenByProvider, verbatim.ProtectedBySystem),
                    ReportedBy = verbatim.ReportedBy,
                    ReportedDate = verbatim.ReportedDate,
                    Substrate = GetSubstrateDescription(verbatim, taxa),
                    UncertainDetermination = verbatim.UnsureDetermination
                },
                Event = new DarwinCoreEvent
                {
                    Habitat = (verbatim.Bioptope != null
                        ? $"{verbatim.Bioptope.Name}{(string.IsNullOrEmpty(verbatim.BiotopeDescription) ? "" : " # ")}{verbatim.BiotopeDescription}"
                        : verbatim.BiotopeDescription).WithMaxLength(255),
                    SamplingProtocol = verbatim.Project?.SurveyMethod ?? verbatim.Project?.SurveyMethodUrl,
                    VerbatimEventDate = $"{(verbatim.StartDate?.ToString("yyyy-MM-dd hh:mm") ?? "")}{(verbatim.StartDate.HasValue && verbatim.EndDate.HasValue ? "-" : "")}{(verbatim.EndDate?.ToString("yyyy-MM-dd hh:mm") ?? "")}"
                },
                Identification = new DarwinCoreIdentification
                {
                    IdentificationVerificationStatus = verbatim.ValidationStatus?.Name,
                    IdentifiedBy = verbatim.VerifiedBy
                },
                Organism = new DarwinCoreOrganism()
                {

                },
                InformationWithheld = "More information can be obtained from the Data Provider",
                InstitutionCode = verbatim.OwnerOrganization?.Name ?? "ArtDatabanken",
                InstitutionID = verbatim.ControlingOrganisationId.HasValue
                    ? $"urn:lsid:artdata.slu.se:organization:{verbatim.ControlingOrganisationId}"
                    : null,
                IsInEconomicZoneOfSweden = verbatim.Site?.XCoord != 0 && verbatim.Site?.YCoord != 0, // Species portal validate all sightings, we rely on that validation as long it has coordinates
                Language = Language.Swedish,
                Location = new DarwinCoreLocation
                {
                    Continent = Continent.Europe,
                    CoordinateUncertaintyInMeters = verbatim.Site?.Accuracy.ToString(),
                    Country = Country.Sweden,
                    CountryCode = CountryCode.Sweden,
                    County = verbatim.Site?.County?.Name ?? string.Empty,
                    DecimalLatitude = wgs84Point.Coordinate?.Y ?? 0,
                    DecimalLongitude = wgs84Point.Coordinate?.X ?? 0,
                    GeodeticDatum = GeodeticDatum.Wgs84,
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
                    AssociatedMedia = verbatim.HasImages
                        ? $"http://www.artportalen.se/sighting/{verbatim.Id}#SightingDetailImages"
                        : "",
                    AssociatedReferences = GetAssociatedReferences(verbatim),
                    Behavior = verbatim.Activity?.Name,
                    CatalogNumber = verbatim.Id.ToString(),
                    EstablishmentMeans = verbatim.Unspontaneous ? "Unspontaneous" : "Natural", // todo - "Unspontaneous" & "Natural" is not in the DwC recomended vocabulary. Get value from Dyntaxa instead?
                    IndividualCount = verbatim.Quantity?.ToString() ?? "",
                    LifeStage = verbatim.Stage?.Name,
                    OccurrenceID = $"urn:lsid:artportalen.se:Sighting:{verbatim.Id}",
                    OccurrenceRemarks = verbatim.Comment,
                    OccurrenceStatus = verbatim.NotPresent || verbatim.NotRecovered
                        ? OccurrenceStatus.Absent
                        : OccurrenceStatus.Present,
                    OrganismQuantity = verbatim.Quantity?.ToString() ?? "",
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

            obs.Event.PopulateDateFields(verbatim.StartDate.Value, verbatim.EndDate);
            return obs;
        }
        
        /// <summary>
        /// Cast multiple sightings entities to models 
        /// </summary>
        /// <param name="verbatims"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static ICollection<DarwinCore<DynamicProperties>> ToDarwinCore(this IEnumerable<APSightingVerbatim> verbatims, IDictionary<int, DarwinCoreTaxon> taxa)
        {
            return verbatims.Select(v => v.ToDarwinCore(taxa)).ToArray();
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
            if (taxon == null || string.IsNullOrEmpty(taxon.DynamicProperties?.ProtectionLevel))
            {
                return 1;
            }

            var regex = new Regex(@"^\d");

            if (int.TryParse(regex.Match(taxon.DynamicProperties.ProtectionLevel).Value, out var protectionLevel))
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

        /// <summary>
        /// Get associated references
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        public static string GetAssociatedReferences(APSightingVerbatim verbatim)
        {
            string associatedReferences = null;
            switch (verbatim.MigrateSightingPortalId ?? 0)
            {
                case 1:
                    if (verbatim.MigrateSightingObsId.HasValue)
                    {
                        associatedReferences = $"urn:lsid:artportalen.se:Sighting:Bird.{verbatim.MigrateSightingObsId.Value}";
                    }
                    break;
                case 2:
                    if (verbatim.MigrateSightingObsId.HasValue)
                    {
                        associatedReferences = $"urn:lsid:artportalen.se:Sighting:PlantAndMushroom.{verbatim.MigrateSightingObsId.Value}";
                    }
                    break;
                case 6:
                    if (verbatim.MigrateSightingObsId.HasValue)
                    {
                        associatedReferences = $"urn:lsid:artportalen.se:Sighting:Vertebrate.{verbatim.MigrateSightingObsId.Value}";
                    }
                    break;
                case 7:
                    if (verbatim.MigrateSightingObsId.HasValue)
                    {
                        associatedReferences = $"urn:lsid:artportalen.se:Sighting:Bugs.{verbatim.MigrateSightingObsId.Value}";
                    }
                    break;
                case 8:
                    if (verbatim.MigrateSightingObsId.HasValue)
                    {
                        associatedReferences = $"urn:lsid:artportalen.se:Sighting:Fish.{verbatim.MigrateSightingObsId.Value}";
                    }
                    break;
                case 9:
                    if (verbatim.MigrateSightingObsId.HasValue)
                    {
                        associatedReferences = $"urn:lsid:artportalen.se:Sighting:MarineInvertebrates.{verbatim.MigrateSightingObsId.Value}";
                    }
                    break;
            }

            return associatedReferences;
        }
    }
}
