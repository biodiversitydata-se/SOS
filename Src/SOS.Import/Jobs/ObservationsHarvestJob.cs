using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Jobs
{
    /// <summary>
    ///     Observation harvest job.
    /// </summary>
    public class ObservationsHarvestJob : IObservationsHarvestJob
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly Dictionary<DataProviderType, IHarvestJob> _harvestJobByType;
        private readonly ILogger<ObservationsHarvestJob> _logger;

        /// <summary>
        /// Run harvest and start processing on success if requested
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="harvestProviders"></param>
        /// <param name="processProviders"></param>
        /// <param name="processOnSuccess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> RunAsync(JobRunModes mode, 
            IEnumerable<DataProvider> harvestProviders,
            IEnumerable<DataProvider> processProviders,
            bool processOnSuccess,
            IJobCancellationToken cancellationToken)
        {
            _logger.LogInformation($"Start harvest job ({mode})");
            var success = mode == JobRunModes.Full ?
                await HarvestAll(harvestProviders, cancellationToken)
                :
                await HarvestIncremental(harvestProviders, mode, cancellationToken);


            if (!success)
            {
                _logger.LogInformation($"Harvest job ({mode}) failed");
                return false;
            }
            
            if (processOnSuccess)
            {
                // If harvest was successful, go on with enqueuing processing job to Hangfire
                var jobId = BackgroundJob.Enqueue<IProcessJob>(job => job.RunAsync(
                    processProviders.Select(dataProvider => dataProvider.Identifier).ToList(),
                    mode,
                    cancellationToken));

                _logger.LogInformation($"Process Job ({mode}) with Id={ jobId } was enqueued");
            }

            _logger.LogInformation($"Finish harvest job ({mode})");

            return true;
        }

        /// <summary>
        /// Harvest all
        /// </summary>
        /// <param name="dataProviders"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> HarvestAll(
            IEnumerable<DataProvider> dataProviders,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Start Harvest Jobs");

                //------------------------------------------------------------------------
                // 1. Harvest observations directly without enqueuing to Hangfire
                //------------------------------------------------------------------------
                _logger.LogInformation("Start observations harvest jobs");
                var harvestTaskByDataProvider = new Dictionary<DataProvider, Task<bool>>();
                foreach (var dataProvider in dataProviders)
                {
                    var harvestJob = _harvestJobByType[dataProvider.Type];
                    harvestTaskByDataProvider.Add(dataProvider, harvestJob.RunAsync(cancellationToken));
                    _logger.LogDebug($"Added {dataProvider.Name} harvest");
                }

                await Task.WhenAll(harvestTaskByDataProvider.Values);
                _logger.LogInformation("Finish observasions harvest jobs");

                //---------------------------------------------------------------------------------------------------------
                // 3. Make sure mandatory providers where successful
                //---------------------------------------------------------------------------------------------------------
                return harvestTaskByDataProvider.Where(dp => dp.Key.HarvestFailPreventProcessing).All(r => r.Value.Result);
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Observation harvest job was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Observation harvest job was cancelled.");
                throw new Exception("Failed to harvest data");
            }
        }

        /// <summary>
        /// Run job
        /// </summary>
        /// <param name="dataProviders"></param>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> HarvestIncremental(
            IEnumerable<DataProvider> dataProviders,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Start incremental harvest jobs");

                //------------------------------------------------------------------------
                // 1. Ensure that any data provider is added
                //------------------------------------------------------------------------
                if (!dataProviders.Any())
                {
                    _logger.LogError(
                        "No data providers to incremental harvest");
                    return false;
                }

                //------------------------------------------------------------------------
                // 2. Harvest observations directly without enqueuing to Hangfire
                //------------------------------------------------------------------------
                _logger.LogInformation("Start incremental observasions harvest jobs");
                var harvestTaskByDataProvider = new Dictionary<DataProvider, Task<bool>>();
                foreach (var dataProvider in dataProviders)
                {
                    var harvestJob = _harvestJobByType[dataProvider.Type];
                    harvestTaskByDataProvider.Add(dataProvider, harvestJob.RunAsync(mode, cancellationToken));
                    _logger.LogDebug($"Added {dataProvider.Name} incremental harvest");
                }

                await Task.WhenAll(harvestTaskByDataProvider.Values);
                var success = harvestTaskByDataProvider.All(p => p.Value.Result);
                _logger.LogInformation($"Finish observasions incremental harvest jobs. Success: { success }");

                return success;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Observation harvest incremental job was cancelled.");
                throw new Exception("Failed to harvest incremental data");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenHarvestJob"></param>
        /// <param name="clamPortalHarvestJob"></param>
        /// <param name="fishDataHarvestJob"></param>
        /// <param name="kulHarvestJob"></param>
        /// <param name="mvmHarvestJob"></param>
        /// <param name="norsHarvestJob"></param>
        /// <param name="sersHarvestJob"></param>
        /// <param name="sharkHarvestJob"></param>
        /// <param name="virtualHerbariumHarvestJob"></param>
        /// <param name="dwcArchiveHarvestJob"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public ObservationsHarvestJob(
            IArtportalenHarvestJob artportalenHarvestJob,
            IClamPortalHarvestJob clamPortalHarvestJob,
            IFishDataHarvestJob fishDataHarvestJob,
            IKulHarvestJob kulHarvestJob,
            IMvmHarvestJob mvmHarvestJob,
            INorsHarvestJob norsHarvestJob,
            ISersHarvestJob sersHarvestJob,
            ISharkHarvestJob sharkHarvestJob,
            IVirtualHerbariumHarvestJob virtualHerbariumHarvestJob,
            IDwcArchiveHarvestJob dwcArchiveHarvestJob,
            IDataProviderManager dataProviderManager,
            ILogger<ObservationsHarvestJob> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (artportalenHarvestJob == null) throw new ArgumentNullException(nameof(artportalenHarvestJob));
            if (clamPortalHarvestJob == null) throw new ArgumentNullException(nameof(clamPortalHarvestJob));
            if (fishDataHarvestJob == null) throw new ArgumentNullException(nameof(fishDataHarvestJob));
            if (kulHarvestJob == null) throw new ArgumentNullException(nameof(kulHarvestJob));
            if (mvmHarvestJob == null) throw new ArgumentNullException(nameof(mvmHarvestJob));
            if (norsHarvestJob == null) throw new ArgumentNullException(nameof(norsHarvestJob));
            if (sersHarvestJob == null) throw new ArgumentNullException(nameof(sersHarvestJob));
            if (sharkHarvestJob == null) throw new ArgumentNullException(nameof(sharkHarvestJob));
            if (virtualHerbariumHarvestJob == null) throw new ArgumentNullException(nameof(virtualHerbariumHarvestJob));
            if (dwcArchiveHarvestJob == null) throw new ArgumentNullException(nameof(dwcArchiveHarvestJob));
            _harvestJobByType = new Dictionary<DataProviderType, IHarvestJob>
            {
                {DataProviderType.ArtportalenObservations, artportalenHarvestJob},
                {DataProviderType.ClamPortalObservations, clamPortalHarvestJob},
                {DataProviderType.SersObservations, sersHarvestJob},
                {DataProviderType.NorsObservations, norsHarvestJob},
                {DataProviderType.FishDataObservations, fishDataHarvestJob},
                {DataProviderType.KULObservations, kulHarvestJob},
                {DataProviderType.MvmObservations, mvmHarvestJob},
                {DataProviderType.SharkObservations, sharkHarvestJob},
                {DataProviderType.VirtualHerbariumObservations, virtualHerbariumHarvestJob},
                {DataProviderType.DwcA, dwcArchiveHarvestJob}
            };
        }

        /// <inheritdoc />
        [DisplayName("Harvest Observations - [Mode = {0}]")]
        public async Task<bool> RunAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            var activeProviders = (await _dataProviderManager.GetAllDataProvidersAsync()).Where(dp =>
                dp.IsActive 
            ).ToArray();

            return await RunAsync(mode, activeProviders.Where(dp =>
                dp.IsReadyToHarvest &&
                dp.IncludeInScheduledHarvest &&
                (
                    mode.Equals(JobRunModes.Full) ||
                    dp.SupportIncrementalHarvest)
                ), 
                activeProviders, 
                true, 
                cancellationToken);
        }

        /// <inheritdoc />
        [DisplayName("Harvest and process observations from passed provides")]
        public async Task<bool> RunAsync(
            List<string> harvestDataProviderIdOrIdentifiers,
            List<string> processDataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken)
        {
            if (harvestDataProviderIdOrIdentifiers == null || harvestDataProviderIdOrIdentifiers.Count == 0)
            {
                _logger.LogInformation(
                    "Couldn't run ObservationHarvestJob because harvestDataProviderIdOrIdentifiers is not set");
                return false;
            }

            if (processDataProviderIdOrIdentifiers == null || processDataProviderIdOrIdentifiers.Count == 0)
            {
                _logger.LogInformation(
                    "Couldn't run ObservationHarvestJob because processDataProviderIdOrIdentifiers is not set");
                return false;
            }

            var harvestDataProviders =
                await _dataProviderManager.GetDataProvidersByIdOrIdentifier(harvestDataProviderIdOrIdentifiers);
            var harvestDataProvidersResult = Result.Combine(harvestDataProviders);
            if (harvestDataProvidersResult.IsFailure)
            {
                _logger.LogInformation(
                    $"Couldn't run ObservationHarvestJob because of: {harvestDataProvidersResult.Error}");
                return false;
            }

            var processDataProviders =
                await _dataProviderManager.GetDataProvidersByIdOrIdentifier(processDataProviderIdOrIdentifiers);
            var processDataProvidersResult = Result.Combine(processDataProviders);
            if (processDataProvidersResult.IsFailure)
            {
                _logger.LogInformation(
                    $"Couldn't run ObservationHarvestJob because of: {processDataProvidersResult.Error}");
                return false;
            }

            return await RunAsync(JobRunModes.Full, harvestDataProviders.Select(p => p.Value), processDataProviders.Select(p => p.Value), true, cancellationToken);
        }

        /// <inheritdoc />
        [DisplayName("Harvest and process observations from passed provides")]
        public async Task<bool> RunHarvestObservationsAsync(
            List<string> harvestDataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken)
        {
            if (harvestDataProviderIdOrIdentifiers == null || harvestDataProviderIdOrIdentifiers.Count == 0)
            {
                _logger.LogInformation(
                    "Couldn't run ObservationHarvestJob because harvestDataProviderIdOrIdentifiers is not set");
                return false;
            }

            var harvestDataProviders =
                await _dataProviderManager.GetDataProvidersByIdOrIdentifier(harvestDataProviderIdOrIdentifiers);
            var harvestDataProvidersResult = Result.Combine(harvestDataProviders);
            if (harvestDataProvidersResult.IsFailure)
            {
                _logger.LogInformation(
                    $"Couldn't run ObservationHarvestJob because of: {harvestDataProvidersResult.Error}");
                return false;
            }

            return await HarvestAll(
                harvestDataProviders.Select(d => d.Value).ToList(),
                cancellationToken);
        }
    }
}