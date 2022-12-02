using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Checklist;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;

namespace SOS.Harvest.Processors.Interfaces
{
    /// <summary>
    /// Interface for dataset factory
    /// </summary>
    public interface IDatasetFactory<TEntity> where TEntity : IEntity<int>
    {
        /// <summary>
        /// Create processed dataset.
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        ObservationDataset CreateProcessedDataset(TEntity verbatim);
    }
}