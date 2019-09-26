using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Search.Service.Models;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class SightingFactory : Interfaces.ISightingFactory
    {
        private ISightingAggregateRepository _sightingAggregateRepository;

        private ILogger<SightingFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sightingAggregateRepository"></param>
        /// <param name="logger"></param>
        public SightingFactory(
            ISightingAggregateRepository sightingAggregateRepository,
            ILogger<SightingFactory> logger)
        {

            _sightingAggregateRepository = sightingAggregateRepository ??
                                           throw new ArgumentNullException(nameof(sightingAggregateRepository));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SightingAggregate>> GetChunkAsync(int taxonId, int skip, int take)
        {
            try
            {
                // Make sure we have an empty collection
                return await _sightingAggregateRepository.GetChunkAsync(taxonId, skip, take);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get chunk of sightings");
                return null;
            }
        }
    }
}
