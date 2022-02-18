using System;
using System.Collections.Generic;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Mvm;
using SOS.Process.Processors.Interfaces;

namespace SOS.Process.Processors.Mvm
{
    public class MvmObservationFactory : ObservationFactoryBase, IObservationFactory<MvmObservationVerbatim>
    {
        private const int DefaultCoordinateUncertaintyInMeters = 500;
        private readonly DataProvider _dataProvider;
        private readonly IAreaHelper _areaHelper;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="areaHelper"></param>
        public MvmObservationFactory(DataProvider dataProvider, IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, IAreaHelper areaHelper) : base(taxa)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <summary>
        ///  Cast verbatim observations to processed data model
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="diffuseIfSupported"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(MvmObservationVerbatim verbatim, bool diffuseIfSupported)
        {
            var taxon = GetTaxon(verbatim.DyntaxaTaxonId);
            var accessRights = new VocabularyValue { Id = (int)AccessRightsId.FreeUsage };

            var obs = new Observation
            {
                AccessRights = accessRights,
                DataProviderId = _dataProvider.Id,
                BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation},
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.MVM}",
                DatasetName = "MVM",
                DynamicProperties = string.IsNullOrEmpty(verbatim.ProductName) ? null : @"{""productName"": " + verbatim.ProductName + "}",
                DiffusionStatus = DiffusionStatus.NotDiffused,
                Event = new Event
                {
                    EndDate = verbatim.End.ToUniversalTime(),
                    StartDate = verbatim.Start.ToUniversalTime(),
                    PlainStartDate = verbatim.Start.ToLocalTime().ToString("yyyy-MM-dd"),
                    PlainEndDate = verbatim.End.ToLocalTime().ToString("yyyy-MM-dd"),
                    PlainStartTime = null,
                    PlainEndTime = null,
                    VerbatimEventDate = DwcFormatter.CreateDateIntervalString(verbatim.Start.ToLocalTime(), verbatim.End.ToLocalTime()),
                    Habitat = verbatim.Habitat
                },
                Identification = new Identification
                {
                    UncertainIdentification = false,
                    Validated = true,
                    Verified = true,
                    ValidationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert },
                    VerificationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert }
                },
                Location = new Location
                {
                    Locality = verbatim.Locality,
                    LocationRemarks = verbatim.LocationRemarks
                },
                Modified = verbatim.Modified?.ToUniversalTime(),
                Occurrence = new Occurrence
                {
                    BirdNestActivityId = taxon?.IsBird() ?? false ? 1000000 : 0,
                    CatalogNumber = string.IsNullOrEmpty(verbatim.CatalogNumber) ? GetCatalogNumber(verbatim.OccurrenceId) : verbatim.CatalogNumber,
                    IndividualCount = verbatim.IndividualCount,
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = GetIsNeverFoundObservation(verbatim.DyntaxaTaxonId),
                    IsNotRediscoveredObservation = false,
                    IsPositiveObservation = verbatim.IsPositiveObservation,
                    OccurrenceId = verbatim.OccurrenceId,
                    ProtectionLevel = CalculateProtectionLevel(taxon, (AccessRightsId)accessRights.Id),
                    SensitivityCategory = CalculateProtectionLevel(taxon, (AccessRightsId)accessRights.Id),
                    OrganismQuantity = verbatim.Quantity,
                    OrganismQuantityUnit = string.IsNullOrEmpty(verbatim.QuantityUnit) ? null : new VocabularyValue { Id = -1, Value = verbatim.QuantityUnit },
                    RecordedBy = verbatim.RecordedBy,
                    ReportedBy = verbatim.ReportedBy,
                    ReportedDate = verbatim.ReportedDate.ToUniversalTime(),
                    OccurrenceStatus = GetOccurrenceStatusId(verbatim.DyntaxaTaxonId)
                },
                OwnerInstitutionCode = verbatim.Owner,
                Projects = verbatim.ProjectID == 0? null : new []{ new Project{ Id = verbatim.ProjectID, Name = verbatim.ProjectName} },
                Taxon = taxon
            };
            AddPositionData(obs.Location, verbatim.DecimalLongitude, verbatim.DecimalLatitude,
                CoordinateSys.WGS84, verbatim.CoordinateUncertaintyInMeters, taxon?.Attributes?.DisturbanceRadius);
            _areaHelper.AddAreaDataToProcessedLocation(obs.Location);

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