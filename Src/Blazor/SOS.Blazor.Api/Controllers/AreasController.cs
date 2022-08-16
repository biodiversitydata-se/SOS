using Microsoft.AspNetCore.Mvc;
using SOS.Blazor.Shared;

namespace SOS.Blazor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AreasController : ControllerBase
    {
        private readonly ILogger<AreasController> _logger;

        public AreasController(ILogger<AreasController> logger)
        {
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<PagedResultDto<Area>?> GetAreas(
            [FromQuery] int skip, 
            [FromQuery] int take, 
            [FromQuery] AreaType areaType)
        {
            var sosClient = new SosClient("https://sos-search-dev.artdata.slu.se/");
            var areas = await sosClient.GetAreas(skip, take, areaType);
            return areas;
        }
    }
}