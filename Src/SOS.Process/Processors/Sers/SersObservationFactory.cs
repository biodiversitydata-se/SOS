using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using NetTopologySuite.Geometries;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.FieldMappingValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Sers;

namespace SOS.Process.Processors.Sers
{
    public class SersObservationFactory
    {
        private const int DefaultCoordinateUncertaintyInMeters = 500;
        private readonly DataProvider _dataProvider;
        private readonly IDictionary<int, ProcessedTaxon> _taxa;

        public SersObservationFactory(DataProvider dataProvider, IDictionary<int, ProcessedTaxon> taxa)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _taxa = taxa ?? throw new ArgumentNullException(nameof(taxa));
        }

        /// <summary>
        /// Cast multiple clam observations to ProcessedObservation
        /// </summary>
        /// <param name="verbatims"></param>
        /// <returns></returns>
        public IEnumerable<ProcessedObservation> CreateProcessedObservations(IEnumerable<SersObservationVerbatim> verbatims)
        {
            return verbatims.Select(CreateProcessedObservation);
        }

        /// <summary>
        /// Cast SERS observation verbatim to ProcessedObservation
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        public ProcessedObservation CreateProcessedObservation(SersObservationVerbatim verbatim)
        {
            Point wgs84Point = null;
            if (verbatim.DecimalLongitude > 0 && verbatim.DecimalLatitude > 0)
            {
                wgs84Point = new Point(verbatim.DecimalLongitude, verbatim.DecimalLatitude) { SRID = (int)CoordinateSys.WGS84 };
            }

            _taxa.TryGetValue(verbatim.DyntaxaTaxonId, out var taxon);

            var obs = new ProcessedObservation
            {
                DataProviderId = _dataProvider.Id,
                BasisOfRecord = new ProcessedFieldMapValue { Id = (int)BasisOfRecordId.HumanObservation },
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.SERS}",
                DatasetName = "SERS",
                Event = new ProcessedEvent
                {
                    EndDate = verbatim.End.ToUniversalTime(),
                    StartDate = verbatim.Start.ToUniversalTime(),
                    VerbatimEventDate = DwcFormatter.CreateDateIntervalString(verbatim.Start, verbatim.End)
                },
                Identification = new ProcessedIdentification
                {
                    Validated = true,
                    UncertainDetermination = false
                },
                Location = new ProcessedLocation
                {
                    CoordinateUncertaintyInMeters = verbatim.CoordinateUncertaintyInMeters ?? DefaultCoordinateUncertaintyInMeters,
                    CountryCode = CountryCode.Sweden,
                    DecimalLatitude = verbatim.DecimalLatitude,
                    DecimalLongitude = verbatim.DecimalLongitude,
                    GeodeticDatum = GeodeticDatum.Wgs84,
                    Continent = new ProcessedFieldMapValue { Id = (int)ContinentId.Europe },
                    Country = new ProcessedFieldMapValue { Id = (int)CountryId.Sweden },
                    Locality = verbatim.Locality,
                    Point = (PointGeoShape)wgs84Point?.ToGeoShape(),
                    PointLocation = wgs84Point?.ToGeoLocation(),
                    PointWithBuffer = (PolygonGeoShape)wgs84Point?.ToCircle(verbatim.CoordinateUncertaintyInMeters)?.ToGeoShape(),
                    VerbatimLatitude = verbatim.DecimalLatitude,
                    VerbatimLongitude = verbatim.DecimalLongitude
                },
                Modified = verbatim.Modified.HasValue ? verbatim.Modified.Value.ToUniversalTime() : (DateTime?)null,
                Occurrence = new ProcessedOccurrence
                {
                    CatalogNumber = GetCatalogNumber(verbatim.OccurrenceId),
                    OccurrenceId = verbatim.OccurrenceId,
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = GetIsNeverFoundObservation(verbatim.DyntaxaTaxonId),
                    IsNotRediscoveredObservation = false,
                    IsPositiveObservation = GetIsPositiveObservation(verbatim.DyntaxaTaxonId),
                    RecordedBy = verbatim.RecordedBy,
                    OccurrenceStatus = GetOccurrenceStatusId(verbatim.DyntaxaTaxonId)
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
        /// Creates occurrence id.
        /// </summary>
        /// <returns>The Catalog Number.</returns>
        private string GetCatalogNumber(string occurrenceId)
        {
            var pos = occurrenceId?.LastIndexOf(":", StringComparison.Ordinal) ?? -1;
            return pos == -1 ? null : occurrenceId?.Substring(pos + 1);
        }

        /// <summary>
        /// Gets the occurrence status. Set to Present if DyntaxaTaxonId from provider is greater than 0 and Absent if DyntaxaTaxonId is 0
        /// </summary>
        private ProcessedFieldMapValue GetOccurrenceStatusId(int dyntaxaTaxonId)
        {
            if (dyntaxaTaxonId == 0)
            {
                return new ProcessedFieldMapValue { Id = (int)OccurrenceStatusId.Absent };
            }

            return new ProcessedFieldMapValue { Id = (int)OccurrenceStatusId.Present };
        }

        /// <summary>
        /// An integer value corresponding to the Enum of the Main field of the SpeciesFact FactorId 761.
        /// By default the value is 1. If the taxon is subordinate to the taxon category Species it is nessecary
        /// to check the Species Fact values of parent taxa.
        /// If the value is greater than 1 for any parent then the value should equal to the max value among parents.
        /// </summary>
        /// <returns></returns>
        private int GetProtectionLevel()
        {
            return 1;
        }

        /// <summary>
        /// Set to False if DyntaxaTaxonId from provider is greater than 0 and True if DyntaxaTaxonId is 0.
        /// </summary>
        private bool GetIsNeverFoundObservation(int dyntaxaTaxonId)
        {
            return dyntaxaTaxonId == 0;
        }

        /// <summary>
        /// Set to True if DyntaxaTaxonId from provider is greater than 0 and False if DyntaxaTaxonId is 0.
        /// </summary>
        private bool GetIsPositiveObservation(int dyntaxaTaxonId)
        {
            return dyntaxaTaxonId != 0;
        }
    }
}