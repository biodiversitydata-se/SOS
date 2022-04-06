﻿using Newtonsoft.Json;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Harvest.Processors.ClamPortal
{
    public class ClamPortalObservationFactory : ObservationFactoryBase, IObservationFactory<ClamObservationVerbatim>
    {
        private const string ValidatedObservationStringValue = "Godkänd";
        private readonly DataProvider _dataProvider;        
        private readonly IAreaHelper _areaHelper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processTimeManager"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ClamPortalObservationFactory(DataProvider dataProvider, 
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, 
            IAreaHelper areaHelper,
            IProcessTimeManager processTimeManager) : base(taxa, processTimeManager)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));            
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <summary>
        ///     Cast verbatim observations to processed data model
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(ClamObservationVerbatim verbatim, bool diffuseIfSupported)
        {
            var taxon = GetTaxon(verbatim.DyntaxaTaxonId ?? -1);
            var accessRights = GetAccessRightsIdFromString(verbatim.AccessRights);

            var obs = new Observation
            {
                DataProviderId = _dataProvider.Id,
                AccessRights = accessRights,
                BasisOfRecord = GetBasisOfRecordIdFromString(verbatim.BasisOfRecord),
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.ClamGateway}",
                DatasetName = "Träd och musselportalen",
                DiffusionStatus = DiffusionStatus.NotDiffused,
                Event = new Event
                {
                    EndDate = verbatim.ObservationDate.ToUniversalTime(),
                    SamplingProtocol = verbatim.SurveyMethod,
                    StartDate = verbatim.ObservationDate.ToUniversalTime(),
                    PlainStartDate = verbatim.ObservationDate.ToLocalTime().ToString("yyyy-MM-dd"),
                    PlainEndDate = verbatim.ObservationDate.ToLocalTime().ToString("yyyy-MM-dd"),
                    PlainStartTime = null,
                    PlainEndTime = null,
                    VerbatimEventDate = DwcFormatter.CreateDateString(verbatim.ObservationDate.ToLocalTime())
                },
                Identification = new Identification
                {
                    Validated = verbatim.IdentificationVerificationStatus.Equals(
                        ValidatedObservationStringValue, StringComparison.CurrentCultureIgnoreCase),
                    Verified = verbatim.IdentificationVerificationStatus.Equals(
                        ValidatedObservationStringValue, StringComparison.CurrentCultureIgnoreCase),
                    ValidationStatus =
                        GetValidationStatusIdFromString(verbatim.IdentificationVerificationStatus),
                    VerificationStatus = 
                        GetValidationStatusIdFromString(verbatim.IdentificationVerificationStatus),
                    UncertainIdentification = verbatim.UncertainDetermination != 0
                },
                InstitutionCode = GetOrganizationIdFromString(verbatim.InstitutionCode),
                Language = verbatim.Language,
                Location = new Location()
                {
                    CountryCode = verbatim.CountryCode,
                    LocationId = verbatim.LocationId,
                    Locality = verbatim.Locality,
                    LocationRemarks = verbatim.LocationRemarks,
                    MaximumDepthInMeters = verbatim.MaximumDepthInMeters,
                    VerbatimCoordinateSystem = "EPSG:4326",
                    WaterBody = verbatim.WaterBody
                },
                Modified = verbatim.Modified?.ToUniversalTime() ?? DateTime.MinValue.ToUniversalTime(),
                Occurrence = new Occurrence
                {
                    BirdNestActivityId = 0,
                    CatalogNumber = verbatim.CatalogNumber.ToString(),
                    OccurrenceId = verbatim.OccurrenceId?.Trim(),
                    IndividualCount = verbatim.IndividualCount,
                    IsNaturalOccurrence = verbatim.IsNaturalOccurrence,
                    IsNeverFoundObservation = verbatim.IsNeverFoundObservation,
                    IsNotRediscoveredObservation = verbatim.IsNotRediscoveredObservation,
                    IsPositiveObservation = verbatim.IsPositiveObservation,
                    LifeStage = GetLifeStageIdFromString(verbatim.LifeStage),
                    OrganismQuantityInt = verbatim.Quantity,
                    OrganismQuantity = verbatim.Quantity.ToString(),
                    OrganismQuantityUnit = GetOrganismQuantityUnitIdFromString(verbatim.QuantityUnit),
                    ProtectionLevel = CalculateProtectionLevel(taxon, accessRights != null ? (AccessRightsId)accessRights.Id : null),
                    SensitivityCategory = CalculateProtectionLevel(taxon, accessRights != null ? (AccessRightsId)accessRights.Id : null),
                    RecordedBy = verbatim.RecordedBy,
                    ReportedBy = verbatim.ReportedBy,
                    ReportedDate = verbatim.ReportedDate.ToUniversalTime(),
                    OccurrenceRemarks = verbatim.OccurrenceRemarks,
                    OccurrenceStatus = GetOccurrenceStatusIdFromString(verbatim.OccurrenceStatus)
                },
                DynamicProperties = string.IsNullOrEmpty(verbatim.ProjectName)
                    ? null
                    : JsonConvert.SerializeObject(new {verbatim.ProjectName}),
                RightsHolder = verbatim.RightsHolder,
                Taxon = taxon
            };

            AddPositionData(obs.Location, verbatim.DecimalLongitude, verbatim.DecimalLatitude,
                CoordinateSys.WGS84, verbatim.CoordinateUncertaintyInMeters, taxon?.Attributes?.DisturbanceRadius);

            _areaHelper.AddAreaDataToProcessedLocation(obs.Location);

            return obs;
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