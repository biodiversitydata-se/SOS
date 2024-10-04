using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SOS.Lib.Managers.ApiUsageStatisticsManager;

namespace SOS.Lib.Managers.Interfaces
{
    public interface IApiUsageStatisticsManager
    {
        Task<bool> HarvestStatisticsAsync();
        Task<bool> CreateExcelFileReportAsync(string reportId, string createdBy);
        Task<Dictionary<string, RequestStatistics>> CalculateRequestStatisticsAsync(DateTime fromDate, DateTime toDate);
        Task<byte[]> CreateRequestStatisticsSummaryExcelFileAsync(DateTime fromDate, DateTime toDate);
    }
}
