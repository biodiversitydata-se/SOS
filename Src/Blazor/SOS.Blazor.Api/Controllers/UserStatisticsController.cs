using Microsoft.AspNetCore.Mvc;
using SOS.Blazor.Shared.Models;

namespace SOS.Blazor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserStatisticsController : ControllerBase
    {
        private readonly ILogger<UserStatisticsController> _logger;

        public UserStatisticsController(ILogger<UserStatisticsController> logger)
        {
            _logger = logger;
        }

        [HttpPost("PagedSpeciesCountAggregation")]
        public async Task<PagedResult<UserStatisticsItem>?> GetByQuery(
            [FromQuery] int skip, 
            [FromQuery] int take,
            [FromQuery] bool? useCache,
            [FromBody] SpeciesCountUserStatisticsQuery query)
        {
            var sosClient = new SosClient("https://sos-search-dev.artdata.slu.se/");
            var userStatistics = await sosClient.GetUserStatisticsAsync(skip, take, useCache.GetValueOrDefault(true), query);
            return userStatistics;
        }
    }
}