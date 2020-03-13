using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Species portal harvest
    /// </summary>
    public class GeoAreasHarvestJob : IGeoAreasHarvestJob
    {
        private readonly IAreaFactory _areaFactory;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<GeoAreasHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaFactory"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public GeoAreasHarvestJob(IAreaFactory areaFactory,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<GeoAreasHarvestJob> logger)
        {
            _areaFactory = areaFactory ?? throw new ArgumentNullException(nameof(areaFactory));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync()
        {
            _logger.LogDebug("Start Geo Harvest Job");
           
            var result = await _areaFactory.HarvestAreasAsync();

            _logger.LogDebug($"End Geo Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            // return result of all harvests
            return result.Status.Equals(RunStatus.Success) ? true : throw new Exception("Geo Harvest Job failed");
        }
    }
}
