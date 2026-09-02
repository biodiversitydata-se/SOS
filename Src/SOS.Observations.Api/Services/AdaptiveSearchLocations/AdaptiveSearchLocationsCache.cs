using SOS.Lib.Dtos.Filter;
using SOS.Lib.Models.Gis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZiggyCreatures.Caching.Fusion;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal interface IAdaptiveSearchLocationsCache
{
    string CreateCacheKey(int? roleId, string? authorizationApplicationIdentifier, bool sensitiveObservations, SearchFilterInternalDto filter);
    bool TryGet(string cacheKey, LatLonBoundingBox? requestedBbox, out List<CachedLocationAggregate> locations);
    void Set(string cacheKey, LatLonBoundingBox? bbox, List<CachedLocationAggregate> locations);
    LatLonBoundingBox? CreateBoundingBox(IEnumerable<CachedLocationAggregate> locations);
}

internal sealed class AdaptiveSearchLocationsCache : IAdaptiveSearchLocationsCache
{
    internal const string CacheKeyPrefix = "adaptive-search-locations:v2:";
    internal const int MaxEntriesPerKey = 8;
    internal const int MaxLocationsPerBucket = 10000;
    internal static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private const double CoordinateEpsilon = 0.000000000001;

    private readonly IFusionCache _cache;
    private readonly AdaptiveSearchLocationsCacheMonitor _cacheMonitor;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public AdaptiveSearchLocationsCache(
        IFusionCacheProvider fusionCacheProvider,
        AdaptiveSearchLocationsCacheMonitor cacheMonitor)
    {
        _cache = (fusionCacheProvider ?? throw new ArgumentNullException(nameof(fusionCacheProvider)))
            .GetCache(Controllers.ObservationsController.AdaptiveSearchLocationsCacheName);
        _cacheMonitor = cacheMonitor ?? throw new ArgumentNullException(nameof(cacheMonitor));
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters =
            {
                new JsonStringEnumConverter(),
                new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
            }
        };
    }

    public string CreateCacheKey(int? roleId, string? authorizationApplicationIdentifier, bool sensitiveObservations, SearchFilterInternalDto filter)
    {
        var cacheKeyParameters = new[]
        {
            "version=2",
            $"roleId={roleId}",
            $"authorizationApplicationIdentifier={authorizationApplicationIdentifier}",
            $"sensitiveObservations={sensitiveObservations}"
        };
        var cacheKeyFilter = CreateSearchFilterWithoutBoundingBox(filter);
        return $"{CacheKeyPrefix}{CreateHash(string.Join("&", cacheKeyParameters), cacheKeyFilter, _jsonSerializerOptions)}";
    }

    public bool TryGet(string cacheKey, LatLonBoundingBox? requestedBbox, out List<CachedLocationAggregate> locations)
    {
        locations = new List<CachedLocationAggregate>();
        var cachedBucket = _cache.TryGet<AdaptiveSearchLocationsCacheBucket>(cacheKey);
        var bucket = cachedBucket.HasValue ? cachedBucket.Value : null;
        if (bucket?.Entries?.Any() != true)
        {
            return false;
        }

        var cacheEntry = bucket.Entries
            .OrderByDescending(entry => entry.CreatedUtc)
            .FirstOrDefault(entry => ContainsBoundingBox(entry.BoundingBox, requestedBbox));
        if (cacheEntry == null)
        {
            if (requestedBbox?.TopLeft == null || requestedBbox.BottomRight == null)
            {
                return false;
            }

            var intersectingEntries = bucket.Entries
                .Where(entry => IntersectsBoundingBox(entry.BoundingBox, requestedBbox))
                .ToList();
            if (!IsBoundingBoxCoveredByEntries(intersectingEntries, requestedBbox))
            {
                return false;
            }

            locations = MergeLocations(intersectingEntries
                .SelectMany(entry => FilterByBoundingBox(entry.Locations, requestedBbox)));
            return true;
        }

        locations = FilterByBoundingBox(cacheEntry.Locations, requestedBbox).ToList();
        return true;
    }

    public void Set(string cacheKey, LatLonBoundingBox? bbox, List<CachedLocationAggregate> locations)
    {
        if (locations.Count > MaxLocationsPerBucket)
        {
            _cacheMonitor.LogSkippedTooLargeEntry(cacheKey, locations.Count, MaxLocationsPerBucket);
            return;
        }

        var cachedBucket = _cache.TryGet<AdaptiveSearchLocationsCacheBucket>(cacheKey);
        var bucket = cachedBucket.HasValue
            ? cachedBucket.Value ?? new AdaptiveSearchLocationsCacheBucket()
            : new AdaptiveSearchLocationsCacheBucket();
        bucket.Entries.RemoveAll(entry => AreSameBoundingBox(entry.BoundingBox, bbox));
        bucket.Entries.Insert(0, new AdaptiveSearchLocationsCacheEntry
        {
            BoundingBox = bbox,
            Locations = locations
        });
        var trimResult = Trim(bucket);
        _cacheMonitor.LogBucketTrim(
            cacheKey,
            trimResult.RemovedByEntryLimit,
            trimResult.RemovedBySizeLimit,
            trimResult.EntryCountBefore,
            trimResult.EntryCountAfter,
            trimResult.SizeUnitsBefore,
            trimResult.SizeUnitsAfter);

        var cacheOptions = new FusionCacheEntryOptions
        {
            Duration = CacheDuration,
            IsFailSafeEnabled = false
        }.SetSize(bucket.EstimatedSizeUnits);
        _cache.Set(cacheKey, bucket, cacheOptions);
        _cacheMonitor.TrackBucketWrite(cacheKey, bucket.EstimatedSizeUnits);
    }

    public LatLonBoundingBox? CreateBoundingBox(IEnumerable<CachedLocationAggregate> locations)
    {
        double? left = null;
        double? top = null;
        double? right = null;
        double? bottom = null;

        foreach (var locationAggregate in locations)
        {
            var location = locationAggregate.Location;
            if (!location.DecimalLongitude.HasValue || !location.DecimalLatitude.HasValue)
            {
                continue;
            }

            var longitude = location.DecimalLongitude.Value;
            var latitude = location.DecimalLatitude.Value;
            left = left.HasValue ? Math.Min(left.Value, longitude) : longitude;
            right = right.HasValue ? Math.Max(right.Value, longitude) : longitude;
            bottom = bottom.HasValue ? Math.Min(bottom.Value, latitude) : latitude;
            top = top.HasValue ? Math.Max(top.Value, latitude) : latitude;
        }

        return left.HasValue && top.HasValue && right.HasValue && bottom.HasValue
            ? LatLonBoundingBox.Create(left, top, right, bottom)
            : null;
    }

    private SearchFilterInternalDto CreateSearchFilterWithoutBoundingBox(SearchFilterInternalDto filter)
    {
        var json = JsonSerializer.Serialize(filter, _jsonSerializerOptions);
        var clone = JsonSerializer.Deserialize<SearchFilterInternalDto>(json, _jsonSerializerOptions) ?? new SearchFilterInternalDto();
        if (clone.Geographics != null)
        {
            clone.Geographics.BoundingBox = null;
        }

        return clone;
    }

    private static string CreateHash(string queryString, object request, JsonSerializerOptions options)
    {
        var json = JsonSerializer.Serialize(request, options);
        var rawKey = $"{queryString}:{json}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private static TrimResult Trim(AdaptiveSearchLocationsCacheBucket bucket)
    {
        var entryCountBefore = bucket.Entries.Count;
        var sizeUnitsBefore = bucket.EstimatedSizeUnits;
        var removedByEntryLimit = 0;
        var removedBySizeLimit = 0;

        while (bucket.Entries.Count > MaxEntriesPerKey)
        {
            bucket.Entries.RemoveAt(bucket.Entries.Count - 1);
            removedByEntryLimit++;
        }

        while (bucket.EstimatedSizeUnits > MaxLocationsPerBucket && bucket.Entries.Count > 0)
        {
            bucket.Entries.RemoveAt(bucket.Entries.Count - 1);
            removedBySizeLimit++;
        }

        return new TrimResult(
            removedByEntryLimit,
            removedBySizeLimit,
            entryCountBefore,
            bucket.Entries.Count,
            sizeUnitsBefore,
            bucket.EstimatedSizeUnits);
    }

    private readonly record struct TrimResult(
        int RemovedByEntryLimit,
        int RemovedBySizeLimit,
        int EntryCountBefore,
        int EntryCountAfter,
        int SizeUnitsBefore,
        int SizeUnitsAfter);

    private static IEnumerable<CachedLocationAggregate> FilterByBoundingBox(
        IEnumerable<CachedLocationAggregate> locations,
        LatLonBoundingBox? bbox)
    {
        if (bbox?.TopLeft == null || bbox.BottomRight == null)
        {
            return locations;
        }

        return locations.Where(location =>
            location.Location.DecimalLongitude.HasValue &&
            location.Location.DecimalLatitude.HasValue &&
            IsCoordinateWithinBoundingBox(
                location.Location.DecimalLongitude.Value,
                location.Location.DecimalLatitude.Value,
                bbox));
    }

    private static bool ContainsBoundingBox(LatLonBoundingBox? cachedBbox, LatLonBoundingBox? requestedBbox)
    {
        if (requestedBbox?.TopLeft == null || requestedBbox.BottomRight == null)
        {
            return cachedBbox?.TopLeft == null || cachedBbox?.BottomRight == null;
        }

        if (cachedBbox?.TopLeft == null || cachedBbox.BottomRight == null)
        {
            return false;
        }

        return cachedBbox.TopLeft.Longitude <= requestedBbox.TopLeft.Longitude &&
               cachedBbox.BottomRight.Longitude >= requestedBbox.BottomRight.Longitude &&
               cachedBbox.TopLeft.Latitude >= requestedBbox.TopLeft.Latitude &&
               cachedBbox.BottomRight.Latitude <= requestedBbox.BottomRight.Latitude;
    }

    private static bool IntersectsBoundingBox(LatLonBoundingBox? cachedBbox, LatLonBoundingBox requestedBbox)
    {
        if (cachedBbox?.TopLeft == null || cachedBbox.BottomRight == null ||
            requestedBbox.TopLeft == null || requestedBbox.BottomRight == null)
        {
            return false;
        }

        return cachedBbox.TopLeft.Longitude <= requestedBbox.BottomRight.Longitude &&
               cachedBbox.BottomRight.Longitude >= requestedBbox.TopLeft.Longitude &&
               cachedBbox.TopLeft.Latitude >= requestedBbox.BottomRight.Latitude &&
               cachedBbox.BottomRight.Latitude <= requestedBbox.TopLeft.Latitude;
    }

    private static bool IsBoundingBoxCoveredByEntries(
        IReadOnlyCollection<AdaptiveSearchLocationsCacheEntry> entries,
        LatLonBoundingBox requestedBbox)
    {
        if (requestedBbox.TopLeft == null || requestedBbox.BottomRight == null)
        {
            return false;
        }

        var requestedLeft = requestedBbox.TopLeft.Longitude;
        var requestedRight = requestedBbox.BottomRight.Longitude;
        var requestedTop = requestedBbox.TopLeft.Latitude;
        var requestedBottom = requestedBbox.BottomRight.Latitude;
        var clippedRectangles = entries
            .Select(entry => ClipBoundingBox(entry.BoundingBox, requestedLeft, requestedTop, requestedRight, requestedBottom))
            .Where(rectangle => rectangle.HasValue)
            .Select(rectangle => rectangle!.Value)
            .ToList();
        if (clippedRectangles.Count == 0)
        {
            return false;
        }

        var longitudeBoundaries = clippedRectangles
            .SelectMany(rectangle => new[] { rectangle.Left, rectangle.Right })
            .Append(requestedLeft)
            .Append(requestedRight)
            .Distinct()
            .OrderBy(longitude => longitude)
            .ToList();
        for (var index = 0; index < longitudeBoundaries.Count - 1; index++)
        {
            var sliceLeft = longitudeBoundaries[index];
            var sliceRight = longitudeBoundaries[index + 1];
            if (sliceRight - sliceLeft <= CoordinateEpsilon)
            {
                continue;
            }

            var latitudeIntervals = clippedRectangles
                .Where(rectangle =>
                    rectangle.Left <= sliceLeft + CoordinateEpsilon &&
                    rectangle.Right >= sliceRight - CoordinateEpsilon)
                .Select(rectangle => new LatitudeInterval(rectangle.Bottom, rectangle.Top))
                .OrderBy(interval => interval.Bottom)
                .ToList();
            if (!CoversLatitudeInterval(latitudeIntervals, requestedBottom, requestedTop))
            {
                return false;
            }
        }

        return true;
    }

    private static ClippedBoundingBox? ClipBoundingBox(
        LatLonBoundingBox? bbox,
        double requestedLeft,
        double requestedTop,
        double requestedRight,
        double requestedBottom)
    {
        if (bbox?.TopLeft == null || bbox.BottomRight == null)
        {
            return null;
        }

        var left = Math.Max(bbox.TopLeft.Longitude, requestedLeft);
        var right = Math.Min(bbox.BottomRight.Longitude, requestedRight);
        var top = Math.Min(bbox.TopLeft.Latitude, requestedTop);
        var bottom = Math.Max(bbox.BottomRight.Latitude, requestedBottom);
        return right - left > CoordinateEpsilon && top - bottom > CoordinateEpsilon
            ? new ClippedBoundingBox(left, top, right, bottom)
            : null;
    }

    private static bool CoversLatitudeInterval(
        IReadOnlyCollection<LatitudeInterval> intervals,
        double requestedBottom,
        double requestedTop)
    {
        if (intervals.Count == 0)
        {
            return false;
        }

        var coveredTop = requestedBottom;
        foreach (var interval in intervals)
        {
            if (interval.Bottom > coveredTop + CoordinateEpsilon)
            {
                return false;
            }

            coveredTop = Math.Max(coveredTop, interval.Top);
            if (coveredTop >= requestedTop - CoordinateEpsilon)
            {
                return true;
            }
        }

        return false;
    }

    private static List<CachedLocationAggregate> MergeLocations(IEnumerable<CachedLocationAggregate> locations)
    {
        return locations
            .GroupBy(location => location.DistinctLocationId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private static bool AreSameBoundingBox(LatLonBoundingBox? left, LatLonBoundingBox? right)
    {
        if (left?.TopLeft == null || left.BottomRight == null || right?.TopLeft == null || right.BottomRight == null)
        {
            return left?.TopLeft == null && left?.BottomRight == null && right?.TopLeft == null && right?.BottomRight == null;
        }

        return left.TopLeft.Longitude.Equals(right.TopLeft.Longitude) &&
               left.TopLeft.Latitude.Equals(right.TopLeft.Latitude) &&
               left.BottomRight.Longitude.Equals(right.BottomRight.Longitude) &&
               left.BottomRight.Latitude.Equals(right.BottomRight.Latitude);
    }

    private static bool IsCoordinateWithinBoundingBox(double longitude, double latitude, LatLonBoundingBox bbox)
    {
        return longitude >= bbox.TopLeft.Longitude &&
               longitude <= bbox.BottomRight.Longitude &&
               latitude <= bbox.TopLeft.Latitude &&
               latitude >= bbox.BottomRight.Latitude;
    }

    private readonly record struct ClippedBoundingBox(double Left, double Top, double Right, double Bottom);

    private readonly record struct LatitudeInterval(double Bottom, double Top);
}
