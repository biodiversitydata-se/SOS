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
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.ObservationDatabase;

namespace SOS.Process.Processors.ObservationDatabase
{
    /// <summary>
    /// Observation database factory
    /// </summary>
    public class ObservationDatabaseObservationFactory
    {
        private readonly DataProvider _dataProvider;
        private readonly IDictionary<int, Lib.Models.Processed.Observation.Taxon> _taxa;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        public ObservationDatabaseObservationFactory(DataProvider dataProvider, IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa)
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
            IEnumerable<ObservationDatabaseVerbatim> verbatims)
        {
            return verbatims.Select(CreateProcessedObservation);
        }

        /// <summary>
        ///     Cast KUL observation verbatim to ProcessedObservation
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(ObservationDatabaseVerbatim verbatim)
        {
            Point wgs84Point = null;
            if (verbatim.CoordinateX > 0 && verbatim.CoordinateY > 0)
            {
                var webMercatorPoint = new Point(verbatim.CoordinateX, verbatim.CoordinateY);
                wgs84Point = webMercatorPoint.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84) as Point;
            }

            _taxa.TryGetValue(verbatim.TaxonId, out var taxon);

            var obs = new Observation
            {
                AccessRights = new VocabularyValue { Id = (int)AccessRightsId.NotForPublicUsage },
                DataProviderId = _dataProvider.Id,
                BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation },
                CollectionCode = verbatim.CollectionCode,
                CollectionId = verbatim.CollectionId,
                DatasetId = $"urn:lsid:observationsdatabasen.se:dataprovider:{DataProviderIdentifiers.ObservationDatabase}",
                DatasetName = "Observation database",
                Event = new Event
                {
                    EndDate = verbatim.EndDate.ToUniversalTime(),
                    FieldNotes = verbatim.Origin, // Is there any better field for this?
                    Habitat = verbatim.Habitat,
                    StartDate = verbatim.StartDate.ToUniversalTime(),
                   // Substrate = verbatim.Substrate, Todo map 
                    VerbatimEventDate = DwcFormatter.CreateDateIntervalString(verbatim.StartDate, verbatim.EndDate)
                },
                Identification = new Identification
                {
                    UncertainDetermination = false,
                    Validated = false,
                    ValidationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert }
                },
                InstitutionId = verbatim.SCI_code,
                Location = new Lib.Models.Processed.Observation.Location
                {
                    Attributes = new LocationAttributes
                    {
                        VerbatimMunicipality = verbatim.Municipality,
                        VerbatimProvince = verbatim.Province
                    },
                    Continent = new VocabularyValue { Id = (int)ContinentId.Europe },
                    CoordinateUncertaintyInMeters = verbatim.CoordinateUncertaintyInMeters,
                    Country = new VocabularyValue { Id = (int)CountryId.Sweden },
                    CountryCode = CountryCode.Sweden,
                    DecimalLatitude = wgs84Point?.Y ?? 0,
                    DecimalLongitude = wgs84Point?.X ?? 0,
                    GeodeticDatum = GeodeticDatum.Wgs84,
                    Locality = verbatim.Locality,
                    Point = (PointGeoShape)wgs84Point?.ToGeoShape(),
                    PointLocation = wgs84Point?.ToGeoLocation(),
                    PointWithBuffer =
                        (PolygonGeoShape)wgs84Point?.ToCircle(verbatim.CoordinateUncertaintyInMeters)?.ToGeoShape(),
                    VerbatimLatitude = verbatim.CoordinateY.ToString(CultureInfo.InvariantCulture),
                    VerbatimLongitude = verbatim.CoordinateX.ToString(CultureInfo.InvariantCulture),
                    VerbatimCoordinateSystem = "EPSG:3857"
                },
                Modified = verbatim.EditDate,
                Occurrence = new Occurrence
                {
                    CatalogId = verbatim.Id,
                    CatalogNumber = verbatim.Id.ToString(),
                    IndividualCount = verbatim.IndividualCount,
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = verbatim.IsNeverFoundObservation,
                    IsNotRediscoveredObservation = verbatim.IsNotRediscoveredObservation,
                    IsPositiveObservation = verbatim.TaxonId != 0,
                    OccurrenceId = $"urn:lsid:observationsdatabasen.se:Observation:{verbatim.Id}",
                    OccurrenceRemarks = verbatim.OccurrenceRemarks,
                    OccurrenceStatus = new VocabularyValue { Id = (int)(verbatim.TaxonId == 0 ? OccurrenceStatusId.Absent : OccurrenceStatusId.Present) },
                    ProtectionLevel = verbatim.ProtectionLevel,
                    RecordedBy = verbatim.Observers,
                    ReportedDate = verbatim.StartDate.ToUniversalTime()
                },
                OwnerInstitutionCode = verbatim.SCI_code,
                RightsHolder = verbatim.SCI_name,
                Taxon = taxon
            };

            return obs;
        }
    }
}