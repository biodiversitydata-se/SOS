namespace SOS.UserStatistics.Api.Managers;

public class UserStatisticsManager : IUserStatisticsManager
{
    private readonly IUserStatisticsObservationRepository _userStatisticsObservationRepository;
    private readonly IUserStatisticsProcessedObservationRepository _processedObservationRepository;
    private readonly ILogger<UserStatisticsManager> _logger;

    private static readonly ICacheManager<SpeciesCountUserStatisticsQuery, Dictionary<int, UserStatisticsItem>> _userStatisticsByUserIdManager = 
        new CacheManager<SpeciesCountUserStatisticsQuery, Dictionary<int, UserStatisticsItem>>("UserStatisticsByUserIdManager");
    private static readonly ICacheManager<SpeciesCountUserStatisticsQuery, List<UserStatisticsItem>> _speciesCountAggregationCacheManager = 
        new CacheManager<SpeciesCountUserStatisticsQuery, List<UserStatisticsItem>>("SpeciesCountAggregationCache");
    private static readonly ICacheManager<SpeciesCountUserStatisticsQuery, List<UserStatisticsItem>> _userStatisticsCacheManager = 
        new CacheManager<SpeciesCountUserStatisticsQuery, List<UserStatisticsItem>>("UserStatisticsCacheManager");
    private static readonly ICacheManager<PagedSpeciesCountUserStatisticsQuery, PagedResult<UserStatisticsItem>> _processedObservationPagedUserStatisticsCacheManager = 
        new CacheManager<PagedSpeciesCountUserStatisticsQuery, PagedResult<UserStatisticsItem>>("ProcessedObservationPagedUserStatisticsCache");
    private static readonly ICacheManager<SpeciesSummaryUserStatisticsQuery, SpeciesSummaryItem> _speciesSummaryAggregationCacheManager = 
        new CacheManager<SpeciesSummaryUserStatisticsQuery, SpeciesSummaryItem>("SpeciesSummaryAggregationCache");

    private const int CacheAreaItemsSkipTakeLimit = 100;

    public UserStatisticsManager(IUserStatisticsObservationRepository userStatisticsObservationRepository, IUserStatisticsProcessedObservationRepository userStatisticsProcessedObservationRepository,
        ILogger<UserStatisticsManager> logger)
    {
        _userStatisticsObservationRepository = userStatisticsObservationRepository;
        _processedObservationRepository = userStatisticsProcessedObservationRepository;
        _logger = logger;
        _userStatisticsObservationRepository.LiveMode = true;
    }

    public void ClearCache()
    {
        _userStatisticsByUserIdManager.ClearCache();
        _speciesCountAggregationCacheManager.ClearCache();
        _userStatisticsCacheManager.ClearCache();
        _processedObservationPagedUserStatisticsCacheManager.ClearCache();
        _speciesSummaryAggregationCacheManager.ClearCache();
    }

