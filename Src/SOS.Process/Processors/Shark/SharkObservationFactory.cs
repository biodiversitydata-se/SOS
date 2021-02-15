using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using NetTopologySuite.Geometries;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shark;
using SOS.Process.Constants;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Process.Processors.Shark
{
    public class SharkObservationFactory
    {
        private const int DefaultCoordinateUncertaintyInMeters = 500;
        private readonly DataProvider _dataProvider;
        private readonly IDictionary<int, Lib.Models.Processed.Observation.Taxon> _taxa;

        public SharkObservationFactory(DataProvider dataProvider, IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _taxa = taxa ?? throw new ArgumentNullException(nameof(taxa));
        }

        /// <summary>
        ///     Cast multiple clam observations to ProcessedObservation
        /// </summary>
        /// <param name="verbatims"></param>
        /// <returns></returns>
        public IEnumerable<Observation> CreateProcessedObservations(
            IEnumerable<SharkObservationVerbatim> verbatims)
        {
            return verbatims.Select(CreateProcessedObservation);
        }

        /// <summary>
        ///     Cast Shark observation verbatim to ProcessedObservation
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(SharkObservationVerbatim verbatim)
        {
            Point wgs84Point = null;
            if (verbatim.SampleLatitudeDd.HasValue && verbatim.SampleLongitudeDd.HasValue)
            {
                wgs84Point = new Point(verbatim.SampleLongitudeDd.Value, verbatim.SampleLatitudeDd.Value)
                    {SRID = (int) CoordinateSys.WGS84};
            }

            _taxa.TryGetValue(verbatim.DyntaxaId.HasValue ? verbatim.DyntaxaId.Value : -1, out var taxon);
            var sharkSampleId = verbatim.Sharksampleidmd5 ?? verbatim.SharkSampleId;
            var obs = new Observation
            {
                DataProviderId = _dataProvider.Id,
                BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation},
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.SHARK}",
                DatasetName = verbatim.DatasetName,
                Event = new Event
                {
                    EndDate = verbatim.SampleDate?.ToUniversalTime(),
                    StartDate = verbatim.SampleDate?.ToUniversalTime(),
                    VerbatimEventDate = DwcFormatter.CreateDateString(verbatim.SampleDate)
                },
                Identification = new Identification
                {
                    IdentifiedBy = verbatim.AnalysedBy,
                    UncertainDetermination = false,
                    Validated = false,
                    ValidationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert }
                },
                Location = new Lib.Models.Processed.Observation.Location
                {
                    CoordinateUncertaintyInMeters = DefaultCoordinateUncertaintyInMeters,
                    CountryCode = CountryCode.Sweden,
                    DecimalLatitude = verbatim.SampleLatitudeDd,
                    DecimalLongitude = verbatim.SampleLongitudeDd,
                    GeodeticDatum = GeodeticDatum.Wgs84,
                    Continent = new VocabularyValue { Id = (int)ContinentId.Europe},
                    Country = new VocabularyValue { Id = (int)CountryId.Sweden},
                    MaximumDepthInMeters = verbatim.WaterDepthM,
                    MinimumDepthInMeters = verbatim.WaterDepthM,
                    Point = (PointGeoShape) wgs84Point?.ToGeoShape(),
                    PointLocation = wgs84Point?.ToGeoLocation(),
                    PointWithBuffer = (PolygonGeoShape) wgs84Point?.ToCircle(ProcessConstants.DefaultAccuracyInMeters)
                        ?.ToGeoShape(),
                    VerbatimLatitude = verbatim.SampleLatitudeDd?.ToString(),
                    VerbatimLongitude = verbatim.SampleLongitudeDd?.ToString()
                },
                Occurrence = new Occurrence
                {
                    CatalogNumber = sharkSampleId,
                    OccurrenceId = $"urn:lsid:shark:Sighting:{sharkSampleId}",
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = GetIsNeverFoundObservation(verbatim.DyntaxaId),
                    IsNotRediscoveredObservation = false,
                    IsPositiveObservation = GetIsPositiveObservation(verbatim.DyntaxaId),
                    ProtectionLevel = taxon?.Attributes?.ProtectionLevel?.Id ?? 1,
                    RecordedBy = verbatim.Taxonomist,
                    ReportedBy = verbatim.ReportedStationName,
                    OccurrenceStatus = GetOccurrenceStatusId(verbatim.DyntaxaId)
                },
                OwnerInstitutionCode = verbatim.ReportingInstituteNameSv,
                Taxon = taxon
            };

            /*
            DataType
            SamplerType
            Species
            ReportingInstitutionCode ?
            AnalyticalLaboratoryCode ?
            Status => Occurrence.Status ?

             */

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
        private VocabularyValue GetOccurrenceStatusId(int? dyntaxaTaxonId)
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
        private bool GetIsNeverFoundObservation(int? dyntaxaTaxonId)
        {
            return dyntaxaTaxonId == 0;
        }

        /// <summary>
        ///     Set to True if DyntaxaTaxonId from provider is greater than 0 and False if DyntaxaTaxonId is 0.
        /// </summary>
        private bool GetIsPositiveObservation(int? dyntaxaTaxonId)
        {
            return dyntaxaTaxonId != 0;
        }
    }
}