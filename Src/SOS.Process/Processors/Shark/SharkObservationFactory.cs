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
using SOS.Lib.Models.Verbatim.Shark;
using SOS.Process.Constants;
using SOS.Process.Processors.Interfaces;

namespace SOS.Process.Processors.Shark
{
    public class SharkObservationFactory : ObservationFactoryBase, IObservationFactory<SharkObservationVerbatim>
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
        public SharkObservationFactory(DataProvider dataProvider, IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, IAreaHelper areaHelper) : base(taxa)
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
        public Observation CreateProcessedObservation(SharkObservationVerbatim verbatim, bool diffuseIfSupported)
        {
            var taxon = GetTaxon(verbatim.DyntaxaId.HasValue ? verbatim.DyntaxaId.Value : -1);
            var accessRights = new VocabularyValue { Id = (int)AccessRightsId.FreeUsage };
            var sharkSampleId = $"{verbatim.Sharksampleidmd5 ?? verbatim.SharkSampleId}-{verbatim.DyntaxaId}-{verbatim.Parameter}-{verbatim.Value}".RemoveWhiteSpace();
            
            var obs = new Observation
            {
                AccessRights = accessRights,
                DataProviderId = _dataProvider.Id,
                BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation},
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.SHARK}",
                DatasetName = verbatim.DatasetName,
                DiffusionStatus = DiffusionStatus.NotDiffused,
                Event = new Event
                {
                    EndDate = verbatim.SampleDate?.ToUniversalTime(),
                    StartDate = verbatim.SampleDate?.ToUniversalTime(),
                    PlainStartDate = verbatim.SampleDate?.ToLocalTime().ToString("yyyy-MM-dd"),
                    PlainEndDate = verbatim.SampleDate?.ToLocalTime().ToString("yyyy-MM-dd"),
                    PlainStartTime = null,
                    PlainEndTime = null,
                    VerbatimEventDate = DwcFormatter.CreateDateString(verbatim.SampleDate?.ToLocalTime())
                },
                Identification = new Identification
                {
                    IdentifiedBy = verbatim.AnalysedBy,
                    UncertainIdentification = false,
                    Validated = true,
                    Verified = true,
                    ValidationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert },
                    VerificationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert }
                },
                Location = new Location
                {
                    MaximumDepthInMeters = verbatim.WaterDepthM,
                    MinimumDepthInMeters = verbatim.WaterDepthM
                },
                Occurrence = new Occurrence
                {
                    BirdNestActivityId = 0,
                    CatalogNumber = sharkSampleId,
                    OccurrenceId = $"urn:lsid:shark:observation:{sharkSampleId}",
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = GetIsNeverFoundObservation(verbatim.DyntaxaId),
                    IsNotRediscoveredObservation = false,
                    IsPositiveObservation = GetIsPositiveObservation(verbatim.DyntaxaId),
                    ProtectionLevel = CalculateProtectionLevel(taxon, (AccessRightsId)accessRights.Id),
                    SensitivityCategory = CalculateProtectionLevel(taxon, (AccessRightsId)accessRights.Id),
                    RecordedBy = verbatim.Taxonomist,
                    ReportedBy = verbatim.ReportedStationName,
                    OccurrenceStatus = GetOccurrenceStatusId(verbatim.DyntaxaId)
                },
                OwnerInstitutionCode = verbatim.ReportingInstituteNameSv,
                Taxon = taxon
            };
            AddPositionData(obs.Location, verbatim.SampleLongitudeDd, verbatim.SampleLatitudeDd, 
                CoordinateSys.WGS84, ProcessConstants.DefaultAccuracyInMeters, taxon?.Attributes?.DisturbanceRadius);
            _areaHelper.AddAreaDataToProcessedLocation(obs.Location);

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