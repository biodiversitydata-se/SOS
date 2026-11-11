using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SOS.Lib.Managers.ApiUsageStatisticsManager;

namespace SOS.Lib.Managers.Interfaces;

public interface IApiUsageStatisticsManager
{
    Task<bool> HarvestStatisticsAsync();
    Task<bool> CreateExcelFileReportAsync(string reportId, string createdBy);
    /// <summary>Create an Excel report with optional taxon list usage and inclusive date bounds.</summary>
    Task<bool> CreateExcelFileReportAsync(string reportId, string createdBy, bool includeTaxonListUsage,
        DateTime? fromDate = null, DateTime? toDate = null);
    Task<Dictionary<string, RequestStatistics>> CalculateRequestStatisticsAsync(DateTime fromDate, DateTime toDate);
    Task<byte[]> CreateRequestStatisticsSummaryExcelFileAsync(DateTime fromDate, DateTime toDate);
}
