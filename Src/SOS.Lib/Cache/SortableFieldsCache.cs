using Microsoft.Extensions.Caching.Memory;
using SOS.Lib.Repositories.Processed.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Lib.Cache;

public class SortableFieldsCache
{
    private readonly IProcessedObservationCoreRepository _processedObservationCoreRepository;
    private readonly IMemoryCache _memoryCache;
    private const string _cacheKey = "SortableFields";

    public SortableFieldsCache(IProcessedObservationCoreRepository processedObservationCoreRepository, IMemoryCache memoryCache)
    {
        _processedObservationCoreRepository = processedObservationCoreRepository ?? throw new ArgumentNullException(nameof(processedObservationCoreRepository));
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// Get sortable fields
    /// </summary>
    /// <returns></returns>
    public async Task<IDictionary<string, string>> GetSortableFieldsAsync()
    {
        if (_memoryCache.TryGetValue(_cacheKey, out IDictionary<string, string> sortableFields) && (sortableFields?.Count() ?? 0) != 0) {
            return sortableFields;
        }

        var hashSet = await _processedObservationCoreRepository.GetSortableFieldsAsync();
        sortableFields = hashSet?.ToDictionary(x => x.ToLower(), x => x);
        var cacheEntryOptions = new MemoryCacheEntryOptions()
                   .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));
        _memoryCache.Set(_cacheKey, hashSet, cacheEntryOptions);

        return sortableFields;
    }
}
