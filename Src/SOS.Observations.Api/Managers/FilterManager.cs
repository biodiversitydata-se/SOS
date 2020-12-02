using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    /// Filter manager.
    /// </summary>
    public class FilterManager : IFilterManager
    {
        private const int BiotaTaxonId = 0;
        private readonly ITaxonManager _taxonManager;
        private readonly IAreaCache _areaCache;

        public FilterManager(ITaxonManager taxonManager, IAreaCache areaCache)
        {
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
            _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
        }

        /// <summary>
        /// Creates a cloned filter with additional information if necessary. E.g. adding underlying taxon ids.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>A cloned filter with additional information.</returns>
        public async Task<SearchFilter> PrepareFilter(SearchFilter filter)
        {
            var preparedFilter = filter.Clone();

            if (preparedFilter.IncludeUnderlyingTaxa && preparedFilter.TaxonIds != null &&
                preparedFilter.TaxonIds.Any())
            {
                if (preparedFilter.TaxonIds.Contains(BiotaTaxonId)) // If Biota, then clear taxon filter
                {
                    preparedFilter.TaxonIds = new List<int>();
                }
                else
                {
                    preparedFilter.TaxonIds =
                        _taxonManager.TaxonTree.GetUnderlyingTaxonIds(preparedFilter.TaxonIds, true);
                }
            }

            // handle the area ids search
            if (preparedFilter.Areas?.Any() ?? false)
            {
                foreach (var areaFilter in preparedFilter.Areas)
                {
                    switch (areaFilter.Type)
                    {
                        case AreaType.County:
                            (preparedFilter.CountyIds ??= new List<string>()).Add(areaFilter.FeatureId);
                            break;
                        case AreaType.Municipality:
                            (preparedFilter.MunicipalityIds ??= new List<string>()).Add(areaFilter.FeatureId);
                            break;
                        case AreaType.Province:
                            (preparedFilter.ProvinceIds ??= new List<string>()).Add(areaFilter.FeatureId);
                            break;
                        case AreaType.Parish:
                            (preparedFilter.ParishIds ??= new List<string>()).Add(areaFilter.FeatureId);
                            break;
                        case AreaType.BirdValidationArea:
                            (preparedFilter.BirdValidationAreaIds ??= new List<string>()).Add(areaFilter.FeatureId);
                            break;
                        default:
                            var geometry = await _areaCache.GetGeometryAsync(areaFilter.Type, areaFilter.FeatureId);

                            if (geometry != null)
                            {
                                (preparedFilter.GeometryFilter ??= new GeometryFilter
                                {
                                    MaxDistanceFromPoint = 0
                                }).Geometries.Add(geometry);
                            }

                            break;
                    }
                }
            }

            return preparedFilter;
        }
    }
}
