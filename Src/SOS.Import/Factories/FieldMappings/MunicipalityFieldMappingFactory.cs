using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.FieldMappings.Interfaces;
using SOS.Import.Repositories.Destination.SpeciesPortal.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    /// <summary>
    /// Class for creating Municipality field mapping.
    /// </summary>
    public class MunicipalityFieldMappingFactory : GeoRegionFieldMappingFactoryBase, IFieldMappingCreatorFactory
    {
        public MunicipalityFieldMappingFactory(
            IAreaVerbatimRepository areaVerbatimRepository,
            ILogger<GeoRegionFieldMappingFactoryBase> logger) : base(areaVerbatimRepository, logger)
        {

        }

        public Task<FieldMapping> CreateFieldMappingAsync()
        {
            return CreateFieldMappingAsync(FieldMappingFieldId.Municipality, AreaType.Municipality);
        }
    }
}