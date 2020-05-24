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
using SOS.Lib.Models.Verbatim.ClamPortal;

namespace SOS.Process.Processors.ClamPortal
{
    public class ClamPortalObservationFactory
    {
        private const string ValidatedObservationStringValue = "Godkänd";
        private readonly IDictionary<int, ProcessedTaxon> _taxa;

        public ClamPortalObservationFactory(IDictionary<int, ProcessedTaxon> taxa)
        {
            _taxa = taxa ?? throw new ArgumentNullException(nameof(taxa));
        }

        /// <summary>
        /// Cast multiple clam observations to processed observations
        /// </summary>
        /// <param name="verbatimObservations"></param>
        /// <returns></returns>
        public IEnumerable<ProcessedObservation> CreateProcessedObservations(IEnumerable<ClamObservationVerbatim> verbatimObservations)
        {
            return verbatimObservations.Select(CreateProcessedObservation);
        }

        /// <summary>
        /// Cast clam observation verbatim to processed observation
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <returns></returns>
        public ProcessedObservation CreateProcessedObservation(ClamObservationVerbatim verbatimObservation)
        {
            Point wgs84Point = null;
            if (verbatimObservation.DecimalLongitude > 0 && verbatimObservation.DecimalLatitude > 0)
            {
                wgs84Point = new Point(verbatimObservation.DecimalLongitude, verbatimObservation.DecimalLatitude) { SRID = (int)CoordinateSys.WGS84 };
            }

            _taxa.TryGetValue(verbatimObservation.DyntaxaTaxonId ?? -1, out var taxon);

            return new ProcessedObservation()
            {
                AccessRights = GetAccessRightsIdFromString(verbatimObservation.AccessRights),
                BasisOfRecord = GetBasisOfRecordIdFromString(verbatimObservation.BasisOfRecord),
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.ClamGateway}",
                DatasetName = "Träd och musselportalen",
                Event = new ProcessedEvent
                {
                    EndDate = verbatimObservation.ObservationDate.ToUniversalTime(),
                    SamplingProtocol = verbatimObservation.SurveyMethod,
                    StartDate = verbatimObservation.ObservationDate.ToUniversalTime(),
                    VerbatimEventDate = DwcFormatter.CreateDateString(verbatimObservation.ObservationDate)
                },
                Identification = new ProcessedIdentification
                {
                    Validated = verbatimObservation.IdentificationVerificationStatus.Equals(ValidatedObservationStringValue, StringComparison.CurrentCultureIgnoreCase),
                    ValidationStatus = GetValidationStatusIdFromString(verbatimObservation.IdentificationVerificationStatus),
                    UncertainDetermination = verbatimObservation.UncertainDetermination != 0
                },
                InstitutionId = GetOrganizationIdFromString(verbatimObservation.InstitutionCode),
                Language = verbatimObservation.Language,
                Location = new ProcessedLocation
                {
                    Continent = new ProcessedFieldMapValue { Id = (int)ContinentId.Europe },
                    CoordinatePrecision = verbatimObservation.CoordinateUncertaintyInMeters,
                    CountryCode = verbatimObservation.CountryCode,
                    DecimalLatitude = verbatimObservation.DecimalLatitude,
                    DecimalLongitude = verbatimObservation.DecimalLongitude,
                    GeodeticDatum = GeodeticDatum.Wgs84,
                    LocationId = verbatimObservation.LocationId,
                    Locality = verbatimObservation.Locality,
                    Point = (PointGeoShape)wgs84Point?.ToGeoShape(),
                    PointLocation = wgs84Point?.ToGeoLocation(),
                    PointWithBuffer = (PolygonGeoShape)wgs84Point?.ToCircle(verbatimObservation.CoordinateUncertaintyInMeters)?.ToGeoShape(),
                    LocationRemarks = verbatimObservation.LocationRemarks,
                    MaximumDepthInMeters = verbatimObservation.MaximumDepthInMeters,
                    VerbatimLatitude = verbatimObservation.DecimalLatitude,
                    VerbatimLongitude = verbatimObservation.DecimalLongitude,
                    VerbatimCoordinateSystem = "EPSG:4326",
                    WaterBody = verbatimObservation.WaterBody
                },
                Modified = verbatimObservation.Modified ?? DateTime.MinValue,
                Occurrence = new ProcessedOccurrence
                {
                    CatalogNumber = verbatimObservation.CatalogNumber.ToString(),
                    OccurrenceId = verbatimObservation.OccurrenceId,
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
                Projects = string.IsNullOrEmpty(verbatimObservation.ProjectName) ? null : new[]
                {
                    new ProcessedProject
                    {
                        Name = verbatimObservation.ProjectName
                    }
                },
                ReportedBy = verbatimObservation.ReportedBy,
                ReportedDate = verbatimObservation.ReportedDate,
                RightsHolder = verbatimObservation.RightsHolder,
                Taxon = taxon
            };
        }

        private ProcessedFieldMapValue GetBasisOfRecordIdFromString(string basisOfRecord)
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

        private ProcessedFieldMapValue GetAccessRightsIdFromString(string accessRights)
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

        private ProcessedFieldMapValue GetOccurrenceStatusIdFromString(string occurrenceStatus)
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

        private ProcessedFieldMapValue GetOrganismQuantityUnitIdFromString(string quantityUnit)
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

        private ProcessedFieldMapValue GetOrganizationIdFromString(string institutionCode)
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

        private ProcessedFieldMapValue GetLifeStageIdFromString(string lifeStage)
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

        private ProcessedFieldMapValue GetValidationStatusIdFromString(string validationStatus)
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
    }
}
