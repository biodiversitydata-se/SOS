using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SOS.Administration.Gui.Services;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using Xunit;

namespace SOS.Administration.Gui.Controllers
{
    public class Observation 
    {
        public string DataSetName { get; set; }
        public string DataSetId { get; set; }
        public string OccurrenceId { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double DiffusionRadius { get; set; }
    }
    [Route("[controller]")]
    [ApiController]
    public class ObservationsController : ControllerBase
    {        
        private readonly ISearchService _service;

        public ObservationsController(ISearchService searchService)
        {
            _service = searchService;
        }        
        [HttpPost]
        [Route("")]
        public async Task<PagedResult<Observation>> SearchSOS(SearchFilterDto searchFilterDto)
        {
           
           
            PagedResult<Observation> result = new PagedResult<Observation>();
           
            try
            {
                var sosResult = await _service.SearchSOS(searchFilterDto, 100, 0);
                var records = new List<Observation>();
                foreach (var sResult in sosResult.Records)
                {
                    records.Add(new Observation()
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
            catch (Exception e)
            {
                return result;
            }
            return result;
        }

    }
}
