using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using System.Linq;

namespace SOS.Lib.Extensions
{
    public static class ObservationDatasetExtensions
    {
        public static ObservationDataset ToObservationDataset(this ObservationDatasetV2 observationDatasetV2)
        {
            return new ObservationDataset()
            {
                AccessRights = observationDatasetV2.AccessRights,
                Assigner = observationDatasetV2.Assigner,
                Creator = observationDatasetV2?.Creator?.FirstOrDefault(),
                DataStewardship = observationDatasetV2.DataStewardship,
                Description = observationDatasetV2.Description,
                EndDate = observationDatasetV2.EndDate,
                EventIds = observationDatasetV2.EventIds,
                Id = observationDatasetV2.Id,
                Identifier = observationDatasetV2.Identifier,
                Language = observationDatasetV2.Language,
                Metadatalanguage = observationDatasetV2.Metadatalanguage,
                Methodology = observationDatasetV2.Methodology,
                OwnerinstitutionCode = observationDatasetV2.OwnerinstitutionCode,
                ProjectCode = observationDatasetV2.ProjectCode,
                ProjectId = observationDatasetV2.ProjectId,
                Publisher = observationDatasetV2.Publisher,
                Purpose = observationDatasetV2.Purpose,
                Spatial = observationDatasetV2.Spatial,
                StartDate = observationDatasetV2.StartDate,
                Title = observationDatasetV2.Title
            };
        }
    }
}