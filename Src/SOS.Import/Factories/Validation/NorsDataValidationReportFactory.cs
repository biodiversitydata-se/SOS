using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Nors;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Processors.Nors;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Import.Factories.Validation
{
    /// <summary>
    /// NORS data validation manager.
    /// </summary>
    public class NorsDataValidationReportFactory : DataValidationReportFactoryBase<NorsObservationVerbatim>
    {
        private readonly INorsObservationVerbatimRepository _norsObservationVerbatimRepository;
        private NorsObservationFactory _norsObservationFactory;

        public NorsDataValidationReportFactory(
            IVocabularyRepository processedVocabularyRepository,
            IValidationManager validationManager,
            IAreaHelper areaHelper,
            IVocabularyValueResolver vocabularyValueResolver,
            ITaxonRepository processedTaxonRepository,
            INorsObservationVerbatimRepository norsObservationVerbatimRepository) 
            : base(processedVocabularyRepository, validationManager, areaHelper, vocabularyValueResolver, processedTaxonRepository)
        {
            _norsObservationVerbatimRepository = norsObservationVerbatimRepository;
        }

        protected override async Task<IAsyncCursor<NorsObservationVerbatim>> GetAllObservationsByCursorAsync()
        {
            return await _norsObservationVerbatimRepository.GetAllByCursorAsync();
        }

        protected override async Task<long> GetTotalObservationsCountAsync()
        {
            return await _norsObservationVerbatimRepository.CountAllDocumentsAsync();
        }

        protected override Observation CreateProcessedObservation(NorsObservationVerbatim verbatimObservation, DataProvider dataProvider)
        {
            var processedObservation = GetObservationFactory(dataProvider).CreateProcessedObservation(verbatimObservation);
            _areaHelper.AddAreaDataToProcessedObservation(processedObservation);
            return processedObservation;
        }

        protected override void ValidateVerbatimTaxon(
            NorsObservationVerbatim verbatimObservation,
            HashSet<int> nonMatchingTaxonIds,
            HashSet<string> nonMatchingScientificNames)
        {
            nonMatchingTaxonIds.Add(verbatimObservation.DyntaxaTaxonId);
        }

        protected override void ValidateVerbatimData(NorsObservationVerbatim verbatimObservation, DwcaValidationRemarksBuilder validationRemarksBuilder)
        {
            validationRemarksBuilder.NrValidatedObservations++;

            if (!verbatimObservation.CoordinateUncertaintyInMeters.HasValue)
            {
                validationRemarksBuilder.NrMissingCoordinateUncertaintyInMeters++;
            }
        }

        protected override void UpdateTermDictionaryValueSummary(
            Observation processedObservation,
            NorsObservationVerbatim verbatimObservation,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, int>> processedFieldValues,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>> verbatimFieldValues)
        {
            // NORS doesn't contain any vocabulary fields.
        }

        private NorsObservationFactory GetObservationFactory(DataProvider dataProvider)
        {
            if (_norsObservationFactory == null)
            {
                _norsObservationFactory = new NorsObservationFactory(
                    dataProvider,
                    _taxonById);
            }

            return _norsObservationFactory;
        }
    }
}