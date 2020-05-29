using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Observations.Api.Repositories.Interfaces
{
    /// <summary>
    ///     Field mapping repository interface
    /// </summary>
    public interface IProcessedFieldMappingRepository : IBaseRepository<FieldMapping, FieldMappingFieldId>
    {
    }
}