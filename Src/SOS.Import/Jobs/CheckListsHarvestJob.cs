using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.CheckLists.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Import.Jobs
{
    /// <summary>
    ///     Observation harvest job.
    /// </summary>
    public class CheckListsHarvestJob : ICheckListsHarvestJob
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<CheckListsHarvestJob> _logger;
        private readonly IDictionary<DataProviderType, ICheckListHarvester> _harvestersByType;

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
                _logger.LogInformation($"Start check list harvest jobs");

                //------------------------------------------------------------------------
                // 1. Ensure that any data provider is added
                //------------------------------------------------------------------------
                if (!dataProviders.Any())
                {
                    _logger.LogError(
                        $"No data providers for check list harvest");
                    return false;
                }

                //------------------------------------------------------------------------
                // 2. Harvest observations 
                //------------------------------------------------------------------------
                var harvestTaskByDataProvider = new Dictionary<DataProvider, Task<HarvestInfo>>();
                _logger.LogInformation("Start adding check list harvesters.");
                foreach (var dataProvider in dataProviders.Where(dp => _harvestersByType.ContainsKey(dp.Type)))
                {
                    var harvestJob = _harvestersByType[dataProvider.Type];

                    harvestTaskByDataProvider.Add(dataProvider, dataProvider.Type == DataProviderType.DwcA ?
                        harvestJob.HarvestCheckListsAsync(dataProvider, cancellationToken) :
                        harvestJob.HarvestCheckListsAsync(cancellationToken));

                    _logger.LogDebug($"Added {dataProvider.Names.Translate("en-GB")} check list harvest");
                }
                _logger.LogInformation($"Finish adding check list harvesters.");

                _logger.LogInformation($"Start check list harvesting.");

                await Task.WhenAll(harvestTaskByDataProvider.Values);

                //---------------------------------------------------------------------------------------------------------
                // 3. Update harvest info
                //---------------------------------------------------------------------------------------------------------
                foreach (var task in harvestTaskByDataProvider)
                {
                    var provider = task.Key;
                    var harvestInfo = task.Value.Result;

                    if (harvestInfo.Status != RunStatus.CanceledSuccess)
                    {
                        harvestInfo.Id = provider.CheckListIdentifier;
                        await _harvestInfoRepository.AddOrUpdateAsync(harvestInfo);
                    }
                }

                var success = harvestTaskByDataProvider.All(r => r.Value.Result.Status == RunStatus.Success || r.Value.Result.Status == RunStatus.CanceledSuccess);

                _logger.LogInformation($"Finish check list harvest jobs. Success: { success }");

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
        /// <param name="artportalenCheckListHarvester"></param>
        /// <param name="dwcCheckListHarvester"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public CheckListsHarvestJob(
            IArtportalenCheckListHarvester artportalenCheckListHarvester,
            IDwcCheckListHarvester dwcCheckListHarvester,
            IDataProviderManager dataProviderManager,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<CheckListsHarvestJob> logger)
        {
            if (artportalenCheckListHarvester == null) throw new ArgumentNullException(nameof(artportalenCheckListHarvester));

            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));


            _harvestersByType = new Dictionary<DataProviderType, ICheckListHarvester>
            {
                {DataProviderType.ArtportalenObservations, artportalenCheckListHarvester},
                {DataProviderType.DwcA, dwcCheckListHarvester}
            };
        }

        /// <inheritdoc />
        [DisplayName("Harvest and process check lists")]
        public async Task<bool> RunAsync(
            IJobCancellationToken cancellationToken)
        {
            var dataProviders = (await _dataProviderManager.GetAllDataProvidersAsync())?.Where(dp => dp.IsActive && dp.SupportCheckLists).ToArray();

            if (!dataProviders?.Any() ?? true)
            {
                _logger.LogInformation($"No data providers found to harvest");
                return false;
            }

            _logger.LogInformation($"Start check list harvest job");

            var success = await Harvest(dataProviders, cancellationToken);

            if (!success)
            {
                _logger.LogInformation($"Check list harvest job failed");
                return false;
            }
            // If harvest was successful, go on with enqueuing processing job to Hangfire
            var jobId = BackgroundJob.Enqueue<IProcessCheckListsJob>(job => job.RunAsync(
                dataProviders.Select(dataProvider => dataProvider.Identifier).ToArray(),
                cancellationToken));

            _logger.LogInformation($"Process Check Lists Job with Id={ jobId } was enqued");

            _logger.LogInformation("Finish check list harvest job");

            return true;
        }
    }
}