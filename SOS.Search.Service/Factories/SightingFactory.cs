using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.DarwinCore;
using SOS.Search.Service.Extensions;
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
        /// <param name="ProcessedDarwinCoreRepository"></param>
        /// <param name="logger"></param>
        public SightingFactory(
            IProcessedDarwinCoreRepository ProcessedDarwinCoreRepository,
            ILogger<SightingFactory> logger)
        {
            _processedDarwinCoreRepository = ProcessedDarwinCoreRepository ??
                                           throw new ArgumentNullException(nameof(ProcessedDarwinCoreRepository));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DarwinCore<string>>> GetChunkAsync(int taxonId, int skip, int take)
        {
            try
            {
                var processedDarwinCore = await _processedDarwinCoreRepository.GetChunkAsync(taxonId, skip, take);
                return processedDarwinCore?.ToDarwinCore();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get chunk of sightings");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<dynamic>> GetChunkAsync(int taxonId, IEnumerable<string> fields, int skip, int take)
        {
            try
            {
                var result = await _processedDarwinCoreRepository.GetChunkAsync(taxonId, fields, skip, take);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get chunk of sightings");
                return null;
            }
        }
    }
}
