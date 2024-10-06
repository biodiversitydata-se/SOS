using Microsoft.Extensions.Logging;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating institution vocabulary.
    /// </summary>
    public class InstitutionVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<InstitutionVocabularyFactory> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        /// <param name="logger"></param>
        public InstitutionVocabularyFactory(
            IMetadataRepository artportalenMetadataRepository,
            ILogger<InstitutionVocabularyFactory> logger)
        {
            _artportalenMetadataRepository = artportalenMetadataRepository ??
                                             throw new ArgumentNullException(nameof(artportalenMetadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override VocabularyId FieldId => VocabularyId.Institution;
        protected override bool Localized => false;

        protected override async Task<ICollection<VocabularyValueInfo>?> GetVocabularyValues()
        {
            var organizations = await _artportalenMetadataRepository.GetOrganizationsAsync();
            var vocabularyValues = base.ConvertToNonLocalizedVocabularyValues(organizations.ToArray());
            HashSet<string> vocabSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in vocabularyValues) {
                vocabSet.Add(item.Value);
            }

            List<string> dataProviderOrganizations = new List<string>
            {
                "SLU Swedish Species Information Centre"
                , "Department of Aquatic Resources, SLU"
                , "Environmental data MVM, SLU"
                , "Umeå University"
                , "Swedish Meteorological and Hydrological Institute (SMHI)"
                , "Swedish Museum of Natural History"                
                , "Station Linné"                
                , "Department of Biology, Lund University"
                , "Swedish Meterological and Hydrological Institute (SMHI)"
                , "Department of Aquatic Resources (SLU Aqua)"
                , "Swedish National Forest Inventory, Department of Forest Resource Management, SLU"
                , "Overstellar Solutions AB"
                , "iNaturalist.org"
                , "Department of Biology, Lund University"                
                , "Swedish Board of Agriculture"
                , "Department of Forest Resource Management, SLU"
                , "Gothenburg Natural History Museum"
                , "Lund University"
            };

            int id = 5000;
            foreach (var organization in dataProviderOrganizations)
            {
                if (!vocabSet.Contains(organization))
                {
                    vocabSet.Add(organization);
                    vocabularyValues.Add(CreateNonLocalizedVocabularyValue(++id, organization));
                }
            }

            return vocabularyValues;
        }
    }
}