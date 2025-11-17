using Hangfire.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using System;

namespace SOS.Lib.Cache;

/// <summary>
/// Area cache
/// </summary>
public class VocabularyCache : CacheBase<VocabularyId, Vocabulary>
{

    public override TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="vocabularyRepository"></param>        
    /// <param name="logger"></param>
    public VocabularyCache(IVocabularyRepository vocabularyRepository, ILogger<CacheBase<VocabularyId, Vocabulary>> logger) : base(vocabularyRepository, logger)
    {
        
    }
}
