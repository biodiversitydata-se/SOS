using SOS.Lib.Models.Processed.Observation;
using SOS.Shared.Api.Dtos.Vocabulary;

namespace SOS.Shared.Api.Extensions.Dto
{
    public static class ProjectExtensions
    {
      
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

        public static IEnumerable<ProjectDto> ToProjectDtos(this IEnumerable<ProjectInfo> projectInfos)
        {
            return projectInfos.Select(vocabulary => vocabulary.ToDto());
        }
    }
}