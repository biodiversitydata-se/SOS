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
    public class LogEntry
    {
        [Text(Name = "log.level")]
        public string Level { get; set; }
        public string Message { get; set; }
        [Date(Name = "@timestamp")]
        public DateTime Timestamp { get; set; }
        public LogProcess Process { get; set; }
        public LogHost Host { get; set; }
    }
    public class LogProcess
    {
        public string Name { get; set; }
    }
    public class LogHost
    {
        public string Name { get; set; }
    }
    public class LogEntriesDto
    {
        public IEnumerable<LogEntryDto> LogEntries { get; set; }
        public IEnumerable<TermAggregationDto> Aggregations { get; set; }
    }
    public class TermDto
    {
        public string Name { get; set; }
        public long? DocCount { get; set; }
    }
    public class TermAggregationDto
    {
        public string Name { get; set; }
        public IEnumerable<TermDto> Terms { get; set; }
    }
    public class LogEntryDto
    {
        public string Level { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string ProcessName { get; set; }
        public string HostName { get; set; }
    }
    [Route("[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {

        private readonly ElasticClient _testElasticClient;
        private readonly string _indexName = "logs-*";


        public LogsController( IOptionsMonitor<ApplicationInsightsConfiguration> aiConfig, IOptionsMonitor<TestElasticSearchConfiguration> testElasticConfiguration)
        {          
            _testElasticClient = testElasticConfiguration.CurrentValue.GetClient(true);
        }
     
        [HttpGet]
        [Route("latest")]
        public async Task<LogEntriesDto> GetLatestLogs(string filters = "", int skip = 0, int take = 100)
        {
            IEnumerable<string> filterLevels = null;
            IEnumerable<string> filterHosts = null;
            IEnumerable<string> filterProcesses = null;
            if (filters?.Length > 0)
            {
                filterLevels = filters.Split(',').Where(p => p.Length > 0).Where(p=>p.StartsWith("levels")).Select(s => s.Substring("levels_".Length));
                filterHosts = filters.Split(',').Where(p => p.Length > 0).Where(p => p.StartsWith("hosts")).Select(s => s.Substring("hosts_".Length));
                filterProcesses = filters.Split(',').Where(p => p.Length > 0).Where(p => p.StartsWith("processes")).Select(s=>s.Substring("processes_".Length));
            }
            var result = await _testElasticClient.SearchAsync<LogEntry>(p => p
                .Index(_indexName)
                .Size(take)
                .Skip(skip)
                .Query(q=>q
                    .Bool(b=>b
                        .Filter(f=>f.Terms(t=>t.Field("log.level.keyword").Terms(filterLevels)),
                                f=>f.Terms(t => t.Field("host.name.keyword").Terms(filterHosts)),
                                f=>f.Terms(t => t.Field("process.name.keyword").Terms(filterProcesses)))
                        ))
                .Aggregations(a => a.Global("global", g => g.Aggregations(aa => aa
                    .Terms("levels", tt => tt.Field("log.level.keyword"))
                    .Terms("hosts", tt => tt.Field("host.name.keyword"))
                    .Terms("processes", tt => tt.Field("process.name.keyword")))))
                .Sort(f => f
                    .Descending(d=>d.Timestamp))
                );
            if (result.IsValid) 
            {
                var logEntriesDto = new LogEntriesDto();
                var resultsDto = new List<LogEntryDto>();
                foreach(var d in result.Documents)
                {
                    resultsDto.Add(new LogEntryDto()
                    {
                        Level = d.Level,
                        Message = d.Message,
                        Timestamp = d.Timestamp,
                        HostName = d.Host.Name,
                        ProcessName = d.Process.Name
                    });
                }
                logEntriesDto.LogEntries = resultsDto;
                var aggregationsDto = new List<TermAggregationDto>(); 
                foreach(var a in result.Aggregations.Global("global"))
                {
                    var agg = result.Aggregations.Global("global").Terms(a.Key);
                    var terms = new List<TermDto>();

                    foreach(var bucket in agg.Buckets)
                    {
                        terms.Add(new TermDto() { Name = bucket.Key, DocCount = bucket.DocCount });
                    }
                    var aggDto = new TermAggregationDto() { Name = a.Key, Terms = terms };
                    aggregationsDto.Add(aggDto);
                }
                logEntriesDto.Aggregations = aggregationsDto;
                
                return logEntriesDto;
            }
            else
            {
                return new LogEntriesDto() { LogEntries = new List<LogEntryDto>(), Aggregations = new List<TermAggregationDto>() };
            }
        }
    }
}
