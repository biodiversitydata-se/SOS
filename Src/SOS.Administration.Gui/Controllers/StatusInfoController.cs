using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using SOS.Administration.Gui.Models;

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
    [ApiController]
    [Route("[controller]")]
    public class StatusInfoController : ControllerBase
    {       
        private readonly ILogger<StatusInfoController> _logger;
        private MongoClient _client;

        public StatusInfoController(ILogger<StatusInfoController> logger, IInvalidObservationsDatabaseSettings invalidObservationsDatabaseSettings)
        {
            _logger = logger;
            _client = new MongoClient(invalidObservationsDatabaseSettings.ConnectionString);
        }

        [HttpGet]
        [Route("harvest")]
        public IEnumerable<HarvestInfo> GetHarvestInfo()
        {
            var database = _client.GetDatabase("sos-harvest");            
            var collection = database.GetCollection<HarvestInfo>("HarvestInfo");
            var providers = collection.Find(new BsonDocument());
            return providers.ToList();          
       }
        [HttpGet]
        [Route("process")]
        public IEnumerable<ProcessInfo> GetProcessInfo()
        {
            var database = _client.GetDatabase("sos");
            var collection = database.GetCollection<ProcessInfo>("ProcessInfo");
            var providers = collection.Find(new BsonDocument());
            return providers.SortByDescending(p=>p.End).ToList();
        }
        [HttpGet]
        [Route("activeinstance")]
        public ActiveInstanceInfo GetActiveInstance()
        {
            var database = _client.GetDatabase("sos");
            var collection = database.GetCollection<ActiveInstanceInfo>("ProcessedConfiguration");
            var instance = collection.Find(new BsonDocument()).ToList();
            return instance.FirstOrDefault();
        }
    }
}
