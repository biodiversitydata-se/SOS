using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating Activity vocabulary.
    /// </summary>
    public class ActivityVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<ActivityVocabularyFactory> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        /// <param name="logger"></param>
        public ActivityVocabularyFactory(
            IMetadataRepository artportalenMetadataRepository,
            ILogger<ActivityVocabularyFactory> logger)
        {
            _artportalenMetadataRepository = artportalenMetadataRepository ??
                                             throw new ArgumentNullException(nameof(artportalenMetadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override VocabularyId FieldId => VocabularyId.Activity;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>> GetVocabularyValues()
        {
            var activities = await _artportalenMetadataRepository.GetActivitiesAsync();
            var vocabularyValues = ConvertToVocabularyValuesWithCategory(activities.ToArray());
            return vocabularyValues;
        }

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
                Key = VocabularyMappingKeyFields.Activity,
                Description = "Activity",
                Values = dwcMappings.Select(pair => new ExternalSystemMappingValue { Value = pair.Key, SosId = pair.Value }).ToList()
            };

            externalSystemMapping.Mappings.Add(mappingField);
            return externalSystemMapping;
        }

        private Dictionary<string, string> GetDwcMappingSynonyms()
        {
            return new Dictionary<string, string>
            {                
            };
        }
    }
}