using System.ComponentModel;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.Artportalen.Interfaces;
using SOS.Harvest.Harvesters.DwC.Interfaces;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Jobs
{
    /// <summary>
    ///     Observation harvest job.
    /// </summary>
    public class ChecklistsHarvestJob : IChecklistsHarvestJob
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly IProcessChecklistsJob _processChecklistsJob;
        private readonly ILogger<ChecklistsHarvestJob> _logger;
        private readonly IDictionary<DataProviderType, IChecklistHarvester> _harvestersByType;

        /// <summary>
        /// Run job
        /// </summary>
        /// <param name="dataProviders"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> Harvest(
            IEnumerable<DataProvider> dataProviders,
        IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Start checklist harvest jobs");

                //------------------------------------------------------------------------
                // 1. Ensure that any data provider is added
                //------------------------------------------------------------------------
                if (!dataProviders.Any())
                {
                    _logger.LogError(
                        $"No data providers for checklist harvest");
                    return false;
                }

                //------------------------------------------------------------------------
                // 2. Harvest observations 
                //------------------------------------------------------------------------
                var harvestTaskByDataProvider = new Dictionary<DataProvider, Task<HarvestInfo>>();
                _logger.LogInformation("Start adding checklist harvesters.");
                foreach (var dataProvider in dataProviders.Where(dp => dp.Type != DataProviderType.DwcA && _harvestersByType.ContainsKey(dp.Type)))
                {
                    var harvestJob = _harvestersByType[dataProvider.Type];

                    harvestTaskByDataProvider.Add(dataProvider, dataProvider.Type == DataProviderType.DwcA ?
                        harvestJob.HarvestChecklistsAsync(dataProvider, cancellationToken) :
                        harvestJob.HarvestChecklistsAsync(cancellationToken));

                    _logger.LogDebug($"Added {dataProvider.Names.Translate("en-GB")} checklist harvest");
                }
                _logger.LogInformation($"Finish adding checklist harvesters.");

                _logger.LogInformation($"Start checklist harvesting.");

                await Task.WhenAll(harvestTaskByDataProvider.Values);

                //---------------------------------------------------------------------------------------------------------
                // 3. Update harvest info
                //---------------------------------------------------------------------------------------------------------
                foreach (var task in harvestTaskByDataProvider)
                {
                    var provider = task.Key;
                    var harvestInfo = task.Value.Result;

                    if (harvestInfo.Status != RunStatus.CanceledSuccess && harvestInfo.Count != -1) // Count -1 = Assume manually harvested, don't update harvest info 
                    {
                        harvestInfo.Id = provider.ChecklistIdentifier;
                        await _harvestInfoRepository.AddOrUpdateAsync(harvestInfo);
                    }
                }

                var success = harvestTaskByDataProvider.All(r => r.Value.Result.Status == RunStatus.Success || r.Value.Result.Status == RunStatus.CanceledSuccess);

                _logger.LogInformation($"Finish checklist harvest jobs. Success: { success }");

                return success;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Check list harvest job was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Check list harvest job failed.");
                throw new Exception("Check list harvest job failed.");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenChecklistHarvester"></param>
        /// <param name="dwcChecklistHarvester"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="processChecklistsJob"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ChecklistsHarvestJob(
            IArtportalenChecklistHarvester artportalenChecklistHarvester,
            IDwcChecklistHarvester dwcChecklistHarvester,
            IDataProviderManager dataProviderManager,
            IHarvestInfoRepository harvestInfoRepository,
            IProcessChecklistsJob processChecklistsJob,
            ILogger<ChecklistsHarvestJob> logger)
        {
            if (artportalenChecklistHarvester == null) throw new ArgumentNullException(nameof(artportalenChecklistHarvester));

            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));

            _processChecklistsJob = processChecklistsJob ?? throw new ArgumentNullException(nameof(processChecklistsJob));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _harvestersByType = new Dictionary<DataProviderType, IChecklistHarvester>
            {
                {DataProviderType.ArtportalenObservations, artportalenChecklistHarvester},
                {DataProviderType.DwcA, dwcChecklistHarvester}
            };
        }

        /// <inheritdoc />
        [DisplayName("Harvest and process checklists")]
        public async Task<bool> RunAsync(
            IJobCancellationToken cancellationToken)
        {
            var dataProviders = (await _dataProviderManager.GetAllDataProvidersAsync())?.Where(dp => dp.IsActive && dp.SupportChecklists).ToArray();

            if (!dataProviders?.Any() ?? true)
            {
                _logger.LogInformation($"No data providers found to harvest");
                return false;
            }

            _logger.LogInformation($"Start checklist harvest job");

            var success = await Harvest(dataProviders!, cancellationToken);

            if (!success)
            {
                _logger.LogInformation($"Check list harvest job failed");
                return false;
            }

            _logger.LogInformation("Finish checklist harvest job");
            
            return await _processChecklistsJob.RunAsync(
                dataProviders!.Select(dataProvider => dataProvider.Identifier).ToArray(),
                cancellationToken);
        }
    }
}