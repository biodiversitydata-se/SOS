using Microsoft.AspNetCore.Mvc;
using SOS.Blazor.Shared.Models;

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
        public async Task<PagedResult<Area>?> GetAreas(
            [FromQuery] int skip, 
            [FromQuery] int take, 
            [FromQuery] AreaType areaType)
        {
            var sosClient = new SosClient("https://localhost:44380/");
            var areas = await sosClient.GetAreas(skip, take, areaType);
            return areas;
        }
    }
}