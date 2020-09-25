using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Repository for retrieving field mapping.
    /// </summary>
    public interface IProcessedFieldMappingRepository : IMongoDbProcessedRepositoryBase<FieldMapping, FieldMappingFieldId>
    {
    }
}