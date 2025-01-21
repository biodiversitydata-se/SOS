﻿using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Helpers;
using SOS.Lib.Swagger;
using System;
using System.Net;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class JobsController : ControllerBase
    {

        private readonly ILogger<ExportsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public JobsController(ILogger<ExportsController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Get status of job
        /// </summary>
        /// <param name="jobId">Job id returned when export was requested</param>
        /// <returns></returns>
        [HttpGet("{jobId}/Status")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
        public IActionResult GetStatus([FromRoute] string jobId)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var connection = JobStorage.Current.GetConnection();
                var jobData = connection.GetJobData(jobId);

                if (jobData == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                return new OkObjectResult(jobData.State);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Getting export job status failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}