using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.FieldMapping.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Import.Factories.FieldMapping
{
    /// <summary>
    ///     Class for creating Municipality field mapping.
    /// </summary>
    public class MunicipalityFieldMappingFactory : GeoRegionFieldMappingFactoryBase, IFieldMappingCreatorFactory
    {
        public MunicipalityFieldMappingFactory(
            IAreaRepository areaProcessedRepository,
            ILogger<MunicipalityFieldMappingFactory> logger) : base(areaProcessedRepository, logger)
        {
        }

        public Task<Lib.Models.Shared.FieldMapping> CreateFieldMappingAsync()
        {
            return CreateFieldMappingAsync(FieldMappingFieldId.Municipality, AreaType.Municipality);
        }
    }
}