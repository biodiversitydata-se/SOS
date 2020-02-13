using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings.Interfaces
{
    /// <summary>
    /// Interface for creating field mappings for geographical regions.
    /// </summary>
    public interface IGeoRegionFieldMappingFactory
    {
        /// <summary>
        /// Create field mappings for geographical regions.
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<FieldMappingFieldId, FieldMapping>> CreateFieldMappingsAsync();
    }
}