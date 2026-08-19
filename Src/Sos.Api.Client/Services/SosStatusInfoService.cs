using SOS.Api.Client.Abstractions;
using SOS.Api.Contracts.Status.Responses;
using System.Globalization;

namespace SOS.Api.Client.Services;

/// <inheritdoc />
public sealed class SosStatusInfoService : ISosStatusInfoService
{
    private readonly ISosSystemsStatusInfoClient _statusInfoClient;

    /// <summary>Creates the service using the system-status client.</summary>
    public SosStatusInfoService(ISosSystemsStatusInfoClient statusInfoClient)
    {
        _statusInfoClient = statusInfoClient ?? throw new ArgumentNullException(nameof(statusInfoClient));
    }

    /// <inheritdoc />
    public async Task<List<DataProviderStatusDto>?> GetDataProviderStatusAsync(CancellationToken cancellationToken = default)
    {
        var processSummary = await _statusInfoClient.GetProcessSummaryAsync(cancellationToken);
        return processSummary?.DataProviderStatuses;
    }

    /// <inheritdoc />
    public async Task<List<DataProviderStatusRow>> GetDataProviderStatusRowsAsync(CancellationToken cancellationToken = default)
    {
        var activeInstance = await _statusInfoClient.GetActiveInstanceAsync(cancellationToken);
        var processInfos = await _statusInfoClient.GetMongoDbProcessInfoAsync(cancellationToken);
        return BuildDataStatusRows(processInfos, activeInstance);
    }

    /// <inheritdoc />
    public async Task<HealthReportDto?> GetObservationsApiHealthAsync(CancellationToken cancellationToken = default)
    {
        var healthReport = await _statusInfoClient.GetHealthAsync(cancellationToken);
        if (healthReport is null)
        {
            return null;
        }

        var enrichedReport = CloneHealthReport(healthReport);
        enrichedReport.Entries?.Remove("self");

        var mongoDbInfo = await _statusInfoClient.GetMongoDatabaseInfoAsync(cancellationToken);
        AddMongoDbDiskEntry(enrichedReport, mongoDbInfo);

        var searchIndexInfo = await _statusInfoClient.GetSearchIndexInfoAsync(cancellationToken);
        AddElasticsearchDiskEntry(enrichedReport, searchIndexInfo);

        return enrichedReport;
    }

    private static List<DataProviderStatusRow> BuildDataStatusRows(
        IEnumerable<MongoDbProcessInfoDto>? processInfos,
        ActiveInstanceInfoDto? activeInstanceInfo)
    {
        if (processInfos is null || activeInstanceInfo is null)
        {
            return [];
        }

        var processInfoList = processInfos.ToList();
        var activeInfos = processInfoList.FirstOrDefault(info => TryGetInstanceNumber(info?.Id) == activeInstanceInfo.ActiveInstance);
        var inactiveInfos = processInfoList.FirstOrDefault(info => TryGetInstanceNumber(info?.Id) is byte instance && instance != activeInstanceInfo.ActiveInstance);

        if (activeInfos?.ProvidersInfo is null || inactiveInfos?.ProvidersInfo is null)
        {
            return [];
        }

        var inactiveProvidersById = inactiveInfos.ProvidersInfo
            .Where(provider => provider.DataProviderId.HasValue)
            .ToDictionary(provider => provider.DataProviderId!.Value, provider => provider);

        return activeInfos.ProvidersInfo
            .Where(provider => provider.DataProviderId.HasValue)
            .OrderBy(provider => provider.DataProviderId)
            .Select(activeProvider =>
            {
                inactiveProvidersById.TryGetValue(activeProvider.DataProviderId!.Value, out var inactiveProvider);
                var activeInvalidCount = activeProvider.ProcessFailCount.GetValueOrDefault();
                var inactiveInvalidCount = inactiveProvider?.ProcessFailCount.GetValueOrDefault() ?? 0;

                return new DataProviderStatusRow
                {
                    Datasource = activeProvider.DataProviderIdentifier ?? string.Empty,
                    DataProviderId = activeProvider.DataProviderId.Value,
                    PublicActive = activeProvider.PublicProcessCount ?? 0,
                    PublicInactive = inactiveProvider?.PublicProcessCount ?? 0,
                    PublicDiff = (activeProvider.PublicProcessCount ?? 0) - (inactiveProvider?.PublicProcessCount ?? 0),
                    ProtectedActive = activeProvider.ProtectedProcessCount ?? 0,
                    ProtectedInactive = inactiveProvider?.ProtectedProcessCount ?? 0,
                    ProtectedDiff = (activeProvider.ProtectedProcessCount ?? 0) - (inactiveProvider?.ProtectedProcessCount ?? 0),
                    InvalidActive = activeInvalidCount,
                    InvalidInactive = inactiveInvalidCount,
                    InvalidDiff = activeInvalidCount - inactiveInvalidCount
                };
            })
            .OrderBy(row => row.DataProviderId)
            .ToList();
    }

    private static byte? TryGetInstanceNumber(string? processId)
    {
        if (string.IsNullOrWhiteSpace(processId))
        {
            return null;
        }

        var lastCharacter = processId[^1];
        return byte.TryParse(lastCharacter.ToString(), out var instanceNumber) ? instanceNumber : null;
    }

    private static HealthReportDto CloneHealthReport(HealthReportDto source)
    {
        return new HealthReportDto
        {
            Status = source.Status,
            TotalDuration = source.TotalDuration,
            Entries = source.Entries?.ToDictionary(
                pair => pair.Key,
                pair => new HealthReportEntryDto
                {
                    Status = pair.Value.Status,
                    Description = pair.Value.Description,
                    Duration = pair.Value.Duration,
                    Tags = pair.Value.Tags?.ToList(),
                    Data = pair.Value.Data is null ? null : new Dictionary<string, object>(pair.Value.Data)
                })
        };
    }

    private static void AddMongoDbDiskEntry(HealthReportDto report, MongoDbInfoDto? mongoDbInfo)
    {
        if (!TryParseDouble(mongoDbInfo?.DiskUsed, out var diskUsed) ||
            !TryParseDouble(mongoDbInfo?.DiskTotal, out var diskTotal) ||
            diskTotal <= 0)
        {
            return;
        }

        EnsureEntries(report)["MongoDB disk"] = new HealthReportEntryDto
        {
            Status = "Healthy",
            Description = $"Disk used: {(int)Math.Round(diskUsed / diskTotal * 100)}%"
        };
    }

    private static void AddElasticsearchDiskEntry(HealthReportDto report, SearchIndexInfoDto? searchIndexInfo)
    {
        var allocation = searchIndexInfo?.Allocations?.FirstOrDefault();
        if (allocation is null ||
            !TryParseDouble(allocation.DiskUsed, out var diskUsed) ||
            !TryParseDouble(allocation.DiskAvailable, out var diskAvailable))
        {
            return;
        }

        var diskTotal = diskUsed + diskAvailable;
        if (diskTotal <= 0)
        {
            return;
        }

        EnsureEntries(report)["Elasticsearch disk"] = new HealthReportEntryDto
        {
            Status = "Healthy",
            Description = $"Disk used: {(int)Math.Round(diskUsed / diskTotal * 100)}%"
        };
    }

    private static Dictionary<string, HealthReportEntryDto> EnsureEntries(HealthReportDto report)
    {
        report.Entries ??= new Dictionary<string, HealthReportEntryDto>(StringComparer.OrdinalIgnoreCase);
        return report.Entries;
    }

    private static bool TryParseDouble(string? value, out double parsedValue)
    {
        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedValue);
    }
}
