using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Services.Interfaces
{
    public interface IApplicationInsightsService
    {
        Task<List<ApplicationInsightsService.ApiUsageStatisticsRow>> GetUsageStatisticsForSpecificDay(DateTime date);
    }
}