using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    public class FieldMapping : IEntity<FieldMappingFieldId>
    {
        public FieldMappingFieldId Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Localized { get; set; }
        public ICollection<FieldMappingValue> Values { get; set; }
        public ICollection<ExternalSystemMapping> ExternalSystemsMapping { get; set; }

        public Dictionary<int, string> CreateValueDictionary(string cultureCode = "en-GB")
        {
            if (Localized)
            {
                return Values.ToDictionary(m => m.Id, m => m.Translations.Single(t => t.CultureCode == cultureCode).Value);
            }

            return Values.ToDictionary(m => m.Id, m => m.Value);
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}";
        }
    }
}
