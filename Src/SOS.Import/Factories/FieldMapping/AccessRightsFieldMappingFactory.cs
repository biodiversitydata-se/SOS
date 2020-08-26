using System.Collections.Generic;
using System.Linq;
using SOS.Import.Extensions;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMapping
{
    /// <summary>
    ///     Class for creating AccessRights field mapping.
    /// </summary>
    public class AccessRightsFieldMappingFactory : DwcFieldMappingFactoryBase
    {
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.AccessRights;
        protected override bool Localized => false;

        protected override ICollection<FieldMappingValue> GetFieldMappingValues()
        {
            var fieldMappingValues = new List<FieldMappingValue>
            {
                new FieldMappingValue {Id = 0, Value = "Free usage"},
                new FieldMappingValue {Id = 1, Value = "Not for public usage"},
                new FieldMappingValue {Id = 2, Value = "CC0"},
                new FieldMappingValue {Id = 3, Value = "CC BY"},
                new FieldMappingValue {Id = 4, Value = "CC BY-NC"}
            };

            return fieldMappingValues;
        }

        protected override Dictionary<string, int> GetMappingSynonyms()
        {
            return new Dictionary<string, int>
            {
                {"Free to use", 0}
            };
        }
    }
}