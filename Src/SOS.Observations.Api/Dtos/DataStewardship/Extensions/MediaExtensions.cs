using SOS.Lib.Models.Processed.DataStewardship.Enums;
using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Observations.Api.Dtos.DataStewardship.Extensions
{
    public static class MediaExtensions
    {
        private static AssociatedMediaType GetAssociatedMediaTypeEnum(string format)
        {
            if (string.IsNullOrEmpty(format)) return AssociatedMediaType.Bild; // default
            string formatLower = format.ToLower();
            if (formatLower.StartsWith("image"))
                return AssociatedMediaType.Bild;
            if (formatLower.StartsWith("pdf"))
                return AssociatedMediaType.Pdf;
            if (formatLower.StartsWith("audio"))
                return AssociatedMediaType.Ljud;
            if (formatLower.StartsWith("video"))
                return AssociatedMediaType.Film;

            return AssociatedMediaType.Bild; // default
        }

        public static List<AssociatedMediaDto> ToDtos(this IEnumerable<Multimedia> multimedias)
        {
            if (multimedias == null || !multimedias.Any()) return null;
            return multimedias.Select(m => m.ToDto()).ToList();
        }

        public static AssociatedMediaDto ToDto(this Multimedia multimedia)
        {
            if (multimedia == null) return null;
            return new AssociatedMediaDto
            {
                AssociatedMediaName = multimedia.Title,
                AssociatedMediaType = GetAssociatedMediaTypeEnum(multimedia.Format),
                AssociatedMediaLink = multimedia.Identifier,
                License = multimedia.License,
                RightsHolder = multimedia.RightsHolder
            };
        }
    }
}
