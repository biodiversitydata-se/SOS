﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Managers.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Observation harvest job.
    /// </summary>
    public class ObservationsHarvestJob : IObservationsHarvestJob
    {
        private readonly ITaxonHarvestJob _taxonHarvestJob;
        private readonly IFieldMappingImportJob _fieldMappingImportJob;
        private readonly IDataProviderManager _dataProviderManager;
        private readonly ILogger<ObservationsHarvestJob> _logger;
        private readonly Dictionary<DataSet, IHarvestJob> _harvestJobByType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxonHarvestJob"></param>
        /// <param name="fieldMappingImportJob"></param>
        /// <param name="artportalenHarvestJob"></param>
        /// <param name="clamPortalHarvestJob"></param>
        /// <param name="kulHarvestJob"></param>
        /// <param name="mvmHarvestJob"></param>
        /// <param name="norsHarvestJob"></param>
        /// <param name="sersHarvestJob"></param>
        /// <param name="sharkHarvestJob"></param>
        /// <param name="virtualHerbariumHarvestJob"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        /// <param name="dwcArchiveHarvestJob"></param>
        public ObservationsHarvestJob(
            ITaxonHarvestJob taxonHarvestJob,
            IFieldMappingImportJob fieldMappingImportJob,
            IArtportalenHarvestJob artportalenHarvestJob,
            IClamPortalHarvestJob clamPortalHarvestJob,
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
            _taxonHarvestJob = taxonHarvestJob ?? throw new ArgumentNullException(nameof(taxonHarvestJob));
            _fieldMappingImportJob = fieldMappingImportJob ?? throw new ArgumentNullException(nameof(fieldMappingImportJob));
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (artportalenHarvestJob == null) throw new ArgumentNullException(nameof(artportalenHarvestJob));
            if (clamPortalHarvestJob == null) throw new ArgumentNullException(nameof(clamPortalHarvestJob));
            if (kulHarvestJob == null) throw new ArgumentNullException(nameof(kulHarvestJob));
            if (mvmHarvestJob == null) throw new ArgumentNullException(nameof(mvmHarvestJob));
            if (norsHarvestJob == null) throw new ArgumentNullException(nameof(norsHarvestJob));
            if (sersHarvestJob == null) throw new ArgumentNullException(nameof(sersHarvestJob));
            if (sharkHarvestJob == null) throw new ArgumentNullException(nameof(sharkHarvestJob));
            if (virtualHerbariumHarvestJob == null) throw new ArgumentNullException(nameof(virtualHerbariumHarvestJob));
            if (dwcArchiveHarvestJob == null) throw new ArgumentNullException(nameof(dwcArchiveHarvestJob));
            _harvestJobByType = new Dictionary<DataSet, IHarvestJob>
            {
                {DataSet.ArtportalenObservations, artportalenHarvestJob},
                {DataSet.ClamPortalObservations, clamPortalHarvestJob},
                {DataSet.SersObservations, sersHarvestJob},
                {DataSet.NorsObservations, norsHarvestJob},
                {DataSet.KULObservations, kulHarvestJob},
                {DataSet.MvmObservations, mvmHarvestJob},
                {DataSet.SharkObservations, sharkHarvestJob},
                {DataSet.VirtualHerbariumObservations, virtualHerbariumHarvestJob},
                {DataSet.DwcA, dwcArchiveHarvestJob}
            };
        }

        public async Task<bool> RunAsync(IJobCancellationToken cancellationToken)
        {
            var allDataProviders = await _dataProviderManager.GetAllDataProviders();
            return await RunAsync(
                allDataProviders.Where(dataProvider => dataProvider.IncludeInScheduledHarvest).ToList(),
                allDataProviders.Where(dataProvider => dataProvider.IsActive).ToList(),
                cancellationToken);
        }

        public async Task<bool> RunAsync(
            List<string> harvestDataProviderIdOrIdentifiers,
            List<string> processDataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken)
        {
            if (harvestDataProviderIdOrIdentifiers == null || harvestDataProviderIdOrIdentifiers.Count == 0)
            {
                _logger.LogInformation("Couldn't run ObservationHarvestJob because harvestDataProviderIdOrIdentifiers is not set");
                return false;
            }
            if (processDataProviderIdOrIdentifiers == null || processDataProviderIdOrIdentifiers.Count == 0)
            {
                _logger.LogInformation("Couldn't run ObservationHarvestJob because processDataProviderIdOrIdentifiers is not set");
                return false;
            }

            var harvestDataProviders = await _dataProviderManager.GetDataProvidersByIdOrIdentifier(harvestDataProviderIdOrIdentifiers);
            var harvestDataProvidersResult = Result.Combine(harvestDataProviders);
            if (harvestDataProvidersResult.IsFailure)
            {
                _logger.LogInformation($"Couldn't run ObservationHarvestJob because of: {harvestDataProvidersResult.Error}");
                return false;
            }

            var processDataProviders = await _dataProviderManager.GetDataProvidersByIdOrIdentifier(processDataProviderIdOrIdentifiers);
            var processDataProvidersResult = Result.Combine(processDataProviders);
            if (processDataProvidersResult.IsFailure)
            {
                _logger.LogInformation($"Couldn't run ObservationHarvestJob because of: {processDataProvidersResult.Error}");
                return false;
            }

            return await RunAsync(
                harvestDataProviders.Select(d => d.Value).ToList(),
                processDataProviders.Select(d => d.Value).ToList(),
                cancellationToken);
        }

        private async Task<bool> RunAsync(
            List<DataProvider> harvestDataProviders, 
            List<DataProvider> processDataProviders, 
            IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Start Harvest Jobs");

                //------------------------------------------------------------------------
                // 1. Ensure that Artportalen always is included in harvestDataProviders
                //------------------------------------------------------------------------
                if (harvestDataProviders.All(dataProvider => dataProvider.Identifier != DataProviderIdentifiers.Artportalen))
                {
                    harvestDataProviders.Add(await _dataProviderManager.GetDataProviderByIdentifier(DataProviderIdentifiers.Artportalen));
                    _logger.LogInformation("Artportalen harvest was added to harvestDataProviders because it's mandatory.");
                }

                //-----------------------------------------------------------------------------
                // 2. Harvest taxonomy & field mappings directly without enqueuing to Hangfire
                //-----------------------------------------------------------------------------
                _logger.LogInformation("Start resource harvest jobs (taxonomy & field mappings)");
                var resourceHarvestTasks = new Dictionary<DataSet, Task<bool>>();
                resourceHarvestTasks.Add(DataSet.Taxa, _taxonHarvestJob.RunAsync());
                resourceHarvestTasks.Add(DataSet.FieldMappings, _fieldMappingImportJob.RunAsync());
                await Task.WhenAll(resourceHarvestTasks.Values);
                _logger.LogInformation("Finish resource harvest jobs (taxonomy & field mappings)");

                //------------------------------------------------------------------------
                // 3. Harvest observations directly without enqueuing to Hangfire
                //------------------------------------------------------------------------
                _logger.LogInformation("Start observasions harvest jobs");
                var harvestTaskByDataProvider = new Dictionary<DataProvider, Task<bool>>();
                foreach (var dataProvider in harvestDataProviders)
                {
                    var harvestJob = _harvestJobByType[dataProvider.Type];
                    harvestTaskByDataProvider.Add(dataProvider, harvestJob.RunAsync(cancellationToken));
                    _logger.LogDebug($"Added {dataProvider.Name} harvest");
                }
                await Task.WhenAll(harvestTaskByDataProvider.Values);
                _logger.LogInformation("Finish observasions harvest jobs");

                //---------------------------------------------------------------------------------------------------------
                // 4. If Artportalen and resource harvest was successful, go on with enqueuing processing job to Hangfire
                //---------------------------------------------------------------------------------------------------------
                var artportalenHarvestTask = harvestTaskByDataProvider.Single(pair => pair.Key.Identifier == DataProviderIdentifiers.Artportalen).Value;
                if (artportalenHarvestTask.Result &&
                    resourceHarvestTasks[DataSet.Taxa].Result &&
                    resourceHarvestTasks[DataSet.FieldMappings].Result)
                {
                    // Ensure that Artportalen always is included in processDataProviders
                    if (processDataProviders.All(dataProvider => dataProvider.Identifier != DataProviderIdentifiers.Artportalen))
                    {
                        processDataProviders.Add(await _dataProviderManager.GetDataProviderByIdentifier(DataProviderIdentifiers.Artportalen));
                        _logger.LogInformation("Artportalen process was added to processDataProviders because it's mandatory.");
                    }

                    // Enqueue process job to Hangfire
                    var jobId = BackgroundJob.Enqueue<IProcessJob>(job => job.RunAsync(
                        processDataProviders.Select(dataProvider => dataProvider.Identifier).ToList(), 
                        true, 
                        true, 
                        true, 
                        cancellationToken));

                    _logger.LogInformation($"Process Job with Id={jobId} was enqueued");
                    return true;
                }

                throw new Exception("Failed to harvest data");
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
    }
}