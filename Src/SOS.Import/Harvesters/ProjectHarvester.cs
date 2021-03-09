using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Factories.Vocabularies;
using SOS.Import.Factories.Vocabularies.Interfaces;
using SOS.Import.Harvesters.Interfaces;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Import.Harvesters
{
    /// <summary>
    ///     Class for harvest projects.
    /// </summary>
    public class ProjectHarvester : IProjectHarvester
    {
        private readonly IProjectRepository _artportalenProjectRepository;
        private readonly IProjectInfoRepository _projectInfoRepository;
        private readonly SosApiConfiguration _sosApiConfiguration;
        private readonly ILogger<ProjectHarvester> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenProjectRepository"></param>
        /// <param name="projectInfoRepository"></param>
        /// <param name="sosApiConfiguration"></param>
        /// <param name="logger"></param>
        public ProjectHarvester(
            IProjectRepository artportalenProjectRepository,
            IProjectInfoRepository projectInfoRepository,
            SosApiConfiguration sosApiConfiguration,
            ILogger<ProjectHarvester> logger)
        {
            _artportalenProjectRepository = artportalenProjectRepository ?? throw new ArgumentNullException(nameof(artportalenProjectRepository));
            _projectInfoRepository = projectInfoRepository ?? throw new ArgumentNullException(nameof(projectInfoRepository));
            _sosApiConfiguration = sosApiConfiguration ?? throw new ArgumentNullException(nameof(sosApiConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HarvestInfo> HarvestProjectsAsync()
        {
            var harvestInfo = new HarvestInfo(DateTime.Now) { Id = nameof(ProjectInfo) };
            try
            {
                _logger.LogDebug("Start getting projects");

                var projectEntities = await _artportalenProjectRepository.GetProjectsAsync();
                if (!projectEntities.Any())
                {
                    harvestInfo.Status = RunStatus.Failed;
                    return harvestInfo;
                }

                var projects = projectEntities.Select(CastToProjectInfo).ToList();
                _logger.LogDebug("Finish getting projects");

                if (await _projectInfoRepository.DeleteCollectionAsync())
                {
                    if (await _projectInfoRepository.AddCollectionAsync())
                    {
                        await _projectInfoRepository.AddManyAsync(projects);
                        await ClearProjectsCache();

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

        private async Task ClearProjectsCache()
        {
            try
            {
                var client = new HttpClient();
                string requestUri = $"{_sosApiConfiguration.ObservationsApiAddress}Caches?cache={nameof(Cache.Projects)}";
                var response = await client.DeleteAsync(requestUri);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Projects cache cleared");
                }
                else
                {
                    _logger.LogInformation("Failed to clear projects cache");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to clear projects cache");
            }
        }

        private ProjectInfo CastToProjectInfo(ProjectEntity projectEntity)
        {
            ProjectInfo projectInfo = new ProjectInfo
            {
                Id = projectEntity.Id,
                Name = projectEntity.Name,
                StartDate = projectEntity.StartDate,
                EndDate = projectEntity.EndDate,
                Category = projectEntity.Category,
                Description = projectEntity.Description,
                IsPublic = projectEntity.IsPublic,
                Owner = projectEntity.Owner,
                ProjectURL =projectEntity.ProjectURL,
                SurveyMethod = projectEntity.SurveyMethod,
                SurveyMethodUrl = projectEntity.SurveyMethodUrl
            };

            return projectInfo;
        }
    }
}