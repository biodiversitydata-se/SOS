using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.ApplicationInsights;

namespace SOS.Lib.Services.Interfaces
{
    public interface IApplicationInsightsService
    {
        /// <summary>
        /// Get usage statistics 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        Task<IEnumerable<ApiUsageStatisticsRow>> GetUsageStatisticsForSpecificDayAsync(DateTime date);
    }
}