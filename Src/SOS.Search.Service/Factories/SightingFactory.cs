using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Search;
using SOS.Search.Service.Enum;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class SightingFactory : Interfaces.ISightingFactory
    {
        private readonly IProcessedSightingRepository _processedSightingRepository;

        private readonly ILogger<SightingFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedSightingRepository"></param>
        /// <param name="logger"></param>
        public SightingFactory(
            IProcessedSightingRepository processedSightingRepository,
            ILogger<SightingFactory> logger)
        {
            _processedSightingRepository = processedSightingRepository ??
                                           throw new ArgumentNullException(nameof(processedSightingRepository));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(AdvancedFilter filter, int skip, int take, string sortBy, SearchSortOrder sortOrder)
        {
            try
            {
                var processedSightings = await _processedSightingRepository.GetChunkAsync(filter, skip, take, sortBy, sortOrder);
                
                return processedSightings;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get chunk of sightings");
                return null;
            }
        }
    }
}
