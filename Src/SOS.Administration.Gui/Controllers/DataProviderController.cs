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
    public class DataProviderController : ControllerBase
    {
        private readonly ILogger<DataProviderController> _logger;
        private MongoClient _client;
        private MongoDbConfiguration _configuration;

        public DataProviderController(ILogger<DataProviderController> logger)
        {
            _logger = logger;
            _client = new MongoClient(Settings.ProcessDbConfiguration.GetMongoDbSettings());
            _configuration = Settings.ProcessDbConfiguration;
        }

        [HttpGet]
        [Route("")]
        public IEnumerable<DataProviderDto> Get()
        {
            var database = _client.GetDatabase(_configuration.DatabaseName);
            var collection = database.GetCollection<DataProviderDto>("DataProvider");
            var providers = collection.Find(new BsonDocument());
            return providers.ToList();
        }
    }
}
