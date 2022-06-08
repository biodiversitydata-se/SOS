using Microsoft.Extensions.Logging;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Enums.Artportalen;
using SOS.Lib.Enums.VocabularyValues;

namespace SOS.Harvest.Repositories.Source.Artportalen
{
    /// <summary>
    ///     Database base class
    /// </summary>
    public class BaseRepository<T> : IBaseRepository<T>
    {
        /// <summary>
        /// Data service
        /// </summary>
        protected IArtportalenDataService DataService
        {
            get;
        }

        /// <summary>
        ///     Logger
        /// </summary>
        protected ILogger<T> Logger { get; }

        protected string SightingsFromBasics => @$"
            SearchableSightings s WITH(NOLOCK)
            INNER JOIN SightingState ss ON s.SightingId = ss.SightingId AND ss.IsActive=1";

        // Todo arguments for protected sightings       
        protected string SightingWhereBasics => @$" 
            s.SightingTypeId IN ({string.Join(",", // s.SightingTypeId IN (0,1,3,8,10)
                (int)SightingType.NormalSighting,
                (int)SightingType.AggregationSighting,
                (int)SightingType.ReplacementSighting,
                (int)SightingType.CorrectionSighting,
                (int)SightingType.AssessmentSightingForOwnBreeding)}) 
            AND s.SightingTypeSearchGroupId IN ({string.Join(",", // s.SightingTypeSearchGroupId IN (1, 2, 4, 16, 32, 128). 2 is unnecessary since we don't harvest SightingTypeId 2,5,6,7,9. Remove?
                (int)SightingTypeSearchGroup.Ordinary,
                (int)SightingTypeSearchGroup.Assessment,
                (int)SightingTypeSearchGroup.Aggregated,
                (int)SightingTypeSearchGroup.AssessmentChild,
                (int)SightingTypeSearchGroup.Replacement,
                (int)SightingTypeSearchGroup.OwnBreedingAssessment)})
	        AND s.ValidationStatusId <> { // s.ValidationStatusId <> 50
            (int)ValidationStatusId.Rejected} 
            AND ss.IsActive = 1
            AND ss.SightingStateTypeId = { // ss.SightingStateTypeId = 30
            (int)SightingStateType.Published}
            AND s.TaxonId IS NOT NULL 
             {((DataService?.Configuration?.HarvestStartDate.HasValue ?? false) ?
            $"AND s.StartDate >= '{DataService.Configuration.HarvestStartDate}'"
            :
            "")}";

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenDataService"></param>
        /// <param name="logger"></param>
        public BaseRepository(IArtportalenDataService artportalenDataService, ILogger<T> logger)
        {
            DataService =
                artportalenDataService ?? throw new ArgumentNullException(nameof(artportalenDataService));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<E>> QueryAsync<E>(string query, dynamic parameters = null!)
        {
            return await DataService.QueryAsync<E>(query, parameters, Live);
        }

        /// <inheritdoc />
        public bool Live { get; set; }
    }
}