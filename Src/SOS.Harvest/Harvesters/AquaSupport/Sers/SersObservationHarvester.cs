using System.Text;
using System.Xml.Linq;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.HarvestersAquaSupport.Sers.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Harvesters.AquaSupport;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Sers;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Harvesters.AquaSupport.Sers
{
    public class SersObservationHarvester : ObservationHarvesterBase<SersObservationVerbatim, int>, ISersObservationHarvester
    {
        private readonly ISersObservationService _sersObservationService;
        private readonly SersServiceConfiguration _sersServiceConfiguration;

        private string GetSersHarvestSettingsInfoString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SERS Harvest settings:");
            sb.AppendLine($"  Start Harvest Year: {_sersServiceConfiguration.StartHarvestYear}");
            if (_sersServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue)
            {
                sb.AppendLine(
                    $"  Max Number Of Sightings Harvested: {_sersServiceConfiguration.MaxNumberOfSightingsHarvested}");
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="sersObservationService"></param>
        /// <param name="sersObservationVerbatimRepository"></param>
        /// <param name="sersServiceConfiguration"></param>
        /// <param name="logger"></param>
        public SersObservationHarvester(
            ISersObservationService sersObservationService,
            ISersObservationVerbatimRepository sersObservationVerbatimRepository,
            SersServiceConfiguration sersServiceConfiguration,
            ILogger<SersObservationHarvester> logger) : base("SERS", sersObservationVerbatimRepository, logger)
        {
            _sersObservationService =
                sersObservationService ?? throw new ArgumentNullException(nameof(sersObservationService));
            _sersServiceConfiguration = sersServiceConfiguration ??
                                        throw new ArgumentNullException(nameof(sersServiceConfiguration));  
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var runStatus = RunStatus.Success;
            var harvestCount = 0;
            (DateTime startDate, long preHarvestCount) initValues = (DateTime.Now, 0);
            try
            {
                initValues.preHarvestCount = await InitializeHarvestAsync(true);
                Logger.LogInformation(GetSersHarvestSettingsInfoString());

                var ns = (XNamespace) "http://schemas.datacontract.org/2004/07/ArtDatabanken.WebService.Data";
                var verbatimFactory = new AquaSupportHarvestFactory<SersObservationVerbatim>();
                var startDate = new DateTime(_sersServiceConfiguration.StartHarvestYear, 1, 1);
                var endDate = DateTime.Now;
                var changeId = 0L;
                var dataLastModified = DateTime.MinValue;

                var xmlDocument = await _sersObservationService.GetAsync(startDate, endDate, changeId);
                changeId = long.Parse(xmlDocument?.Descendants(ns + "MaxChangeId")?.FirstOrDefault()?.Value ?? "0");

                // Loop until all sightings are fetched.
                while (changeId != 0)
                {
                    var lastRequesetTime = DateTime.Now;

                    Logger.LogDebug(
                           $"Fetching SERS observations between dates {startDate.ToString("yyyy-MM-dd")} and {endDate.ToString("yyyy-MM-dd")}, changeid: {changeId}");

                    var verbatims = await verbatimFactory.CastEntitiesToVerbatimsAsync(xmlDocument);
                    // Clean up
                    xmlDocument = null;

                    // Add sightings to MongoDb
                    await VerbatimRepository.AddManyAsync(verbatims);

                    harvestCount += verbatims.Count();

                    Logger.LogDebug($"{harvestCount} SERS observations harvested");

                    var batchDataLastModified = verbatims.Select(a => a.Modified).Max();

                    if (batchDataLastModified.HasValue && batchDataLastModified.Value > dataLastModified)
                    {
                        dataLastModified = batchDataLastModified.Value;
                    }

                    cancellationToken?.ThrowIfCancellationRequested();
                    if (_sersServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        harvestCount >= _sersServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        Logger.LogInformation("Max SERS observations reached");
                        break;
                    }

                    var timeSinceLastCall = (DateTime.Now - lastRequesetTime).Milliseconds;
                    if (timeSinceLastCall < 2000)
                    {
                        Thread.Sleep(2000 - timeSinceLastCall);
                    }

                    xmlDocument = await _sersObservationService.GetAsync(startDate, endDate, changeId);
                    changeId = long.Parse(xmlDocument?.Descendants(ns + "MaxChangeId")?.FirstOrDefault()?.Value ?? "0");
                }
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("SERS harvest was cancelled.");
                runStatus = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to harvest SERS");
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