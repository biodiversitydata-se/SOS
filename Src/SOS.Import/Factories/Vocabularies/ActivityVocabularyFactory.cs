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
    ///     Class for creating Activity field mapping.
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
    }
}