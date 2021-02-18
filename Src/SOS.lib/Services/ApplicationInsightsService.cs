using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Services
{
    public class ApplicationInsightsService : Interfaces.IApplicationInsightsService
    {
        private readonly ApplicationInsightsConfiguration _aiConfig;
        private const string aiTelemetryURL = "https://api.applicationinsights.io/v1/apps/{0}/{1}/{2}?{3}";
        private const string aiQueryURL = "https://api.applicationinsights.io/v1/apps/{0}/{1}?{2}";

        public ApplicationInsightsService(ApplicationInsightsConfiguration applicationInsightsConfiguration)
        {
            _aiConfig = applicationInsightsConfiguration ?? throw new ArgumentNullException(nameof(applicationInsightsConfiguration));
        }

        public string GetTelemetry(
            string appid,
            string apikey,
            string queryType,
            string parameterString)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("x-api-key", apikey);
            var req = string.Format(aiQueryURL, appid, queryType, parameterString);
            HttpResponseMessage response = client.GetAsync(req).Result;
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                return response.ReasonPhrase;
            }
        }

        private string GetUsageStatisticsQueryForspecificDay(DateTime date)
        {
            var queryTemplate = @"requests 
                | where timestamp >= datetime(""{0} 00:00:00.0"") and 
                    timestamp <= datetime(""{0} 23:59:59.9"")
                | project  dateTime = datetime(""{0} 00:00:00.0""),            
                    method = operation_Name,
                    endpoint = substring(tostring(parseurl(url).Path), 1),                        
                    duration,
                    failures = success
                | summarize requestCount = tostring(count()),
                    failureCount = tostring(count(failures == false)),
                    averageDuration = tostring(toint(avg(duration)))
                by dateTime, method, endpoint";

            return string.Format(queryTemplate, date.ToString("yyyy-MM-dd"));
        }

        public async Task<List<ApiUsageStatisticsRow>> GetUsageStatisticsForSpecificDay(DateTime date)
        {
            var query = GetUsageStatisticsQueryForspecificDay(date);
            var json = GetTelemetry(_aiConfig.ApplicationId, _aiConfig.ApiKey, "query", "query=" + query);
            var result = JsonConvert.DeserializeObject<ApplicationInsightsQueryReturn>(json);
            var apiUsageStatisticsRows = new List<ApiUsageStatisticsRow>();
            foreach (var row in result.Tables[0].Rows)
            {
                var usageStatisticsRow = new ApiUsageStatisticsRow()
                {
                    Date = (DateTime)row[0],
                    Method = (string)row[1],
                    Endpoint = (string)row[2],
                    RequestCount = long.Parse((string)row[3]),
                    FailureCount = long.Parse((string)row[4]),
                    AverageDuration = long.Parse((string)row[5])
                };

                apiUsageStatisticsRows.Add(usageStatisticsRow);
            }

            return apiUsageStatisticsRows;
        }

        public class ApplicationInsightsQueryReturn
        {
            public class Table
            {
                public string Name { get; set; }
                public IList<IList<object>> Rows { get; set; }
            }
            public Table[] Tables { get; set; }
        }

        public class ApiUsageStatisticsRow
        {
            public DateTime Date { get; set; }
            public string Method { get; set; }
            public string Endpoint { get; set; }
            public long RequestCount { get; set; }
            public long FailureCount { get; set; }
            public long AverageDuration { get; set; }
        }
    }
}