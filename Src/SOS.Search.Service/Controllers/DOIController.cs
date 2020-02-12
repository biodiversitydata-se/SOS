using System;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Export.Jobs;
using SOS.Export.Jobs.Interfaces;
using SOS.Lib.Models.Search;

namespace SOS.Search.Service.Controllers
{
    /// <summary>
    /// Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DOIController : ControllerBase, Interfaces.IDOIController
    {
        private readonly ILogger<DOIController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public DOIController(ILogger<DOIController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("Create")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunDOIExportJob([FromBody]AdvancedFilter filter)
        {
            try
            {
                var fileName = Guid.NewGuid().ToString();
                BackgroundJob.Enqueue<IDOIJob>(job => job.RunAsync(filter, fileName, JobCancellationToken.Null));

                return new OkObjectResult($"Running DOI export. File name {fileName}.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running DOI export failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
