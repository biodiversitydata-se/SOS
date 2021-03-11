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

        protected override async Task<ICollection<VocabularyValueInfo>> GetVocabularyValues()
        {
            var organizations = await _artportalenMetadataRepository.GetOrganizationsAsync();
            var vocabularyValues = base.ConvertToNonLocalizedVocabularyValues(organizations.ToArray());
            return vocabularyValues;
        }
    }
}