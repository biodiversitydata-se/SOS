using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.ClamPortal;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Harvest.Factories.Validation
{
    /// <summary>
    /// Clamp Portal validation manager.
    /// </summary>
    public class ClamPortalDataValidationReportFactory : DataValidationReportFactoryBase<ClamObservationVerbatim>
    {
        private readonly IClamObservationVerbatimRepository _clamPortalObservationVerbatimRepository;
        private ClamPortalObservationFactory _clamPortalObservationFactory;

        public ClamPortalDataValidationReportFactory(
            IVocabularyRepository processedVocabularyRepository,
            IValidationManager validationManager,
            IAreaHelper areaHelper,
            IVocabularyValueResolver vocabularyValueResolver,
            ITaxonRepository processedTaxonRepository,
            IClamObservationVerbatimRepository clamPortalObservationVerbatimRepository,
            IProcessTimeManager processTimeManager) 
            : base(processedVocabularyRepository, validationManager, areaHelper, vocabularyValueResolver, processedTaxonRepository, processTimeManager)
        {
            _clamPortalObservationVerbatimRepository = clamPortalObservationVerbatimRepository;
        }

        protected override async Task<IAsyncCursor<ClamObservationVerbatim>> GetAllObservationsByCursorAsync(DataProvider dataProvider)
        {
            return await _clamPortalObservationVerbatimRepository.GetAllByCursorAsync();
        }

        protected override async Task<long> GetTotalObservationsCountAsync(DataProvider dataProvider)
        {
            return await _clamPortalObservationVerbatimRepository.CountAllDocumentsAsync();
        }

        protected override async Task<Observation> CreateProcessedObservationAsync(ClamObservationVerbatim verbatimObservation, DataProvider dataProvider)
        {
            var processedObservation = GetObservationFactory(dataProvider).CreateProcessedObservation(verbatimObservation, false);
            _areaHelper.AddAreaDataToProcessedLocation(processedObservation.Location);
            return processedObservation;
        }

        protected override void ValidateVerbatimTaxon(
            ClamObservationVerbatim verbatimObservation,
            HashSet<string> nonMatchingTaxonIds,
            HashSet<string> nonMatchingScientificNames)
        {
            if (verbatimObservation.DyntaxaTaxonId.HasValue)
            {
                nonMatchingTaxonIds.Add(verbatimObservation.DyntaxaTaxonId.Value.ToString());
            }
        }

        protected override void ValidateVerbatimData(ClamObservationVerbatim verbatimObservation, DwcaValidationRemarksBuilder validationRemarksBuilder)
        {
            validationRemarksBuilder.NrValidatedObservations++;

            if (!verbatimObservation.CoordinateUncertaintyInMeters.HasValue)
            {
                validationRemarksBuilder.NrMissingCoordinateUncertaintyInMeters++;
            }
        }

        protected override void UpdateTermDictionaryValueSummary(
            Observation processedObservation,
            ClamObservationVerbatim verbatimObservation,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, int>> processedFieldValues,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>> verbatimFieldValues)
        {
            // Clam Portal doesn't contain any vocabulary fields.
        }

        private ClamPortalObservationFactory GetObservationFactory(DataProvider dataProvider)
        {
            if (_clamPortalObservationFactory == null)
            {
                _clamPortalObservationFactory = new ClamPortalObservationFactory(
                    dataProvider,
                    _taxonById,
                    _areaHelper,
                    _processTimeManager);
            }

            return _clamPortalObservationFactory;
        }
    }
}