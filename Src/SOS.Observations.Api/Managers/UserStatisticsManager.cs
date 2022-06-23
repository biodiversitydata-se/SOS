using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     User statistics manager.
    /// </summary>
    public class UserStatisticsManager : IUserStatisticsManager
    {
        private readonly IProcessedLocationRepository _processedLocationRepository;
        private readonly IFilterManager _filterManager;
        private readonly ILogger<UserStatisticsManager> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedLocationRepository"></param>
        /// <param name="filterManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public UserStatisticsManager(
            IProcessedLocationRepository processedLocationRepository,
            IFilterManager filterManager,
            ILogger<UserStatisticsManager> logger)
        {
            _processedLocationRepository = processedLocationRepository ??
                                              throw new ArgumentNullException(nameof(processedLocationRepository));
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Make sure we are working with live data
            _processedLocationRepository.LiveMode = true;
        }

        public async Task<IEnumerable<UserStatisticsItem>> SpeciesCountSearchAsync()
        {
            // todo - implement logic
            //await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            //var result = await _processedLocationRepository.SearchUserSpeciesCountAsync(filter, skip, take);
            var result = new List<UserStatisticsItem>
            {
                new() {UserId = 1, SpeciesCount = 5},
                new() {UserId = 2, SpeciesCount = 4},
                new() {UserId = 3, SpeciesCount = 4},
                new() {UserId = 4, SpeciesCount = 2},
                new() {UserId = 5, SpeciesCount = 1}
            };

            return result;
        }

        public async Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(int? skip, int? take)
        {
            // todo - implement logic
            //await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            //var result = await _processedLocationRepository.SearchUserSpeciesCountAsync(filter, skip, take);
            var result = new PagedResult<UserStatisticsItem>
            {
                Skip = 0,
                Take = 5,
                TotalCount = 5,
                Records = new List<UserStatisticsItem>
                {
                    new() {UserId = 1, SpeciesCount = 5},
                    new() {UserId = 2, SpeciesCount = 4},
                    new() {UserId = 3, SpeciesCount = 4},
                    new() {UserId = 4, SpeciesCount = 2},
                    new() {UserId = 5, SpeciesCount = 1}
                }
            };

            return result;
        }
    }
}