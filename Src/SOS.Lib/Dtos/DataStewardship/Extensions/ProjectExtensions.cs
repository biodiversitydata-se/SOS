using SOS.Lib.Models.Processed.DataStewardship.Common;
using SOS.Lib.Dtos.DataStewardship.Enums;

namespace SOS.Lib.Dtos.DataStewardship.Extensions;

public static class ProjectExtensions
{
    extension(Project project)
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
