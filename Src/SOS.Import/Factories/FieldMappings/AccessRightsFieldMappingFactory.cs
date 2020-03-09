using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    /// <summary>
    /// Class for creating AccessRights field mapping.
    /// </summary>
    public class AccessRightsFieldMappingFactory : DwcFieldMappingFactoryBase
    {
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.AccessRights;
        protected override bool Localized => false;
        protected override ICollection<FieldMappingValue> GetFieldMappingValues()
        {
            var fieldMappingValues = new List<FieldMappingValue>
            {
                new FieldMappingValue {Id = 0, Name = "Free usage"},
                new FieldMappingValue {Id = 1, Name = "Not for public usage"}
            };

            return fieldMappingValues;
        }
    }
}