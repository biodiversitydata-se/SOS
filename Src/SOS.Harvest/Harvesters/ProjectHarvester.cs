﻿using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Harvest.Harvesters
{
    /// <summary>
    ///     Class for harvest projects.
    /// </summary>
    public class ProjectHarvester : IProjectHarvester
    {
        private readonly IProjectRepository _artportalenProjectRepository;
        private readonly IProjectInfoRepository _projectInfoRepository;
        private readonly ICacheManager _cacheManager;
        private readonly ILogger<ProjectHarvester> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenProjectRepository"></param>
        /// <param name="projectInfoRepository"></param>
        /// <param name="cacheManager"></param>
        /// <param name="logger"></param>
        public ProjectHarvester(
            IProjectRepository artportalenProjectRepository,
            IProjectInfoRepository projectInfoRepository,
            ICacheManager cacheManager,
            ILogger<ProjectHarvester> logger)
        {
            _artportalenProjectRepository = artportalenProjectRepository ?? throw new ArgumentNullException(nameof(artportalenProjectRepository));
            _projectInfoRepository = projectInfoRepository ?? throw new ArgumentNullException(nameof(projectInfoRepository));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HarvestInfo> HarvestProjectsAsync()
        {
            var harvestInfo = new HarvestInfo(nameof(ProjectInfo), DateTime.Now);
            try
            {
                _logger.LogDebug("Start getting projects");

                var projectEntities = await _artportalenProjectRepository.GetProjectsAsync();
                if (!projectEntities?.Any() ?? true)
                {
                    harvestInfo.Status = RunStatus.Failed;
                    return harvestInfo;
                }

                var projects = projectEntities!
                    .Select(CastToProjectInfo)
                    .ToList()
                    .OrderBy(m => m.Id);
                _logger.LogDebug("Finish getting projects");

                if (await _projectInfoRepository.DeleteCollectionAsync())
                {
                    if (await _projectInfoRepository.AddCollectionAsync())
                    {
                        //Todo fix index creation await _projectInfoRepository.CreateIndexesAsync();
                        await _projectInfoRepository.AddManyAsync(projects);
                        // Clear observation api cache
                        await _cacheManager.ClearAsync(Cache.Projects);

                        // Update harvest info
                        harvestInfo.End = DateTime.Now;
                        harvestInfo.Status = RunStatus.Success;
                        harvestInfo.Count = projects?.Count() ?? 0;

                        _logger.LogDebug("Adding projects succeeded");
                        return harvestInfo;
                    }
                }

                _logger.LogDebug("Failed harvest of projects");
                harvestInfo.Status = RunStatus.Failed;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest of projects");
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }


        private ProjectInfo CastToProjectInfo(ProjectEntity projectEntity)
        {
            if (projectEntity == null)
            {
                return null!;
            }

            var projectInfo = new ProjectInfo
            {
                ControlingOrganisationId = projectEntity.ControlingOrganisationId,
                ControlingUserId = projectEntity.ControlingUserId,
                Id = projectEntity.Id,
                Name = projectEntity.Name,
                StartDate = projectEntity.StartDate,
                EndDate = projectEntity.EndDate,
                Category = projectEntity.Category,
                CategorySwedish = projectEntity.CategorySwedish,
                Description = projectEntity.Description?.Clean(),
                IsPublic = projectEntity.IsPublic,
                IsHidden = projectEntity.IsHideall,
                MemberIds = projectEntity.MembersIds,
                Owner = projectEntity.Owner,
                ProjectParameters = projectEntity.Parameters?.Select(p => new ProjectParameter
                {
                    DataType = p.DataType,
                    Description = p.Description,
                    Id = p.Id,
                    Name = p.Name,
                    Unit = p.Unit
                }),
                ProjectURL = projectEntity.ProjectURL,
                SurveyMethod = projectEntity.SurveyMethod,
                SurveyMethodUrl = projectEntity.SurveyMethodUrl,
                UserServiceUserId = projectEntity.UserServiceUserId
            };

            return projectInfo;
        }
    }
}