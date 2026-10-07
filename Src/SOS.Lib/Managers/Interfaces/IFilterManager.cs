using SOS.Api.Contracts.Observations.Requests;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Shared;
using SOS.Lib.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Managers.Interfaces;

/// <summary>
/// Filter manager.
/// </summary>
public interface IFilterManager
{
    /// <summary>
    /// The user service to use when getting authorities.
    /// </summary>
    public IUserService UserService { get; set; }

    /// <summary>
    /// Creates a with additional information if necessary. E.g. adding underlying taxon ids.
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="authorizationApplicationIdentifier"></param>
    /// <param name="filter"></param>
    /// <param name="authorityIdentity"></param>
    /// <param name="areaBuffer"></param>
    /// <param name="authorizationUsePointAccuracy"></param>
    /// <param name="authorizationUseDisturbanceRadius"></param>
    /// <param name="setDefaultProviders"></param>
    /// <param name="addAreaGeometries"></param>
    /// <returns></returns>
    Task PrepareFilterAsync(
        int? roleId,
        string authorizationApplicationIdentifier,
        SearchFilterBase filter,
        string authorityIdentity = "Sighting",
        int? areaBuffer = 0,
        bool? authorizationUsePointAccuracy = false,
        bool? authorizationUseDisturbanceRadius = false,
        bool? setDefaultProviders = true,
        bool? addAreaGeometries = false);

    /// <summary>
    /// Add additional information if necessary
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task PrepareFilterAsync(ChecklistSearchFilter filter);

    /// <summary>
    /// Prepare taxon filter
    /// </summary>
    /// <param name="filter"></param>
    Task PrepareTaxonFilterAsync(TaxonFilter filter);

    /// <summary>
    /// Get taxon ids from filter.
    /// </summary>
    /// <param name="taxonIds"></param>
    /// <param name="includeUnderlyingTaxa"></param>
    /// <param name="listIds"></param>
    /// <param name="listOperator"></param>
    /// <param name="taxonCategories"></param>
    /// <param name="returnBiotaResultAsNull"></param>
    /// <param name="taxonomyType"></param>
    /// <returns></returns>
    Task<List<int>> GetTaxonIdsAsync(IEnumerable<int> taxonIds,
        bool includeUnderlyingTaxa,
        IEnumerable<int> listIds,
        TaxonFilter.TaxonListOp listOperator = TaxonFilter.TaxonListOp.Merge,
        IEnumerable<int> taxonCategories = null,
        bool returnBiotaResultAsNull = true,
        TaxonomyType taxonomyType = TaxonomyType.Dyntaxa);

    /// <summary>
    /// Get taxon ids from filter.
    /// </summary>
    /// <param name="filter">The taxon filter.</param>
    /// <returns></returns>
    Task<HashSet<int>> GetTaxonIdsFromFilterAsync(TaxonFilter filter);

    Task<List<Feature>?> GetAreaFiltersAsGeoJsonFeaturesAsync(IEnumerable<AreaFilter>? areaFilters);

    Task<List<(Area area, Geometry geometry)>?> GetAreaFiltersAsAreaTuplesAsync(IEnumerable<AreaFilter>? areaFilters);

    Task<List<Feature>?> GetGeographicsFilterAsGeoJsonFeaturesAsync(GeographicsFilter geographicsFilter);
}
