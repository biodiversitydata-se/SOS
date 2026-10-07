using SOS.Api.Contracts.Lookups.Responses;
using SOS.Lib.Models.Processed.Observation;
using SOS.Api.Contracts.DataStewardship.Responses;

namespace SOS.Lib.Extensions.Dto;

/// <summary>
/// Maps project models to API responses.
/// </summary>
public static class ProjectExtensions
{

    extension(ProjectInfo projectInfo)
    {
        /// <summary>
        /// Cast object to dto
        /// </summary>
        /// <returns></returns>
        public ProjectDto ToDto()
        {
            if (projectInfo == null)
            {
                return null!;
            }

            return new ProjectDto
            {
                Id = projectInfo.Id,
                Name = projectInfo.Name,
                StartDate = projectInfo.StartDate.ToUtcDateTimeOffset(),
                EndDate = projectInfo.EndDate.ToUtcDateTimeOffset(),
                Category = projectInfo.Category,
                CategorySwedish = projectInfo.CategorySwedish,
                Description = projectInfo.Description,
                IsPublic = projectInfo.IsPublic,
                Owner = projectInfo.Owner,
                ProjectURL = projectInfo.ProjectURL,
                SurveyMethod = projectInfo.SurveyMethod,
                SurveyMethodUrl = projectInfo.SurveyMethodUrl
            };
        }
    }

    extension(IEnumerable<ProjectInfo> projectInfos)
    {
        /// <summary>
        /// Cast multiple project objects to dto's
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ProjectDto> ToProjectDtos()
        {
            return projectInfos.Select(project => project.ToDto());
        }
    }

    extension(SOS.Lib.Models.Processed.DataStewardship.Common.Project project)
    {
        public DsProjectDto ToDto()
        {
            if (project == null) return null;
            return new DsProjectDto
            {
                ProjectCode = project.ProjectCode,
                ProjectID = project.ProjectId,
                ProjectType = project.ProjectType == null ? null : (DsProjectType)project.ProjectType
            };
        }
    }
}
