using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Managers;
using SOS.Lib.Models.Shared;
using SOS.Lib.Services;

namespace SOS.Lib.Extensions
{
    public static class UsageStatisticsExtensions
    {
        public static ApiUsageStatistics ToApiUsageStatistics(
            this ApplicationInsightsService.ApiUsageStatisticsRow apiUsageStatisticsRow)
        {
            return new ApiUsageStatistics
            {
                AverageDuration = apiUsageStatisticsRow.AverageDuration,
                Date = apiUsageStatisticsRow.Date,
                Endpoint = apiUsageStatisticsRow.Endpoint,
                FailureCount = apiUsageStatisticsRow.FailureCount,
                Method = apiUsageStatisticsRow.Method,
                RequestCount = apiUsageStatisticsRow.RequestCount
            };
        }

    }
}
