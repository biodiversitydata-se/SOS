using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Extensions;
using SOS.Lib.Models.ApiInfo ;
using System;
using System.Net;
using System.Reflection;

namespace SOS.Analysis.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiInfoController : Controller
    {
        [HttpGet]
        [ProducesResponseType(typeof(ApiInformation), (int)HttpStatusCode.OK)]
        public IActionResult GetApiInfoAsync()
        {
            var buildDate = Assembly.GetExecutingAssembly().GetBuildDate();
            string version = Assembly.GetExecutingAssembly().GetVersionNumber();

            var apiInformation = new ApiInformation
            {
                ApiName = "SOS Search API",
                ApiStatus = "active",
                ApiVersion = version,
                ApiDocumentation = new Uri("https://github.com/biodiversitydata-se/SOS"),
                ApiReleased = new DateTimeOffset(buildDate.ToLocalTime())
            };

            return new OkObjectResult(apiInformation!);
        }
    }
}
