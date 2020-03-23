using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.Shared;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Jobs
{
    public class CopyAreasJob : ProcessJobBase, ICopyAreasJob
    {
        private readonly IAreaVerbatimRepository _areaVerbatimRepository;
        private readonly IProcessedAreaRepository _processedAreaRepository;
        private readonly ILogger<CopyAreasJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaVerbatimRepository"></param>
        /// <param name="processedAreaRepository"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="logger"></param>
        public CopyAreasJob(
            IAreaVerbatimRepository areaVerbatimRepository,
            IProcessedAreaRepository processedAreaRepository,
            IHarvestInfoRepository harvestInfoRepository,
            IProcessInfoRepository processInfoRepository,
            ILogger<CopyAreasJob> logger) : base(harvestInfoRepository, processInfoRepository)
        {
            _areaVerbatimRepository = areaVerbatimRepository ?? throw new ArgumentNullException(nameof(areaVerbatimRepository));
            _processedAreaRepository = processedAreaRepository ?? throw new ArgumentNullException(nameof(processedAreaRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync()
        {
            var start = DateTime.Now;

            var areas = await _areaVerbatimRepository.GetAllAsync();
            if (!areas?.Any() ?? true)
            {
                _logger.LogDebug("Failed to get areas");
                return false;
            }

            _logger.LogDebug("Start deleting areas");
            if (!await _processedAreaRepository.DeleteCollectionAsync())
            {
                _logger.LogError("Failed to delete areas");
                return false;
            }
            _logger.LogDebug("Finish deleting areas");

            _logger.LogDebug("Start copy areas");
            var success = await _processedAreaRepository.AddManyAsync(areas);
            //var success = await CopyAreas();
            _logger.LogDebug("Finish copy areas");

            _logger.LogDebug("Start updating process info for areas");
            var harvestInfo = await GetHarvestInfoAsync(nameof(Area));
            var providerInfo = CreateProviderInfo(DataSet.Areas, harvestInfo,  start, DateTime.Now, success ? RunStatus.Success : RunStatus.Failed, areas.Count);
            await SaveProcessInfo(nameof(Area), start, areas.Count,
                success ? RunStatus.Success : RunStatus.Failed, new [] { providerInfo } );
            _logger.LogDebug("Finish updating process info for areas");

            return success ? true : throw new Exception("Copy field areas job failed");
        }
    }
}