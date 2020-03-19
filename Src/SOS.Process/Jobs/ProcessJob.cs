using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Models.Verbatim.Kul;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Factories.Interfaces;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Jobs
{
    /// <summary>
    /// Artportalen harvest
    /// </summary>
    public class ProcessJob : IProcessJob
    {
        private readonly IProcessedObservationRepository _darwinCoreRepository;
        private readonly IProcessInfoRepository _processInfoRepository;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly IArtportalenProcessFactory _artportalenProcessFactory;
        private readonly IClamPortalProcessFactory _clamPortalProcessFactory;
        private readonly IKulProcessFactory _kulProcessFactory;
        private readonly IInstanceFactory _instanceFactory;
        private readonly ITaxonProcessedRepository _taxonProcessedRepository;
        private readonly ICopyFieldMappingsJob _copyFieldMappingsJob;
        private readonly IProcessTaxaJob _processTaxaJob;
        private readonly IAreaHelper _areaHelper;
        private readonly ILogger<ProcessJob> _logger;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="clamPortalProcessFactory"></param>
        /// <param name="kulProcessFactory"></param>
        /// <param name="artportalenProcessFactory"></param>
        /// <param name="taxonProcessedRepository"></param>
        /// <param name="instanceFactory"></param>
        /// <param name="copyFieldMappingsJob"></param>
        /// <param name="processTaxaJob"></param>
        /// <param name="areaHelper"></param>
        /// <param name="logger"></param>
        public ProcessJob(
            IProcessedObservationRepository processedObservationRepository,
            IProcessInfoRepository processInfoRepository,
            IHarvestInfoRepository harvestInfoRepository,
            IClamPortalProcessFactory clamPortalProcessFactory,
            IKulProcessFactory kulProcessFactory,
            IArtportalenProcessFactory artportalenProcessFactory,
            ITaxonProcessedRepository taxonProcessedRepository,
            IInstanceFactory instanceFactory,
            ICopyFieldMappingsJob copyFieldMappingsJob,
            IProcessTaxaJob processTaxaJob,
            IAreaHelper areaHelper,
            ILogger<ProcessJob> logger)
        {
            _darwinCoreRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
            _processInfoRepository = processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _clamPortalProcessFactory = clamPortalProcessFactory ?? throw new ArgumentNullException(nameof(clamPortalProcessFactory));
            _kulProcessFactory = kulProcessFactory ?? throw new ArgumentNullException(nameof(kulProcessFactory));
            _artportalenProcessFactory = artportalenProcessFactory ?? throw new ArgumentNullException(nameof(artportalenProcessFactory));
            _taxonProcessedRepository = taxonProcessedRepository ?? throw new ArgumentNullException(nameof(taxonProcessedRepository));
            _copyFieldMappingsJob = copyFieldMappingsJob ?? throw new ArgumentNullException(nameof(copyFieldMappingsJob));
            _processTaxaJob = processTaxaJob ?? throw new ArgumentNullException(nameof(processTaxaJob));
            _instanceFactory = instanceFactory ?? throw new ArgumentNullException(nameof(instanceFactory));
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

                _logger.LogDebug("Start processing meta data");
                if (!metadataTasks.All(t => t.Result))
                {
                    _logger.LogError("Failed to process meta data");
                    return false;
                }
                _logger.LogDebug("Finish processing meta data");

                // Create task list
                _logger.LogDebug("Start getting processed taxa");

                // Get taxa
                var taxa = await _taxonProcessedRepository.GetTaxaAsync();
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
                    await _darwinCoreRepository.DeleteCollectionAsync();
                    _logger.LogDebug("Finish deleting current collection");
                    _logger.LogDebug("Start creating new collection");
                    await _darwinCoreRepository.AddCollectionAsync();
                    _logger.LogDebug("Finish creating new collection");

                    newCollection = true;
                }
                else
                {
                    _logger.LogDebug("Start verifying collection");
                    newCollection = await _darwinCoreRepository.VerifyCollectionAsync();
                    _logger.LogDebug("Finish verifying collection");
                }

                cancellationToken?.ThrowIfCancellationRequested();

                var currentHarvestInfo = (await _harvestInfoRepository.GetAllAsync())?.ToArray();
                var providerInfo = new Dictionary<DataProvider, ProviderInfo>();
                var processTasks = new Dictionary<DataProvider, Task<RunInfo>>();

                // Add Artportalen import if first bit is set
                if ((sources & (int)DataProvider.Artportalen) > 0)
                {
                    processTasks.Add(DataProvider.Artportalen, _artportalenProcessFactory.ProcessAsync(taxonById, cancellationToken));

                    var harvestInfo = currentHarvestInfo?.FirstOrDefault(hi => hi.Id.Equals(nameof(ArtportalenVerbatimObservation))) ?? new HarvestInfo(nameof(ArtportalenVerbatimObservation), DataProvider.Artportalen, DateTime.MinValue);

                    //Add information about harvest
                    providerInfo.Add(DataProvider.Artportalen, CreateProviderInfo(DataProvider.Artportalen, harvestInfo,
                        currentHarvestInfo?.Where(hi => hi.Id.Equals(nameof(DarwinCoreTaxon))).ToArray())
                    );
                }

                if ((sources & (int)DataProvider.ClamPortal) > 0)
                {
                    processTasks.Add(DataProvider.ClamPortal, _clamPortalProcessFactory.ProcessAsync(taxonById, cancellationToken));

                    var harvestInfo = currentHarvestInfo?.FirstOrDefault(hi => hi.Id.Equals(nameof(ClamObservationVerbatim))) ?? new HarvestInfo(nameof(ClamObservationVerbatim), DataProvider.ClamPortal, DateTime.MinValue);
                    
                    //Add information about harvest
                    providerInfo.Add(DataProvider.ClamPortal, CreateProviderInfo(DataProvider.ClamPortal, harvestInfo,
                        currentHarvestInfo?.Where(hi => hi.Id.Equals(nameof(DarwinCoreTaxon)) || hi.Id.Equals(nameof(Area))).ToArray())
                    );
                }
                
                if ((sources & (int)DataProvider.KUL) > 0)
                {
                    processTasks.Add(DataProvider.KUL, _kulProcessFactory.ProcessAsync(taxonById, cancellationToken));

                    var harvestInfo = currentHarvestInfo?.FirstOrDefault(hi => hi.Id.Equals(nameof(KulObservationVerbatim))) ?? new HarvestInfo(nameof(KulObservationVerbatim), DataProvider.KUL, DateTime.MinValue);

                    //Add information about harvest
                    providerInfo.Add(DataProvider.KUL, CreateProviderInfo(DataProvider.KUL, harvestInfo,
                        currentHarvestInfo?.Where(hi => hi.Id.Equals(nameof(DarwinCoreTaxon)) || hi.Id.Equals(nameof(Area))).ToArray())
                    );
                }
                
                // Run all tasks async
                await Task.WhenAll(processTasks.Values);

                var success = processTasks.Values.All(t => t.Result.Status == RunStatus.Success);

                // If some task/s failed and it was not Artportalen, Try to copy provider data from active instance
                if (!success 
                    && copyFromActiveOnFail 
                    && processTasks.ContainsKey(DataProvider.Artportalen) 
                    && processTasks[DataProvider.Artportalen].Result.Status == RunStatus.Success)
                {
                    var copyTasks = processTasks
                        .Where(t => t.Value.Result.Status == RunStatus.Failed)
                        .Select(t => _instanceFactory.CopyProviderDataAsync(t.Key)).ToArray();
                    
                    await Task.WhenAll(copyTasks);

                    success = copyTasks.All(t => t.Result);
                }

                // Create index if great success
                if (success)
                {
                    if (newCollection)
                    {
                        _logger.LogDebug("Start creating indexes");
                        await _darwinCoreRepository.CreateIndexAsync();
                        _logger.LogDebug("Finish creating indexes");
                    }

                    if (toggleInstanceOnSuccess)
                    {
                        _logger.LogDebug("Toggle instance");
                        await _darwinCoreRepository.SetActiveInstanceAsync(_darwinCoreRepository.InstanceToUpdate);
                    }
                }

                _logger.LogDebug($"Processing done: {success}");

                // Update provider info from process result
                foreach (var task in processTasks)
                {
                    var vi = providerInfo[task.Key];
                    vi.ProcessCount = task.Value.Result.Count;
                    vi.ProcessEnd = task.Value.Result.End;
                    vi.ProcessStart = task.Value.Result.Start;
                    vi.ProcessStatus = task.Value.Result.Status;
                }

                // Add metadata about processing to db
                await _processInfoRepository.VerifyCollectionAsync();

                // Get saved process info or create new object. 
                var processInfo = await _processInfoRepository.GetAsync(_darwinCoreRepository.InstanceToUpdate) ?? new ProcessInfo(_darwinCoreRepository.InstanceToUpdate);

                // Update process info
                processInfo.End = DateTime.Now;
                processInfo.Start = start;
                processInfo.Success = success;                      // Merge current verbatim info with our new data
                processInfo.ProviderInfo = providerInfo.Values.Union(processInfo.ProviderInfo.Where(pi => !providerInfo.Values.Select(v => v.Provider).Contains(pi.Provider)));

                // Save process info
                success = success && await _processInfoRepository.AddOrUpdateAsync(processInfo);

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
            catch (Exception e)
            {
                _logger.LogError(e, "Process job failed");
                return false;
            }
        }

        /// <summary>
        /// Create provider information object
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="harvestInfo"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        private ProviderInfo CreateProviderInfo(DataProvider provider, HarvestInfo harvestInfo, IEnumerable<HarvestInfo> metadata)
        {
            return new ProviderInfo(DataProvider.ClamPortal)
            {
                HarvestEnd = harvestInfo.End,
                HarvestCount = harvestInfo.Count,
                HarvestMetadata = metadata,
                HarvestStart = harvestInfo.Start,
                HarvestStatus = harvestInfo.Status
            };
        }
    }
}
