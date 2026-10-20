using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Skogsskada.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Skogsskada;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Processors.Skogsskada;

/// <summary>
///     Process factory class
/// </summary>
public class SkogsskadaObservationProcessor : ObservationProcessorBase<SkogsskadaObservationProcessor, SkogsskadaObservationVerbatim, ISkogsskadaObservationVerbatimRepository>,
    ISkogsskadaObservationProcessor
{
    private readonly ISkogsskadaObservationVerbatimRepository _skogsskadaObservationVerbatimRepository;
    private readonly IAreaHelper _areaHelper;

    /// <inheritdoc />
    protected override async Task<(int publicCount, int protectedCount, int failedCount)> ProcessObservationsAsync(
        DataProvider dataProvider,
        IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
        IDictionary<VocabularyId, IDictionary<object, int>> dwcaVocabularyById,
        JobRunModes mode,
        IJobCancellationToken cancellationToken)
    {
        var observationFactory = new SkogsskadaObservationFactory(
            dataProvider, taxa, dwcaVocabularyById, _areaHelper, TimeManager, ProcessConfiguration);

        return await base.ProcessObservationsAsync(
            dataProvider,
            mode,
            observationFactory,
            _skogsskadaObservationVerbatimRepository,
            cancellationToken);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="skogsskadaObservationVerbatimRepository"></param>
    /// <param name="areaHelper"></param>
    /// <param name="processedObservationRepository"></param>
    /// <param name="vocabularyValueResolver"></param>
    /// <param name="dwcArchiveFileWriterCoordinator"></param>
    /// <param name="processManager"></param>
    /// <param name="processTimeManager"></param>
    /// <param name="validationManager"></param>
    /// <param name="diffusionManager"></param>
    /// <param name="processConfiguration"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public SkogsskadaObservationProcessor(ISkogsskadaObservationVerbatimRepository skogsskadaObservationVerbatimRepository,
        IAreaHelper areaHelper,
        IProcessedObservationCoreRepository processedObservationRepository,
        IVocabularyValueResolver vocabularyValueResolver,
        IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
        IProcessManager processManager,
        IProcessTimeManager processTimeManager,
        IValidationManager validationManager,
        IDiffusionManager diffusionManager,
        ProcessConfiguration processConfiguration,
        ILogger<SkogsskadaObservationProcessor> logger) :
        base(processedObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, processManager, validationManager, diffusionManager, processTimeManager, null, processConfiguration, logger)
    {
        _skogsskadaObservationVerbatimRepository = skogsskadaObservationVerbatimRepository ??
                                                  throw new ArgumentNullException(nameof(skogsskadaObservationVerbatimRepository));
        _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
    }

    public override DataProviderType Type => DataProviderType.SkogsskadaObservations;
}