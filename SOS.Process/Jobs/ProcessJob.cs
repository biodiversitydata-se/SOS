﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared.Shared;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Models.Verbatim.Kul;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Models.Verbatim.SpeciesPortal;
using SOS.Process.Factories.Interfaces;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Jobs.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Jobs
{
    /// <summary>
    /// Species portal harvest
    /// </summary>
    public class ProcessJob : IProcessJob
    {
        private readonly IDarwinCoreRepository _darwinCoreRepository;
        private readonly IProcessInfoRepository _processInfoRepository;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ISpeciesPortalProcessFactory _speciesPortalProcessFactory;
        private readonly IClamPortalProcessFactory _clamPortalProcessFactory;
        private readonly IKulProcessFactory _kulProcessFactory;
        private readonly ITaxonVerbatimRepository _taxonVerbatimRepository;
        private readonly IAreaHelper _areaHelper;
        private readonly ILogger<ProcessJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="darwinCoreRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="clamPortalProcessFactory"></param>
        /// <param name="kulProcessFactory"></param>
        /// <param name="speciesPortalProcessFactory"></param>
        /// <param name="taxonVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="logger"></param>
        public ProcessJob(
            IDarwinCoreRepository darwinCoreRepository,
            IProcessInfoRepository processInfoRepository,
            IHarvestInfoRepository harvestInfoRepository,
            IClamPortalProcessFactory clamPortalProcessFactory,
            IKulProcessFactory kulProcessFactory,
            ISpeciesPortalProcessFactory speciesPortalProcessFactory,
            ITaxonVerbatimRepository taxonVerbatimRepository,
            IAreaHelper areaHelper,
            ILogger<ProcessJob> logger)
        {
            _darwinCoreRepository = darwinCoreRepository ?? throw new ArgumentNullException(nameof(darwinCoreRepository));
            _processInfoRepository = processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _clamPortalProcessFactory = clamPortalProcessFactory ?? throw new ArgumentNullException(nameof(clamPortalProcessFactory));
            _kulProcessFactory = kulProcessFactory ?? throw new ArgumentNullException(nameof(kulProcessFactory));
            _speciesPortalProcessFactory = speciesPortalProcessFactory ?? throw new ArgumentNullException(nameof(speciesPortalProcessFactory));
            _taxonVerbatimRepository = taxonVerbatimRepository ?? throw new ArgumentNullException(nameof(taxonVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(int sources, bool toggleInstanceOnSuccess, IJobCancellationToken cancellationToken)
        {
            try
            {
                var start = DateTime.Now;

                // Create task list
                _logger.LogDebug("Start getting taxa");

                // Map out taxon id
                var taxa = new Dictionary<int, DarwinCoreTaxon>();
                var skip = 0;
                var tmpTaxa = await _taxonVerbatimRepository.GetBatchAsync(skip);

                while (tmpTaxa?.Any() ?? false)
                {
                    foreach (var taxon in tmpTaxa)
                    {
                        taxa.Add(taxon.Id, taxon);
                    }

                    skip += tmpTaxa.Count();
                    tmpTaxa = await _taxonVerbatimRepository.GetBatchAsync(skip);
                }

                if (!taxa?.Any() ?? true)
                {
                    _logger.LogDebug("Failed to get taxa");

                    return false;
                }

                cancellationToken?.ThrowIfCancellationRequested();
                _logger.LogDebug("Verify collection");
               
                // Make sure we have a collection
                await _darwinCoreRepository.VerifyCollectionAsync();
                cancellationToken?.ThrowIfCancellationRequested();

                var currentHarvestInfo = (await _harvestInfoRepository.GetAllAsync())?.ToArray();
                var providerInfo = new Dictionary<DataProvider, ProviderInfo>();
                var processTasks = new Dictionary<DataProvider, Task<RunInfo>>();

                // Add species portal import if first bit is set
                if ((sources & (int)DataProvider.Artdatabanken) > 0)
                {
                    processTasks.Add(DataProvider.Artdatabanken, _speciesPortalProcessFactory.ProcessAsync(taxa, cancellationToken));

                    var harvestInfo = currentHarvestInfo?.FirstOrDefault(hi => hi.Id.Equals(nameof(APSightingVerbatim))) ?? new HarvestInfo(nameof(APSightingVerbatim), DataProvider.Artdatabanken, DateTime.MinValue);

                    //Add information about harvest
                    providerInfo.Add(DataProvider.Artdatabanken, CreateProviderInfo(DataProvider.Artdatabanken, harvestInfo,
                        currentHarvestInfo?.Where(hi => hi.Id.Equals(nameof(DarwinCoreTaxon))).ToArray())
                    );
                }

                if ((sources & (int)DataProvider.ClamPortal) > 0)
                {
                    processTasks.Add(DataProvider.ClamPortal, _clamPortalProcessFactory.ProcessAsync(taxa, cancellationToken));

                    var harvestInfo = currentHarvestInfo?.FirstOrDefault(hi => hi.Id.Equals(nameof(ClamObservationVerbatim))) ?? new HarvestInfo(nameof(ClamObservationVerbatim), DataProvider.ClamPortal, DateTime.MinValue);
                    
                    //Add information about harvest
                    providerInfo.Add(DataProvider.ClamPortal, CreateProviderInfo(DataProvider.ClamPortal, harvestInfo,
                        currentHarvestInfo?.Where(hi => hi.Id.Equals(nameof(DarwinCoreTaxon)) || hi.Id.Equals(nameof(Area))).ToArray())
                    );
                }

                if ((sources & (int)DataProvider.KUL) > 0)
                {
                    processTasks.Add(DataProvider.KUL, _kulProcessFactory.ProcessAsync(taxa, cancellationToken));

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
                    _logger.LogDebug("Create indexes");
                    await _darwinCoreRepository.CreateIndexAsync();

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
                return success;
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
