using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Harvest;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Sers;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Import.Harvesters.Observations
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

        public async Task<HarvestInfo> HarvestObservationsAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(DateTime.Now);

            try
            {
                _logger.LogInformation("Start harvesting sightings for SERS data provider");
                _logger.LogInformation(GetSersHarvestSettingsInfoString());

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for SERS verbatim collection");
                await _sersObservationVerbatimRepository.DeleteCollectionAsync();
                await _sersObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for SERS verbatim collection");

                var ns = (XNamespace)"http://schemas.datacontract.org/2004/07/ArtDatabanken.WebService.Data";
                var changeId = 0L;
                var nrSightingsHarvested = 0;
                var xmlDocument = await _sersObservationService.GetAsync(changeId);
                var dataLastModified = DateTime.MinValue;
                var verbatimFactory = new AquaSupportHarvestFactory<SersObservationVerbatim>();

                // Loop until all sightings are fetched.
                while (xmlDocument != null)
                {
                    changeId = long.Parse(xmlDocument.Descendants(ns + "MaxChangeId").FirstOrDefault()?.Value ?? "0");
                    if (changeId.Equals(0))
                    {
                        break;
                    }

                    var verbatims = await verbatimFactory.CastEntitiesToVerbatimsAsync(xmlDocument);

                    // Add sightings to MongoDb
                    await _sersObservationVerbatimRepository.AddManyAsync(verbatims);

                    nrSightingsHarvested += verbatims.Count();

                    _logger.LogDebug($"{ nrSightingsHarvested } SERS observations harvested");

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

                    xmlDocument = await _sersObservationService.GetAsync(changeId + 1);
                }
                _logger.LogInformation("Finished harvesting sightings for SERS data provider");

                // Update harvest info
                harvestInfo.DataLastModified =
                    dataLastModified == DateTime.MinValue ? (DateTime?) null : dataLastModified;
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;
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

            return harvestInfo;
        }

        private string GetSersHarvestSettingsInfoString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SERS Harvest settings:");
            sb.AppendLine($"  Page size: {_sersServiceConfiguration.MaxReturnedChangesInOnePage}");
            sb.AppendLine(
                $"  Max Number Of Sightings Harvested: {_sersServiceConfiguration.MaxNumberOfSightingsHarvested}");
            return sb.ToString();
        }
    }
}