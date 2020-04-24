using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Models.Verbatim.Kul;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Nors;
using SOS.Lib.Models.Verbatim.Sers;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.Artportalen.Interfaces;
using SOS.Process.Processors.ClamPortal.Interfaces;
using SOS.Process.Processors.Kul.Interfaces;
using SOS.Process.Processors.Nors.Interfaces;
using SOS.Process.Processors.Sers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Jobs
{
    /// <summary>
    /// Artportalen harvest
    /// </summary>
    public class ProcessJob : ProcessJobBase, IProcessJob
    {
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IArtportalenObservationProcessor _artportalenObservationProcessor;
        private readonly IClamPortalObservationProcessor _clamPortalObservationProcessor;
        private readonly IKulObservationProcessor _kulObservationProcessor;
        private readonly INorsObservationProcessor _norsObservationProcessor;
        private readonly ISersObservationProcessor _sersObservationProcessor;
        private readonly IInstanceManager _instanceManager;
        private readonly IProcessedTaxonRepository _processedTaxonRepository;
        private readonly ICopyFieldMappingsJob _copyFieldMappingsJob;
        private readonly IProcessTaxaJob _processTaxaJob;
        private readonly IAreaHelper _areaHelper;
        private readonly ILogger<ProcessJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="clamPortalObservationProcessor"></param>
        /// <param name="kulObservationProcessor"></param>
        /// <param name="norsObservationProcessor"></param>
        /// <param name="sersObservationProcessor"></param>
        /// <param name="artportalenObservationProcessor"></param>
        /// <param name="processedTaxonRepository"></param>
        /// <param name="instanceManager"></param>
        /// <param name="copyFieldMappingsJob"></param>
        /// <param name="processTaxaJob"></param>
        /// <param name="areaHelper"></param>
        /// <param name="logger"></param>
        public ProcessJob(
            IProcessedObservationRepository processedObservationRepository,
            IProcessInfoRepository processInfoRepository,
            IHarvestInfoRepository harvestInfoRepository,
            IClamPortalObservationProcessor clamPortalObservationProcessor,
            IKulObservationProcessor kulObservationProcessor,
            INorsObservationProcessor norsObservationProcessor,
            ISersObservationProcessor sersObservationProcessor,
            IArtportalenObservationProcessor artportalenObservationProcessor,
            IProcessedTaxonRepository processedTaxonRepository,
            IInstanceManager instanceManager,
            ICopyFieldMappingsJob copyFieldMappingsJob,
            IProcessTaxaJob processTaxaJob,
            IAreaHelper areaHelper,
            ILogger<ProcessJob> logger) : base(harvestInfoRepository, processInfoRepository)
        {
            _processedObservationRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
            _clamPortalObservationProcessor = clamPortalObservationProcessor ?? throw new ArgumentNullException(nameof(clamPortalObservationProcessor));
            _kulObservationProcessor = kulObservationProcessor ?? throw new ArgumentNullException(nameof(kulObservationProcessor));
            _norsObservationProcessor = norsObservationProcessor ?? throw new ArgumentNullException(nameof(norsObservationProcessor));
            _sersObservationProcessor = sersObservationProcessor ?? throw new ArgumentNullException(nameof(sersObservationProcessor));
            _artportalenObservationProcessor = artportalenObservationProcessor ?? throw new ArgumentNullException(nameof(artportalenObservationProcessor));
            _processedTaxonRepository = processedTaxonRepository ?? throw new ArgumentNullException(nameof(processedTaxonRepository));
            _copyFieldMappingsJob = copyFieldMappingsJob ?? throw new ArgumentNullException(nameof(copyFieldMappingsJob));
            _processTaxaJob = processTaxaJob ?? throw new ArgumentNullException(nameof(processTaxaJob));
            _instanceManager = instanceManager ?? throw new ArgumentNullException(nameof(instanceManager));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(int sources, bool cleanStart, bool copyFromActiveOnFail, bool toggleInstanceOnSuccess, IJobCancellationToken cancellationToken)
        {
            try
            {
                if (sources == 0)
                {
                    return false;
                }

                var start = DateTime.Now;
                var metadataTasks = new[]
                {
                    _copyFieldMappingsJob.RunAsync(),
                    _processTaxaJob.RunAsync()
                };
                await Task.WhenAll(metadataTasks);

                _logger.LogInformation("Start processing meta data");
                if (!metadataTasks.All(t => t.Result))
                {
                    _logger.LogError("Failed to process meta data");
                    return false;
                }
                _logger.LogDebug("Finish processing meta data");

                // Create task list
                _logger.LogDebug("Start getting processed taxa");

                // Get taxa
                var taxa = await _processedTaxonRepository.GetAllAsync();
                if (!taxa?.Any() ?? true)
                {
                    _logger.LogDebug("Failed to get processed taxa");
                    return false;
                }
                _logger.LogDebug("Finish getting processed taxa");

                var taxonById = taxa.ToDictionary(m => m.Id, m => m);
                cancellationToken?.ThrowIfCancellationRequested();
                _logger.LogDebug("Verify collection");

                var newCollection = false;
                // Make sure we have a collection
                if (cleanStart)
                {
                    _logger.LogDebug("Start deleting current collection");
                    await _processedObservationRepository.DeleteCollectionAsync();
                    _logger.LogDebug("Finish deleting current collection");
                    _logger.LogDebug("Start creating new collection");
                    await _processedObservationRepository.AddCollectionAsync();
                    _logger.LogDebug("Finish creating new collection");

                    newCollection = true;
                }
                else
                {
                    _logger.LogDebug("Start verifying collection");
                    newCollection = await _processedObservationRepository.VerifyCollectionAsync();
                    _logger.LogDebug("Finish verifying collection");
                }

                cancellationToken?.ThrowIfCancellationRequested();

                var providersInfo = new Dictionary<ObservationProvider, ProviderInfo>();
                var processTasks = new Dictionary<ObservationProvider, Task<ProcessingStatus>>();
                var metaDataProviderInfo = await GetProviderInfoAsync(new Dictionary<string, DataSet>
                {
                    {nameof(Area), DataSet.Areas},
                    {nameof(ProcessedTaxon), DataSet.Taxa}
                });

                // Add Artportalen import if first bit is set
                if ((sources & (int)ObservationProvider.Artportalen) > 0)
                {
                    processTasks.Add(ObservationProvider.Artportalen, _artportalenObservationProcessor.ProcessAsync(taxonById, cancellationToken));

                    // Get harvest info and create a provider info object that we can add processing info to later
                    var harvestInfo = await GetHarvestInfoAsync(nameof(ArtportalenVerbatimObservation));
                    var providerInfo = CreateProviderInfo(DataSet.ArtportalenObservations, harvestInfo, start);
                    providerInfo.MetadataInfo =
                        metaDataProviderInfo.Where(mdp => new[] { DataSet.Taxa }.Contains(mdp.Provider)).ToArray();
                    providersInfo.Add(ObservationProvider.Artportalen, providerInfo);
                }

                if ((sources & (int)ObservationProvider.ClamPortal) > 0)
                {
                    processTasks.Add(ObservationProvider.ClamPortal, _clamPortalObservationProcessor.ProcessAsync(taxonById, cancellationToken));

                    // Get harvest info and create a provider info object  that we can add processing info to later
                    var harvestInfo = await GetHarvestInfoAsync(nameof(ClamObservationVerbatim));
                    var providerInfo = CreateProviderInfo(DataSet.ClamPortalObservations, harvestInfo, start);
                    providerInfo.MetadataInfo =
                        metaDataProviderInfo.Where(mdp => new[] { DataSet.Areas, DataSet.Taxa }.Contains(mdp.Provider)).ToArray();
                    providersInfo.Add(ObservationProvider.ClamPortal, providerInfo);
                }

                if ((sources & (int)ObservationProvider.KUL) > 0)
                {
                    processTasks.Add(ObservationProvider.KUL, _kulObservationProcessor.ProcessAsync(taxonById, cancellationToken));

                    // Get harvest info and create a provider info object  that we can add processing info to later
                    var harvestInfo = await GetHarvestInfoAsync(nameof(KulObservationVerbatim));
                    var providerInfo = CreateProviderInfo(DataSet.KULObservations, harvestInfo, start);
                    providerInfo.MetadataInfo =
                        metaDataProviderInfo.Where(mdp => new[] { DataSet.Areas, DataSet.Taxa }.Contains(mdp.Provider)).ToArray();
                    providersInfo.Add(ObservationProvider.KUL, providerInfo);
                }

                if ((sources & (int)ObservationProvider.NORS) > 0)
                {
                    processTasks.Add(ObservationProvider.NORS, _norsObservationProcessor.ProcessAsync(taxonById, cancellationToken));

                    // Get harvest info and create a provider info object  that we can add processing info to later
                    var harvestInfo = await GetHarvestInfoAsync(nameof(NorsObservationVerbatim));
                    var providerInfo = CreateProviderInfo(DataSet.NorsObservations, harvestInfo, start);
                    providerInfo.MetadataInfo =
                        metaDataProviderInfo.Where(mdp => new[] { DataSet.Areas, DataSet.Taxa }.Contains(mdp.Provider)).ToArray();
                    providersInfo.Add(ObservationProvider.NORS, providerInfo);
                }

                if ((sources & (int)ObservationProvider.SERS) > 0)
                {
                    processTasks.Add(ObservationProvider.SERS, _sersObservationProcessor.ProcessAsync(taxonById, cancellationToken));

                    // Get harvest info and create a provider info object  that we can add processing info to later
                    var harvestInfo = await GetHarvestInfoAsync(nameof(SersObservationVerbatim));
                    var providerInfo = CreateProviderInfo(DataSet.SersObservations, harvestInfo, start);
                    providerInfo.MetadataInfo =
                        metaDataProviderInfo.Where(mdp => new[] { DataSet.Areas, DataSet.Taxa }.Contains(mdp.Provider)).ToArray();
                    providersInfo.Add(ObservationProvider.SERS, providerInfo);
                }

                // Run all tasks async
                await Task.WhenAll(processTasks.Values);

                var success = processTasks.Values.All(t => t.Result.Status == RunStatus.Success);

                // Update provider info from process result
                foreach (var task in processTasks)
                {
                    var vi = providersInfo[task.Key];
                    vi.ProcessCount = task.Value.Result.Count;
                    vi.ProcessEnd = task.Value.Result.End;
                    vi.ProcessStart = task.Value.Result.Start;
                    vi.ProcessStatus = task.Value.Result.Status;
                }

                _logger.LogDebug("Start updating process info for observations");

                await SaveProcessInfo(_processedObservationRepository.InActiveCollectionName, start, providersInfo.Sum(pi => pi.Value.ProcessCount ?? 0),
                    success ? RunStatus.Success : RunStatus.Failed, providersInfo.Values);
                _logger.LogDebug("Finish updating process info for observations");

                // If some task/s failed and it was not Artportalen, Try to copy provider data from active instance
                if (!success
                    && copyFromActiveOnFail
                    && processTasks.ContainsKey(ObservationProvider.Artportalen)
                    && processTasks[ObservationProvider.Artportalen].Result.Status == RunStatus.Success)
                {
                    var copyTasks = processTasks
                        .Where(t => t.Value.Result.Status == RunStatus.Failed)
                        .Select(t => _instanceManager.CopyProviderDataAsync(t.Key)).ToArray();

                    await Task.WhenAll(copyTasks);

                    success = copyTasks.All(t => t.Result);
                }

                // Create index if great success
                if (success)
                {
                    if (newCollection)
                    {
                        _logger.LogDebug("Start creating indexes");
                        await _processedObservationRepository.CreateIndexAsync();
                        _logger.LogDebug("Finish creating indexes");
                    }

                    if (toggleInstanceOnSuccess)
                    {
                        _logger.LogDebug("Toggle instance");
                        await _processedObservationRepository.SetActiveInstanceAsync(_processedObservationRepository.InActiveInstance);
                    }
                }

                _logger.LogInformation($"Processing done: {success}");

                _logger.LogDebug("Persist area cache");
                _areaHelper.PersistCache();

                // return result of all processing
                return success ? true : throw new Exception("Process sightings job failed");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Process job was cancelled.");
                return false;
            }
        }
    }
}
