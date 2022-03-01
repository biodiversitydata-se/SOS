using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.CheckList;

namespace SOS.Harvest.Processors.Interfaces
{
    /// <summary>
    /// Interface for check list factory
    /// </summary>
    public interface ICheckListFactory<TEntity> where TEntity : IEntity<int>
    {
        /// <summary>
        /// Cast verbatim to CheckList
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        CheckList CreateProcessedCheckList(TEntity verbatim);
    }
}