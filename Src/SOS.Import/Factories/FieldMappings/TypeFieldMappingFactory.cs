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
                new FieldMappingValue {Id = 0, Name = "Collection"},
                new FieldMappingValue {Id = 1, Name = "Dataset"},
                new FieldMappingValue {Id = 2, Name = "Event"},
                new FieldMappingValue {Id = 3, Name = "Image"},
                new FieldMappingValue {Id = 4, Name = "InteractiveResource"},
                new FieldMappingValue {Id = 5, Name = "MovingImage"},
                new FieldMappingValue {Id = 6, Name = "PhysicalObject"},
                new FieldMappingValue {Id = 7, Name = "Service"},
                new FieldMappingValue {Id = 8, Name = "Software"},
                new FieldMappingValue {Id = 9, Name = "Sound"},
                new FieldMappingValue {Id = 10, Name = "StillImage"},
                new FieldMappingValue {Id = 11, Name = "Text"}
            };

            return fieldMappingValues;
        }
    }
}