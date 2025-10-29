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
        public override TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projectInfoRepository"></param>       
        /// <param name="logger"></param>
        public ProjectCache(IProjectInfoRepository projectInfoRepository, ILogger<CacheBase<int, ProjectInfo>> logger) : base(projectInfoRepository, logger)
        {
            
        }
    }
}
