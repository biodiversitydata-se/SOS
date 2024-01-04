﻿using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace SOS.Lib.Models.Shared
{
    public class VocabularyValueInfo
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public bool Localized { get; set; }

        [BsonIgnoreIfNull]
        [JsonConverter(typeof(ExpandoObjectConverter))]
        public dynamic Extra { get; set; }

        [BsonIgnoreIfNull] public VocabularyValueInfoCategory Category { get; set; }

        /// <summary>
        ///     Translations.
        /// </summary>
        /// <remarks>
        ///     Translations exists if the <see cref="Localized" /> property is set to true.
        /// </remarks>
        [BsonIgnoreIfNull]
        public ICollection<VocabularyValueTranslation> Translations { get; set; }

        /// <summary>
        /// True if the value doesn't exist in Artportalen; otherwise false.
        /// </summary>
        [BsonIgnore]
        [JsonIgnore]
        public bool IsCustomValue { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Description)}: {Description}";
        }
    }
}