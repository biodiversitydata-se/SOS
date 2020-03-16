using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.FieldMappings.Interfaces;
using SOS.Import.Repositories.Destination.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    /// <summary>
    /// Class for creating Province field mapping.
    /// </summary>
    public class ProvinceFieldMappingFactory : GeoRegionFieldMappingFactoryBase, IFieldMappingCreatorFactory
    {
        public ProvinceFieldMappingFactory(
            IAreaVerbatimRepository areaVerbatimRepository,
            ILogger<GeoRegionFieldMappingFactoryBase> logger) : base(areaVerbatimRepository, logger)
        {

        }

        public Task<FieldMapping> CreateFieldMappingAsync()
        {
            return CreateFieldMappingAsync(FieldMappingFieldId.Province, AreaType.Province);
        }
    }
}