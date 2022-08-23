using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        //private static readonly Dictionary<PagedSpeciesCountUserStatisticsQuery, PagedResult<UserStatisticsItem>> _pagedUserStatisticsItemsCache = new Dictionary<PagedSpeciesCountUserStatisticsQuery, PagedResult<UserStatisticsItem>>(); // todo - use proper cache solution.
        private static readonly Dictionary<PagedSpeciesCountUserStatisticsQuery, PagedResult<UserStatisticsItem>> _processedObservationPagedUserStatisticsItemsCache = new Dictionary<PagedSpeciesCountUserStatisticsQuery, PagedResult<UserStatisticsItem>>(); // todo - use proper cache solution.
        private static readonly Dictionary<SpeciesCountUserStatisticsQuery, List<UserStatisticsItem>> _userStatisticsItemsCacheNew = new Dictionary<SpeciesCountUserStatisticsQuery, List<UserStatisticsItem>>(); // todo - use proper cache solution.
        private static readonly Dictionary<SpeciesCountUserStatisticsQuery, Dictionary<int, UserStatisticsItem>> _userStatisticsByUserIdCache = new Dictionary<SpeciesCountUserStatisticsQuery, Dictionary<int, UserStatisticsItem>>();
        private const int CacheAreaItemsSkipTakeLimit = 100;

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
            IEnumerable<UserStatisticsItem> selectedRecords = null;
            List<UserStatisticsItem> items = null;
            if (useCache && _userStatisticsItemsCacheNew.ContainsKey(query))
            {
                items = _userStatisticsItemsCacheNew[query];
            }
            else
            {
                var sortQuery = query;
                if (!string.IsNullOrEmpty(query.SortByFeatureId))
                {
                    sortQuery = query.Clone();
                    sortQuery.FeatureId = query.SortByFeatureId;
                }
                items = await _userObservationRepository.SpeciesCountSearchAsync(sortQuery);
                if (useCache && !_userStatisticsItemsCacheNew.ContainsKey(query))
                {
                    _userStatisticsItemsCacheNew.TryAdd(query, items); // todo - fix proper caching solution and concurrency handling.
                }
            }

            UpdateSkipAndTake(ref skip, ref take, items.Count);
            selectedRecords = items
                .Skip(skip.GetValueOrDefault())
                .Take(take.GetValueOrDefault())
                .Select(m => m.Clone()).ToList(); // avoid store SpeciesCountByFeatureId in _userStatisticsItemsCache

            if (query.IncludeOtherAreasSpeciesCount)
            {
                // todo - add semaphore to handle concurrency issues?
                if (!_userStatisticsByUserIdCache.TryGetValue(query, out var userStatisticsById))
                {
                    userStatisticsById = new Dictionary<int, UserStatisticsItem>();
                    _userStatisticsByUserIdCache.TryAdd(query, userStatisticsById);
                }

                // Get cached values
                var foundCachedItemsById = new Dictionary<int, UserStatisticsItem>();
                var notFoundUserIds = new List<int>();
                foreach (var item in selectedRecords)
                {
                    if (userStatisticsById.TryGetValue(item.UserId, out var userStatisticsItem))
                    {
                        foundCachedItemsById.Add(item.UserId, userStatisticsItem);
                    }
                    else
                    {
                        notFoundUserIds.Add(item.UserId);
                    }
                }

                // Get values for records not found in cache
                Dictionary<int, UserStatisticsItem> areaStatisticsByUserId = foundCachedItemsById;
                if (notFoundUserIds.Any())
                {
                    var fetchedItems = await _userObservationRepository.AreaSpeciesCountSearchCompositeAsync(query, notFoundUserIds);
                    areaStatisticsByUserId = foundCachedItemsById.Values.Union(fetchedItems).ToDictionary(m => m.UserId, m => m);
                    if (useCache)
                    {
                        int nrItemsToCache = Math.Min(take.GetValueOrDefault(), CacheAreaItemsSkipTakeLimit - skip.GetValueOrDefault()); // just add the most used items to cache
                        var itemsToCache = selectedRecords.Take(nrItemsToCache).Where(s => fetchedItems.Any(f => f.UserId == s.UserId));

                        if (itemsToCache.Any())
                        {
                            // Add items to cache
                            foreach (var item in itemsToCache)
                            {
                                userStatisticsById.TryAdd(item.UserId, item);
                            }

                            _logger.LogDebug($"Added items to userStatisticsById: [{string.Join(", ", itemsToCache.Select(m => m.UserId))}]");
                        }
                    }
                }
                
                // Update values in selectedRecords
                foreach (var selectedRecord in selectedRecords)
                {
                    var areaStatistics = areaStatisticsByUserId[selectedRecord.UserId];
                    selectedRecord.SpeciesCount = areaStatistics.SpeciesCount;
                    selectedRecord.ObservationCount = areaStatistics.ObservationCount;
                    selectedRecord.SpeciesCountByFeatureId = areaStatistics.SpeciesCountByFeatureId;
                }
            }
            
            var result = new PagedResult<UserStatisticsItem>()
            {
                Records = selectedRecords,
                Skip = skip.GetValueOrDefault(),
                Take = take.GetValueOrDefault(),
                TotalCount = items.Count
            };

            return result;
        }

        public async Task<PagedResult<UserStatisticsItem>> SpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query,
            int? skip,
            int? take,
            bool useCache = true)
        {
            List<UserStatisticsItem> records;
            string sortByFeatureId = query.SortByFeatureId; // todo - temporary hack in order to exclude SortByFeatureId in cache key. Sorting is done in memory.
            query.SortByFeatureId = null;
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
                    var sumCountRecords = await _userObservationRepository.SpeciesCountSearchAsync(query);
                    var sumCountRecordsByUserId = sumCountRecords.ToDictionary(m => m.UserId, m => m);
                    //var records = await _userObservationRepository.AreaSpeciesCountSearchAsync(query, null); // Get observations without composite aggregation.
                    records = await _userObservationRepository.AreaSpeciesCountSearchCompositeAsync(query, null);
                    foreach (var item in records)
                    {
                        item.ObservationCount = sumCountRecordsByUserId[item.UserId].ObservationCount;
                        item.SpeciesCount = sumCountRecordsByUserId[item.UserId].SpeciesCount;
                    }

                    records = records
                        .OrderByDescending(m => m.SpeciesCount)
                        .ThenBy(m => m.UserId)
                        .ToList();
                }
            }

            IEnumerable<UserStatisticsItem> orderedRecords;
            if (!string.IsNullOrEmpty(sortByFeatureId))
            {
                orderedRecords = records
                    .Where(m => m.SpeciesCountByFeatureId.ContainsKey(sortByFeatureId))
                    .OrderByDescending(v => v.SpeciesCountByFeatureId[sortByFeatureId])
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

//public async Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query,
//     int? skip,
//     int? take,
//     bool useCache = true)
//{
//    PagedResult<UserStatisticsItem> result;
//    IEnumerable<UserStatisticsItem> selectedRecords = null;
//    List<UserStatisticsItem> items = null;
//    //var pagedQuery = PagedSpeciesCountUserStatisticsQuery.Create(query, skip, take);
//    if (useCache && _userStatisticsItemsCache.ContainsKey(query))
//    {
//        items = _userStatisticsItemsCache[query];
//    }
//    else
//    {
//        var sortQuery = query;
//        if (!string.IsNullOrEmpty(query.SortByFeatureId))
//        {
//            sortQuery = query.Clone();
//            sortQuery.FeatureId = query.SortByFeatureId;
//        }
//        items = await _userObservationRepository.SpeciesCountSearchAsync(sortQuery);
//        if (useCache && !_userStatisticsItemsCache.ContainsKey(query))
//        {
//            _userStatisticsItemsCache.Add(query, items); // todo - fix proper caching solution and concurrency handling.
//        }
//    }

//    UpdateSkipAndTake(ref skip, ref take, items.Count);
//    selectedRecords = items
//        .Skip(skip.GetValueOrDefault())
//        .Take(take.GetValueOrDefault())
//        .Select(m => m.Clone()).ToList(); // avoid store SpeciesCountByFeatureId in _userStatisticsItemsCache

//    var selectedRecordById = selectedRecords.ToDictionary(m => m.UserId, m => m);

//    if (query.IncludeOtherAreasSpeciesCount)
//    {
//        if (!_userStatisticsByUserIdCache.TryGetValue(query, out var userStatisticsById))
//        {
//            userStatisticsById = new Dictionary<int, UserStatisticsItem>();
//            _userStatisticsByUserIdCache.Add(query, userStatisticsById);
//        }

//        var foundCachedItemsById = new Dictionary<int, UserStatisticsItem>();
//        var foundRecordById = new Dictionary<int, UserStatisticsItem>();
//        var notFoundUserIds = new List<int>();
//        var notFoundRecordById = new Dictionary<int, UserStatisticsItem>();
//        foreach (var item in selectedRecords)
//        {
//            if (userStatisticsById.TryGetValue(item.UserId, out var userStatisticsItem))
//            {
//                foundCachedItemsById.Add(item.UserId, userStatisticsItem);
//                foundRecordById.Add(item.UserId, item);
//            }
//            else
//            {
//                notFoundUserIds.Add(item.UserId);
//                notFoundRecordById.Add(item.UserId, item);
//            }
//        }

//        var fetchedItemById = new Dictionary<int, UserStatisticsItem>();
//        if (notFoundUserIds.Any())
//        {
//            var notFoundAreaRecords = await _userObservationRepository.AreaSpeciesCountSearchCompositeAsync(query, notFoundUserIds);
//            Dictionary<int, UserStatisticsItem> notFoundAreaRecordsByUserId = notFoundAreaRecords.ToDictionary(m => m.UserId, m => m);
//            List<UserStatisticsItem> notFoundSumRecords = await _userObservationRepository.SpeciesCountSearchAsync(query, notFoundUserIds);
//            Dictionary<int, UserStatisticsItem> notFoundSumRecordByUserId = notFoundSumRecords.ToDictionary(m => m.UserId, m => m);
//            foreach (var item in notFoundRecordById.Values)
//            {
//                var record = notFoundAreaRecordsByUserId[item.UserId];
//                record.ObservationCount = notFoundSumRecordByUserId[item.UserId].ObservationCount;
//                record.SpeciesCount = notFoundSumRecordByUserId[item.UserId].SpeciesCount;
//                //item.SpeciesCountByFeatureId = record.SpeciesCountByFeatureId;
//                //fetchedItemById.Add(item.UserId, item);
//                fetchedItemById.Add(item.UserId, record);
//            }
//        }

//        if (useCache && skip + take < 100)
//        {
//            // Add items to cache
//            foreach (var item in fetchedItemById)
//            {
//                userStatisticsById.Add(item.Key, item.Value);
//            }
//        }

//        Dictionary<int, UserStatisticsItem> areaStatisticsByUserId = new Dictionary<int, UserStatisticsItem>();
//        areaStatisticsByUserId = foundCachedItemsById.Union(fetchedItemById).ToDictionary(m => m.Key, m => m.Value);
//        foreach (var selectedRecord in selectedRecords)
//        {
//            var areaStatistics = areaStatisticsByUserId[selectedRecord.UserId];
//            selectedRecord.SpeciesCount = areaStatistics.SpeciesCount;
//            selectedRecord.ObservationCount = areaStatistics.ObservationCount;
//            selectedRecord.SpeciesCountByFeatureId = areaStatistics.SpeciesCountByFeatureId;
//        }

//    }

//    result = new PagedResult<UserStatisticsItem>()
//    {
//        Records = selectedRecords,
//        Skip = skip.GetValueOrDefault(),
//        Take = take.GetValueOrDefault(),
//        TotalCount = items.Count
//    };

//    return result;
//}