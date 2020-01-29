using System.Collections.Generic;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    public class FieldMappingWithCategoryValue : IFieldMappingValue
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public ICollection<FieldMappingTranslation> Translations { get; set; }
        public FieldMappingValue Category { get; set; }
    }
}