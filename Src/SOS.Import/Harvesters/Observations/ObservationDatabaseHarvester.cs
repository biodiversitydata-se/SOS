using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SOS.Import.Entities.ObservationsDatabase;
using SOS.Import.Repositories.Source.ObservationsDatabase.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.ObservationDatabase;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Import.Harvesters.Observations
{
    /// <summary>
    ///     observation database observation harvester
    /// </summary>
    public class ObservationDatabaseHarvester : IObservationDatabaseHarvester
    {
        private readonly IObservationDatabaseRepository _observationDatabaseRepository;
        private readonly IObservationDatabaseVerbatimRepository _observationDatabaseVerbatimRepository;
        private readonly ObservationDatabaseConfiguration _observationDatabaseConfiguration;
        private readonly SemaphoreSlim _semaphore;
        private readonly ILogger<ObservationDatabaseHarvester> _logger;

        /// <summary>
        ///  Harvest a batch of sightings
        /// </summary>
        /// <param name="getChunkTask"></param>
        /// <param name="batchIndex"></param>
        /// <returns></returns>
        private async Task<int> HarvestBatchAsync(
            Task<IEnumerable<ObservationEntity>> getChunkTask,
            int batchIndex
        )
        {
            try
            {
                _logger.LogDebug(
                    $"Start getting observation database sightings ({batchIndex})");
                // Get chunk of observations
                var observations = (await getChunkTask)?.ToArray();
                _logger.LogDebug(
                    $"Finish getting observation database sightings ({batchIndex})");

                if (!observations?.Any() ?? true)
                {
                    _logger.LogDebug(
                    $"No observations found ({batchIndex})");
                    return 0;
                }

                _logger.LogDebug($"Start casting entities to verbatim ({batchIndex})");

                // Cast sightings to verbatim observations
                var verbatimObservations = observations.Select(e => CastEntityToVerbatim(e)); ;

                _logger.LogDebug($"Finish casting entities to verbatim ({batchIndex})");

                _logger.LogDebug($"Start storing batch ({batchIndex})");
                // Add sightings to mongodb

                await _observationDatabaseVerbatimRepository.AddManyAsync(verbatimObservations);
                _logger.LogDebug($"Finish storing batch ({batchIndex})");

                return verbatimObservations?.Count() ?? 0;
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    $"Harvest observation database sightings ({batchIndex})");
                throw;
            }
            finally
            {
                // Release semaphore in order to let next thread start getting data from source db 
                _semaphore.Release();
            }

            throw new Exception("Harvest observation database batch failed");
        }

        /// <summary>
        /// Cast sighting itemEntity to model .
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private ObservationDatabaseVerbatim CastEntityToVerbatim(ObservationEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            var observation = new ObservationDatabaseVerbatim
            {
                CollectionCode = entity.CollectionCode,
                CollectionId = entity.CollectionId,
                CoordinateUncertaintyInMeters = entity.CoordinateUncertaintyInMeters,
                CoordinateX = entity.CoordinateX,
                CoordinateY = entity.CoordinateY,
                County = entity.County,
                EditDate = entity.EditDate,
                EndDate = entity.EndDate,
                Habitat = entity.Habitat,
                Id = entity.Id,
                IndividualCount = entity.IndividualCount,
                IsNeverFoundObservation = entity.IsNeverFoundObservation,
                IsNotRediscoveredObservation = entity.IsNotRediscoveredObservation,
                Locality = entity.Locality,
                Municipality = entity.Municipality,
                OccurrenceRemarks = entity.OccurrenceRemarks,
                Observers = entity.Observers,
                Origin = entity.Origin,
                Parish = entity.Parish,
                ProtectionLevel = entity.ProtectionLevel,
                Province = entity.Province,
                RegisterDate = entity.RegisterDate,
                SCI_code = entity.SCI_code,
                SCI_name = entity.SCI_name,
                Stadium = entity.Stadium,
                StartDate = entity.StartDate,
                Substrate = entity.Substrate,
                TaxonId = entity.TaxonId,
                VerifiedBy = entity.VerifiedBy
            };

            return observation;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationDatabaseRepository"></param>
        /// <param name="observationDatabaseVerbatimRepository"></param>
        /// <param name="observationDatabaseConfiguration"></param>
        /// <param name="logger"></param>
        public ObservationDatabaseHarvester(
            IObservationDatabaseRepository observationDatabaseRepository,
            IObservationDatabaseVerbatimRepository observationDatabaseVerbatimRepository,
            ObservationDatabaseConfiguration observationDatabaseConfiguration,
            ILogger<ObservationDatabaseHarvester> logger)
        {
            _observationDatabaseRepository = observationDatabaseRepository ?? throw new ArgumentNullException(nameof(observationDatabaseRepository));
            _observationDatabaseVerbatimRepository = observationDatabaseVerbatimRepository ?? throw new ArgumentNullException(nameof(observationDatabaseVerbatimRepository));
            _observationDatabaseConfiguration = observationDatabaseConfiguration ??
                                                throw new ArgumentNullException(nameof(observationDatabaseConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _semaphore = new SemaphoreSlim(_observationDatabaseConfiguration.NoOfThreads);

            _observationDatabaseVerbatimRepository.TempMode = true;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(DateTime.Now);
            try
            {
                _logger.LogInformation("Start harvesting sightings for observation database provider");
                await _observationDatabaseVerbatimRepository.DeleteCollectionAsync();
                await _observationDatabaseVerbatimRepository.AddCollectionAsync();

                var nrObservationsHarvested = 0;
                var (minId, maxId) = await _observationDatabaseRepository.GetIdSpanAsync();

                if (maxId > minId)
                {
                    var currentId = minId;
                    var harvestBatchTasks = new List<Task<int>>();

                    _logger.LogDebug($"Start getting observation database observations");

                    var batchIndex = 0;
                    // Loop until all sightings are fetched
                    while (currentId <= maxId)
                    {
                        cancellationToken?.ThrowIfCancellationRequested();
                        if (_observationDatabaseConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                            currentId - minId >= _observationDatabaseConfiguration.MaxNumberOfSightingsHarvested)
                        {
                            break;
                        }

                        await _semaphore.WaitAsync();
                        batchIndex++;

                        // Add batch task to list
                        harvestBatchTasks.Add(HarvestBatchAsync(
                            _observationDatabaseRepository.GetChunkAsync(currentId,
                                _observationDatabaseConfiguration.ChunkSize), batchIndex));

                        // Calculate start of next chunk
                        currentId += _observationDatabaseConfiguration.ChunkSize;
                    }

                    // Execute harvest tasks, no of parallel threads running is handled by semaphore
                    await Task.WhenAll(harvestBatchTasks);

                    // Sum each batch harvested
                    nrObservationsHarvested = harvestBatchTasks.Sum(t => t.Result);

                    _logger.LogDebug($"Finish getting observation database sightings ({nrObservationsHarvested})");
                }

                cancellationToken?.ThrowIfCancellationRequested();

                _logger.LogInformation("Finished harvesting sightings for observation database data provider");

                // Update harvest info
                harvestInfo.DataLastModified = await _observationDatabaseRepository.GetLastModifiedDateAsyc();
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrObservationsHarvested;

                _logger.LogInformation("Start permanentize temp collection for observation database verbatim");
                await _observationDatabaseVerbatimRepository.PermanentizeCollectionAsync();
                _logger.LogInformation("Finish permanentize temp collection for observation database verbatim");
            }
            catch (JobAbortedException e)
            {
                _logger.LogError(e, "Canceled harvest of observation database");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest of observation database");
                harvestInfo.Status = RunStatus.Failed;
            }

            _logger.LogInformation($"Finish harvesting sightings for observation database provider. Status={harvestInfo.Status}");
            return harvestInfo;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for this provider");
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider provider, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for this provider");
        }
    }
}