using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nest;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.ClamPortal;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Process.Processors.ClamPortal
{
    public class ClamPortalObservationFactory
    {
        private const string ValidatedObservationStringValue = "Godkänd";
        private readonly DataProvider _dataProvider;
        private readonly IDictionary<int, Lib.Models.Processed.Observation.Taxon> _taxa;

        public ClamPortalObservationFactory(DataProvider dataProvider, IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _taxa = taxa ?? throw new ArgumentNullException(nameof(taxa));
        }

        /// <summary>
        ///     Cast multiple clam observations to processed observations
        /// </summary>
        /// <param name="verbatimObservations"></param>
        /// <returns></returns>
        public IEnumerable<Observation> CreateProcessedObservations(
            IEnumerable<ClamObservationVerbatim> verbatimObservations)
        {
            return verbatimObservations.Select(CreateProcessedObservation);
        }

        /// <summary>
        ///     Cast clam observation verbatim to processed observation
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(ClamObservationVerbatim verbatimObservation)
        {
            Point wgs84Point = null;
            if (verbatimObservation.DecimalLongitude > 0 && verbatimObservation.DecimalLatitude > 0)
            {
                wgs84Point = new Point(verbatimObservation.DecimalLongitude, verbatimObservation.DecimalLatitude)
                    {SRID = (int) CoordinateSys.WGS84};
            }

            _taxa.TryGetValue(verbatimObservation.DyntaxaTaxonId ?? -1, out var taxon);

            return new Observation
            {
                DataProviderId = _dataProvider.Id,
                AccessRights = GetAccessRightsIdFromString(verbatimObservation.AccessRights),
                BasisOfRecord = GetBasisOfRecordIdFromString(verbatimObservation.BasisOfRecord),
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.ClamGateway}",
                DatasetName = "Träd och musselportalen",
                Event = new Event
                {
                    EndDate = verbatimObservation.ObservationDate.ToUniversalTime(),
                    SamplingProtocol = verbatimObservation.SurveyMethod,
                    StartDate = verbatimObservation.ObservationDate.ToUniversalTime(),
                    VerbatimEventDate = DwcFormatter.CreateDateString(verbatimObservation.ObservationDate)
                },
                Identification = new Identification
                {
                    Validated = verbatimObservation.IdentificationVerificationStatus.Equals(
                        ValidatedObservationStringValue, StringComparison.CurrentCultureIgnoreCase),
                    ValidationStatus =
                        GetValidationStatusIdFromString(verbatimObservation.IdentificationVerificationStatus),
                    UncertainDetermination = verbatimObservation.UncertainDetermination != 0
                },
                InstitutionCode = GetOrganizationIdFromString(verbatimObservation.InstitutionCode),
                Language = verbatimObservation.Language,
                Location = new Lib.Models.Processed.Observation.Location
                {
                    Continent = new VocabularyValue { Id = (int)ContinentId.Europe},
                    CoordinatePrecision = verbatimObservation.CoordinateUncertaintyInMeters,
                    CountryCode = verbatimObservation.CountryCode,
                    DecimalLatitude = verbatimObservation.DecimalLatitude,
                    DecimalLongitude = verbatimObservation.DecimalLongitude,
                    GeodeticDatum = GeodeticDatum.Wgs84,
                    LocationId = verbatimObservation.LocationId,
                    Locality = verbatimObservation.Locality,
                    Point = (PointGeoShape) wgs84Point?.ToGeoShape(),
                    PointLocation = wgs84Point?.ToGeoLocation(),
                    PointWithBuffer = (PolygonGeoShape) wgs84Point
                        ?.ToCircle(verbatimObservation.CoordinateUncertaintyInMeters)?.ToGeoShape(),
                    LocationRemarks = verbatimObservation.LocationRemarks,
                    MaximumDepthInMeters = verbatimObservation.MaximumDepthInMeters,
                    VerbatimLatitude = verbatimObservation.DecimalLatitude.ToString(CultureInfo.InvariantCulture),
                    VerbatimLongitude = verbatimObservation.DecimalLongitude.ToString(CultureInfo.InvariantCulture),
                    VerbatimCoordinateSystem = "EPSG:4326",
                    WaterBody = verbatimObservation.WaterBody
                },
                Modified = verbatimObservation.Modified?.ToUniversalTime() ?? DateTime.MinValue.ToUniversalTime(),
                Occurrence = new Occurrence
                {
                    CatalogNumber = verbatimObservation.CatalogNumber.ToString(),
                    OccurrenceId = verbatimObservation.OccurrenceId.Trim(),
                    IndividualCount = verbatimObservation.IndividualCount,
                    IsNaturalOccurrence = verbatimObservation.IsNaturalOccurrence,
                    IsNeverFoundObservation = verbatimObservation.IsNeverFoundObservation,
                    IsNotRediscoveredObservation = verbatimObservation.IsNotRediscoveredObservation,
                    IsPositiveObservation = verbatimObservation.IsPositiveObservation,
                    LifeStage = GetLifeStageIdFromString(verbatimObservation.LifeStage),
                    OrganismQuantityInt = verbatimObservation.Quantity,
                    OrganismQuantity = verbatimObservation.Quantity.ToString(),
                    OrganismQuantityUnit = GetOrganismQuantityUnitIdFromString(verbatimObservation.QuantityUnit),
                    RecordedBy = verbatimObservation.RecordedBy,
                    OccurrenceRemarks = verbatimObservation.OccurrenceRemarks,
                    OccurrenceStatus = GetOccurrenceStatusIdFromString(verbatimObservation.OccurrenceStatus)
                },
                DynamicProperties = string.IsNullOrEmpty(verbatimObservation.ProjectName)
                    ? null
                    : JsonConvert.SerializeObject(new {ProjectName = verbatimObservation.ProjectName}),
                ReportedBy = verbatimObservation.ReportedBy,
                ReportedDate = verbatimObservation.ReportedDate.ToUniversalTime(),
                RightsHolder = verbatimObservation.RightsHolder,
                Taxon = taxon
            };
        }

        private VocabularyValue GetBasisOfRecordIdFromString(string basisOfRecord)
        {
            if (string.IsNullOrEmpty(basisOfRecord)) return null;

            switch (basisOfRecord)
            {
                case "Human observation":
                    return new VocabularyValue
                    {
                        Id = (int) BasisOfRecordId.HumanObservation
                    };

                default:
                    return new VocabularyValue
                    {
                        Id = VocabularyConstants.NoMappingFoundCustomValueIsUsedId,
                        Value = basisOfRecord
                    };
            }
        }

        private VocabularyValue GetAccessRightsIdFromString(string accessRights)
        {
            if (string.IsNullOrEmpty(accessRights)) return null;

            switch (accessRights)
            {
                case "FreeUsage":
                    return new VocabularyValue
                    {
                        Id = (int) AccessRightsId.FreeUsage
                    };

                default:
                    return new VocabularyValue
                    {
                        Id = VocabularyConstants.NoMappingFoundCustomValueIsUsedId,
                        Value = accessRights
                    };
            }
        }

        private VocabularyValue GetOccurrenceStatusIdFromString(string occurrenceStatus)
        {
            if (string.IsNullOrEmpty(occurrenceStatus)) return null;

            switch (occurrenceStatus)
            {
                case "Present":
                    return new VocabularyValue
                    {
                        Id = (int) OccurrenceStatusId.Present
                    };

                case "Not rediscovered":
                    return new VocabularyValue
                    {
                        Id = (int) OccurrenceStatusId.Absent
                    };

                default:
                    return new VocabularyValue
                    {
                        Id = VocabularyConstants.NoMappingFoundCustomValueIsUsedId,
                        Value = occurrenceStatus
                    };
            }
        }

        private VocabularyValue GetOrganismQuantityUnitIdFromString(string quantityUnit)
        {
            if (string.IsNullOrEmpty(quantityUnit)) return null;

            switch (quantityUnit)
            {
                case "Antal individer":
                    return new VocabularyValue
                    {
                        Id = (int) UnitId.Individuals
                    };

                default:
                    return new VocabularyValue
                    {
                        Id = VocabularyConstants.NoMappingFoundCustomValueIsUsedId,
                        Value = quantityUnit
                    };
            }
        }

        private VocabularyValue GetOrganizationIdFromString(string institutionCode)
        {
            if (string.IsNullOrEmpty(institutionCode)) return null;

            switch (institutionCode)
            {
                case "ArtDatabanken":
                    return new VocabularyValue
                    {
                        Id = (int) OrganizationId.ArtDatabanken
                    };

                default:
                    return new VocabularyValue
                    {
                        Id = VocabularyConstants.NoMappingFoundCustomValueIsUsedId,
                        Value = institutionCode
                    };
            }
        }

        private VocabularyValue GetLifeStageIdFromString(string lifeStage)
        {
            if (string.IsNullOrEmpty(lifeStage)) return null;
            // Sample values from ClamPortal web service for LifeStage field:
            // "Ant. lev:31,död:1"
            // "Ant. lev:13,död:12"

            // todo - should we return null or NoMappingFoundCustomValueIsUsedId?
            return new VocabularyValue
            {
                Id = VocabularyConstants.NoMappingFoundCustomValueIsUsedId,
                Value = lifeStage
            };
            //return null; // no valid values for LifeStage
        }

        private VocabularyValue GetValidationStatusIdFromString(string validationStatus)
        {
            if (string.IsNullOrEmpty(validationStatus)) return null;
            if (validationStatus.Equals(ValidatedObservationStringValue, StringComparison.CurrentCultureIgnoreCase))
            {
                return new VocabularyValue
                {
                    Id = (int) ValidationStatusId.ApprovedBasedOnReportersDocumentation
                };
            }

            return new VocabularyValue
            {
                Id = VocabularyConstants.NoMappingFoundCustomValueIsUsedId,
                Value = validationStatus
            };
        }
    }
}