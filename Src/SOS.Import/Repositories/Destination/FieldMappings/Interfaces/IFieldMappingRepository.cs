using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Repositories.Destination.FieldMappings.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IFieldMappingRepository : IVerbatimRepository<FieldMapping, FieldMappingFieldId>
    {
    }
}