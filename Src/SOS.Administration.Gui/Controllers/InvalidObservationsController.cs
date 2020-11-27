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
using SOS.Administration.Gui.Models;
using SOS.Lib.Configuration.Shared;

namespace SOS.Administration.Gui.Controllers
{    
    [ApiController]
    [Route("[controller]")]
    public class InvalidObservationsController : ControllerBase
    {       
        private readonly ILogger<InvalidObservationsController> _logger;
        private MongoClient _client;
        private MongoDbConfiguration _configuration;

        public InvalidObservationsController(ILogger<InvalidObservationsController> logger, IOptionsMonitor<MongoDbConfiguration> mongoDbSettings)
        {
            _logger = logger;
            _client = new MongoClient(mongoDbSettings.CurrentValue.GetMongoDbSettings());
            _configuration = mongoDbSettings.CurrentValue;
        }

        [HttpGet]        
        public IEnumerable<InvalidLocationDto> Get(string dataSetId, int instanceId)
        {
            var database = _client.GetDatabase(_configuration.DatabaseName);            
            var observationCollection = database.GetCollection<InvalidObservationDto>("InvalidObservation-" + instanceId.ToString());
            
            var filter = Builders<InvalidObservationDto>.Filter.Eq(p=>p.DatasetID, dataSetId);
            if(dataSetId == "0")
            {
                filter = new BsonDocument();
            }
           
            var documents = observationCollection.Find(filter).ToList();
            var invalidLocations = new List<InvalidLocationDto>();
            foreach(var document in documents)
            {                
                foreach (var defect in document.Defects)
                {
                    try
                    {
                        if (defect.Contains("Swedish"))
                        {
                            var latlonstring = defect.Substring(defect.IndexOf("("), defect.IndexOf(")") - defect.IndexOf("("));
                            var lonstring = latlonstring.Substring(6, latlonstring.IndexOf("lat:") -6 );                            
                            var latstring = latlonstring.Substring(latlonstring.IndexOf("lat:") + 4);
                            var lon = float.Parse(lonstring.Substring(0,lonstring.Length -2));
                            var lat = float.Parse(latstring);
                            invalidLocations.Add(new InvalidLocationDto()
                            {
                                DataSetId = document.DatasetID,
                                DataSetName = document.DatasetName,
                                OccurrenceId = document.OccurrenceID,
                                Lat = lat,
                                Lon = lon
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }             
            }
            return invalidLocations;          
        }
        [HttpGet]
        [Route("list")]
        public IEnumerable<InvalidObservationDto> GetList(string dataSetId, int instanceId)
        {
            var database = _client.GetDatabase(_configuration.DatabaseName);          
            var observationCollection = database.GetCollection<InvalidObservationDto>("InvalidObservation-" + instanceId.ToString());

            var filter = Builders<InvalidObservationDto>.Filter.Eq(p => p.DatasetID, dataSetId);
            if (dataSetId == "0")
            {
                filter = new BsonDocument();
            }           
            var documents = observationCollection.Find(filter).ToList();
            //var ids = documents.Where(p=>p.DatasetID == "1").Select(p => p.OccurrenceID.Substring(p.OccurrenceID.LastIndexOf(":") + 1,p.OccurrenceID.Length - p.OccurrenceID.LastIndexOf(":") - 1) + ",");
            //System.IO.File.WriteAllLines("test_ids.txt", ids.ToArray());
            return documents;
        }
    }
}
