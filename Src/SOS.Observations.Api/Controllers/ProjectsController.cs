using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Swagger;
using SOS.Shared.Api.Dtos.Vocabulary;
using SOS.Shared.Api.Extensions.Controller;
using SOS.Shared.Api.Extensions.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Observation controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectManager _projectManager;
        private readonly ILogger<ProjectsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projectManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ProjectsController(
            IProjectManager projectManager,
            ILogger<ProjectsController> logger) 
        {
            _projectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get projects matching passed filter
        /// </summary>
        /// <param name="filter">Limit project list by this filter</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProjectDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
        public async Task<IActionResult> GetProjects(
            [FromQuery] string filter)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);

                var projects = await _projectManager.GetAsync(filter, base.User?.GetUserId());

                if ((projects?.Count() ?? 0) == 0)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }
                return new OkObjectResult(projects.Select(p => p.ToDto()));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting projects");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }


        [HttpPost]
        [ProducesResponseType(typeof(IEnumerable<ProjectDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
        public async Task<IActionResult> GetProjectesById(
            [FromBody] int[] ids)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);

                var projects = await _projectManager.GetAsync(ids);

                if ((projects?.Count() ?? 0) == 0)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }
                return new OkObjectResult(projects.Select(p => p.ToDto()));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting projects");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get project by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
        public async Task<IActionResult> GetProject(
            [FromRoute] int id)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);

                var project = await _projectManager.GetAsync(id, base.User?.GetUserId());
                if (project == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                return new OkObjectResult(project.ToDto());
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error getting project: {id}");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}