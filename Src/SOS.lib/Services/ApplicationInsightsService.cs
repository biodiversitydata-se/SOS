using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Models.ApplicationInsights;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Services
{
    public class ApplicationInsightsService : IApplicationInsightsService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly ApplicationInsightsConfiguration _applicationInsightsConfiguration;
        private readonly ILogger<ApplicationInsightsService> _logger;

        /// <summary>
        /// Post a query to application insights
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task<T> QueryApplicationInsightsAsync<T>(
            string query)
        {
            var headerData = new Dictionary<string, string>();
            headerData.Add("Accept", _applicationInsightsConfiguration.AcceptHeaderContentType);
            headerData.Add("x-api-key", _applicationInsightsConfiguration.ApiKey);

            var result = await _httpClientService.PostDataAsync<T>(
                new Uri($"{_applicationInsightsConfiguration.BaseAddress}/apps/{_applicationInsightsConfiguration.ApplicationId }/query"), new { query = query?
                    .Replace("\n", "")
                    .Replace("\r", "")
                    .Replace("\t", "")
                }, 
                headerData);

            return result;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="applicationInsightsConfiguration"></param>
        /// <param name="logger"></param>
        public ApplicationInsightsService(IHttpClientService httpClientService,
            ApplicationInsightsConfiguration applicationInsightsConfiguration,
            ILogger<ApplicationInsightsService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _applicationInsightsConfiguration = applicationInsightsConfiguration ?? throw new ArgumentNullException(nameof(applicationInsightsConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        
        /// <inheritdoc />
        public async Task<IEnumerable<ApiUsageStatisticsRow>> GetUsageStatisticsForSpecificDayAsync(DateTime date)
        {
            var query = $@"requests 
                    | where 
                        timestamp >= datetime('{date.ToString("yyyy-MM-dd")} 00:00:00.0')  
                        and timestamp < datetime('{date.AddDays(1).ToString("yyyy-MM-dd")} 00:00:00.0')
                    | project 
                        timestamp, 
                        method = substring(name, 0, indexof(name, ' ' )), 
                        endpoint = substring(tostring(parseurl(url).Path), 1),  
                        user_AccountId,
                        user_AuthenticatedId, 
                        duration, 
                        success
                    | summarize 
                        requestCount = count(), 
                        failureCount = count(success == false), 
                        averageDuration = toint(avg(duration)) 
                        by 
                            method, 
                            endpoint,
                            user_AccountId,
                            user_AuthenticatedId";

            var result = await QueryApplicationInsightsAsync<ApplicationInsightsQueryResponse>(query);

            return result?.Tables.FirstOrDefault()?.Rows?.Select(r =>
                new ApiUsageStatisticsRow()
                {
                    Date = date,
                    Method = ((JsonElement)r[0]).GetString(),
                    Endpoint = ((JsonElement)r[1]).GetString(),
                    AccountId = ((JsonElement)r[2]).GetString(),
                    UserId = ((JsonElement)r[3]).GetString(),
                    RequestCount = ((JsonElement)r[4]).GetInt64(),
                    FailureCount = ((JsonElement)r[5]).GetInt64(),
                    AverageDuration = ((JsonElement)r[6]).GetInt64()
                });
        }
    }
}
