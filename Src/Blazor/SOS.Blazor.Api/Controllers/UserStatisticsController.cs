using Microsoft.AspNetCore.Mvc;
using SOS.Blazor.Shared;

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
        public async Task<PagedResultDto<UserStatisticsItem>?> GetByQuery(
            [FromQuery] int skip, 
            [FromQuery] int take, 
            [FromBody] SpeciesCountUserStatisticsQuery query)
        {
            var sosClient = new SosClient("https://sos-search-dev.artdata.slu.se/");
            var userStatistics = await sosClient.GetUserStatisticsAsync(skip, take, query);
            return userStatistics;
        }
    }
}