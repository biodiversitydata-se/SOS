using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Export.Repositories.Interfaces;

namespace SOS.Export.Factories
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
        public async Task<bool> ExportAllAsync()
        {
            try
            {
                var skip = 0;
                var take = 1000000;
                var processedDarwinCore = await _processedDarwinCoreRepository.GetChunkAsync(skip, take);
                // TODO create file
                while (processedDarwinCore?.Any() ?? false)
                {

                    skip += take;
                    processedDarwinCore = await _processedDarwinCoreRepository.GetChunkAsync(skip, take);
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to export all sightings");
                return false;
            }
        }
    }
}
