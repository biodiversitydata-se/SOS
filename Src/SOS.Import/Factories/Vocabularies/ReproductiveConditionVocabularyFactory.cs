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
    ///     Class for creating ReproductiveCondition vocabulary.
    /// </summary>
    public class ReproductiveConditionVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        private readonly IMetadataRepository _metadataRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="metadataRepository"></param>
        public ReproductiveConditionVocabularyFactory(
            IMetadataRepository metadataRepository)
        {
            _metadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
        }

        protected override VocabularyId FieldId => VocabularyId.ReproductiveCondition;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>> GetVocabularyValues()
        {
            var activities = await _metadataRepository.GetActivitiesAsync();
            var selectedActivities = activities.Where(a => _artportalenIds.Contains(a.Id));
            var vocabularyValues = base.ConvertToLocalizedVocabularyValues(selectedActivities.ToArray());
            int id = activities.Max(f => f.Id);
            vocabularyValues.Add(CreateVocabularyValue(++id, "testes developed"));
            vocabularyValues.Add(CreateVocabularyValue(++id, "mating [not bird]"));
            vocabularyValues.Add(CreateVocabularyValue(++id, "not breeding/non-reproductive"));
            vocabularyValues.Add(CreateVocabularyValue(++id, "carrying food for young"));
            vocabularyValues.Add(CreateVocabularyValue(++id, "sterile"));
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
                Key = VocabularyMappingKeyFields.DwcReproductiveCondition,
                Description = "The reproductiveCondition term (http://rs.tdwg.org/dwc/terms/reproductiveCondition)",
                Values = dwcMappings.Select(pair => new ExternalSystemMappingValue { Value = pair.Key, SosId = pair.Value }).ToList()
            };

            externalSystemMapping.Mappings.Add(mappingField);
            return externalSystemMapping;
        }

        private readonly int[] _artportalenIds =
        {
            13,
            54,
            61,
            79,
            80,
            82,
            85,
            88,
            101
        };

        private Dictionary<string, string> GetDwcMappingSynonyms()
        {
            return new Dictionary<string, string>
            {
                
            };
        }
    }
    //public class ReproductiveConditionVocabularyFactory : DwcVocabularyFactoryBase
    //{
    //    protected override VocabularyId FieldId => VocabularyId.ReproductiveCondition;
    //    protected override bool Localized => true;

    //    protected override ICollection<VocabularyValueInfo> GetVocabularyValues()
    //    {
    //        int id = 0;
    //        var vocabularyValues = new List<VocabularyValueInfo>();
    //        vocabularyValues.Add(CreateVocabularyValue(id++, "brood patch", "ruvfläckar")); // AP id=13
    //        vocabularyValues.Add(CreateVocabularyValue(id++, "egg laying", "äggläggande")); // AP id=54
    //        vocabularyValues.Add(CreateVocabularyValue(id++, "trace of oestrous female", "spår från löpande hona")); // AP id=61
    //        vocabularyValues.Add(CreateVocabularyValue(id++, "lactating", "ammande hondjur")); // AP id=79
    //        vocabularyValues.Add(CreateVocabularyValue(id++, "pregnant female", "dräktig hona")); // AP id=80
    //        vocabularyValues.Add(CreateVocabularyValue(id++, "spawning", "leking")); // AP id=82
    //        vocabularyValues.Add(CreateVocabularyValue(id++, "female with offspring", "obs av hona med unge/ungar")); // AP id=85
    //        vocabularyValues.Add(CreateVocabularyValue(id++, "in breeding colours", "i lekdräkt")); // AP id=88
    //        vocabularyValues.Add(CreateVocabularyValue(id++, "breeding ground or offspring", "yngelplats med ungar")); // AP id=101
    //        vocabularyValues.Add(CreateVocabularyValue(id++, "testes developed", "testes developed"));
    //        vocabularyValues.Add(CreateVocabularyValue(id++, "mating [not bird]", "mating [not bird]"));
    //        vocabularyValues.Add(CreateVocabularyValue(id++, "not breeding/non-reproductive", "not breeding/non-reproductive"));
    //        vocabularyValues.Add(CreateVocabularyValue(id++, "carrying food for young", "carrying food for young"));
    //        vocabularyValues.Add(CreateVocabularyValue(id++, "sterile", "sterile"));

    //        return vocabularyValues;
    //    }

    //    protected override Dictionary<string, int> GetMappingSynonyms()
    //    {
    //        return null;
    //    }
    //}
}