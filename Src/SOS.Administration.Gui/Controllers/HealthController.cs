using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOS.Administration.Gui.Services;

namespace SOS.Administration.Gui.Controllers
{
   
    [Route("[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {        
        private readonly ISearchService _service;
       
        public HealthController(ISearchService searchService)
        {
            _service = searchService;
        }

        [HttpGet]
        public async Task<dynamic> GetHealthStatus()
        {
            try
            {
                var healtStatus = await _service.GetHealthStatus();

                return healtStatus;
            }
            catch
            {
                return null;
            } ;
        }
    }
}
