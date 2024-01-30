using SOS.Lib.Models.Processed.Observation;
using SOS.Shared.Api.Dtos.Vocabulary;

namespace SOS.Shared.Api.Extensions.Dto
{
    /// <summary>
    /// Extension methods for project
    /// </summary>
    public static class ProjectExtensions
    {
      
        /// <summary>
        /// Cast object to dto
        /// </summary>
        /// <param name="projectInfo"></param>
        /// <returns></returns>
        public static ProjectDto ToDto(this ProjectInfo projectInfo)
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

        /// <summary>
        /// Cast multiple project objects to dto's
        /// </summary>
        /// <param name="projectInfos"></param>
        /// <returns></returns>
        public static IEnumerable<ProjectDto> ToProjectDtos(this IEnumerable<ProjectInfo> projectInfos)
        {
            return projectInfos.Select(vocabulary => vocabulary.ToDto());
        }
    }
}