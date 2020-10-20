using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.FieldMapping.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Import.Factories.FieldMapping
{
    /// <summary>
    ///     Class for creating Province field mapping.
    /// </summary>
    public class ProvinceFieldMappingFactory : GeoRegionFieldMappingFactoryBase, IFieldMappingCreatorFactory
    {
        public ProvinceFieldMappingFactory(
            IAreaRepository areaProcessedRepository,
            ILogger<ProvinceFieldMappingFactory> logger) : base(areaProcessedRepository, logger)
        {
        }

        public Task<Lib.Models.Shared.FieldMapping> CreateFieldMappingAsync()
        {
            return CreateFieldMappingAsync(FieldMappingFieldId.Province, AreaType.Province);
        }
    }
}