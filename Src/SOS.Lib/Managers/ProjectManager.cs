using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Managers.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SOS.Lib.Managers
{
    /// <summary>
    ///     Vocabulary manager.
    /// </summary>
    public class ProjectManager : IProjectManager
    {
        private readonly ILogger<IProjectManager> _logger;
        private readonly ICache<int, ProjectInfo> _projectCache;

        private async Task<IEnumerable<ProjectInfo>> GetPermittedAsync(string filter, int? userId)
        {
            if ((userId ?? 0).Equals(0))
            {
                userId = -1; // UserServiceUserId equals 0 if not found, set userid = -1 to not match them
            }
            try
            {
                return (await _projectCache.GetAllAsync())?.Where(p =>
                    (
                        !p.IsHidden ||
                        p.UserServiceUserId == userId ||
                        p.MemberIds.Contains(userId ?? 0)
                    ) &&
                    (
                        string.IsNullOrEmpty(filter) ||
                        (p.Category?.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                        (p.CategorySwedish?.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                        (p.Name?.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ?? false)
                    )
               );
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get projects");
                return null;
            }
        }

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

        /// <inheritdoc/>
        public async Task<IEnumerable<ProjectInfo>> GetAllAsync(bool includeHidden = false)
        {
            return (await _projectCache.GetAllAsync())?.Where(p => !p.IsHidden || includeHidden);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProjectInfo>> GetAsync(IEnumerable<int> projectIds)
        {
            if (!projectIds?.Any() ?? true)
            {
                return null!;
            }
            return (await _projectCache.GetAllAsync()).Where(p => !p.IsHidden && projectIds.Contains(p.Id));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProjectInfo>> GetAsync(string filter, int? userId)
        {
            return await GetPermittedAsync(filter, userId);
        }

        /// <inheritdoc/>
        public async Task<ProjectInfo> GetAsync(int id, int? userId)
        {
            return (await GetPermittedAsync(null, userId)).FirstOrDefault(p => p.Id.Equals(id));
        }
    }
}