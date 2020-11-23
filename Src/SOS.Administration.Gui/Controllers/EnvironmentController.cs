using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Administration.Gui.Controllers
{
    public class EnvironmentDto
    {
        public string Environment { get; set; }
    }
    [Route("[controller]")]
    [ApiController]
    public class EnvironmentController : ControllerBase
    {
        private IWebHostEnvironment _hostingEnvironment;
        public EnvironmentController(IWebHostEnvironment hostingEnvironment) 
        {
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet]        
        public EnvironmentDto Get()
        {
            return new EnvironmentDto()
            {
                Environment = _hostingEnvironment.EnvironmentName
            };
        }
    }
}
