using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SOS.Batch.Import.AP.Entities;
using SOS.Batch.Import.AP.Services.Interfaces;

namespace SOS.Batch.Import.AP.Repositories.Source {
    /// <summary>
    /// Taxon repository
    /// </summary>
    public class TaxonRepository : BaseRepository<TaxonRepository>, Interfaces.ITaxonRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="speciesPortalDataService"></param>
        /// <param name="logger"></param>
        public TaxonRepository(ISpeciesPortalDataService speciesPortalDataService, ILogger<TaxonRepository> logger) : base(speciesPortalDataService, logger)
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
	                INNER JOIN Translation trc ON rc.Id = trc.ResourceId AND trc.GlobalizationCultureId = 175
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
