using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    /// <summary>
    /// Class for creating EstablishmentMeans field mapping.
    /// </summary>
    public class EstablishmentMeansFieldMappingFactory : DwcFieldMappingFactoryBase
    {
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.EstablishmentMeans;
        protected override bool Localized => false;
        protected override ICollection<FieldMappingValue> GetFieldMappingValues()
        {
            // Vocabulary from http://rs.gbif.org/vocabulary/gbif/establishment_means.xml.
            var fieldMappingValues = new List<FieldMappingValue>
            {
                new FieldMappingValue {Id = 0, Value = "native"},
                new FieldMappingValue {Id = 1, Value = "introduced"},
                new FieldMappingValue {Id = 2, Value = "naturalised"},
                new FieldMappingValue {Id = 3, Value = "invasive"},
                new FieldMappingValue {Id = 4, Value = "managed"},
                new FieldMappingValue {Id = 5, Value = "uncertain"}
            };

            return fieldMappingValues;
        }
    }
}