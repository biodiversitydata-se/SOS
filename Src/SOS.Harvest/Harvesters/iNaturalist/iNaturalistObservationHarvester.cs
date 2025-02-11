using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using SOS.Harvest.Harvesters.iNaturalist.Interfaces;
using SOS.Harvest.Services;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.INaturalist.Service;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System.Text;

namespace SOS.Harvest.Harvesters.iNaturalist
{
    public class iNaturalistObservationHarvester : IiNaturalistObservationHarvester
    {
        private readonly IVerbatimClient _verbatimClient;
        private readonly IiNaturalistObservationService _iNaturalistObservationService;
        private readonly iNaturalistServiceConfiguration _iNaturalistServiceConfiguration;
        private readonly iNaturalistApiObservationService _iNaturalistApiObservationService;
        private readonly IiNaturalistCompleteObservationVerbatimRepository _iNaturalistCompleteRepository;
        private readonly ILogger<iNaturalistObservationHarvester> _logger;
        private const string IncrementalCollectionName = "iNaturalistObservations";
        private const string IncrementalTempCollectionName = "iNaturalistObservations_temp";
        private const string FullCollectionName = "iNaturalistFullHarvest";
        private const string FullTempCollectionName = "iNaturalistFullHarvest_temp";
        private const int MaxNrIncrementalObservations = 100000;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="verbatimClient"></param>
        /// <param name="iNaturalistObservationService"></param>
        /// <param name="iNaturalistServiceConfiguration"></param>
        /// <param name="logger"></param>
        public iNaturalistObservationHarvester(
            IVerbatimClient verbatimClient,
            IiNaturalistObservationService iNaturalistObservationService,
            iNaturalistApiObservationService iNaturalistApiObservationService,
            iNaturalistServiceConfiguration iNaturalistServiceConfiguration,
            IiNaturalistCompleteObservationVerbatimRepository iNaturalistCompleteRepository,
            ILogger<iNaturalistObservationHarvester> logger)
        {
            _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));
            _iNaturalistObservationService =
                iNaturalistObservationService ?? throw new ArgumentNullException(nameof(iNaturalistObservationService));
            _iNaturalistApiObservationService =
                iNaturalistApiObservationService ?? throw new ArgumentNullException(nameof(iNaturalistApiObservationService));
            _iNaturalistServiceConfiguration = iNaturalistServiceConfiguration ??
                                               throw new ArgumentNullException(nameof(iNaturalistServiceConfiguration));
            _iNaturalistCompleteRepository = iNaturalistCompleteRepository ??
                                               throw new ArgumentNullException(nameof(iNaturalistCompleteRepository));            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HarvestInfo> HarvestObservationsAsync(JobRunModes mode,
            DateTime? fromDate,
            IJobCancellationToken cancellationToken)
        {
            await Task.Run(() => throw new NotImplementedException("Not implemented for this provider"));
            return null!;
        }

        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var oldWayTask = HarvestObservationsOldWayAsync(cancellationToken);
            var newWayTask = HarvestObservationsNewWayAsync(cancellationToken);
            var results = await Task.WhenAll(oldWayTask, newWayTask);
            return await oldWayTask;
        }

        private async Task<HarvestInfo> HarvestObservationsOldWayAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo("iNaturallist", DateTime.Now);
            var dataProvider = new Lib.Models.Shared.DataProvider() { Id = 19, Identifier = "iNaturalist" };
            using var dwcCollectionRepository = new DwcCollectionRepository(dataProvider, _verbatimClient, _logger);
            dwcCollectionRepository.BeginTempMode();

            // Get current document count from permanent index
            var currentDocCount = await dwcCollectionRepository.OccurrenceRepository.CountAllDocumentsAsync();
            dwcCollectionRepository.BeginTempMode();

            try
            {
                _logger.LogInformation("Start harvesting sightings for iNaturalist data provider");
                _logger.LogInformation(GetINatHarvestSettingsInfoString());

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for iNaturalist verbatim collection");
                await dwcCollectionRepository.OccurrenceRepository.DeleteCollectionAsync();
                _logger.LogInformation("Finish empty collection for iNaturalist verbatim collection");

                var nrSightingsHarvested = 0;

                var startDate = new DateTime(_iNaturalistServiceConfiguration.StartHarvestYear, 1, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                var gBIFResult = await _iNaturalistObservationService.GetAsync(startDate, endDate);

                var id = 0;
                // Loop until all sightings are fetched.
                do
                {
                    _logger.LogDebug(
                        $"Fetching iNaturalist observations between dates {startDate.ToString("yyyy-MM-dd")} and {endDate.ToString("yyyy-MM-dd")}");

                    foreach (var observation in gBIFResult)
                    {
                        observation.Id = ++id;
                    }

                    // Add sightings to MongoDb
                    await dwcCollectionRepository.OccurrenceRepository.AddManyAsync(gBIFResult);

                    nrSightingsHarvested += gBIFResult.Count();

                    _logger.LogDebug($"{nrSightingsHarvested} iNaturalist observations harvested");

                    cancellationToken?.ThrowIfCancellationRequested();
                    if (_iNaturalistServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= _iNaturalistServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        _logger.LogInformation("Max iNaturalist observations reached");
                        break;
                    }

                    startDate = endDate.AddDays(1);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                    gBIFResult = await _iNaturalistObservationService.GetAsync(startDate, endDate);

                } while (gBIFResult != null && endDate <= DateTime.Now);

                _logger.LogInformation("Finished harvesting sightings for iNaturalist data provider");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Count = nrSightingsHarvested;

                if (nrSightingsHarvested >= currentDocCount * 0.8)
                {
                    harvestInfo.Status = RunStatus.Success;
                    _logger.LogInformation("Start permanentize temp collection for iNaturalist verbatim");
                    await dwcCollectionRepository.OccurrenceRepository.PermanentizeCollectionAsync();
                    _logger.LogInformation("Finish permanentize temp collection for iNaturalist verbatim");
                }
                else
                {
                    harvestInfo.Status = RunStatus.Failed;
                    _logger.LogError("iNaturalist: Previous harvested observation count is: {@currentDocCount}. Now only {@nrSightingsHarvested} observations where harvested.", currentDocCount, nrSightingsHarvested);
                }

            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("iNaturalist harvest was cancelled.");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest iNaturalist");
                harvestInfo.Status = RunStatus.Failed;
            }

            _logger.LogInformation("Finish harvesting sightings for iNaturalist data provider. Status={@harvestStatus}", harvestInfo.Status);
            return harvestInfo;
        }

        private async Task<HarvestInfo> HarvestObservationsNewWayAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo("iNaturallist", DateTime.Now);
            
            try
            {
                // If iNaturalist_full exists, rename it to iNaturalist_incremental_temp
                // Otherwise, create a copy of iNaturalist_incremental with the name iNaturalist_incremental_temp
                bool fullCollectionExists = await _iNaturalistCompleteRepository.CheckIfCollectionExistsAsync(FullCollectionName);
                bool incrementalCollectionExists = await _iNaturalistCompleteRepository.CheckIfCollectionExistsAsync(IncrementalCollectionName);
                if (!fullCollectionExists && !incrementalCollectionExists)
                {
                    _logger.LogError("{@dataProvider}: No full or incremental collection exists. Cannot harvest.", "iNaturalist");
                    harvestInfo.Status = RunStatus.Failed;
                    return harvestInfo;
                }
                if (fullCollectionExists)
                {
                    await _iNaturalistCompleteRepository.RenameCollectionAsync(FullCollectionName, IncrementalTempCollectionName);
                }
                else
                {
                    await _iNaturalistCompleteRepository.CopyCollectionAsync(IncrementalCollectionName, IncrementalTempCollectionName);
                }

                // Harvest latest day(s) observations
                var incrementalMongoCollection = _iNaturalistCompleteRepository.GetMongoCollection(IncrementalCollectionName);
                var incrementalTempMongoCollection = _iNaturalistCompleteRepository.GetMongoCollection(IncrementalTempCollectionName);
                _logger.LogInformation("Start harvesting incremental {@dataProvider} observations", "iNaturalist");
                _logger.LogInformation(GetINatHarvestSettingsInfoString());
                var nrSightingsHarvested = 0;
                DateTimeOffset? latestDate = await GetLatestUpdatedDate(incrementalTempMongoCollection);
                var startHarvestDate = latestDate ?? DateTimeOffset.UtcNow - TimeSpan.FromDays(7);                
                await foreach (var pageResult in _iNaturalistApiObservationService.GetByIterationAsync(startHarvestDate.DateTime, _iNaturalistServiceConfiguration.HarvestCompleBatchDelayInSeconds))
                {
                    if (pageResult.TotalCount > MaxNrIncrementalObservations)
                    {
                        _logger.LogWarning("Number of incremental {@dataProvider} observations exceeds limit of 100 000. Aborting harvest.", "iNaturalist");
                        break;
                    }
                    
                    await _iNaturalistCompleteRepository.UpsertManyAsync(pageResult.Observations, incrementalTempMongoCollection);
                    nrSightingsHarvested += pageResult.Observations.Count();

                    if (nrSightingsHarvested % 10000 == 0)
                    {
                        _logger.LogInformation("{nrSightingsHarvested} incremental {@dataProvider} observations harvested. Total count={totalCount}", nrSightingsHarvested, "iNaturalist", pageResult.TotalCount);
                    }

                    cancellationToken?.ThrowIfCancellationRequested();
                }

                _logger.LogInformation("Finished harvesting incremental {@dataProvider} observations", "iNaturalist");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                var tempDocCount = await _iNaturalistCompleteRepository.CountAllDocumentsAsync(incrementalTempMongoCollection);
                var currentDocCount = await _iNaturalistCompleteRepository.CountAllDocumentsAsync(incrementalMongoCollection);
                harvestInfo.Count = (int)tempDocCount;
                if (tempDocCount >= currentDocCount * 0.8)
                {
                    harvestInfo.Status = RunStatus.Success;
                    _logger.LogInformation("Start permanentize temp collection for incremental {@dataProvider} verbatim. Temp name={incrementalTempCollectionName}, New name={incrementalCollectionName}", "iNaturalist", IncrementalTempCollectionName, IncrementalCollectionName);
                    await _iNaturalistCompleteRepository.PermanentizeCollectionAsync(IncrementalTempCollectionName, IncrementalCollectionName);
                    _logger.LogInformation("Finish permanentize temp collection for incremental {@dataProvider} verbatim", "iNaturalist");
                }
                else
                {
                    harvestInfo.Status = RunStatus.Failed;
                    _logger.LogError("{@dataProvider}: Previous incremental harvested observation count is: {@currentDocCount}. Now only {@tempDocCount} observations where harvested.", "iNaturalist", currentDocCount, tempDocCount);
                }

            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("{@dataProvider} incremental harvest was cancelled.", "iNaturalist");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed incremental {@dataProvider} harvest", "iNaturalist");
                harvestInfo.Status = RunStatus.Failed;
            }

            _logger.LogInformation("Finish incremental harvesting observations for {@dataProvider} data provider. Status={@harvestStatus}", "iNaturalist", harvestInfo.Status);
            return harvestInfo;
        }

        public async Task<HarvestInfo> HarvestCompleteObservationsWithDelayAsync(Lib.Models.Shared.DataProvider provider, IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo("iNaturalist", DateTime.Now);

            try
            {
                var completeMongoCollection = _iNaturalistCompleteRepository.GetMongoCollection(FullCollectionName);
                var completeTempMongoCollection = _iNaturalistCompleteRepository.GetMongoCollection(FullTempCollectionName);
                int idAbove = _iNaturalistServiceConfiguration.HarvestCompleteStartId;                
                var currentDocCount = await _iNaturalistCompleteRepository.CountAllDocumentsAsync(completeMongoCollection);                
                (bool tempExists, int? latestId) = await CheckCompleteHarvestTempModeAsync(_iNaturalistCompleteRepository, completeTempMongoCollection);
                if (tempExists)
                {
                    // Continue harvest from last id
                    idAbove = latestId ?? idAbove;
                }
                else
                {                    
                    await _iNaturalistCompleteRepository.AddCollectionAsync(completeTempMongoCollection);
                }

                _logger.LogInformation("Start harvesting complete {@dataProvider} observations", "iNaturalist");
                _logger.LogInformation(GetINatHarvestSettingsInfoString());
                var nrSightingsHarvested = 0;                
                await foreach (var pageResult in _iNaturalistApiObservationService.GetByIterationAsync(idAbove, _iNaturalistServiceConfiguration.HarvestCompleBatchDelayInSeconds))
                {                    
                    await _iNaturalistCompleteRepository.AddManyAsync(pageResult.Observations, completeTempMongoCollection);
                    nrSightingsHarvested += pageResult.Observations.Count();

                    if (nrSightingsHarvested % 10000 == 0)
                    {
                        _logger.LogInformation("{nrSightingsHarvested} complete {@dataProvider} observations harvested. Total count={totalCount}", nrSightingsHarvested, "iNaturalist", pageResult.TotalCount);                        
                    }

                    cancellationToken?.ThrowIfCancellationRequested();
                }

                _logger.LogInformation("Finished harvesting complete {@dataProvider} observations", "iNaturalist");

                // Update harvest info
                harvestInfo.End = DateTime.Now;                
                var tempDocCount = await _iNaturalistCompleteRepository.CountAllDocumentsAsync(completeTempMongoCollection);
                harvestInfo.Count = (int)tempDocCount;                
                if (tempDocCount >= currentDocCount * 0.8)
                {
                    harvestInfo.Status = RunStatus.Success;                    
                    _logger.LogInformation("Start permanentize temp collection for complete {@dataProvider} verbatim. Temp name={incrementalTempCollectionName}, New name={incrementalCollectionName}", "iNaturalist", completeTempMongoCollection, completeMongoCollection);
                    await _iNaturalistCompleteRepository.PermanentizeCollectionAsync(completeTempMongoCollection, completeMongoCollection);
                    _logger.LogInformation("Finish permanentize temp collection for complete {@dataProvider} verbatim", "iNaturalist");
                }
                else
                {
                    harvestInfo.Status = RunStatus.Failed;
                    _logger.LogError("{@dataProvider}: Previous complete harvested observation count is: {@currentDocCount}. Now only {@tempDocCount} observations where harvested.", "iNaturalist", currentDocCount, tempDocCount);
                }
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("{@dataProvider} complete harvest was cancelled.", "iNaturalist");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed complete {@dataProvider} harvest", "iNaturalist");
                harvestInfo.Status = RunStatus.Failed;
            }

            _logger.LogInformation("Finish complete harvesting observations for {@dataProvider} data provider. Status={@harvestStatus}", "iNaturalist", harvestInfo.Status);
            return harvestInfo;
        }

        public Task<HarvestInfo> HarvestObservationsAsync(Lib.Models.Shared.DataProvider provider, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for this provider");
        }

        private async Task<DateTimeOffset?> GetLatestUpdatedDate(IMongoCollection<iNaturalistVerbatimObservation> incrementalTempMongoCollection)
        {
            try
            {
                var filter = Builders<iNaturalistVerbatimObservation>.Filter.Exists("Updated_at", true);
                var sort = Builders<iNaturalistVerbatimObservation>.Sort.Descending("Updated_at");
                var res = await incrementalTempMongoCollection
                    .Find(FilterDefinition<iNaturalistVerbatimObservation>.Empty)
                    .Sort(sort)
                    .Limit(1)
                    .FirstOrDefaultAsync();

                if (res == null)
                {
                    return null;
                }

                return res.Updated_at;
            }
            catch (Exception e)
            {
                // Log error and return null
                _logger.LogError(e, "Failed to get latest updated date from iNaturalist_incremental_temp");
                return null;
            }
        }        

        private string GetINatHarvestSettingsInfoString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("iNaturalist Harvest settings:");
            sb.AppendLine($"  Start Harvest Year: {_iNaturalistServiceConfiguration.StartHarvestYear}");
            sb.AppendLine(
                $"  Max Number Of Sightings Harvested: {_iNaturalistServiceConfiguration.MaxNumberOfSightingsHarvested}");
            sb.AppendLine($"  HarvestCompleteStartId: {_iNaturalistServiceConfiguration.HarvestCompleteStartId}");
            sb.AppendLine($"  HarvestCompleBatchDelayInSeconds: {_iNaturalistServiceConfiguration.HarvestCompleBatchDelayInSeconds}");
            return sb.ToString();
        }        

        private async Task<(bool tempExists, int? latestId)> CheckCompleteHarvestTempModeAsync(
            IiNaturalistCompleteObservationVerbatimRepository repository,
            MongoDB.Driver.IMongoCollection<iNaturalistVerbatimObservation> tempMongoCollection)
        {            
            bool collectionExists = await repository.CheckIfCollectionExistsAsync(tempMongoCollection.CollectionNamespace.CollectionName);
            if (collectionExists)
            {
                var maxId = await repository.GetMaxIdAsync(tempMongoCollection);                
                return (true, maxId);
            }
            
            return (false, null);
        }
    }
}