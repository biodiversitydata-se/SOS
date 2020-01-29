using System.Collections.Generic;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    public class FieldMappingValue : IFieldMappingValue
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public ICollection<FieldMappingTranslation> Translations { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Description)}: {Description}";
        }
    }
}