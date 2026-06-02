using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using SOS.Observations.Api.Configuration;
using SOS.Observations.Api.Repositories;
using System;
using ZiggyCreatures.Caching.Fusion;

namespace SOS.Observations.Api.Services;

public sealed class LocationGeometryCacheMonitor
{
    private readonly FusionCacheUsageMonitor _monitor;

    public LocationGeometryCacheMonitor(
        IFusionCacheProvider fusionCacheProvider,
        ILogger<LocationGeometryCacheMonitor> logger,
        ObservationApiConfiguration observationApiConfiguration)
    {
        var cache = (fusionCacheProvider ?? throw new ArgumentNullException(nameof(fusionCacheProvider)))
            .GetCache(ProcessedLocationRepository.LocationGeometryCacheName);
        var cacheMonitoring = (observationApiConfiguration ?? throw new ArgumentNullException(nameof(observationApiConfiguration))).CacheMonitoring;
        _monitor = new FusionCacheUsageMonitor(
            cache,
            logger ?? throw new ArgumentNullException(nameof(logger)),
            ProcessedLocationRepository.MaxLocationGeometryCacheSizeUnits,
            cacheMonitoring.LocationGeometryLogEveryWriteCount,
            "LocationGeometry",
            cacheMonitoring.Enabled);
    }

    public void StartMonitoring()
    {
        _monitor.StartMonitoring();
    }

    public void TrackGeometryEntry(string locationId, Geometry? geometry)
    {
        _monitor.TrackWrite(locationId, CalculateGeometrySizeUnits(geometry));
    }

    internal static int CalculateGeometrySizeUnits(Geometry? geometry)
    {
        return Math.Max(1, geometry?.NumPoints / 100 ?? 0);
    }
}
