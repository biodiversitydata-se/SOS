using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.VirtualHerbarium.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Harvesters.VirtualHerbarium
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
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            // Get current document count from permanent index
            _virtualHerbariumObservationVerbatimRepository.TempMode = false;
            var currentDocCount = await _virtualHerbariumObservationVerbatimRepository.CountAllDocumentsAsync();

            var harvestInfo = new HarvestInfo("VirtualHerbarium", DateTime.Now);
            harvestInfo.Status = RunStatus.Failed;
            var occurrenceIdsSet = new HashSet<string>();
            _virtualHerbariumObservationVerbatimRepository.TempMode = true;

            try
            {
                _logger.LogInformation("Start harvesting sightings for Virtual Herbarium data provider");

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for Virtual Herbarium verbatim collection");
                await _virtualHerbariumObservationVerbatimRepository.DeleteCollectionAsync();
                await _virtualHerbariumObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for Virtual Herbarium verbatim collection");

                _logger.LogDebug($"Start getting Localities for Virtual Herbarium");
                var localitiesXml = await _virtualHerbariumObservationService.GetLocalitiesAsync();
                _logger.LogDebug($"Finish getting Localities for Virtual Herbarium");

                var verbatimFactory = new VirtualHerbariumHarvestFactory(localitiesXml);
                var pageIndex = 1;
                var nrSightingsHarvested = 0;
                var fromDate = new DateTime(1628, 1, 1);
                
                while (true)
                {
                    _logger.LogDebug($"Start getting Virtual Herbarium observations page: {pageIndex}");
                    var observations = await _virtualHerbariumObservationService.GetAsync(fromDate, pageIndex, _virtualHerbariumServiceConfiguration.MaxReturnedChangesInOnePage);
                    _logger.LogDebug($"Finish getting Virtual Herbarium observations page: {pageIndex}");

                    cancellationToken?.ThrowIfCancellationRequested();
                    _logger.LogDebug($"Start casting Virtual Herbarium observations page: {pageIndex}");
                    var verbatims = (await verbatimFactory.CastEntitiesToVerbatimsAsync(observations))?.ToArray();
                    observations = null;
                    _logger.LogDebug($"Finish casting Virtual Herbarium observations page: {pageIndex}");

                    if ((verbatims?.Length ?? 0) == 0)
                    {
                        break;
                    }

                    nrSightingsHarvested += verbatims.Count();

                    // Remove duplicates
                    var distinctVerbatims = new HashSet<VirtualHerbariumObservationVerbatim>();
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
                    _logger.LogDebug($"Start storing Virtual Herbarium observations page: {pageIndex}");
                    // Add sightings to MongoDb
                    await _virtualHerbariumObservationVerbatimRepository.AddManyAsync(distinctVerbatims);
                    _logger.LogDebug($"Finish storing Virtual Herbarium observations page: {pageIndex}");

                    if (_virtualHerbariumServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= _virtualHerbariumServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        break;
                    }

                    pageIndex++;
                }

                _logger.LogInformation("Finished harvesting sightings for Virtual Herbarium data provider");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Count = nrSightingsHarvested;

                if (nrSightingsHarvested >= currentDocCount * 0.8)
                {
                    harvestInfo.Status = RunStatus.Success;
                    _logger.LogInformation("Start permanentize temp collection for Virtual Herbarium verbatim");
                    await _virtualHerbariumObservationVerbatimRepository.PermanentizeCollectionAsync();
                    _logger.LogInformation("Finish permanentize temp collection for Virtual Herbarium verbatim");
                }
                else
                {
                    harvestInfo.Status = RunStatus.Failed;
                    _logger.LogError($"Virtual Herbarium: Previous harvested observation count is: {currentDocCount}. Now only {nrSightingsHarvested} observations where harvested.");
                }

                
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
            finally
            {
                _virtualHerbariumObservationVerbatimRepository.TempMode = false;
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