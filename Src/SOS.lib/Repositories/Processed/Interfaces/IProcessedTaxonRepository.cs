using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Repository for retrieving processd taxa.
    /// </summary>
    public interface IProcessedTaxonRepository : IMongoDbProcessedRepositoryBase<Taxon, int>
    {
    }
}