using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
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
        private readonly IUserService _userService;
        private readonly IAreaCache _areaCache;
        private readonly IDataProviderCache _dataProviderCache;

        /// <summary>
        /// Add extended authorization if any
        /// </summary>
        /// <returns></returns>
        private async Task<IEnumerable<ExtendedAuthorizationFilter>> AddAuthorizationAsync()
        {
            // Get user
            var user = await _userService.GetUserAsync();

            if (user == null)
            {
                return null;
            }
            
            // Get user authorities
            var authorities = await _userService.GetUserAuthoritiesAsync(user.Id);

            if (authorities == null)
            {
                return null;
            }
            
            var extendedAuthorizationFilters = new List<ExtendedAuthorizationFilter>();

            foreach (var authority in authorities)
            {
                // taxonid's and area are mandatory to get extended authorization
                if ((!authority?.TaxonIds?.Any() ?? true) || 
                    (!authority?.Areas?.Any() ?? true))
                {
                    continue;
                }

                var extendedAuthorizationFilter = new ExtendedAuthorizationFilter()
                {
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
                extendedAuthorizationFilter.GeographicAreas = await PopulateGeographicalFilterAsync(areaFilters, false);
                extendedAuthorizationFilter.TaxonIds = PopulateTaxonFilter(authority.TaxonIds, true, null);

                // To get extended authorization, taxon id's and some area must be set 
                if (
                        (
                            (extendedAuthorizationFilter.TaxonIds?.Any() ?? false) ||
                            (authority.TaxonIds?.Contains(BiotaTaxonId) ?? false)
                        ) &&
                        (
                            authorizedToSweden ||
                            (extendedAuthorizationFilter.GeographicAreas?.BirdValidationAreaIds?.Any() ?? false) ||
                            (extendedAuthorizationFilter.GeographicAreas?.CountyIds?.Any() ?? false) ||
                            (extendedAuthorizationFilter.GeographicAreas?.GeometryFilter?.Geometries?.Any() ?? false) ||
                            (extendedAuthorizationFilter.GeographicAreas?.MunicipalityIds?.Any() ?? false) ||
                            (extendedAuthorizationFilter.GeographicAreas?.ParishIds?.Any() ?? false) ||
                            (extendedAuthorizationFilter.GeographicAreas?.ProvinceIds?.Any() ?? false)
                        )
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
                return allProviders?.Where(p => p.IsActive && p.DataQualityIsApproved).Select(p => p.Id);
            }

            return dataproviderIds;
        }

        /// <summary>
        ///  Handle taxon filtering
        /// </summary>
        /// <param name="taxonIds"></param>
        /// <param name="includeUnderlyingTaxa"></param>
        /// <returns></returns>
        private IEnumerable<int> PopulateTaxonFilter(
            IEnumerable<int> taxonIds, 
            bool includeUnderlyingTaxa,
            IEnumerable<int> taxonListIds,
            FilterBase.TaxonListOp taxonListOperator = FilterBase.TaxonListOp.Merge
            )
        {
            var taxa = GetTaxonFilterIds(taxonIds, includeUnderlyingTaxa);
            if (taxonListIds == null || !taxonListIds.Any()) return taxa;
            var taxonListIdsSet = new HashSet<int>();
            foreach (var taxonListId in taxonListIds)
            {
                if (_taxonManager.TaxonListSetById.TryGetValue(taxonListId, out var taxonListSet))
                {
                    taxonListIdsSet.UnionWith(taxonListSet);
                }
            }

            if (taxonListIdsSet.Count == 0) return taxa;
            if (taxa == null) return taxonListIdsSet;
            if (taxonListOperator == FilterBase.TaxonListOp.Merge)
            {
                taxonListIdsSet.UnionWith(taxa);
                return taxonListIdsSet;
            }
            else if (taxonListOperator == FilterBase.TaxonListOp.Filter)
            {
                HashSet<int> taxaSet = new HashSet<int>();
                taxaSet.UnionWith(taxa);
                taxaSet.IntersectWith(taxonListIdsSet);
                return taxaSet;
            }

            return null;
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
        /// <param name="areaGeometrySearchForced"></param>
        /// <returns></returns>
        private async Task<GeographicFilter> PopulateGeographicalFilterAsync(IEnumerable<AreaFilter> areas, bool areaGeometrySearchForced)
        {
            if (!areas?.Any() ?? true)
            {
                return null;
            }

            var geographicFilter = new GeographicFilter();
            foreach (var areaFilter in areas)
            {
                if (areaGeometrySearchForced)
                {
                    await AddGeometryAsync(geographicFilter, areaFilter.AreaType, areaFilter.FeatureId, true);
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
                        await AddGeometryAsync(geographicFilter, areaFilter.AreaType, areaFilter.FeatureId, false);
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
        /// <param name="usePointAccuracy"></param>
        /// <returns></returns>
        private async Task AddGeometryAsync(GeographicFilter geographicFilter, AreaType areaType, string featureId, bool usePointAccuracy)
        {
            var geometry = await _areaCache.GetGeometryAsync(areaType, featureId);

            if (geometry != null)
            {
                (geographicFilter.GeometryFilter ??= new GeometryFilter
                {
                    MaxDistanceFromPoint = 0,
                    UseDisturbanceRadius = false,
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
        public async Task PrepareFilter(FilterBase filter)
        {
            if (filter.ProtectedObservations)
            {
                filter.ExtendedAuthorizations = await AddAuthorizationAsync();
            }

            filter.DataProviderIds = await PopulateDataProviderFilterAsync(filter.DataProviderIds);
            filter.AreaGeographic = await PopulateGeographicalFilterAsync(filter.Areas, filter.AreaGeometrySearchForced);
            filter.TaxonIds = PopulateTaxonFilter(filter.TaxonIds, filter.IncludeUnderlyingTaxa, filter.TaxonListIds, filter.TaxonListOperator);
        }
    }
}
