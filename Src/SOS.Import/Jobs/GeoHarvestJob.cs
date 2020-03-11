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
    public class GeoHarvestJob : IGeoHarvestJob
    {
        private readonly IGeoFactory _geoFactory;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<GeoHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="geoFactory"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public GeoHarvestJob(IGeoFactory geoFactory,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<GeoHarvestJob> logger)
        {
            _geoFactory = geoFactory ?? throw new ArgumentNullException(nameof(geoFactory));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync()
        {
            _logger.LogDebug("Start Geo Harvest Job");
           
            var result = await _geoFactory.HarvestAreasAsync();

            _logger.LogDebug($"End Geo Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            // return result of all harvests
            return result.Status.Equals(RunStatus.Success) ? true : throw new Exception("Geo Harvest Job failed");
        }
    }
}
