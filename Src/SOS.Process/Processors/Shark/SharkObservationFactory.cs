using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shark;
using SOS.Process.Constants;
using SOS.Process.Processors.Interfaces;

namespace SOS.Process.Processors.Shark
{
    public class SharkObservationFactory : ObservationfactoryBase, IObservationFactory<SharkObservationVerbatim>
    {
        private const int DefaultCoordinateUncertaintyInMeters = 500;
        private readonly DataProvider _dataProvider;
        private readonly IDictionary<int, Lib.Models.Processed.Observation.Taxon> _taxa;
        private readonly IAreaHelper _areaHelper;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="areaHelper"></param>
        /// <param name="geometryManager"></param>
        public SharkObservationFactory(DataProvider dataProvider, IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, IAreaHelper areaHelper)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _taxa = taxa ?? throw new ArgumentNullException(nameof(taxa));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <summary>
        ///     Cast multiple clam observations to ProcessedObservation
        /// </summary>
        /// <param name="verbatims"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Observation>> CreateProcessedObservationsAsync(
            IEnumerable<SharkObservationVerbatim> verbatims)
        {
            return await Task.WhenAll(verbatims.Select(CreateProcessedObservationAsync));
        }

        /// <summary>
        ///     Cast Shark observation verbatim to ProcessedObservation
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        public async Task<Observation> CreateProcessedObservationAsync(SharkObservationVerbatim verbatim)
        {
            _taxa.TryGetValue(verbatim.DyntaxaId.HasValue ? verbatim.DyntaxaId.Value : -1, out var taxon);
            var sharkSampleId = verbatim.Sharksampleidmd5 ?? verbatim.SharkSampleId;
            var obs = new Observation
            {
                AccessRights = new VocabularyValue { Id = (int)AccessRightsId.FreeUsage },
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
                    UncertainIdentification = false,
                    Validated = false,
                    ValidationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert }
                },
                Location = new Location
                {
                    MaximumDepthInMeters = verbatim.WaterDepthM,
                    MinimumDepthInMeters = verbatim.WaterDepthM
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
            AddPositionData(obs.Location, verbatim.SampleLongitudeDd, verbatim.SampleLatitudeDd, 
                CoordinateSys.WGS84, ProcessConstants.DefaultAccuracyInMeters, taxon?.Attributes?.DisturbanceRadius);
            _areaHelper.AddAreaDataToProcessedObservation(obs);

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