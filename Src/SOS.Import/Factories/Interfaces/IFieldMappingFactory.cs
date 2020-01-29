using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Factories.Interfaces
{
    /// <summary>
    /// Interface for importing field mappings into MongoDb.
    /// </summary>
    public interface IFieldMappingFactory
    {
        /// <summary>
        /// Import field mappings.
        /// </summary>
        /// <returns></returns>
        Task<bool> ImportAsync();
    }
}
