using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Factories.Validation;

/// <summary>
/// NORS data validation manager.
/// </summary>
public class NorsDataValidationReportFactory : DwcaDataValidationReportFactory
{
    public NorsDataValidationReportFactory(
        IVerbatimClient verbatimClient,
        IVocabularyRepository processedVocabularyRepository,
        IValidationManager validationManager,
        IAreaHelper areaHelper,
        IVocabularyValueResolver vocabularyValueResolver,
        ITaxonRepository processedTaxonRepository,
        IProcessTimeManager processTimeManager,
        ProcessConfiguration processConfiguration,
        ICache<int, Taxon> taxonCache,
        ICache<VocabularyId, Vocabulary> vocabularyCache,
        ILoggerFactory loggerFactory)
        : base(verbatimClient, processedVocabularyRepository, validationManager, areaHelper, vocabularyValueResolver, processedTaxonRepository, processTimeManager, processConfiguration, taxonCache, vocabularyCache, loggerFactory)
    {

    }
}