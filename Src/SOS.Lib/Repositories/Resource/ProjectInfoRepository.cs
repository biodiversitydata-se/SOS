using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Repositories.Resource
{
    /// <summary>
    ///     Project repository.
    /// </summary>
    public class ProjectInfoRepository : RepositoryBase<ProjectInfo, int>, IProjectInfoRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processClient"></param>
        /// <param name="logger"></param>
        public ProjectInfoRepository(
            IProcessClient processClient,
            ILogger<ProjectInfoRepository> logger) : base(processClient, logger)
        {
        }
    }
}