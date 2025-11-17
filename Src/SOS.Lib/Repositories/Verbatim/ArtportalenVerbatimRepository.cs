using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Verbatim;

/// <summary>
///     Species data service
/// </summary>
public class ArtportalenVerbatimRepository : VerbatimRepositoryBase<ArtportalenObservationVerbatim, int>,
    IArtportalenVerbatimRepository
{
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="importClient"></param>
    /// <param name="logger"></param>
    public ArtportalenVerbatimRepository(
        IVerbatimClient importClient,
        ILogger<ArtportalenVerbatimRepository> logger) : base(importClient, logger)
    {
    }

    public override async Task<bool> AddCollectionAsync()
    {
        var added = await base.AddCollectionAsync();
        if (!added) return false;

        var indexModels = new[]
        {
            // Add index to prevent duplicate entries
            new CreateIndexModel<ArtportalenObservationVerbatim>(
                Builders<ArtportalenObservationVerbatim>.IndexKeys.Ascending(io => io.SightingId),
                new CreateIndexOptions { Unique = true })
        };

        await AddIndexes(indexModels);
        return true;
    }
}