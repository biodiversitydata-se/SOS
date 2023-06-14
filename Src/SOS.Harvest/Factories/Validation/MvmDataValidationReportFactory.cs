using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Mvm;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Mvm;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;
using SOS.Lib.Configuration.Process;

namespace SOS.Harvest.Factories.Validation
{
    /// <summary>
    /// MVM data validation manager.
    /// </summary>
    public class MvmDataValidationReportFactory : DataValidationReportFactoryBase<MvmObservationVerbatim>
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
            ProcessConfiguration processConfiguration) 
            : base(processedVocabularyRepository, validationManager, areaHelper, vocabularyValueResolver, processedTaxonRepository, processTimeManager, processConfiguration)
        {
            _mvmObservationVerbatimRepository = mvmObservationVerbatimRepository;
        }

        protected override async Task<IAsyncCursor<MvmObservationVerbatim>> GetAllObservationsByCursorAsync(DataProvider dataProvider)
        {
            return await _mvmObservationVerbatimRepository.GetAllByCursorAsync();
        }

        protected override async Task<long> GetTotalObservationsCountAsync(DataProvider dataProvider)
        {
            return await _mvmObservationVerbatimRepository.CountAllDocumentsAsync();
        }

        protected override async Task<Observation?> CreateProcessedObservationAsync(MvmObservationVerbatim verbatimObservation, DataProvider dataProvider)
        {
            return await Task.Run(() =>
            {
                var processedObservation = GetObservationFactory(dataProvider).CreateProcessedObservation(verbatimObservation, false);
                _areaHelper.AddAreaDataToProcessedLocation(processedObservation?.Location);
                return processedObservation;
            });
        }

        protected override void ValidateVerbatimTaxon(
            MvmObservationVerbatim verbatimObservation,
            HashSet<string> nonMatchingTaxonIds,
            HashSet<string> nonMatchingScientificNames)
        {
            nonMatchingTaxonIds.Add(verbatimObservation.DyntaxaTaxonId.ToString());
        }

        protected override void ValidateVerbatimData(MvmObservationVerbatim verbatimObservation, DwcaValidationRemarksBuilder validationRemarksBuilder)
        {
            validationRemarksBuilder.NrValidatedObservations++;

            if (!verbatimObservation.CoordinateUncertaintyInMeters.HasValue)
            {
                validationRemarksBuilder.NrMissingCoordinateUncertaintyInMeters++;
            }
        }

        protected override void UpdateTermDictionaryValueSummary(
            Observation processedObservation,
            MvmObservationVerbatim verbatimObservation,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, int>> processedFieldValues,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>> verbatimFieldValues)
        {
            // MVM doesn't contain any vocabulary fields.
        }

        private MvmObservationFactory GetObservationFactory(DataProvider dataProvider)
        {
            if (_mvmObservationFactory == null)
            {
                _mvmObservationFactory = new MvmObservationFactory(
                    dataProvider,
                    _taxonById,
                    _areaHelper,
                    _processTimeManager,
                    ProcessConfiguration);
            }

            return _mvmObservationFactory;
        }
    }
}