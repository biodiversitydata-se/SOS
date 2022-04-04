using System.Text;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.Mvm.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Harvesters.Mvm
{
    public class MvmObservationHarvester : IMvmObservationHarvester
    {
        private readonly IMvmObservationService _mvmObservationService;
        private readonly IMvmObservationVerbatimRepository _mvmObservationVerbatimRepository;
        private readonly MvmServiceConfiguration _mvmServiceConfiguration;
        private readonly ILogger<MvmObservationHarvester> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="mvmObservationService"></param>
        /// <param name="mvmObservationVerbatimRepository"></param>
        /// <param name="mvmServiceConfiguration"></param>
        /// <param name="logger"></param>
        public MvmObservationHarvester(
            IMvmObservationService mvmObservationService,
            IMvmObservationVerbatimRepository mvmObservationVerbatimRepository,
            MvmServiceConfiguration mvmServiceConfiguration,
            ILogger<MvmObservationHarvester> logger)
        {
            _mvmObservationService =
                mvmObservationService ?? throw new ArgumentNullException(nameof(mvmObservationService));
            _mvmObservationVerbatimRepository = mvmObservationVerbatimRepository ??
                                                throw new ArgumentNullException(
                                                    nameof(mvmObservationVerbatimRepository));
            _mvmServiceConfiguration = mvmServiceConfiguration ??
                                       throw new ArgumentNullException(nameof(mvmServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _mvmObservationVerbatimRepository.TempMode = true;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(DateTime.Now);

            try
            {
                _logger.LogInformation("Start harvesting sightings for MVM data provider");
                _logger.LogInformation(GetMvmHarvestSettingsInfoString());

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for MVM verbatim collection");
                await _mvmObservationVerbatimRepository.DeleteCollectionAsync();
                await _mvmObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for MVM verbatim collection");

                var nrSightingsHarvested = 0;
                var result = await _mvmObservationService.GetAsync(0);
               
                var dataLastModified = DateTime.MinValue;
                var verbatimFactory = new MvmHarvestFactory();

                // Loop until all sightings are fetched.
                while (result.MaxChangeId != 0)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    var verbatims = (await verbatimFactory.CastEntitiesToVerbatimsAsync(result.Observations))?.ToArray();
                    result.Observations = null;

                    nrSightingsHarvested += verbatims.Length;

                    // Add sightings to MongoDb
                    await _mvmObservationVerbatimRepository.AddManyAsync(verbatims);

                    var batchDataLastModified = verbatims.Select(a => a.Modified).Max();

                    if (batchDataLastModified.HasValue && batchDataLastModified.Value > dataLastModified)
                    {
                        dataLastModified = batchDataLastModified.Value;
                    }

                    if (_mvmServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= _mvmServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        break;
                    }

                    // Give target service some slack
                    Thread.Sleep(1000);

                    result = await _mvmObservationService.GetAsync(result.MaxChangeId + 1);
                }

                _logger.LogInformation("Finished harvesting sightings for MVM data provider");

                // Update harvest info
                harvestInfo.DataLastModified =
                    dataLastModified == DateTime.MinValue ? (DateTime?) null : dataLastModified;
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;

                _logger.LogInformation("Start permanentize temp collection for MVM verbatim");
                await _mvmObservationVerbatimRepository.PermanentizeCollectionAsync();
                _logger.LogInformation("Finish permanentize temp collection for MVM verbatim");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("MVM harvest was cancelled.");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest MVM");
                harvestInfo.Status = RunStatus.Failed;
            }

            _logger.LogInformation($"Finish harvesting sightings for MVM data provider. Status={harvestInfo.Status}");
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

        private string GetMvmHarvestSettingsInfoString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("MVM Harvest settings:");
            sb.AppendLine($"  Page size: {_mvmServiceConfiguration.MaxReturnedChangesInOnePage}");
            sb.AppendLine(
                $"  Max Number Of Sightings Harvested: {_mvmServiceConfiguration.MaxNumberOfSightingsHarvested}");
            return sb.ToString();
        }
    }
}