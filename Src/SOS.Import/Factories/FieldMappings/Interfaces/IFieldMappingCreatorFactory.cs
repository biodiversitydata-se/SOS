using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings.Interfaces
{
    /// <summary>
    /// Interface for creating field mapping.
    /// </summary>
    public interface IFieldMappingCreatorFactory
    {
        /// <summary>
        /// Create field mapping.
        /// </summary>
        /// <returns></returns>
        Task<FieldMapping> CreateFieldMappingAsync();
    }
}