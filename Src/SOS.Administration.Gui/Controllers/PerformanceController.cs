using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using SOS.Administration.Gui.Models;
using SOS.Lib.Configuration.Shared;

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
    public class ApplicationsInsightsReturn
    {
        public class AiValue
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
        public AiValue Value { get; set; }
    }
    [Route("[controller]")]
    [ApiController]
    public class PerformanceController : ControllerBase
    {
        private readonly ElasticClient _elasticClient;
        private readonly string _indexName;
        private const string aiURL = "https://api.applicationinsights.io/v1/apps/{0}/{1}/{2}?{3}";
        private ApplicationInsightsConfiguration _aiConfig;


        public PerformanceController(IOptionsMonitor<ElasticSearchConfiguration> elasticConfiguration, IOptionsMonitor<ApplicationInsightsConfiguration> aiConfig)
        {
            _elasticClient = elasticConfiguration.CurrentValue.GetClient();
            _indexName = elasticConfiguration.CurrentValue.IndexPrefix + "-performance-data";
            _aiConfig = aiConfig.CurrentValue;
        }
     
        public static string GetTelemetry(string appid, string apikey,
                string queryType, string queryPath, string parameterString)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("x-api-key", apikey);
            var req = string.Format(aiURL, appid, queryType, queryPath, parameterString);
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
        [Route("")]
        public async Task<PerformanceData> GetPerformanceData(string interval, string timespan)
        {
            if(string.IsNullOrEmpty(interval))
            {
                interval = "PT30M";
            }
            if (string.IsNullOrEmpty(interval))
            {
                interval = "PT12H";
            }

            var json = GetTelemetry(_aiConfig.ApplicationId, _aiConfig.ApiKey,"metrics", "requests/duration", $"interval={interval}&timespan={timespan}&segment=request/name&top={1000}");
            var ret = JsonConvert.DeserializeObject<ApplicationsInsightsReturn>(json);
            Dictionary<string, List<PerformanceData.Request>> requestDictionary = new Dictionary<string, List<PerformanceData.Request>>();
            foreach(var segment in ret.Value.Segments)
            {
                var startDate = segment.Start;
                var endDate = segment.End;
                if (segment.Segments != null)
                {
                    foreach(var innerSeg in segment.Segments)
                    {
                        if (!requestDictionary.ContainsKey(innerSeg.RequestName))
                        {
                            requestDictionary[innerSeg.RequestName] = new List<PerformanceData.Request>();
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
                requests.Add(value.OrderBy(p=>p.Timestamp).ToArray());
            }
            return new PerformanceData()
            {
                Requests = requests
            };
        }
    }
}
