using System;
using System.Collections.Generic;
using System.Linq;
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

                var localitiesXml = await _virtualHerbariumObservationService.GetLocalitiesAsync();
                var verbatimFactory = new VirtualHerbariumHarvestFactory(localitiesXml);

                var pageIndex = 1;
                var nrSightingsHarvested = 0;
                var fromDate = new DateTime(1628, 1, 1);
                _logger.LogDebug($"Start getting observations page: {pageIndex}");
                var observations = await _virtualHerbariumObservationService.GetAsync(fromDate, pageIndex, 10000);
                _logger.LogDebug($"Finish getting observations page: {pageIndex}");

                while (true)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    var verbatims = (await verbatimFactory.CastEntitiesToVerbatimsAsync(observations))?.ToArray();

                    if ((verbatims?.Length ?? 0) == 0)
                    {
                        break;
                    }

                    nrSightingsHarvested += verbatims.Count();

                    // Remove duplicates
                    var distinctVerbatims = new List<VirtualHerbariumObservationVerbatim>();
                    foreach (var verbatim in verbatims)
                    {
                        var occurrenceId = $"{verbatim.InstitutionCode}#{verbatim.AccessionNo}#{verbatim.DyntaxaId}";
                        if (occurrenceIdsSet.Contains(occurrenceId))
                        {
                           _logger.LogWarning($"Duplicate observation found in Virtual Herbarium: {occurrenceId}");
                            continue;
                        }

                        occurrenceIdsSet.Add(occurrenceId);
                        distinctVerbatims.Add(verbatim);
                    }

                    // Add sightings to MongoDb
                    await _virtualHerbariumObservationVerbatimRepository.AddManyAsync(distinctVerbatims);

                    if (_virtualHerbariumServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= _virtualHerbariumServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        break;
                    }

                    pageIndex++;
                    _logger.LogDebug($"Start getting observations page: {pageIndex}");
                    observations =
                        await _virtualHerbariumObservationService.GetAsync(fromDate, pageIndex, 10000);
                    _logger.LogDebug($"Finish getting observations page: {pageIndex}");
                }

                _logger.LogInformation("Finished harvesting sightings for Virtual Herbarium data provider");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;

                _logger.LogInformation("Start permanentize temp collection for Virtual Herbarium verbatim");
                await _virtualHerbariumObservationVerbatimRepository.PermanentizeCollectionAsync();
                _logger.LogInformation("Finish permanentize temp collection for Virtual Herbarium verbatim");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Virtual Herbarium harvest was cancelled.");
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest Virtual Herbarium");
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Failed;
            }

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