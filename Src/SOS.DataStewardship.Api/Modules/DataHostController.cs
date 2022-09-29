//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Net;
//using System.Text.Json.Serialization;
//using System.Text.Json;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using SOS.DataStewardship.Api.Models;
//using SOS.Lib.Repositories.Processed.Interfaces;
//using SOS.Observations.Api.Controllers.Interfaces;
//using SOS.Observations.Api.Dtos;
//using SOS.Observations.Api.Managers.Interfaces;
//using SOS.Observations.Api.Swagger;
//using SOS.DataStewardship.Api.Models.SampleData;

//namespace SOS.Observations.Api.Controllers
//{
//    /// <summary>
//    ///     Datahost controller (Datavärdskap)
//    /// </summary>
//    [Route("[controller]")]
//    [ApiController]
//    public class DataHostController : ControllerBase
//    {
//        /*
//         * Todo
//         * ====
//         * 1. Create sample request-response for Artportalen data
//         * 2. Create sample request-response for other data provider
//         * 3. Harvest additional Artportalen metadata
//         * 4. Implement controller actions.
//         * 5. Create integration tests         
//         */

//        private readonly ILogger<DataHostController> _logger;
//        private readonly IProcessInfoManager _processInfoManager;
//        private readonly IProcessedObservationRepository _processedObservationRepository;
//        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
//        {
//            PropertyNameCaseInsensitive = true,
//            Converters = { new JsonStringEnumConverter() }
//        };

//        /// <summary>
//        /// Constructor
//        /// </summary>
//        /// <param name="processInfoManager"></param>
//        /// <param name="processedObservationRepository"></param>
//        /// <param name="logger"></param>
//        /// <exception cref="ArgumentNullException"></exception>
//        public DataHostController(IProcessInfoManager processInfoManager,
//            IProcessedObservationRepository processedObservationRepository,
//            ILogger<DataHostController> logger)
//        {
//            _processInfoManager = processInfoManager ?? throw new ArgumentNullException(nameof(processInfoManager));
//            _processedObservationRepository = processedObservationRepository ??
//                                              throw new ArgumentNullException(nameof(processedObservationRepository));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//        /// <summary>
//        /// Get dataset by ID
//        /// </summary>
//        /// <remarks>Get dataset by ID</remarks>
//        /// <param name="id">ID of the dataset to get</param>
//        /// <response code="200">Success</response>
//        /// <response code="400">Bad Request</response>
//        /// <response code="500">Internal Server Error</response>
//        [HttpGet("/datasets/{id}")]
//        [ProducesResponseType(typeof(Dataset), (int)HttpStatusCode.OK)]
//        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
//        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
//        public async Task<IActionResult> GetDatasetById([FromRoute][Required] string id)
//        {
//            var datasetExample = DataStewardshipArtportalenSampleData.DatasetBats;
//            return new OkObjectResult(datasetExample);
//        }

//        /// <summary>
//        /// Get datasets by search
//        /// </summary>
//        /// <remarks>Get datasets by search</remarks>
//        /// <param name="body">Filter used to limit the search.</param>
//        /// <param name="skip">Start index</param>
//        /// <param name="take">Number of items to return. 1000 items is the max to return in one call.</param>
//        /// <response code="200">Success</response>
//        /// <response code="400">Bad Request</response>
//        /// <response code="500">Internal Server Error</response>
//        [HttpPost("/datasets")]
//        [ProducesResponseType(typeof(List<Dataset>), (int)HttpStatusCode.OK)]
//        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
//        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
//        public async Task<IActionResult> GetDatasetsBySearch([FromBody] DatasetFilter body, [FromQuery] int? skip, [FromQuery] int? take)
//        {
//            var datasetExample = DataStewardshipArtportalenSampleData.DatasetBats;
//            return new OkObjectResult(new List<Dataset> {datasetExample});
//        }

//        /// <summary>
//        /// Get event by ID
//        /// </summary>
//        /// <remarks>Get event by ID</remarks>
//        /// <param name="eventID">EventId of the event to get</param>
//        /// <response code="200">Success</response>
//        /// <response code="400">Bad Request</response>
//        /// <response code="500">Internal Server Error</response>
//        [HttpGet("/events/{eventID}")]
//        [ProducesResponseType(typeof(EventModel), (int)HttpStatusCode.OK)]
//        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
//        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
//        public async Task<IActionResult> GetEventsByID([FromRoute][Required] string eventID)
//        {
//            var eventExample = DataStewardshipArtportalenSampleData.EventBats1;
//            return new OkObjectResult(eventExample);
//        }

//        /// <summary>
//        /// Get event by search
//        /// </summary>
//        /// <remarks>Get event by search</remarks>
//        /// <param name="body">Filter used to limit the search.</param>
//        /// <param name="skip">Start index</param>
//        /// <param name="take">Number of items to return. 1000 items is the max to return in one call.</param>
//        /// <response code="200">Success</response>
//        /// <response code="400">Bad Request</response>
//        /// <response code="500">Internal Server Error</response>
//        [HttpPost("/events")]
//        [ProducesResponseType(typeof(List<EventModel>), (int)HttpStatusCode.OK)]
//        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
//        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
//        public async Task<IActionResult> GetEventsBySearch([FromBody] EventsFilter body, [FromQuery] int? skip, [FromQuery] int? take)
//        {
//            return new OkObjectResult(new List<EventModel>
//            {
//                DataStewardshipArtportalenSampleData.EventBats1,
//                DataStewardshipArtportalenSampleData.EventBats2
//            });
//        }

//        /// <summary>
//        /// Get occurrence by ID
//        /// </summary>
//        /// <param name="occurrenceId">OccurrenceId of the occurrence to get</param>
//        /// <response code="200">Success</response>
//        /// <response code="400">Bad Request</response>
//        /// <response code="500">Internal Server Error</response>
//        [HttpGet("/occurrences/{occurrenceId}")]
//        [ProducesResponseType(typeof(OccurrenceModel), (int)HttpStatusCode.OK)]
//        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
//        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
//        public async Task<IActionResult> GetOccurrencesByID([FromRoute][Required] string occurrenceId)
//        {
//            var occurrenceExample = DataStewardshipArtportalenSampleData.EventBats1Occurrence1;
//            return new OkObjectResult(occurrenceExample);
//        }

//        /// <summary>
//        /// Get occurrences by search
//        /// </summary>
//        /// <param name="body">Filter used to limit the search.</param>
//        /// <param name="skip">Start index</param>
//        /// <param name="take">Number of items to return. 1000 items is the max to return in one call.</param>
//        /// <response code="200">Success</response>
//        /// <response code="400">Bad Request</response>
//        /// <response code="500">Internal Server Error</response>
//        [HttpPost("/occurrences")]
//        [ProducesResponseType(typeof(OccurrenceModel), (int)HttpStatusCode.OK)]
//        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
//        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
//        public async Task<IActionResult> GetOccurrencesBySearch([FromBody] OccurrenceFilter body, [FromQuery] int? skip, [FromQuery] int? take)
//        {
//            var occurrenceExample = DataStewardshipSampleData.Occurrence1;
//            return new OkObjectResult(new List<OccurrenceModel>
//            {
//                DataStewardshipArtportalenSampleData.EventBats1Occurrence1,
//                DataStewardshipArtportalenSampleData.EventBats1Occurrence2,
//                DataStewardshipArtportalenSampleData.EventBats1Occurrence3,
//                DataStewardshipArtportalenSampleData.EventBats2Occurrence1,
//                DataStewardshipArtportalenSampleData.EventBats2Occurrence2,
//            });
//        }
//    }
//}