using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Helpers;
using SOS.Lib.Swagger;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using SOS.Lib.Extensions;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Sighting controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class SystemsController : ControllerBase
    {
        private readonly IDevOpsManager _devOpsManager;
        private readonly IProcessInfoManager _processInfoManager;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IDataProviderCache _dataProviderCache;
        private MongoClient _mongoClient;
        private ElasticsearchClient _elasticClient;
        private string _mongoSuffix = "";
        private MongoDbConfiguration _mongoConfiguration;
        private readonly ILogger<SystemsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="devOpsManager"></param>
        /// <param name="processInfoManager"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="dataProviderCache"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SystemsController(
            IDevOpsManager devOpsManager,
            IProcessInfoManager processInfoManager,
            IProcessedObservationRepository processedObservationRepository,
            ElasticSearchConfiguration elasticConfiguration,
            IDataProviderCache dataProviderCache,
            ILogger<SystemsController> logger)
        {
            _processInfoManager = processInfoManager ?? throw new ArgumentNullException(nameof(processInfoManager));
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _devOpsManager = devOpsManager ?? throw new ArgumentNullException(nameof(devOpsManager));
            _mongoClient = new MongoClient(Settings.ProcessDbConfiguration.GetMongoDbSettings());
            _mongoConfiguration = Settings.ProcessDbConfiguration;
            if (_mongoConfiguration.DatabaseName.EndsWith("-st"))
            {
                _mongoSuffix = "-st";
            }
            else if (_mongoConfiguration.DatabaseName.EndsWith("-dev"))
            {
                _mongoSuffix = "-dev";
            }
            _elasticClient = elasticConfiguration.GetClients().FirstOrDefault();
            _dataProviderCache = dataProviderCache ?? throw new System.ArgumentNullException(nameof(dataProviderCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /*
        /// <inheritdoc />
        [HttpGet("BuildInfo")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetBuildInfoAsync()
        {
            try
            {
                var buildInfo = await _devOpsManager.GetBuildInfoAsync();
                return new OkObjectResult(buildInfo);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting copyright");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        */

        /// <summary>
        /// Get copyright including system build time
        /// </summary>
        /// <returns></returns>
        [HttpGet("Copyright")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public IActionResult Copyright()
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                return new OkObjectResult(fileVersionInfo.LegalCopyright);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting copyright");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get information about observation processing
        /// </summary>
        /// <param name="active">True: get information about last processing, false get information about previous processing</param>
        /// <returns>Meta data about processing. E.g, Start time, end time, number of observations processed...</returns>
        [HttpGet("ProcessInformation")]
        [ProducesResponseType(typeof(ProcessInfoDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
        public async Task<IActionResult> GetProcessInfo([FromQuery] bool active)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                return new OkObjectResult(await _processInfoManager.GetProcessInfoAsync(_processedObservationRepository.UniquePublicIndexName));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting process information");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [Route("harvest")]
        [InternalApi]
        public IEnumerable<HarvestInfoDto> GetHarvestInfo()
        {
            var database = _mongoClient.GetDatabase("sos-harvest" + _mongoSuffix);
            var collection = database.GetCollection<HarvestInfoDto>("HarvestInfo");
            var providers = collection.Find(new BsonDocument());
            return providers.ToList();
        }

        [HttpGet]
        [Route("processmongodb")]
        [InternalApi]
        public IEnumerable<MongoDbProcessInfoDto> GetMongoDbProcessInfo()
        {
            var database = _mongoClient.GetDatabase(_mongoConfiguration.DatabaseName);
            var queryBuilder = Builders<MongoDbProcessInfoDto>.Filter;
            var query = queryBuilder.Regex(pi => pi.Id, @"observation-\d");
            var processInfos = database.GetCollection<MongoDbProcessInfoDto>("ProcessInfo")
            .Find(query)
            .SortByDescending(p => p.End);

            return processInfos?.ToList();
        }

        [HttpGet]
        [Route("activeinstance")]
        [InternalApi]
        public ActiveInstanceInfoDto GetActiveInstance()
        {
            var database = _mongoClient.GetDatabase(_mongoConfiguration.DatabaseName);
            var collection = database.GetCollection<ActiveInstanceInfoDto>("ProcessedConfiguration");
            var instance = collection.Find(new BsonDocument())?.ToList();
            return instance.FirstOrDefault(i => i.Id.Equals("Observation", System.StringComparison.CurrentCultureIgnoreCase));
        }

        [HttpGet]
        [Route("processing")]
        [InternalApi]
        public IEnumerable<HangfireJobDto> GetProcessing()
        {
            var database = _mongoClient.GetDatabase("sos-hangfire" + _mongoSuffix);
            var collection = database.GetCollection<HangfireJobDto>("hangfire.jobGraph");
            var filter = Builders<HangfireJobDto>.Filter.Eq(p => p.StateName, "Processing");
            var jobs = collection.Find(filter).ToList();
            return jobs;
        }

        [HttpGet]
        [Route("searchindex")]
        [InternalApi]
        public async Task<SearchIndexInfoDto> GetSearchIndexInfo()
        {
            var diskUsage = new Dictionary<string, int>();
            var response = await _elasticClient.Nodes.StatsAsync(stats => stats.Metric(new Metrics("fs")));
            var info = new SearchIndexInfoDto();

            if (!response.IsValidResponse)
            {
                return info;
            }

            var allocations = new List<SearchIndexInfoDto.AllocationInfo>();
            foreach (var node in response.Nodes)
            {
                foreach (var data in node.Value.Fs.Data)
                {
                    allocations.Add(new SearchIndexInfoDto.AllocationInfo()
                    {
                        Node = data.Path,
                        DiskAvailable = data.FreeInBytes?.ToString(),
                        DiskTotal = data.TotalInBytes?.ToString(),
                        DiskUsed = (data.TotalInBytes ?? 0 - data.FreeInBytes ?? 0).ToString(),
                        Percentage = (int)((data.FreeInBytes ?? 0) / (data.TotalInBytes ?? 1))
                    });
                }
            }
            info.Allocations = allocations;
            return info;
        }

        [HttpGet]
        [Route("mongoinfo")]
        [InternalApi]
        public MongoDbInfoDto GetMongoDatabaseInfo()
        {
            var db = _mongoClient.GetDatabase("sos-hangfire" + _mongoSuffix);
            var command = new BsonDocument { { "dbStats", 1 }, { "scale", 1000 } };
            var result = db.RunCommand<BsonDocument>(command);
            var usedSize = result.GetValue("fsUsedSize");
            var totalSize = result.GetValue("fsTotalSize");
            var info = new MongoDbInfoDto()
            {
                DiskTotal = totalSize.ToString(),
                DiskUsed = usedSize.ToString()
            };
            return info;
        }

        [HttpGet]
        [Route("process-summary")]
        [InternalApi]
        public ProcessSummaryDto GetProcessSummary()
        {
            var activeInstance = GetActiveInstance();
            var processInfos = GetMongoDbProcessInfo();
            var processSummary = GetProcessSummary(processInfos, activeInstance);
            return processSummary;
        }

        private ProcessSummaryDto GetProcessSummary(IEnumerable<MongoDbProcessInfoDto>? processInfos, ActiveInstanceInfoDto? activeInstanceInfo)
        {
            MongoDbProcessInfoDto activeInfos = processInfos.FirstOrDefault(m => int.Parse(m.Id.Last().ToString()) == activeInstanceInfo.ActiveInstance);
            var inactiveInfos = processInfos.FirstOrDefault(m => int.Parse(m.Id.Last().ToString()) != activeInstanceInfo.ActiveInstance);
            var processSummary = new ProcessSummaryDto
            {
                ActiveProcessStatus = CreateProcessStatus(activeInfos),
                InactiveProcessStatus = CreateProcessStatus(inactiveInfos),
                DataProviderStatuses = GetDataProviderStatuses(activeInfos, inactiveInfos)
            };

            return processSummary;
        }

        private ProcessStatusDto CreateProcessStatus(MongoDbProcessInfoDto processInfo)
        {
            return new ProcessStatusDto
            {
                Name = processInfo.Id,
                Status = processInfo.Status,
                PublicCount = processInfo.PublicCount,
                ProtectedCount = processInfo.ProtectedCount,
                InvalidCount = processInfo.ProcessFailCount,
                Start = processInfo.Start,
                End = processInfo.End
            };
        }

        private List<DataProviderStatusDto> GetDataProviderStatuses(MongoDbProcessInfoDto activeInfos, MongoDbProcessInfoDto inactiveInfos)
        {
            List<DataProviderStatusDto> rows = new();
            var inactiveProvidersById = inactiveInfos.ProvidersInfo
                            .ToDictionary(m => m.DataProviderId!.Value, m => m);

            var dataProviderById = _dataProviderCache.GetAllAsync().Result.ToDictionary(m => m.Id, m => m);
            foreach (var activeProvider in activeInfos.ProvidersInfo.OrderBy(m => m.DataProviderId))
            {
                var dataProvider = dataProviderById[activeProvider.DataProviderId.GetValueOrDefault()];
                var inactiveProvider = inactiveProvidersById.GetValueOrDefault(activeProvider.DataProviderId!.Value, null);
                var activeHarvestTime = (activeProvider.HarvestEnd ?? DateTime.UtcNow) - (activeProvider.HarvestStart ?? DateTime.UtcNow);
                var inactiveHarvestTime = (inactiveProvider?.HarvestEnd ?? DateTime.UtcNow) - (inactiveProvider?.HarvestStart ?? DateTime.UtcNow);
                var activeProcessTime = (activeProvider.ProcessEnd ?? DateTime.UtcNow) - (activeProvider.ProcessStart ?? DateTime.UtcNow);
                var inactiveProcessTime = (inactiveProvider?.ProcessEnd ?? DateTime.UtcNow) - (inactiveProvider?.ProcessStart ?? DateTime.UtcNow);

                var row = new DataProviderStatusDto
                {
                    Id = activeProvider.DataProviderId ?? 0,
                    Identifier = activeProvider.DataProviderIdentifier,
                    Name = dataProvider.Names.Translate("en-GB"),
                    SwedishName = dataProvider.Names.Translate("sv-SE"),
                    PublicActive = activeProvider?.PublicProcessCount ?? 0,
                    PublicInactive = inactiveProvider?.PublicProcessCount ?? 0,
                    PublicDiff = (activeProvider?.PublicProcessCount ?? 0) - (inactiveProvider?.PublicProcessCount ?? 0),
                    ProtectedActive = activeProvider?.ProtectedProcessCount ?? 0,
                    ProtectedInactive = inactiveProvider?.ProtectedProcessCount ?? 0,
                    ProtectedDiff = (activeProvider?.ProtectedProcessCount ?? 0) - (inactiveProvider?.ProtectedProcessCount ?? 0),
                    InvalidActive = activeProvider?.ProcessFailCount.GetValueOrDefault(0) ?? 0,
                    InvalidInactive = inactiveProvider?.ProcessFailCount.GetValueOrDefault(0) ?? 0,
                    InvalidDiff = (activeProvider?.ProcessFailCount.GetValueOrDefault(0) ?? 0) - (inactiveProvider?.ProcessFailCount.GetValueOrDefault(0) ?? 0),
                    HarvestTimeActive = activeHarvestTime,
                    HarvestTimeInactive = inactiveHarvestTime,
                    HarvestTimeDiff = activeHarvestTime - inactiveHarvestTime,
                    ProcessTimeActive = activeProcessTime,
                    ProcessTimeInactive = inactiveProcessTime,
                    ProcessTimeDiff = activeProcessTime - inactiveProcessTime,
                    HarvestStatusActive = activeProvider.HarvestStatus,
                    HarvestStatusInactive = inactiveProvider?.HarvestStatus ?? "Unknown",
                    LatestIncrementalPublicCount = activeProvider.LatestIncrementalPublicCount,
                    LatestIncrementalProtectedCount = activeProvider.LatestIncrementalProtectedCount,
                    LatestIncrementalEnd = activeProvider.LatestIncrementalEnd,
                    LatestIncrementalTime = (activeProvider.LatestIncrementalEnd ?? DateTime.UtcNow) - (activeProvider.LatestIncrementalStart ?? DateTime.UtcNow),
                };

                rows.Add(row);
            }

            return rows.OrderBy(r => r.Id).ToList();
        }
    }
}