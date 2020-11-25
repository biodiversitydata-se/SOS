using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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

        public HostingEnvironmentController(IWebHostEnvironment hostingEnvironment, IOptionsMonitor<ApiTestConfiguration> optionsMonitor) 
        {
            _hostingEnvironment = hostingEnvironment;
            _hangfireUrl = optionsMonitor.CurrentValue.HangfireUrl;
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
