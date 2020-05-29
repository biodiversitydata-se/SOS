using System.Threading.Tasks;

namespace SOS.Import.Factories.FieldMapping.Interfaces
{
    /// <summary>
    ///     Interface for creating field mapping.
    /// </summary>
    public interface IFieldMappingCreatorFactory
    {
        /// <summary>
        ///     Create field mapping.
        /// </summary>
        /// <returns></returns>
        Task<Lib.Models.Shared.FieldMapping> CreateFieldMappingAsync();
    }
}