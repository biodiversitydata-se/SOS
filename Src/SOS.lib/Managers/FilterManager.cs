using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.UserService;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Managers
{
    /// <summary>
    /// Filter manager.
    /// </summary>
    public class FilterManager : IFilterManager
    {
        private const int BiotaTaxonId = 0;
        
        private readonly ITaxonManager _taxonManager;
        private IUserService _userService;
        private readonly IAreaCache _areaCache;
        private readonly IDataProviderCache _dataProviderCache;

        public IUserService UserService
        {
            get => _userService;
            set => _userService = value;
        }

        /// <summary>
        ///  Add extended authorization if any
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="authorityIdentity"></param>
        /// <param name="areaBuffer"></param>
        /// <param name="usePointAccuracy"></param>
        /// <param name="useDisturbanceRadius"></param>
        /// <returns></returns>
        private async Task<List<ExtendedAuthorizationAreaFilter>> GetExtendedAuthorizationAreas(UserModel user, int roleId, string authorizationApplicationIdentifier, string authorityIdentity,  int areaBuffer, bool usePointAccuracy, bool useDisturbanceRadius)
        {
            if (user == null)
            {
                return null;
            }

            IEnumerable<AuthorityModel> authorities = null;

            if (roleId != 0)
            {
                var userRoles = await _userService.GetUserRolesAsync(user.Id, authorizationApplicationIdentifier);
                
                var role = userRoles?.Where(r => r.Id.Equals(roleId))?.FirstOrDefault();
                authorities = role?.Authorities;
            }
            else
            {
                // Get user authorities
                authorities = (await _userService.GetUserAuthoritiesAsync(user.Id, authorizationApplicationIdentifier))?.Where(a => a.AuthorityIdentity.Equals(authorityIdentity));
            }
            
            if (authorities == null)
            {
                return null;
            }
            
            var extendedAuthorizationAreaFilters = new List<ExtendedAuthorizationAreaFilter>();

            foreach (var authority in authorities)
            {
                // Max protection level must be greater than or equal to 3
                if (authority.MaxProtectionLevel < 3 && authority.AuthorityIdentity != "SightingIndication")
                {
                    continue;
                }

                var extendedAuthorizationAreaFilter = new ExtendedAuthorizationAreaFilter
                {
                    Identity = authority.AuthorityIdentity,
                    MaxProtectionLevel = authority.MaxProtectionLevel
                };

                if (authority.Areas?.Any() ?? false)
                {
                    var areaFilters = new List<AreaFilter>();
                    
                    foreach (var area in authority.Areas)
                    {
                        // 100 = sweden, no meaning to add since all processed observations are in sweden
                        if (area.AreaTypeId == 18 && area.FeatureId == "100")
                        {
                            continue;
                        }

                        areaFilters.Add(new AreaFilter { AreaType = (AreaType)area.AreaTypeId, FeatureId = area.FeatureId });
                    }
                    extendedAuthorizationAreaFilter.GeographicAreas = await PopulateGeographicalFilterAsync(areaFilters, areaBuffer, usePointAccuracy, useDisturbanceRadius);
                }

                extendedAuthorizationAreaFilter.TaxonIds = GetTaxonIds(authority.TaxonIds, true, null)?.ToList();

                extendedAuthorizationAreaFilters.Add(extendedAuthorizationAreaFilter);
            }

            return extendedAuthorizationAreaFilters;
        }

        private async Task<IEnumerable<int>> GetDefaultDataProvidersIfEmptyAsync(IEnumerable<int> dataproviderIds)
        {
            // If no data provider is passed, get them with data quality is approved
            if (!dataproviderIds?.Any() ?? true)
            {
                var allProviders = await _dataProviderCache.GetAllAsync();
                return allProviders?.Where(p => p.IsActive && p.IncludeInSearchByDefault).Select(p => p.Id).ToList();
            }

            return dataproviderIds;
        }

        /// <summary>
        ///  Handle taxon filtering
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public void PrepareTaxonFilter(TaxonFilter filter)
        {
            if (filter == null)
            {
                return;
            }

            filter.Ids = GetTaxonIds(filter.Ids, 
                filter.IncludeUnderlyingTaxa, 
                filter.ListIds,
                filter.TaxonListOperator, 
                filter.TaxonCategories);
        }

        private List<int> GetTaxonIds(IEnumerable<int> taxonIds, 
            bool includeUnderlyingTaxa, 
            IEnumerable<int> listIds, 
            TaxonFilter.TaxonListOp listOperator = TaxonFilter.TaxonListOp.Merge,
            List<int> taxonCategories = null)
        {            
            var taxaIds = GetTaxonFilterIds(taxonIds, includeUnderlyingTaxa);
            if (listIds != null && listIds.Count() > 0)
            {
                taxaIds = FilterTaxonByTaxonLists(taxaIds, listIds, listOperator);
            }

            if (taxonCategories != null && taxonCategories.Count > 0)
            {
                if (taxaIds == null || taxaIds.Count() == 0 && includeUnderlyingTaxa)
                {
                    // If there are no taxaIds we need to add all taxa before taxon category filters are applied.
                    taxaIds = _taxonManager.TaxonTree.GetUnderlyingTaxonIds(BiotaTaxonId, true);
                }

                taxaIds = FilterTaxonIdsByTaxonCategories(taxaIds, taxonCategories);
                if (taxaIds == null || taxaIds.Count() == 0)
                {
                    // If the filter results in no taxa, there should be no matching occurrences.
                    taxaIds = new List<int> { -1 };
                }
            }

            return taxaIds?.ToList();
        }

        private List<int> FilterTaxonByTaxonLists(IEnumerable<int> taxaIds, IEnumerable<int> listIds, TaxonFilter.TaxonListOp listOperator)
        {
            var taxonListIdsSet = new HashSet<int>();
            foreach (var taxonListId in listIds)
            {
                if (_taxonManager.TaxonListSetById.TryGetValue(taxonListId, out var taxonListSet))
                {
                    taxonListIdsSet.UnionWith(taxonListSet);
                }
            }

            if (taxonListIdsSet.Count == 0)
            {
                return taxaIds?.ToList();
            }

            if (!taxaIds?.Any() ?? true)
            {
                return taxonListIdsSet.ToList();
            }

            if (listOperator == TaxonFilter.TaxonListOp.Merge)
            {
                taxonListIdsSet.UnionWith(taxaIds);
                return taxonListIdsSet.ToList();
            }

            var taxaSet = new HashSet<int>();
            taxaSet.UnionWith(taxaIds);
            taxaSet.IntersectWith(taxonListIdsSet);
            return taxaSet.ToList();
        }

        private IEnumerable<int> GetTaxonFilterIds(
            IEnumerable<int> taxonIds,
            bool includeUnderlyingTaxa
        )
        {                        
            if ((!taxonIds?.Any() ?? true) || !includeUnderlyingTaxa)
            {
                return taxonIds;
            }

            return taxonIds.Contains(BiotaTaxonId) ? null : _taxonManager.TaxonTree.GetUnderlyingTaxonIds(taxonIds, true);
        }

        /// <summary>
        /// Filter taxon ids by taxon categories.
        /// </summary>
        /// <param name="taxonIds"></param>
        /// <param name="taxonCategories"></param>
        /// <returns></returns>
        private IEnumerable<int> FilterTaxonIdsByTaxonCategories(IEnumerable<int> taxonIds, 
            List<int> taxonCategories)
        {
            if (taxonCategories == null || !taxonCategories.Any() || taxonIds == null)
            {
                return taxonIds;
            }

            var taxonCategorySet = new HashSet<int>(taxonCategories);
            var filteredIdsByTaxonCategories = new List<int>();
            foreach (var taxonId in taxonIds)
            {
                var node = _taxonManager.TaxonTree.GetTreeNode(taxonId);
                if (node != null)
                {
                    int? taxonCategoryId = node?.Data?.Attributes?.TaxonCategory?.Id;
                    if (!taxonCategoryId.HasValue)
                    {
                        filteredIdsByTaxonCategories.Add(taxonId);
                    }
                    else
                    {
                        if (taxonCategorySet.Contains(taxonCategoryId.Value))
                        {
                            filteredIdsByTaxonCategories.Add(taxonId);
                        }
                    }
                }
            }

            taxonIds = filteredIdsByTaxonCategories;
            return taxonIds;
        }

        /// <summary>
        /// Populate geographical filter
        /// </summary>
        /// <param name="areas"></param>
        /// <param name="areaBuffer"></param>
        /// <param name="usePointAccuracy"></param>
        /// <param name="useDisturbanceRadius"></param>
        /// <returns></returns>
        private async Task<GeographicAreasFilter> PopulateGeographicalFilterAsync(IEnumerable<AreaFilter> areas, int areaBuffer, bool usePointAccuracy, bool useDisturbanceRadius)
        {
            if (!areas?.Any() ?? true)
            {
                return null;
            }

            var geographicFilter = new GeographicAreasFilter();
            foreach (var areaFilter in areas)
            {
                if (areaBuffer != 0 || usePointAccuracy || useDisturbanceRadius)
                {
                    await AddGeometryAsync(geographicFilter, areaFilter.AreaType, areaFilter.FeatureId, areaBuffer, usePointAccuracy, useDisturbanceRadius);
                    continue;
                }

                switch (areaFilter.AreaType)
                {
                    case AreaType.County:
                        (geographicFilter.CountyIds ??= new List<string>()).Add(areaFilter.FeatureId);
                        break;
                    case AreaType.Municipality:
                        (geographicFilter.MunicipalityIds ??= new List<string>()).Add(areaFilter.FeatureId);
                        break;
                    case AreaType.Province:
                        (geographicFilter.ProvinceIds ??= new List<string>()).Add(areaFilter.FeatureId);
                        break;
                    case AreaType.Parish:
                        (geographicFilter.ParishIds ??= new List<string>()).Add(areaFilter.FeatureId);
                        break;
                    case AreaType.BirdValidationArea:
                        if (areaFilter.FeatureId == "100")
                        {
                            // 100 = sweden, no meaning to add since all processed observations are in sweden
                            continue;
                        }

                        (geographicFilter.BirdValidationAreaIds ??= new List<string>()).Add(areaFilter.FeatureId);
                        break;
                    default:
                        await AddGeometryAsync(geographicFilter, areaFilter.AreaType, areaFilter.FeatureId, areaBuffer, usePointAccuracy, useDisturbanceRadius);
                        break;
                }
            }

            return geographicFilter;
        }

        /// <summary>
        /// Add geometry to geographic filter
        /// </summary>
        /// <param name="geographicFilter"></param>
        /// <param name="areaType"></param>
        /// <param name="featureId"></param>
        /// <param name="areaBuffer"></param>
        /// <param name="usePointAccuracy"></param>
        /// <param name="useDisturbanceRadius"></param>
        /// <returns></returns>
        private async Task AddGeometryAsync(GeographicAreasFilter geographicFilter, AreaType areaType, string featureId, int areaBuffer, bool usePointAccuracy, bool useDisturbanceRadius)
        {
            var geoShape = await _areaCache.GetGeometryAsync(areaType, featureId);

            if (geoShape != null)
            {
                // If area buffer is set. Extend area with buffer in order to hit outside original area
                if (areaBuffer != 0)
                {
                    geoShape = geoShape.ToGeometry().Buffer(areaBuffer).ToGeoShape();
                }

                (geographicFilter.GeometryFilter ??= new GeographicsFilter
                {
                    MaxDistanceFromPoint = 0,
                    UseDisturbanceRadius = useDisturbanceRadius,
                    UsePointAccuracy = usePointAccuracy
                }).Geometries.Add(geoShape);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxonManager"></param>
        /// <param name="userService"></param>
        /// <param name="areaCache"></param>
        /// <param name="dataProviderCache"></param>
        public FilterManager(ITaxonManager taxonManager, IUserService userService, IAreaCache areaCache, IDataProviderCache dataProviderCache)
        {
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
            _dataProviderCache = dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));
        }

        /// <inheritdoc />
        public async Task PrepareFilter(int? roleId, string authorizationApplicationIdentifier, SearchFilterBase filter, string authorityIdentity, int? areaBuffer, bool? authorizationUsePointAccuracy, bool? authorizationUseDisturbanceRadius, bool? setDefaultProviders)
        {
            // Get user
            var user = await _userService.GetUserAsync();
            filter.ExtendedAuthorization.UserId = user?.Id ?? 0;

            if (filter.ExtendedAuthorization.ProtectedObservations)
            {
                filter.ExtendedAuthorization.ExtendedAreas = await GetExtendedAuthorizationAreas(user, roleId ?? 0, authorizationApplicationIdentifier, authorityIdentity, areaBuffer ?? 0, authorizationUsePointAccuracy ?? false, authorizationUseDisturbanceRadius ?? false);

                // If it's a request for protected observations, make sure occurrence.occurrenceId will be returned for log purpose
                if (filter is SearchFilter searchFilter)
                {
                    if ((searchFilter.OutputFields?.Any() ?? false) &&
                        !searchFilter.OutputFields.Any(f => f.Equals("occurrence", StringComparison.CurrentCultureIgnoreCase)) &&
                        !searchFilter.OutputFields.Any(f => f.Equals("occurrence.occurrenceId", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        searchFilter.OutputFields.Add("occurrence.occurrenceId");
                    }
                }
            }

            if (setDefaultProviders.HasValue && setDefaultProviders.Value)
            {
                filter.DataProviderIds = (await GetDefaultDataProvidersIfEmptyAsync(filter.DataProviderIds))?.ToList();
            }

            if (filter.Location?.Areas != null)
            {
                filter.Location.AreaGeographic = await PopulateGeographicalFilterAsync(filter.Location.Areas, areaBuffer ?? 0, filter.Location.Geometries?.UsePointAccuracy ?? false, filter.Location.Geometries?.UseDisturbanceRadius ?? false);
            }
            
            PrepareTaxonFilter(filter.Taxa);
        }        
    }
}
