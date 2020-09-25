using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.DOI.Controllers.Interfaces;
using SOS.Lib.Services.Interfaces;

namespace SOS.DOI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoiController : ControllerBase, IDoiController
    {
        private readonly IDataCiteService _dataCiteService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<DoiController> _logger;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="dataCiteService"></param>
        /// <param name="blobStorageService"></param>
        /// <param name="logger"></param>
        public DoiController(
            IDataCiteService dataCiteService,
            IBlobStorageService blobStorageService,
            ILogger<DoiController> logger)
        {
            _dataCiteService = dataCiteService ?? throw new ArgumentNullException(nameof(dataCiteService));
            _blobStorageService = blobStorageService ?? throw new ArgumentException(nameof(blobStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpGet("{suffix}/URL")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetDOIFileUrl([FromRoute] string suffix)
        {
            try
            {
                var downloadUrl = _blobStorageService.GetDOIDownloadUrl(suffix);

                return new OkObjectResult(downloadUrl);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting DOI file");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpGet("{prefix}/{suffix}")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMetadata([FromRoute] string prefix, [FromRoute] string suffix)
        {
            try
            {
                if (string.IsNullOrEmpty(suffix))
                {
                    return BadRequest("You must provide a doi");
                }

                var metadata = await _dataCiteService.GetMetadataAsync(prefix, suffix);

                return new OkObjectResult(metadata);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Getting DOI metadata failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchMetadata([FromQuery] string searchFor)
        {
            try
            {
                if (string.IsNullOrEmpty(searchFor))
                {
                    return BadRequest("You must provide something to search for");
                }

                var metadata = await _dataCiteService.SearchMetadataAsync(searchFor);

                return new OkObjectResult(metadata);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Getting DOI metadata failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
