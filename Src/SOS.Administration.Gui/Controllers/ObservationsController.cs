using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SOS.Administration.Gui.Dtos;
using SOS.Administration.Gui.Services;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Administration.Gui.Controllers
{
    public class ObservationDto
    {
        public string DataSetName { get; set; }
        public string DataSetId { get; set; }
        public string OccurrenceId { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double DiffusionRadius { get; set; }
    }
    public class RealObservationDto
    {
        public string OccurrenceId { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class ObservationsController : ControllerBase
    {        
        private readonly ISearchService _service;
        private MongoClient _client;
        private MongoDbConfiguration _configuration;

        public ObservationsController(ISearchService searchService, IOptionsMonitor<MongoDbConfiguration> mongoDbSettings)
        {
            _service = searchService;
            var mongoSettings = mongoDbSettings.CurrentValue.GetMongoDbSettings();
            mongoSettings.SocketTimeout = new TimeSpan(0, 2, 0);
            _client = new MongoClient(mongoDbSettings.CurrentValue.GetMongoDbSettings());
            _configuration = mongoDbSettings.CurrentValue;
        }        
        [HttpGet]
        [Route("real/{occurrenceId}")]
        public async Task<RealObservationDto> GetRealObservation(string occurrenceId)
        {
            
            var databaseName = "harvest";
            var suffix = "";
            if(_configuration.DatabaseName.Contains("-"))
            {
                suffix = _configuration.DatabaseName.Substring(_configuration.DatabaseName.LastIndexOf("-"));
            }
            var database = _client.GetDatabase("sos-" + databaseName + suffix);
            var observationCollection = database.GetCollection<ArtportalenObservationVerbatim>("ArtportalenObservationVerbatim");

            var sightingIdString = occurrenceId.Substring(occurrenceId.LastIndexOf(':') + 1);
            var sightingId = int.Parse(sightingIdString);

            var filter = Builders<ArtportalenObservationVerbatim>.Filter.Eq(p => p.SightingId, sightingId);

            var observations = await observationCollection.FindAsync(filter);
            try 
            {
                var obs = await observations.FirstAsync();
                var realObs = new RealObservationDto
                {
                    OccurrenceId = obs.SightingId.ToString(),
                    Lat = (double)obs.Site.Point.Coordinates[1],
                    Lon = (double)obs.Site.Point.Coordinates[0]
                };
                return realObs;
            }
            catch
            {
                return null;
            }
        }
        [HttpPost]
        [Route("")]
        public async Task<PagedResult<ObservationDto>> SearchSOS(SearchFilterDto searchFilterDto)
        {
           
           
            PagedResult<ObservationDto> result = new PagedResult<ObservationDto>();
           
            try
            {
                var sosResult = await _service.SearchSOS(searchFilterDto, 100, 0);
                var records = new List<ObservationDto>();
                foreach (var sResult in sosResult.Records)
                {
                    records.Add(new ObservationDto()
                    {
                        OccurrenceId = sResult.Occurrence.OccurrenceId,
                        DataSetId = sResult.DataSetId,
                        DataSetName = sResult.DataSetName,
                        DiffusionRadius = sResult.Location.CoordinateUncertaintyInMeters,
                        Lat = sResult.Location.DecimalLatitude,
                        Lon = sResult.Location.DecimalLongitude
                    });
                }
                result.Records = records;
                result.Skip = sosResult.Skip;
                result.Take = sosResult.Take;
                result.TotalCount = sosResult.TotalCount;
            }
            catch
            {
                return result;
            }
            return result;
        }
    }
}
