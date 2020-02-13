using System;
using System.Collections.Generic;
using System.Text;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    public class FieldMapping : IEntity<int>
    {
        public int Id { get; set; }
        public FieldMappingFieldId FieldMappingFieldId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Localized { get; set; }
        public ICollection<FieldMappingValue> Values { get; set; }
        public ICollection<ExternalSystemMapping> ExternalSystemsMapping { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}";
        }
    }
}
