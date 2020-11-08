using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating DiscoveryMethod vocabulary.
    /// </summary>
    public class DiscoveryMethodVocabularyFactory : ArtportalenVocabularyFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<DiscoveryMethodVocabularyFactory> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        /// <param name="logger"></param>
        public DiscoveryMethodVocabularyFactory(IMetadataRepository artportalenMetadataRepository, ILogger<DiscoveryMethodVocabularyFactory> logger)
        {
            _artportalenMetadataRepository = artportalenMetadataRepository ?? throw new ArgumentNullException(nameof(artportalenMetadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override VocabularyId FieldId => VocabularyId.DiscoveryMethod;
        protected override bool Localized => true;

        protected override async Task<ICollection<VocabularyValueInfo>> GetVocabularyValues()
        {
            var discoveryMethods = await _artportalenMetadataRepository.GetDiscoveryMethodsAsync();
            var vocabularyValues = ConvertToLocalizedVocabularyValues(discoveryMethods.ToArray());
            return vocabularyValues;
        }
    }
}