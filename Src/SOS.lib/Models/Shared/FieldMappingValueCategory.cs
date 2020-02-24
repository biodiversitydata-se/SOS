using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    public class FieldMappingValueCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Localized { get; set; }

        /// <summary>
        /// Translations.
        /// </summary>
        /// <remarks>
        /// Translations exists if the <see cref="Localized"/> property is set to true.
        /// </remarks>
        [BsonIgnoreIfNull]
        public ICollection<FieldMappingTranslation> Translations { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Description)}: {Description}";
        }
    }
}