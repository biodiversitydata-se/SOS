using SOS.DataStewardship.Api.Models;
using SOS.Lib.Models.Processed.Dataset;
using SOS.Lib.Models.Shared;
using System.Data;

namespace SOS.DataStewardship.Api.Extensions
{
    public static class DtoExtensions
    {
        public static List<Dataset> ToDatasets(this IEnumerable<ObservationDataset> datasets)
        {
            if (datasets == null || !datasets.Any()) return null;             
            return datasets.Select(m => m.ToDataset()).ToList();
        }

        public static Dataset ToDataset(this ObservationDataset dataset)
        {
            if (dataset == null) return null;
                            
            return new Dataset
            {
                AccessRights = dataset.AccessRights.ToDatasetAccessRightsEnum(),
                Assigner = dataset.Assigner.ToOrganisation(),
                Creator = dataset.Creator.ToOrganisation(),
                DataStewardship = dataset.DataStewardship,
                Description = dataset.Description,
                EndDate = dataset.EndDate,
                Events = dataset.EventIds,
                Identifier = dataset.Identifier,
                Language = dataset.Language,
                Metadatalanguage = dataset.Metadatalanguage,
                Methodology = dataset.Methodology.ToMethodologies(),
                OwnerinstitutionCode = dataset.OwnerinstitutionCode.ToOrganisation(),
                ProjectCode = dataset.ProjectCode,
                ProjectID = dataset.ProjectId,
                Publisher = dataset.Publisher.ToOrganisation(),
                Purpose = dataset.Purpose.ToDatasetPurposeEnum(),
                Spatial = dataset.Spatial,
                StartDate = dataset.StartDate,
                Title = dataset.Title
            };
        }

        public static Dataset.PurposeEnum? ToDatasetPurposeEnum(this ObservationDataset.PurposeEnum? purposeEnum)
        {
            if (purposeEnum == null) return null;
            return (Dataset.PurposeEnum)purposeEnum;
        }

        public static Dataset.AccessRightsEnum? ToDatasetAccessRightsEnum(this ObservationDataset.AccessRightsEnum? accessRightsEnum)
        {
            if (accessRightsEnum == null) return null;
            return (Dataset.AccessRightsEnum)accessRightsEnum;
        }

        public static Organisation ToOrganisation(this ObservationDataset.Organisation organisation)
        {
            if (organisation == null) return null;
            return new Organisation
            {
                OrganisationID = organisation.OrganisationID,
                OrganisationCode = organisation.OrganisationCode
            };
        }

        public static List<Methodology> ToMethodologies(this IEnumerable<ObservationDataset.MethodologyModel> methodologies)
        {
            if (methodologies == null || !methodologies.Any()) return null;
            return methodologies.Select(m => m.ToMethodology()).ToList();
        }

        public static Methodology ToMethodology(this ObservationDataset.MethodologyModel methodology)
        {
            if (methodology == null) return null;
            return new Methodology
            {
                MethodologyDescription = methodology.MethodologyDescription,
                MethodologyLink = methodology.MethodologyLink,
                MethodologyName = methodology.MethodologyName,
                SpeciesList = methodology.SpeciesList
            };
        }
    }
}