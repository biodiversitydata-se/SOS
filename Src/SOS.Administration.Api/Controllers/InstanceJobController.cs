﻿using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using System;
using System.Net;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Instance job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class InstanceJobController : ControllerBase, IInstanceJobController
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly ILogger<InstanceJobController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public InstanceJobController(
            IDataProviderManager dataProviderManager,
            ILogger<InstanceJobController> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("Activate")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RunSetActivateInstanceJob([FromQuery] byte instance)
        {
            try
            {
                if (instance < 0 || instance > 1)
                {
                    _logger.LogError("Instance must be 0 or 1");
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }

                BackgroundJob.Enqueue<IActivateInstanceJob>(job => job.RunAsync(instance));
                return new OkObjectResult("Activate instance job was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing Activate instance failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}