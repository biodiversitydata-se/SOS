using MongoDB.Driver;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Mvm;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Mvm;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Harvest.Factories.Validation
{
    /// <summary>
    /// MVM data validation manager.
    /// </summary>
    public class MvmDataValidationReportFactory : DataValidationReportFactoryBase<MvmSampleVerbatim>
    {
        private readonly IMvmObservationVerbatimRepository _mvmObservationVerbatimRepository;
        private MvmObservationFactory? _mvmObservationFactory;

    public MvmDataValidationReportFactory(
        IVocabularyRepository processedVocabularyRepository,
        IValidationManager validationManager,
        IAreaHelper areaHelper,
        IVocabularyValueResolver vocabularyValueResolver,
        ITaxonRepository processedTaxonRepository,
        IMvmObservationVerbatimRepository mvmObservationVerbatimRepository,
        IProcessTimeManager processTimeManager,
        ProcessConfiguration processConfiguration,
        ICache<int, Taxon> taxonCache,
        ICache<VocabularyId, Vocabulary> vocabularyCache)
        : base(processedVocabularyRepository, validationManager, areaHelper, vocabularyValueResolver, processedTaxonRepository, processTimeManager, processConfiguration, taxonCache, vocabularyCache)
    {
        _mvmObservationVerbatimRepository = mvmObservationVerbatimRepository;
    }

        protected override async Task<IAsyncCursor<MvmSampleVerbatim>> GetAllObservationsByCursorAsync(DataProvider dataProvider)
        {
            return await _mvmObservationVerbatimRepository.GetAllByCursorAsync();
        }

    protected override async Task<long> GetTotalObservationsCountAsync(DataProvider dataProvider)
    {
        return await _mvmObservationVerbatimRepository.CountAllDocumentsAsync();
    }

        protected override async Task<Observation?> CreateProcessedObservationAsync(MvmSampleVerbatim verbatimObservation, DataProvider dataProvider)
        {
            throw new NotImplementedException();
        }

        protected override Task<IEnumerable<Observation>?> CreateProcessedObservationsAsync(MvmSampleVerbatim verbatimSample, DataProvider dataProvider)
        {
            var observationFactory = GetObservationFactory(dataProvider);
            var processedObservations = observationFactory.CreateProcessedObservations(verbatimSample, false);
            //AddAreaDataToProcessedLocation(processedObservations);
            return Task.FromResult(processedObservations);
        }

        protected override void ValidateVerbatimData(MvmSampleVerbatim verbatimObservation, DwcaValidationRemarksBuilder validationRemarksBuilder)
        {
            validationRemarksBuilder.NrValidatedObservations++;

            //if (!verbatimObservation.CoordinateUncertaintyInMeters.HasValue)
            //{
            //    validationRemarksBuilder.NrMissingCoordinateUncertaintyInMeters++;
            //}
        }

        protected override void UpdateTermDictionaryValueSummary(Observation processedObservation, MvmSampleVerbatim verbatimObservation, Dictionary<VocabularyId, Dictionary<VocabularyValue, int>> processedFieldValues, Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>> verbatimFieldValues)
        {
            // MVM doesn't contain any vocabulary fields.
        }

        protected override void ValidateVerbatimTaxon(
            MvmSampleVerbatim verbatimObservation,
            Observation processedObservation,
            HashSet<string> nonMatchingTaxonIds, 
            HashSet<string> nonMatchingScientificNames)
        {
            if (!string.IsNullOrEmpty(processedObservation.Taxon.VerbatimId))
            {
                nonMatchingTaxonIds.Add(processedObservation.Taxon.VerbatimId);
            }
        }

        protected override object GetVerbatimObservationForReport(MvmSampleVerbatim verbatimObservation, Observation processedObservation)
        {
            var occurrenceId = processedObservation.Occurrence?.OccurrenceId;
            if (occurrenceId != null && verbatimObservation.Observations != null)
            {
                const string prefix = "urn:lsid:mvm:observation:";
                if (occurrenceId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(occurrenceId.AsSpan(prefix.Length), out var observationId))
                {
                    var match = verbatimObservation.Observations.FirstOrDefault(o => o.ObservationId == observationId);
                    if (match != null)
                        return verbatimObservation.WithSingleObservation(match);
                }
            }
            return verbatimObservation;
        }
        
        private MvmObservationFactory GetObservationFactory(DataProvider dataProvider)
        {
            if (_mvmObservationFactory == null)
            {
                _mvmObservationFactory = new MvmObservationFactory(
                    dataProvider,
                    _taxonById,
                    _dwcaVocabularyById,
                    _areaHelper,
                    _processTimeManager,
                    _processedVocabularyRepository,
                    ProcessConfiguration);
            }

            return _mvmObservationFactory;
        }        
    }
}