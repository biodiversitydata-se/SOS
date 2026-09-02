using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Areas.Responses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SOS.Api.Contracts.Areas.Requests;
using System.IO.Compression;
using System.Text.Json;
using SOS.Api.Client.Clients;
using System.Runtime.CompilerServices;

namespace SOS.Api.Client.Services;

/// <summary>
/// Service for area lookup workflows.
/// </summary>
public sealed class SosLookupService : ISosLookupService
{
    private static readonly ConditionalWeakTable<IMemoryCache, SosAreaGeometryCache> AreaGeometryCaches = new();
    private readonly ISosAreasClient _areasClient;
    private readonly SosAreaGeometryCache _areaGeometryCache;
    private readonly SosClientOptions _options;

    /// <summary>Creates a lookup service with shared area caching.</summary>
    public SosLookupService(
        ISosAreasClient areasClient,
        IMemoryCache cache,
        IOptions<SosClientOptions> options)
    {
        _areasClient = areasClient;
        _options = options.Value;
        _areaGeometryCache = AreaGeometryCaches.GetValue(
            cache,
            value => new SosAreaGeometryCache(value, _options.AreaGeometryMaxConcurrency));
    }

    /// <inheritdoc />
    public async Task<AreaWithGeometryDto?> GetAreaWithGeometryAsync(
        AreaTypeDto areaType,
        string featureId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureId))
        {
            throw new ArgumentException("Feature id must not be empty.", nameof(featureId));
        }

        var area = await _areasClient.GetAreaAsync(areaType, featureId, cancellationToken);
        return area is null
            ? null
            : await GetAreaWithGeometryCachedAsync(area, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AreaWithGeometryPagedResult> GetAreasWithGeometryAsync(
        AreasOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var page = await _areasClient.GetAreasAsync(options, cancellationToken);
        if (page?.Records is null || page.Records.Count == 0)
        {
            return new AreaWithGeometryPagedResult
            {
                Skip = page?.Skip ?? options?.Skip ?? 0,
                Take = page?.Take ?? options?.Take ?? 0,
                TotalCount = page?.TotalCount ?? 0
            };
        }

        var tasks = page.Records.Select(area =>
            GetAreaWithGeometryCachedAsync(area, cancellationToken));

        return new AreaWithGeometryPagedResult
        {
            Records = await Task.WhenAll(tasks),
            Skip = page.Skip,
            Take = page.Take,
            TotalCount = page.TotalCount
        };
    }

    private async Task<AreaWithGeometryDto> GetAreaWithGeometryCachedAsync(
        AreaBaseDto area,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"Sos.Api.Client:Area:{area.AreaType}:{area.FeatureId}";
        return await _areaGeometryCache.GetOrCreateAsync(
            cacheKey,
            _options.AreaGeometryCacheSlidingExpiration,
            async token =>
            {
                var bytes = await _areasClient.GetAreaExportAsync(
                    area.AreaType,
                    area.FeatureId,
                    AreaExportFormatDto.Json,
                    token);
                var exported = bytes is null ? null : DeserializeAreaExport(bytes);
                return new AreaWithGeometryDto
                {
                    AreaType = area.AreaType,
                    FeatureId = area.FeatureId,
                    Name = area.Name,
                    BoundingBox = exported?.BoundingBox ?? area.BoundingBox,
                    Geometry = exported?.Geometry
                };
            },
            cancellationToken);
    }

    private static AreaWithGeometryDto? DeserializeAreaExport(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return null;
        }

        if (bytes.Length >= 2 && bytes[0] == (byte)'P' && bytes[1] == (byte)'K')
        {
            using var archiveStream = new MemoryStream(bytes, writable: false);
            using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read);
            var entry = archive.Entries.FirstOrDefault(candidate =>
                candidate.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
            if (entry is null)
            {
                throw new InvalidDataException("The SOS area export did not contain a JSON file.");
            }

            using var entryStream = entry.Open();
            return JsonSerializer.Deserialize<AreaWithGeometryDto>(entryStream, SosClientHttp.SerializerOptions);
        }

        return JsonSerializer.Deserialize<AreaWithGeometryDto>(bytes, SosClientHttp.SerializerOptions);
    }
}
