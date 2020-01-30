using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.GeoJsonObjectModel;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Kul;

namespace SOS.Process.Extensions
{
    /// <summary>
    /// Entity extensions
    /// </summary>
    public static class KulObservationExtensions
    {
        private const int DefaultCoordinateUncertaintyInMeters = 500;

        /// <summary>
        /// Cast KUL observation verbatim to Darwin Core
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static ProcessedSighting ToProcessed(this KulObservationVerbatim verbatim, IDictionary<int, ProcessedTaxon> taxa)
        {
            var hasPosition = verbatim.DecimalLongitude > 0 && verbatim.DecimalLatitude > 0;
            taxa.TryGetValue(verbatim.DyntaxaTaxonId, out var taxon);
            // todo - ProtectionLevel, CoordinateX_RT90, CoordinateY_RT90, CoordinateX_SWEREF99, CoordinateY_SWEREF99, CoordinateX, CoordinateY
            var obs = new ProcessedSighting(DataProvider.KUL)
            {
                BasisOfRecord = BasisOfRecord.HumanObservation,
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProvider.KUL.ToString()}",
                DatasetName = "KUL",
                Event = new ProcessedEvent
                {
                    EndDate = verbatim.End.ToUniversalTime(),
                    StartDate = verbatim.Start.ToUniversalTime(),
                    VerbatimEndDate = verbatim.End,
                    VerbatimStartDate = verbatim.Start
                },
                Identification = new ProcessedIdentification
                {
                    Validated = true,
                    UncertainDetermination = false
                },
                Location = new ProcessedLocation
                {
                    CoordinateUncertaintyInMeters = verbatim.CoordinateUncertaintyInMeters ?? DefaultCoordinateUncertaintyInMeters,
                    CountryCode = verbatim.CountryCode,
                    DecimalLatitude = verbatim.DecimalLatitude,
                    DecimalLongitude = verbatim.DecimalLongitude,
                    GeodeticDatum = GeodeticDatum.Wgs84,
                    Continent = Continent.Europe,
                    Country = Country.Sweden,
                    Locality = verbatim.Locality,
                    Point = hasPosition ? new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(verbatim.DecimalLongitude, verbatim.DecimalLatitude)) : null,
                    PointWithBuffer = hasPosition ? new[] { verbatim.DecimalLongitude, verbatim.DecimalLatitude }.ToCircle(verbatim.CoordinateUncertaintyInMeters)?.ToGeoJsonGeometry() : null,
                    VerbatimLatitude = verbatim.DecimalLatitude,
                    VerbatimLongitude = verbatim.DecimalLongitude
                },
                Modified = verbatim.Start,
                Occurrence = new ProcessedOccurrence
                {
                    CatalogNumber = GetCatalogNumber(verbatim.OccurrenceId),
                    Id = verbatim.OccurrenceId,
                    IndividualCount = verbatim.IndividualCount?.ToString(),
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = GetIsNeverFoundObservation(verbatim.DyntaxaTaxonId),
                    IsNotRediscoveredObservation = false,
                    IsPositiveObservation = GetIsPositiveObservation(verbatim.DyntaxaTaxonId),
                    RecordedBy = verbatim.RecordedBy,
                    Status = GetOccurrenceStatus(verbatim.DyntaxaTaxonId)
                },
                OwnerInstitutionCode = verbatim.Owner,
                ProtectionLevel = GetProtectionLevel(),
                ReportedBy = verbatim.ReportedBy,
                ReportedDate = verbatim.Start,
                Taxon = taxon
            };

            return obs;
        }

        /// <summary>
        /// Cast multiple clam observations to Darwin Core 
        /// </summary>
        /// <param name="verbatims"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static IEnumerable<ProcessedSighting> ToProcessed(
            this IEnumerable<KulObservationVerbatim> verbatims, 
            IDictionary<int, ProcessedTaxon> taxa)
        {
            return verbatims.Select(v => v.ToProcessed(taxa));
        }

        /// <summary>
        /// Creates occurrence id.
        /// </summary>
        /// <returns>The Catalog Number.</returns>
        private static string GetCatalogNumber(string occurrenceId)
        {
            int pos = occurrenceId.LastIndexOf(":", StringComparison.Ordinal);
            return occurrenceId.Substring(pos + 1);
        }


        /// <summary>
        /// Gets the occurrence status. Set to Present if DyntaxaTaxonId from provider is greater than 0 and Absent if DyntaxaTaxonId is 0
        /// </summary>
        private static string GetOccurrenceStatus(int dyntaxaTaxonId)
        {
            return dyntaxaTaxonId == 0 ? OccurrenceStatus.Absent : OccurrenceStatus.Present;
        }

        /// <summary>
        /// An integer value corresponding to the Enum of the Main field of the SpeciesFact FactorId 761.
        /// By default the value is 1. If the taxon is subordinate to the taxon category Species it is nessecary
        /// to check the Species Fact values of parent taxa.
        /// If the value is greater than 1 for any parent then the value should equal to the max value among parents.
        /// </summary>
        /// <returns></returns>
        private static int GetProtectionLevel()
        {
            return 1;
        }

        /// <summary>
        /// Set to False if DyntaxaTaxonId from provider is greater than 0 and True if DyntaxaTaxonId is 0.
        /// </summary>
        private static bool GetIsNeverFoundObservation(int dyntaxaTaxonId)
        {
            return dyntaxaTaxonId == 0;
        }

        /// <summary>
        /// Set to True if DyntaxaTaxonId from provider is greater than 0 and False if DyntaxaTaxonId is 0.
        /// </summary>
        private static bool GetIsPositiveObservation(int dyntaxaTaxonId)
        {
            return dyntaxaTaxonId != 0;
        }
    }
}
