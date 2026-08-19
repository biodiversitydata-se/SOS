using SOS.Api.Client.Abstractions;
using SOS.Status.Web.Client.Abstractions;
using SOS.Status.Web.HttpClients;
using WebDtos = SOS.Status.Web.Client.Dtos;
using WebModels = SOS.Status.Web.Client.Models;

namespace SOS.Status.Web.Services;

public class StatusInfoService : IStatusInfoService
{
    private readonly ISosStatusInfoService _sosStatusInfoService;
    private readonly ISosSystemsStatusInfoClient _systemsStatusInfoClient;
    private readonly SosAnalysisApiClient _analysisApiClient;
    private readonly SosElasticsearchProxyClient _elasticsearchProxyClient;
    private readonly SosDataStewardshipApiClient _dataStewardshipApiClient;
    private readonly ILogger<StatusInfoService> _logger;

    public StatusInfoService(
        ISosStatusInfoService sosStatusInfoService,
        ISosSystemsStatusInfoClient systemsStatusInfoClient,
        SosAnalysisApiClient analysisApiClient,
        SosElasticsearchProxyClient sosElasticsearchProxyClient,
        SosDataStewardshipApiClient sosDataStewardshipApiClient,
        ILogger<StatusInfoService> logger)
    {
        _sosStatusInfoService = sosStatusInfoService ?? throw new ArgumentNullException(nameof(sosStatusInfoService));
        _systemsStatusInfoClient = systemsStatusInfoClient ?? throw new ArgumentNullException(nameof(systemsStatusInfoClient));
        _analysisApiClient = analysisApiClient ?? throw new ArgumentNullException(nameof(analysisApiClient));
        _elasticsearchProxyClient = sosElasticsearchProxyClient ?? throw new ArgumentNullException(nameof(sosElasticsearchProxyClient));
        _dataStewardshipApiClient = sosDataStewardshipApiClient ?? throw new ArgumentNullException(nameof(sosDataStewardshipApiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<WebDtos.ProcessSummaryDto?> GetProcessSummaryAsync()
    {
        var processSummary = await _systemsStatusInfoClient.GetProcessSummaryAsync();
        return processSummary is null ? null : MapProcessSummary(processSummary);
    }

    public async Task<WebDtos.ProcessInfoDto?> GetProcessInfoAsync(bool active)
    {
        var processInfo = await _systemsStatusInfoClient.GetProcessInfoAsync(active);
        return processInfo is null ? null : MapProcessInfo(processInfo);
    }

    public async Task<IEnumerable<WebDtos.MongoDbProcessInfoDto>?> GetProcessInfoAsync()
    {
        var processInfo = await _systemsStatusInfoClient.GetMongoDbProcessInfoAsync();
        return processInfo?.Select(MapMongoDbProcessInfo);
    }

    public async Task<List<WebDtos.DataProviderStatusDto>?> GetDataProviderStatusAsync()
    {
        var dataProviderStatus = await _sosStatusInfoService.GetDataProviderStatusAsync();
        return dataProviderStatus?.Select(MapDataProviderStatus).ToList();
    }

    public async Task<List<WebModels.DataProviderStatusRow>?> GetDataProviderStatusRowsAsync()
    {
        var rows = await _sosStatusInfoService.GetDataProviderStatusRowsAsync();
        return rows.Select(MapDataProviderStatusRow).ToList();
    }

    public async Task<WebModels.HealthReportDto?> GetObservationsApiHealthAsync()
    {
        try
        {
            var healthReport = await _sosStatusInfoService.GetObservationsApiHealthAsync();
            return healthReport is null ? null : MapHealthReport(healthReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not fetch health from observations API.");
            return null;
        }
    }

    public async Task<WebModels.HealthReportDto?> GetAnalysisApiHealthAsync()
    {
        var healthReport = await _analysisApiClient.GetHealthAsync();
        healthReport?.Entries?.Remove("self");
        return healthReport;
    }

    public async Task<WebModels.HealthReportDto?> GetElasticsearchProxyHealthAsync()
    {
        var healthReport = await _elasticsearchProxyClient.GetHealthAsync();
        healthReport?.Entries?.Remove("self");
        return healthReport;
    }

    public async Task<WebModels.HealthReportDto?> GetDataStewardshipApiHealthAsync()
    {
        var healthReport = await _dataStewardshipApiClient.GetHealthAsync();
        healthReport?.Entries?.Remove("self");
        return healthReport;
    }

    private static WebDtos.ProcessSummaryDto MapProcessSummary(SOS.Api.Contracts.Status.Responses.ProcessSummaryDto source)
    {
        return new WebDtos.ProcessSummaryDto
        {
            ActiveProcessStatus = MapProcessStatus(source.ActiveProcessStatus),
            InactiveProcessStatus = MapProcessStatus(source.InactiveProcessStatus),
            DataProviderStatuses = source.DataProviderStatuses?.Select(MapDataProviderStatus).ToList()
        };
    }

    private static WebDtos.ProcessStatusDto MapProcessStatus(SOS.Api.Contracts.Status.Responses.ProcessStatusDto? source)
    {
        return new WebDtos.ProcessStatusDto
        {
            Name = source?.Name ?? string.Empty,
            Status = source?.Status ?? string.Empty,
            PublicCount = source?.PublicCount ?? 0,
            ProtectedCount = source?.ProtectedCount ?? 0,
            InvalidCount = source?.InvalidCount ?? 0,
            Start = source?.Start ?? default,
            End = source?.End ?? default
        };
    }

    private static WebDtos.ProcessInfoDto MapProcessInfo(SOS.Api.Contracts.Metadata.Responses.ProcessInfoDto source)
    {
        return new WebDtos.ProcessInfoDto
        {
            Id = source.Id,
            PublicCount = source.PublicCount,
            ProtectedCount = source.ProtectedCount,
            End = source.End.DateTime,
            Start = source.Start.DateTime,
            Status = source.Status,
            ProvidersInfo = source.ProvidersInfo?.Select(MapProviderInfo)
        };
    }

    private static WebDtos.ProcessInfoDto.Provider MapProviderInfo(SOS.Api.Contracts.Metadata.Responses.ProviderInfoDto source)
    {
        return new WebDtos.ProcessInfoDto.Provider
        {
            DataProviderId = source.DataProviderId,
            DataProviderIdentifier = source.DataProviderIdentifier,
            ProcessEnd = source.ProcessEnd?.DateTime,
            ProcessStart = source.ProcessStart.DateTime,
            ProcessStatus = source.ProcessStatus,
            PublicProcessCount = source.PublicProcessCount,
            ProtectedProcessCount = source.ProtectedProcessCount,
            HarvestEnd = source.HarvestEnd?.DateTime,
            HarvestStart = source.HarvestStart?.DateTime,
            HarvestNotes = source.HarvestNotes,
            HarvestStatus = source.HarvestStatus,
            HarvestCount = source.HarvestCount,
            LatestIncrementalEnd = source.LatestIncrementalEnd?.DateTime,
            LatestIncrementalStart = source.LatestIncrementalStart?.DateTime,
            LatestIncrementalStatus = source.LatestIncrementalStatus,
            LatestIncrementalPublicCount = source.LatestIncrementalPublicCount,
            LatestIncrementalProtectedCount = source.LatestIncrementalProtectedCount,
            DataProviderType = source.DataProviderType
        };
    }

    private static WebDtos.MongoDbProcessInfoDto MapMongoDbProcessInfo(SOS.Api.Contracts.Status.Responses.MongoDbProcessInfoDto source)
    {
        return new WebDtos.MongoDbProcessInfoDto
        {
            Id = source.Id,
            ProcessFailCount = source.ProcessFailCount,
            PublicCount = source.PublicCount,
            ProtectedCount = source.ProtectedCount,
            End = source.End,
            Start = source.Start,
            Status = source.Status,
            ProvidersInfo = source.ProvidersInfo?.Select(MapMongoDbProcessProvider)
        };
    }

    private static WebDtos.MongoDbProcessInfoDto.Provider MapMongoDbProcessProvider(SOS.Api.Contracts.Status.Responses.MongoDbProcessInfoDto.Provider source)
    {
        return new WebDtos.MongoDbProcessInfoDto.Provider
        {
            DataProviderId = source.DataProviderId,
            DataProviderIdentifier = source.DataProviderIdentifier,
            ProcessEnd = source.ProcessEnd,
            ProcessStart = source.ProcessStart,
            ProcessStatus = source.ProcessStatus,
            ProcessFailCount = source.ProcessFailCount,
            PublicProcessCount = source.PublicProcessCount,
            ProtectedProcessCount = source.ProtectedProcessCount,
            HarvestEnd = source.HarvestEnd,
            HarvestStart = source.HarvestStart,
            HarvestNotes = source.HarvestNotes,
            HarvestStatus = source.HarvestStatus,
            HarvestCount = source.HarvestCount,
            LatestIncrementalEnd = source.LatestIncrementalEnd,
            LatestIncrementalStart = source.LatestIncrementalStart,
            LatestIncrementalStatus = source.LatestIncrementalStatus,
            LatestIncrementalPublicCount = source.LatestIncrementalPublicCount,
            LatestIncrementalProtectedCount = source.LatestIncrementalProtectedCount,
            DataProviderType = source.DataProviderType
        };
    }

    private static WebDtos.DataProviderStatusDto MapDataProviderStatus(SOS.Api.Contracts.Status.Responses.DataProviderStatusDto source)
    {
        return new WebDtos.DataProviderStatusDto
        {
            Id = source.Id,
            Identifier = source.Identifier,
            Name = source.Name,
            SwedishName = source.SwedishName,
            PublicActive = source.PublicActive,
            PublicInactive = source.PublicInactive,
            PublicDiff = source.PublicDiff,
            ProtectedActive = source.ProtectedActive,
            ProtectedInactive = source.ProtectedInactive,
            ProtectedDiff = source.ProtectedDiff,
            InvalidActive = source.InvalidActive,
            InvalidInactive = source.InvalidInactive,
            InvalidDiff = source.InvalidDiff,
            HarvestTimeActive = source.HarvestTimeActive,
            HarvestTimeInactive = source.HarvestTimeInactive,
            HarvestTimeDiff = source.HarvestTimeDiff,
            ProcessTimeActive = source.ProcessTimeActive,
            ProcessTimeInactive = source.ProcessTimeInactive,
            ProcessTimeDiff = source.ProcessTimeDiff,
            HarvestStatusActive = source.HarvestStatusActive,
            HarvestStatusInactive = source.HarvestStatusInactive,
            LatestIncrementalPublicCount = source.LatestIncrementalPublicCount,
            LatestIncrementalProtectedCount = source.LatestIncrementalProtectedCount,
            LatestIncrementalEnd = source.LatestIncrementalEnd,
            LatestIncrementalTime = source.LatestIncrementalTime
        };
    }

    private static WebModels.DataProviderStatusRow MapDataProviderStatusRow(SOS.Api.Contracts.Status.Responses.DataProviderStatusRow source)
    {
        return new WebModels.DataProviderStatusRow
        {
            Datasource = source.Datasource,
            DataProviderId = source.DataProviderId,
            PublicActive = source.PublicActive,
            PublicInactive = source.PublicInactive,
            PublicDiff = source.PublicDiff,
            ProtectedActive = source.ProtectedActive,
            ProtectedInactive = source.ProtectedInactive,
            ProtectedDiff = source.ProtectedDiff,
            InvalidActive = source.InvalidActive,
            InvalidInactive = source.InvalidInactive,
            InvalidDiff = source.InvalidDiff
        };
    }

    private static WebModels.HealthReportDto MapHealthReport(SOS.Api.Contracts.Status.Responses.HealthReportDto source)
    {
        return new WebModels.HealthReportDto
        {
            Status = source.Status,
            TotalDuration = source.TotalDuration,
            Entries = source.Entries?.ToDictionary(
                pair => pair.Key,
                pair => MapHealthReportEntry(pair.Value))
        };
    }

    private static WebModels.HealthReportEntryDto MapHealthReportEntry(SOS.Api.Contracts.Status.Responses.HealthReportEntryDto source)
    {
        return new WebModels.HealthReportEntryDto
        {
            Status = source.Status,
            Description = source.Description,
            Duration = source.Duration,
            Tags = source.Tags?.ToList(),
            Data = source.Data
        };
    }
}
