using SOS.Harvest.Entities.Artportalen;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Harvest.Extensions;

public static class MetadataEntityExtensions
{
    extension(IEnumerable<MetadataEntity<int>> metadataEntities)
    {
        public void TrimValues()
        {
            foreach (var metadataEntity in metadataEntities)
            {
                metadataEntity.TrimValue();
            }
        }
    }

    extension(MetadataEntity<int> metadataEntity)
    {
        public void TrimValue()
        {
            metadataEntity.Translation = metadataEntity.Translation?.Trim();
        }
    }

    extension(ProjectEntity entity)
    {
        public Project ToVerbatim()
        {
            return new Project
            {
                Category = entity.Category,
                CategorySwedish = entity.CategorySwedish,
                Description = entity.Description?.Clean(),
                EndDate = entity.EndDate,
                Id = entity.Id,
                IsPublic = entity.IsPublic,
                Name = entity.Name,
                Owner = entity.Owner,
                StartDate = entity.StartDate,
                ProjectURL = entity.ProjectURL,
                SurveyMethod = entity.SurveyMethod,
                SurveyMethodUrl = entity.SurveyMethodUrl
            };
        }
    }
}
