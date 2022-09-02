using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Exceptions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Swagger;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Extensions;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     User controller.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {        
        private readonly ILogger<UserController> _logger;
        private readonly IUserManager _userManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userManager"></param>        
        /// <param name="logger"></param>
        public UserController(
            IUserManager userManager,
            ILogger<UserController> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get user information. Add an authorization header to get information about the user.
        /// </summary>
        /// <param name="applicationIdentifier">Application identifier making the request, used for retrieve roles and authorizations for the application you use.</param>
        /// <param name="cultureCode">The culture code used for translating role descriptions.</param>
        /// <returns></returns>
        [HttpGet("Information")]
        [ProducesResponseType(typeof(IEnumerable<UserInformationDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GetUserInformation(
            [FromQuery] string applicationIdentifier = null,
            [FromQuery] string cultureCode = "sv-SE")
        {
            try
            {
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                var userInfo = await _userManager.GetUserInformationAsync(applicationIdentifier, cultureCode);
                var dto = userInfo.ToUserInformationDto();
                return new OkObjectResult(dto);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting projects");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}