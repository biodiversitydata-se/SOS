using Microsoft.Extensions.Logging;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Harvest.Jobs;

public class ProcessArtportalenTaxaJob : ProcessJobBase, IProcessArtportalenTaxaJob
{
    private readonly IArtportalenTaxonProcessor _artportalenTaxonProcessor;
    private readonly IArtportalenTaxonRepository _artportalenTaxonRepository;
    private readonly ILogger<ProcessArtportalenTaxaJob> _logger;

    public ProcessArtportalenTaxaJob(
        IArtportalenTaxonProcessor artportalenTaxonProcessor,
        IArtportalenTaxonRepository artportalenTaxonRepository,
        IHarvestInfoRepository harvestInfoRepository,
        IProcessInfoRepository processInfoRepository,
        ILogger<ProcessArtportalenTaxaJob> logger) : base(harvestInfoRepository, processInfoRepository)
    {
        _artportalenTaxonProcessor = artportalenTaxonProcessor ?? throw new ArgumentNullException(nameof(artportalenTaxonProcessor));
        _artportalenTaxonRepository = artportalenTaxonRepository ?? throw new ArgumentNullException(nameof(artportalenTaxonRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> RunAsync()
    {
        var start = DateTime.Now;

        var taxaCount = await _artportalenTaxonProcessor.ProcessArtportalenTaxaAsync();
        var success = taxaCount >= 0;
        _logger.LogDebug("Start updating process info for artportalen taxa");

        await SaveProcessInfo(new ProcessInfo("ArtportalenTaxon", start)
        {
            PublicCount = taxaCount,
            End = DateTime.Now,
            Status = success ? RunStatus.Success : RunStatus.Failed
        });

        _logger.LogDebug("Finish updating process info for artportalen taxa");

        return success ? true : throw new Exception("Process artportalen taxa job failed");
    }
}
