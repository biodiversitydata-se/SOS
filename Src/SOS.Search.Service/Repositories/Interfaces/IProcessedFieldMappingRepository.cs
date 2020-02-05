using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Shared;

namespace SOS.Search.Service.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessedFieldMappingRepository : IBaseRepository<FieldMapping, int>
    {
        /// <summary>
        /// Gets all field mappings.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<FieldMapping>> GetFieldMappingsAsync();
    }
}