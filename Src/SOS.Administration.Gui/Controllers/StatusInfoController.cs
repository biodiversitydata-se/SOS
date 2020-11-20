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
using SOS.Administration.Gui.Models;
using SOS.Lib.Configuration.Shared;

namespace SOS.Administration.Gui.Controllers
{
   
   
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
        public IEnumerable<HarvestInfoDto> GetHarvestInfo()
        {
            var database = _mongoClient.GetDatabase("sos-harvest");            
            var collection = database.GetCollection<HarvestInfoDto>("HarvestInfo");
            var providers = collection.Find(new BsonDocument());
            return providers.ToList();          
       }
        [HttpGet]
        [Route("process")]
        public IEnumerable<ProcessInfoDto> GetProcessInfo()
        {
            var database = _mongoClient.GetDatabase("sos");
            var collection = database.GetCollection<ProcessInfoDto>("ProcessInfo");
            var providers = collection.Find(new BsonDocument());
            return providers.SortByDescending(p=>p.End).ToList();
        }
        [HttpGet]
        [Route("activeinstance")]
        public ActiveInstanceInfoDto GetActiveInstance()
        {
            var database = _mongoClient.GetDatabase("sos");
            var collection = database.GetCollection<ActiveInstanceInfoDto>("ProcessedConfiguration");
            var instance = collection.Find(new BsonDocument()).ToList();
            return instance.FirstOrDefault();
        }
        [HttpGet]
        [Route("processing")]
        public IEnumerable<HangfireJobDto> GetProcessing()
        {
            var database = _mongoClient.GetDatabase("sos-hangfire");
            var collection = database.GetCollection<HangfireJobDto>("hangfire.jobGraph");
            var filter = Builders<HangfireJobDto>.Filter.Eq(p => p.StateName, "Processing");
            var jobs = collection.Find(filter).ToList();
            return jobs;
        }
        [HttpGet]
        [Route("searchindex")]
        public SearchIndexInfoDto GetSearchIndexInfo()
        {
            var allocation = _elasticClient.Cat.Allocation();
            var info = new SearchIndexInfoDto();
            if (allocation.IsValid)
            {
                var allocations = new List<SearchIndexInfoDto.AllocationInfo>();
                foreach(var record in allocation.Records)
                {
                    allocations.Add(new SearchIndexInfoDto.AllocationInfo()
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
