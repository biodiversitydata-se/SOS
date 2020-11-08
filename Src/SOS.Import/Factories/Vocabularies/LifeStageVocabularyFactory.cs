using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating life stage field mapping.
    /// </summary>
    public class LifeStageVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        private readonly ILogger<LifeStageVocabularyFactory> _logger;
        private readonly IMetadataRepository _metadataRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="metadataRepository"></param>
        /// <param name="logger"></param>
        public LifeStageVocabularyFactory(
            IMetadataRepository metadataRepository,
            ILogger<LifeStageVocabularyFactory> logger)
        {
            _metadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override VocabularyId FieldId => VocabularyId.LifeStage;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>> GetVocabularyValues()
        {
            var stages = await _metadataRepository.GetStagesAsync();
            var vocabularyValues = base.ConvertToLocalizedVocabularyValues(stages.ToArray());
            int id = vocabularyValues.Max(f => f.Id);
            vocabularyValues.Add(CreateVocabularyValue(++id, "Nauplius larva", "Nauplius larv"));
            vocabularyValues.Add(CreateVocabularyValue(++id, "Copepodite", "Copepodit"));
            vocabularyValues.Add(CreateVocabularyValue(++id, "Sub adult"));
            vocabularyValues.Add(CreateVocabularyValue(++id, "zygote"));
            vocabularyValues.Add(CreateVocabularyValue(++id, "embryo"));
            vocabularyValues.Add(CreateVocabularyValue(++id, "seed"));
            vocabularyValues.Add(CreateVocabularyValue(++id, "spore"));
            vocabularyValues.Add(CreateVocabularyValue(++id, "pollen"));
            vocabularyValues.Add(CreateVocabularyValue(++id, "sporophyte"));
            vocabularyValues.Add(CreateVocabularyValue(++id, "gametophyte"));
            vocabularyValues.Add(CreateVocabularyValue(++id, "sperm"));
            vocabularyValues.Add(CreateVocabularyValue(++id, "dormant"));
            

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
                Key = VocabularyMappingKeyFields.DwcLifeStage,
                Description = "The lifeStage term (http://rs.tdwg.org/dwc/terms/lifeStage)",
                Values = dwcMappings.Select(pair => new ExternalSystemMappingValue { Value = pair.Key, SosId = pair.Value }).ToList()
            };

            externalSystemMapping.Mappings.Add(mappingField);
            return externalSystemMapping;
        }

        private Dictionary<string, string> GetDwcMappingSynonyms()
        {
            return new Dictionary<string, string>
            {
                {"egg", "egg"},
                {"eg (egg)", "egg"},
                {"eggs", "egg"},
                {"female with eggs", "egg"},
                {"nestling", "nestling"},
                {"pull", "nestling"},
                {"pull.", "nestling"},
                {"adult", "adult"},
                {"ad.", "adult"},
                {"adults", "adult"},
                {"ad (adult)", "adult"},
                {"adult female", "adult"},
                {"adult male", "adult"},
                {"larva/nymph", "larva/nymph"},
                {"nymph", "larva/nymph"},
                {"larvae", "larvae"},
                {"lv (larvae)", "larvae"},
                {"larva", "larvae"},
                {"juvenile", "juvenile"},
                {"juvenil", "juvenile"},
                {"jv (juvenile)", "juvenile"},
                {"juv.", "juvenile"},
                {"juv", "juvenile"},
                {"juveniles", "juvenile"},
                {"juvenile female", "juvenile"},
                {"juvenile male", "juvenile"},
                {"juveniles, males, fe", "juvenile"},
                {"pupa", "pupa"},
                {"imago/adult", "image/adult"},
                {"1st calendar year", "1st calendar year"},
                {"1K", "1st calendar year"},
                {"HY", "1st calendar year"},
                {"HY (hatching year)", "1st calendar year"},
                {"1K+", "at least 1st calendar year"},
                {"AHY", "at least 1st calendar year"},
                {"AHY (after hatching year)", "at least 1st calendar year"},
                {"2nd calendar year", "2nd calendar year"},
                {"2K", "2nd calendar year"},
                {"SY", "2nd calendar year"},
                {"SY (second year)", "2nd calendar year"},
                {"2K+", "at least 2nd calendar year"},
                {"ASY", "at least 2nd calendar year"},
                {"ASY (after second year)", "at least 2nd calendar year"},
                {"3rd calendar year", "3rd calendar year"},
                {"3K", "3rd calendar year"},
                {"TH", "3rd calendar year"},
                {"TH (third year)", "3rd calendar year"},
                {"3K+", "at least 3rd calendar year"},
                {"ATH", "at least 3rd calendar year"},
                {"ATH (after third year)", "at least 3rd calendar year"},
                {"fruiting", "in fruit"},
                {"male, immature", "immature"},
                {"female, immature", "immature"},
                {"immature male", "immature"},
                {"immature female", "immature"},
                {"embryos", "embryo"},
                {"NP (Nauplius)", "Nauplius larva"},
                {"NP", "Nauplius larva"},
                {"Nauplius larvae", "Nauplius larva"},
                {"C1 (Copepodite I)", "Copepodite"},
                {"C2 (Copepodite II)", "Copepodite"},
                {"C3 (Copepodite III)", "Copepodite"},
                {"C4 (Copepodite IV)", "Copepodite"},
                {"C5 (Copepodite V)", "Copepodite"},
                {"immature", "Sub adult"},
                {"omogen", "Sub adult"},
                {"subad.", "Sub adult"},
                {"subad", "Sub adult"},
                {"sub-adult", "Sub adult"},
                {"sub-adults", "Sub adult"},
                {"subadult male", "Sub adult"},
                {"subadult", "Sub adult"},
                {"subadults", "Sub adult"},
                {"sub adults", "Sub adult"},
                {"male, subadult", "Sub adult"},
                {"subadult female", "Sub adult"},
                {"subadult male, femal", "Sub adult"}
            };
        }
    }
}