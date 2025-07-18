﻿using Microsoft.Extensions.Logging;
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
        public async Task<IEnumerable<ProjectInfo>> GetAllAsync()
        {
            return await _projectCache.GetAllAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProjectInfo>> GetAsync(IEnumerable<int> projectIds, int? userId = null)
        {
            if (!projectIds?.Any() ?? true)
            {
                return null!;
            }
            return (await GetAllAsync()).Where(p => projectIds.Contains(p.Id) && (userId == null || p.IsPublic || p.UserServiceUserId == userId));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProjectInfo>> GetAsync(string filter, int? userId)
        {
            try
            {
                return (await GetAllAsync()).Where(p => 
                    (
                        p.IsPublic || 
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

        /// <inheritdoc/>
        public async Task<ProjectInfo> GetAsync(int id, int? userId)
        {
            try
            {
                return (await GetAllAsync()).FirstOrDefault(p =>
                    (p.IsPublic || p.UserServiceUserId == userId) && p.Id.Equals(id)
               );
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to get project: {id}");
                return null;
            }
        }
    }
}