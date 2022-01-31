using System.Linq;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Process.Processors.Artportalen
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