using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
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
        private string _mongoSuffix = "";
        private MongoDbConfiguration _mongoConfiguration;

        public StatusInfoController(ILogger<StatusInfoController> logger, IOptionsMonitor<MongoDbConfiguration> mongoDbSettings, ElasticSearchConfiguration elasticConfiguration)
        {
            _logger = logger;
            _mongoClient = new MongoClient(mongoDbSettings.CurrentValue.GetMongoDbSettings());
            _mongoConfiguration = mongoDbSettings.CurrentValue;
            if (_mongoConfiguration.DatabaseName.EndsWith("-st"))
            {
                _mongoSuffix = "-st";
            }
            _elasticClient = elasticConfiguration.GetClient();

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
            var collection = database.GetCollection<ProcessInfoDto>("ProcessInfo");
            var providers = collection.Find(new BsonDocument());
            var providerList = providers.SortByDescending(p => p.End).ToList();
            providerList.Where(g => g.ProvidersInfo != null).ToList();
            return providerList;
        }
        [HttpGet]
        [Route("activeinstance")]
        public ActiveInstanceInfoDto GetActiveInstance()
        {
            var database = _mongoClient.GetDatabase(_mongoConfiguration.DatabaseName);
            var collection = database.GetCollection<ActiveInstanceInfoDto>("ProcessedConfiguration");
            var instance = collection.Find(new BsonDocument()).ToList();
            return instance.FirstOrDefault();
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
        public SearchIndexInfoDto GetSearchIndexInfo()
        {
            var allocation = _elasticClient.Cat.Allocation();
            var info = new SearchIndexInfoDto();
            if (allocation.IsValid)
            {
                var allocations = new List<SearchIndexInfoDto.AllocationInfo>();
                foreach(var record in allocation.Records)
                {
                    if (int.TryParse(record.DiskPercent, out int percentage))
                    {
                        allocations.Add(new SearchIndexInfoDto.AllocationInfo()
                        {
                            Node = record.Node,
                            DiskAvailable = record.DiskAvailable,
                            DiskTotal = record.DiskTotal,
                            DiskUsed = record.DiskUsed,
                            Percentage = percentage
                        });
                    }
                }
                info.Allocations = allocations;
            }
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
