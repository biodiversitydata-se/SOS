using SOS.Harvest.Entities.Artportalen;

namespace SOS.Harvest.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    ///     Dataset repository interface
    /// </summary>
    public interface IDatasetRepository : IBaseRepository<IDatasetRepository>
    {
        Task<DatasetEntities> GetDatasetEntitiesAsync();
    }
}