using SOS.Harvest.Entities.Aqua;

namespace SOS.Harvest.Repositories.Source.Aqua.Interfaces;

/// <summary>
///     Sighting repository interface
/// </summary>
public interface INorsRepository 
{
    /// <summary>
    /// Asynchronously retrieves a chunk of EMOF records starting at the specified offset.
    /// </summary>
    /// <param name="offset">The zero-based index of the first record to retrieve. Must be greater than or equal to 0.</param>
    /// <param name="take">The maximum number of records to retrieve. Must be greater than 0.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of EMOF
    /// records, or null if no records are available.</returns>
    Task<IEnumerable<SbdiEmof>?> GetEmofsChunkAsync(int offset, int take);

    /// <summary>
    /// Asynchronously retrieves a subset of SBDI events starting at the specified offset.
    /// </summary>
    /// <param name="offset">The zero-based index of the first event to retrieve. Must be greater than or equal to 0.</param>
    /// <param name="take">The maximum number of events to retrieve. Must be greater than 0.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of SBDI
    /// events, or null if no events are available.</returns>
    Task<IEnumerable<SbdiEvent>?> GetEventsChunkAsync(int offset, int take);

    /// <summary>
    /// Asynchronously retrieves a subset of SBDI occurrences starting at the specified offset.
    /// </summary>
    /// <param name="offset">The zero-based index of the first occurrence to retrieve. Must be greater than or equal to 0.</param>
    /// <param name="take">The maximum number of occurrences to retrieve. Must be greater than 0.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of SBDI
    /// occurrences, or null if no occurrences are available.</returns>
    Task<IEnumerable<SbdiOccurrence>?> GetOccurrencesChunkAsync(int offset, int take);
}