using System;
using System.Collections.Generic;
using System.Linq;
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
        public static DarwinCore<DynamicProperties> ToDarwinCore(this APSightingVerbatim verbatim, IDictionary<string, DarwinCoreTaxon> taxa)
        {
            
            var taxonId = verbatim.TaxonId.ToString();

            var googleMercatorPoint = new Point(verbatim.Site?.XCoord ?? 0, verbatim.Site?.YCoord ?? 0);
            var wgs84Point = googleMercatorPoint.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84);

            string associatedReferences = null;
            switch (verbatim.MigrateSightingPortalId.HasValue ? verbatim.MigrateSightingPortalId.Value : 0)
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
                CollectionID = verbatim.CollectionID,
                DatasetID = "urn:lsid:swedishlifewatch.se:dataprovider:1",
                DatasetName = "Artportalen",
                DynamicProperties = new DynamicProperties
                {
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
                    ReportedDate = verbatim.ReportedDate,
                    UncertainDetermination = verbatim.UnsureDetermination
                },
                Event = new DarwinCoreEvent
                {
                    Day = verbatim.EndDate?.Day ?? verbatim.StartDate?.Day ?? 0,
                    EndDayOfYear = verbatim.EndDate?.DayOfYear ?? 0,
                    EventDate = $"{(verbatim.StartDate.HasValue ? verbatim.StartDate.Value.ToString("O") : "")}{(verbatim.StartDate.HasValue && verbatim.EndDate.HasValue ? "-" : "")}{(verbatim.EndDate.HasValue ? verbatim.EndDate.Value.ToString("O") : "")}",
                    EventTime = $"{(verbatim.StartTime.HasValue ? verbatim.StartTime.Value.ToString("hh:mm") : "")}+1{(verbatim.StartTime.HasValue && verbatim.EndTime.HasValue ? "/" : "")}{(verbatim.EndTime.HasValue ? verbatim.EndTime.Value.ToString("hh:mm") : "")}+1",
                    Month = verbatim.EndDate?.Month ?? verbatim.StartDate?.Month ?? 0,
                    StartDayOfYear = verbatim.StartDate?.DayOfYear ?? 0,
                    VerbatimEventDate = $"{(verbatim.StartDate.HasValue ? verbatim.StartDate.Value.ToString("yyyy-MM-dd hh:mm") : "")}{(verbatim.StartDate.HasValue && verbatim.EndDate.HasValue ? "-" : "")}{(verbatim.EndDate.HasValue ? verbatim.EndDate.Value.ToString("yyyy-MM-dd hh:mm") : "")}",
                    Year = verbatim.EndDate?.Year ?? verbatim.StartDate?.Year ?? 0
                },
                Identification = new DarwinCoreIdentification
                {
                    IdentificationVerificationStatus = verbatim.ValidationStatus?.Name 
                },
                InformationWithheld = "More information can be obtained from the Data Provider",
                InstitutionCode = verbatim.InstitutionCode ?? "ArtDatabanken",
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
                    RecordNumber = verbatim.Label,
                    ReproductiveCondition = verbatim.Activity?.Category?.Name,
                    Sex = verbatim.Gender?.Name
                },
                OwnerInstitutionCode = verbatim.InstitutionCode ?? "ArtDatabanken",
                Taxon = taxa.ContainsKey(taxonId) ? taxa[taxonId] : null,
                Type = "Occurrence"
            };
        }

        /// <summary>
        /// Cast multiple sightings entities to models 
        /// </summary>
        /// <param name="verbatims"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static IEnumerable<DarwinCore<DynamicProperties>> ToDarwinCore(this IEnumerable<APSightingVerbatim> verbatims, IDictionary<string, DarwinCoreTaxon> taxa)
        {
            return verbatims.Select(v => v.ToDarwinCore(taxa));
        }
    }
}
