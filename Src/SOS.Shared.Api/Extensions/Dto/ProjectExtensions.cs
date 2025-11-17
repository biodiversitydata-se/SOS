using SOS.Lib.Models.Processed.Observation;
using SOS.Shared.Api.Dtos.Vocabulary;

namespace SOS.Shared.Api.Extensions.Dto;

/// <summary>
/// Extension methods for project
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
                StartDate = projectInfo.StartDate,
                EndDate = projectInfo.EndDate,
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
            return projectInfos.Select(vocabulary => vocabulary.ToDto());
        }
    }
}