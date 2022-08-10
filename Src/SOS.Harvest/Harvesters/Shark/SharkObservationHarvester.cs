using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.Shark.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Models.Verbatim.Shark;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Harvesters.Shark
{
    public class SharkObservationHarvester : ISharkObservationHarvester
    {
        private readonly ILogger<SharkObservationHarvester> _logger;
        private readonly ISharkObservationService _sharkObservationService;
        private readonly ISharkObservationVerbatimRepository _sharkObservationVerbatimRepository;
        private readonly SharkServiceConfiguration _sharkServiceConfiguration;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="sharkObservationService"></param>
        /// <param name="sharkObservationVerbatimRepository"></param>
        /// <param name="sharkServiceConfiguration"></param>
        /// <param name="logger"></param>
        public SharkObservationHarvester(
            ISharkObservationService sharkObservationService,
            ISharkObservationVerbatimRepository sharkObservationVerbatimRepository,
            SharkServiceConfiguration sharkServiceConfiguration,
            ILogger<SharkObservationHarvester> logger)
        {
            _sharkObservationService = sharkObservationService ??
                                       throw new ArgumentNullException(nameof(sharkObservationService));
            _sharkObservationVerbatimRepository = sharkObservationVerbatimRepository ??
                                                  throw new ArgumentNullException(
                                                      nameof(sharkObservationVerbatimRepository));
            _sharkServiceConfiguration = sharkServiceConfiguration ??
                                         throw new ArgumentNullException(nameof(sharkServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));            
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo("SHARK", DateTime.Now);
            harvestInfo.Status = RunStatus.Failed;
            _sharkObservationVerbatimRepository.TempMode = true;

            try
            {
                var start = DateTime.Now;
                _logger.LogInformation("Start harvesting sightings for SHARK data provider");

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for SHARK verbatim collection");
                await _sharkObservationVerbatimRepository.DeleteCollectionAsync();
                await _sharkObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for SHARK verbatim collection");

                var nrSightingsHarvested = 0;
                var dataSetsInfo = await _sharkObservationService.GetDataSetsAsync();

                if (!dataSetsInfo?.Rows.Any() ?? true)
                {
                    _logger.LogInformation("SHARK harvest failed due too missing data set info.");
                    return harvestInfo;
                }

                var datasetNameIndex = -1;
                foreach (var header in dataSetsInfo.Header)
                {
                    datasetNameIndex++;

                    if (header.Equals("dataset_name", StringComparison.CurrentCultureIgnoreCase))
                    {
                        break;
                    }
                }

                if (datasetNameIndex == -1)
                {
                    _logger.LogInformation("SHARK harvest failed. Could not find data set name index");
                    return harvestInfo;
                }

                var verbatimFactory = new SharkHarvestFactory();

                var harvestedSharkSampleIds = new HashSet<string>();
                var rows = dataSetsInfo.Rows.Where(r => r != null).Select(r => r.ToArray());

                foreach (var row in rows)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    if (_sharkServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= _sharkServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        break;
                    }

                    var dataSetName = row[datasetNameIndex];

                    if (_sharkServiceConfiguration.ValidDataTypes.Count(vt =>
                        dataSetName.IndexOf(vt, StringComparison.CurrentCultureIgnoreCase) != -1) == 0)
                    {
                        continue;
                    }

                    _logger.LogDebug($"Start getting file: {dataSetName}");

                    var data = await _sharkObservationService.GetAsync(dataSetName);

                    _logger.LogDebug($"Finish getting file: {dataSetName}");

                    if (data == null)
                    {
                        continue;
                    }

                    var verbatims = (await verbatimFactory.CastEntitiesToVerbatimsAsync(data))?.ToArray() ?? Array.Empty<SharkObservationVerbatim>();
                    nrSightingsHarvested += verbatims?.Count() ?? 0;

                    // Add sightings to MongoDb
                    await _sharkObservationVerbatimRepository.AddManyAsync(verbatims);
                }

                _logger.LogInformation("Finished harvesting sightings for SHARK data provider");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;

                _logger.LogInformation("Start permanentize temp collection for SHARK verbatim");
                await _sharkObservationVerbatimRepository.PermanentizeCollectionAsync();
                _logger.LogInformation("Finish permanentize temp collection for SHARK verbatim");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("SHARK harvest was cancelled.");
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest SHARK");
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Failed;
            }
            finally
            {
                _sharkObservationVerbatimRepository.TempMode = false;
            }

            _logger.LogInformation($"Finish harvesting sightings for SHARK data provider. Status={harvestInfo.Status}");
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
    }
}