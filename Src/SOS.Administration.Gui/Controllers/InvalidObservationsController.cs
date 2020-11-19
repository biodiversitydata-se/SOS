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
    public class InvalidObservation
    {
        public string DatasetID { get; set; }

        /// <summary>
        ///     Name of data set
        /// </summary>
        public string DatasetName { get; set; }

        /// <summary>
        ///     List of defects
        /// </summary>
        public ICollection<string> Defects { get; set; }

        public string OccurrenceID { get; set; }

        public DateTime ModifiedDate { get; set; }

        /// <summary>
        ///     Object id
        /// </summary>
        public ObjectId Id { get; set; }
    }
    public class InvalidLocation
    {
        public string DataSetName { get; set; }
        public string DataSetId { get; set; }
        public string OccurrenceId { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class InstanceInfo
    {        
        public int ActiveInstance { get; set; }        
    }
    [ApiController]
    [Route("[controller]")]
    public class InvalidObservationsController : ControllerBase
    {       
        private readonly ILogger<InvalidObservationsController> _logger;
        private MongoClient _client;

        public InvalidObservationsController(ILogger<InvalidObservationsController> logger, IInvalidObservationsDatabaseSettings invalidObservationsDatabaseSettings)
        {
            _logger = logger;
            _client = new MongoClient(invalidObservationsDatabaseSettings.ConnectionString);
        }

        [HttpGet]        
        public IEnumerable<InvalidLocation> Get(string dataSetId, int instanceId)
        {
            var database = _client.GetDatabase("sos");
            var names = _client.ListDatabaseNames();
            var collection = database.GetCollection<InstanceInfo>("ProcessedConfiguration");
            var info = collection.Find(new BsonDocument()).FirstOrDefault();
            
            var observationCollection = database.GetCollection<InvalidObservation>("InvalidObservation-" + instanceId.ToString());
            
            var filter = Builders<InvalidObservation>.Filter.Eq(p=>p.DatasetID, dataSetId);
            if(dataSetId == "0")
            {
                filter = new BsonDocument();
            }
           
            var documents = observationCollection.Find(filter).ToList();
            var invalidLocations = new List<InvalidLocation>();
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
                            invalidLocations.Add(new InvalidLocation()
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
        public IEnumerable<InvalidObservation> GetList(string dataSetId, int instanceId)
        {
            var database = _client.GetDatabase("sos");
            var names = _client.ListDatabaseNames();
            var collection = database.GetCollection<InstanceInfo>("ProcessedConfiguration");
            var info = collection.Find(new BsonDocument()).FirstOrDefault();

            var observationCollection = database.GetCollection<InvalidObservation>("InvalidObservation-" + instanceId.ToString());

            var filter = Builders<InvalidObservation>.Filter.Eq(p => p.DatasetID, dataSetId);
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
