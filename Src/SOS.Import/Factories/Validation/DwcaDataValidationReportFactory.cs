using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Process.Processors.DarwinCoreArchive;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Import.Factories.Validation
{
    /// <summary>
    /// DwC data validation manager.
    /// </summary>
    public class DwcaDataValidationReportFactory : DataValidationReportFactoryBase<DwcObservationVerbatim>
    {
        private DwcaObservationFactory _dwcaObservationFactory;
        private readonly IVerbatimClient _verbatimClient;
        private readonly ILoggerFactory _loggerFactory;

        public DwcaDataValidationReportFactory(
            IVerbatimClient verbatimClient,
            IVocabularyRepository processedVocabularyRepository,
            IValidationManager validationManager,
            IAreaHelper areaHelper,
            IVocabularyValueResolver vocabularyValueResolver,
            ITaxonRepository processedTaxonRepository,
            ILoggerFactory loggerFactory)
            : base(processedVocabularyRepository, validationManager, areaHelper, vocabularyValueResolver, processedTaxonRepository)
        {
            _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        protected override async Task<IAsyncCursor<DwcObservationVerbatim>> GetAllObservationsByCursorAsync(DataProvider dataProvider)
        {
            using var dwcArchiveVerbatimRepository = new DarwinCoreArchiveVerbatimRepository(
                    dataProvider,
                    _verbatimClient,
                    new Logger<DarwinCoreArchiveVerbatimRepository>(_loggerFactory));
           
            return await dwcArchiveVerbatimRepository.GetAllByCursorAsync();
        }

        protected override async Task<long> GetTotalObservationsCountAsync(DataProvider dataProvider)
        {
            using var dwcArchiveVerbatimRepository = new DarwinCoreArchiveVerbatimRepository(
                dataProvider,
                _verbatimClient,
                new Logger<DarwinCoreArchiveVerbatimRepository>(_loggerFactory));

            return await dwcArchiveVerbatimRepository.CountAllDocumentsAsync();
        }

        protected override async Task<Observation> CreateProcessedObservationAsync(DwcObservationVerbatim verbatimObservation, DataProvider dataProvider)
        {
            var processedObservation = GetObservationFactory(dataProvider).CreateProcessedObservation(verbatimObservation);
            _areaHelper.AddAreaDataToProcessedObservation(processedObservation);
            return processedObservation;
        }

        protected override void ValidateVerbatimTaxon(
            DwcObservationVerbatim verbatimObservation,
            HashSet<string> nonMatchingTaxonIds,
            HashSet<string> nonMatchingScientificNames)
        {
            nonMatchingScientificNames.Add(verbatimObservation.ScientificName);
            nonMatchingTaxonIds.Add(verbatimObservation.TaxonID);
        }

        protected override void ValidateVerbatimData(DwcObservationVerbatim verbatimObservation, DwcaValidationRemarksBuilder validationRemarksBuilder)
        {
            validationRemarksBuilder.NrValidatedObservations++;

            if (string.IsNullOrWhiteSpace(verbatimObservation.CoordinateUncertaintyInMeters))
            {
                validationRemarksBuilder.NrMissingCoordinateUncertaintyInMeters++;
            }

            if (string.IsNullOrWhiteSpace(verbatimObservation.IdentificationVerificationStatus))
            {
                validationRemarksBuilder.NrMissingIdentificationVerificationStatus++;
            }
        }

        protected override void UpdateTermDictionaryValueSummary(
            Observation processedObservation,
            DwcObservationVerbatim verbatimObservation,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, int>> processedFieldValues,
            Dictionary<VocabularyId, Dictionary<VocabularyValue, HashSet<string>>> verbatimFieldValues)
        {
            UpdateTermDictionaryValue(VocabularyId.LifeStage, verbatimObservation.LifeStage, processedObservation.Occurrence.LifeStage, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.AccessRights, verbatimObservation.AccessRights, processedObservation.AccessRights, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Sex, verbatimObservation.Sex, processedObservation.Occurrence.Sex, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.ReproductiveCondition, verbatimObservation.ReproductiveCondition, processedObservation.Occurrence.ReproductiveCondition, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Behavior, verbatimObservation.Behavior, processedObservation.Occurrence.Behavior, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.VerificationStatus, verbatimObservation.IdentificationVerificationStatus, processedObservation.Identification.ValidationStatus, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.VerificationStatus, verbatimObservation.IdentificationVerificationStatus, processedObservation.Identification.VerificationStatus, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Institution, verbatimObservation.InstitutionCode, processedObservation.InstitutionCode, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Unit, verbatimObservation.OrganismQuantityType, processedObservation.Occurrence.OrganismQuantityUnit, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.BasisOfRecord, verbatimObservation.BasisOfRecord, processedObservation.BasisOfRecord, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.EstablishmentMeans, verbatimObservation.EstablishmentMeans, processedObservation.Occurrence.EstablishmentMeans, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Type, verbatimObservation.Type, processedObservation.Type, processedFieldValues, verbatimFieldValues);
        }

        private DwcaObservationFactory GetObservationFactory(DataProvider dataProvider)
        {
            if (_dwcaObservationFactory == null)
            {
                var dwcaVocabularyById = DwcaObservationFactory.GetVocabulariesDictionary(
                    ExternalSystemId.DarwinCore,
                    _vocabularyById.Values,
                    true);

                _dwcaObservationFactory = new DwcaObservationFactory(
                    dataProvider,
                    _taxonById,
                    dwcaVocabularyById,
                    _areaHelper);
            }

            return _dwcaObservationFactory;
        }
    }
}