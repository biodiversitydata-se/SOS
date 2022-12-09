using System.Text;
using System.Xml.Linq;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.AquaSupport.Nors.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Harvesters.AquaSupport;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Nors;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Harvesters.AquaSupport.Nors
{
    public class NorsObservationHarvester : INorsObservationHarvester
    {
        private readonly ILogger<NorsObservationHarvester> _logger;
        private readonly INorsObservationService _norsObservationService;
        private readonly INorsObservationVerbatimRepository _norsObservationVerbatimRepository;
        private readonly NorsServiceConfiguration _norsServiceConfiguration;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="norsObservationService"></param>
        /// <param name="norsObservationVerbatimRepository"></param>
        /// <param name="norsServiceConfiguration"></param>
        /// <param name="logger"></param>
        public NorsObservationHarvester(
            INorsObservationService norsObservationService,
            INorsObservationVerbatimRepository norsObservationVerbatimRepository,
            NorsServiceConfiguration norsServiceConfiguration,
            ILogger<NorsObservationHarvester> logger)
        {
            _norsObservationService =
                norsObservationService ?? throw new ArgumentNullException(nameof(norsObservationService));
            _norsObservationVerbatimRepository = norsObservationVerbatimRepository ??
                                                 throw new ArgumentNullException(
                                                     nameof(norsObservationVerbatimRepository));
            _norsServiceConfiguration = norsServiceConfiguration ??
                                        throw new ArgumentNullException(nameof(norsServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            // Get current document count from permanent index
            _norsObservationVerbatimRepository.TempMode = false;
            var currentDocCount = await _norsObservationVerbatimRepository.CountAllDocumentsAsync();

            var harvestInfo = new HarvestInfo("NORS", DateTime.Now);
            _norsObservationVerbatimRepository.TempMode = true;

            try
            {
                _logger.LogInformation("Start harvesting sightings for NORS data provider");
                _logger.LogInformation(GetNorsHarvestSettingsInfoString());

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for NORS verbatim collection");
                await _norsObservationVerbatimRepository.DeleteCollectionAsync();
                await _norsObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for NORS verbatim collection");

                var ns = (XNamespace) "http://schemas.datacontract.org/2004/07/ArtDatabanken.WebService.Data";
                var verbatimFactory = new AquaSupportHarvestFactory<NorsObservationVerbatim>();

                var nrSightingsHarvested = 0;
                var startDate = new DateTime(_norsServiceConfiguration.StartHarvestYear, 1, 1);
                var endDate = startDate.AddYears(1).AddDays(-1);
                var changeId = 0L;
                var dataLastModified = DateTime.MinValue;

                var xmlDocument = await _norsObservationService.GetAsync(startDate, endDate, changeId);
                changeId = long.Parse(xmlDocument?.Descendants(ns + "MaxChangeId")?.FirstOrDefault()?.Value ?? "0");

                // Loop until all sightings are fetched.
                while (changeId != 0)
                {
                    var lastRequesetTime = DateTime.Now;

                    _logger.LogDebug(
                            $"Fetching NORS observations between dates {startDate.ToString("yyyy-MM-dd")} and {endDate.ToString("yyyy-MM-dd")}, changeid: {changeId}");

                    var verbatims = await verbatimFactory.CastEntitiesToVerbatimsAsync(xmlDocument);
                    // Clean up
                    xmlDocument = null;

                    // Add sightings to MongoDb
                    await _norsObservationVerbatimRepository.AddManyAsync(verbatims);

                    nrSightingsHarvested += verbatims.Count();

                    _logger.LogDebug($"{nrSightingsHarvested} NORS observations harvested");

                    var batchDataLastModified = verbatims.Select(a => a.Modified).Max();

                    if (batchDataLastModified.HasValue && batchDataLastModified.Value > dataLastModified)
                    {
                        dataLastModified = batchDataLastModified.Value;
                    }

                    cancellationToken?.ThrowIfCancellationRequested();
                    if (_norsServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= _norsServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        _logger.LogInformation("Max NORS observations reached");
                        break;
                    }

                    var timeSinceLastCall = (DateTime.Now - lastRequesetTime).Milliseconds;
                    if (timeSinceLastCall < 2000)
                    {
                        Thread.Sleep(2000 - timeSinceLastCall);
                    }

                    xmlDocument = await _norsObservationService.GetAsync(startDate, endDate, changeId);
                    changeId = long.Parse(xmlDocument?.Descendants(ns + "MaxChangeId")?.FirstOrDefault()?.Value ?? "0");
                }

                _logger.LogInformation("Finished harvesting sightings for NORS data provider");

                // Update harvest info
                harvestInfo.DataLastModified =
                    dataLastModified == DateTime.MinValue ? (DateTime?) null : dataLastModified;
                harvestInfo.End = DateTime.Now;
                harvestInfo.Count = nrSightingsHarvested;

                if (nrSightingsHarvested >= currentDocCount * 0.8)
                {
                    harvestInfo.Status = RunStatus.Success;
                    _logger.LogInformation("Start permanentize temp collection for NORS verbatim");
                    await _norsObservationVerbatimRepository.PermanentizeCollectionAsync();
                    _logger.LogInformation("Finish permanentize temp collection for NORS verbatim");
                }
                else
                {
                    harvestInfo.Status = RunStatus.Failed;
                    _logger.LogError($"NORS: Previous harvested observation count is: {currentDocCount}. Now only {nrSightingsHarvested} observations where harvested.");
                }
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("NORS harvest was cancelled.");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest NORS");
                harvestInfo.Status = RunStatus.Failed;
            }
            finally
            {
                _norsObservationVerbatimRepository.TempMode = false;
            }

            _logger.LogInformation($"Finish harvesting sightings for NORS data provider. Status={harvestInfo.Status}");
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


        private string GetNorsHarvestSettingsInfoString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("NORS Harvest settings:");
            sb.AppendLine($"  Start Harvest Year: {_norsServiceConfiguration.StartHarvestYear}");
            if(_norsServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue)
            {
                sb.AppendLine(
                    $"  Max Number Of Sightings Harvested: {_norsServiceConfiguration.MaxNumberOfSightingsHarvested}");
            }
            return sb.ToString();
        }
    }
}