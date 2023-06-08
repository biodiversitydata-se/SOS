using SOS.Lib.Models.Processed.DataStewardship.Common;
using SOS.Observations.Api.Dtos.DataStewardship.Enums;

namespace SOS.Observations.Api.Dtos.DataStewardship.Extensions
{
    public static class ProjectExtensions
    {
        public static DsProjectDto ToDto(this Project project)
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
