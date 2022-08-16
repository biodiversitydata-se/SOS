using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Location;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Observation manager class
    /// </summary>
    public class LocationManager : ILocationManager
    {
        private readonly IProcessedLocationRepository _processedLocationRepository;
        private readonly IFilterManager _filterManager;
        private readonly ILogger<LocationManager> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedLocationRepository"></param>
        /// <param name="filterManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public LocationManager(
            IProcessedLocationRepository processedLocationRepository,
            IFilterManager filterManager,
            ILogger<LocationManager> logger)
        {
            _processedLocationRepository = processedLocationRepository ??
                                              throw new ArgumentNullException(nameof(processedLocationRepository));
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Make sure we are working with live data
            _processedLocationRepository.LiveMode = true;
        }


        /// <inheritdoc />
        public async Task<IEnumerable<LocationDto>> GetLocationsAsync(IEnumerable<string> locationIds)
        {
            var locations = await _processedLocationRepository.GetLocationsAsync(locationIds);

            return locations?.Select(l => l.ToDto());
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LocationSearchResultDto>> SearchAsync(int? userId, int? roleId, string authorizationApplicationIdentifier,
            SearchFilter filter, int skip, int take)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(userId, roleId, authorizationApplicationIdentifier, filter);
                var result = await _processedLocationRepository.SearchAsync(filter, skip, take);

                return result?.Select(l => l.ToDto());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to search for locations.");
                throw;
            }
        }
    }
}