using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.FieldMapping.Interfaces;
using SOS.Import.Repositories.Destination.Area.Interfaces;
using SOS.Lib.Enums;

namespace SOS.Import.Factories.FieldMapping
{
    /// <summary>
    ///     Class for creating Parish field mapping.
    /// </summary>
    public class ParishFieldMappingFactory : GeoRegionFieldMappingFactoryBase, IFieldMappingCreatorFactory
    {
        public ParishFieldMappingFactory(
            IAreaProcessedRepository areaProcessedRepository,
            ILogger<ParishFieldMappingFactory> logger) : base(areaProcessedRepository, logger)
        {
        }

        public Task<Lib.Models.Shared.FieldMapping> CreateFieldMappingAsync()
        {
            return CreateFieldMappingAsync(FieldMappingFieldId.Parish, AreaType.Parish);
        }
    }
}