    public async Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query,
        int? skip, int? take, bool useCache = true)
    {
        IEnumerable<UserStatisticsItem> selectedRecords = null;
        List<UserStatisticsItem> items = null;
        bool itemsFetchedFromCache = false;
        Cache<SpeciesCountUserStatisticsQuery, List<UserStatisticsItem>> cache = null;
        if (useCache)
        {
            cache = _speciesCountAggregationCacheManager.GetCache();
            itemsFetchedFromCache = cache.TryGetValue(query, out items);
        }

        if (!itemsFetchedFromCache)
        {
            var sortQuery = query;
            if (!string.IsNullOrEmpty(query.SortByFeatureId))
            {
                sortQuery = query.Clone();
                sortQuery.FeatureId = query.SortByFeatureId;
            }
            items = await _userStatisticsObservationRepository.SpeciesCountSearchAsync(sortQuery);
            if (useCache)
            {
                cache.AddOrUpdate(query, items, (key, oldvalue) => items);
            }
        }

        UpdateSkipAndTake(ref skip, ref take, items.Count);
        selectedRecords = items
            .Skip(skip.GetValueOrDefault())
            .Take(take.GetValueOrDefault())
            .Select(m => m.Clone()).ToList(); // avoid store SpeciesCountByFeatureId in _userStatisticsItemsCache

        if (query.IncludeOtherAreasSpeciesCount)
        {
            Dictionary<int, UserStatisticsItem> userStatisticsById = new Dictionary<int, UserStatisticsItem>();
            var userStatisticsByIdKey = query.Clone();
            userStatisticsByIdKey.SortByFeatureId = null;
            var userStatisticsByUserIdManagerCache = _userStatisticsByUserIdManager.GetCache();
            if (useCache)
            {
                if (!userStatisticsByUserIdManagerCache.TryGetValue(userStatisticsByIdKey, out userStatisticsById))
                {
                    userStatisticsById = new Dictionary<int, UserStatisticsItem>();
                    userStatisticsByUserIdManagerCache.AddOrUpdate(userStatisticsByIdKey, userStatisticsById, (key, oldvalue) => userStatisticsById);
                }
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
            var areaStatisticsByUserId = foundCachedItemsById;
            if (notFoundUserIds.Any())
            {
                var fetchedItems = await _userStatisticsObservationRepository.AreaSpeciesCountSearchCompositeAsync(query, notFoundUserIds);
                areaStatisticsByUserId = foundCachedItemsById.Values.Union(fetchedItems).ToDictionary(m => m.UserId, m => m);
                if (useCache)
                {
                    int nrItemsToCache = Math.Min(take.GetValueOrDefault(), CacheAreaItemsSkipTakeLimit - skip.GetValueOrDefault()); // just add the most used items to cache
                    var possibleIdsToCache = selectedRecords.Take(nrItemsToCache).Select(m => m.UserId).ToHashSet();
                    var itemsToCache = fetchedItems.Where(m => possibleIdsToCache.Contains(m.UserId));

                    if (itemsToCache.Any())
                    {
                        // Add items to cache
                        foreach (var item in itemsToCache)
                        {
                            userStatisticsById.TryAdd(item.UserId, item);
                        }

                        _logger.LogDebug($"Added items to userStatisticsById: [{string.Join(", ", itemsToCache.Select(m => m.UserId))}]");
                    }

                    _speciesCountAggregationCacheManager.CheckCleanup();
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

    public async Task<PagedResult<SpeciesSummaryItem>> PagedSpeciesSummaryListAsync(SpeciesSummaryUserStatisticsQuery query, int? skip, int? take, bool useCache)
    {
        return null;
    }

    public async Task<PagedResult<UserStatisticsItem>> SpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query,
        int? skip, int? take, bool useCache = true)
    {
        List<UserStatisticsItem> records;
        string sortByFeatureId = query.SortByFeatureId; // todo - temporary hack in order to exclude SortByFeatureId in cache key. Sorting is done in memory.
        query.SortByFeatureId = null;
        var cache = _userStatisticsCacheManager.GetCache();
        if (useCache && cache.TryGetValue(query, out var value))
        {
            records = value;
        }
        else
        {
            if (!query.IncludeOtherAreasSpeciesCount)
            {
                records = await _userStatisticsObservationRepository.SpeciesCountSearchAsync(query);
                records = records
                    .OrderByDescending(m => m.SpeciesCount)
                    .ThenBy(m => m.UserId)
                    .ToList();
            }
            else
            {
                var sumCountRecords = await _userStatisticsObservationRepository.SpeciesCountSearchAsync(query);
                var sumCountRecordsByUserId = sumCountRecords.ToDictionary(m => m.UserId, m => m);
                //var records = await _userObservationRepository.AreaSpeciesCountSearchAsync(query, null); // Get observations without composite aggregation.
                records = await _userStatisticsObservationRepository.AreaSpeciesCountSearchCompositeAsync(query, null);
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

        if (useCache && !cache.ContainsKey(query))
        {
            cache.AddOrUpdate(query, records, (key, oldvalue) => records); // todo - fix proper caching solution and concurrency handling.
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
        var cache = _processedObservationPagedUserStatisticsCacheManager.GetCache();
        if (useCache && cache.TryGetValue(pagedQuery, out var value))
        {
            _logger.LogInformation("Return result from cache");
            return value;
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
            var records = new List<UserStatisticsItem>(pagedResult.Records.Count());
            foreach (var item in pagedResult.Records)
            {
                records.Add(areaRecordsByUserId[item.UserId]);
            }

            pagedResult.Records = records;
            result = pagedResult;
        }

        if (useCache && !cache.ContainsKey(pagedQuery))
        {
            _logger.LogInformation("Add item to cache");
            cache.AddOrUpdate(pagedQuery, result, (key, oldvalue) => result);
        }

        return result;
    }



    private static void UpdateSkipAndTake(ref int? skip, ref int? take, int recordCount)
    {
        skip ??= 0;

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

//public async Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query,
//            int? skip,
//            int? take,
//            bool useCache = true)
//{
//    IEnumerable<UserStatisticsItem> selectedRecords = null;
//    List<UserStatisticsItem> items = null;
//    bool itemsFetchedFromCache = false;
//    SpeciesCountAggregationCache cache = null;
//    if (useCache)
//    {
//        //cache = _speciesCountAggregationCacheManager.GetCache();
//        itemsFetchedFromCache = _speciesCountAggregationCacheManager.TryGetListItems(query, out items);
//    }

//    //if (!useCache || !cache.UserStatisticsItemsCache.ContainsKey(query))
//    if (!itemsFetchedFromCache)
//    {
//        var sortQuery = query;
//        if (!string.IsNullOrEmpty(query.SortByFeatureId))
//        {
//            sortQuery = query.Clone();
//            sortQuery.FeatureId = query.SortByFeatureId;
//        }
//        items = await _userObservationRepository.SpeciesCountSearchAsync(sortQuery);
//        if (useCache)
//        {
//            //cache.UserStatisticsItemsCache.AddOrUpdate(query, items, (key, oldvalue) => items);
//            _speciesCountAggregationCacheManager.Add(query, items);
//        }
//    }

//    UpdateSkipAndTake(ref skip, ref take, items.Count);
//    selectedRecords = items
//        .Skip(skip.GetValueOrDefault())
//        .Take(take.GetValueOrDefault())
//        .Select(m => m.Clone()).ToList(); // avoid store SpeciesCountByFeatureId in _userStatisticsItemsCache

//    if (query.IncludeOtherAreasSpeciesCount)
//    {
//        Dictionary<int, UserStatisticsItem> userStatisticsById = new Dictionary<int, UserStatisticsItem>();
//        var userStatisticsByIdKey = query.Clone();
//        userStatisticsByIdKey.SortByFeatureId = null;
//        if (useCache)
//        {
//            if (!_speciesCountAggregationCacheManager.TryGetIndividualUserStatisticsDictionary(userStatisticsByIdKey, out userStatisticsById))
//            {
//                userStatisticsById = new Dictionary<int, UserStatisticsItem>();
//                _speciesCountAggregationCacheManager.Add(userStatisticsByIdKey, userStatisticsById);
//            }
//        }

//        // Get cached values
//        var foundCachedItemsById = new Dictionary<int, UserStatisticsItem>();
//        var notFoundUserIds = new List<int>();
//        foreach (var item in selectedRecords)
//        {
//            if (userStatisticsById.TryGetValue(item.UserId, out var userStatisticsItem))
//            {
//                foundCachedItemsById.Add(item.UserId, userStatisticsItem);
//            }
//            else
//            {
//                notFoundUserIds.Add(item.UserId);
//            }
//        }

//        // Get values for records not found in cache
//        Dictionary<int, UserStatisticsItem> areaStatisticsByUserId = foundCachedItemsById;
//        if (notFoundUserIds.Any())
//        {
//            var fetchedItems = await _userObservationRepository.AreaSpeciesCountSearchCompositeAsync(query, notFoundUserIds);
//            areaStatisticsByUserId = foundCachedItemsById.Values.Union(fetchedItems).ToDictionary(m => m.UserId, m => m);
//            if (useCache)
//            {
//                int nrItemsToCache = Math.Min(take.GetValueOrDefault(), CacheAreaItemsSkipTakeLimit - skip.GetValueOrDefault()); // just add the most used items to cache
//                var possibleIdsToCache = selectedRecords.Take(nrItemsToCache).Select(m => m.UserId).ToHashSet();
//                var itemsToCache = fetchedItems.Where(m => possibleIdsToCache.Contains(m.UserId));

//                if (itemsToCache.Any())
//                {
//                    // Add items to cache
//                    foreach (var item in itemsToCache)
//                    {
//                        userStatisticsById.TryAdd(item.UserId, item);
//                    }

//                    _logger.LogDebug($"Added items to userStatisticsById: [{string.Join(", ", itemsToCache.Select(m => m.UserId))}]");
//                }
//            }
//        }

//        // Update values in selectedRecords
//        foreach (var selectedRecord in selectedRecords)
//        {
//            var areaStatistics = areaStatisticsByUserId[selectedRecord.UserId];
//            selectedRecord.SpeciesCount = areaStatistics.SpeciesCount;
//            selectedRecord.ObservationCount = areaStatistics.ObservationCount;
//            selectedRecord.SpeciesCountByFeatureId = areaStatistics.SpeciesCountByFeatureId;
//        }
//    }

//    var result = new PagedResult<UserStatisticsItem>()
//    {
//        Records = selectedRecords,
//        Skip = skip.GetValueOrDefault(),
//        Take = take.GetValueOrDefault(),
//        TotalCount = items.Count
//    };

//    return result;
//}
