using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Interfaces;
using SOS.Import.Jobs.Interfaces;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Species portal harvest
    /// </summary>
    public class GeoHarvestJob : IGeoHarvestJob
    {
        private readonly IGeoFactory _geoFactory;
        private readonly ILogger<GeoHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="geoFactory"></param>
        /// <param name="logger"></param>
        public GeoHarvestJob(IGeoFactory geoFactory, ILogger<GeoHarvestJob> logger)
        {
            _geoFactory = geoFactory ?? throw new ArgumentNullException(nameof(geoFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> Run()
        {
            _logger.LogDebug("Start Geo Harvest Job");
           
            var success = await _geoFactory.HarvestAreasAsync();

            _logger.LogDebug($"End Geo Harvest Job. Success: {success}");

            // return result of all harvests
            return success;
        }
    }
}
