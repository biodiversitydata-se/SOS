using System;
using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Enums;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Verbatim.ClamPortal;

namespace SOS.Process.Extensions
{
    /// <summary>
    /// Entity extensions
    /// </summary>
    public static class ClamPortalExtensions
    {
        /// <summary>
        /// Cast clam observation verbatim to Darwin Core
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static DarwinCore<DynamicProperties> ToDarwinCore(this ClamObservationVerbatim verbatim, IDictionary<int, DarwinCoreTaxon> taxa)
        {
            taxa.TryGetValue(verbatim.DyntaxaTaxonId ?? -1, out var taxon);

            return new DarwinCore<DynamicProperties>(DataProvider.ClamPortal)
            {
                AccessRights = verbatim.AccessRights,
                BasisOfRecord = verbatim.BasisOfRecord,
                DatasetID = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProvider.ClamPortal.ToString()}",
                DatasetName = "Träd och musselportalen",
                DynamicProperties = new DynamicProperties
                {
                    IsNaturalOccurrence = verbatim.IsNaturalOccurrence,
                    IsNeverFoundObservation = verbatim.IsNeverFoundObservation,
                    IsNotRediscoveredObservation = verbatim.IsNotRediscoveredObservation,
                    IsPositiveObservation = verbatim.IsPositiveObservation,
                    Projects = string.IsNullOrEmpty(verbatim.ProjectName) ? null : new[]
                    {
                        new DarwinCoreProject
                        {
                            Name = verbatim.ProjectName
                        }
                    }, 
                    ReportedBy = verbatim.ReportedBy,
                    ReportedDate = verbatim.ReportedDate,
                    UncertainDetermination = verbatim.UncertainDetermination != 0
                },
                Event = new DarwinCoreEvent
                {
                    EventDate = verbatim.ObservationDate.ToString("yyyy-MM-dd"),
                    EventTime = verbatim.ObservationDate.ToString("HH:mm:ss"),
                    SamplingProtocol = verbatim.SurveyMethod
                },
                Identification = new DarwinCoreIdentification
                {
                    IdentificationVerificationStatus = verbatim.IdentificationVerificationStatus
                },
                Organism = new DarwinCoreOrganism()
                {

                },
                InstitutionCode = verbatim.InstitutionCode,
                Language = verbatim.Language,
                Location = new DarwinCoreLocation
                {
                    Continent = "Europa",
                    CoordinatePrecision = verbatim.CoordinateUncertaintyInMeters?.ToString(),
                    CountryCode = verbatim.CountryCode,
                    DecimalLatitude = verbatim.DecimalLatitude,
                    DecimalLongitude = verbatim.DecimalLongitude,
                    GeodeticDatum = "EPSG:4326",
                    LocationID = verbatim.LocationId,
                    Locality = verbatim.Locality,
                    LocationRemarks = verbatim.LocationRemarks,
                    MaximumDepthInMeters = verbatim.MaximumDepthInMeters,
                    VerbatimLatitude = verbatim.DecimalLatitude.ToString(),
                    VerbatimLongitude = verbatim.DecimalLongitude.ToString(),
                    VerbatimCoordinateSystem = "EPSG:4326",
                    WaterBody = verbatim.WaterBody
                },
                Modified = verbatim.Modified ?? DateTime.MinValue,
                Occurrence = new DarwinCoreOccurrence
                {

                    CatalogNumber = verbatim.CatalogNumber.ToString(),
                    IndividualCount = verbatim.IndividualCount,
                    LifeStage = verbatim.LifeStage,
                    OccurrenceID = verbatim.OccurrenceId,
                    OccurrenceRemarks = verbatim.OccurrenceRemarks,
                    OccurrenceStatus = verbatim.OccurrenceStatus,
                    OrganismQuantity = verbatim.Quantity,
                    OrganismQuantityType = verbatim.QuantityUnit,
                    RecordedBy = verbatim.RecordedBy
                },
                RightsHolder = verbatim.RightsHolder,
                Taxon = taxon
            };
        }

        /// <summary>
        /// Cast multiple clam observations to Darwin Core 
        /// </summary>
        /// <param name="verbatims"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static IEnumerable<DarwinCore<DynamicProperties>> ToDarwinCore(this IEnumerable<ClamObservationVerbatim> verbatims, IDictionary<int, DarwinCoreTaxon> taxa)
        {
            return verbatims.Select(v => v.ToDarwinCore(taxa));
        }
    }
}
