using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Asn1.Cms;
using SOS.Import.Entities.Artportalen;

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
    }
}
