using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMapping
{
    /// <summary>
    ///     Class for creating BasisOfRecord field mapping.
    /// </summary>
    public class BasisOfRecordFieldMappingFactory : DwcFieldMappingFactoryBase
    {
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.BasisOfRecord;
        protected override bool Localized => false;

        protected override ICollection<FieldMappingValue> GetFieldMappingValues()
        {
            // Vocabulary from https://dwc.tdwg.org/terms/#dwc:basisOfRecord and GBIF website.
            var fieldMappingValues = new List<FieldMappingValue>
            {
                new FieldMappingValue {Id = 0, Value = "HumanObservation"},
                new FieldMappingValue {Id = 1, Value = "PreservedSpecimen"},
                new FieldMappingValue {Id = 2, Value = "FossilSpecimen"},
                new FieldMappingValue {Id = 3, Value = "LivingSpecimen"},
                new FieldMappingValue {Id = 4, Value = "MaterialSample"},
                new FieldMappingValue {Id = 5, Value = "Event"},
                new FieldMappingValue {Id = 6, Value = "MachineObservation"},
                new FieldMappingValue {Id = 7, Value = "Taxon"},
                new FieldMappingValue {Id = 8, Value = "Occurrence"},
                new FieldMappingValue {Id = 9, Value = "Literature"},
                new FieldMappingValue {Id = 10, Value = "Unknown"}
            };

            return fieldMappingValues;
        }
    }
}