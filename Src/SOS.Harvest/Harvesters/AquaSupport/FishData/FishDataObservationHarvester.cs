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
    public class FishDataObservationHarvester : IFishDataObservationHarvester
    {
        private readonly IFishDataObservationService _fishDataObservationService;
        private readonly IFishDataObservationVerbatimRepository _fishDataObservationVerbatimRepository;
        private readonly FishDataServiceConfiguration _fishDataServiceConfiguration;
        private readonly ILogger<FishDataObservationHarvester> _logger;

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
            ILogger<FishDataObservationHarvester> logger)
        {
            _fishDataObservationService =
                fishDataObservationService ?? throw new ArgumentNullException(nameof(fishDataObservationService));
            _fishDataObservationVerbatimRepository = fishDataObservationVerbatimRepository ??
                                                throw new ArgumentNullException(
                                                    nameof(fishDataObservationVerbatimRepository));
            _fishDataServiceConfiguration = fishDataServiceConfiguration ??
                                       throw new ArgumentNullException(nameof(fishDataServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));            
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(DateTime.Now);
            _fishDataObservationVerbatimRepository.TempMode = true;

            try
            {
                _logger.LogInformation("Start harvesting sightings for Fish Data data provider");
                _logger.LogInformation(GetFishDataHarvestSettingsInfoString());

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for Fish Data verbatim collection");
                await _fishDataObservationVerbatimRepository.DeleteCollectionAsync();
                await _fishDataObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for Fish Data verbatim collection");

                var ns = (XNamespace) "http://schemas.datacontract.org/2004/07/ArtDatabanken.WebService.Data";
                var verbatimFactory = new AquaSupportHarvestFactory<FishDataObservationVerbatim>();

                var nrSightingsHarvested = 0;
                var startDate = new DateTime(_fishDataServiceConfiguration.StartHarvestYear, 1, 1);
                var endDate = startDate.AddYears(1).AddDays(-1);
                var changeId = 0L;
                var dataLastModified = DateTime.MinValue;

                // Loop until all sightings are fetched.
                while (startDate < DateTime.Now)
                {
                    var lastRequesetTime = DateTime.Now;
                    var xmlDocument = await _fishDataObservationService.GetAsync(startDate, endDate, changeId);
                    changeId = long.Parse(xmlDocument?.Descendants(ns + "MaxChangeId")?.FirstOrDefault()?.Value ?? "0");

                    // Change id equals 0 => nothing more to fetch for this year
                    if (changeId.Equals(0))
                    {
                        startDate = endDate.AddDays(1);
                        endDate = startDate.AddYears(1).AddDays(-1);
                    }
                    else
                    {
                        _logger.LogDebug(
                            $"Fetching Fish data observations between dates {startDate.ToString("yyyy-MM-dd")} and {endDate.ToString("yyyy-MM-dd")}, changeid: {changeId}");

                        var verbatims = await verbatimFactory.CastEntitiesToVerbatimsAsync(xmlDocument);
                        // Clean up
                        xmlDocument = null;

                        // Add sightings to MongoDb
                        await _fishDataObservationVerbatimRepository.AddManyAsync(verbatims);

                        nrSightingsHarvested += verbatims.Count();

                        _logger.LogDebug($"{nrSightingsHarvested} Fish data observations harvested");

                        var batchDataLastModified = verbatims.Select(a => a.Modified).Max();

                        if (batchDataLastModified.HasValue && batchDataLastModified.Value > dataLastModified)
                        {
                            dataLastModified = batchDataLastModified.Value;
                        }

                        cancellationToken?.ThrowIfCancellationRequested();
                        if (_fishDataServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                            nrSightingsHarvested >= _fishDataServiceConfiguration.MaxNumberOfSightingsHarvested)
                        {
                            _logger.LogInformation("Max Fish data observations reached");
                            break;
                        }
                    }

                    var timeSinceLastCall = (DateTime.Now - lastRequesetTime).Milliseconds;
                    if (timeSinceLastCall < 2000)
                    {
                        Thread.Sleep(2000 - timeSinceLastCall);
                    }
                }

                _logger.LogInformation("Finished harvesting sightings for Fish Data data provider");

                // Update harvest info
                harvestInfo.DataLastModified =
                    dataLastModified == DateTime.MinValue ? (DateTime?)null : dataLastModified;
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;

                _logger.LogInformation("Start permanentize temp collection for Fish Data verbatim");
                await _fishDataObservationVerbatimRepository.PermanentizeCollectionAsync();
                _logger.LogInformation("Finish permanentize temp collection for Fish Data verbatim");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Fish Data harvest was cancelled.");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest Fish Data");
                harvestInfo.Status = RunStatus.Failed;
            }
            finally
            {
                _fishDataObservationVerbatimRepository.TempMode = false;
            }

            _logger.LogInformation($"Finish harvesting sightings for Fish Data data provider. Status={harvestInfo.Status}");
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
    }
}