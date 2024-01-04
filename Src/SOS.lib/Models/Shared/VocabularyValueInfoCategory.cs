using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace SOS.Lib.Models.Shared
{
    public class VocabularyValueInfoCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Localized { get; set; }

        /// <summary>
        ///     Translations.
        /// </summary>
        /// <remarks>
        ///     Translations exists if the <see cref="Localized" /> property is set to true.
        /// </remarks>
        [BsonIgnoreIfNull]
        public ICollection<VocabularyValueTranslation> Translations { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Description)}: {Description}";
        }
    }
}