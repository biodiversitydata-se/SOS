using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Extensions;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Repositories.Destination.Kul.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Kul;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations
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

        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(nameof(KulObservationVerbatim), DataProviderType.KULObservations,
                DateTime.Now);

            try
            {
                var start = DateTime.Now;
                _logger.LogInformation("Start harvesting sightings for KUL data provider");
                _logger.LogInformation(GetKulHarvestSettingsInfoString());

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for KUL verbatim collection");
                await _kulObservationVerbatimRepository.DeleteCollectionAsync();
                await _kulObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for KUL verbatim collection");

                var ns = (XNamespace)"http://schemas.datacontract.org/2004/07/ArtDatabanken.WebService.Data";
                var changeId = 0L;
                var nrSightingsHarvested = 0;
                var xmlDocument = await _kulObservationService.GetAsync(changeId);
                
                var verbatims = xmlDocument.ToVerbatims<KulObservationVerbatim>(ns);
                
                // Loop until all sightings are fetched.
                while (verbatims?.Any() ?? false)
                {
                    // Add sightings to MongoDb
                    await _kulObservationVerbatimRepository.AddManyAsync(verbatims);

                    nrSightingsHarvested += verbatims.Count();

                    _logger.LogInformation($"{ nrSightingsHarvested } KUL observations harvested");

                    cancellationToken?.ThrowIfCancellationRequested();
                    if (_kulServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= _kulServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        _logger.LogInformation("Max KUL observations reached");
                        break;
                    }

                    var maxIdField = xmlDocument.Descendants(ns + "MaxChangeId").FirstOrDefault();

                    changeId = long.Parse(maxIdField.Value);
                    xmlDocument = await _kulObservationService.GetAsync(changeId);
                    verbatims = xmlDocument.ToVerbatims<KulObservationVerbatim>(ns);
                }

                _logger.LogInformation("Finished harvesting sightings for KUL data provider");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;
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

            return harvestInfo;
        }

        private string GetKulHarvestSettingsInfoString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("KUL Harvest settings:");
            sb.AppendLine($"  Start Harvest Year: {_kulServiceConfiguration.StartHarvestYear}");
            sb.AppendLine(
                $"  Max Number Of Sightings Harvested: {_kulServiceConfiguration.MaxNumberOfSightingsHarvested}");
            return sb.ToString();
        }
    }
}