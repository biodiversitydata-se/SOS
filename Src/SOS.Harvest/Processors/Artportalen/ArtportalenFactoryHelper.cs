using SOS.Lib.Models.Processed.Observation;
using Project = SOS.Lib.Models.Verbatim.Artportalen.Project;
using ProjectParameter = SOS.Lib.Models.Verbatim.Artportalen.ProjectParameter;

namespace SOS.Harvest.Processors.Artportalen
{
    public class ArtportalenFactoryHelper
    {
        /// <summary>
        /// Cast verbatim project to processed project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static Lib.Models.Processed.Observation.Project CreateProcessedProject(Project project)
        {
            if (project == null) return null;

            return new Lib.Models.Processed.Observation.Project
            {
                IsPublic = project.IsPublic,
                Category = project.Category,
                CategorySwedish = project.CategorySwedish,
                Description = project.Description,
                EndDate = project.EndDate?.ToUniversalTime(),
                Id = project.Id,
                Name = project.Name,
                Owner = project.Owner,
                ProjectURL = project.ProjectURL,
                StartDate = project.StartDate?.ToUniversalTime(),
                SurveyMethod = project.SurveyMethod,
                SurveyMethodUrl = project.SurveyMethodUrl,
                ProjectParameters = project.ProjectParameters?.Select(CreateProcessedProjectParameter)
            };
        }

        public static ProjectsSummary? CreateProjectsSummary(IEnumerable<Lib.Models.Processed.Observation.Project>? projects)
        {
            if (projects == null || !projects.Any()) return null;

            var projectsSummary = new ProjectsSummary();
            var projectsList = projects.ToArray();
            var project1 = projectsList[0];
            projectsSummary.Project1Id = project1.Id;
            projectsSummary.Project1Category = project1.CategorySwedish;
            projectsSummary.Project1Name = project1.Name;
            projectsSummary.Project1Url = project1.ProjectURL;
            projectsSummary.Project1Values = project1.ProjectParameters != null ? string.Join(", ", project1.ProjectParameters.Select(m => $"[{m.Name}={m.Value}]")) : null;

            if (projectsList.Length > 1)
            {
                var project2 = projectsList[1];
                projectsSummary.Project2Id = project2.Id;
                projectsSummary.Project2Category = project2.CategorySwedish;
                projectsSummary.Project2Name = project2.Name;
                projectsSummary.Project2Url = project2.ProjectURL;
                projectsSummary.Project2Values = project2.ProjectParameters != null ? string.Join(", ", project2.ProjectParameters.Select(m => $"[{m.Name}={m.Value}]")) : null;
            }

            return projectsSummary;
        }

        /// <summary>
        /// Cast verbatim project params to processed project params 
        /// </summary>
        /// <param name="projectParameter"></param>
        /// <returns></returns>
        public static Lib.Models.Processed.Observation.ProjectParameter CreateProcessedProjectParameter(ProjectParameter projectParameter)
        {
            if (projectParameter == null)
            {
                return null;
            }

            return new Lib.Models.Processed.Observation.ProjectParameter
            {
                Value = projectParameter.Value,
                DataType = projectParameter.DataType,
                Description = projectParameter.Description,
                Name = projectParameter.Name,
                Id = projectParameter.Id,
                Unit = projectParameter.Unit
            };
        }
    }
}