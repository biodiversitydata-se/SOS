using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Processors.DarwinCoreArchive;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Import.Factories.Validation
{
    /// <summary>
    /// DwC data validation manager.
    /// </summary>
    public class DwcaDataValidationReportFactory : DataValidationReportFactoryBase<DwcObservationVerbatim>
    {
        private readonly IDarwinCoreArchiveVerbatimRepository _dwcaVerbatimRepository;
        private DwcaObservationFactory _dwcaObservationFactory;

        public DwcaDataValidationReportFactory(
            IVocabularyRepository processedVocabularyRepository,
            IValidationManager validationManager,
            IAreaHelper areaHelper,
            IVocabularyValueResolver vocabularyValueResolver,
            ITaxonRepository processedTaxonRepository,
            IDarwinCoreArchiveVerbatimRepository dwcaVerbatimRepository)
            : base(processedVocabularyRepository, validationManager, areaHelper, vocabularyValueResolver, processedTaxonRepository)
        {
            _dwcaVerbatimRepository = dwcaVerbatimRepository;
        }

        protected override async Task<IAsyncCursor<DwcObservationVerbatim>> GetAllObservationsByCursorAsync(DataProvider dataProvider)
        {
            return await _dwcaVerbatimRepository.GetAllByCursorAsync(dataProvider.Id, dataProvider.Identifier);
        }

        protected override async Task<long> GetTotalObservationsCountAsync(DataProvider dataProvider)
        {
            return await _dwcaVerbatimRepository.CountAllDocumentsAsync(dataProvider.Id, dataProvider.Identifier);
        }

        protected override Observation CreateProcessedObservation(DwcObservationVerbatim verbatimObservation, DataProvider dataProvider)
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
            UpdateTermDictionaryValue(VocabularyId.Gender, verbatimObservation.Sex, processedObservation.Occurrence.Gender, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.ReproductiveCondition, verbatimObservation.ReproductiveCondition, processedObservation.Occurrence.ReproductiveCondition, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.Behavior, verbatimObservation.Behavior, processedObservation.Occurrence.Behavior, processedFieldValues, verbatimFieldValues);
            UpdateTermDictionaryValue(VocabularyId.ValidationStatus, verbatimObservation.IdentificationVerificationStatus, processedObservation.Identification.ValidationStatus, processedFieldValues, verbatimFieldValues);
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