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
    /// Class for creating Continent field mapping.
    /// </summary>
    public class ContinentFieldMappingFactory : DwcFieldMappingFactoryBase, Interfaces.IContinentFieldMappingFactory
    {
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.Continent;
        protected override bool Localized => false;
        protected override ICollection<FieldMappingValue> GetFieldMappingValues()
        {
            // Vocabulary from https://dwc.tdwg.org/terms/#dwc:continent
            var fieldMappingValues = new List<FieldMappingValue>
            {
                new FieldMappingValue {Id = 0, Name = "Africa"},
                new FieldMappingValue {Id = 1, Name = "Antarctica"},
                new FieldMappingValue {Id = 2, Name = "Asia"},
                new FieldMappingValue {Id = 3, Name = "Oceania"},
                new FieldMappingValue {Id = 4, Name = "Europe"},
                new FieldMappingValue {Id = 5, Name = "North America"},
                new FieldMappingValue {Id = 6, Name = "South America"}
            };

            return fieldMappingValues;
        }
    }
}