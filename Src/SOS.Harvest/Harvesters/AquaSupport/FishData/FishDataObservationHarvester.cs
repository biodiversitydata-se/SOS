using System.Text;
using System.Xml.Linq;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.AquaSupport.FishData.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Harvesters.AquaSupport;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.FishData;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Harvesters.AquaSupport.FishData
{
    public class FishDataObservationHarvester : ObservationHarvesterBase<FishDataObservationVerbatim, int>, IFishDataObservationHarvester
    {
        private readonly IFishDataObservationService _fishDataObservationService;
        private readonly FishDataServiceConfiguration _fishDataServiceConfiguration;

        private string GetFishDataHarvestSettingsInfoString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Fish Data Harvest settings:");
            sb.AppendLine($"  Start Harvest Year: {_fishDataServiceConfiguration.StartHarvestYear}");
            if (_fishDataServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue)
            {
                sb.AppendLine(
                    $"  Max Number Of Sightings Harvested: {_fishDataServiceConfiguration.MaxNumberOfSightingsHarvested}");
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="fishDataObservationService"></param>
        /// <param name="fishDataObservationVerbatimRepository"></param>
        /// <param name="fishDataServiceConfiguration"></param>
        /// <param name="logger"></param>
        public FishDataObservationHarvester(
            IFishDataObservationService fishDataObservationService,
            IFishDataObservationVerbatimRepository fishDataObservationVerbatimRepository,
            FishDataServiceConfiguration fishDataServiceConfiguration,
            ILogger<FishDataObservationHarvester> logger) : base("FishData2", fishDataObservationVerbatimRepository, logger)
        {
            _fishDataObservationService =
                fishDataObservationService ?? throw new ArgumentNullException(nameof(fishDataObservationService));
            
            _fishDataServiceConfiguration = fishDataServiceConfiguration ??
                                       throw new ArgumentNullException(nameof(fishDataServiceConfiguration));
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
                Logger.LogInformation(GetFishDataHarvestSettingsInfoString());

                var ns = (XNamespace) "http://schemas.datacontract.org/2004/07/ArtDatabanken.WebService.Data";
                var verbatimFactory = new AquaSupportHarvestFactory<FishDataObservationVerbatim>();
                var startDate = new DateTime(_fishDataServiceConfiguration.StartHarvestYear, 1, 1);
                var endDate = DateTime.Now;
                var changeId = 0L;
                var dataLastModified = DateTime.MinValue;

                var xmlDocument = await _fishDataObservationService.GetAsync(startDate, endDate, changeId);
                changeId = long.Parse(xmlDocument?.Descendants(ns + "MaxChangeId")?.FirstOrDefault()?.Value ?? "0");

                // Loop until all sightings are fetched.
                while (changeId != 0)
                {
                    var lastRequesetTime = DateTime.Now;
                    
                    Logger.LogDebug(
                        $"Fetching Fish data observations between dates {startDate.ToString("yyyy-MM-dd")} and {endDate.ToString("yyyy-MM-dd")}, changeid: {changeId}");

                    var verbatims = await verbatimFactory.CastEntitiesToVerbatimsAsync(xmlDocument);
                    // Clean up
                    xmlDocument = null;

                    // Add sightings to MongoDb
                    await VerbatimRepository.AddManyAsync(verbatims);

                    harvestCount += verbatims.Count();

                    Logger.LogDebug($"{harvestCount} Fish data observations harvested");

                    var batchDataLastModified = verbatims.Select(a => a.Modified).Max();

                    if (batchDataLastModified.HasValue && batchDataLastModified.Value > dataLastModified)
                    {
                        dataLastModified = batchDataLastModified.Value;
                    }

                    cancellationToken?.ThrowIfCancellationRequested();
                    if (_fishDataServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        harvestCount >= _fishDataServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        Logger.LogInformation("Max Fish data observations reached");
                        break;
                    }

                    var timeSinceLastCall = (DateTime.Now - lastRequesetTime).Milliseconds;
                    if (timeSinceLastCall < 2000)
                    {
                        Thread.Sleep(2000 - timeSinceLastCall);
                    }

                    xmlDocument = await _fishDataObservationService.GetAsync(startDate, endDate, changeId);
                    changeId = long.Parse(xmlDocument?.Descendants(ns + "MaxChangeId")?.FirstOrDefault()?.Value ?? "0");
                }
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("Fish Data harvest was cancelled.");
                runStatus = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to harvest Fish Data");
                runStatus = RunStatus.Failed;
            }

            return await FinishHarvestAsync(initValues, runStatus, harvestCount);
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(JobRunModes mode,
            DateTime? fromDate,
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