using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
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
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IFilterManager _filterManager;
        private readonly ILogger<UserStatisticsManager> _logger;
        private readonly Dictionary<SpeciesCountUserStatisticsQuery, List<UserStatisticsItem>> _userStatisticsItemsCache = new Dictionary<SpeciesCountUserStatisticsQuery, List<UserStatisticsItem>>(); // todo - use proper cache solution.


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="filterManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public UserStatisticsManager(
            IProcessedObservationRepository processedObservationRepository,
            IFilterManager filterManager,
            ILogger<UserStatisticsManager> logger)
        {
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Make sure we are working with live data
            _processedObservationRepository.LiveMode = true;
        }

        public async Task<IEnumerable<UserStatisticsItem>> SpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query, bool useCache = true)
        {
            if (useCache && _userStatisticsItemsCache.ContainsKey(query))
            {
                return _userStatisticsItemsCache[query];
            }
            
            // todo - implement logic
            //await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            //var result = await _processedLocationRepository.SearchUserSpeciesCountAsync(filter, skip, take);
            var result = PredefinedRecords;

            if (useCache && !_userStatisticsItemsCache.ContainsKey(query))
            {
                _userStatisticsItemsCache.Add(query, result); // todo - fix proper caching solution and concurrency handling.
            }

            return result;
        }

        public async Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query, 
            int? skip, 
            int? take,
            string sortBy,
            bool useCache = true)
        {
            List<UserStatisticsItem> records;
            if (useCache && _userStatisticsItemsCache.ContainsKey(query))
            {
                records = _userStatisticsItemsCache[query];
            }
            else
            {
                if (!query.IncludeOtherAreasSpeciesCount)
                {
                    // todo - implement logic. For caching purpose: Load all records, not just the ones returned by skip and take.
                    // Perhaps loading all records could be slow. One solution could be to only records for the selected page
                    // (30 persons in Artportalen), but also start another thread with a query for all persons and store
                    // that result in cache when its ready.
                    // The number of threads should probably be limited by using a semaphore.
                    records = PredefinedRecords;
                }
                else
                {
                    // todo - implement logic. For caching purpose: Load all records, not just the ones returned by skip and take?
                    // Perhaps loading all records for other areas (provinces) could be really slow. One solution could be to only
                    // load other areas for the selected page (30 persons in Artportalen), but also start another thread with a
                    // query for all persons and store that result in cache when its ready.
                    // The number of threads should probably be limited by using a semaphore.
                    records = PredefinedRecordsWithAreas;
                }
            }

            UpdateSkipAndTake(ref skip, ref take, records.Count);
            var selectedRecords = records
                .OrderByDescending(m => m.SpeciesCount) // todo - use sortBy to decide sort property
                .Skip(skip.GetValueOrDefault())
                .Take(take.GetValueOrDefault());

            var result = new PagedResult<UserStatisticsItem>
            {
                Skip = skip.GetValueOrDefault(),
                Take = take.GetValueOrDefault(),
                TotalCount = records.Count,
                Records = selectedRecords
            };

            if (useCache && !_userStatisticsItemsCache.ContainsKey(query))
            {
                _userStatisticsItemsCache.Add(query, records); // todo - fix proper caching solution and concurrency handling.
            }

            // todo - implement logic
            //await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            //var result = await _processedLocationRepository.SearchUserSpeciesCountAsync(filter, skip, take);

            return result;
        }

        private static void UpdateSkipAndTake(ref int? skip, ref int? take, int recordCount)
        {
            if (skip == null)
            {
                skip = 0;
            }

            if (skip > recordCount)
            {
                skip = recordCount;
            }

            if (take == null)
            {
                take = recordCount - skip;
            }
            else
            {
                take = Math.Min(recordCount - skip.Value, take.Value);
            }
        }


        private static List<UserStatisticsItem> PredefinedRecords = new()
        {
            new() { UserId = 1, SpeciesCount = 5 },
            new() { UserId = 2, SpeciesCount = 4 },
            new() { UserId = 3, SpeciesCount = 3 },
            new() { UserId = 4, SpeciesCount = 2 },
            new() { UserId = 5, SpeciesCount = 1 }
        };

        private static List<UserStatisticsItem> PredefinedRecordsWithAreas = new()
        {
            new() {UserId = 1, SpeciesCount = 5, SpeciesCountByFeatureId = new Dictionary<string, int> {
                    {"P1", 5}, {"P2", 2}, {"P3", 2}
                }
            },
            new() {UserId = 2, SpeciesCount = 4, SpeciesCountByFeatureId = new Dictionary<string, int> {
                    {"P1", 4}, {"P2", 3}, {"P3", 2}
                }
            },
            new() {UserId = 3, SpeciesCount = 3, SpeciesCountByFeatureId = new Dictionary<string, int> {
                    {"P1", 3}, {"P2", 1}, {"P3", 1}
                }
            },
            new() {UserId = 4, SpeciesCount = 2, SpeciesCountByFeatureId = new Dictionary<string, int> {
                    {"P1", 2}, {"P2", 2}
                }
            },
            new() {UserId = 5, SpeciesCount = 1, SpeciesCountByFeatureId = new Dictionary<string, int> {
                    {"P1", 1}, {"P4", 1}
                }
            }
        };
    }
}