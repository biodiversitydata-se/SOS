using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Processed.Observation;
using System.Linq;

namespace SOS.Lib.Extensions
{
    public static class DatasetExtensions
    {
        public static Dataset ToDataset(this ArtportalenDatasetMetadata apDataset)
        {
            var dataset = new Dataset()
            {
                Identifier = apDataset.Identifier,
                Metadatalanguage = apDataset.Metadatalanguage,
                Language = apDataset.Language,
                AccessRights = apDataset.DatasetAccessRights?.ToAccessRights(),
                DescriptionAccessRights = apDataset.DescriptionAccessRights,
                Purpose = apDataset.DatasetPurpose?.ToPurpose(),
                Assigner = apDataset.Assigner?.ToOrganisation(),
                Creator = apDataset.Creators?.Select(m => m.ToOrganisation()).ToList(),
                Methodology = apDataset.Methodologies?.Select(m => m.ToMethodology()).ToList(),
                OwnerinstitutionCode = apDataset.OwnerInstitution?.ToOrganisation(),
                Publisher = apDataset.Publisher?.ToOrganisation(),
                DataStewardship = apDataset.DataStewardship,
                StartDate = apDataset.StartDate,
                EndDate = apDataset.EndDate,
                Description = apDataset.Description,
                Title = apDataset.Title,
                Spatial = apDataset.Spatial,
                ProgrammeArea = apDataset.DatasetProgrammeArea?.ToProgrammeArea(),
                Project = apDataset.Projects?.Select(m => m.ToProject()).ToList()
            };

            return dataset;
        }

        public static Models.Processed.DataStewardship.Enums.AccessRights ToAccessRights(this ArtportalenDatasetMetadata.AccessRights accessRights)
        {
            return accessRights.Id.ToEnum<Models.Processed.DataStewardship.Enums.AccessRights>();
        }

        public static Models.Processed.DataStewardship.Enums.Purpose ToPurpose(this ArtportalenDatasetMetadata.Purpose purpose)
        {
            return purpose.Id.ToEnum<Models.Processed.DataStewardship.Enums.Purpose>();
        }

        public static Methodology ToMethodology(this ArtportalenDatasetMetadata.Methodology apMethodology)
        {
            return new Methodology
            {                
                MethodologyName = apMethodology.Name,
                MethodologyDescription = apMethodology.Description,
                MethodologyLink = apMethodology.Link,
                SpeciesList = apMethodology.SpeciesList
            };
        }

        public static SOS.Lib.Models.Processed.DataStewardship.Common.Organisation ToOrganisation(this ArtportalenDatasetMetadata.Organisation apOrganisation)
        {
            return new Models.Processed.DataStewardship.Common.Organisation
            {
                OrganisationCode = apOrganisation.Code,
                OrganisationID = apOrganisation.Identifier
            };
        }

        public static SOS.Lib.Models.Processed.DataStewardship.Common.Project ToProject(this ArtportalenDatasetMetadata.Project apProject)
        {
            return new Models.Processed.DataStewardship.Common.Project
            {
                ProjectId = apProject.ProjectId,
                ProjectCode = apProject.ProjectCode,
                ProjectType = apProject.ProjectType?.ToProjectType()
            };
        }

        public static Models.Processed.DataStewardship.Enums.ProgrammeArea ToProgrammeArea(this ArtportalenDatasetMetadata.ProgrammeArea programmeArea)
        {
            return programmeArea.Id.ToEnum<Models.Processed.DataStewardship.Enums.ProgrammeArea>();            
        }

        public static Models.Processed.DataStewardship.Enums.ProjectType ToProjectType(this ArtportalenDatasetMetadata.ProjectType projectType)
        {
            return projectType.Id.ToEnum<Models.Processed.DataStewardship.Enums.ProjectType>();
        }
    }
}
