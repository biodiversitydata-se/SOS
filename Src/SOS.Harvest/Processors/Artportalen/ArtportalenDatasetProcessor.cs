using DnsClient.Internal;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System.Data;

namespace SOS.Harvest.Processors.Artportalen
{
    /// <summary>
    ///     Artportalen dataset processor.
    /// </summary>
    public class ArtportalenDatasetProcessor : DatasetProcessorBase<ArtportalenDatasetProcessor, DwcVerbatimDataset, IVerbatimRepositoryBase<DwcVerbatimDataset, int>>,
        IArtportalenDatasetProcessor
    {
        private readonly IArtportalenDatasetMetadataRepository _datasetRepository;
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        public override DataProviderType Type => DataProviderType.ArtportalenObservations;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="verbatimClient"></param>
        /// <param name="processedDatasetsRepository"></param>
        /// <param name="processManager"></param>
        /// <param name="processTimeManager"></param>        
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArtportalenDatasetProcessor(
            IProcessedObservationCoreRepository processedObservationRepository,
            IDatasetRepository processedDatasetsRepository,
            IArtportalenDatasetMetadataRepository datasetRepository,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ILogger<ArtportalenDatasetProcessor> logger) :
                base(processedDatasetsRepository, processManager, processTimeManager, processConfiguration, logger)
        {
            _processedObservationRepository = processedObservationRepository;
            _datasetRepository = datasetRepository;
        }

        public async Task<int> ProcessDatasetsAsync(DataProvider dataProvider, IEnumerable<ArtportalenDatasetMetadata> verbatimDatasets)
        {
            int nrAddedDatasets = await AddDatasetsAsync(dataProvider, verbatimDatasets.ToList());
            return nrAddedDatasets;
        }

        /// <inheritdoc />
        protected override async Task<int> ProcessDatasetsAsync(
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken)
        {
            var verbatimDatasets = await _datasetRepository.GetAllAsync();
            int nrAddedDatasets = await AddDatasetsAsync(dataProvider, verbatimDatasets);
            return nrAddedDatasets;
        }

        private async Task<int> AddDatasetsAsync(DataProvider dataProvider, List<ArtportalenDatasetMetadata> verbatimDatasets)
        {
            try
            {
                Logger.LogInformation("Start AddDatasetsAsync()");
                var datasets = verbatimDatasets.Select(m => m.ToDataset()).ToList();
                foreach (var dataset in datasets)
                {
                    dataset.EventIds = await GetEventIdsAsync(dataset.Identifier);
                }

                int datasetCount = await ValidateAndStoreDatasets(dataProvider, datasets, "");
                Logger.LogInformation("End AddDatasetsAsync()");
                return datasetCount;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Add {@dataProvider} datasets failed.", "Artportalen");
                return 0;
            }
        }

        private async Task<List<string>?> GetEventIdsAsync(string datasetIdentifier)
        {
            var searchFilter = new SearchFilter(0);
            searchFilter.DataStewardshipDatasetIds = new List<string> { datasetIdentifier };
            var eventIds = await _processedObservationRepository.GetAllAggregationItemsAsync(searchFilter, "event.eventId");
            Logger.LogDebug("ArtportalenDatasetProcessor.GetEventIdsAsync() used the index: {@esPublicIndexName}", _processedObservationRepository.PublicIndexName);
            return eventIds?.Select(m => m.AggregationKey).ToList();
        }
    }
}