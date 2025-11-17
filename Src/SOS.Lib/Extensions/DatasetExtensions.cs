using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Processed.Observation;
using System.Linq;

namespace SOS.Lib.Extensions;

public static class DatasetExtensions
{
    extension(ArtportalenDatasetMetadata apDataset)
    {
        public Dataset ToDataset()
        {
            var dataset = new Dataset()
            {
                Identifier = apDataset.Identifier,
                Metadatalanguage = apDataset.Metadatalanguage?.Clean(),
                Language = apDataset.Language?.Clean(),
                AccessRights = apDataset.DatasetAccessRights?.ToAccessRights(),
                DescriptionAccessRights = apDataset.DescriptionAccessRights?.Clean(),
                Purpose = apDataset.DatasetPurpose?.ToPurpose(),
                Assigner = apDataset.Assigner?.ToOrganisation(),
                Creator = apDataset.Creators?.Select(m => m.ToOrganisation()).ToList(),
                Methodology = apDataset.Methodologies?.Select(m => m.ToMethodology()).ToList(),
                OwnerinstitutionCode = apDataset.OwnerInstitution?.ToOrganisation(),
                Publisher = apDataset.Publisher?.ToOrganisation(),
                DataStewardship = apDataset.DataStewardship,
                StartDate = apDataset.StartDate,
                EndDate = apDataset.EndDate,
                Description = apDataset.Description?.Clean(),
                Title = apDataset.Title?.Clean(),
                Spatial = apDataset.Spatial?.Clean(),
                ProgrammeArea = apDataset.DatasetProgrammeArea?.ToProgrammeArea(),
                Project = apDataset.Projects?.Select(m => m.ToProject()).ToList()
            };

            return dataset;
        }
    }

    extension(ArtportalenDatasetMetadata.AccessRights accessRights)
    {
        public Models.Processed.DataStewardship.Enums.AccessRights ToAccessRights()
        {
            return accessRights.Id.ToEnum<Models.Processed.DataStewardship.Enums.AccessRights>();
        }
    }

    extension(ArtportalenDatasetMetadata.Purpose purpose)
    {
        public Models.Processed.DataStewardship.Enums.Purpose ToPurpose()
        {
            return purpose.Id.ToEnum<Models.Processed.DataStewardship.Enums.Purpose>();
        }
    }

    extension(ArtportalenDatasetMetadata.Methodology apMethodology)
    {
        public Methodology ToMethodology()
        {
            return new Methodology
            {
                MethodologyName = apMethodology.Name,
                MethodologyDescription = apMethodology.Description?.Clean(),
                MethodologyLink = apMethodology.Link,
                SpeciesList = apMethodology.SpeciesList?.Clean()
            };
        }
    }

    extension(ArtportalenDatasetMetadata.Organisation apOrganisation)
    {
        public Models.Processed.DataStewardship.Common.Organisation ToOrganisation()
        {
            return new Models.Processed.DataStewardship.Common.Organisation
            {
                OrganisationCode = apOrganisation.Code,
                OrganisationID = apOrganisation.Identifier
            };
        }
    }

    extension(ArtportalenDatasetMetadata.Project apProject)
    {
        public Models.Processed.DataStewardship.Common.Project ToProject()
        {
            return new Models.Processed.DataStewardship.Common.Project
            {
                ProjectId = apProject.ProjectId,
                ProjectCode = apProject.ProjectCode,
                ProjectType = apProject.ProjectType?.ToProjectType()
            };
        }
    }

    extension(ArtportalenDatasetMetadata.ProgrammeArea programmeArea)
    {
        public Models.Processed.DataStewardship.Enums.ProgrammeArea ToProgrammeArea()
        {
            return programmeArea.Id.ToEnum<Models.Processed.DataStewardship.Enums.ProgrammeArea>();
        }
    }

    extension(ArtportalenDatasetMetadata.ProjectType projectType)
    {
        public Models.Processed.DataStewardship.Enums.ProjectType ToProjectType()
        {
            return projectType.Id.ToEnum<Models.Processed.DataStewardship.Enums.ProjectType>();
        }
    }
}
