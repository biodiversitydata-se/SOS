using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Administration.Gui.Models;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Administration.Gui.Controllers
{


    [ApiController]
    [Route("[controller]")]
    public class StatusInfoController : ControllerBase
    {
        private readonly ILogger<StatusInfoController> _logger;
        private MongoClient _mongoClient;
        private ElasticsearchClient _elasticClient;
        private string _mongoSuffix = "";
        private MongoDbConfiguration _mongoConfiguration;

        public StatusInfoController(ILogger<StatusInfoController> logger, ElasticSearchConfiguration elasticConfiguration)
        {
            _logger = logger;
            _mongoClient = new MongoClient(Settings.ProcessDbConfiguration.GetMongoDbSettings());
            _mongoConfiguration = Settings.ProcessDbConfiguration;
            if (_mongoConfiguration.DatabaseName.EndsWith("-st"))
            {
                _mongoSuffix = "-st";
            }
            _elasticClient = elasticConfiguration.GetClients().FirstOrDefault();
        }

        [HttpGet]
        [Route("harvest")]
        public IEnumerable<HarvestInfoDto> GetHarvestInfo()
        {
            var database = _mongoClient.GetDatabase("sos-harvest" + _mongoSuffix);
            var collection = database.GetCollection<HarvestInfoDto>("HarvestInfo");
            var providers = collection.Find(new BsonDocument());
            return providers.ToList();
        }
        [HttpGet]
        [Route("process")]
        public IEnumerable<ProcessInfoDto> GetProcessInfo()
        {
            var database = _mongoClient.GetDatabase(_mongoConfiguration.DatabaseName);
            var queryBuilder = Builders<ProcessInfoDto>.Filter;
            var query = queryBuilder.Regex(pi => pi.Id, @"observation-\d");
            var processInfos = database.GetCollection<ProcessInfoDto>("ProcessInfo")
            .Find(query)
            .SortByDescending(p => p.End);

            return processInfos?.ToList();
        }
        [HttpGet]
        [Route("activeinstance")]
        public ActiveInstanceInfoDto GetActiveInstance()
        {
            var database = _mongoClient.GetDatabase(_mongoConfiguration.DatabaseName);
            var collection = database.GetCollection<ActiveInstanceInfoDto>("ProcessedConfiguration");
            var instance = collection.Find(new BsonDocument())?.ToList();
            return instance.FirstOrDefault(i => i.Id.Equals("Observation", System.StringComparison.CurrentCultureIgnoreCase));
        }
        [HttpGet]
        [Route("processing")]
        public IEnumerable<HangfireJobDto> GetProcessing()
        {
            var database = _mongoClient.GetDatabase("sos-hangfire" + _mongoSuffix);
            var collection = database.GetCollection<HangfireJobDto>("hangfire.jobGraph");
            var filter = Builders<HangfireJobDto>.Filter.Eq(p => p.StateName, "Processing");
            var jobs = collection.Find(filter).ToList();
            return jobs;
        }
        [HttpGet]
        [Route("searchindex")]
        public async Task<SearchIndexInfoDto> GetSearchIndexInfo()
        {
            var diskUsage = new Dictionary<string, int>();
            var response = await _elasticClient.Nodes.StatsAsync(stats => stats.Metric(new Metrics("fs")));
            var info = new SearchIndexInfoDto();
            
            if (!response.IsValidResponse)
            {
                return info;
            }

            var allocations = new List<SearchIndexInfoDto.AllocationInfo>();
            foreach (var node in response.Nodes)
            {
                foreach (var data in node.Value.Fs.Data)
                {
                    allocations.Add(new SearchIndexInfoDto.AllocationInfo()
                    {
                        Node = data.Path,
                        DiskAvailable = data.FreeInBytes?.ToString(),
                        DiskTotal = data.TotalInBytes?.ToString(),
                        DiskUsed = (data.TotalInBytes ?? 0 - data.FreeInBytes ?? 0).ToString(),
                        Percentage = (int)((data.FreeInBytes ?? 0) / (data.TotalInBytes ?? 1))
                    });
                }
            }
            info.Allocations = allocations;
            return info; 
        }

        [HttpGet]
        [Route("mongoinfo")]
        public MongoDbInfoDto GetMongoDatabaseInfo()
        {
            var db = _mongoClient.GetDatabase("sos-hangfire" + _mongoSuffix);
            var command = new BsonDocument { { "dbStats", 1 }, { "scale", 1000 } };
            var result = db.RunCommand<BsonDocument>(command);
            var usedSize = result.GetValue("fsUsedSize");
            var totalSize = result.GetValue("fsTotalSize");
            var info = new MongoDbInfoDto()
            {
                DiskTotal = totalSize.ToString(),
                DiskUsed = usedSize.ToString()
            };
            return info;
        }
    }
}
