using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Search.Service.Extensions;
using SOS.Search.Service.Models;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class SightingFactory : Interfaces.ISightingFactory
    {
        private readonly IProcessedDarwinCoreRepository _ProcessedDarwinCoreRepository;

        private readonly ILogger<SightingFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ProcessedDarwinCoreRepository"></param>
        /// <param name="logger"></param>
        public SightingFactory(
            IProcessedDarwinCoreRepository ProcessedDarwinCoreRepository,
            ILogger<SightingFactory> logger)
        {
            _ProcessedDarwinCoreRepository = ProcessedDarwinCoreRepository ??
                                           throw new ArgumentNullException(nameof(ProcessedDarwinCoreRepository));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DarwinCore<string>>> GetChunkAsync(int taxonId, int skip, int take)
        {
            try
            {
                var processedDarwinCore = await _ProcessedDarwinCoreRepository.GetChunkAsync(taxonId, skip, take);
                return processedDarwinCore?.ToDarwinCore();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get chunk of sightings");
                return null;
            }
        }
    }
}
