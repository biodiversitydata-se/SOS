using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.FieldMapping.Interfaces;
using SOS.Import.Repositories.Destination.Artportalen.Interfaces;
using SOS.Lib.Enums;

namespace SOS.Import.Factories.FieldMapping
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

        public Task<Lib.Models.Shared.FieldMapping> CreateFieldMappingAsync()
        {
            return CreateFieldMappingAsync(FieldMappingFieldId.County, AreaType.County);
        }
    }
}