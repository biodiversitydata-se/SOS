using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Search;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class SightingFactory : Interfaces.ISightingFactory
    {
        private readonly IProcessedDarwinCoreRepository _processedDarwinCoreRepository;

        private readonly ILogger<SightingFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedDarwinCoreRepository"></param>
        /// <param name="logger"></param>
        public SightingFactory(
            IProcessedDarwinCoreRepository processedDarwinCoreRepository,
            ILogger<SightingFactory> logger)
        {
            _processedDarwinCoreRepository = processedDarwinCoreRepository ??
                                           throw new ArgumentNullException(nameof(processedDarwinCoreRepository));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<dynamic>> GetChunkAsync(AdvancedFilter filter, int skip, int take)
        {
            try
            {
                var processedDarwinCore = await _processedDarwinCoreRepository.GetChunkAsync(filter, skip, take);
                
                return processedDarwinCore;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get chunk of sightings");
                return null;
            }
        }
    }
}
