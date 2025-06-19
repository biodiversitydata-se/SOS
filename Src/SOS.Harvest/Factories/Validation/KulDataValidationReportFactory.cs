﻿using MongoDB.Driver;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Kul;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Kul;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Harvest.Factories.Validation
{
    /// <summary>
    /// KUL data validation manager.
    /// </summary>
    public class KulDataValidationReportFactory : DataValidationReportFactoryBase<KulObservationVerbatim>
    {
        private readonly IKulObservationVerbatimRepository _kulObservationVerbatimRepository;
        private KulObservationFactory? _kulObservationFactory;

        public KulDataValidationReportFactory(
            IVocabularyRepository processedVocabularyRepository,
            IValidationManager validationManager,
            IAreaHelper areaHelper,
            IVocabularyValueResolver vocabularyValueResolver,
            ITaxonRepository processedTaxonRepository,
            IKulObservationVerbatimRepository kulObservationVerbatimRepository,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ICache<int, Taxon> taxonCache,
            ICache<VocabularyId, Vocabulary> vocabularyCache)
            : base(processedVocabularyRepository, validationManager, areaHelper, vocabularyValueResolver, processedTaxonRepository, processTimeManager, processConfiguration, taxonCache, vocabularyCache)
        {
            _kulObservationVerbatimRepository = kulObservationVerbatimRepository;
        }

        protected override async Task<IAsyncCursor<KulObservationVerbatim>> GetAllObservationsByCursorAsync(DataProvider dataProvider)
        {
            return await _kulObservationVerbatimRepository.GetAllByCursorAsync();
        }

        protected override async Task<long> GetTotalObservationsCountAsync(DataProvider dataProvider)
        {
            return await _kulObservationVerbatimRepository.CountAllDocumentsAsync();
        }

        protected override async Task<Observation?> CreateProcessedObservationAsync(KulObservationVerbatim verbatimObservation, DataProvider dataProvider)
        {
            return await Task.Run(() =>
            {
                var processedObservation = GetObservationFactory(dataProvider).CreateProcessedObservation(verbatimObservation, false);
                _areaHelper.AddAreaDataToProcessedLocation(processedObservation?.Location);
                return processedObservation;
            });
        }

        protected override void ValidateVerbatimTaxon(
            KulObservationVerbatim verbatimObservation,
            HashSet<string> nonMatchingTaxonIds,
            HashSet<string> nonMatchingScientificNames)
        {
            nonMatchingTaxonIds.Add(verbatimObservation.DyntaxaTaxonId.ToString());
        }

        protected override void ValidateVerbatimData(KulObservationVerbatim verbatimObservation, DwcaValidationRemarksBuilder validationRemarksBuilder)
        {
            validationRemarksBuilder.NrValidatedObservations++;

            if (!verbatimObservation.CoordinateUncertaintyInMeters.HasValue)
            {
                validationRemarksBuilder.NrMissingCoordinateUncertaintyInMeters++;
            }
        }

        protected override void UpdateTermDictionaryValueSummary(
            Observation processedObservation,
            KulObservationVerbatim verbatimObservation,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, int>> processedFieldValues,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>> verbatimFieldValues)
        {
            // KUL doesn't contain any vocabulary fields.
        }

        private KulObservationFactory GetObservationFactory(DataProvider dataProvider)
        {
            if (_kulObservationFactory == null)
            {
                _kulObservationFactory = new KulObservationFactory(
                    dataProvider,
                    _taxonById,
                    _dwcaVocabularyById,
                    _areaHelper,
                    _processTimeManager,
                    ProcessConfiguration);
            }

            return _kulObservationFactory;
        }
    }
}