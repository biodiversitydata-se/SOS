using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.Shared;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Jobs
{
    public class ProcessAreasJob : ProcessJobBase, IProcessAreasJob
    {
        private readonly IAreaVerbatimRepository _areaVerbatimRepository;
        private readonly IProcessedAreaRepository _processedAreaRepository;
        private readonly ILogger<ProcessAreasJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaVerbatimRepository"></param>
        /// <param name="processedAreaRepository"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="logger"></param>
        public ProcessAreasJob(
            IAreaVerbatimRepository areaVerbatimRepository,
            IProcessedAreaRepository processedAreaRepository,
            IHarvestInfoRepository harvestInfoRepository,
            IProcessInfoRepository processInfoRepository,
            ILogger<ProcessAreasJob> logger) : base(harvestInfoRepository, processInfoRepository)
        {
            _areaVerbatimRepository = areaVerbatimRepository ?? throw new ArgumentNullException(nameof(areaVerbatimRepository));
            _processedAreaRepository = processedAreaRepository ?? throw new ArgumentNullException(nameof(processedAreaRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<bool> ProcessGeometryAsync(int id)
        {
            var webMercatorGeometry = await _areaVerbatimRepository.GetGeometryAsync(id);
            var wgs84Geometry = webMercatorGeometry.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84);

            return await _processedAreaRepository.StoreGeometryAsync(id, wgs84Geometry.ToGeoShape());
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
           
            _logger.LogDebug("Finish copy areas");

            if (success)
            {
                _logger.LogDebug("Start deleting geometries");
                await _processedAreaRepository.DropGeometriesAsync();
                _logger.LogDebug("Finish deleting geometries");

                _logger.LogDebug("Start processing geometries");
                foreach (var area in areas)
                {
                    success = success && await ProcessGeometryAsync(area.Id);
                }
                _logger.LogDebug("Finish processing geometries");

                _logger.LogDebug("Start indexing areas");
                await _processedAreaRepository.CreateIndexAsync();
                _logger.LogDebug("Finish indexing areas");
            }

            _logger.LogDebug("Start updating process info for areas");
            var harvestInfo = await GetHarvestInfoAsync(nameof(Area));
            var providerInfo = CreateProviderInfo(DataProviderType.Areas, harvestInfo,  start, DateTime.Now, success ? RunStatus.Success : RunStatus.Failed, areas.Count);
            await SaveProcessInfo(nameof(Area), start, areas.Count,
                success ? RunStatus.Success : RunStatus.Failed, new [] { providerInfo } );
            _logger.LogDebug("Finish updating process info for areas");

            return success ? true : throw new Exception("Copy field areas job failed");
        }
    }
}