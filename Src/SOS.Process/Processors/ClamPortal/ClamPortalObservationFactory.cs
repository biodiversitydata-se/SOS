using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Process.Processors.Interfaces;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Process.Processors.ClamPortal
{
    public class ClamPortalObservationFactory : ObservationfactoryBase, IObservationFactory<ClamObservationVerbatim>
    {
        private const string ValidatedObservationStringValue = "Godkänd";
        private readonly DataProvider _dataProvider;
        private readonly IDictionary<int, Lib.Models.Processed.Observation.Taxon> _taxa;
        private readonly IAreaHelper _areaHelper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="areaHelper"></param>
        public ClamPortalObservationFactory(DataProvider dataProvider, IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, IAreaHelper areaHelper)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _taxa = taxa ?? throw new ArgumentNullException(nameof(taxa));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <summary>
        ///     Cast multiple clam observations to processed observations
        /// </summary>
        /// <param name="verbatims"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Observation>> CreateProcessedObservationsAsync(
            IEnumerable<ClamObservationVerbatim> verbatims)
        {
            return await Task.WhenAll(verbatims.Select(CreateProcessedObservationAsync));
        }

        /// <summary>
        ///     Cast clam observation verbatim to processed observation
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        public async Task<Observation> CreateProcessedObservationAsync(ClamObservationVerbatim verbatim)
        {
            _taxa.TryGetValue(verbatim.DyntaxaTaxonId ?? -1, out var taxon);

            var obs = new Observation
            {
                DataProviderId = _dataProvider.Id,
                AccessRights = GetAccessRightsIdFromString(verbatim.AccessRights),
                BasisOfRecord = GetBasisOfRecordIdFromString(verbatim.BasisOfRecord),
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.ClamGateway}",
                DatasetName = "Träd och musselportalen",
                Event = new Event
                {
                    EndDate = verbatim.ObservationDate.ToUniversalTime(),
                    SamplingProtocol = verbatim.SurveyMethod,
                    StartDate = verbatim.ObservationDate.ToUniversalTime(),
                    VerbatimEventDate = DwcFormatter.CreateDateString(verbatim.ObservationDate)
                },
                Identification = new Identification
                {
                    Validated = verbatim.IdentificationVerificationStatus.Equals(
                        ValidatedObservationStringValue, StringComparison.CurrentCultureIgnoreCase),
                    ValidationStatus =
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
                    ProtectionLevel = taxon?.Attributes?.ProtectionLevel?.Id ?? 1,
                    RecordedBy = verbatim.RecordedBy,
                    ReportedBy = verbatim.ReportedBy,
                    ReportedDate = verbatim.ReportedDate.ToUniversalTime(),
                    OccurrenceRemarks = verbatim.OccurrenceRemarks,
                    OccurrenceStatus = GetOccurrenceStatusIdFromString(verbatim.OccurrenceStatus)
                },
                DynamicProperties = string.IsNullOrEmpty(verbatim.ProjectName)
                    ? null
                    : JsonConvert.SerializeObject(new {ProjectName = verbatim.ProjectName}),
                RightsHolder = verbatim.RightsHolder,
                Taxon = taxon
            };

            AddPositionData(obs.Location, verbatim.DecimalLongitude, verbatim.DecimalLatitude,
                CoordinateSys.WGS84, verbatim.CoordinateUncertaintyInMeters, taxon?.Attributes?.DisturbanceRadius);

            _areaHelper.AddAreaDataToProcessedObservation(obs);

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