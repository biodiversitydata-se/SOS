using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Artportalen harvest
    /// </summary>
    public class GeoAreasHarvestJob : IGeoAreasHarvestJob
    {
        private readonly IAreaHarvester _areaHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<GeoAreasHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public GeoAreasHarvestJob(IAreaHarvester areaHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<GeoAreasHarvestJob> logger)
        {
            _areaHarvester = areaHarvester ?? throw new ArgumentNullException(nameof(areaHarvester));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync()
        {
            _logger.LogInformation("Start Geo Harvest Job");
           
            var result = await _areaHarvester.HarvestAreasAsync();

            _logger.LogInformation($"End Geo Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            // return result of all harvests
            return result.Status.Equals(RunStatus.Success) ? true : throw new Exception("Geo Harvest Job failed");
        }
    }
}
