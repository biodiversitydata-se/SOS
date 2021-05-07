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
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.FishData;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Import.Harvesters.Observations
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

            _fishDataObservationVerbatimRepository.TempMode = true;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(DateTime.Now);

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
                var changeId = 0L;
                var nrSightingsHarvested = 0;
                var xmlDocument = await _fishDataObservationService.GetAsync(changeId);
                var verbatimFactory = new AquaSupportHarvestFactory<FishDataObservationVerbatim>();

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
                    await _fishDataObservationVerbatimRepository.AddManyAsync(verbatims);

                    nrSightingsHarvested += verbatims.Count();

                    _logger.LogDebug($"{nrSightingsHarvested} Fish Data observations harvested");

                    cancellationToken?.ThrowIfCancellationRequested();
                    if (_fishDataServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= _fishDataServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        _logger.LogInformation("Max Fish Data observations reached");
                        break;
                    }

                    xmlDocument = await _fishDataObservationService.GetAsync(changeId + 1);
                }

                _logger.LogInformation("Finished harvesting sightings for Fish Data data provider");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;
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
                _logger.LogInformation("Start permanentize temp collection for Fish Data verbatim");
                await _fishDataObservationVerbatimRepository.PermanentizeCollectionAsync();
                _logger.LogInformation("Finish permanentize temp collection for Fish Data verbatim");
            }

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
            sb.AppendLine(
                $"  Max Number Of Sightings Harvested: {_fishDataServiceConfiguration.MaxNumberOfSightingsHarvested}");
            return sb.ToString();
        }
    }
}