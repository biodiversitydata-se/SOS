using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DwC_A;
using Microsoft.Extensions.Logging;
using SOS.Import.DarwinCore.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.DarwinCoreArchive;

namespace SOS.Import.Managers
{
    /// <summary>
    /// DwC-A data validation manager.
    /// </summary>
    public class DwcaDataValidationReportManager : Interfaces.IDwcaDataValidationReportManager
    {
        private readonly IFieldMappingResolverHelper _fieldMappingResolverHelper;
        private readonly IDwcArchiveReader _dwcArchiveReader;
        private readonly IValidationManager _validationManager;
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;
        private readonly IAreaHelper _areaHelper;
        private readonly IProcessedTaxonRepository _processedTaxonRepository;
        private readonly ILogger<DwcaDataValidationReportManager> _logger;
        private Dictionary<int, ProcessedTaxon> _taxonById;
        private IDictionary<FieldMappingFieldId, IDictionary<object, int>> _dwcaFieldMappings;
        private IDictionary<FieldMappingFieldId, FieldMapping> _fieldMappings;

        public DwcaDataValidationReportManager(IDwcArchiveReader dwcArchiveReader,
            IProcessedFieldMappingRepository processedFieldMappingRepository,
            IValidationManager validationManager,
            IAreaHelper areaHelper,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            IProcessedTaxonRepository processedTaxonRepository,
            ILogger<DwcaDataValidationReportManager> logger)
        {
            _fieldMappingResolverHelper = fieldMappingResolverHelper ?? throw new ArgumentNullException(nameof(fieldMappingResolverHelper));
            _dwcArchiveReader = dwcArchiveReader ?? throw new ArgumentNullException(nameof(dwcArchiveReader));
            _validationManager = validationManager ?? throw new ArgumentNullException(nameof(validationManager));
            _processedFieldMappingRepository = processedFieldMappingRepository ?? throw new ArgumentNullException(nameof(processedFieldMappingRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _processedTaxonRepository = processedTaxonRepository ?? throw new ArgumentNullException(nameof(processedTaxonRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Task.Run(InitializeAsync).Wait();
        }

        private async Task InitializeAsync()
        {
            var taxa = await _processedTaxonRepository.GetAllAsync();
            _taxonById = taxa.ToDictionary(m => m.Id, m => m);

            var allFieldMappings = await _processedFieldMappingRepository.GetAllAsync();
            _fieldMappings = allFieldMappings.ToDictionary(f => f.Id, f => f);
            _dwcaFieldMappings = DwcaObservationFactory.GetFieldMappingsDictionary(
                ExternalSystemId.DarwinCore,
                allFieldMappings,
                true);
        }

        public async Task<DwcaDataValidationReport<DwcObservationVerbatim, ProcessedObservation>>
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
                _dwcaFieldMappings,
                _areaHelper);

            var totalNumberOfObservations = archiveReader.GetNumberOfRowsInOccurrenceFile();
            var observationsBatches = _dwcArchiveReader.ReadArchiveInBatchesAsync(
                archiveReader,
                dataProvider);

            var validObservations = new List<ValidObservationTuple<DwcObservationVerbatim, ProcessedObservation>>();
            var invalidObservations = new List<InvalidObservationTuple<DwcObservationVerbatim>>();
            int nrProcessedObservations = 0;
            int nrValidObservations = 0;
            int nrInvalidObservations = 0;
            var validationRemarksBuilder = new DwcaValidationRemarksBuilder();
            var observationDefects = new Dictionary<string, int>();
            var processedFieldValues = new Dictionary<FieldMappingFieldId, Dictionary<ProcessedFieldMapValue, int>>();
            var verbatimFieldValues = new Dictionary<FieldMappingFieldId, Dictionary<ProcessedFieldMapValue, HashSet<string>>>();
            foreach (FieldMappingFieldId fieldMappingFieldId in (FieldMappingFieldId[])Enum.GetValues(typeof(FieldMappingFieldId)))
            {
                processedFieldValues.Add(fieldMappingFieldId, new Dictionary<ProcessedFieldMapValue, int>());
                verbatimFieldValues.Add(fieldMappingFieldId, new Dictionary<ProcessedFieldMapValue, HashSet<string>>());
            }

            await foreach (var observationsBatch in observationsBatches)
            {
                if (nrProcessedObservations >= maxNrObservationsToRead) continue;
                foreach (var verbatimObservation in observationsBatch)
                {
                    if (nrProcessedObservations >= maxNrObservationsToRead) continue;
                    var processedObservation = dwcaObservationFactory.CreateProcessedObservation(verbatimObservation);
                    nrProcessedObservations++;
                    _fieldMappingResolverHelper.ResolveFieldMappedValues(new List<ProcessedObservation>
                        {processedObservation});
                    dwcaObservationFactory.ValidateVerbatimData(verbatimObservation, validationRemarksBuilder);
                    UpdateTermDictionaryValueSummary(processedObservation, verbatimObservation, processedFieldValues, verbatimFieldValues);
                    var observationValidation = _validationManager.ValidateObservation(processedObservation);
                    if (observationValidation.IsValid)
                    {
                        nrValidObservations++;
                        if (validObservations.Count < nrValidObservationsInReport)
                        {
                            validObservations.Add(new ValidObservationTuple<DwcObservationVerbatim, ProcessedObservation>
                            {
                                VerbatimObservation = verbatimObservation,
                                ProcessedObservation = processedObservation
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

                            foreach (var validationDefect in observationValidation.Defects)
                            {
                                if (!observationDefects.ContainsKey(validationDefect))
                                {
                                    observationDefects.Add(validationDefect, 0);
                                }

                                observationDefects[validationDefect]++;
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
                    SosVocabulary = _fieldMappings[pair.Key].Values.Select(v => new ProcessedFieldMapValue() {Id = v.Id, Value = v.Value}).ToList()
                }).ToList();

            var remarks = validationRemarksBuilder.CreateRemarks();
            if (!remarks.HasItems())
            {
                remarks.Add("Everything looks ok. Great!");
            }
            return new DwcaDataValidationReport<DwcObservationVerbatim, ProcessedObservation>
            {
                Settings = new { NrValidObservationsInReport = nrValidObservationsInReport, NrInvalidObservationsInReport = nrInvalidObservationsInReport},
                Summary = new DwcaDataValidationReportSummary
                {
                    TotalNumberOfObservationsInFile = totalNumberOfObservations,
                    NrObservationsProcessed = nrProcessedObservations,
                    NrValidObservations = nrValidObservations,
                    NrInvalidObservations = nrInvalidObservations,
                    Remarks = remarks,
                    ObservationDefects = observationDefects.OrderByDescending(m => m.Value).Select(m => new DefectItem { Defect = m.Key, Count = m.Value}).ToList()
                },
                InvalidObservations = invalidObservations,
                ValidObservations = validObservations,
                DictionaryValues = distinctValuesSummaries
            };
        }

        private void UpdateTermDictionaryValueSummary(
            ProcessedObservation processedObservation,
            DwcObservationVerbatim verbatimObservation,
            Dictionary<FieldMappingFieldId, Dictionary<ProcessedFieldMapValue, int>> processedFieldValues,
            Dictionary<FieldMappingFieldId, Dictionary<ProcessedFieldMapValue, HashSet<string>>> verbatimFieldValues)
        {
            UpdateTermDictionaryValue(FieldMappingFieldId.LifeStage, verbatimObservation.LifeStage, processedObservation.Occurrence.LifeStage, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(FieldMappingFieldId.AccessRights, verbatimObservation.AccessRights, processedObservation.AccessRights, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(FieldMappingFieldId.Gender, verbatimObservation.Sex, processedObservation.Occurrence.Gender, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(FieldMappingFieldId.ValidationStatus, verbatimObservation.IdentificationVerificationStatus, processedObservation.Identification.ValidationStatus, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(FieldMappingFieldId.Institution, verbatimObservation.InstitutionCode, processedObservation.InstitutionCode, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(FieldMappingFieldId.Unit, verbatimObservation.OrganismQuantityType, processedObservation.Occurrence.OrganismQuantityUnit, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(FieldMappingFieldId.BasisOfRecord, verbatimObservation.BasisOfRecord, processedObservation.BasisOfRecord, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(FieldMappingFieldId.EstablishmentMeans, verbatimObservation.EstablishmentMeans, processedObservation.Occurrence.EstablishmentMeans, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(FieldMappingFieldId.Type, verbatimObservation.Type, processedObservation.Type, processedFieldValues, verbatimFieldValues);
        }

        private void UpdateTermDictionaryValue(FieldMappingFieldId fieldMappingFieldId,
            string verbatimValue,
            ProcessedFieldMapValue processedFieldMapValue,
            Dictionary<FieldMappingFieldId, Dictionary<ProcessedFieldMapValue, int>> processedFieldValues,
            Dictionary<FieldMappingFieldId, Dictionary<ProcessedFieldMapValue, HashSet<string>>> verbatimFieldValues)
        {
            if (processedFieldMapValue != null)
            {
                if (!processedFieldValues[fieldMappingFieldId].ContainsKey(processedFieldMapValue))
                {
                    processedFieldValues[fieldMappingFieldId].Add(processedFieldMapValue, 0);
                }
                processedFieldValues[fieldMappingFieldId][processedFieldMapValue]++;

                if (!string.IsNullOrWhiteSpace(verbatimValue))
                {
                    if (!verbatimFieldValues[fieldMappingFieldId].ContainsKey(processedFieldMapValue))
                    {
                        verbatimFieldValues[fieldMappingFieldId].Add(processedFieldMapValue, new HashSet<string>());
                    }
                    verbatimFieldValues[fieldMappingFieldId][processedFieldMapValue].Add(verbatimValue);
                }
            }
        }
    }

    public static class ProcessedFieldMapValueExtension
    {
        public static bool IsCustomValue(this ProcessedFieldMapValue processedFieldMapValue)
        {
            return processedFieldMapValue != null && processedFieldMapValue.Id == -1;
        }
    }
}
