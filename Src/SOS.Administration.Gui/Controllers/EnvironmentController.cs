using Microsoft.AspNetCore.Hosting;

namespace SOS.Administration.Gui.Controllers
{
    public class EnvironmentDto
    {
        public string Environment { get; set; }
        public string HangfireUrl { get; set; }
    }
    [Route("[controller]")]
    [ApiController]
    public class HostingEnvironmentController : ControllerBase
    {
        private IWebHostEnvironment _hostingEnvironment;
        private readonly string _hangfireUrl;

        public HostingEnvironmentController(IWebHostEnvironment hostingEnvironment, ApiTestConfiguration apiTestConfiguration)
        {
            _hostingEnvironment = hostingEnvironment;
            _hangfireUrl = apiTestConfiguration.HangfireUrl;
        }
        [HttpGet]
        public EnvironmentDto Get()
        {
            return new EnvironmentDto()
            {
                Environment = _hostingEnvironment.EnvironmentName,
                HangfireUrl = _hangfireUrl
            };
        }
    }
}
