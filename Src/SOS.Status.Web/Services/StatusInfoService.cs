using SOS.Api.Contracts.Metadata.Responses;
using SOS.Api.Client.Abstractions;
using SOS.Status.Web.Client.Abstractions;
using SOS.Status.Web.HttpClients;
using SOS.Api.Contracts.Status.Responses;
using SOS.Status.Web.Client.Dtos;
using SOS.Status.Web.Client.Models;

namespace SOS.Status.Web.Services;

public class StatusInfoService : IStatusInfoService
{
    private readonly ISosStatusInfoService _sosStatusInfoService;
    private readonly ISosSystemsStatusInfoClient _systemsStatusInfoClient;
    private readonly SosAnalysisApiClient _analysisApiClient;
    private readonly SosElasticsearchProxyClient _elasticsearchProxyClient;
    private readonly SosDataStewardshipApiClient _dataStewardshipApiClient;
    private readonly IpifyClient _ipifyClient;
    private readonly ILogger<StatusInfoService> _logger;

    public StatusInfoService(
        ISosStatusInfoService sosStatusInfoService,
        ISosSystemsStatusInfoClient systemsStatusInfoClient,
        SosAnalysisApiClient analysisApiClient,
        SosElasticsearchProxyClient sosElasticsearchProxyClient,
        SosDataStewardshipApiClient sosDataStewardshipApiClient,
        IpifyClient ipifyClient,
        ILogger<StatusInfoService> logger)
    {
        _sosStatusInfoService = sosStatusInfoService ?? throw new ArgumentNullException(nameof(sosStatusInfoService));
        _systemsStatusInfoClient = systemsStatusInfoClient ?? throw new ArgumentNullException(nameof(systemsStatusInfoClient));
        _analysisApiClient = analysisApiClient ?? throw new ArgumentNullException(nameof(analysisApiClient));
        _elasticsearchProxyClient = sosElasticsearchProxyClient ?? throw new ArgumentNullException(nameof(sosElasticsearchProxyClient));
        _dataStewardshipApiClient = sosDataStewardshipApiClient ?? throw new ArgumentNullException(nameof(sosDataStewardshipApiClient));
        _ipifyClient = ipifyClient ?? throw new ArgumentNullException(nameof(ipifyClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProcessSummaryViewModel?> GetProcessSummaryAsync()
    {
        var processSummary = await _systemsStatusInfoClient.GetProcessSummaryAsync();
        return processSummary is null ? null : MapProcessSummary(processSummary);
    }

    public async Task<ProcessInfoViewModel?> GetProcessInfoAsync(bool active)
    {
        var processInfo = await _systemsStatusInfoClient.GetProcessInfoAsync(active);
        return processInfo is null ? null : MapProcessInfo(processInfo);
    }

    public async Task<IEnumerable<MongoDbProcessInfoDto>?> GetProcessInfoAsync()
    {
        var processInfo = await _systemsStatusInfoClient.GetMongoDbProcessInfoAsync();
        return processInfo;
    }

    public async Task<List<DataProviderStatusViewModel>?> GetDataProviderStatusAsync()
    {
        var dataProviderStatus = await _sosStatusInfoService.GetDataProviderStatusAsync();
        return dataProviderStatus?.Select(MapDataProviderStatus).ToList();
    }

    public async Task<List<SOS.Status.Web.Client.Models.DataProviderStatusRow>?> GetDataProviderStatusRowsAsync()
    {
        var rows = await _sosStatusInfoService.GetDataProviderStatusRowsAsync();
        return rows.Select(MapDataProviderStatusRow).ToList();
    }

    public async Task<HealthReportDto?> GetObservationsApiHealthAsync()
    {
        try
        {
            var healthReport = await _sosStatusInfoService.GetObservationsApiHealthAsync();
            return healthReport;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not fetch health from observations API.");
            return null;
        }
    }

    public async Task<HealthReportDto?> GetAnalysisApiHealthAsync()
    {
        var healthReport = await _analysisApiClient.GetHealthAsync();
        healthReport?.Entries?.Remove("self");
        return healthReport;
    }

    public async Task<HealthReportDto?> GetElasticsearchProxyHealthAsync()
    {
        var healthReport = await _elasticsearchProxyClient.GetHealthAsync();
        healthReport?.Entries?.Remove("self");
        return healthReport;
    }

    public async Task<HealthReportDto?> GetDataStewardshipApiHealthAsync()
    {
        var healthReport = await _dataStewardshipApiClient.GetHealthAsync();
        healthReport?.Entries?.Remove("self");
        return healthReport;
    }

    public async Task<string> GetOutboundIpAsync(CancellationToken cancellationToken = default)
    {
        return await _ipifyClient.GetOutboundIpAsync(cancellationToken);
    }

    private static ProcessSummaryViewModel MapProcessSummary(ProcessSummaryDto source)
    {
        return new ProcessSummaryViewModel
        {
            ActiveProcessStatus = MapProcessStatus(source.ActiveProcessStatus),
            InactiveProcessStatus = MapProcessStatus(source.InactiveProcessStatus),
            DataProviderStatuses = source.DataProviderStatuses?.Select(MapDataProviderStatus).ToList()
        };
    }

    private static ProcessStatusDto MapProcessStatus(ProcessStatusDto? source)
    {
        return new ProcessStatusDto
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

    private static ProcessInfoViewModel MapProcessInfo(ProcessInfoDto source)
    {
        return new ProcessInfoViewModel
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

    private static ProcessInfoViewModel.Provider MapProviderInfo(ProviderInfoDto source)
    {
        return new ProcessInfoViewModel.Provider
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

    private static DataProviderStatusViewModel MapDataProviderStatus(DataProviderStatusDto source)
    {
        return new DataProviderStatusViewModel
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

    private static SOS.Status.Web.Client.Models.DataProviderStatusRow MapDataProviderStatusRow(SOS.Api.Contracts.Status.Responses.DataProviderStatusRow source)
    {
        return new SOS.Status.Web.Client.Models.DataProviderStatusRow
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

}
