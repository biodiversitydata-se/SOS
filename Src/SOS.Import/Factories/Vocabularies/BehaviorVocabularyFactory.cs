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
    ///     Class for creating EstablishmentMeans field mapping.
    /// </summary>
    public class BehaviorVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        private readonly IMetadataRepository _metadataRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="metadataRepository"></param>
        public BehaviorVocabularyFactory(
            IMetadataRepository metadataRepository)
        {
            _metadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
        }

        protected override VocabularyId FieldId => VocabularyId.Behavior;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>> GetVocabularyValues()
        {
            var activities = await _metadataRepository.GetActivitiesAsync();
            var selectedActivities = activities.Where(a => _artportalenIds.Contains(a.Id));
            var vocabularyValues = base.ConvertToLocalizedVocabularyValues(selectedActivities.ToArray());
            int id = activities.Max(f => f.Id);
            vocabularyValues.Add(CreateVocabularyValue(++id, "roosting"));
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
                Key = VocabularyMappingKeyFields.DwcBehavior,
                Description = "The behavior term (http://rs.tdwg.org/dwc/terms/behavior)",
                Values = dwcMappings.Select(pair => new ExternalSystemMappingValue { Value = pair.Key, SosId = pair.Value }).ToList()
            };

            externalSystemMapping.Mappings.Add(mappingField);
            return externalSystemMapping;
        }

        private readonly int[] _artportalenIds =
        {
            3,
            4,
            6,
            7,
            8,
            11,
            12,
            14,
            15,
            16,
            17,
            18,
            19,
            20,
            24,
            25,
            26,
            28,
            31,
            32,
            33,
            34,
            35,
            36,
            37,
            38,
            39,
            40,
            49,
            55,
            56,
            57,
            60,
            66,
            67,
            84,
            89,
            90,
            91,
            92,
            93
        };

        private Dictionary<string, string> GetDwcMappingSynonyms()
        {
            return new Dictionary<string, string>
            {
                {"running", "running/crawling"}
            };
        }
    }
}