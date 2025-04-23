using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using SOS.Lib.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Administration.Gui.Controllers
{
    public class LogEntry
    {
        public string Level { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public LogProcess Process { get; set; }
        public LogHost Host { get; set; }
        public LogError Error { get; set; }
    }
    public class LogProcess
    {
        public string Name { get; set; }
    }
    public class LogHost
    {
        public string Name { get; set; }
    }
    public class LogError
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Type { get; set; }
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
        public string ErrorMessage { get; set; }
        public string ErrorStackTrace { get; set; }
    }
    [Route("[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {

        private readonly ElasticsearchClient _elasticClient;
        private readonly string _indexName = "logs-*";


        public LogsController(ElasticSearchConfiguration elasticConfiguration)
        {
            _elasticClient = elasticConfiguration.GetClients().FirstOrDefault();
        }

        [HttpGet]
        [Route("latest")]
        public async Task<LogEntriesDto> GetLatestLogs(string filters = "", string timespan = "", string textFilter = "", int skip = 0, int take = 100)
        {
            IEnumerable<string> filterLevels = null;
            IEnumerable<string> filterHosts = null;
            IEnumerable<string> filterProcesses = null;
            int dateFilterMinutes = GetDateFilterMinutes(timespan);
            if (filters?.Length > 0)
            {
                filterLevels = filters.Split(',').Where(p => p.Length > 0).Where(p => p.StartsWith("Log Levels")).Select(s => s.Substring("Log Levels_".Length));
                filterHosts = filters.Split(',').Where(p => p.Length > 0).Where(p => p.StartsWith("Hosts")).Select(s => s.Substring("Hosts_".Length));
                filterProcesses = filters.Split(',').Where(p => p.Length > 0).Where(p => p.StartsWith("Processes")).Select(s => s.Substring("Processes_".Length));
            }
            var queryString = "";
            if (textFilter?.Length > 0)
            {
                queryString = "*" + textFilter + "*";
            }
            var queries = new List<Action<QueryDescriptor<LogEntry>>>();
            queries.TryAddTermsCriteria("log.level.keyword", filterLevels);
            queries.TryAddTermsCriteria("host.name.keyword", filterHosts);
            queries.TryAddTermsCriteria("process.name.keyword", filterProcesses);
            queries.TryAddDateRangeCriteria("timestamp", DateTime.Now.AddMinutes(-dateFilterMinutes), RangeTypes.GreaterThanOrEquals);

            var filtered_levels_queries = new List<Action<QueryDescriptor<LogEntry>>>();
            filtered_levels_queries.TryAddWildcardCriteria("message", queryString);
            filtered_levels_queries.TryAddTermsCriteria("host.name.keyword", filterHosts);
            filtered_levels_queries.TryAddTermsCriteria("process.name.keyword", filterProcesses);
            filtered_levels_queries.TryAddDateRangeCriteria("timestamp", DateTime.Now.AddMinutes(-dateFilterMinutes), RangeTypes.GreaterThanOrEquals);

            var filtered_hosts_queries = new List<Action<QueryDescriptor<LogEntry>>>();
            filtered_hosts_queries.TryAddWildcardCriteria("message", queryString);
            filtered_hosts_queries.TryAddTermsCriteria("log.level.keyword", filterLevels);
            filtered_hosts_queries.TryAddTermsCriteria("process.name.keyword", filterProcesses);
            filtered_hosts_queries.TryAddDateRangeCriteria("timestamp", DateTime.Now.AddMinutes(-dateFilterMinutes), RangeTypes.GreaterThanOrEquals);

            var filtered_processes_queries = new List<Action<QueryDescriptor<LogEntry>>>();
            filtered_processes_queries.TryAddWildcardCriteria("message", queryString);
            filtered_processes_queries.TryAddTermsCriteria("log.level.keyword", filterLevels);
            filtered_processes_queries.TryAddTermsCriteria("host.name.keyword", filterHosts);
            filtered_processes_queries.TryAddDateRangeCriteria("timestamp", DateTime.Now.AddMinutes(-dateFilterMinutes), RangeTypes.GreaterThanOrEquals);

            var result = await _elasticClient.SearchAsync<LogEntry>(p => p
                .Indices(_indexName)
                .Size(take)
                .From(skip)
                .Query(q => q
                    .Bool(b => b
                        .Must(f => f.Wildcard(m => m.Field(ff => ff.Error.Message).Value(queryString)))
                        .Filter(queries.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("global", a => a
                        .Aggregations(a => a
                            .Add("filtered_levels", a => a
                                .Filter(f => f
                                    .Bool(b => b
                                        .Filter(filtered_levels_queries.ToArray())
                                    )
                                )
                                .Aggregations(fa => fa
                                    .Add("Log Levels", a => a
                                        .Terms(t => t
                                            .Field("log.level.keyword")
                                        )
                                    )
                                )
                            )
                            .Add("filtered_hosts", a => a
                                .Filter(f => f
                                    .Bool(b => b
                                        .Filter(filtered_hosts_queries.ToArray())
                                    )
                                )
                                .Aggregations(fa => fa
                                    .Add("Hosts", a => a
                                        .Terms(t => t
                                            .Field("host.name.keyword")
                                        )
                                    )
                                )
                            )
                            .Add("filtered_processes", a => a
                                .Filter(f => f
                                    .Bool(b => b
                                        .Filter(filtered_processes_queries.ToArray())
                                    )
                                )
                                .Aggregations(fa => fa
                                    .Add("Processes", a => a
                                        .Terms(t => t
                                            .Field("process.name.keyword")
                                        )
                                    )
                                )
                            )
                        )
                    )
                )   
                .Highlight(h => h
                    .Fields(f => f
                        .Add("message" ,f => f
                            .PreTags(["<b style='color:yellow'>"])
                            .PostTags(["</b>"])
                        )
                    )
                )
                .Sort(s => s
                    .Field(f => f.Timestamp, SortOrder.Desc)
                )
            );
            if (result.IsValidResponse && result.Aggregations.Count > 0)
            {
                var logEntriesDto = new LogEntriesDto();
                var resultsDto = new List<LogEntryDto>();
                foreach (var d in result.Documents)
                {
                    resultsDto.Add(new LogEntryDto()
                    {
                        Level = d.Level,
                        Message = d.Message,
                        Timestamp = d.Timestamp,
                        HostName = d.Host.Name,
                        ProcessName = d.Process.Name,
                        ErrorMessage = d?.Error?.Message,
                        ErrorStackTrace = d?.Error?.StackTrace
                    });
                }
                logEntriesDto.LogEntries = resultsDto;
                var aggregationsDto = new List<TermAggregationDto>();

                var filterAggregations= new[] { 
                    (f: "filtered_levels", a: "Log Levels"), 
                    (f: "filtered_hosts", a : "Hosts"), 
                    (f: "filtered_processes", a: "Processes") };
                foreach (var filterAgg in filterAggregations)
                {
                    var a = result.Aggregations.GetGlobal("global").Aggregations.GetFilter(filterAgg.f);
                    var agg = a.Aggregations.GetStringTerms(filterAgg.a);
                    var terms = new List<TermDto>();

                    foreach (var bucket in agg.Buckets)
                    {
                        terms.Add(new TermDto() { Name = (string)bucket.Key.Value, DocCount = bucket.DocCount });
                    }
                    var aggDto = new TermAggregationDto() { Name = filterAgg.a, Terms = terms };
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

        private int GetDateFilterMinutes(string timespan)
        {
            if (timespan?.Length > 0)
            {
                if (timespan == "30m") { return 30; }
                if (timespan == "1h") { return 60; }
                if (timespan == "3h") { return 60 * 3; }
                if (timespan == "6h") { return 60 * 6; }
                if (timespan == "12h") { return 60 * 12; }
                if (timespan == "24h") { return 60 * 24; }
                if (timespan == "3d") { return 60 * 24 * 3; }
                if (timespan == "7d") { return 60 * 24 * 7; }

            }
            return 30;
        }
    }
}
