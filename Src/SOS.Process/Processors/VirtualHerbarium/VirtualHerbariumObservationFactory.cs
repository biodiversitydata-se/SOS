﻿using System;
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
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Process.Processors.VirtualHerbarium
{
    public class VirtualHerbariumObservationFactory
    {
        private readonly DataProvider _dataProvider;
        private readonly IDictionary<int, Lib.Models.Processed.Observation.Taxon> _taxa;
       

        public VirtualHerbariumObservationFactory(DataProvider dataProvider, IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa)
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
            IEnumerable<VirtualHerbariumObservationVerbatim> verbatims)
        {
            return verbatims.Select(CreateProcessedObservation);
        }

        /// <summary>
        ///     Cast Virtual Herbarium observation verbatim to ProcessedObservation
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(VirtualHerbariumObservationVerbatim verbatim)
        {
            if (verbatim == null)
            {
                return null;
            }

            Point wgs84Point = null;
            if (verbatim.DecimalLongitude > 0 && verbatim.DecimalLatitude > 0)
            {
                wgs84Point = new Point(verbatim.DecimalLongitude, verbatim.DecimalLatitude)
                    { SRID = (int)CoordinateSys.WGS84 };
            }

            _taxa.TryGetValue(verbatim.DyntaxaId, out var taxon);

            var defects = new Dictionary<string, string>();
            DateTime? dateCollected = null;
            if (DateTime.TryParse(verbatim.DateCollected, out var date))
            {
                dateCollected = date;
            }
            else // In correct date, add it to defects
            {
                defects.Add("DateCollected", verbatim.DateCollected);
            }

            var obs = new Observation
            {
                DataProviderId = _dataProvider.Id,
                BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation},
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.VirtualHerbarium}",
                DatasetName = "Virtual Herbarium",
                Defects = defects.Count == 0 ? null : defects,
                Event = new Event
                {
                    EndDate = dateCollected?.ToUniversalTime(),
                    StartDate = dateCollected?.ToUniversalTime(),
                    VerbatimEventDate = DwcFormatter.CreateDateString(dateCollected)
                },
                Identification = new Identification
                {
                    Validated = false,
                    UncertainDetermination = false
                },
                Location = new Lib.Models.Processed.Observation.Location
                {
                    CoordinateUncertaintyInMeters = verbatim.CoordinatePrecision,
                    CountryCode = CountryCode.Sweden,
                    DecimalLatitude = verbatim.DecimalLatitude,
                    DecimalLongitude = verbatim.DecimalLongitude,
                    GeodeticDatum = GeodeticDatum.Wgs84,
                    Continent = new VocabularyValue { Id = (int)ContinentId.Europe},
                    Country = new VocabularyValue { Id = (int)CountryId.Sweden},
                    Point = (PointGeoShape) wgs84Point?.ToGeoShape(),
                    PointLocation = wgs84Point?.ToGeoLocation(),
                    PointWithBuffer =
                        (PolygonGeoShape) wgs84Point?.ToCircle(verbatim.CoordinatePrecision)?.ToGeoShape(),
                    VerbatimLatitude = verbatim.DecimalLatitude.ToString(CultureInfo.InvariantCulture),
                    VerbatimLongitude = verbatim.DecimalLongitude.ToString(CultureInfo.InvariantCulture)
                },
                Occurrence = new Occurrence
                {
                    CatalogNumber = verbatim.AccessionNo,
                    OccurrenceId = verbatim.AccessionNo,
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = GetIsNeverFoundObservation(verbatim.DyntaxaId),
                    IsNotRediscoveredObservation = false,
                    IsPositiveObservation = GetIsPositiveObservation(verbatim.DyntaxaId),
                    OccurrenceStatus = GetOccurrenceStatusId(verbatim.DyntaxaId),
                    RecordedBy = verbatim.Collector,
                    OccurrenceRemarks = verbatim.Notes
                },
                OwnerInstitutionCode = verbatim.InstitutionCode,
                ProtectionLevel = GetProtectionLevel(),
                Taxon = taxon
            };

            return obs;
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
        ///     An integer value corresponding to the Enum of the Main field of the SpeciesFact FactorId 761.
        ///     By default the value is 1. If the taxon is subordinate to the taxon category Species it is nessecary
        ///     to check the Species Fact values of parent taxa.
        ///     If the value is greater than 1 for any parent then the value should equal to the max value among parents.
        /// </summary>
        /// <returns></returns>
        private int GetProtectionLevel()
        {
            return 1;
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