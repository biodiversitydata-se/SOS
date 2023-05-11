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
    public class SharkObservationHarvester : ObservationHarvesterBase<SharkObservationVerbatim, int>, ISharkObservationHarvester
    {
        private readonly ISharkObservationService _sharkObservationService;
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
            ILogger<SharkObservationHarvester> logger) : base("SHARK", sharkObservationVerbatimRepository, logger)
        {
            _sharkObservationService = sharkObservationService ??
                                       throw new ArgumentNullException(nameof(sharkObservationService));
            _sharkServiceConfiguration = sharkServiceConfiguration ??
                                         throw new ArgumentNullException(nameof(sharkServiceConfiguration));
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var runStatus = RunStatus.Success;
            var harvestCount = 0;
            (DateTime startDate, long preHarvestCount) initValues = (DateTime.Now, 0);
            try
            {
                initValues.preHarvestCount = await InitializeHarvestAsync(true);
               
                var dataSetsInfo = await _sharkObservationService.GetDataSetsAsync();

                if (!dataSetsInfo?.Rows.Any() ?? true)
                {
                    runStatus = RunStatus.Failed;
                    Logger.LogInformation("SHARK harvest failed due too missing data set info.");
                }
                else
                {
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
                        runStatus = RunStatus.Failed;
                        Logger.LogInformation("SHARK harvest failed. Could not find data set name index");
                    }
                    else
                    {
                        var verbatimFactory = new SharkHarvestFactory();

                        var harvestedSharkSampleIds = new HashSet<string>();
                        var rows = dataSetsInfo.Rows.Where(r => r != null).Select(r => r.ToArray());

                        foreach (var row in rows)
                        {
                            cancellationToken?.ThrowIfCancellationRequested();
                            if (_sharkServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                                harvestCount >= _sharkServiceConfiguration.MaxNumberOfSightingsHarvested)
                            {
                                break;
                            }

                            var dataSetName = row[datasetNameIndex];

                            if (_sharkServiceConfiguration.ValidDataTypes.Count(vt =>
                                dataSetName.IndexOf(vt, StringComparison.CurrentCultureIgnoreCase) != -1) == 0)
                            {
                                continue;
                            }

                            Logger.LogDebug($"Start getting file: {dataSetName}");

                            var data = await _sharkObservationService.GetAsync(dataSetName);

                            Logger.LogDebug($"Finish getting file: {dataSetName}");

                            if (data == null)
                            {
                                continue;
                            }

                            var verbatims = (await verbatimFactory.CastEntitiesToVerbatimsAsync(data))?.ToArray() ?? Array.Empty<SharkObservationVerbatim>();
                            harvestCount += verbatims?.Count() ?? 0;

                            // Add sightings to MongoDb
                            await VerbatimRepository.AddManyAsync(verbatims);
                        }
                    }
                }
            }
            catch (JobAbortedException)
            {
                runStatus = RunStatus.Canceled;
                Logger.LogInformation("SHARK harvest was cancelled.");
            }
            catch (Exception e)
            {
                runStatus = RunStatus.Failed;
                Logger.LogError(e, "Failed to harvest SHARK");
            }

            return await FinishHarvestAsync(initValues, runStatus, harvestCount);
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(JobRunModes mode,
            DateTime? fromDate,
            IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for this provider");
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider provider, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for this provider");
        }

        public async Task<List<string>> GetDatasetsToHarvestAsync()
        {
            List<string> datasets = new List<string>();
            var dataSetsInfo = await _sharkObservationService.GetDataSetsAsync();
            var datasetNameIndex = -1;
            foreach (var header in dataSetsInfo.Header)
            {
                datasetNameIndex++;

                if (header.Equals("dataset_name", StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }
            }

            var rows = dataSetsInfo.Rows.Where(r => r != null).Select(r => r.ToArray());

            foreach (var row in rows)
            {
                var dataSetName = row[datasetNameIndex];

                if (_sharkServiceConfiguration.ValidDataTypes.Count(vt =>
                    dataSetName.IndexOf(vt, StringComparison.CurrentCultureIgnoreCase) != -1) == 0)
                {
                    continue;
                }

                datasets.Add(dataSetName);
            }

            return datasets;
        }
    }
}