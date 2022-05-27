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
    public class SersObservationHarvester : ISersObservationHarvester
    {
        private readonly ILogger<SersObservationHarvester> _logger;
        private readonly ISersObservationService _sersObservationService;
        private readonly ISersObservationVerbatimRepository _sersObservationVerbatimRepository;
        private readonly SersServiceConfiguration _sersServiceConfiguration;

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
            ILogger<SersObservationHarvester> logger)
        {
            _sersObservationService =
                sersObservationService ?? throw new ArgumentNullException(nameof(sersObservationService));
            _sersObservationVerbatimRepository = sersObservationVerbatimRepository ??
                                                 throw new ArgumentNullException(
                                                     nameof(sersObservationVerbatimRepository));
            _sersServiceConfiguration = sersServiceConfiguration ??
                                        throw new ArgumentNullException(nameof(sersServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));            
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(DateTime.Now);
            _sersObservationVerbatimRepository.TempMode = true;

            try
            {
                _logger.LogInformation("Start harvesting sightings for SERS data provider");
                _logger.LogInformation(GetSersHarvestSettingsInfoString());

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for SERS verbatim collection");
                await _sersObservationVerbatimRepository.DeleteCollectionAsync();
                await _sersObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for SERS verbatim collection");

                var ns = (XNamespace) "http://schemas.datacontract.org/2004/07/ArtDatabanken.WebService.Data";
                var verbatimFactory = new AquaSupportHarvestFactory<SersObservationVerbatim>();
                var nrSightingsHarvested = 0;
                var startDate = new DateTime(_sersServiceConfiguration.StartHarvestYear, 1, 1);
                var endDate = startDate.AddYears(1).AddDays(-1);
                var changeId = 0L;
                var dataLastModified = DateTime.MinValue;

                // Loop until all sightings are fetched.
                while (startDate < DateTime.Now)
                {
                    var lastRequesetTime = DateTime.Now;
                    var xmlDocument = await _sersObservationService.GetAsync(startDate, endDate, changeId);
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
                            $"Fetching SERS observations between dates {startDate.ToString("yyyy-MM-dd")} and {endDate.ToString("yyyy-MM-dd")}, changeid: {changeId}");

                        var verbatims = await verbatimFactory.CastEntitiesToVerbatimsAsync(xmlDocument);
                        // Clean up
                        xmlDocument = null;

                        // Add sightings to MongoDb
                        await _sersObservationVerbatimRepository.AddManyAsync(verbatims);

                        nrSightingsHarvested += verbatims.Count();

                        _logger.LogDebug($"{nrSightingsHarvested} SERS observations harvested");

                        var batchDataLastModified = verbatims.Select(a => a.Modified).Max();

                        if (batchDataLastModified.HasValue && batchDataLastModified.Value > dataLastModified)
                        {
                            dataLastModified = batchDataLastModified.Value;
                        }

                        cancellationToken?.ThrowIfCancellationRequested();
                        if (_sersServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                            nrSightingsHarvested >= _sersServiceConfiguration.MaxNumberOfSightingsHarvested)
                        {
                            _logger.LogInformation("Max SERS observations reached");
                            break;
                        }
                    }

                    var timeSinceLastCall = (DateTime.Now - lastRequesetTime).Milliseconds;
                    if (timeSinceLastCall < 2000)
                    {
                        Thread.Sleep(2000 - timeSinceLastCall);
                    }
                }

                _logger.LogInformation("Finished harvesting sightings for SERS data provider");

                // Update harvest info
                harvestInfo.DataLastModified =
                    dataLastModified == DateTime.MinValue ? (DateTime?) null : dataLastModified;
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;

                _logger.LogInformation("Start permanentize temp collection for SERS verbatim");
                await _sersObservationVerbatimRepository.PermanentizeCollectionAsync();
                _logger.LogInformation("Finish permanentize temp collection for SERS verbatim");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("SERS harvest was cancelled.");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest SERS");
                harvestInfo.Status = RunStatus.Failed;
            }
            finally
            {
                _sersObservationVerbatimRepository.TempMode = false;
            }

            _logger.LogInformation($"Finish harvesting sightings for SERS data provider. Status={harvestInfo.Status}");
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
    }
}