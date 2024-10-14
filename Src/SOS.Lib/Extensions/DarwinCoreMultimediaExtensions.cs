using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Lib.Extensions
{
    /// <summary>
    ///     Extensions for Darwin Core
    /// </summary>
    public static class DarwinCoreMultimediaExtensions
    {
        public static Multimedia ToProcessedMultimedia(this DwcMultimedia dwcMultimedia)
        {
            return new Multimedia
            {
                Type = dwcMultimedia.Type,
                Format = dwcMultimedia.Format,
                Identifier = dwcMultimedia.Identifier,
                References = dwcMultimedia.References,
                Title = dwcMultimedia.Title,
                Description = dwcMultimedia.Description,
                Created = dwcMultimedia.Created,
                Creator = dwcMultimedia.Creator,
                Contributor = dwcMultimedia.Contributor,
                Publisher = dwcMultimedia.Publisher,
                Audience = dwcMultimedia.Audience,
                Source = dwcMultimedia.Source,
                License = dwcMultimedia.License,
                RightsHolder = dwcMultimedia.RightsHolder,
                DatasetID = dwcMultimedia.DatasetID
            };
        }

        public static Multimedia ToProcessedMultimedia(this DwcAudubonMedia dwcAudubonMedia)
        {
            return new Multimedia
            {
                Type = dwcAudubonMedia.Type,
                Format = dwcAudubonMedia.Format,
                Identifier = !string.IsNullOrEmpty(dwcAudubonMedia.AccessURI) ? dwcAudubonMedia.AccessURI
                    : !string.IsNullOrEmpty(dwcAudubonMedia.Identifier) ? dwcAudubonMedia.Identifier : null,
                References = dwcAudubonMedia.AccessURI, // An html webpage that shows the image or its metadata. 
                Title = dwcAudubonMedia.Title,
                Description = dwcAudubonMedia.Description,
                Created = !string.IsNullOrEmpty(dwcAudubonMedia.CreateDate) ? dwcAudubonMedia.CreateDate
                    : !string.IsNullOrEmpty(dwcAudubonMedia.Modified) ? dwcAudubonMedia.Modified : null,
                Creator = dwcAudubonMedia.Creator,
                Contributor = null, // no mapping?
                Publisher = null, // no mapping?
                Audience = null, // no mapping?
                Source = dwcAudubonMedia.Source,
                License = dwcAudubonMedia.Rights,
                RightsHolder = dwcAudubonMedia.Rights,
                DatasetID = null // no mapping?
            };
        }
    }
}