using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Processed.ProcessInfo;
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
        private readonly IDarwinCoreRepository _processRepository;
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
        /// <param name="processRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="clamPortalProcessFactory"></param>
        /// <param name="kulProcessFactory"></param>
        /// <param name="speciesPortalProcessFactory"></param>
        /// <param name="taxonVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="logger"></param>
        public ProcessJob(
            IDarwinCoreRepository processRepository,
            IProcessInfoRepository processInfoRepository,
            IHarvestInfoRepository harvestInfoRepository,
            IClamPortalProcessFactory clamPortalProcessFactory,
            IKulProcessFactory kulProcessFactory,
            ISpeciesPortalProcessFactory speciesPortalProcessFactory,
            ITaxonVerbatimRepository taxonVerbatimRepository,
            IAreaHelper areaHelper,
            ILogger<ProcessJob> logger)
        {
            _processRepository = processRepository ?? throw new ArgumentNullException(nameof(processRepository));
            _processInfoRepository = processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _kulProcessFactory = kulProcessFactory;
            _clamPortalProcessFactory = clamPortalProcessFactory ?? throw new ArgumentNullException(nameof(clamPortalProcessFactory));
            _speciesPortalProcessFactory = speciesPortalProcessFactory ?? throw new ArgumentNullException(nameof(speciesPortalProcessFactory));
            _taxonVerbatimRepository = taxonVerbatimRepository ?? throw new ArgumentNullException(nameof(taxonVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> Run(int sources, bool toggleInstanceOnSuccess, IJobCancellationToken cancellationToken)
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
                await _processRepository.VerifyCollectionAsync();
                cancellationToken?.ThrowIfCancellationRequested();

                var currentHarvestInfo = (await _harvestInfoRepository.GetAllAsync())?.ToArray();
                var verbatimInfo = new List<VerbatimInfo>();

                // Create task list
                var processTasks = new Dictionary<DataProvider, Task<bool>>();

                // Add species portal import if first bit is set
                if ((sources & (int)DataProvider.Artdatabanken) > 0)
                {
                    processTasks.Add(DataProvider.Artdatabanken, _speciesPortalProcessFactory.ProcessAsync(taxa, cancellationToken));

                    var harvestInfo = currentHarvestInfo?.FirstOrDefault(hi => hi.Id.Equals(nameof(APSightingVerbatim))) ?? new HarvestInfo(nameof(APSightingVerbatim), DataProvider.Artdatabanken);
                    //Add information about harvest
                    verbatimInfo.Add(new VerbatimInfo(nameof(APSightingVerbatim), DataProvider.Artdatabanken)
                    {
                        End = harvestInfo?.End ?? DateTime.MinValue,
                        Count = harvestInfo?.Count ?? 0,
                        Metadata = currentHarvestInfo?.Where(hi => hi.Id.Equals(nameof(DarwinCoreTaxon))).ToArray(),
                        Start = harvestInfo?.Start ?? DateTime.MinValue
                    });
                }

                if ((sources & (int)DataProvider.ClamPortal) > 0)
                {
                    processTasks.Add(DataProvider.ClamPortal, _clamPortalProcessFactory.ProcessAsync(taxa, cancellationToken));

                    var harvestInfo = currentHarvestInfo?.FirstOrDefault(hi => hi.Id.Equals(nameof(ClamObservationVerbatim))) ?? new HarvestInfo(nameof(ClamObservationVerbatim), DataProvider.ClamPortal);
                    //Add information about harvest
                    verbatimInfo.Add(new VerbatimInfo(nameof(ClamObservationVerbatim), DataProvider.ClamPortal)
                    {
                        End = harvestInfo?.End ?? DateTime.MinValue,
                        Count = harvestInfo?.Count ?? 0,
                        Metadata = currentHarvestInfo?.Where(hi => hi.Id.Equals(nameof(DarwinCoreTaxon)) || hi.Id.Equals(nameof(Area))).ToArray(),
                        Start = harvestInfo?.Start ?? DateTime.MinValue
                    });
                }

                if ((sources & (int)DataProvider.KUL) > 0)
                {
                    processTasks.Add(DataProvider.KUL, _kulProcessFactory.ProcessAsync(taxa, cancellationToken));

                    var harvestInfo = currentHarvestInfo?.FirstOrDefault(hi => hi.Id.Equals(nameof(KulObservationVerbatim))) ?? new HarvestInfo(nameof(KulObservationVerbatim), DataProvider.KUL);
                    //Add information about harvest
                    verbatimInfo.Add(new VerbatimInfo(nameof(KulObservationVerbatim), DataProvider.KUL)
                    {
                        End = harvestInfo?.End ?? DateTime.MinValue,
                        Count = harvestInfo?.Count ?? 0,
                        Metadata = currentHarvestInfo?.Where(hi => hi.Id.Equals(nameof(DarwinCoreTaxon)) || hi.Id.Equals(nameof(Area))).ToArray(),
                        Start = harvestInfo?.Start ?? DateTime.MinValue
                    });
                }

                // Run all tasks async
                await Task.WhenAll(processTasks.Values);

                var success = processTasks.Values.All(t => t.Result);

                // Create index if great success
                if (success)
                {
                    _logger.LogDebug("Create indexes");
                    await _processRepository.CreateIndexAsync();

                    // Add metadata about processing to db
                    await _processInfoRepository.VerifyCollectionAsync();

                    // Get saved process info or create new object. 
                    var processInfo = await _processInfoRepository.GetAsync(_processRepository.InstanceToUpdate) ?? new ProcessInfo(_processRepository.InstanceToUpdate);

                    // Update process info
                    processInfo.End = DateTime.Now;
                    processInfo.Start = start;        // Merge current verbatim info with our new data
                    processInfo.VerbatimInfo = verbatimInfo.Union(processInfo.VerbatimInfo.Where(vi => !verbatimInfo.Select(v => v.DataProvider).Contains(vi.DataProvider)));
                    
                    // Save process info
                    await _processInfoRepository.AddOrUpdateAsync(processInfo);

                    if (toggleInstanceOnSuccess)
                    {
                        _logger.LogDebug("Toggle instance");
                        await _processRepository.ToggleInstanceAsync(start, await _harvestInfoRepository.GetAllAsync());
                    }
                }

                _logger.LogDebug($"Processing done: {success}");

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
    }
}
