using System;
using System.Collections.Generic;
using System.Text;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    public class FieldMapping : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<IFieldMappingValue> Values { get; set; }
        public ICollection<DataProviderTypeFieldMapping> DataProviderTypeFieldMappings { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}";
        }
    }
}
