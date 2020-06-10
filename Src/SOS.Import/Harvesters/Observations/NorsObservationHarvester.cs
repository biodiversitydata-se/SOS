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
using SOS.Import.Repositories.Destination.Nors.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Nors;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations
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

        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(nameof(NorsObservationVerbatim), DataProviderType.NorsObservations,
                DateTime.Now);

            try
            {
                var start = DateTime.Now;
                _logger.LogInformation("Start harvesting sightings for NORS data provider");
                _logger.LogInformation(GetNorsHarvestSettingsInfoString());

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for NORS verbatim collection");
                await _norsObservationVerbatimRepository.DeleteCollectionAsync();
                await _norsObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for NORS verbatim collection");

                var ns = (XNamespace)"http://schemas.datacontract.org/2004/07/ArtDatabanken.WebService.Data";
                var changeId = 0L;
                var nrSightingsHarvested = 0;
                var xmlDocument = await _norsObservationService.GetAsync(changeId);
                var dataLastModified = DateTime.MinValue;

                var verbatims = xmlDocument.ToVerbatims<NorsObservationVerbatim>(ns);

                // Loop until all sightings are fetched.
                while (verbatims?.Any() ?? false)
                {
                    // Add sightings to MongoDb
                    await _norsObservationVerbatimRepository.AddManyAsync(verbatims);

                    nrSightingsHarvested += verbatims.Count();

                    _logger.LogInformation($"{ nrSightingsHarvested } NORS observations harvested");

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

                    var maxIdField = xmlDocument.Descendants(ns + "MaxChangeId").FirstOrDefault();

                    changeId = long.Parse(maxIdField.Value);
                    xmlDocument = await _norsObservationService.GetAsync(changeId);
                    verbatims = xmlDocument.ToVerbatims<NorsObservationVerbatim>(ns);
                }

                _logger.LogInformation("Finished harvesting sightings for NORS data provider");

                // Update harvest info
                harvestInfo.DataLastModified =
                    dataLastModified == DateTime.MinValue ? (DateTime?) null : dataLastModified;
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;
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

            return harvestInfo;
        }

        private string GetNorsHarvestSettingsInfoString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("NORS Harvest settings:");
            sb.AppendLine($"  Page size: {_norsServiceConfiguration.MaxReturnedChangesInOnePage}");
            sb.AppendLine(
                $"  Max Number Of Sightings Harvested: {_norsServiceConfiguration.MaxNumberOfSightingsHarvested}");
            return sb.ToString();
        }
    }
}