using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Repositories.Resource;

/// <summary>
///     Repository for retrieving processed taxa.
/// </summary>
public class ArtportalenTaxonRepository : RepositoryBase<Taxon, int>, IArtportalenTaxonRepository
{
    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="logger"></param>
    public ArtportalenTaxonRepository(
        IProcessClient client,
        ILogger<ArtportalenTaxonRepository> logger)
        : base(client, "ArtportalenTaxon", logger)
    {
    }

    /// <summary>
    ///     Get chunk of taxa
    /// </summary>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    public async Task<IEnumerable<IBasicTaxon>> GetBasicTaxonChunkAsync(int skip, int take)
    {
        var res = await MongoCollection
            .Find(x => true)
            .Project(m => new BasicTaxon
            {
                Id = m.Id,
                SecondaryParentDyntaxaTaxonIds = m.SecondaryParentDyntaxaTaxonIds,
                ScientificName = m.ScientificName,
                ScientificNameAuthorship = m.ScientificNameAuthorship,
                VernacularName = m.VernacularName,
                Attributes = m.Attributes
            })
            .Skip(skip)
            .Limit(take)
            .ToListAsync();

        return res;
    }

    /// <summary>
    ///     Get chunk of taxa
    /// </summary>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Taxon>> GetChunkAsync(int skip, int take)
    {
        var res = await MongoCollection
            .Find(x => true)
            .Skip(skip)
            .Limit(take)
            .ToListAsync();

        return res;
    }
}
