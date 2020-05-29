using SOS.Lib.Models.Processed.Observation;

namespace SOS.Process.Repositories.Destination.Interfaces
{
    /// <summary>
    ///     Repository for retrieving processd taxa.
    /// </summary>
    public interface IProcessedTaxonRepository : IProcessBaseRepository<ProcessedTaxon, int>
    {
    }
}