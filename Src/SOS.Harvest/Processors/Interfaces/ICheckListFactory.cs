using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Checklist;

namespace SOS.Harvest.Processors.Interfaces
{
    /// <summary>
    /// Interface for checklist factory
    /// </summary>
    public interface IChecklistFactory<TEntity> where TEntity : IEntity<int>
    {
        /// <summary>
        /// Cast verbatim to Checklist
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        Checklist CreateProcessedChecklist(TEntity verbatim);
    }
}