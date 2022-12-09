using System.Text;
using System.Xml.Linq;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.AquaSupport.Kul.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Harvesters.AquaSupport;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Kul;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Harvesters.AquaSupport.Kul
{
    public class KulObservationHarvester : IKulObservationHarvester
    {
        private readonly IKulObservationService _kulObservationService;
        private readonly IKulObservationVerbatimRepository _kulObservationVerbatimRepository;
        private readonly KulServiceConfiguration _kulServiceConfiguration;
        private readonly ILogger<KulObservationHarvester> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="kulObservationService"></param>
        /// <param name="kulObservationVerbatimRepository"></param>
        /// <param name="kulServiceConfiguration"></param>
        /// <param name="logger"></param>
        public KulObservationHarvester(
            IKulObservationService kulObservationService,
            IKulObservationVerbatimRepository kulObservationVerbatimRepository,
            KulServiceConfiguration kulServiceConfiguration,
            ILogger<KulObservationHarvester> logger)
        {
            _kulObservationService =
                kulObservationService ?? throw new ArgumentNullException(nameof(kulObservationService));
            _kulObservationVerbatimRepository = kulObservationVerbatimRepository ??
                                                throw new ArgumentNullException(
                                                    nameof(kulObservationVerbatimRepository));
            _kulServiceConfiguration = kulServiceConfiguration ??
                                       throw new ArgumentNullException(nameof(kulServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));            
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            // Get current document count from permanent index
            _kulObservationVerbatimRepository.TempMode = false;
            var currentDocCount = await _kulObservationVerbatimRepository.CountAllDocumentsAsync();

            var harvestInfo = new HarvestInfo("KUL", DateTime.Now);
            _kulObservationVerbatimRepository.TempMode = true;

            try
            {
                _logger.LogInformation("Start harvesting sightings for KUL data provider");
                _logger.LogInformation(GetKulHarvestSettingsInfoString());

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for KUL verbatim collection");
                await _kulObservationVerbatimRepository.DeleteCollectionAsync();
                await _kulObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for KUL verbatim collection");

                var ns = (XNamespace) "http://schemas.datacontract.org/2004/07/ArtDatabanken.WebService.Data";
                var verbatimFactory = new AquaSupportHarvestFactory<KulObservationVerbatim>();

                var nrSightingsHarvested = 0;
                var startDate = new DateTime(_kulServiceConfiguration.StartHarvestYear, 1, 1);
                var endDate = DateTime.Now;
                var changeId = 0L;

                var xmlDocument = await _kulObservationService.GetAsync(startDate, endDate, changeId);
                changeId = long.Parse(xmlDocument?.Descendants(ns + "MaxChangeId")?.FirstOrDefault()?.Value ?? "0");

                // Loop until all sightings are fetched.
                while (changeId != 0)
                {
                    var lastRequesetTime = DateTime.Now;

                    _logger.LogDebug(
                        $"Fetching KUL observations between dates {startDate.ToString("yyyy-MM-dd")} and {endDate.ToString("yyyy-MM-dd")}, changeid: {changeId}");

                    var verbatims = await verbatimFactory.CastEntitiesToVerbatimsAsync(xmlDocument);
                    // Clean up
                    xmlDocument = null;

                    // Add sightings to MongoDb
                    await _kulObservationVerbatimRepository.AddManyAsync(verbatims);

                    nrSightingsHarvested += verbatims.Count();

                    _logger.LogDebug($"{nrSightingsHarvested} KUL observations harvested");

                    cancellationToken?.ThrowIfCancellationRequested();
                    if (_kulServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= _kulServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        _logger.LogInformation("Max KUL observations reached");
                        break;
                    }
                    
                    var timeSinceLastCall = (DateTime.Now - lastRequesetTime).Milliseconds;
                    if (timeSinceLastCall < 2000)
                    {
                        Thread.Sleep(2000 - timeSinceLastCall);
                    }

                    xmlDocument = await _kulObservationService.GetAsync(startDate, endDate, changeId);
                    changeId = long.Parse(xmlDocument?.Descendants(ns + "MaxChangeId")?.FirstOrDefault()?.Value ?? "0");
                }

                _logger.LogInformation("Finished harvesting sightings for KUL data provider");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;

                _logger.LogInformation("Start permanentize temp collection for KUL verbatim");
                await _kulObservationVerbatimRepository.PermanentizeCollectionAsync();
                _logger.LogInformation("Finish permanentize temp collection for KUL verbatim");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("KUL harvest was cancelled.");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest KUL");
                harvestInfo.Status = RunStatus.Failed;
            }
            finally
            {
                _kulObservationVerbatimRepository.TempMode = false;
            }

            _logger.LogInformation($"Finish harvesting sightings for KUL data provider. Status={harvestInfo.Status}");
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

        private string GetKulHarvestSettingsInfoString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("KUL Harvest settings:");
            sb.AppendLine($"  Start Harvest Year: {_kulServiceConfiguration.StartHarvestYear}");
            if (_kulServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue)
            {
                sb.AppendLine(
                    $"  Max Number Of Sightings Harvested: {_kulServiceConfiguration.MaxNumberOfSightingsHarvested}");
            }
            return sb.ToString();
        }
    }
}