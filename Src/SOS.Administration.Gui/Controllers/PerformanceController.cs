using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nest;
using SOS.Administration.Gui.Models;
using SOS.Lib.Configuration.Shared;

namespace SOS.Administration.Gui.Controllers
{
    public class PerformanceData
    {
        public DateTime Timestamp { get; set; }
        public double? TimeTakenMs { get; set; }
        public long? EventCount { get; set; }
    }
    [Route("[controller]")]
    [ApiController]
    public class PerformanceController : ControllerBase
    {
        private readonly ElasticClient _elasticClient;
        private readonly string _indexName;

        public PerformanceController(IOptionsMonitor<ElasticSearchConfiguration> elasticConfiguration)
        {
            _elasticClient = elasticConfiguration.CurrentValue.GetClient();
            _indexName = elasticConfiguration.CurrentValue.IndexPrefix + "-performance-data";
        }
        [HttpGet]
        [Route("{testId}")]
        public async Task<IEnumerable<PerformanceData>> GetPerformanceData(int testId, DateTime? startTime, DateTime? endTime)
        {
            DateTime startDateTime = DateTime.Now.AddHours(-6);
            DateTime endDateTime = DateTime.Now;
            if (startTime.HasValue)
            {
                startDateTime = startTime.Value;
            }
            if (endTime.HasValue)
            {
                endDateTime = endTime.Value;
            }
            var interval = TimeSpan.FromMinutes(5);
            var timespan = endDateTime.Subtract(startDateTime);
            if (timespan.Days > 0)
            {
                interval = TimeSpan.FromMinutes(60);
            }           
            var result = await _elasticClient.SearchAsync<PerformanceResult>(p => p.
                Index(_indexName).
                Query(q =>q
                        .Bool(b=>b.Filter(
                            bs=>bs.Term(t=>t.Field("testId").Value(testId)), 
                            bs=> bs.DateRange(range => range.Field("timestamp").GreaterThanOrEquals(startDateTime).LessThanOrEquals(endDateTime))))).
                Aggregations(agg => agg.
                                DateHistogram("date_histo",
                                    e=>e.
                                        Field("timestamp").
                                        FixedInterval(new Time(interval)).
                                        Aggregations(agg2=>agg2.
                                            Average("the_avg", su=>su.
                                                Field("timeTakenMs"))
                                            ))));
            var histogram = result.Aggregations.DateHistogram("date_histo");
            var data = new List<PerformanceData>();
            foreach(var item in histogram.Buckets)
            {
                var performanceData = new PerformanceData()
                {
                    Timestamp = item.Date,
                    EventCount = item.DocCount
                };
                if (item.ContainsKey("the_avg"))
                {
                    var nested = (ValueAggregate)item["the_avg"];
                    performanceData.TimeTakenMs = nested.Value;
                }
                data.Add(performanceData);
            }
            return data;
        }
    }
}
