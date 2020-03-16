using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Factories.FieldMappings.Interfaces;
using SOS.Import.Repositories.Destination.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    /// <summary>
    /// Class for creating County field mapping.
    /// </summary>
    public class CountyFieldMappingFactory : GeoRegionFieldMappingFactoryBase, IFieldMappingCreatorFactory
    {
        public CountyFieldMappingFactory(
            IAreaVerbatimRepository areaVerbatimRepository, 
            ILogger<GeoRegionFieldMappingFactoryBase> logger) : base(areaVerbatimRepository, logger)
        {

        }

        public Task<FieldMapping> CreateFieldMappingAsync()
        {
            return CreateFieldMappingAsync(FieldMappingFieldId.County, AreaType.County);
        }
    }
}