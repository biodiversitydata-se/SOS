using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    /// <summary>
    /// Class for creating Occurrence Status field mapping.
    /// </summary>
    public class OccurrenceStatusFieldMappingFactory : DwcFieldMappingFactoryBase
    {
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.OccurrenceStatus;
        protected override bool Localized => false;
        protected override ICollection<FieldMappingValue> GetFieldMappingValues()
        {
            var fieldMappingValues = new List<FieldMappingValue>
            {
                new FieldMappingValue {Id = 0, Value = "present"},
                new FieldMappingValue {Id = 1, Value = "absent"}
            };
             
            return fieldMappingValues;
        }
    }
}