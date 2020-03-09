using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.GeoJsonObjectModel;
using NetTopologySuite.Geometries;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.FieldMappingValues;
using SOS.Lib.Extensions;
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.ClamPortal;

namespace SOS.Process.Extensions
{
    /// <summary>
    /// Entity extensions
    /// </summary>
    public static class ClamPortalExtensions
    {
        private const string ValidatedObservationStringValue = "Godkänd";

        /// <summary>
        /// Cast clam observation verbatim to processed sighting
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static ProcessedSighting ToProcessed(this ClamObservationVerbatim verbatim, IDictionary<int, ProcessedTaxon> taxa)
        {
            Point wgs84Point = null;
            if (verbatim.DecimalLongitude > 0 && verbatim.DecimalLatitude > 0)
            {
                wgs84Point = new Point(verbatim.DecimalLongitude, verbatim.DecimalLatitude) { SRID = (int)CoordinateSys.WGS84 };
            }
            
            taxa.TryGetValue(verbatim.DyntaxaTaxonId ?? -1, out var taxon);

            return new ProcessedSighting(DataProvider.ClamPortal)
            {
                AccessRightsId = GetAccessRightsIdFromString(verbatim.AccessRights),
                BasisOfRecordId = GetBasisOfRecordIdFromString(verbatim.BasisOfRecord),
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProvider.ClamPortal.ToString()}",
                DatasetName = "Träd och musselportalen",
                Event = new ProcessedEvent
                {
                    EndDate = verbatim.ObservationDate.ToUniversalTime(),
                    SamplingProtocol = verbatim.SurveyMethod,
                    StartDate = verbatim.ObservationDate.ToUniversalTime(),
                    VerbatimEndDate = verbatim.ObservationDate,
                    VerbatimStartDate = verbatim.ObservationDate
                },
                Identification = new ProcessedIdentification
                {
                    Validated = verbatim.IdentificationVerificationStatus.Equals(ValidatedObservationStringValue, StringComparison.CurrentCultureIgnoreCase),
                    ValidationStatusId = GetValidationStatusIdFromString(verbatim.IdentificationVerificationStatus),
                    UncertainDetermination = verbatim.UncertainDetermination != 0
                },
                OrganizationId = GetOrganizationIdFromString(verbatim.InstitutionCode),
                Language = verbatim.Language,
                Location = new ProcessedLocation
                {
                    ContinentId = new ProcessedFieldMapValue { Id = (int)ContinentId.Europe },
                    CoordinatePrecision = verbatim.CoordinateUncertaintyInMeters,
                    CountryCode = verbatim.CountryCode,
                    DecimalLatitude = verbatim.DecimalLatitude,
                    DecimalLongitude = verbatim.DecimalLongitude,
                    GeodeticDatum = GeodeticDatum.Wgs84,
                    Id = verbatim.LocationId,
                    Locality = verbatim.Locality,
                    Point = (GeoJsonPoint<GeoJson2DGeographicCoordinates>)wgs84Point?.ToGeoJsonGeometry(),
                    PointWithBuffer = wgs84Point?.ToCircle(verbatim.CoordinateUncertaintyInMeters)?.ToGeoJsonGeometry(),
                    Remarks = verbatim.LocationRemarks,
                    MaximumDepthInMeters = verbatim.MaximumDepthInMeters,
                    VerbatimLatitude = verbatim.DecimalLatitude,
                    VerbatimLongitude = verbatim.DecimalLongitude,
                    VerbatimCoordinateSystem = "EPSG:4326",
                    WaterBody = verbatim.WaterBody
                },
                Modified = verbatim.Modified ?? DateTime.MinValue,
                Occurrence = new ProcessedOccurrence
                {
                    CatalogNumber = verbatim.CatalogNumber.ToString(),
                    Id = verbatim.OccurrenceId,
                    IndividualCount = verbatim.IndividualCount,
                    IsNaturalOccurrence = verbatim.IsNaturalOccurrence,
                    IsNeverFoundObservation = verbatim.IsNeverFoundObservation,
                    IsNotRediscoveredObservation = verbatim.IsNotRediscoveredObservation,
                    IsPositiveObservation = verbatim.IsPositiveObservation,
                    LifeStageId = GetLifeStageIdFromString(verbatim.LifeStage),
                    OrganismQuantity = verbatim.Quantity,
                    OrganismQuantityUnitId = GetOrganismQuantityUnitIdFromString(verbatim.QuantityUnit),
                    RecordedBy = verbatim.RecordedBy,
                    Remarks = verbatim.OccurrenceRemarks,
                    OccurrenceStatusId = GetOccurrenceStatusIdFromString(verbatim.OccurrenceStatus)
                },
                Projects = string.IsNullOrEmpty(verbatim.ProjectName) ? null : new[]
                {
                    new ProcessedProject
                    {
                        Name = verbatim.ProjectName
                    }
                },
                ReportedBy = verbatim.ReportedBy,
                ReportedDate = verbatim.ReportedDate,
                RightsHolder = verbatim.RightsHolder,
                Taxon = taxon
            };
        }

