using SOS.Harvest.Entities.Skogsskada;

namespace SOS.Harvest.Repositories.Source.Skogsskada.Interfaces;

/// <summary>
///     Skogsskada repository interface
/// </summary>
public interface ISkogsskadaRepository
{
    /// <summary>
    /// Get a chunk of observations from the sosview view, ordered by occurrenceID.
    /// </summary>
    /// <param name="afterOccurrenceId">
    /// Only return rows with occurrenceID greater than this value. Pass null to start from the beginning.
    /// </param>
    /// <param name="batchSize">Maximum number of rows to return.</param>
    /// <returns></returns>
    Task<IEnumerable<SkogsskadaEntity>> GetChunkAsync(string? afterOccurrenceId, int batchSize);
}