using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Processed.Observation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            switch (accessRights.Id)
            {
                case 0:
                    return Models.Processed.DataStewardship.Enums.AccessRights.Publik;
                case 1:
                    return Models.Processed.DataStewardship.Enums.AccessRights.Begränsad;
                case 2:
                    return Models.Processed.DataStewardship.Enums.AccessRights.EjOffentlig;
                default:
                    throw new ArgumentOutOfRangeException($"ToAccessRights() don't have support for value {accessRights.Id}");
            }
        }

        public static Models.Processed.DataStewardship.Enums.Purpose ToPurpose(this ArtportalenDatasetMetadata.Purpose purpose)
        {
            switch (purpose.Id)
            {
                case 0:
                    return Models.Processed.DataStewardship.Enums.Purpose.NationellMiljöövervakning;
                case 1:
                    return Models.Processed.DataStewardship.Enums.Purpose.RegionalMiljöövervakning;
                case 2:
                    return Models.Processed.DataStewardship.Enums.Purpose.BiogeografiskUppföljning;
                default:
                    throw new ArgumentOutOfRangeException($"ToPurpose() don't have support for value {purpose.Id}");
            }
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
            switch (programmeArea.Id)
            {
                case 0:
                    return Models.Processed.DataStewardship.Enums.ProgrammeArea.BiogeografiskUppföljningAvNaturtyperOchArter;
                case 1:
                    return Models.Processed.DataStewardship.Enums.ProgrammeArea.Fjäll;
                case 2:
                    return Models.Processed.DataStewardship.Enums.ProgrammeArea.Jordbruksmark;
                case 3:
                    return Models.Processed.DataStewardship.Enums.ProgrammeArea.KustOchHav;
                case 4:
                    return Models.Processed.DataStewardship.Enums.ProgrammeArea.Landskap;
                case 5:
                    return Models.Processed.DataStewardship.Enums.ProgrammeArea.Skog;
                default:
                    throw new ArgumentOutOfRangeException($"ToProgrammeArea() don't have support for value {programmeArea.Id}");
            }
        }

        public static Models.Processed.DataStewardship.Enums.ProjectType ToProjectType(this ArtportalenDatasetMetadata.ProjectType projectType)
        {
            switch (projectType.Id)
            {
                case 0:
                    return Models.Processed.DataStewardship.Enums.ProjectType.Artportalenprojekt;
                case 1:
                    return Models.Processed.DataStewardship.Enums.ProjectType.Delprogram;
                case 2:
                    return Models.Processed.DataStewardship.Enums.ProjectType.Delsystem;
                case 3:
                    return Models.Processed.DataStewardship.Enums.ProjectType.GemensamtDelprogram;                
                default:
                    throw new ArgumentOutOfRangeException($"ToProjectType() don't have support for value {projectType.Id}");
            }
        }
    }
}
