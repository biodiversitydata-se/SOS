using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IUserObservationRepository _userObservationRepository;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly ILogger<UserStatisticsManager> _logger;
        private static readonly Dictionary<SpeciesCountUserStatisticsQuery, List<UserStatisticsItem>> _userStatisticsItemsCache = new Dictionary<SpeciesCountUserStatisticsQuery, List<UserStatisticsItem>>(); // todo - use proper cache solution.
        private static readonly Dictionary<PagedSpeciesCountUserStatisticsQuery, PagedResult<UserStatisticsItem>> _pagedUserStatisticsItemsCache = new Dictionary<PagedSpeciesCountUserStatisticsQuery, PagedResult<UserStatisticsItem>>(); // todo - use proper cache solution.
        private static readonly Dictionary<PagedSpeciesCountUserStatisticsQuery, PagedResult<UserStatisticsItem>> _processedObservationPagedUserStatisticsItemsCache = new Dictionary<PagedSpeciesCountUserStatisticsQuery, PagedResult<UserStatisticsItem>>(); // todo - use proper cache solution.


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userObservationRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public UserStatisticsManager(
            IUserObservationRepository userObservationRepository,
            IProcessedObservationRepository processedObservationRepository,
            ILogger<UserStatisticsManager> logger)
        {
            _userObservationRepository = userObservationRepository ??
                                              throw new ArgumentNullException(nameof(userObservationRepository));
            _processedObservationRepository = processedObservationRepository ??
                                         throw new ArgumentNullException(nameof(processedObservationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Make sure we are working with live data
            _userObservationRepository.LiveMode = true;
        }

        public async Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query, 
            int? skip, 
            int? take,
            bool useCache = true)
        {
            PagedResult<UserStatisticsItem> result;
            var pagedQuery = PagedSpeciesCountUserStatisticsQuery.Create(query, skip, take);
            if (useCache && _pagedUserStatisticsItemsCache.ContainsKey(pagedQuery))
            {
                result = _pagedUserStatisticsItemsCache[pagedQuery];
                return result;
            }

            if (!query.IncludeOtherAreasSpeciesCount)
            {
                result = await _userObservationRepository.PagedSpeciesCountSearchAsync(query, skip, take);
            }
            else
            {
                var sortQuery = query;
                if (!string.IsNullOrEmpty(query.SortByFeatureId))
                {
                    sortQuery = query.Clone();
                    sortQuery.FeatureId = query.SortByFeatureId;
                }

                var pagedResult = await _userObservationRepository.PagedSpeciesCountSearchAsync(sortQuery, skip, take);
                var userIds = pagedResult.Records.Select(m => m.UserId).ToList();
                var areaRecords = await _userObservationRepository.AreaSpeciesCountSearchAsync(query, userIds);
                var areaRecordsByUserId = areaRecords.ToDictionary(m => m.UserId, m => m);
                List<UserStatisticsItem> records = new List<UserStatisticsItem>(pagedResult.Records.Count());
                foreach (var item in pagedResult.Records)
                {
                    records.Add(areaRecordsByUserId[item.UserId]);
                }

                pagedResult.Records = records;
                result = pagedResult;
            }
            
            if (useCache && !_pagedUserStatisticsItemsCache.ContainsKey(pagedQuery))
            {
                _pagedUserStatisticsItemsCache.Add(pagedQuery, result); // todo - fix proper caching solution and concurrency handling.
            }
            
            return result;
        }

        public async Task<PagedResult<UserStatisticsItem>> SpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query,
            int? skip,
            int? take,
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
                    records = await _userObservationRepository.SpeciesCountSearchAsync(query);
                    records = records
                        .OrderByDescending(m => m.SpeciesCount)
                        .ThenBy(m => m.UserId)
                        .ToList();
                }
                else
                {
                    // todo - use composite aggregation in order to be sure that the aggregation is correct?
                    records = await _userObservationRepository.AreaSpeciesCountSearchAsync(query, null);
                    records = records
                        .OrderByDescending(m => m.SpeciesCount)
                        .ThenBy(m => m.UserId)
                        .ToList();
                }
            }

            IEnumerable<UserStatisticsItem> orderedRecords;
            if (!string.IsNullOrEmpty(query.SortByFeatureId))
            {
                // todo - fix fast sorting by introducing a dictionary property for AreaCounts.
                orderedRecords = records
                    .Where(m => m.AreaCounts.Any(a => a.FeatureId == "P2"))
                    .OrderByDescending(v => v.AreaCounts.Single(a => a.FeatureId == "P2").SpeciesCount)
                    .ThenBy(m => m.UserId);
            }
            else
            {
                orderedRecords = records;
            }

            UpdateSkipAndTake(ref skip, ref take, orderedRecords.Count());
            var selectedRecords = orderedRecords
                .Skip(skip.GetValueOrDefault())
                .Take(take.GetValueOrDefault());

            var result = new PagedResult<UserStatisticsItem>
            {
                Skip = skip.GetValueOrDefault(),
                Take = take.GetValueOrDefault(),
                TotalCount = orderedRecords.Count(),
                Records = selectedRecords
            };

            if (useCache && !_userStatisticsItemsCache.ContainsKey(query))
            {
                _userStatisticsItemsCache.Add(query, records); // todo - fix proper caching solution and concurrency handling.
            }

            return result;
        }

        public async Task<PagedResult<UserStatisticsItem>> ProcessedObservationPagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query,
            int? skip,
            int? take,
            bool useCache = true)
        {
            PagedResult<UserStatisticsItem> result;
            var pagedQuery = PagedSpeciesCountUserStatisticsQuery.Create(query, skip, take);
            if (useCache && _processedObservationPagedUserStatisticsItemsCache.ContainsKey(pagedQuery))
            {
                _logger.LogInformation("Return result from cache");
                result = _processedObservationPagedUserStatisticsItemsCache[pagedQuery];
                return result;
            }

            if (!query.IncludeOtherAreasSpeciesCount)
            {
                result = await _processedObservationRepository.PagedSpeciesCountSearchAsync(query, skip, take);
            }
            else
            {
                var sortQuery = query;
                if (!string.IsNullOrEmpty(query.SortByFeatureId))
                {
                    sortQuery = query.Clone();
                    sortQuery.FeatureId = query.SortByFeatureId;
                }

                var pagedResult = await _processedObservationRepository.PagedSpeciesCountSearchAsync(sortQuery, skip, take);
                var userIds = pagedResult.Records.Select(m => m.UserId).ToList();
                var areaRecords = await _processedObservationRepository.AreaSpeciesCountSearchAsync(query, userIds);
                var areaRecordsByUserId = areaRecords.ToDictionary(m => m.UserId, m => m);
                List<UserStatisticsItem> records = new List<UserStatisticsItem>(pagedResult.Records.Count());
                foreach (var item in pagedResult.Records)
                {
                    records.Add(areaRecordsByUserId[item.UserId]);
                }

                pagedResult.Records = records;
                result = pagedResult;
            }

            if (useCache && !_processedObservationPagedUserStatisticsItemsCache.ContainsKey(pagedQuery))
            {
                _logger.LogInformation("Add item to cache");
                _processedObservationPagedUserStatisticsItemsCache.Add(pagedQuery, result); // todo - fix proper caching solution and concurrency handling.
            }

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
    }
}