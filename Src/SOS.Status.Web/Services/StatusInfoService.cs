using SOS.Shared.Api.Dtos.Status;
using SOS.Status.Web.Client.Abstractions;
using SOS.Status.Web.Client.Models;
using SOS.Status.Web.HttpClients;
using System.Globalization;

namespace SOS.Status.Web.Services;

public class StatusInfoService : IStatusInfoService
{
    private readonly SosObservationsApiClient _observationsApiClient;
    private readonly SosAnalysisApiClient _analysisApiClient;
    private readonly SosElasticsearchProxyClient _elasticsearchProxyClient;
    private readonly SosDataStewardshipApiClient _dataStewardshipApiClient;

    public StatusInfoService(
        SosObservationsApiClient observationsApiClient,
        SosAnalysisApiClient analysisApiClient,
        SosElasticsearchProxyClient sosElasticsearchProxyClient,
        SosDataStewardshipApiClient sosDataStewardshipApiClient)
    {
        _observationsApiClient = observationsApiClient ?? throw new ArgumentNullException(nameof(observationsApiClient));
        _analysisApiClient = analysisApiClient ?? throw new ArgumentNullException(nameof(analysisApiClient));
        _elasticsearchProxyClient = sosElasticsearchProxyClient ?? throw new ArgumentNullException(nameof(sosElasticsearchProxyClient));
        _dataStewardshipApiClient = sosDataStewardshipApiClient ?? throw new ArgumentNullException(nameof(sosDataStewardshipApiClient));
    }

    public async Task<Client.Dtos.ProcessSummaryDto?> GetProcessSummaryAsync()
    {
        var processSummary = await _observationsApiClient.GetProcessSummary();
        return processSummary!;
    }

    public async Task<Client.Dtos.ProcessInfoDto?> GetProcessInfoAsync(bool active)
    {
        var processInfo = await _observationsApiClient.GetProcessInfo(active);
        return processInfo!;
    }

    public async Task<IEnumerable<Client.Dtos.MongoDbProcessInfoDto>?> GetProcessInfoAsync()
    {
        var processInfo = await _observationsApiClient.GetProcessInfo();
        return processInfo!;
    }

    public async Task<List<Client.Dtos.DataProviderStatusDto>?> GetDataProviderStatusAsync()
    {
        var dataProviderStatus = await _observationsApiClient.GetDataProviderStatus();
        return dataProviderStatus!;
    }

    public async Task<List<DataProviderStatusRow>?> GetDataProviderStatusRowsAsync()
    {
        var activeInstance = await _observationsApiClient.GetActiveInstance();
        var processInfos = await _observationsApiClient.GetProcessInfo();
        var dataStatusRows = BuildDataStatusRows(processInfos, activeInstance);
        return dataStatusRows;
    }

    public async Task<HealthReportDto?> GetObservationsApiHealthAsync()
    {
        var healthReport = await _observationsApiClient.GetHealthAsync();
        healthReport!.Entries.Remove("self");

        var mongoDbInfo = await _observationsApiClient.GetMongoDatabaseInfo();
        var searchIndexInfo = await _observationsApiClient.GetSearchIndexInfo();

        if (mongoDbInfo != null)
        {
            double diskUsed = double.Parse(mongoDbInfo.DiskUsed, NumberStyles.Any, CultureInfo.InvariantCulture);
            double diskTotal = double.Parse(mongoDbInfo.DiskTotal, NumberStyles.Any, CultureInfo.InvariantCulture);
            healthReport.Entries["MongoDB disk"] = new HealthReportEntryDto
            {
                Status = "Healthy",
                Description = $"Disk used: {(int)Math.Round(diskUsed / diskTotal * 100)}%"
            };
        }

        if (searchIndexInfo != null)
        {
            double diskUsed = double.Parse(searchIndexInfo.Allocations.First().DiskUsed, NumberStyles.Any, CultureInfo.InvariantCulture);
            double diskTotal = double.Parse(searchIndexInfo.Allocations.First().DiskAvailable, NumberStyles.Any, CultureInfo.InvariantCulture) + diskUsed;
            healthReport.Entries["Elasticsearch disk"] = new HealthReportEntryDto
            {
                Status = "Healthy",
                Description = $"Disk used: {(int)Math.Round(diskUsed / diskTotal * 100)}%"
            };
        }

        return healthReport!;
    }

    public async Task<HealthReportDto?> GetAnalysisApiHealthAsync()
    {
        var healthReport = await _analysisApiClient.GetHealthAsync();
        healthReport!.Entries.Remove("self");
        return healthReport;
    }

    public async Task<HealthReportDto?> GetElasticsearchProxyHealthAsync()
    {
        var healthReport = await _elasticsearchProxyClient.GetHealthAsync();
        healthReport!.Entries.Remove("self");
        return healthReport;
    }

    public async Task<HealthReportDto?> GetDataStewardshipApiHealthAsync()
    {
        var healthReport = await _dataStewardshipApiClient.GetHealthAsync();
        healthReport!.Entries.Remove("self");
        return healthReport;
    }

    private List<DataProviderStatusRow> BuildDataStatusRows(IEnumerable<Client.Dtos.MongoDbProcessInfoDto>? processInfos, ActiveInstanceInfoDto? activeInstanceInfo)
    {
        if (processInfos == null || activeInstanceInfo == null)
            return new List<DataProviderStatusRow>();

        var activeInfos = processInfos.FirstOrDefault(m => int.Parse(m.Id.Last().ToString()) == (int)(activeInstanceInfo.ActiveInstance));
        var inactiveInfos = processInfos.FirstOrDefault(m => int.Parse(m.Id.Last().ToString()) != (int)(activeInstanceInfo.ActiveInstance));
        if (activeInfos == null || inactiveInfos == null)
            return new List<DataProviderStatusRow>();

        var rows = new List<DataProviderStatusRow>();
        var inactiveProvidersById = inactiveInfos.ProvidersInfo
            .ToDictionary(m => m.DataProviderId!.Value, m => m);

        foreach (var activeProvider in activeInfos.ProvidersInfo.OrderBy(m => m.DataProviderId))
        {
            var inactiveProvider = inactiveProvidersById!.GetValueOrDefault(activeProvider.DataProviderId!.Value, null);

            var row = new DataProviderStatusRow
            {
                Datasource = activeProvider.DataProviderIdentifier,
                DataProviderId = activeProvider.DataProviderId ?? 0,
                PublicActive = activeProvider?.PublicProcessCount ?? 0,
                PublicInactive = inactiveProvider?.PublicProcessCount ?? 0,
                PublicDiff = (activeProvider?.PublicProcessCount ?? 0) - (inactiveProvider?.PublicProcessCount ?? 0),
                ProtectedActive = activeProvider?.ProtectedProcessCount ?? 0,
                ProtectedInactive = inactiveProvider?.ProtectedProcessCount ?? 0,
                ProtectedDiff = (activeProvider?.ProtectedProcessCount ?? 0) - (inactiveProvider?.ProtectedProcessCount ?? 0),
                // Note: Invalid counts are not available in the ProcessInfoDto, keeping them as 0
                InvalidActive = activeProvider?.ProcessFailCount.GetValueOrDefault(0) ?? 0,
                InvalidInactive = inactiveProvider?.ProcessFailCount.GetValueOrDefault(0) ?? 0,
                InvalidDiff = (activeProvider?.ProcessFailCount.GetValueOrDefault(0) ?? 0) - (inactiveProvider?.ProcessFailCount.GetValueOrDefault(0) ?? 0)
            };

            rows.Add(row);
        }

        return rows.OrderBy(r => r.DataProviderId).ToList();
    }
}
