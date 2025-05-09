﻿using Elastic.Clients.Elasticsearch;
using Newtonsoft.Json;
using SOS.Lib.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;


namespace SOS.Administration.Gui.Controllers
{
    public class PerformanceData
    {
        public class Request
        {
            public string RequestName { get; set; }
            public DateTime Timestamp { get; set; }
            public double TimeTakenMs { get; set; }
            public long? EventCount { get; set; }
        }
        public List<Request[]> Requests { get; set; }
    }
    public class FailedData
    {
        public string Name { get; set; }
        public long Count { get; set; }
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
    public class ApplicationsInsightsTelemetryReturn
    {
        public class ApplicationInsightsValue
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public string Interval { get; set; }
            public class Segment
            {
                public DateTime Start { get; set; }
                public DateTime End { get; set; }
                public class RequestSegment
                {
                    [JsonProperty("request/name")]
                    public string RequestName { get; set; }
                    public class RequestsDuration
                    {
                        public double Avg { get; set; }
                    }
                    [JsonProperty("requests/duration")]
                    public RequestsDuration RequestDuration { get; set; }
                }
                public IEnumerable<RequestSegment> Segments { get; set; }
            }
            public IEnumerable<Segment> Segments { get; set; }
        }
        public ApplicationInsightsValue Value { get; set; }
    }
    public class LoadTestSummary
    {
        public class LoadTestMetrics
        {
            public class LoadTestChecks
            {
                public int Fails { get; set; }
                public int Passes { get; set; }
                public int Value { get; set; }
            }
            public class LoadTestIterations
            {
                public int Count { get; set; }
                public double Rate { get; set; }
            }
            public class LoadTestIterationDuration
            {
                public double Avg { get; set; }
                public double Med { get; set; }
                public double Max { get; set; }
                public double Min { get; set; }
            }
            public LoadTestChecks Checks { get; set; }
            public LoadTestIterations Iterations { get; set; }
            public LoadTestIterationDuration Iteration_duration { get; set; }
        }
        public LoadTestMetrics Metrics { get; set; }
        public DateTime Timestamp { get; set; }
    }

    [Route("[controller]")]
    [ApiController]
    public class PerformanceController : ControllerBase
    {

        private readonly ElasticsearchClient _testElasticClient;
        private const string aiTelemetryURL = "https://api.applicationinsights.io/v1/apps/{0}/{1}/{2}?{3}";
        private const string aiQueryURL = "https://api.applicationinsights.io/v1/apps/{0}/{1}?{2}";
        private ApplicationInsightsConfiguration _aiConfig;


        public PerformanceController(ApplicationInsightsConfiguration aiConfig, TestElasticSearchConfiguration testElasticConfiguration)
        {
            _testElasticClient = testElasticConfiguration.GetClients().FirstOrDefault();
            _aiConfig = aiConfig;
        }

        public static string GetTelemetry(string appid, string apikey,
                string queryType, string parameterString)
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
        [HttpGet]
        [Route("failed")]
        public IEnumerable<FailedData> GetFailedRequests()
        {
            var query = @"requests
                            | where success == false and resultCode != 404
                            | summarize failedCount = sum(itemCount) by name
                            | top 10 by failedCount desc";
            var json = GetTelemetry(_aiConfig.ApplicationId, _aiConfig.ApiKey, "query", "timespan=P1D&query=" + query);

            var failedRequests = new List<FailedData>();

            if (json.StartsWith('{') && json.EndsWith('}'))
            {
                var result = JsonConvert.DeserializeObject<ApplicationInsightsQueryReturn>(json);

                foreach (var row in result.Tables[0].Rows)
                {
                    var failedRequest = new FailedData()
                    {
                        Name = (string)row[0],
                        Count = (long)row[1]
                    };
                    failedRequests.Add(failedRequest);
                }
            }

            return failedRequests;
        }

        [HttpGet]
        [Route("loadtestsummary")]
        public async Task<IEnumerable<LoadTestSummary>> GetLoadTestSummary()
        {
            var result = await _testElasticClient.SearchAsync<LoadTestSummary>(p => p
                .Index("sos-st-loadtests-summaries")
                .Size(5)
                .Sort(f => f
                    .Field("timestamp".ToField(), SortOrder.Desc )
                )
            );
            if (result.IsValidResponse)
            {
                return result.Documents;
            }
            else
            {
                return new List<LoadTestSummary>();
            }
        }
        [HttpGet]
        [Route("")]
        public PerformanceData GetPerformanceData(string interval, string timespan)
        {
            try
            {
                if (string.IsNullOrEmpty(interval))
                {
                    interval = "PT30M";
                }
                if (string.IsNullOrEmpty(timespan))
                {
                    timespan = "PT12H";
                }

                var json = GetTelemetry(_aiConfig.ApplicationId, _aiConfig.ApiKey, "metrics/requests/duration", $"interval={interval}&timespan={timespan}&segment=request/name&top={1000}");
                var ret = JsonConvert.DeserializeObject<ApplicationsInsightsTelemetryReturn>(json);
                Dictionary<string, List<PerformanceData.Request>> requestDictionary = new Dictionary<string, List<PerformanceData.Request>>();
                foreach (var segment in ret.Value.Segments)
                {
                    var startDate = segment.Start;
                    var endDate = segment.End;
                    if (segment.Segments != null)
                    {
                        foreach (var innerSeg in segment.Segments)
                        {
                            if (!requestDictionary.ContainsKey(innerSeg.RequestName))
                            {
                                requestDictionary.Add(innerSeg.RequestName, new List<PerformanceData.Request>());
                            }
                            requestDictionary[innerSeg.RequestName].Add(new PerformanceData.Request()
                            {
                                RequestName = innerSeg.RequestName,
                                Timestamp = startDate,
                                TimeTakenMs = innerSeg.RequestDuration.Avg
                            });
                        }
                    }
                }
                var requests = new List<PerformanceData.Request[]>();
                foreach (var value in requestDictionary.Values)
                {
                    requests.Add(value.OrderBy(p => p.Timestamp).ToArray());
                }
                return new PerformanceData()
                {
                    Requests = requests
                };
            }
            catch (Exception)
            {
                return null;
            }

        }
    }
}
