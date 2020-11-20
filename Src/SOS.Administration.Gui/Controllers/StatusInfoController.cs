using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Nest;
using SOS.Lib.Configuration.Shared;

namespace SOS.Administration.Gui.Controllers
{
    [BsonIgnoreExtraElements]
    public class HarvestInfo
    {
        public string Id { get; set; }

        public int Count { get; set; }

        public DateTime? DataLastModified { get; set; }
        public DateTime End { get; set; }
        public DateTime Start { get; set; }
        public string Status { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class ProcessInfo
    {
        [BsonIgnoreExtraElements]
        public class Provider
        {
            public int? DataProviderId { get; set; }
            public string DataProviderIdentifier { get; set; }
            public DateTime? ProcessEnd { get; set; }
            public DateTime? ProcessStart { get; set; }
            public string ProcessStatus { get; set; }
            public int? ProcessCount { get; set; }
            public DateTime? HarvestEnd { get; set; }
            public DateTime? HarvestStart { get; set; }
            public string HarvestStatus { get; set; }
            public int? HarvestCount { get; set; }
            public DateTime? LatestIncrementalEnd { get; set; }
            public DateTime? LatestIncrementalStart { get; set; }
            public int? LatestIncrementalStatus { get; set; }
            public int? LatestIncrementalCount { get; set; }
        }
        public string Id { get; set; }

        public int Count { get; set; }
        public DateTime End { get; set; }
        public DateTime Start { get; set; }
        public string Status { get; set; }
        public IEnumerable<Provider> ProvidersInfo { get; set; }
    }
    public class ActiveInstanceInfo
    {
        public int Id { get; set; }
        public int ActiveInstance { get; set; }
    }
    public class SearchIndexInfo
    {
        public class AllocationInfo
        {
            public int Percentage { get; set; }
            public string Node{ get; set; }
            public string DiskAvailable { get; set; }
            public string DiskUsed { get; set; }
            public string DiskTotal { get; set; }
        }
        public IEnumerable<AllocationInfo> Allocations { get; set; }
    }
    [ApiController]
    [Route("[controller]")]
    public class StatusInfoController : ControllerBase
    {       
        private readonly ILogger<StatusInfoController> _logger;
        private MongoClient _mongoClient;
        private ElasticClient _elasticClient;

        public StatusInfoController(ILogger<StatusInfoController> logger, IOptionsMonitor<MongoDbConfiguration> mongoDbSettings, IOptionsMonitor<ElasticSearchConfiguration> elasticConfiguration)
        {
            _logger = logger;
            _mongoClient = new MongoClient(mongoDbSettings.CurrentValue.GetMongoDbSettings());            
            _elasticClient = elasticConfiguration.CurrentValue.GetClient();

        }

        [HttpGet]
        [Route("harvest")]
        public IEnumerable<HarvestInfo> GetHarvestInfo()
        {
            var database = _mongoClient.GetDatabase("sos-harvest");            
            var collection = database.GetCollection<HarvestInfo>("HarvestInfo");
            var providers = collection.Find(new BsonDocument());
            return providers.ToList();          
       }
        [HttpGet]
        [Route("process")]
        public IEnumerable<ProcessInfo> GetProcessInfo()
        {
            var database = _mongoClient.GetDatabase("sos");
            var collection = database.GetCollection<ProcessInfo>("ProcessInfo");
            var providers = collection.Find(new BsonDocument());
            return providers.SortByDescending(p=>p.End).ToList();
        }
        [HttpGet]
        [Route("activeinstance")]
        public ActiveInstanceInfo GetActiveInstance()
        {
            var database = _mongoClient.GetDatabase("sos");
            var collection = database.GetCollection<ActiveInstanceInfo>("ProcessedConfiguration");
            var instance = collection.Find(new BsonDocument()).ToList();
            return instance.FirstOrDefault();
        }
        [HttpGet]
        [Route("searchindex")]
        public SearchIndexInfo GetSearchIndexInfo()
        {
            var allocation = _elasticClient.Cat.Allocation();
            var info = new SearchIndexInfo();
            if (allocation.IsValid)
            {
                var allocations = new List<SearchIndexInfo.AllocationInfo>();
                foreach(var record in allocation.Records)
                {
                    allocations.Add(new SearchIndexInfo.AllocationInfo()
                    {
                        Node = record.Node,
                        DiskAvailable = record.DiskAvailable,
                        DiskTotal = record.DiskTotal,
                        DiskUsed = record.DiskUsed,
                        Percentage = int.Parse(record.DiskPercent)
                    });
                }
                info.Allocations = allocations;
            }
            return info;
        }
    }
}
