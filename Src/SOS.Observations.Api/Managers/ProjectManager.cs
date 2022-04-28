using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Vocabulary manager.
    /// </summary>
    public class ProjectManager : IProjectManager
    {
        private readonly ILogger<IProjectManager> _logger;
        private readonly ICache<int, ProjectInfo> _projectCache;
        
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="projectCache"></param>
        /// <param name="logger"></param>
        public ProjectManager(
            ICache<int, ProjectInfo> projectCache,
            ILogger<IProjectManager> logger)
        {
            _projectCache = projectCache ?? throw new ArgumentNullException(nameof(projectCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ProjectInfo>> GetAllAsync()
        {
            return await _projectCache.GetAllAsync();
        }
    }
}