﻿using System.Text;
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

            _kulObservationVerbatimRepository.TempMode = true;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(DateTime.Now);

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
                var endDate = startDate.AddYears(1).AddDays(-1);
                var changeId = 0L;

                // Loop until all sightings are fetched.
                while (startDate < DateTime.Now)
                {
                    var lastRequesetTime = DateTime.Now;
                    var xmlDocument = await _kulObservationService.GetAsync(startDate, endDate, changeId);
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
                    }

                    var timeSinceLastCall = (DateTime.Now - lastRequesetTime).Milliseconds;
                    if (timeSinceLastCall < 2000)
                    {
                        Thread.Sleep(2000 - timeSinceLastCall);
                    }
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