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
using SOS.Process.Processors.Mvm;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Import.Factories.Validation
{
    /// <summary>
    /// MVM data validation manager.
    /// </summary>
    public class MvmDataValidationReportFactory : DataValidationReportFactoryBase<MvmObservationVerbatim>
    {
        private readonly IMvmObservationVerbatimRepository _mvmObservationVerbatimRepository;
        private MvmObservationFactory _mvmObservationFactory;

        public MvmDataValidationReportFactory(
            IVocabularyRepository processedVocabularyRepository,
            IValidationManager validationManager,
            IAreaHelper areaHelper,
            IVocabularyValueResolver vocabularyValueResolver,
            ITaxonRepository processedTaxonRepository,
            IMvmObservationVerbatimRepository mvmObservationVerbatimRepository) 
            : base(processedVocabularyRepository, validationManager, areaHelper, vocabularyValueResolver, processedTaxonRepository)
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

        protected override async Task<Observation> CreateProcessedObservationAsync(MvmObservationVerbatim verbatimObservation, DataProvider dataProvider)
        {
            var processedObservation = await GetObservationFactory(dataProvider).CreateProcessedObservationAsync(verbatimObservation);
            _areaHelper.AddAreaDataToProcessedObservation(processedObservation);
            return processedObservation;
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
                    _areaHelper);
            }

            return _mvmObservationFactory;
        }
    }
}