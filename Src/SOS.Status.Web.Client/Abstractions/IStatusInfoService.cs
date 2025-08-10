using SOS.Status.Web.Client.Dtos;
using SOS.Status.Web.Client.Models;

namespace SOS.Status.Web.Client.Abstractions;

public interface IStatusInfoService
{
    Task<List<DataProviderStatusRow>?> GetDataProviderStatusRowsAsync();

    Task<ProcessSummaryDto?> GetProcessSummaryAsync();

    Task<IEnumerable<MongoDbProcessInfoDto>?> GetProcessInfoAsync();

    Task<ProcessInfoDto?> GetProcessInfoAsync(bool active);

    Task<List<DataProviderStatusDto>?> GetDataProviderStatusAsync();

    Task<HealthReportDto?> GetObservationsApiHealthAsync();

    Task<HealthReportDto?> GetAnalysisApiHealthAsync();

    Task<HealthReportDto?> GetElasticsearchProxyHealthAsync();

    Task<HealthReportDto?> GetDataStewardshipApiHealthAsync();
}
