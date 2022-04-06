using Dapper;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Extensions;

namespace SOS.Harvest.Repositories.Source.Artportalen
{
    public class SightingRepository : BaseRepository<ISightingRepository>, ISightingRepository
    {
        

        private string GetSightingQuery(int top, string where) => GetSightingQuery(top, null, where);


        /// <summary>
        /// Create sighting query
        /// </summary>
        /// <param name="top"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        private string GetSightingQuery(int top, string join, string where)
        {
            var topCount = "";

            // Adding TOP increases the performance
            if (top > 0)
            {
                topCount = $"TOP {top}";
            }

            var triggerRuleSelect = "svr.RegionalSightingStateId";
            var triggerRuleFrom = @"LEFT JOIN TriggeredValidationRule tvr on tvr.SightingId = ss.SightingId 
                    LEFT JOIN StatusValidationRule svr ON svr.Id = tvr.StatusValidationRuleId ";
            
            if (DataService.Configuration?.UseTriggeredObservationRule ?? false)
            {
                triggerRuleSelect = @"tor.FrequencyId,
                tor.ReproductionId";
                triggerRuleFrom = @"LEFT JOIN TriggeredObservationRule tor ON tor.SightingId = s.SightingId";
            }

            var query = $@"
                SELECT {topCount} 
                    si.Id, 
                    s.ActivityId,
                    s.DiscoveryMethodId,
					s.BiotopeId,
					sdb.[Description] AS BiotopeDescription,
	                scp.Comment,
                    s.EditDate,
	                s.EndDate,
	                s.EndTime,
	                s.GenderId,
                    s.HasImages,
	                s.HiddenByProvider,
	                s.[Length],
                    s.MaxDepth,
					s.MaxHeight,
					s.MinDepth,
					s.MinHeight,
	                s.NotPresent,
	                s.NotRecovered,
                    s.OwnerOrganizationId,
                    msi.PortalId AS MigrateSightingPortalId,
                    msi.obsid AS MigrateSightingObsId,
	                s.ProtectedBySystem,
	                s.Quantity,
					s.QuantityOfSubstrate,
                    s.RegisterDate,
	                CASE WHEN p.Id IS NULL THEN null ELSE p.FirstName + ' ' + p.LastName END AS RightsHolder,
                    si.SiteId,
                    CASE WHEN sic.SightingId IS NULL THEN 0 ELSE 1 END AS HasUserComments,
	                s.StageId,
	                s.StartDate,
	                s.StartTime,
					s.SubstrateId,
					sds.[Description] AS SubstrateDescription,
					sdss.[Description] AS SubstrateSpeciesDescription,
					s.SubstrateSpeciesId,
	                s.TaxonId,
	                s.UnsureDetermination,
	                s.Unspontaneous,
	                s.UnitId,
	                sb.URL AS SightingBarcodeURL,
                    s.ValidationStatusId,
	                s.[Weight], 
	                s.HasTriggeredValidationRules, 
	                s.HasAnyTriggeredValidationRuleWithWarning,
	                s.NoteOfInterest,
	                si.DeterminationMethodId,
                    s.SightingTypeId,
                    s.SightingTypeSearchGroupId,
	                srDeterminer.UserId AS DeterminerUserId,
	                srDeterminer.DeterminationYear AS DeterminationYear,
	                srConfirmator.UserId AS ConfirmatorUserId,
	                srConfirmator.DeterminationYear AS ConfirmationYear,
	                {triggerRuleSelect},
                    (select string_agg(SightingPublishTypeId, ',') from SightingPublish sp where SightingId = s.SightingId group by SightingId) AS SightingPublishTypeIds,
                    (select string_agg(SpeciesFactId , ',') from SpeciesFactTaxon sft where sft.TaxonId = s.TaxonId AND sft.IsSearchFilter = 1 group by sft.TaxonId) AS SpeciesFactsIds,
                    sdc.DatasourceId
                FROM
	                {SightingsFromBasics}
                    {join}
					INNER JOIN Sighting si ON s.SightingId = si.Id
	                LEFT JOIN SightingCommentPublic scp ON s.SightingId = scp.SightingId                                   
	                LEFT JOIN SightingBarcode sb ON s.SightingId = sb.SightingId
                    LEFT JOIN [User] u ON s.OwnerUserId = u.Id 
	                LEFT JOIN Person p ON u.PersonId = p.Id
                    LEFT JOIN MigrateSightingid msi ON s.SightingId = msi.Id
					LEFT JOIN SightingDescription sdb ON si.SightingBiotopeDescriptionId = sdb.Id 
					LEFT JOIN SightingDescription sds ON si.SightingSubstrateDescriptionId = sds.Id 
					LEFT JOIN SightingDescription sdss ON si.SightingSubstrateSpeciesDescriptionId = sdss.Id
                    LEFT JOIN SightingRelation srDeterminer ON srDeterminer.SightingId = s.SightingId AND srDeterminer.IsPublic = 1 AND srDeterminer.SightingRelationTypeId = 3
                    LEFT JOIN SightingRelation srConfirmator ON srConfirmator.SightingId = s.SightingId AND srConfirmator.IsPublic = 1 AND srConfirmator.SightingRelationTypeId = 5
                    {triggerRuleFrom}
                    LEFT JOIN SightingDatasource sdc ON s.SightingId = sdc.SightingId
                    LEFT JOIN (SELECT SightingId FROM SightingComment WITH(NOLOCK) GROUP BY SightingId) sic ON s.SightingId = sic.SightingId
                WHERE
	                {SightingWhereBasics}
                    {where} ";

            return query;
        }
            

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenDataService"></param>
        /// <param name="logger"></param>
        public SightingRepository(IArtportalenDataService artportalenDataService, ILogger<SightingRepository> logger) :
            base(artportalenDataService, logger)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SightingEntity>> GetChunkAsync(int startId, int maxRows)
        {
            try
            {
                var query = GetSightingQuery(0, "AND s.SightingId BETWEEN @StartId AND @EndId");

                var result = (await QueryAsync<SightingEntity>(query, new {StartId = startId, EndId = startId + maxRows - 1}, Live))?.ToArray();
                if ((result?.Count() ?? 0) == 0)
                {
                    Logger.LogInformation($"Artportalen SightingRepository.GetChunkAsync(int startId, int maxRows) returned no sightings. startId={startId}, maxRows={maxRows}, Query: {query}");
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sightings");

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SightingEntity>> GetChunkAsync(IEnumerable<int> sightingIds)
        {
            try
            {
                var query = GetSightingQuery(sightingIds?.Count() ?? 0, "INNER JOIN @tvp t ON s.SightingId = t.Id", null);

                var result = (await QueryAsync<SightingEntity>(query, new { tvp = sightingIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") }, Live))?.ToArray();                
                if ((result?.Count() ?? 0) == 0)
                {
                    Logger.LogInformation($"Artportalen SightingRepository.GetChunkAsync(IEnumerable<int> sightingIds) returned no sightings. Live={Live}, sightingIds.Count()={sightingIds.Count()}, Query: {query}\n,The first five SightingIds used in @tvp are: {string.Join(", ", sightingIds.Take(5))}");
                }

                return result;                
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sightings");

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SightingEntity>> GetChunkAsync(DateTime modifiedSince, int maxRows)
        {
            try
            {
                var query = GetSightingQuery(maxRows, "AND s.EditDate > @modifiedSince");

                var result = (await QueryAsync<SightingEntity>(query, new { modifiedSince = modifiedSince.ToLocalTime() }, Live))?.ToArray();
                if ((result?.Count() ?? 0) == 0)
                {
                    Logger.LogInformation($"Artportalen SightingRepository.GetChunkAsync(DateTime modifiedSince, int maxRows) returned no sightings. modifiedSince={modifiedSince}, maxRows={maxRows}, Query: {query}");
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sightings");

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<(int minId, int maxId)> GetIdSpanAsync()
        {
            try
            {
                string query = $@"
                SELECT 
                    MIN(s.SightingId) AS minId,
                    MAX(s.SightingId) AS maxId
		        FROM 
		            {SightingsFromBasics}
                WHERE 
                    {SightingWhereBasics}";

                return (await QueryAsync<(int minId, int maxId)>(query, null, Live)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting min and max id");

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
	                MAX(s.EditDate)
                FROM 
	               {SightingsFromBasics}
                WHERE
                    {SightingWhereBasics}";

                return (await QueryAsync<DateTime?>(query, null, Live)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting last modified date");

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<int>> GetModifiedIdsAsync(DateTime modifiedSince, int limit)
        {
            try
            {
                var query = $@"SELECT TOP({limit})   
	               s.SightingId AS Id
                FROM
	                {SightingsFromBasics}
                WHERE
	                {SightingWhereBasics}
                    AND s.EditDate > @modifiedSince
                ORDER BY
                    s.EditDate";

                var result = await QueryAsync<int>(query, new { modifiedSince = modifiedSince.ToLocalTime() }, Live);                
                Logger.LogDebug($"GetModifiedIdsAsync({modifiedSince}, {limit}, Live={Live}) returned { (result == null ? "null" : result.Count())} sightingIds");
                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting modified id's");

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<(int SightingId, int ProjectId)>> GetSightingProjectIdsAsync(IEnumerable<int> sightingIds)
        {
            try
            {
                var query = $@"
                SELECT 
                    ps.SightingId AS SightingId,
	                ps.ProjectId AS ProjectId
                FROM 
	                ProjectSighting ps
                    INNER JOIN @tvp t ON ps.SightingId = t.Id 
                ORDER BY 
                    ps.ProjectId DESC";

                return await QueryAsync<(int SightingId, int ProjectId)>(
                    query,
                    new { tvp = sightingIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") }, Live);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sighting/project connections");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IDictionary<int, ICollection<(int sightingId, int taxonId)>>> GetSightingsAndTaxonIdsForCheckListsAsync(IEnumerable<int> checkListIds)
        {
            var query = $@"
	        SELECT 
		        s.ChecklistId,
		        s.[Id] AS SightingId,
		        s.[TaxonId]
	        FROM 
		        Sighting s
                INNER JOIN @tvp t ON s.ChecklistId = t.Id";

            var result = await QueryAsync<(int checklistId, int sightingId, int taxonId)>(query,
                new { tvp = checkListIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") });

            if (!result?.Any() ?? true)
            {
                return null;
            }

            var checkListsData = new Dictionary<int, ICollection<(int sightingId, int taxonId)>>();

            foreach (var item in result)
            {
                if (!checkListsData.TryGetValue(item.checklistId, out var checkListData))
                {
                    checkListData = new List<(int sightingId, int taxonId)>();
                    checkListsData.Add(item.checklistId, checkListData);
                }
                checkListData.Add((item.sightingId, item.taxonId));
            }

            return checkListsData;
        }

        /// <inheritdoc />
        public bool Live { get; set; }
    }
}