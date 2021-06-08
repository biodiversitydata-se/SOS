﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search;
using SOS.Lib.Managers.Interfaces;
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
        /// Add extended authorization if any
        /// </summary>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="authorityIdentity"></param>
        /// <param name="areaBuffer"></param>
        /// <param name="usePointAccuracy"></param>
        /// <param name="useDisturbanceRadius"></param>
        /// <returns></returns>
        private async Task<IEnumerable<ExtendedAuthorizationFilter>> AddAuthorizationAsync(string authorizationApplicationIdentifier, string authorityIdentity,  int areaBuffer, bool usePointAccuracy, bool useDisturbanceRadius)
        {
            // Get user
            var user = await _userService.GetUserAsync();

            if (user == null)
            {
                return null;
            }
            
            // Get user authorities
            var authorities = (await _userService.GetUserAuthoritiesAsync(user.Id, authorizationApplicationIdentifier))?.Where(a => a.AuthorityIdentity.Equals(authorityIdentity));

            if (authorities == null)
            {
                return null;
            }
            
            var extendedAuthorizationFilters = new List<ExtendedAuthorizationFilter>();

            foreach (var authority in authorities)
            {
                // area are mandatory to get extended authorization
                if (!authority?.Areas?.Any() ?? true)
                {
                    continue;
                }

                var extendedAuthorizationFilter = new ExtendedAuthorizationFilter
                {
                    Identity = authority.AuthorityIdentity,
                    MaxProtectionLevel = authority.MaxProtectionLevel
                };
                var areaFilters = new List<AreaFilter>();

                var authorizedToSweden = false;

                foreach (var area in authority.Areas)
                {
                    // 100 = sweden, no meaning to add since all processed observations are in sweden
                    if (area.AreaTypeId == 18 && area.FeatureId == "100")
                    {
                        authorizedToSweden = true;
                        continue;
                    }

                    areaFilters.Add(new AreaFilter { AreaType = (AreaType)area.AreaTypeId, FeatureId = area.FeatureId });
                }
                extendedAuthorizationFilter.GeographicAreas = await PopulateGeographicalFilterAsync(areaFilters, areaBuffer, usePointAccuracy, useDisturbanceRadius);
                extendedAuthorizationFilter.TaxonIds = GetTaxonIds(authority.TaxonIds, true, null);

                // To get extended authorization, taxon id's and some area must be set 
                if (
                        authorizedToSweden ||
                        (extendedAuthorizationFilter.GeographicAreas?.BirdValidationAreaIds?.Any() ?? false) ||
                        (extendedAuthorizationFilter.GeographicAreas?.CountyIds?.Any() ?? false) ||
                        (extendedAuthorizationFilter.GeographicAreas?.GeometryFilter?.Geometries?.Any() ?? false) ||
                        (extendedAuthorizationFilter.GeographicAreas?.MunicipalityIds?.Any() ?? false) ||
                        (extendedAuthorizationFilter.GeographicAreas?.ParishIds?.Any() ?? false) ||
                        (extendedAuthorizationFilter.GeographicAreas?.ProvinceIds?.Any() ?? false)
                )
                {
                    extendedAuthorizationFilters.Add(extendedAuthorizationFilter);
                }
            }

            return extendedAuthorizationFilters;
        }

        private async Task<IEnumerable<int>> PopulateDataProviderFilterAsync(IEnumerable<int> dataproviderIds)
        {
            
            // If no data provider is passed, get them with data quality is approved
            if (!dataproviderIds?.Any() ?? true)
            {
                var allProviders = await _dataProviderCache.GetAllAsync();
                return allProviders?.Where(p => p.IsActive && p.IncludeInSearchByDefault).Select(p => p.Id);
            }

            return dataproviderIds;
        }

        /// <summary>
        ///  Handle taxon filtering
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private void PopulateTaxonFilter(TaxonFilter filter)
        {
            if (filter == null)
            {
                return;
            }

            filter.Ids = GetTaxonIds(filter.Ids, filter.IncludeUnderlyingTaxa, filter.ListIds,
                filter.TaxonListOperator);
        }

        private IEnumerable<int> GetTaxonIds(IEnumerable<int> taxonIds, bool includeUnderlyingTaxa, IEnumerable<int> listIds, TaxonFilter.TaxonListOp listOperator = TaxonFilter.TaxonListOp.Merge)
        {
            var taxaIds = GetTaxonFilterIds(taxonIds, includeUnderlyingTaxa);
            if (!listIds?.Any() ?? true)
            {
                return taxaIds;
            }

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
                return taxaIds;
            }

            if (!taxaIds?.Any() ?? true)
            {
                return taxonListIdsSet;
            }

            if (listOperator == TaxonFilter.TaxonListOp.Merge)
            {
                taxonListIdsSet.UnionWith(taxaIds);
                return taxonListIdsSet;
            }

            var taxaSet = new HashSet<int>();
            taxaSet.UnionWith(taxaIds);
            taxaSet.IntersectWith(taxonListIdsSet);
            return taxaSet;
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
        public async Task PrepareFilter(string authorizationApplicationIdentifier, FilterBase filter, string authorityIdentity, int? areaBuffer, bool? authorizationUsePointAccuracy, bool? authorizationUseDisturbanceRadius)
        {
            if (filter.ProtectedObservations)
            {
                filter.ExtendedAuthorizations = await AddAuthorizationAsync(authorizationApplicationIdentifier, authorityIdentity, areaBuffer ?? 0, authorizationUsePointAccuracy ?? false, authorizationUseDisturbanceRadius ?? false);
            }

            filter.DataProviderIds = await PopulateDataProviderFilterAsync(filter.DataProviderIds);
            filter.AreaGeographic = await PopulateGeographicalFilterAsync(filter.Areas, areaBuffer ?? 0, filter.Geometries?.UsePointAccuracy ?? false, filter.Geometries?.UseDisturbanceRadius ?? false);
            PopulateTaxonFilter(filter.Taxa);
        }
    }
}
