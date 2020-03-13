using System;
using System.Collections.Generic;
using System.Dynamic;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace SOS.Lib.Models.Shared
{
    public class FieldMappingValue
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public bool Localized { get; set; }

        [BsonIgnoreIfNull]
        [JsonConverter(typeof(ExpandoObjectConverter))]
        public dynamic Extra { get; set; }

        [BsonIgnoreIfNull]
        public FieldMappingValueCategory Category { get; set; }

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