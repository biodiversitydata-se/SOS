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
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Processors.Interfaces;
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
        private readonly IArtportalenProcessor _artportalenProcessor;
        private readonly IClamPortalProcessor _clamPortalProcessor;
        private readonly IKulProcessor _kulProcessor;
        private readonly ITaxonProcessedRepository _taxonProcessedRepository;
        private readonly IAreaHelper _areaHelper;
        private readonly ILogger<ProcessJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="clamPortalProcessor"></param>
        /// <param name="kulProcessor"></param>
        /// <param name="artportalenProcessor"></param>
        /// <param name="taxonProcessedRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="logger"></param>
        public ProcessJob(
            IProcessedObservationRepository processedObservationRepository,
            IProcessInfoRepository processInfoRepository,
            IHarvestInfoRepository harvestInfoRepository,
            IClamPortalProcessor clamPortalProcessor,
            IKulProcessor kulProcessor,
            IArtportalenProcessor artportalenProcessor,
            ITaxonProcessedRepository taxonProcessedRepository,
            IAreaHelper areaHelper,
            ILogger<ProcessJob> logger)
        {
            _darwinCoreRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
            _processInfoRepository = processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _clamPortalProcessor = clamPortalProcessor ?? throw new ArgumentNullException(nameof(clamPortalProcessor));
            _kulProcessor = kulProcessor ?? throw new ArgumentNullException(nameof(kulProcessor));
            _artportalenProcessor = artportalenProcessor ?? throw new ArgumentNullException(nameof(artportalenProcessor));
            _taxonProcessedRepository = taxonProcessedRepository ?? throw new ArgumentNullException(nameof(taxonProcessedRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(int sources, bool cleanStart, bool toggleInstanceOnSuccess, IJobCancellationToken cancellationToken)
        {
            try
            {
                var start = DateTime.Now;

                // Create task list
                _logger.LogDebug("Start getting taxa");

                // Get taxa
                var taxa = await _taxonProcessedRepository.GetTaxaAsync();
                if (!taxa?.Any() ?? true)
                {
                    _logger.LogDebug("Failed to get taxa");
                    return false;
                }

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
                    processTasks.Add(DataProvider.Artportalen, _artportalenProcessor.ProcessAsync(taxonById, cancellationToken));

                    var harvestInfo = currentHarvestInfo?.FirstOrDefault(hi => hi.Id.Equals(nameof(ArtportalenVerbatimObservation))) ?? new HarvestInfo(nameof(ArtportalenVerbatimObservation), DataProvider.Artportalen, DateTime.MinValue);

                    //Add information about harvest
                    providerInfo.Add(DataProvider.Artportalen, CreateProviderInfo(DataProvider.Artportalen, harvestInfo,
                        currentHarvestInfo?.Where(hi => hi.Id.Equals(nameof(DarwinCoreTaxon))).ToArray())
                    );
                }

                if ((sources & (int)DataProvider.ClamPortal) > 0)
                {
                    processTasks.Add(DataProvider.ClamPortal, _clamPortalProcessor.ProcessAsync(taxonById, cancellationToken));

                    var harvestInfo = currentHarvestInfo?.FirstOrDefault(hi => hi.Id.Equals(nameof(ClamObservationVerbatim))) ?? new HarvestInfo(nameof(ClamObservationVerbatim), DataProvider.ClamPortal, DateTime.MinValue);
                    
                    //Add information about harvest
                    providerInfo.Add(DataProvider.ClamPortal, CreateProviderInfo(DataProvider.ClamPortal, harvestInfo,
                        currentHarvestInfo?.Where(hi => hi.Id.Equals(nameof(DarwinCoreTaxon)) || hi.Id.Equals(nameof(Area))).ToArray())
                    );
                }

                if ((sources & (int)DataProvider.KUL) > 0)
                {
                    processTasks.Add(DataProvider.KUL, _kulProcessor.ProcessAsync(taxonById, cancellationToken));

                    var harvestInfo = currentHarvestInfo?.FirstOrDefault(hi => hi.Id.Equals(nameof(KulObservationVerbatim))) ?? new HarvestInfo(nameof(KulObservationVerbatim), DataProvider.KUL, DateTime.MinValue);

                    //Add information about harvest
                    providerInfo.Add(DataProvider.KUL, CreateProviderInfo(DataProvider.KUL, harvestInfo,
                        currentHarvestInfo?.Where(hi => hi.Id.Equals(nameof(DarwinCoreTaxon)) || hi.Id.Equals(nameof(Area))).ToArray())
                    );
                }

                // Run all tasks async
                await Task.WhenAll(processTasks.Values);

                var success = processTasks.Values.All(t => t.Result.Status == RunStatus.Success);

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
