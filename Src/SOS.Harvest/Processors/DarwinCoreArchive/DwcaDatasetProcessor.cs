using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;

namespace SOS.Harvest.Processors.DarwinCoreArchive
{
    /// <summary>
    ///     DwC-A dataset processor.
    /// </summary>
    public class DwcaDatasetProcessor : DatasetProcessorBase<DwcaDatasetProcessor, DwcVerbatimObservationDataset, IVerbatimRepositoryBase<DwcVerbatimObservationDataset, int>>,
        IDwcaDatasetProcessor
    {
        private readonly IVerbatimClient _verbatimClient;        

        /// <inheritdoc />
        protected override async Task<int> ProcessDatasetsAsync(
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken)
        {
            using var dwcCollectionRepository = new DwcCollectionRepository(
                dataProvider,
                _verbatimClient,
                Logger);

            var datasetFactory = new DwcaDatasetFactory(dataProvider, TimeManager, ProcessConfiguration);

            return await base.ProcessDatasetsAsync(
                dataProvider,
                datasetFactory,
                dwcCollectionRepository.DatasetRepository,
                cancellationToken);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="verbatimClient"></param>
        /// <param name="processedDatasetsRepository"></param>
        /// <param name="processManager"></param>
        /// <param name="processTimeManager"></param>        
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DwcaDatasetProcessor(
            IVerbatimClient verbatimClient,
            IObservationDatasetRepository processedDatasetsRepository,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,            
            ProcessConfiguration processConfiguration,
            ILogger<DwcaDatasetProcessor> logger) :
                base(processedDatasetsRepository, processManager, processTimeManager, processConfiguration, logger)
        {
            _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));            
        }

        public override DataProviderType Type => DataProviderType.DwcA;
    }
}