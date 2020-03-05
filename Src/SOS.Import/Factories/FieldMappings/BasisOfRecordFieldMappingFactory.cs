using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    /// <summary>
    /// Class for creating BasisOfRecord field mapping.
    /// </summary>
    public class BasisOfRecordFieldMappingFactory : DwcFieldMappingFactoryBase
    {
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.BasisOfRecord;
        protected override bool Localized => false;
        protected override ICollection<FieldMappingValue> GetFieldMappingValues()
        {
            // Vocabulary from https://dwc.tdwg.org/terms/#dwc:basisOfRecord
            var fieldMappingValues = new List<FieldMappingValue>
            {
                new FieldMappingValue {Id = 0, Name = "HumanObservation"},
                new FieldMappingValue {Id = 1, Name = "PreservedSpecimen"},
                new FieldMappingValue {Id = 2, Name = "FossilSpecimen"},
                new FieldMappingValue {Id = 3, Name = "LivingSpecimen"},
                new FieldMappingValue {Id = 4, Name = "MaterialSample"},
                new FieldMappingValue {Id = 5, Name = "Event"},
                new FieldMappingValue {Id = 6, Name = "MachineObservation"},
                new FieldMappingValue {Id = 7, Name = "Taxon"},
                new FieldMappingValue {Id = 8, Name = "Occurrence"},
                new FieldMappingValue {Id = 9, Name = "Literature"}
            };

            return fieldMappingValues;
        }
    }
}