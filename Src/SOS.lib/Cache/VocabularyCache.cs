using Hangfire.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Area cache
    /// </summary>
    public class VocabularyCache : CacheBase<VocabularyId, Vocabulary>
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vocabularyRepository"></param>
        public VocabularyCache(IVocabularyRepository vocabularyRepository, IMemoryCache memoryCache, ILogger<CacheBase<VocabularyId, Vocabulary>> logger) : base(vocabularyRepository, memoryCache, logger)
        {

        }
    }
}
