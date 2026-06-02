using Microsoft.Extensions.Logging;
using SOS.Lib.Helpers;
using SOS.Observations.Api.Configuration;
using SOS.Observations.Api.Controllers;
using SOS.Observations.Api.Services;
using System;
using ZiggyCreatures.Caching.Fusion;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal sealed class AdaptiveSearchLocationsCacheMonitor
{
    private readonly bool _isEnabled;
    private readonly ILogger<AdaptiveSearchLocationsCacheMonitor> _logger;
    private readonly FusionCacheUsageMonitor _monitor;

    public AdaptiveSearchLocationsCacheMonitor(
        IFusionCacheProvider fusionCacheProvider,
        ILogger<AdaptiveSearchLocationsCacheMonitor> logger,
        ObservationApiConfiguration observationApiConfiguration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var cache = (fusionCacheProvider ?? throw new ArgumentNullException(nameof(fusionCacheProvider)))
            .GetCache(ObservationsController.AdaptiveSearchLocationsCacheName);
        var cacheMonitoring = (observationApiConfiguration ?? throw new ArgumentNullException(nameof(observationApiConfiguration))).CacheMonitoring;
        _isEnabled = cacheMonitoring.Enabled;
        _monitor = new FusionCacheUsageMonitor(
            cache,
            logger,
            ObservationsController.AdaptiveSearchLocationsCacheSizeLimit,
            cacheMonitoring.AdaptiveSearchLocationsLogEveryWriteCount,
            "AdaptiveSearchLocations",
            cacheMonitoring.Enabled);
    }

    public void StartMonitoring()
    {
        _monitor.StartMonitoring();
    }

    public void TrackBucketWrite(string cacheKey, int sizeUnits)
    {
        _monitor.TrackWrite(cacheKey, sizeUnits);
    }

    public void LogSkippedTooLargeEntry(string cacheKey, int locationCount, int maxLocationsPerBucket)
    {
        if (!_isEnabled)
        {
            return;
        }

        _logger.LogInformation(
            "AdaptiveSearchLocations cache write skipped because location count exceeds max. CacheName={CacheName}, Key=\"{Key}\", LocationCount={LocationCount}, MaxLocationsPerBucket={MaxLocationsPerBucket}, MemoryUsageSummary=\"{MemoryUsageSummary}\"",
            ObservationsController.AdaptiveSearchLocationsCacheName,
            cacheKey,
            locationCount,
            maxLocationsPerBucket,
            LogHelper.GetMemoryUsageSummary());
    }

    public void LogBucketTrim(
        string cacheKey,
        int removedByEntryLimit,
        int removedBySizeLimit,
        int entryCountBefore,
        int entryCountAfter,
        int sizeUnitsBefore,
        int sizeUnitsAfter)
    {
        if (!_isEnabled)
        {
            return;
        }

        if (removedByEntryLimit == 0 && removedBySizeLimit == 0)
        {
            return;
        }

        _logger.LogInformation(
            "AdaptiveSearchLocations cache bucket trimmed. CacheName={CacheName}, Key=\"{Key}\", RemovedByEntryLimit={RemovedByEntryLimit}, RemovedBySizeLimit={RemovedBySizeLimit}, EntryCountBefore={EntryCountBefore}, EntryCountAfter={EntryCountAfter}, SizeUnitsBefore={SizeUnitsBefore}, SizeUnitsAfter={SizeUnitsAfter}, MemoryUsageSummary=\"{MemoryUsageSummary}\"",
            ObservationsController.AdaptiveSearchLocationsCacheName,
            cacheKey,
            removedByEntryLimit,
            removedBySizeLimit,
            entryCountBefore,
            entryCountAfter,
            sizeUnitsBefore,
            sizeUnitsAfter,
            LogHelper.GetMemoryUsageSummary());
    }
}
