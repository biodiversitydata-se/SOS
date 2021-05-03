using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities.ObservationsDatabase;
using SOS.Import.Repositories.Source.ObservationsDatabase.Interfaces;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.Repositories.Source.ObservationsDatabase
{
    public class ObservationDatabaseRepository : IObservationDatabaseRepository
    {
        /// <summary>
        ///     Logger
        /// </summary>
        private readonly ILogger<ObservationDatabaseRepository> _logger;

        /// <summary>
        /// Date service
        /// </summary>
        private readonly IObservationDatabaseDataService _observationDatabaseDataService;

        
        private async Task<IEnumerable<E>> QueryAsync<E>(string query, dynamic parameters = null)
        {
            return await _observationDatabaseDataService.QueryAsync<E>(query, parameters);
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="observationDatabaseDataService"></param>
        /// <param name="logger"></param>
        public ObservationDatabaseRepository(IObservationDatabaseDataService observationDatabaseDataService, ILogger<ObservationDatabaseRepository> logger)
        {
            _observationDatabaseDataService = observationDatabaseDataService ??
                                              throw new ArgumentNullException(nameof(observationDatabaseDataService));

            _logger = logger ??
                      throw new ArgumentNullException(nameof(logger));
        }


        /// <inheritdoc />
        public async Task<IEnumerable<ObservationEntity>> GetChunkAsync(int startId, int maxRows)
        {
            try
            {
                var query = $@"SELECT 
	                h.ArtDataIDNR AS Id,
	                h.artnr	AS TaxonId,
	                h.datum1 AS StartDate, 
	                h.datum2 AS EndDate, 
	                h.lokal	AS Locality,
	                h.rn2 AS CoordinateY, 
	                h.rn1 AS CoordinateX,
	                h.noggrannhet_tal AS CoordinateUncertaintyInMeters,
	                COALESCE(h.leg, '')	AS Observers, 
	                COALESCE(h.[källaklartext], '')	AS Origin,
	                COALESCE(h.[Antal/Yta], '')	AS IndividualCount,	
	                COALESCE(h.Stadium, '')	AS Stadium, 
	                COALESCE(h.biotop, '')	AS Habitat,
	                COALESCE(h.Substrat, '') AS Substrate, 
	                SUBSTRING(h.[TEXT], 1, 1024) AS OccurrenceRemarks,
	                COALESCE(h.Det, '')	AS VerifiedBy, 
	                COALESCE(h.samling, '')	AS CollectionCode, 
	                COALESCE(h.nummer, '') AS CollectionId, 
	                h.[Eftersökt_men_ej_funnen]	AS IsNotRediscoveredObservation, 
	                CASE 
		                WHEN (h.Forekomststatus != 'A') THEN 0
		                WHEN (h.Forekomststatus = 'A') THEN 1 
	                END AS IsNeverFoundObservation,
	                COALESCE(SUBSTRING(h.pSCI_SITE_CODE, 1, 10), '') AS SCI_code,
	                COALESCE(h.pSCI_SITE_NAME, '')  AS SCI_name,
	                h.CreatedDate AS RegisterDate,
	                h.UpdatedDate AS EditDate, 
	                CASE 
		                WHEN (t.ProtectionLevel < 4) THEN 3 
		                ELSE t.ProtectionLevel
	                END AS [ProtectionLevel],
	                COALESCE(s.[LänsNamn], '')	AS County,
	                COALESCE(s.Landskapsnamn, '') AS Province,
	                COALESCE(s.[KommunNamn], '') AS Municipality,
	                COALESCE(s.[Församlingunik], '') AS Parish
                FROM	
	                huvudtabell h
	                INNER JOIN Taxon AS t ON h.artnr = t.TaxonId
                    LEFT JOIN Socknar s ON h.fgnr = s.fgnr
                WHERE
	                h.ArtDataIDNR BETWEEN @StartId AND @EndId AND t.ProtectionLevel < 2 
	                AND (
		                h.Forekomststatus = 'A' OR h.Forekomststatus = 'R' OR h.Forekomststatus = 'U' OR
		                h.Forekomststatus = 'X1' OR h.Forekomststatus = 'X2'
	                )
	                -- The following tests should be removed
	                -- as soon as the database is corrected.
	                AND h.datum1 IS NOT NULL
	                AND h.datum2 IS NOT NULL
	                AND h.rn1 IS NOT NULL
	                AND h.rn2 IS NOT NULL";
                
                return await QueryAsync<ObservationEntity>(query, new {StartId = startId, EndId = startId + maxRows - 1});
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting observations");

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<(int minId, int maxId)> GetIdSpanAsync()
        {
            try
            {
                string query = $@"
                 SELECT 
                    MIN(h.ArtDataIDNR) AS minId,
                    MAX(h.ArtDataIDNR) AS maxId
                FROM 
	                HUVUDTABELL h";

                return (await QueryAsync<(int minId, int maxId)>(query, null)).FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting min and max id");

                return default;
            }
        }

        /// <inheritdoc />
        public async Task<DateTime?> GetLastModifiedDateAsyc()
        {
            try
            {
                string query = $@"
                SELECT 
	                MAX(h.UpdatedDate)
                FROM 
	               HUVUDTABELL h";

                return (await QueryAsync<DateTime?>(query)).FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting last modified date");

                return null;
            }
        }
    }
}