        private static ProcessedFieldMapValue GetBasisOfRecordIdFromString(string basisOfRecord)
        {
            if (string.IsNullOrEmpty(basisOfRecord)) return null;

            switch (basisOfRecord)
            {
                case "Human observation":
                    return new ProcessedFieldMapValue
                    {
                        Id = (int)BasisOfRecordId.HumanObservation
                    };

                default:
                    return new ProcessedFieldMapValue
                    {
                        Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId,
                        Value = basisOfRecord
                    };
            }
        }

        private static ProcessedFieldMapValue GetAccessRightsIdFromString(string accessRights)
        {
            if (string.IsNullOrEmpty(accessRights)) return null;

            switch (accessRights)
            {
                case "FreeUsage":
                    return new ProcessedFieldMapValue
                    {
                        Id = (int)AccessRightsId.FreeUsage
                    };

                default:
                    return new ProcessedFieldMapValue
                    {
                        Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId,
                        Value = accessRights
                    };
            }
        }

        private static ProcessedFieldMapValue GetOccurrenceStatusIdFromString(string occurrenceStatus)
        {
            if (string.IsNullOrEmpty(occurrenceStatus)) return null;

            switch (occurrenceStatus)
            {
                case "Present":
                    return new ProcessedFieldMapValue
                    {
                        Id = (int)OccurrenceStatusId.Present
                    };

                case "Not rediscovered":
                    return new ProcessedFieldMapValue
                    {
                        Id = (int)OccurrenceStatusId.Absent
                    };

                default:
                    return new ProcessedFieldMapValue
                    {
                        Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId,
                        Value = occurrenceStatus
                    };
            }
        }

        private static ProcessedFieldMapValue GetOrganismQuantityUnitIdFromString(string quantityUnit)
        {
            if (string.IsNullOrEmpty(quantityUnit)) return null;

            switch (quantityUnit)
            {
                case "Antal individer":
                    return new ProcessedFieldMapValue
                    {
                        Id = (int)UnitId.Individuals
                    };

                default:
                    return new ProcessedFieldMapValue
                    {
                        Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId,
                        Value = quantityUnit
                    };
            }
        }

        private static ProcessedFieldMapValue GetOrganizationIdFromString(string institutionCode)
        {
            if (string.IsNullOrEmpty(institutionCode)) return null;

            switch (institutionCode)
            {
                case "ArtDatabanken":
                    return new ProcessedFieldMapValue
                    {
                        Id = (int)OrganizationId.ArtDatabanken
                    };

                default:
                    return new ProcessedFieldMapValue
                    {
                        Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId,
                        Value = institutionCode
                    };
            }
        }

        private static ProcessedFieldMapValue GetLifeStageIdFromString(string lifeStage)
        {
            if (string.IsNullOrEmpty(lifeStage)) return null;
            // Sample values from ClamPortal web service for LifeStage field:
            // "Ant. lev:31,död:1"
            // "Ant. lev:13,död:12"

            // todo - should we return null or NoMappingFoundCustomValueIsUsedId?
            return new ProcessedFieldMapValue
            {
                Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId,
                Value = lifeStage
            };
            //return null; // no valid values for LifeStage
        }

        private static ProcessedFieldMapValue GetValidationStatusIdFromString(string validationStatus)
        {
            if (string.IsNullOrEmpty(validationStatus)) return null;
            if (validationStatus.Equals(ValidatedObservationStringValue, StringComparison.CurrentCultureIgnoreCase))
            {
                return new ProcessedFieldMapValue
                {
                    Id = (int)ValidationStatusId.ApprovedBasedOnReportersDocumentation
                };
            }

            return new ProcessedFieldMapValue
            {
                Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId,
                Value = validationStatus
            };
        }

        /// <summary>
        /// Cast multiple clam observations to processed sightings
        /// </summary>
        /// <param name="verbatims"></param>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public static IEnumerable<ProcessedSighting> ToProcessed(this IEnumerable<ClamObservationVerbatim> verbatims, IDictionary<int, ProcessedTaxon> taxa)
        {
            return verbatims.Select(v => v.ToProcessed(taxa));
        }
    }
}
