using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Extensions;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Import.Repositories.Destination.SpeciesPortal.Interfaces;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class GeoFactory : Interfaces.IGeoFactory
    {
        private readonly IAreaRepository _areaRepository;
        private readonly IAreaVerbatimRepository _areaVerbatimRepository;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<GeoFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaRepository"></param>
        /// <param name="areaVerbatimRepository"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public GeoFactory(
            IAreaRepository areaRepository,
            IAreaVerbatimRepository areaVerbatimRepository,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<GeoFactory> logger)
        {
            _areaRepository = areaRepository ?? throw new ArgumentNullException(nameof(areaRepository));
            _areaVerbatimRepository = areaVerbatimRepository ?? throw new ArgumentNullException(nameof(areaVerbatimRepository));
            _areaVerbatimRepository = areaVerbatimRepository ?? throw new ArgumentNullException(nameof(areaVerbatimRepository));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> HarvestAreasAsync()
        {
            try
            {
                var start = DateTime.Now;
                _logger.LogDebug("Start getting areas");

                var areas = (await _areaRepository.GetAsync()).ToAggregates();

                _logger.LogDebug("Empty area collection");
                // Make sure we have an empty collection
                if (await _areaVerbatimRepository.DeleteCollectionAsync())
                {
                    if (await _areaVerbatimRepository.AddCollectionAsync())
                    {
                        _logger.LogDebug("Adding areas to db");
                        if (await _areaVerbatimRepository.AddManyAsync(areas))
                        {
                            await _areaVerbatimRepository.CreateIndexAsync();
                            _logger.LogDebug("Succeeded adding areas");

                            // Update harvest info
                            return await _harvestInfoRepository.UpdateHarvestInfoAsync(
                                nameof(Area),
                                DataProvider.Artdatabanken,
                                start,
                                DateTime.Now,
                                areas.Count());
                        }
                    }
                }

                _logger.LogDebug("Failed adding areas");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed aggregation of areas");
                return false;
            }
        }
    }
}
