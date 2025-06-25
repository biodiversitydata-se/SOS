using MongoDB.Driver;
using SOS.Harvest.Factories.Validation.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.Validation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Harvest.Factories.Validation
{
    /// <summary>
    /// Data validation report factory base class.
    /// </summary>
    public abstract class DataValidationReportFactoryBase<TVerbatimObservation> : IDataValidationReportFactory
    {
        protected readonly IVocabularyValueResolver _vocabularyValueResolver;
        protected readonly IValidationManager _validationManager;
        protected readonly IVocabularyRepository _processedVocabularyRepository;
        protected readonly IAreaHelper _areaHelper;
        protected readonly ITaxonRepository _processedTaxonRepository;
        protected readonly ICache<int, Taxon> _taxonCache;
        protected readonly ICache<VocabularyId, Vocabulary> _vocabularyCache;
        protected Dictionary<int, Taxon>? _taxonById;
        protected IDictionary<VocabularyId, IDictionary<object, int>> _dwcaVocabularyById;
        protected IDictionary<VocabularyId, Vocabulary>? _vocabularyById;        
        protected readonly IProcessTimeManager _processTimeManager;
        protected readonly ProcessConfiguration ProcessConfiguration;
        private readonly SemaphoreSlim _initializeSemaphoreSlim = new SemaphoreSlim(1, 1);

        protected DataValidationReportFactoryBase(
            IVocabularyRepository processedVocabularyRepository,
            IValidationManager validationManager,
            IAreaHelper areaHelper,
            IVocabularyValueResolver vocabularyValueResolver,
            ITaxonRepository processedTaxonRepository,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ICache<int, Taxon> taxonCache,
            ICache<VocabularyId, Vocabulary> vocabularyCache)
        {
            _vocabularyValueResolver = vocabularyValueResolver ?? throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _validationManager = validationManager ?? throw new ArgumentNullException(nameof(validationManager));
            _processedVocabularyRepository = processedVocabularyRepository ?? throw new ArgumentNullException(nameof(processedVocabularyRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _processedTaxonRepository = processedTaxonRepository ?? throw new ArgumentNullException(nameof(processedTaxonRepository));
            _processTimeManager = processTimeManager;
            ProcessConfiguration = processConfiguration ?? throw new ArgumentNullException(nameof(processConfiguration));
            _taxonCache = taxonCache ?? throw new ArgumentNullException(nameof(taxonCache));
            _vocabularyCache = vocabularyCache ?? throw new ArgumentNullException(nameof(vocabularyCache));
            Task.Run(InitializeAsync).Wait();
        }

        private async Task InitializeAsync()
        {
            await _initializeSemaphoreSlim.WaitAsync();
            try
            {
                var taxa = await _taxonCache.GetAllAsync();
                _taxonById = taxa.ToDictionary(m => m.Id, m => m);
                var allVocabularies = (await _vocabularyCache.GetAllAsync()).ToList();
                _vocabularyById = allVocabularies.ToDictionary(f => f.Id, f => f);
                _dwcaVocabularyById = VocabularyHelper.GetVocabulariesDictionary(
                        ExternalSystemId.DarwinCore,
                        allVocabularies,
                        true);
                await _areaHelper.InitializeAsync();
            }
            finally
            {
                _initializeSemaphoreSlim.Release();
            }            
        }

        protected abstract Task<IAsyncCursor<TVerbatimObservation>> GetAllObservationsByCursorAsync(DataProvider dataProvider);
        protected abstract Task<long> GetTotalObservationsCountAsync(DataProvider dataProvider);
        protected abstract Task<Observation?> CreateProcessedObservationAsync(TVerbatimObservation verbatimObservation, DataProvider dataProvider);
        protected abstract void ValidateVerbatimData(TVerbatimObservation verbatimObservation, DwcaValidationRemarksBuilder validationRemarksBuilder);
        protected abstract void UpdateTermDictionaryValueSummary(
            Observation processedObservation,
            TVerbatimObservation verbatimObservation,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, int>> processedFieldValues,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>> verbatimFieldValues);
        protected abstract void ValidateVerbatimTaxon(
            TVerbatimObservation verbatimObservation,
            HashSet<string> nonMatchingTaxonIds,
            HashSet<string> nonMatchingScientificNames);

        public async Task<DataValidationReport<object, Observation>> CreateDataValidationSummary(
            DataProvider dataProvider,
            int maxNrObservationsToRead = 100000,
            int nrValidObservationsInReport = 100,
            int nrInvalidObservationsInReport = 100)
        {
            var validObservations = new List<ValidObservationTuple<object, Observation>>();
            var invalidObservations = new List<InvalidObservationTuple<object>>();
            int nrProcessedObservations = 0;
            int nrValidObservations = 0;
            int nrInvalidObservations = 0;
            var validationRemarksBuilder = new DwcaValidationRemarksBuilder();
            var observationDefects = new Dictionary<ObservationDefect.ObservationDefectType, int>();
            var processedFieldValues = new Dictionary<VocabularyId, Dictionary<VocabularyValue, int>>();
            var verbatimFieldValues = new Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>>();
            foreach (VocabularyId vocabularyId in (VocabularyId[])Enum.GetValues(typeof(VocabularyId)))
            {
                processedFieldValues.Add(vocabularyId, new Dictionary<VocabularyValue, int>());
                verbatimFieldValues.Add(vocabularyId, new Dictionary<VocabularyValue, HashSet<string>>());
            }
            HashSet<string> nonMatchingTaxonIds = new HashSet<string>();
            HashSet<string> nonMatchingScientificNames = new HashSet<string>();
            var totalCount = await GetTotalObservationsCountAsync(dataProvider);
            using var cursor = await GetAllObservationsByCursorAsync(dataProvider);
            while (await cursor.MoveNextAsync())
            {
                if (nrProcessedObservations >= maxNrObservationsToRead) continue;
                foreach (var verbatimObservation in cursor.Current)
                {
                    if (verbatimObservation == null || nrProcessedObservations >= maxNrObservationsToRead) continue;
                    var processedObservation = await CreateProcessedObservationAsync(verbatimObservation, dataProvider);
                    nrProcessedObservations++;
                    if (processedObservation == null) continue;
                    LocalDateTimeConverterHelper.ConvertToLocalTime(processedObservation);
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(new List<Observation>
                            {processedObservation!}, true);
                    ValidateVerbatimData(verbatimObservation, validationRemarksBuilder);
                    UpdateTermDictionaryValueSummary(processedObservation!, verbatimObservation, processedFieldValues, verbatimFieldValues);
                    var observationValidation = _validationManager.ValidateObservation(processedObservation, dataProvider);
                    if (observationValidation.IsValid)
                    {
                        nrValidObservations++;
                        if (validObservations.Count < nrValidObservationsInReport)
                        {
                            var dwcExport = CreateDwcExportObject(processedObservation!);
                            validObservations.Add(new ValidObservationTuple<object, Observation>
                            {
                                VerbatimObservation = verbatimObservation,
                                ProcessedObservation = processedObservation!,
                                DwcExport = dwcExport
                            });
                        }
                    }
                    else
                    {
                        nrInvalidObservations++;
                        if (invalidObservations.Count < nrInvalidObservationsInReport)
                        {
                            invalidObservations.Add(new InvalidObservationTuple<object>
                            {
                                VerbatimObservation = verbatimObservation,
                                ProcessedObservationDefects = observationValidation.Defects?.Select(d => d.Information).ToList()
                            });
                        }
                        if (observationValidation.Defects?.Any() ?? false)
                        {
                            foreach (var validationDefect in observationValidation.Defects)
                            {
                                if (validationDefect.DefectType == ObservationDefect.ObservationDefectType.TaxonNotFound)
                                {
                                    ValidateVerbatimTaxon(verbatimObservation, nonMatchingTaxonIds, nonMatchingScientificNames);
                                }
                                if (!observationDefects.ContainsKey(validationDefect.DefectType))
                                {
                                    observationDefects.Add(validationDefect.DefectType, 0);
                                }

                                observationDefects[validationDefect.DefectType]++;
                            }
                        }
                    }

                    if (nrProcessedObservations >= maxNrObservationsToRead) break;
                }

                if (nrProcessedObservations >= maxNrObservationsToRead) break;
            }

            var distinctValuesSummaries = processedFieldValues
                .Where(pair => pair.Value.Any())
                .Select(pair => new DistinctValuesSummary
                {
                    Term = pair.Key.ToString(),
                    MappedValues = pair.Value.Where(valuePair => !valuePair.Key.IsCustomValue())
                        .Select(valuePair => new DistinctValuesSummaryItem
                        {
                            Id = valuePair.Key.Id,
                            Value = valuePair.Key.Value,
                            Count = valuePair.Value,
                            VerbatimValues = verbatimFieldValues[pair.Key]
                                .FirstOrDefault(k => Equals(k.Key, valuePair.Key)).Value?.ToList() ?? new List<string>()
                        }).ToList(),
                    CustomValues = pair.Value.Where(valuePair => valuePair.Key.IsCustomValue())
                        .Select(valuePair => new DistinctValuesSummaryItem
                        {
                            Id = valuePair.Key.Id,
                            Value = valuePair.Key.Value,
                            Count = valuePair.Value,
                            Comment = "-1 is the Id for custom values. No matching value or synonyme were found in SOS term dictionary."
                        }).ToList(),
                    SosVocabulary = _vocabularyById![pair.Key].Values.Select(v => new VocabularyValue() { Id = v.Id, Value = v.Value }).ToList()
                }).ToList();

            var remarks = validationRemarksBuilder.CreateRemarks();
            if (!remarks.HasItems())
            {
                remarks.Add("No remarks.");
            }
            return new DataValidationReport<object, Observation>
            {
                Settings = new { MaxNrObservationsToProcess = maxNrObservationsToRead, NrValidObservationsInReport = nrValidObservationsInReport, NrInvalidObservationsInReport = nrInvalidObservationsInReport },
                Summary = new DataValidationReportSummary
                {
                    ReportCreatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    TotalNumberOfObservationsInDb = totalCount,
                    NrObservationsProcessed = nrProcessedObservations,
                    NrValidObservations = nrValidObservations,
                    NrInvalidObservations = nrInvalidObservations,
                    Remarks = remarks,
                    ObservationDefects = observationDefects.OrderByDescending(m => m.Value).Select(m => new DefectItem { Defect = m.Key.ToString(), Count = m.Value }).ToList(),
                    NonMatchingTaxonIds = nonMatchingTaxonIds.Count == 0 ? null : nonMatchingTaxonIds.ToList(),
                    NonMatchingScientificNames = nonMatchingScientificNames.Count == 0 ? null : nonMatchingScientificNames.ToList()
                },
                InvalidObservations = invalidObservations,
                ValidObservations = validObservations,
                DictionaryValues = distinctValuesSummaries
            };
        }

        private DwcExport CreateDwcExportObject(Observation processedObservation)
        {
            var dwcObservation = processedObservation.ToDarwinCore();
            IEnumerable<ExtendedMeasurementOrFactRow> emofRows = processedObservation.ToExtendedMeasurementOrFactRows();
            var dwcExport = new DwcExport
            {
                Observation = dwcObservation,
                Extensions = new DwcExportExtensions()
                {
                    ExtendedMeasurementOrFacts = emofRows
                }
            };
            return dwcExport;
        }

        protected void UpdateTermDictionaryValue(
            VocabularyId vocabularyId,
            string verbatimValue,
            VocabularyValue vocabularyValue,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, int>> processedFieldValues,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>> verbatimFieldValues)
        {
            if (vocabularyValue != null)
            {
                if (!processedFieldValues[vocabularyId].ContainsKey(vocabularyValue))
                {
                    processedFieldValues[vocabularyId].Add(vocabularyValue, 0);
                }
                processedFieldValues[vocabularyId][vocabularyValue]++;

                if (verbatimValue == null)
                {
                    verbatimValue = "[null]";
                }
                else if (verbatimValue == "")
                {
                    verbatimValue = "[empty]";
                }
                if (!string.IsNullOrWhiteSpace(verbatimValue))
                {
                    if (!verbatimFieldValues[vocabularyId].ContainsKey(vocabularyValue))
                    {
                        verbatimFieldValues[vocabularyId].Add(vocabularyValue, new HashSet<string>());
                    }
                    verbatimFieldValues[vocabularyId][vocabularyValue].Add(verbatimValue);
                }
            }
        }
    }
}