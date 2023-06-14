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
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Harvesters.VirtualHerbarium
{
    public class VirtualHerbariumObservationHarvester : ObservationHarvesterBase<VirtualHerbariumObservationVerbatim, int>, IVirtualHerbariumObservationHarvester
    {
        private readonly IVirtualHerbariumObservationService _virtualHerbariumObservationService;
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
            ILogger<VirtualHerbariumObservationHarvester> logger) : base("Virtual Herbarium", virtualHerbariumObservationVerbatimRepository, logger)
        {
            _virtualHerbariumObservationService = virtualHerbariumObservationService ??
                                                  throw new ArgumentNullException(
                                                      nameof(virtualHerbariumObservationService));
            _virtualHerbariumServiceConfiguration = virtualHerbariumServiceConfiguration ??
                                                    throw new ArgumentNullException(
                                                        nameof(virtualHerbariumServiceConfiguration));           
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var occurrenceIdsSet = new HashSet<string>();
            var runStatus = RunStatus.Success;
            var harvestCount = 0;
            (DateTime startDate, long preHarvestCount) initValues = (DateTime.Now, 0);
            try
            {
                initValues.preHarvestCount = await InitializeHarvestAsync(true);

                Logger.LogDebug($"Start getting Localities for Virtual Herbarium");
                var localitiesXml = await _virtualHerbariumObservationService.GetLocalitiesAsync();
                Logger.LogDebug($"Finish getting Localities for Virtual Herbarium");

                var verbatimFactory = new VirtualHerbariumHarvestFactory(localitiesXml!);
                var pageIndex = 1;
                var fromDate = new DateTime(1628, 1, 1);
                
                while (true)
                {
                    Logger.LogDebug($"Start getting Virtual Herbarium observations page: {pageIndex}");
                    var observations = await _virtualHerbariumObservationService.GetAsync(fromDate, pageIndex, _virtualHerbariumServiceConfiguration.MaxReturnedChangesInOnePage);
                    Logger.LogDebug($"Finish getting Virtual Herbarium observations page: {pageIndex}");

                    cancellationToken?.ThrowIfCancellationRequested();
                    Logger.LogDebug($"Start casting Virtual Herbarium observations page: {pageIndex}");
                    var verbatims = (await verbatimFactory.CastEntitiesToVerbatimsAsync(observations!))?.ToArray();
                    observations = null;
                    Logger.LogDebug($"Finish casting Virtual Herbarium observations page: {pageIndex}");

                    if ((verbatims?.Length ?? 0) == 0)
                    {
                        break;
                    }

                    harvestCount += verbatims!.Count();

                    // Remove duplicates
                    var distinctVerbatims = new HashSet<VirtualHerbariumObservationVerbatim>();
                    foreach (var verbatim in verbatims!)
                    {
                        var occurrenceId = $"{verbatim.InstitutionCode}#{verbatim.AccessionNo}#{verbatim.DyntaxaId}";
                        if (occurrenceIdsSet.Contains(occurrenceId))
                        {
                            Logger.LogWarning($"Duplicate observation found in Virtual Herbarium: {occurrenceId}");
                            continue;
                        }

                        occurrenceIdsSet.Add(occurrenceId);
                        distinctVerbatims.Add(verbatim);
                    }
                    Logger.LogDebug($"Start storing Virtual Herbarium observations page: {pageIndex}");
                    // Add sightings to MongoDb
                    await VerbatimRepository.AddManyAsync(distinctVerbatims);
                    Logger.LogDebug($"Finish storing Virtual Herbarium observations page: {pageIndex}");

                    if (_virtualHerbariumServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        harvestCount >= _virtualHerbariumServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        break;
                    }

                    pageIndex++;
                } 
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("Virtual Herbarium harvest was cancelled.");
                runStatus = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to harvest Virtual Herbarium");
                runStatus = RunStatus.Failed;
            }

            return await FinishHarvestAsync(initValues, runStatus, harvestCount);
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(JobRunModes mode,
            DateTime? fromDate,
            IJobCancellationToken cancellationToken)
        {
            await Task.Run(() => throw new NotImplementedException("Not implemented for this provider"));
            return null!;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider provider, IJobCancellationToken cancellationToken)
        {
            await Task.Run(() => throw new NotImplementedException("Not implemented for this provider"));
            return null!;
        }
    }
}