using System;
using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.DarwinCore;
using  SOS.Lib.Models.Processed.DarwinCore.Vocabulary;
using SOS.Lib.Models.Verbatim.ClamPortal;
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
        public static DarwinCore<DynamicProperties> ToDarwinCore(this KulObservationVerbatim verbatim, IDictionary<int, DarwinCoreTaxon> taxa)
        {
            taxa.TryGetValue(verbatim.DyntaxaTaxonId, out var taxon);
            // todo - ProtectionLevel, CoordinateX_RT90, CoordinateY_RT90, CoordinateX_SWEREF99, CoordinateY_SWEREF99, CoordinateX, CoordinateY
            var obs = new DarwinCore<DynamicProperties>(DataProvider.KUL)
            {
                BasisOfRecord = BasisOfRecord.HumanObservation,
                DatasetID = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProvider.KUL.ToString()}",
                DatasetName = "KUL",
                DynamicProperties = new DynamicProperties
                {
                    DataProviderId = (int)DataProvider.KUL,
                    CoordinateX = verbatim.DecimalLongitude, // todo - convert to WebMercator?
                    CoordinateY = verbatim.DecimalLatitude, // todo - convert to WebMercator?
                    ObservationDateStart = verbatim.Start,
                    ObservationDateEnd = verbatim.End,
                    DyntaxaTaxonID = verbatim.DyntaxaTaxonId,
                    Conservation = new DarwinCoreConservation()
                    {
                        ProtectionLevel = GetProtectionLevel()
                    },
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = GetIsNeverFoundObservation(verbatim.DyntaxaTaxonId),
                    IsNotRediscoveredObservation = false,
                    IsPositiveObservation = GetIsPositiveObservation(verbatim.DyntaxaTaxonId),
                    Owner = verbatim.Owner,
                    ReportedBy = verbatim.ReportedBy,
                    ReportedDate = verbatim.Start,
                    UncertainDetermination = false
                },
                Event = new DarwinCoreEvent
                {
                    EventDate = $"{verbatim.Start.ToUniversalTime().ToString("s")}Z",
                    EventTime = verbatim.Start.ToUniversalTime().ToString("HH':'mm':'ss''K"),
                    VerbatimEventDate = verbatim.Start.ToString("yyyy-MM-dd HH:mm:ss")
                },
                Identification = new DarwinCoreIdentification
                {
                    
                },
                Organism = new DarwinCoreOrganism()
                {

                },
                Location = new DarwinCoreLocation
                {
                    CoordinateUncertaintyInMeters = verbatim.CoordinateUncertaintyInMeters ?? DefaultCoordinateUncertaintyInMeters,
                    CountryCode = verbatim.CountryCode,
                    DecimalLatitude = verbatim.DecimalLatitude,
                    DecimalLongitude = verbatim.DecimalLongitude,
                    GeodeticDatum = GeodeticDatum.Wgs84,
                    Continent = Continent.Europe,
                    Country = Country.Sweden,
                    Locality = verbatim.Locality
                },
                Modified = verbatim.Start,
                Occurrence = new DarwinCoreOccurrence
                {
                    CatalogNumber = GetCatalogNumber(verbatim.OccurrenceId),
                    IndividualCount = verbatim.IndividualCount?.ToString(),
                    OccurrenceID = verbatim.OccurrenceId,
                    OccurrenceStatus = GetOccurrenceStatus(verbatim.DyntaxaTaxonId),
                    RecordedBy = verbatim.RecordedBy
                },
                Taxon = taxon
            };

            obs.Event.PopulateDateFields(verbatim.Start, verbatim.End);
            return obs;
        }

        /// <summary>
        /// Cast multiple clam observations to Darwin Core 
        /// </summary>
        /// <param name="verbatims"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static IEnumerable<DarwinCore<DynamicProperties>> ToDarwinCore(
            this IEnumerable<KulObservationVerbatim> verbatims, 
            IDictionary<int, DarwinCoreTaxon> taxa)
        {
            return verbatims.Select(v => v.ToDarwinCore(taxa));
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
