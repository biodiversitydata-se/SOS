using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource.Interfaces;
using System;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Project cache.
    /// </summary>
    public class ProjectCache : CacheBase<int, ProjectInfo>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projectInfoRepository"></param>
        /// <param name="memoryCache"></param>
        /// <param name="logger"></param>
        public ProjectCache(IProjectInfoRepository projectInfoRepository, IMemoryCache memoryCache, ILogger<CacheBase<int, ProjectInfo>> logger) : base(projectInfoRepository, memoryCache, logger)
        {
            CacheDuration = TimeSpan.FromMinutes(10);
        }
    }
}
