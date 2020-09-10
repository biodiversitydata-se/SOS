using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.FieldMapping.Interfaces;
using SOS.Import.Repositories.Destination.Area.Interfaces;
using SOS.Lib.Enums;

namespace SOS.Import.Factories.FieldMapping
{
    /// <summary>
    ///     Class for creating Municipality field mapping.
    /// </summary>
    public class MunicipalityFieldMappingFactory : GeoRegionFieldMappingFactoryBase, IFieldMappingCreatorFactory
    {
        public MunicipalityFieldMappingFactory(
            IAreaProcessedRepository areaProcessedRepository,
            ILogger<MunicipalityFieldMappingFactory> logger) : base(areaProcessedRepository, logger)
        {
        }

        public Task<Lib.Models.Shared.FieldMapping> CreateFieldMappingAsync()
        {
            return CreateFieldMappingAsync(FieldMappingFieldId.Municipality, AreaType.Municipality);
        }
    }
}