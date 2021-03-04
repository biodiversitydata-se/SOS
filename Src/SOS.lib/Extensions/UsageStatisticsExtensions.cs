using SOS.Lib.Models.ApplicationInsights;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Extensions
{
    public static class UsageStatisticsExtensions
    {
        public static ApiUsageStatistics ToApiUsageStatistics(
            this ApiUsageStatisticsRow apiUsageStatisticsRow)
        {
            return new ApiUsageStatistics
            {
                AccountId = apiUsageStatisticsRow.AccountId,
                AverageDuration = apiUsageStatisticsRow.AverageDuration,
                Date = apiUsageStatisticsRow.Date,
                Endpoint = apiUsageStatisticsRow.Endpoint,
                FailureCount = apiUsageStatisticsRow.FailureCount,
                Method = apiUsageStatisticsRow.Method,
                RequestCount = apiUsageStatisticsRow.RequestCount,
                UserId = apiUsageStatisticsRow.UserId
            };
        }

    }
}
