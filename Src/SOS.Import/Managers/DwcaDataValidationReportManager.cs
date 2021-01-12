using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DwC_A;
using Microsoft.Extensions.Logging;
using SOS.Import.DarwinCore.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Process.Processors.DarwinCoreArchive;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Import.Managers
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
            ILogger<DwcaDataValidationReportManager> logger)
        {
            _vocabularyValueResolver = vocabularyValueResolver ?? throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _dwcArchiveReader = dwcArchiveReader ?? throw new ArgumentNullException(nameof(dwcArchiveReader));
            _validationManager = validationManager ?? throw new ArgumentNullException(nameof(validationManager));
            _processedVocabularyRepository = processedVocabularyRepository ?? throw new ArgumentNullException(nameof(processedVocabularyRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _processedTaxonRepository = processedTaxonRepository ?? throw new ArgumentNullException(nameof(processedTaxonRepository));
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
                int nrInvalidObservationsInReport = 100)
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
                _areaHelper);

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
            var observationDefects = new Dictionary<string, int>();
            var processedFieldValues = new Dictionary<VocabularyId, Dictionary<VocabularyValue, int>>();
            var verbatimFieldValues = new Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>>();
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
                    var processedObservation = dwcaObservationFactory.CreateProcessedObservation(verbatimObservation);
                    nrProcessedObservations++;
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(new List<Observation>
                        {processedObservation}, true);
                    dwcaObservationFactory.ValidateVerbatimData(verbatimObservation, validationRemarksBuilder);
                    UpdateTermDictionaryValueSummary(processedObservation, verbatimObservation, processedFieldValues, verbatimFieldValues);
                    var observationValidation = _validationManager.ValidateObservation(processedObservation, dataProvider);
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
                                ProcessedObservationDefects = observationValidation.Defects
                            });
                        }

                        foreach (var validationDefect in observationValidation.Defects)
                        {
                            if (validationDefect == "Taxon not found" && string.IsNullOrWhiteSpace(verbatimObservation.TaxonID))
                            {
                                nonMatchingScientificNames.Add(verbatimObservation.ScientificName);
                                nonMatchingTaxonIds.Add(verbatimObservation.TaxonID);
                            }
                            if (!observationDefects.ContainsKey(validationDefect))
                            {
                                observationDefects.Add(validationDefect, 0);
                            }

                            observationDefects[validationDefect]++;
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
                            VerbatimValues = verbatimFieldValues[pair.Key].Single(k => Equals(k.Key, valuePair.Key)).Value.ToList(),
                        }).ToList(),
                    CustomValues = pair.Value.Where(valuePair => valuePair.Key.IsCustomValue())
                        .Select(valuePair => new DistinctValuesSummaryItem
                        {
                            Id = valuePair.Key.Id, 
                            Value = valuePair.Key.Value, 
                            Count = valuePair.Value,
                            Comment = "-1 is the Id for custom values. No matching value or synonyme were found in SOS term dictionary."
                        } ).ToList(),
                    SosVocabulary = _vocabularyById[pair.Key].Values.Select(v => new VocabularyValue() {Id = v.Id, Value = v.Value}).ToList()
                }).ToList();

            var remarks = validationRemarksBuilder.CreateRemarks();
            if (!remarks.HasItems())
            {
                remarks.Add("No remarks.");
            }
            return new DwcaDataValidationReport<DwcObservationVerbatim, Observation>
            {
                Settings = new { MaxNrObservationsToProcess = maxNrObservationsToRead, NrValidObservationsInReport = nrValidObservationsInReport, NrInvalidObservationsInReport = nrInvalidObservationsInReport},
                Summary = new DwcaDataValidationReportSummary
                {
                    ReportCreatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    TotalNumberOfObservationsInFile = totalNumberOfObservations,
                    NrObservationsProcessed = nrProcessedObservations,
                    NrValidObservations = nrValidObservations,
                    NrInvalidObservations = nrInvalidObservations,
                    Remarks = remarks,
                    ObservationDefects = observationDefects.OrderByDescending(m => m.Value).Select(m => new DefectItem { Defect = m.Key, Count = m.Value}).ToList(),
                    NonMatchingScientificNames = nonMatchingScientificNames.Count == 0 ? null : nonMatchingScientificNames.ToList(),
                    NonMatchingTaxonIds = nonMatchingTaxonIds.Count == 0 ? null : nonMatchingTaxonIds.ToList()
                },
                InvalidObservations = invalidObservations,
                ValidObservations = validObservations,
                DictionaryValues = distinctValuesSummaries
            };
        }

        private void UpdateTermDictionaryValueSummary(
            Observation processedObservation,
            DwcObservationVerbatim verbatimObservation,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, int>> processedFieldValues,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>> verbatimFieldValues)
        {
            UpdateTermDictionaryValue(VocabularyId.LifeStage, verbatimObservation.LifeStage, processedObservation.Occurrence.LifeStage, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.AccessRights, verbatimObservation.AccessRights, processedObservation.AccessRights, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Gender, verbatimObservation.Sex, processedObservation.Occurrence.Gender, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.ReproductiveCondition, verbatimObservation.ReproductiveCondition, processedObservation.Occurrence.ReproductiveCondition, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Behavior, verbatimObservation.Behavior, processedObservation.Occurrence.Behavior, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.ValidationStatus, verbatimObservation.IdentificationVerificationStatus, processedObservation.Identification.ValidationStatus, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Institution, verbatimObservation.InstitutionCode, processedObservation.InstitutionCode, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Unit, verbatimObservation.OrganismQuantityType, processedObservation.Occurrence.OrganismQuantityUnit, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.BasisOfRecord, verbatimObservation.BasisOfRecord, processedObservation.BasisOfRecord, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.EstablishmentMeans, verbatimObservation.EstablishmentMeans, processedObservation.Occurrence.EstablishmentMeans, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Type, verbatimObservation.Type, processedObservation.Type, processedFieldValues, verbatimFieldValues);
        }

        private void UpdateTermDictionaryValue(VocabularyId vocabularyId,
            string verbatimValue,
            VocabularyValue processedFieldMapValue,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, int>> processedFieldValues,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>> verbatimFieldValues)
        {
            if (processedFieldMapValue != null)
            {
                if (!processedFieldValues[vocabularyId].ContainsKey(processedFieldMapValue))
                {
                    processedFieldValues[vocabularyId].Add(processedFieldMapValue, 0);
                }
                processedFieldValues[vocabularyId][processedFieldMapValue]++;

                if (!string.IsNullOrWhiteSpace(verbatimValue))
                {
                    if (!verbatimFieldValues[vocabularyId].ContainsKey(processedFieldMapValue))
                    {
                        verbatimFieldValues[vocabularyId].Add(processedFieldMapValue, new HashSet<string>());
                    }
                    verbatimFieldValues[vocabularyId][processedFieldMapValue].Add(verbatimValue);
                }
            }
        }
    }

    public static class ProcessedFieldMapValueExtension
    {
        public static bool IsCustomValue(this VocabularyValue processedFieldMapValue)
        {
            return processedFieldMapValue != null && processedFieldMapValue.Id == -1;
        }
    }
}
