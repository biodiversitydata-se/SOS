using Microsoft.Extensions.Logging;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Enums.Artportalen;
using SOS.Lib.Enums.VocabularyValues;
using System.Data;

namespace SOS.Harvest.Repositories.Source.Artportalen;

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
            INNER JOIN SightingState ss ON s.SightingId = ss.SightingId ";  

    // Todo arguments for protected sightings       
    protected string GetSightingWhereBasics(bool isIncrementalHarvest)
    {
        var where = @$" 
                s.SightingTypeId IN ({string.Join(",", // s.SightingTypeId IN (0,1,2,3,5,6.7,8,9,10)
                        (int)SightingType.NormalSighting,
                        (int)SightingType.AggregationSighting,
                        (int)SightingType.AssessmentSighting,
                        (int)SightingType.ReplacementSighting,
                        (int)SightingType.AssessmentSightingForBreeding,
                        (int)SightingType.AssessmentSightingForMaxCount,
                        (int)SightingType.AssessmentSightingForDifferentSites,
                        (int)SightingType.CorrectionSighting,
                        (int)SightingType.AssessmentSightingForHerd,
                        (int)SightingType.AssessmentSightingForOwnBreeding)}) 
                AND s.SightingTypeSearchGroupId IN ({string.Join(",", // s.SightingTypeSearchGroupId IN (1, 2, 4, 8, 16, 32, 64, 128, 256).
                        (int)SightingTypeSearchGroup.Ordinary,
                        (int)SightingTypeSearchGroup.Assessment,
                        (int)SightingTypeSearchGroup.Aggregated,
                        (int)SightingTypeSearchGroup.AggregatedChild,
                        (int)SightingTypeSearchGroup.AssessmentChild,
                        (int)SightingTypeSearchGroup.Replacement,
                        (int)SightingTypeSearchGroup.ReplacementChild,
                        (int)SightingTypeSearchGroup.OwnBreedingAssessment,
                        (int)SightingTypeSearchGroup.OwnBreedingAssessmentChild)})
                AND s.ValidationStatusId <> {(int)ValidationStatusId.Rejected} 
                AND ss.IsActive = 1
                AND ss.SightingStateTypeId = {(int)SightingStateType.Published}
                AND s.TaxonId IS NOT NULL";

        if (!isIncrementalHarvest && (DataService?.Configuration?.HarvestStartDate.HasValue ?? false))
        {
            where += $" AND s.StartDate >= '{DataService.Configuration.HarvestStartDate}'";
        }

        return where;
    }


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
    public async Task<IEnumerable<E>> QueryAsync<E>(string query, dynamic? parameters = null!, CommandType commandType = CommandType.Text)
    {
        return await DataService.QueryAsync<E>(query, parameters, Live, commandType);
    }

    /// <inheritdoc />
    public bool Live { get; set; }
}