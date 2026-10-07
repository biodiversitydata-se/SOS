using SOS.Lib.Models.Processed.Observation;
using SOS.Api.Contracts.DataStewardship.Responses;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Lib.Extensions.Dto.DataStewardship;

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

    extension(IEnumerable<Multimedia> multimedias)
    {
        public List<DsAssociatedMediaDto> ToDtos()
        {
            if (multimedias == null || !multimedias.Any()) return null;
            return multimedias.Select(m => m.ToDto()).ToList();
        }
    }

    extension(Multimedia multimedia)
    {
        public DsAssociatedMediaDto ToDto()
        {
            if (multimedia == null) return null;
            return new DsAssociatedMediaDto
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
