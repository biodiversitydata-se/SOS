using SOS.Status.Web.Client.Dtos;
using SOS.Status.Web.Client.Models;
using SOS.Api.Contracts.Status.Responses;

namespace SOS.Status.Web.Client.Abstractions;

public interface IStatusInfoService
{
    Task<List<SOS.Status.Web.Client.Models.DataProviderStatusRow>?> GetDataProviderStatusRowsAsync();

    Task<ProcessSummaryViewModel?> GetProcessSummaryAsync();

    Task<IEnumerable<MongoDbProcessInfoDto>?> GetProcessInfoAsync();

    Task<ProcessInfoViewModel?> GetProcessInfoAsync(bool active);

    Task<List<DataProviderStatusViewModel>?> GetDataProviderStatusAsync();

    Task<HealthReportDto?> GetObservationsApiHealthAsync();

    Task<HealthReportDto?> GetAnalysisApiHealthAsync();

    Task<HealthReportDto?> GetElasticsearchProxyHealthAsync();

    Task<HealthReportDto?> GetDataStewardshipApiHealthAsync();

    Task<string> GetOutboundIpAsync(CancellationToken cancellationToken = default);
}
