using MongoDB.Bson;
using SOS.Lib.Models.Processed.Validation;

namespace SOS.Process.Repositories.Destination.Interfaces
{
    /// <summary>
    /// Processed data class
    /// </summary>
    public interface IInadequateItemRepository : IProcessBaseRepository<InadequateItem, ObjectId>
    {
    }
}
