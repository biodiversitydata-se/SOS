using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.FieldMappings.Interfaces;
using SOS.Import.Repositories.Destination.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    /// <summary>
    /// Class for creating Parish field mapping.
    /// </summary>
    public class ParishFieldMappingFactory : GeoRegionFieldMappingFactoryBase, IFieldMappingCreatorFactory
    {
        public ParishFieldMappingFactory(
            IAreaVerbatimRepository areaVerbatimRepository,
            ILogger<GeoRegionFieldMappingFactoryBase> logger) : base(areaVerbatimRepository, logger)
        {

        }

        public Task<FieldMapping> CreateFieldMappingAsync()
        {
            return CreateFieldMappingAsync(FieldMappingFieldId.Parish, AreaType.Parish);
        }
    }
}