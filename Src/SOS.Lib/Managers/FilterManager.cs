﻿using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.UserService;
using SOS.Lib.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// Add extended authorization if any
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="authorityIdentity"></param>
        /// <param name="areaBuffer"></param>
        /// <param name="usePointAccuracy"></param>
        /// <param name="useDisturbanceRadius"></param>
        /// <param name="addAreaGeometries"></param>
        /// <returns></returns>
        private async Task<List<ExtendedAuthorizationAreaFilter>> GetExtendedAuthorizationAreas(int userId, int roleId, string authorizationApplicationIdentifier, string authorityIdentity, int areaBuffer, bool usePointAccuracy, bool useDisturbanceRadius, bool addAreaGeometries)
        {
            if (userId == 0)
            {
                return null;
            }

            IEnumerable<AuthorityModel> authorities = null;

            if (roleId != 0)
            {
                var userRoles = await _userService.GetUserRolesAsync(userId, authorizationApplicationIdentifier);
                var role = userRoles?.Where(r => r.Id.Equals(roleId))?.FirstOrDefault();
                authorities = role?.Authorities;
            }
            else
            {
                // Get user authorities
                authorities = (await _userService.GetUserAuthoritiesAsync(userId, authorizationApplicationIdentifier))?.Where(a => a.AuthorityIdentity.Equals(authorityIdentity));
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
                    extendedAuthorizationAreaFilter.GeographicAreas = await PopulateGeographicalFilterAsync(areaFilters, areaBuffer, usePointAccuracy, useDisturbanceRadius, addAreaGeometries);
                }

                extendedAuthorizationAreaFilter.TaxonIds = (await GetTaxonIdsAsync(authority.TaxonIds, true, null))?.ToList();

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
        public async Task PrepareTaxonFilterAsync(TaxonFilter filter)
        {
            if (filter == null)
            {
                return;
            }

            filter.Ids = await GetTaxonIdsAsync(filter.Ids,
                filter.IncludeUnderlyingTaxa,
                filter.ListIds,
                filter.TaxonListOperator,
                filter.TaxonCategories);
        }

        private async Task<List<int>> GetTaxonIdsAsync(IEnumerable<int> taxonIds,
            bool includeUnderlyingTaxa,
            IEnumerable<int> listIds,
            TaxonFilter.TaxonListOp listOperator = TaxonFilter.TaxonListOp.Merge,
            IEnumerable<int> taxonCategories = null,
            bool returnBiotaResultAsNull = true)
        {
            var taxaIds = await GetTaxonFilterIdsAsync(taxonIds, includeUnderlyingTaxa, returnBiotaResultAsNull);
            if (listIds != null && listIds.Count() > 0)
            {
                taxaIds = await FilterTaxonByTaxonListsAsync(taxaIds, listIds, listOperator, includeUnderlyingTaxa);
            }

            if (taxonCategories?.Any() ?? false)
            {
                if (taxaIds == null || taxaIds.Count() == 0 && includeUnderlyingTaxa)
                {
                    // If there are no taxaIds we need to add all taxa before taxon category filters are applied.
                    var taxonTree = await _taxonManager.GetTaxonTreeAsync();
                    taxaIds = taxonTree.GetUnderlyingTaxonIds(BiotaTaxonId, true);
                }

                taxaIds = await FilterTaxonIdsByTaxonCategoriesAsync(taxaIds, taxonCategories);
                if (taxaIds == null || taxaIds.Count() == 0)
                {
                    // If the filter results in no taxa, there should be no matching occurrences.
                    taxaIds = new List<int> { -1 };
                }
            }

            return taxaIds?.Distinct().ToList();
        }

        private async Task<List<int>> FilterTaxonByTaxonListsAsync(IEnumerable<int> taxaIds,
            IEnumerable<int> listIds,
            TaxonFilter.TaxonListOp listOperator,
            bool includeUnderlyingTaxa)
        {
            var taxonListIdsSet = new HashSet<int>();
            var taxonListSetById = await _taxonManager.GetTaxonListSetByIdAsync();
            foreach (var taxonListId in listIds)
            {
                if (taxonListSetById?.TryGetValue(taxonListId, out var taxonListSet) ?? false)
                {
                    if (includeUnderlyingTaxa)
                    {
                        taxonListIdsSet.UnionWith(taxonListSet.WithUnderlyingTaxa);
                    }
                    else
                    {
                        taxonListIdsSet.UnionWith(taxonListSet.Taxa);
                    }
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

            // When TaxonListOp.Filter is used and there are no taxa in the result, this should result in no observations result.
            if (listOperator == TaxonFilter.TaxonListOp.Filter && taxaSet.Count == 0)
            {
                return new List<int> { -1 };
            }

            return taxaSet.ToList();
        }

        /// <summary>
        /// Get taxon ids.
        /// </summary>
        /// <param name="taxonIds"></param>
        /// <param name="includeUnderlyingTaxa"></param>
        /// <param name="returnBiotaResultAsNull">If true, and the result will be every taxa, returnu null; otherwise return all taxon ids.</param>
        /// <returns></returns>
        private async Task<IEnumerable<int>> GetTaxonFilterIdsAsync(
            IEnumerable<int> taxonIds,
            bool includeUnderlyingTaxa,
            bool returnBiotaResultAsNull = true
        )
        {
            var taxonTree = await _taxonManager.GetTaxonTreeAsync();
            if ((!taxonIds?.Any() ?? true) || !includeUnderlyingTaxa)
            {
                if (!returnBiotaResultAsNull && includeUnderlyingTaxa)
                {                    
                    return taxonTree.GetUnderlyingTaxonIds(new int[] { 0 }, true);
                }
                else
                {
                    return taxonIds;
                }
            }

            if (returnBiotaResultAsNull && taxonIds.Contains(BiotaTaxonId))
            {
                return null;
            }
            else
            {
                return taxonTree.GetUnderlyingTaxonIds(taxonIds, true);
            }
        }

        /// <summary>
        /// Filter taxon ids by taxon categories.
        /// </summary>
        /// <param name="taxonIds"></param>
        /// <param name="taxonCategories"></param>
        /// <returns></returns>
        private async Task<IEnumerable<int>> FilterTaxonIdsByTaxonCategoriesAsync(IEnumerable<int> taxonIds,
            IEnumerable<int> taxonCategories)
        {
            if (taxonCategories == null || !taxonCategories.Any() || taxonIds == null)
            {
                return taxonIds;
            }

            var taxonCategorySet = new HashSet<int>(taxonCategories);
            var filteredIdsByTaxonCategories = new List<int>();
            foreach (var taxonId in taxonIds)
            {
                var taxonTree = await _taxonManager.GetTaxonTreeAsync();
                var node = taxonTree.GetTreeNode(taxonId);
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
        /// <param name="addAreaGeometries">Used by signal serach to validate bounding boxes and geometries</param>
        /// <returns></returns>
        private async Task<GeographicAreasFilter> PopulateGeographicalFilterAsync(IEnumerable<AreaFilter> areas, int areaBuffer, bool usePointAccuracy, bool useDisturbanceRadius, bool addAreaGeometries = false)
        {
            if (!areas?.Any() ?? true)
            {
                return null;
            }

            var geographicFilter = new GeographicAreasFilter();

            foreach (var areaFilter in areas)
            {
                if (addAreaGeometries || areaBuffer != 0 || usePointAccuracy || useDisturbanceRadius)
                {
                    await AddGeometryAsync(geographicFilter, areaFilter.AreaType, areaFilter.FeatureId, areaBuffer, usePointAccuracy, useDisturbanceRadius);
                    continue;
                }

                switch (areaFilter.AreaType)
                {
                    case AreaType.CountryRegion:
                        (geographicFilter.CountryRegionIds ??= new List<string>()).Add(areaFilter.FeatureId);
                        break;
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
            var geometry = await _areaCache.GetGeometryAsync(areaType, featureId);

            if (geometry != null)
            {
                // If area buffer is set. Extend area with buffer in order to hit outside original area
                if (areaBuffer != 0)
                {
                    geometry = geometry.Buffer(areaBuffer);
                }

                (geographicFilter.GeometryFilter ??= new GeographicsFilter
                {
                    MaxDistanceFromPoint = 0,
                    UseDisturbanceRadius = useDisturbanceRadius,
                    UsePointAccuracy = usePointAccuracy
                }).Geometries.Add(geometry);
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
        public async Task PrepareFilterAsync(int? roleId, 
            string authorizationApplicationIdentifier, 
            SearchFilterBase filter, 
            string authorityIdentity, 
            int? areaBuffer, 
            bool? authorizationUsePointAccuracy, 
            bool? authorizationUseDisturbanceRadius, 
            bool? setDefaultProviders,
            bool? addAreaGeometries)
        {
            filter.RoleId = roleId;
            filter.AuthorizationApplicationIdentifier = authorizationApplicationIdentifier;

            if (filter.ExtendedAuthorization.ProtectionFilter == ProtectionFilter.BothPublicAndSensitive && filter.ExtendedAuthorization != null && filter.ExtendedAuthorization.UserId != 0)
            {
                var searchFilter = filter as SearchFilter;
                if (searchFilter != null)
                {
                    EnsureIsGeneralizedObservationIsRetrievedFromDb(searchFilter.Output);
                }
            }

            if (!filter.ExtendedAuthorization.ProtectionFilter.Equals(ProtectionFilter.Public))
            {
                filter.ExtendedAuthorization.ExtendedAreas = await GetExtendedAuthorizationAreas(filter.ExtendedAuthorization.UserId, roleId ?? 0, authorizationApplicationIdentifier, authorityIdentity, areaBuffer ?? 0, authorizationUsePointAccuracy ?? false, authorizationUseDisturbanceRadius ?? false, addAreaGeometries ?? false);

                // If it's a request for protected observations, make sure occurrence.occurrenceId will be returned for log purpose
                if (filter is SearchFilter searchFilter)
                {
                    // try fix the following error "Collection was modified; enumeration operation may not execute"
                    // by introducing a copy of the searchFilter.Output?.Fields? property.
                    var fields = searchFilter.Output?.Fields?.ToList(); 

                    if (fields?.Any() ?? false)
                    {
                        if (!fields.Any(f => f.Equals("occurrence", StringComparison.CurrentCultureIgnoreCase)) &&
                            !fields.Any(f => f.Equals("occurrence.occurrenceId", StringComparison.CurrentCultureIgnoreCase)))
                        {
                            searchFilter.Output ??= new OutputFilter();
                            searchFilter.Output.Fields.Add("occurrence.occurrenceId");
                        }
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

            await PrepareTaxonFilterAsync(filter.Taxa);
        }

        private static void EnsureIsGeneralizedObservationIsRetrievedFromDb(OutputFilter outputFilter)
        {
            if (outputFilter?.Fields == null) return;
            if (!outputFilter.Fields.Any(f => f.Equals("IsGeneralized", StringComparison.CurrentCultureIgnoreCase)))
            {
                outputFilter.Fields.Add("IsGeneralized");
            }
        }

        /// <inheritdoc />
        public async Task PrepareFilterAsync(ChecklistSearchFilter filter)
        {
            if (filter.Location?.Areas != null)
            {
                filter.Location.AreaGeographic = await PopulateGeographicalFilterAsync(filter.Location.Areas, 0, filter.Location.Geometries?.UsePointAccuracy ?? false, filter.Location.Geometries?.UseDisturbanceRadius ?? false);
            }
        }

        public async Task<HashSet<int>> GetTaxonIdsFromFilterAsync(TaxonFilter filter)
        {
            if (filter == null)
            {
                filter = new TaxonFilter() { IncludeUnderlyingTaxa = true };
            }

            // todo - add support for red list categories.
            var taxonIds = await GetTaxonIdsAsync(filter.Ids,
                filter.IncludeUnderlyingTaxa,
                filter.ListIds,
                filter.TaxonListOperator,
                filter.TaxonCategories,
                true);

            if (taxonIds == null && !filter.IncludeUnderlyingTaxa)
            {
                taxonIds = new List<int> { 0 }; // Return only Biota if includeUnderlyingTaxa=false
            }

            return taxonIds?.ToHashSet();
        }
    }
}