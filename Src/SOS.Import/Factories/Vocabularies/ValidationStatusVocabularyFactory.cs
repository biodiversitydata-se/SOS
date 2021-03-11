using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating validation status vocabulary.
    /// </summary>
    public class ValidationStatusVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        public ValidationStatusVocabularyFactory(IMetadataRepository artportalenMetadataRepository)
        {
            _artportalenMetadataRepository = artportalenMetadataRepository ??
                                             throw new ArgumentNullException(nameof(artportalenMetadataRepository));
        }

        protected override VocabularyId FieldId => VocabularyId.ValidationStatus;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>> GetVocabularyValues()
        {
            var validationStatusList = await _artportalenMetadataRepository.GetValidationStatusAsync();
            var vocabularyValues = base.ConvertToLocalizedVocabularyValues(validationStatusList.ToArray());
            vocabularyValues = vocabularyValues.Where(v => !_excludeArtportalenIds.Contains(v.Id)).ToList();
            //int id = vocabularyValues.Max(f => f.Id);
            vocabularyValues.Add(CreateVocabularyValue(0, "Verified", "Validerad"));
            vocabularyValues.Add(CreateVocabularyValue(1, "Reported by expert", "Rapporterad av expert"));
            vocabularyValues = vocabularyValues.OrderBy(f => f.Id).ToList();
            return vocabularyValues;
        }

        private readonly int[] _excludeArtportalenIds =
        {
            50 // Rejected (Underkänd)
        };

        protected override List<ExternalSystemMapping> GetExternalSystemMappings(
            ICollection<VocabularyValueInfo> vocabularyValues)
        {
            return new List<ExternalSystemMapping>
            {
                GetArtportalenExternalSystemMapping(vocabularyValues),
                GetDarwinCoreExternalSystemMapping(vocabularyValues)
            };
        }

        private ExternalSystemMapping GetDarwinCoreExternalSystemMapping(
            ICollection<VocabularyValueInfo> vocabularyValues)
        {
            var externalSystemMapping = new ExternalSystemMapping
            {
                Id = ExternalSystemId.DarwinCore,
                Name = ExternalSystemId.DarwinCore.ToString(),
                Description = "The Darwin Core format (https://dwc.tdwg.org/terms/)",
                Mappings = new List<ExternalSystemMappingField>()
            };

            var dwcMappingSynonyms = GetDwcMappingSynonyms();
            var dwcMappings = CreateDwcMappings(vocabularyValues, dwcMappingSynonyms);
            var mappingField = new ExternalSystemMappingField
            {
                Key = VocabularyMappingKeyFields.DwcIdentificationVerificationStatus,
                Description = "The identificationVerificationStatus term (http://rs.tdwg.org/dwc/terms/identificationVerificationStatus)",
                Values = dwcMappings.Select(pair => new ExternalSystemMappingValue { Value = pair.Key, SosId = pair.Value }).ToList()
            };

            externalSystemMapping.Mappings.Add(mappingField);
            return externalSystemMapping;
        }

        private Dictionary<string, string> GetDwcMappingSynonyms()
        {
            return new Dictionary<string, string>
            {
                {"unverified", "Unvalidated"}
            };
        }
    }
}