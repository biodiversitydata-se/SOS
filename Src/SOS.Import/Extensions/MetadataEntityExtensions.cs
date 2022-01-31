using System.Collections.Generic;
using SOS.Import.Entities.Artportalen;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Import.Extensions
{
    public static class MetadataEntityExtensions
    {
        public static void TrimValues(this IEnumerable<MetadataEntity> metadataEntities)
        {
            foreach (var metadataEntity in metadataEntities)
            {
                metadataEntity.TrimValue();
            }
        }

        public static void TrimValue(this MetadataEntity metadataEntity)
        {
            metadataEntity.Translation = metadataEntity.Translation?.Trim();
        }

        public static Project ToVerbatim(this ProjectEntity entity)
        {
            return new Project
            {
                Category = entity.Category,
                CategorySwedish = entity.CategorySwedish,
                Description = entity.Description,
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
