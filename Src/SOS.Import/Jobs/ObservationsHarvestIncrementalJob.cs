using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Managers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Jobs
{
    /// <summary>
    ///     Observation harvest job.
    /// </summary>
    public class ObservationsHarvestIncrementalJob : IObservationsHarvestIncrementalJob
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly Dictionary<DataProviderType, IHarvestJob> _harvestJobByType;
        private readonly ILogger<ObservationsHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenHarvestJob"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public ObservationsHarvestIncrementalJob(
            IArtportalenHarvestJob artportalenHarvestJob,
            IDataProviderManager dataProviderManager,
            ILogger<ObservationsHarvestJob> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (artportalenHarvestJob == null) throw new ArgumentNullException(nameof(artportalenHarvestJob));
            
            _harvestJobByType = new Dictionary<DataProviderType, IHarvestJob>
            {
                {DataProviderType.ArtportalenObservations, artportalenHarvestJob},
            };
        }

        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            var incrementalDataProviders = (await _dataProviderManager.GetAllDataProviders()).Where(p => 
                p.SupportIncrementalHarvest && 
                p.IsActive &&
                p.IncludeInScheduledHarvest).ToList();
            return await RunAsync(
                incrementalDataProviders,
                cancellationToken);
        }

        private async Task<bool> RunAsync(
            IEnumerable<DataProvider> dataProviders,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Start Harvest Jobs");

                //------------------------------------------------------------------------
                // 1. Ensure that any data provider is added
                //------------------------------------------------------------------------
                if (!dataProviders.Any())
                {
                    _logger.LogError(
                        "No data providers to incremental harvest");
                    return false;
                }

                //------------------------------------------------------------------------
                // 2. Harvest observations directly without enqueuing to Hangfire
                //------------------------------------------------------------------------
                _logger.LogInformation("Start observasions harvest jobs");
                var harvestTaskByDataProvider = new Dictionary<DataProvider, Task<bool>>();
                foreach (var dataProvider in dataProviders)
                {
                    var harvestJob = _harvestJobByType[dataProvider.Type];
                    harvestTaskByDataProvider.Add(dataProvider, harvestJob.RunAsync(true, cancellationToken));
                    _logger.LogDebug($"Added {dataProvider.Name} harvest");
                }

                await Task.WhenAll(harvestTaskByDataProvider.Values);
                _logger.LogInformation("Finish observasions incremental harvest jobs");

                //---------------------------------------------------------------------------------------------------------
                // 3. If harvest was successful, go on with enqueuing processing job to Hangfire
                //---------------------------------------------------------------------------------------------------------
                if (harvestTaskByDataProvider.All(p => p.Value.Result))
                {
                    // Enqueue process job to Hangfire
                    var jobId = BackgroundJob.Enqueue<IProcessJob>(job => job.RunAsync(
                        dataProviders.Select(dataProvider => dataProvider.Identifier).ToList(),
                        false,
                        true,
                        false,
                        false,
                        cancellationToken));

                    _logger.LogInformation($"Process Job with Id={jobId} was enqueued");
                    return true;
                }

                throw new Exception("Failed to harvest incremental data");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Observation harvest incremental job was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Observation harvest incremental job was cancelled.");
                throw new Exception("Failed to harvest incremental data");
            }
        }
    }
}