using System.Text;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.Mvm.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Mvm;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Harvesters.Mvm
{
    public class MvmObservationHarvester : ObservationHarvesterBase<MvmObservationVerbatim, int>, IMvmObservationHarvester
    {
        private readonly IMvmObservationService _mvmObservationService;
        private readonly MvmServiceConfiguration _mvmServiceConfiguration;

        private string GetMvmHarvestSettingsInfoString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("MVM Harvest settings:");
            sb.AppendLine($"  Page size: {_mvmServiceConfiguration.MaxReturnedChangesInOnePage}");
            sb.AppendLine(
                $"  Max Number Of Sightings Harvested: {_mvmServiceConfiguration.MaxNumberOfSightingsHarvested}");
            return sb.ToString();
        }

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
            ILogger<MvmObservationHarvester> logger) : base("Mvm", mvmObservationVerbatimRepository, logger)
        {
            _mvmObservationService =
                mvmObservationService ?? throw new ArgumentNullException(nameof(mvmObservationService));
            _mvmServiceConfiguration = mvmServiceConfiguration ??
                                       throw new ArgumentNullException(nameof(mvmServiceConfiguration));           
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var runStatus = RunStatus.Success;
            var harvestCount = 0;
            (DateTime startDate, long preHarvestCount) initValues = (DateTime.Now, 0);
            try
            {
                initValues.preHarvestCount = await InitializeharvestAsync(true);
                Logger.LogInformation(GetMvmHarvestSettingsInfoString());

                var result = await _mvmObservationService.GetAsync(0);
               
                var dataLastModified = DateTime.MinValue;
                var verbatimFactory = new MvmHarvestFactory();

                // Loop until all sightings are fetched.
                while (result.MaxChangeId != 0)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    var verbatims = (await verbatimFactory.CastEntitiesToVerbatimsAsync(result.Observations))?.ToArray();
                    result.Observations = null;

                    harvestCount += verbatims.Length;

                    // Add sightings to MongoDb
                    await VerbatimRepository.AddManyAsync(verbatims);

                    var batchDataLastModified = verbatims.Select(a => a.Modified).Max();

                    if (batchDataLastModified.HasValue && batchDataLastModified.Value > dataLastModified)
                    {
                        dataLastModified = batchDataLastModified.Value;
                    }

                    if (_mvmServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        harvestCount >= _mvmServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        break;
                    }

                    // Give target service some slack
                    Thread.Sleep(1000);

                    result = await _mvmObservationService.GetAsync(result.MaxChangeId + 1);
                }
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("MVM harvest was cancelled.");
                runStatus = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to harvest MVM");
                runStatus = RunStatus.Failed;
            }

            return await FinishHarvestAsync(initValues, runStatus, harvestCount);
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