using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Harvest;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Import.Harvesters.Observations
{
    public class VirtualHerbariumObservationHarvester : IVirtualHerbariumObservationHarvester
    {
        private readonly ILogger<VirtualHerbariumObservationHarvester> _logger;
        private readonly IVirtualHerbariumObservationService _virtualHerbariumObservationService;
        private readonly IVirtualHerbariumObservationVerbatimRepository _virtualHerbariumObservationVerbatimRepository;
        private readonly VirtualHerbariumServiceConfiguration _virtualHerbariumServiceConfiguration;
        private ConcurrentBag<string> _occurrenceIdsSet;


        private async Task<int> HarvestBatchAsync(VirtualHerbariumHarvestFactory verbatimFactory, DateTime fromDate, int pageIndex, IJobCancellationToken cancellationToken)
        {
            _logger.LogDebug($"Start getting Virtual Herbarium observations page: {pageIndex}");
            var observations = await _virtualHerbariumObservationService.GetAsync(fromDate, pageIndex, 10000);
            _logger.LogDebug($"Finish getting Virtual Herbarium observations page: {pageIndex}");

            cancellationToken?.ThrowIfCancellationRequested();

            _logger.LogDebug($"Start casting Virtual Herbarium observations page: {pageIndex}");
            var verbatims = (await verbatimFactory.CastEntitiesToVerbatimsAsync(observations))?.ToArray();
            _logger.LogDebug($"Finish casting Virtual Herbarium observations page: {pageIndex}");

            cancellationToken?.ThrowIfCancellationRequested();

            if ((verbatims?.Length ?? 0) == 0)
            {
                return 0;
            }
            
            // Remove duplicates
            var distinctVerbatims = new HashSet<VirtualHerbariumObservationVerbatim>();
            foreach (var verbatim in verbatims)
            {
                var occurrenceId = $"{verbatim.InstitutionCode}-{verbatim.AccessionNo}-{verbatim.DyntaxaId}";
                if (_occurrenceIdsSet.Contains(occurrenceId))
                {
                    _logger.LogWarning($"Duplicate observation found in Virtual Herbarium: {occurrenceId}");
                    continue;
                }

                _occurrenceIdsSet.Add(occurrenceId);
                distinctVerbatims.Add(verbatim);
            }

            cancellationToken?.ThrowIfCancellationRequested();

            _logger.LogDebug($"Start storing Virtual Herbarium observations page: {pageIndex}");
            // Add sightings to MongoDb
            await _virtualHerbariumObservationVerbatimRepository.AddManyAsync(distinctVerbatims);
            _logger.LogDebug($"Finish storing Virtual Herbarium observations page: {pageIndex}");

            return verbatims.Count();
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="virtualHerbariumObservationService"></param>
        /// <param name="virtualHerbariumObservationVerbatimRepository"></param>
        /// <param name="virtualHerbariumServiceConfiguration"></param>
        /// <param name="logger"></param>
        public VirtualHerbariumObservationHarvester(
            IVirtualHerbariumObservationService virtualHerbariumObservationService,
            IVirtualHerbariumObservationVerbatimRepository virtualHerbariumObservationVerbatimRepository,
            VirtualHerbariumServiceConfiguration virtualHerbariumServiceConfiguration,
            ILogger<VirtualHerbariumObservationHarvester> logger)
        {
            _virtualHerbariumObservationService = virtualHerbariumObservationService ??
                                                  throw new ArgumentNullException(
                                                      nameof(virtualHerbariumObservationService));
            _virtualHerbariumObservationVerbatimRepository = virtualHerbariumObservationVerbatimRepository ??
                                                             throw new ArgumentNullException(
                                                                 nameof(virtualHerbariumObservationVerbatimRepository));
            _virtualHerbariumServiceConfiguration = virtualHerbariumServiceConfiguration ??
                                                    throw new ArgumentNullException(
                                                        nameof(virtualHerbariumServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _virtualHerbariumObservationVerbatimRepository.TempMode = true;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(DateTime.Now);
            harvestInfo.Status = RunStatus.Failed;
            var occurrenceIdsSet = new HashSet<string>();

            try
            {
                _logger.LogInformation("Start harvesting sightings for Virtual Herbarium data provider");

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for Virtual Herbarium verbatim collection");
                await _virtualHerbariumObservationVerbatimRepository.DeleteCollectionAsync();
                await _virtualHerbariumObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for Virtual Herbarium verbatim collection");

                _logger.LogInformation("Start getting localities for Virtual Herbarium");
                var localitiesXml = await _virtualHerbariumObservationService.GetLocalitiesAsync();
                var verbatimFactory = new VirtualHerbariumHarvestFactory(localitiesXml);
                _logger.LogInformation("Finish getting localities for Virtual Herbarium");

                var pageIndex = 1;
                var nrSightingsHarvested = 0;
                var fromDate = new DateTime(1628, 1, 1);
                var keepHarvesting = true;
                _occurrenceIdsSet = new ConcurrentBag<string>();
 
                while (keepHarvesting)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    var harvestTasks = new List<Task<int>>();
                    for (var i = 0; i < 10; i++)
                    {
                        if (_virtualHerbariumServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue && pageIndex > 1 &&
                            _virtualHerbariumServiceConfiguration.MaxReturnedChangesInOnePage * pageIndex >= _virtualHerbariumServiceConfiguration.MaxNumberOfSightingsHarvested)
                        {
                            keepHarvesting = false;
                            break;
                        }

                        harvestTasks.Add(HarvestBatchAsync(verbatimFactory, fromDate, pageIndex, cancellationToken));
                        pageIndex++;
                    }

                    // Run 10 tasks in parallel 
                    Parallel.ForEach(harvestTasks, harvestTask =>
                    {
                        if (harvestTask.Result.Equals(0))
                        {
                            keepHarvesting = false;
                            cancellationToken = new JobCancellationToken(true);
                        }

                        nrSightingsHarvested += harvestTask.Result;
                    });
                }

                _logger.LogInformation("Finished harvesting sightings for Virtual Herbarium data provider");

                // Update harvest info
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;

                _logger.LogInformation("Start permanentize temp collection for Virtual Herbarium verbatim");
                await _virtualHerbariumObservationVerbatimRepository.PermanentizeCollectionAsync();
                _logger.LogInformation("Finish permanentize temp collection for Virtual Herbarium verbatim");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Virtual Herbarium harvest was cancelled.");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest Virtual Herbarium");
                harvestInfo.Status = RunStatus.Failed;
            }
            finally
            {
                harvestInfo.End = DateTime.Now;
            }

            _logger.LogInformation($"Finish harvesting sightings for Virtual Herbarium data provider. Status={harvestInfo.Status}");
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