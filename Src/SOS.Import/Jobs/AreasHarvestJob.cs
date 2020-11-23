using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Import.Jobs
{
    /// <summary>
    ///     Areas harvest.
    /// </summary>
    public class AreasHarvestJob : IAreasHarvestJob
    {
        private readonly IAreaHarvester _areaHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<AreasHarvestJob> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="areaHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public AreasHarvestJob(IAreaHarvester areaHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<AreasHarvestJob> logger)
        {
            _areaHarvester = areaHarvester ?? throw new ArgumentNullException(nameof(areaHarvester));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Harvest areas from Artportalen db")]
        public async Task<bool> RunAsync()
        {
            _logger.LogInformation("Start Geo Harvest Job");

            var result = await _areaHarvester.HarvestAreasAsync();

            _logger.LogInformation($"End Geo Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            // return result of all harvests
            return result.Status.Equals(RunStatus.Success) && result.Count > 0
                ? true
                : throw new Exception("Geo Harvest Job failed");
        }
    }
}