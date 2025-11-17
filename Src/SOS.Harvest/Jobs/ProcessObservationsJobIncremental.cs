using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System.Data;

namespace SOS.Harvest.Jobs;

/// <summary>
///     Artportalen harvest
/// </summary>
public class ProcessObservationsJobIncremental : ProcessObservationsJobBase, IProcessObservationsJobIncremental
{

    /// <summary>
    /// Constructor
    /// </summary>      
    public ProcessObservationsJobIncremental(IProcessedObservationCoreRepository processedObservationRepository,
        IProcessInfoRepository processInfoRepository,
        IHarvestInfoRepository harvestInfoRepository,
        IObservationProcessorManager observationProcessorManager,
        ICache<int, Taxon> taxonCache,
        ICache<VocabularyId, Vocabulary> vocabularyCache,
        IDataProviderCache dataProviderCache,
        IProcessTimeManager processTimeManager,
        IValidationManager validationManager,
        IProcessTaxaJob processTaxaJob,
        IAreaHelper areaHelper,
        ProcessConfiguration processConfiguration,
        ILogger<ProcessObservationsJobIncremental> logger) : base(processedObservationRepository, processInfoRepository, harvestInfoRepository, observationProcessorManager,
            taxonCache, vocabularyCache, dataProviderCache, processTimeManager, validationManager, processTaxaJob, areaHelper, processConfiguration,
        logger)
    {

    }

    /// <inheritdoc />
    public async Task<bool> RunAsync(
        JobRunModes mode,
        IJobCancellationToken cancellationToken)
    {
        _logger.BeginScope(new[] { new KeyValuePair<string, object>("mode", mode.GetLoggerMode()) });

        var allDataProviders = await _dataProviderCache.GetAllAsync();
        var dataProvidersToProcess = allDataProviders.Where(dataProvider =>
                    dataProvider.IsActive &&
                    (mode == JobRunModes.Full || dataProvider.SupportIncrementalHarvest))
                .ToList();

        return await RunAsync(
            dataProvidersToProcess,
            mode,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ProcessArtportalenObservationsAsync(IEnumerable<ArtportalenObservationVerbatim> verbatims)
    {
        _logger.BeginScope(new[] { new KeyValuePair<string, object>("mode", JobRunModes.IncrementalActiveInstance.GetLoggerMode()) });
        var processor = _observationProcessorManager.GetProcessor(DataProviderType.ArtportalenObservations) as IArtportalenObservationProcessor;
        var provider = await _dataProviderCache.GetAsync(1);
        var taxa = await GetTaxaAsync(JobRunModes.IncrementalActiveInstance);
        _processedObservationRepository.LiveMode = true;
        return await processor!.ProcessObservationsAsync(provider, taxa, verbatims);
    }
}