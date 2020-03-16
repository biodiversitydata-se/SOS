using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    /// <summary>
    /// Class for creating Type field mapping.
    /// </summary>
    public class TypeFieldMappingFactory : DwcFieldMappingFactoryBase
    {
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.Type;
        protected override bool Localized => false;
        protected override ICollection<FieldMappingValue> GetFieldMappingValues()
        {
            var fieldMappingValues = new List<FieldMappingValue>
            {
                new FieldMappingValue {Id = 0, Value = "Collection"},
                new FieldMappingValue {Id = 1, Value = "Dataset"},
                new FieldMappingValue {Id = 2, Value = "Event"},
                new FieldMappingValue {Id = 3, Value = "Image"},
                new FieldMappingValue {Id = 4, Value = "InteractiveResource"},
                new FieldMappingValue {Id = 5, Value = "MovingImage"},
                new FieldMappingValue {Id = 6, Value = "PhysicalObject"},
                new FieldMappingValue {Id = 7, Value = "Service"},
                new FieldMappingValue {Id = 8, Value = "Software"},
                new FieldMappingValue {Id = 9, Value = "Sound"},
                new FieldMappingValue {Id = 10, Value = "StillImage"},
                new FieldMappingValue {Id = 11, Value = "Text"}
            };

            return fieldMappingValues;
        }
    }
}