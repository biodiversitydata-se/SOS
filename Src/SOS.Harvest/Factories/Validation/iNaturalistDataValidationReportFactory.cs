using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.iNaturalist;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.INaturalist.Service;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Harvest.Factories.Validation;

/// <summary>
/// iNaturalist data validation factory.
/// </summary>
public class iNaturalistDataValidationReportFactory : DataValidationReportFactoryBase<iNaturalistVerbatimObservation>
{
    private readonly IiNaturalistObservationVerbatimRepository _iNaturalistObservationVerbatimRepository;
    private readonly ILogger<iNaturalistDataValidationReportFactory> _logger;
    private iNaturalistObservationFactory? _iNaturalistObservationFactory;

    public iNaturalistDataValidationReportFactory(
        IVocabularyRepository processedVocabularyRepository,
        IValidationManager validationManager,
        IAreaHelper areaHelper,
        IVocabularyValueResolver vocabularyValueResolver,
        ITaxonRepository processedTaxonRepository,
        IiNaturalistObservationVerbatimRepository iNaturalistObservationVerbatimRepository,
        IProcessTimeManager processTimeManager,
        ProcessConfiguration processConfiguration,
        ICache<int, Taxon> taxonCache,
        ICache<VocabularyId, Vocabulary> vocabularyCache,
        ILogger<iNaturalistDataValidationReportFactory> logger)
        : base(processedVocabularyRepository, validationManager, areaHelper, vocabularyValueResolver, processedTaxonRepository, processTimeManager, processConfiguration, taxonCache, vocabularyCache)
    {
        _iNaturalistObservationVerbatimRepository = iNaturalistObservationVerbatimRepository;
        _logger = logger;
    }

    protected override async Task<IAsyncCursor<iNaturalistVerbatimObservation>> GetAllObservationsByCursorAsync(DataProvider dataProvider)
    {
        return await _iNaturalistObservationVerbatimRepository.GetAllByCursorAsync();
    }

    protected override async Task<long> GetTotalObservationsCountAsync(DataProvider dataProvider)
    {
        return await _iNaturalistObservationVerbatimRepository.CountAllDocumentsAsync();
    }

    protected override async Task<Observation?> CreateProcessedObservationAsync(iNaturalistVerbatimObservation verbatimObservation, DataProvider dataProvider)
    {
        return await Task.Run(() =>
        {
            var processedObservation = GetObservationFactory(dataProvider).CreateProcessedObservation(verbatimObservation, false);
            _areaHelper.AddAreaDataToProcessedLocation(processedObservation?.Location);
            return processedObservation;
        });
    }

    protected override void ValidateVerbatimTaxon(
        iNaturalistVerbatimObservation verbatimObservation,
        HashSet<string> nonMatchingTaxonIds,
        HashSet<string> nonMatchingScientificNames)
    {
        if (verbatimObservation?.Taxon?.Name != null)
        {
            nonMatchingScientificNames.Add(verbatimObservation.Taxon.Name);
        }
    }

    protected override void ValidateVerbatimData(iNaturalistVerbatimObservation verbatimObservation, DwcaValidationRemarksBuilder validationRemarksBuilder)
    {
        
    }

    protected override void UpdateTermDictionaryValueSummary(
        Observation processedObservation,
        iNaturalistVerbatimObservation verbatimObservation,
        Dictionary<VocabularyId, Dictionary<VocabularyValue, int>> processedFieldValues,
        Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>> verbatimFieldValues)
    {
        
    }

    private iNaturalistObservationFactory GetObservationFactory(DataProvider dataProvider)
    {
        if (_iNaturalistObservationFactory == null)
        {
            _iNaturalistObservationFactory = new iNaturalistObservationFactory(dataProvider, _taxonById, _dwcaVocabularyById, _areaHelper, _processTimeManager, ProcessConfiguration, _logger);                
        }

        return _iNaturalistObservationFactory;
    }
}