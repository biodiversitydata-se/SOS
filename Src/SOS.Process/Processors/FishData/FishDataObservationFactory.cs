using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nest;
using NetTopologySuite.Geometries;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.FishData;
using SOS.Process.Processors.Interfaces;

namespace SOS.Process.Processors.FishData
{
    public class FishDataObservationFactory : IObservationFactory<FishDataObservationVerbatim>
    {
        private const int DefaultCoordinateUncertaintyInMeters = 500;
        private readonly DataProvider _dataProvider;
        private readonly IDictionary<int, Lib.Models.Processed.Observation.Taxon> _taxa;
        private readonly IAreaHelper _areaHelper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="areaHelper"></param>
        public FishDataObservationFactory(DataProvider dataProvider, IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IAreaHelper areaHelper)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _taxa = taxa ?? throw new ArgumentNullException(nameof(taxa));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <summary>
        ///     Cast multiple clam observations to ProcessedObservation
        /// </summary>
        /// <param name="verbatims"></param>
        /// <returns></returns>
        public IEnumerable<Observation> CreateProcessedObservations(
            IEnumerable<FishDataObservationVerbatim> verbatims)
        {
            return verbatims.Select(CreateProcessedObservation);
        }

        /// <summary>
        ///     Cast KUL observation verbatim to ProcessedObservation
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(FishDataObservationVerbatim verbatim)
        {
            Point wgs84Point = null;
            if (verbatim.DecimalLongitude > 0 && verbatim.DecimalLatitude > 0)
            {
                wgs84Point = new Point(verbatim.DecimalLongitude, verbatim.DecimalLatitude)
                    {SRID = (int) CoordinateSys.WGS84};
            }

            _taxa.TryGetValue(verbatim.DyntaxaTaxonId, out var taxon);

            var obs = new Observation
            {
                AccessRights = new VocabularyValue { Id = (int)AccessRightsId.FreeUsage },
                DataProviderId = _dataProvider.Id,
                BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation},
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.FishData}",
                DatasetName = "Fish data",
                Event = new Event
                {
                    EndDate = verbatim.End.ToUniversalTime(),
                    StartDate = verbatim.Start.ToUniversalTime(),
                    VerbatimEventDate = DwcFormatter.CreateDateIntervalString(verbatim.Start, verbatim.End)
                },
                Identification = new Identification
                {
                    UncertainIdentification = false,
                    Validated = false,
                    ValidationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert }
                },
                Location = new Lib.Models.Processed.Observation.Location
                {
                    CoordinateUncertaintyInMeters =
                        verbatim.CoordinateUncertaintyInMeters ?? DefaultCoordinateUncertaintyInMeters,
                    DecimalLatitude = verbatim.DecimalLatitude,
                    DecimalLongitude = verbatim.DecimalLongitude,
                    GeodeticDatum = GeodeticDatum.Wgs84,
                    Continent = new VocabularyValue { Id = (int)ContinentId.Europe},
                    Country = new VocabularyValue { Id = (int)CountryId.Sweden},
                    Locality = verbatim.Locality,
                    Point = (PointGeoShape) wgs84Point?.ToGeoShape(),
                    PointLocation = wgs84Point?.ToGeoLocation(),
                    PointWithBuffer =
                        (PolygonGeoShape) wgs84Point?.ToCircle(verbatim.CoordinateUncertaintyInMeters)?.ToGeoShape(),
                    VerbatimLatitude = verbatim.DecimalLatitude.ToString(CultureInfo.InvariantCulture),
                    VerbatimLongitude = verbatim.DecimalLongitude.ToString(CultureInfo.InvariantCulture)
                },
                Modified = verbatim.Start,
                Occurrence = new Occurrence
                {
                    CatalogNumber = GetCatalogNumber(verbatim.OccurrenceId),
                    OccurrenceId = verbatim.OccurrenceId,
                    IndividualID = verbatim.IndividualId?.ToString(),
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = GetIsNeverFoundObservation(verbatim.DyntaxaTaxonId),
                    IsNotRediscoveredObservation = false,
                    IsPositiveObservation = GetIsPositiveObservation(verbatim.DyntaxaTaxonId),
                    ProtectionLevel = taxon?.Attributes?.ProtectionLevel?.Id ?? 1,
                    RecordedBy = verbatim.RecordedBy,
                    ReportedBy = verbatim.ReportedBy,
                    ReportedDate = verbatim.Start.ToUniversalTime(),
                    OccurrenceStatus = GetOccurrenceStatusId(verbatim.DyntaxaTaxonId)
                },
                OwnerInstitutionCode = verbatim.Owner,
                Taxon = taxon
            };

            _areaHelper.AddAreaDataToProcessedObservation(obs);

            return obs;
        }

        /// <summary>
        ///     Creates occurrence id.
        /// </summary>
        /// <returns>The Catalog Number.</returns>
        private string GetCatalogNumber(string occurrenceId)
        {
            var pos = occurrenceId?.LastIndexOf(":", StringComparison.Ordinal) ?? -1;
            return pos == -1 ? null : occurrenceId?.Substring(pos + 1);
        }

        /// <summary>
        ///     Gets the occurrence status. Set to Present if DyntaxaTaxonId from provider is greater than 0 and Absent if
        ///     DyntaxaTaxonId is 0
        /// </summary>
        private VocabularyValue GetOccurrenceStatusId(int dyntaxaTaxonId)
        {
            if (dyntaxaTaxonId == 0)
            {
                return new VocabularyValue {Id = (int) OccurrenceStatusId.Absent};
            }

            return new VocabularyValue {Id = (int) OccurrenceStatusId.Present};
        }

        /// <summary>
        ///     Set to False if DyntaxaTaxonId from provider is greater than 0 and True if DyntaxaTaxonId is 0.
        /// </summary>
        private bool GetIsNeverFoundObservation(int dyntaxaTaxonId)
        {
            return dyntaxaTaxonId == 0;
        }

        /// <summary>
        ///     Set to True if DyntaxaTaxonId from provider is greater than 0 and False if DyntaxaTaxonId is 0.
        /// </summary>
        private bool GetIsPositiveObservation(int dyntaxaTaxonId)
        {
            return dyntaxaTaxonId != 0;
        }
    }
}