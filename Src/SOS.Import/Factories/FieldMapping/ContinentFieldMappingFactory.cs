using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMapping
{
    /// <summary>
    /// Class for creating Continent field mapping.
    /// </summary>
    public class ContinentFieldMappingFactory : DwcFieldMappingFactoryBase
    {
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.Continent;
        protected override bool Localized => false;
        protected override ICollection<FieldMappingValue> GetFieldMappingValues()
        {
            // Vocabulary from https://dwc.tdwg.org/terms/#dwc:continent
            var fieldMappingValues = new List<FieldMappingValue>
            {
                new FieldMappingValue {Id = 0, Value = "Africa"},
                new FieldMappingValue {Id = 1, Value = "Antarctica"},
                new FieldMappingValue {Id = 2, Value = "Asia"},
                new FieldMappingValue {Id = 3, Value = "Oceania"},
                new FieldMappingValue {Id = 4, Value = "Europe"},
                new FieldMappingValue {Id = 5, Value = "North America"},
                new FieldMappingValue {Id = 6, Value = "South America"}
            };

            return fieldMappingValues;
        }
    }
}