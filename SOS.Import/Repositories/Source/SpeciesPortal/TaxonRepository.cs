using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.Repositories.Source.SpeciesPortal
{
    /// <summary>
    /// Taxon repository
    /// </summary>
    public class TaxonRepository : BaseRepository<TaxonRepository>, Interfaces.ITaxonRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="SpeciesPortalDataService"></param>
        /// <param name="logger"></param>
        public TaxonRepository(ISpeciesPortalDataService SpeciesPortalDataService, ILogger<TaxonRepository> logger) : base(SpeciesPortalDataService, logger)
        {
           
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TaxonEntity>> GetAsync()
        {
            try
            {
                const string query = @"
                SELECT
	                t.Id, 
	                trc.Value AS Category,
	                tn.Name AS ScientificName,
	                tns.Name AS SwedishName
                FROM 
	                Taxon t
	                INNER JOIN TaxonCategory tc ON t.TaxonCategoryId = tc.Id
	                INNER JOIN [Resource] rc ON tc.ResourceLabel = rc.Label
	                INNER JOIN Translation trc ON rc.Id = trc.ResourceId AND trc.GlobalizationCultureId = 49
	                LEFT JOIN TaxonName tn ON t.Id = tn.TaxonId AND tn.SpeciesNamesLanguageId = 1 AND tn.ValidForTaxonomy = 1
	                LEFT JOIN TaxonName tns ON t.Id = tns.TaxonId AND tns.SpeciesNamesLanguageId = 2 AND tns.ValidForTaxonomy = 1";

                    return await QueryAsync<TaxonEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting taxa");
                return null;
            }
            
        }
    }
}
