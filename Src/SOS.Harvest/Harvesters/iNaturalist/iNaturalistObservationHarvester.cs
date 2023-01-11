using System.Text;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.iNaturalist.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim;

namespace SOS.Harvest.Harvesters.iNaturalist
{
    public class iNaturalistObservationHarvester : IiNaturalistObservationHarvester
    {
        private readonly IVerbatimClient _verbatimClient;
        private readonly IiNaturalistObservationService _iNaturalistObservationService;
        private readonly iNaturalistServiceConfiguration _iNaturalistServiceConfiguration;
        private readonly ILogger<iNaturalistObservationHarvester> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="verbatimClient"></param>
        /// <param name="iNaturalistObservationService"></param>
        /// <param name="iNaturalistServiceConfiguration"></param>
        /// <param name="logger"></param>
        public iNaturalistObservationHarvester(
            IVerbatimClient verbatimClient,
            IiNaturalistObservationService iNaturalistObservationService,
            iNaturalistServiceConfiguration iNaturalistServiceConfiguration,
            ILogger<iNaturalistObservationHarvester> logger)
        {
            _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));
            _iNaturalistObservationService =
                iNaturalistObservationService ?? throw new ArgumentNullException(nameof(iNaturalistObservationService));

            _iNaturalistServiceConfiguration = iNaturalistServiceConfiguration ??
                                               throw new ArgumentNullException(nameof(iNaturalistServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HarvestInfo> HarvestObservationsAsync(JobRunModes mode,
            DateTime? fromDate,
            IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for this provider");
        }

        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo("iNaturallist", DateTime.Now);
            var dataProvider = new Lib.Models.Shared.DataProvider() { Id = 19, Identifier = "iNaturalist" };
            using var dwcArchiveVerbatimRepository = new DarwinCoreArchiveVerbatimRepository(
                dataProvider,
                _verbatimClient,
                _logger)
            {
                TempMode = false
            };

            // Get current document count from permanent index
            var currentDocCount = await dwcArchiveVerbatimRepository.CountAllDocumentsAsync();
            dwcArchiveVerbatimRepository.TempMode = true;

            try
            {
                _logger.LogInformation("Start harvesting sightings for iNaturalist data provider");
                _logger.LogInformation(GetINatHarvestSettingsInfoString());

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for iNaturalist verbatim collection");
                await dwcArchiveVerbatimRepository.DeleteCollectionAsync();
                _logger.LogInformation("Finish empty collection for iNaturalist verbatim collection");

                var nrSightingsHarvested = 0;
       
                var startDate = new DateTime(_iNaturalistServiceConfiguration.StartHarvestYear, 1, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                var gBIFResult = await _iNaturalistObservationService.GetAsync(startDate, endDate);

                var id = 0;
                // Loop until all sightings are fetched.
                do
                {
                    _logger.LogDebug(
                        $"Fetching iNaturalist observations between dates {startDate.ToString("yyyy-MM-dd")} and {endDate.ToString("yyyy-MM-dd")}");

                    foreach (var observation in gBIFResult)
                    {
                        observation.Id = ++id;
                    }

                    // Add sightings to MongoDb
                    await dwcArchiveVerbatimRepository.AddManyAsync(gBIFResult);

                    nrSightingsHarvested += gBIFResult.Count();

                    _logger.LogDebug($"{nrSightingsHarvested} iNaturalist observations harvested");

                    cancellationToken?.ThrowIfCancellationRequested();
                    if (_iNaturalistServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= _iNaturalistServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        _logger.LogInformation("Max iNaturalist observations reached");
                        break;
                    }

                    startDate = endDate.AddDays(1);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                    gBIFResult = await _iNaturalistObservationService.GetAsync(startDate, endDate);

                } while (gBIFResult != null && endDate <= DateTime.Now);

                _logger.LogInformation("Finished harvesting sightings for iNaturalist data provider");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Count = nrSightingsHarvested;

                if (nrSightingsHarvested >= currentDocCount * 0.8)
                {
                    harvestInfo.Status = RunStatus.Success;
                    _logger.LogInformation("Start permanentize temp collection for iNaturalist verbatim");
                    await dwcArchiveVerbatimRepository.PermanentizeCollectionAsync();
                    _logger.LogInformation("Finish permanentize temp collection for iNaturalist verbatim");
                }
                else
                {
                    harvestInfo.Status = RunStatus.Failed;
                    _logger.LogError($"iNaturalist: Previous harvested observation count is: {currentDocCount}. Now only {nrSightingsHarvested} observations where harvested.");
                }
                
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("iNaturalist harvest was cancelled.");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest iNaturalist");
                harvestInfo.Status = RunStatus.Failed;
            }
            
            _logger.LogInformation($"Finish harvesting sightings for iNaturalist data provider. Status={harvestInfo.Status}");
            return harvestInfo;
        }

        public Task<HarvestInfo> HarvestObservationsAsync(Lib.Models.Shared.DataProvider provider, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for this provider");
        }

        private string GetINatHarvestSettingsInfoString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("iNaturalist Harvest settings:");
            sb.AppendLine($"  Start Harvest Year: {_iNaturalistServiceConfiguration.StartHarvestYear}");
            sb.AppendLine(
                $"  Max Number Of Sightings Harvested: {_iNaturalistServiceConfiguration.MaxNumberOfSightingsHarvested}");
            return sb.ToString();
        }
    }
}