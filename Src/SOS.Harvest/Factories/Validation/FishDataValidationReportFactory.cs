﻿using MongoDB.Driver;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.FishData;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.FishData;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Harvest.Factories.Validation
{
    /// <summary>
    /// FishData validation manager.
    /// </summary>
    public class FishDataValidationReportFactory : DataValidationReportFactoryBase<FishDataObservationVerbatim>
    {
        private readonly IFishDataObservationVerbatimRepository _fishDataObservationVerbatimRepository;
        private FishDataObservationFactory? _fishdataObservationFactory;

        public FishDataValidationReportFactory(
            IVocabularyRepository processedVocabularyRepository,
            IValidationManager validationManager,
            IAreaHelper areaHelper,
            IVocabularyValueResolver vocabularyValueResolver,
            ITaxonRepository processedTaxonRepository,
            IFishDataObservationVerbatimRepository fishDataObservationVerbatimRepository,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ICache<int, Taxon> taxonCache,
            ICache<VocabularyId, Vocabulary> vocabularyCache)
            : base(processedVocabularyRepository, validationManager, areaHelper, vocabularyValueResolver, processedTaxonRepository, processTimeManager, processConfiguration, taxonCache, vocabularyCache)
        {
            _fishDataObservationVerbatimRepository = fishDataObservationVerbatimRepository;
        }

        protected override async Task<IAsyncCursor<FishDataObservationVerbatim>> GetAllObservationsByCursorAsync(DataProvider dataProvider)
        {
            return await _fishDataObservationVerbatimRepository.GetAllByCursorAsync();
        }

        protected override async Task<long> GetTotalObservationsCountAsync(DataProvider dataProvider)
        {
            return await _fishDataObservationVerbatimRepository.CountAllDocumentsAsync();
        }

        protected override async Task<Observation?> CreateProcessedObservationAsync(FishDataObservationVerbatim verbatimObservation, DataProvider dataProvider)
        {
            return await Task.Run(() =>
            {
                var processedObservation = GetObservationFactory(dataProvider).CreateProcessedObservation(verbatimObservation, false);
                _areaHelper.AddAreaDataToProcessedLocation(processedObservation?.Location);
                return processedObservation;
            });
        }

        protected override void ValidateVerbatimTaxon(
            FishDataObservationVerbatim verbatimObservation,
            HashSet<string> nonMatchingTaxonIds,
            HashSet<string> nonMatchingScientificNames)
        {
            nonMatchingTaxonIds.Add(verbatimObservation.DyntaxaTaxonId.ToString());
        }

        protected override void ValidateVerbatimData(FishDataObservationVerbatim verbatimObservation, DwcaValidationRemarksBuilder validationRemarksBuilder)
        {
            validationRemarksBuilder.NrValidatedObservations++;

            if (!verbatimObservation.CoordinateUncertaintyInMeters.HasValue)
            {
                validationRemarksBuilder.NrMissingCoordinateUncertaintyInMeters++;
            }
        }

        protected override void UpdateTermDictionaryValueSummary(
            Observation processedObservation,
            FishDataObservationVerbatim verbatimObservation,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, int>> processedFieldValues,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>> verbatimFieldValues)
        {
            // FishData doesn't contain any vocabulary fields.
        }

        private FishDataObservationFactory GetObservationFactory(DataProvider dataProvider)
        {
            if (_fishdataObservationFactory == null)
            {                
                _fishdataObservationFactory = new FishDataObservationFactory(
                    dataProvider,
                    _taxonById,
                    _dwcaVocabularyById,
                    _areaHelper,
                    _processTimeManager,
                    ProcessConfiguration);
            }

            return _fishdataObservationFactory;
        }
    }
}