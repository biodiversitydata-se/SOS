using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource.Interfaces;

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
        public ProjectCache(IProjectInfoRepository projectInfoRepository) : base(projectInfoRepository)
        {

        }
    }
}
