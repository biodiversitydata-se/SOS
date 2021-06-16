using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Sers;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Processors.Sers;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Import.Factories.Validation
{
    /// <summary>
    /// SERS data validation manager.
    /// </summary>
    public class SersDataValidationReportFactory : DataValidationReportFactoryBase<SersObservationVerbatim>
    {
        private readonly ISersObservationVerbatimRepository _sersObservationVerbatimRepository;
        private SersObservationFactory _sersObservationFactory;

        public SersDataValidationReportFactory(
            IVocabularyRepository processedVocabularyRepository,
            IValidationManager validationManager,
            IAreaHelper areaHelper,
            IVocabularyValueResolver vocabularyValueResolver,
            ITaxonRepository processedTaxonRepository,
            ISersObservationVerbatimRepository sersObservationVerbatimRepository) 
            : base(processedVocabularyRepository, validationManager, areaHelper, vocabularyValueResolver, processedTaxonRepository)
        {
            _sersObservationVerbatimRepository = sersObservationVerbatimRepository;
        }

        protected override async Task<IAsyncCursor<SersObservationVerbatim>> GetAllObservationsByCursorAsync(DataProvider dataProvider)
        {
            return await _sersObservationVerbatimRepository.GetAllByCursorAsync();
        }

        protected override async Task<long> GetTotalObservationsCountAsync(DataProvider dataProvider)
        {
            return await _sersObservationVerbatimRepository.CountAllDocumentsAsync();
        }

        protected override async Task<Observation> CreateProcessedObservationAsync(SersObservationVerbatim verbatimObservation, DataProvider dataProvider)
        {
            var processedObservation = GetObservationFactory(dataProvider).CreateProcessedObservation(verbatimObservation);
            _areaHelper.AddAreaDataToProcessedObservation(processedObservation);
            return processedObservation;
        }

        protected override void ValidateVerbatimTaxon(
            SersObservationVerbatim verbatimObservation,
            HashSet<string> nonMatchingTaxonIds,
            HashSet<string> nonMatchingScientificNames)
        {
            nonMatchingTaxonIds.Add(verbatimObservation.DyntaxaTaxonId.ToString());
        }

        protected override void ValidateVerbatimData(SersObservationVerbatim verbatimObservation, DwcaValidationRemarksBuilder validationRemarksBuilder)
        {
            validationRemarksBuilder.NrValidatedObservations++;

            if (!verbatimObservation.CoordinateUncertaintyInMeters.HasValue)
            {
                validationRemarksBuilder.NrMissingCoordinateUncertaintyInMeters++;
            }
        }

        protected override void UpdateTermDictionaryValueSummary(
            Observation processedObservation,
            SersObservationVerbatim verbatimObservation,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, int>> processedFieldValues,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>> verbatimFieldValues)
        {
            // SERS doesn't contain any vocabulary fields.
        }

        private SersObservationFactory GetObservationFactory(DataProvider dataProvider)
        {
            if (_sersObservationFactory == null)
            {
                _sersObservationFactory = new SersObservationFactory(
                    dataProvider,
                    _taxonById,
                    _areaHelper);
            }

            return _sersObservationFactory;
        }
    }
}