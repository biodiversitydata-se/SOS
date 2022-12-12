using DwC_A;
using Microsoft.Extensions.Logging;
using SOS.Harvest.DarwinCore.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive;
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
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Resource.Interfaces;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Harvest.Managers
{
    /// <summary>
    /// DwC-A data validation manager.
    /// </summary>
    public class DwcaDataValidationReportManager : Interfaces.IDwcaDataValidationReportManager
    {
        private readonly IVocabularyValueResolver _vocabularyValueResolver;
        private readonly IDwcArchiveReader _dwcArchiveReader;
        private readonly IValidationManager _validationManager;
        private readonly IVocabularyRepository _processedVocabularyRepository;
        private readonly IAreaHelper _areaHelper;
        private readonly ITaxonRepository _processedTaxonRepository;
        private readonly IProcessTimeManager _processTimeManager;
        private readonly ProcessConfiguration _processConfiguration;
        private readonly ILogger<DwcaDataValidationReportManager> _logger;
        private Dictionary<int, Taxon> _taxonById;
        private IDictionary<VocabularyId, IDictionary<object, int>> _dwcaVocabularyById;
        private IDictionary<VocabularyId, Vocabulary> _vocabularyById;

        public DwcaDataValidationReportManager(IDwcArchiveReader dwcArchiveReader,
            IVocabularyRepository processedVocabularyRepository,
            IValidationManager validationManager,
            IAreaHelper areaHelper,
            IVocabularyValueResolver vocabularyValueResolver,
            ITaxonRepository processedTaxonRepository,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ILogger<DwcaDataValidationReportManager> logger)
        {
            _vocabularyValueResolver = vocabularyValueResolver ?? throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _dwcArchiveReader = dwcArchiveReader ?? throw new ArgumentNullException(nameof(dwcArchiveReader));
            _validationManager = validationManager ?? throw new ArgumentNullException(nameof(validationManager));
            _processedVocabularyRepository = processedVocabularyRepository ?? throw new ArgumentNullException(nameof(processedVocabularyRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _processedTaxonRepository = processedTaxonRepository ?? throw new ArgumentNullException(nameof(processedTaxonRepository));
            _processTimeManager = processTimeManager ?? throw new ArgumentNullException(nameof(processTimeManager));
            _processConfiguration = processConfiguration ?? throw new ArgumentNullException(nameof(processConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Task.Run(InitializeAsync).Wait();
        }

        private async Task InitializeAsync()
        {
            var taxa = await _processedTaxonRepository.GetAllAsync();
            _taxonById = taxa.ToDictionary(m => m.Id, m => m);

            var allVocabularies = await _processedVocabularyRepository.GetAllAsync();
            _vocabularyById = allVocabularies.ToDictionary(f => f.Id, f => f);
            _dwcaVocabularyById = DwcaObservationFactory.GetVocabulariesDictionary(
                ExternalSystemId.DarwinCore,
                allVocabularies,
                true);
            await _areaHelper.InitializeAsync();
        }

        public async Task<DwcaDataValidationReport<DwcObservationVerbatim, Observation>>
        CreateDataValidationSummary(ArchiveReader archiveReader,
                int maxNrObservationsToRead = 100000,
                int nrValidObservationsInReport = 100,
                int nrInvalidObservationsInReport = 100,
                int nrTaxaInSummary = 20)
        {
            var dataProvider = new DataProvider
            {
                Id = 0,
                Identifier = "DwcaDataValidationReport",
                Type = DataProviderType.DwcA
            };

            var dwcaObservationFactory = new DwcaObservationFactory(
                dataProvider,
                _taxonById,
                _dwcaVocabularyById,
                _areaHelper,
                _processTimeManager,
                _processConfiguration);

            var totalNumberOfObservations = archiveReader.GetNumberOfRowsInOccurrenceFile();
            var observationsBatches = _dwcArchiveReader.ReadArchiveInBatchesAsync(
                archiveReader,
                dataProvider);

            var validObservations = new List<ValidObservationTuple<DwcObservationVerbatim, Observation>>();
            var invalidObservations = new List<InvalidObservationTuple<DwcObservationVerbatim>>();
            int nrProcessedObservations = 0;
            int nrValidObservations = 0;
            int nrInvalidObservations = 0;
            var validationRemarksBuilder = new DwcaValidationRemarksBuilder();
            var observationDefects = new Dictionary<ObservationDefect.ObservationDefectType, int>();
            var processedFieldValues = new Dictionary<VocabularyId, Dictionary<VocabularyValue, int>>();
            var verbatimFieldValues = new Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>>();
            var taxaStatistics = new TaxaStatistics();
            foreach (VocabularyId vocabularyId in (VocabularyId[])Enum.GetValues(typeof(VocabularyId)))
            {
                processedFieldValues.Add(vocabularyId, new Dictionary<VocabularyValue, int>());
                verbatimFieldValues.Add(vocabularyId, new Dictionary<VocabularyValue, HashSet<string>>());
            }
            HashSet<string> nonMatchingScientificNames = new HashSet<string>();
            HashSet<string> nonMatchingTaxonIds = new HashSet<string>();
            await foreach (var observationsBatch in observationsBatches)
            {
                if (nrProcessedObservations >= maxNrObservationsToRead) continue;
                foreach (var verbatimObservation in observationsBatch)
                {
                    if (nrProcessedObservations >= maxNrObservationsToRead) continue;
                    var processedObservation = dwcaObservationFactory.CreateProcessedObservation(verbatimObservation, true);
                    nrProcessedObservations++;
                    LocalDateTimeConverterHelper.ConvertToLocalTime(processedObservation);
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(new List<Observation>
                        {processedObservation}, true);
                    dwcaObservationFactory.ValidateVerbatimData(verbatimObservation, validationRemarksBuilder);
                    UpdateTermDictionaryValueSummary(processedObservation, verbatimObservation, processedFieldValues, verbatimFieldValues);
                    var observationValidation = _validationManager.ValidateObservation(processedObservation, dataProvider);
                    UpdateTaxaStatistics(taxaStatistics, processedObservation);

                    if (observationValidation.IsValid)
                    {
                        nrValidObservations++;
                        if (validObservations.Count < nrValidObservationsInReport)
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
                            validObservations.Add(new ValidObservationTuple<DwcObservationVerbatim, Observation>
                            {
                                VerbatimObservation = verbatimObservation,
                                ProcessedObservation = processedObservation,
                                DwcExport = dwcExport
                            });
                        }
                    }
                    else
                    {
                        nrInvalidObservations++;
                        if (invalidObservations.Count < nrInvalidObservationsInReport)
                        {
                            invalidObservations.Add(new InvalidObservationTuple<DwcObservationVerbatim>
                            {
                                VerbatimObservation = verbatimObservation,
                                ProcessedObservationDefects = observationValidation.Defects?.Select(d => d.Information).ToList()
                            });
                        }

                        foreach (var validationDefect in observationValidation.Defects)
                        {
                            if (validationDefect.DefectType == ObservationDefect.ObservationDefectType.TaxonNotFound)
                            {
                                nonMatchingScientificNames.Add(verbatimObservation.ScientificName);
                                nonMatchingTaxonIds.Add(verbatimObservation.TaxonID);
                            }
                            if (!observationDefects.ContainsKey(validationDefect.DefectType))
                            {
                                observationDefects.Add(validationDefect.DefectType, 0);
                            }

                            observationDefects[validationDefect.DefectType]++;
                        }
                    }

                    if (nrProcessedObservations >= maxNrObservationsToRead) break;
                }

                if (nrProcessedObservations >= maxNrObservationsToRead) break;
            }

            var distinctValuesSummaries = GetDistinctDictionaryValuesSummary(processedFieldValues, verbatimFieldValues);
            var remarks = validationRemarksBuilder.CreateRemarks();
            var taxaStatisticsSummary = CreateTaxaStatisticsSummary(taxaStatistics, nrTaxaInSummary);

            return new DwcaDataValidationReport<DwcObservationVerbatim, Observation>
            {
                Settings = new { MaxNrObservationsToProcess = maxNrObservationsToRead, NrValidObservationsInReport = nrValidObservationsInReport, NrInvalidObservationsInReport = nrInvalidObservationsInReport },
                Summary = new DwcaDataValidationReportSummary
                {
                    ReportCreatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    TotalNumberOfObservationsInFile = totalNumberOfObservations,
                    NrObservationsProcessed = nrProcessedObservations,
                    NrValidObservations = nrValidObservations,
                    NrInvalidObservations = nrInvalidObservations,
                    Remarks = remarks,
                    ObservationDefects = observationDefects.OrderByDescending(m => m.Value).Select(m => new DefectItem { Defect = m.Key.ToString(), Count = m.Value }).ToList(),
                    NonMatchingScientificNames = nonMatchingScientificNames.Count == 0 ? null : nonMatchingScientificNames.ToList(),
                    NonMatchingTaxonIds = nonMatchingTaxonIds.Count == 0 ? null : nonMatchingTaxonIds.ToList()
                },
                InvalidObservations = invalidObservations,
                ValidObservations = validObservations,
                DictionaryValues = distinctValuesSummaries,
                TaxaStatistics = taxaStatisticsSummary
            };
        }

        private List<DistinctValuesSummary> GetDistinctDictionaryValuesSummary(Dictionary<VocabularyId, Dictionary<VocabularyValue, int>> processedFieldValues, Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>> verbatimFieldValues)
        {
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
                            VerbatimValues = verbatimFieldValues[pair.Key].Single(k => Equals(k.Key, valuePair.Key)).Value
                                .ToList(),
                        }).ToList(),
                    CustomValues = pair.Value.Where(valuePair => valuePair.Key.IsCustomValue())
                        .Select(valuePair => new DistinctValuesSummaryItem
                        {
                            Id = valuePair.Key.Id,
                            Value = valuePair.Key.Value,
                            Count = valuePair.Value,
                            Comment =
                                "-1 is the Id for custom values. No matching value or synonyme were found in SOS term dictionary."
                        }).ToList(),
                    SosVocabulary = _vocabularyById[pair.Key].Values
                        .Select(v => new VocabularyValue() { Id = v.Id, Value = v.Value }).ToList()
                }).ToList();
            return distinctValuesSummaries;
        }

        private void UpdateTermDictionaryValueSummary(
            Observation processedObservation,
            DwcObservationVerbatim verbatimObservation,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, int>> processedFieldValues,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>> verbatimFieldValues)
        {
            UpdateTermDictionaryValue(VocabularyId.LifeStage, verbatimObservation.LifeStage, processedObservation.Occurrence.LifeStage, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.AccessRights, verbatimObservation.AccessRights, processedObservation.AccessRights, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Sex, verbatimObservation.Sex, processedObservation.Occurrence.Sex, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.ReproductiveCondition, verbatimObservation.ReproductiveCondition, processedObservation.Occurrence.ReproductiveCondition, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Behavior, verbatimObservation.Behavior, processedObservation.Occurrence.Behavior, processedFieldValues, verbatimFieldValues);
           // UpdateTermDictionaryValue(VocabularyId.VerificationStatus, verbatimObservation.IdentificationVerificationStatus, processedObservation.Identification.ValidationStatus, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.VerificationStatus, verbatimObservation.IdentificationVerificationStatus, processedObservation.Identification.VerificationStatus, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Institution, verbatimObservation.InstitutionCode, processedObservation.InstitutionCode, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Unit, verbatimObservation.OrganismQuantityType, processedObservation.Occurrence.OrganismQuantityUnit, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.BasisOfRecord, verbatimObservation.BasisOfRecord, processedObservation.BasisOfRecord, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.EstablishmentMeans, verbatimObservation.EstablishmentMeans, processedObservation.Occurrence.EstablishmentMeans, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Type, verbatimObservation.Type, processedObservation.Type, processedFieldValues, verbatimFieldValues);
        }

        private void UpdateTermDictionaryValue(VocabularyId vocabularyId,
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

        private TaxaStatisticsSummary CreateTaxaStatisticsSummary(TaxaStatistics taxaStatistics, int nrTaxaToShow = 20)
        {
            List<TaxonRedlistStats> redlistStats = new List<TaxonRedlistStats>();
            foreach (var item in taxaStatistics.RedlistTaxa)
            {
                var redlistStat = TaxonRedlistStats.Create(item.Key);
                redlistStat.TaxaCount = item.Value.Count;
                redlistStat.ObservationCount = item.Value.Values.Sum(m => m.ObservationCount);
                redlistStat.Taxa = item
                    .Value
                    .Values
                    .OrderByDescending(entry => entry.ObservationCount)
                    .Take(nrTaxaToShow).ToList();
                redlistStats.Add(redlistStat);
            }

            var protectedByLawStats = new ProtectedByLawStats();
            protectedByLawStats.ObservationCount =
                taxaStatistics.ProtectedByLawTaxa.Values.Sum(m => m.ObservationCount);
            protectedByLawStats.TaxaCount = taxaStatistics.ProtectedByLawTaxa.Count;
            protectedByLawStats.Taxa = taxaStatistics
                .ProtectedByLawTaxa
                .Values
                .OrderByDescending(m => m.ObservationCount)
                .Take(nrTaxaToShow).ToList();

            var summary = new TaxaStatisticsSummary
            {
                TaxaCount = taxaStatistics.TaxaSet.Count,
                RedListTaxa = redlistStats,
                ProtectedByLawTaxa = protectedByLawStats
            };

            return summary;
        }

        private void UpdateTaxaStatistics(TaxaStatistics taxaStatistics, Observation processedObservation)
        {
            if (processedObservation.Taxon == null) return;
            var taxon = _taxonById[processedObservation.Taxon.Id];
            taxaStatistics.TaxaSet.Add(taxon.Id);

            if (taxon.Attributes.ProtectedByLaw)
            {
                if (!taxaStatistics.ProtectedByLawTaxa.ContainsKey(taxon.Id))
                {
                    taxaStatistics.ProtectedByLawTaxa.Add(taxon.Id, new TaxonInfo(taxon.Id, taxon.TaxonRank, taxon.ScientificName, taxon.VernacularName));
                }

                taxaStatistics.ProtectedByLawTaxa[taxon.Id].ObservationCount++;
            }

            if (!string.IsNullOrWhiteSpace(taxon.Attributes.RedlistCategory))
            {
                // Remove degree sign in redlist category
                var redlistCategory = taxon.Attributes.RedlistCategory.Length > 2 ? taxon.Attributes.RedlistCategory.Substring(0, 2) : taxon.Attributes.RedlistCategory;
                if (redlistCategory == "NA" || redlistCategory == "LC" || redlistCategory == "NE") return;

                if (!taxaStatistics.RedlistTaxa.ContainsKey(redlistCategory))
                {
                    taxaStatistics.RedlistTaxa.Add(redlistCategory, new Dictionary<int, TaxonInfo>());
                }

                var taxonRedlistStats = taxaStatistics.RedlistTaxa[redlistCategory];
                if (!taxonRedlistStats.ContainsKey(taxon.Id))
                {
                    taxonRedlistStats.Add(taxon.Id, new TaxonInfo(taxon.Id, taxon.TaxonRank, taxon.ScientificName, taxon.VernacularName));
                }

                taxonRedlistStats[taxon.Id].ObservationCount++;
            }
        }
    }

    public class TaxaStatistics
    {
        public HashSet<int> TaxaSet { get; set; } = new HashSet<int>();
        public Dictionary<string, Dictionary<int, TaxonInfo>> RedlistTaxa { get; set; } = new Dictionary<string, Dictionary<int, TaxonInfo>>();
        public Dictionary<int, TaxonInfo> ProtectedByLawTaxa { get; set; } = new Dictionary<int, TaxonInfo>();
    }
}