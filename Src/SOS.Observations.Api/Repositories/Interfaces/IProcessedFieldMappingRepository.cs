using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Observations.Api.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessedFieldMappingRepository : IBaseRepository<FieldMapping, FieldMappingFieldId>
    {
        /// <summary>
        /// Gets all field mappings.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<FieldMapping>> GetFieldMappingsAsync();
    }
}