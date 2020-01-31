using System.Collections.Generic;

namespace SOS.Lib.Models.Shared
{
    public class DataProviderTypeFieldMapping
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<DataProviderTypeFieldValueMapping> Mappings { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}";
        }
    }
}