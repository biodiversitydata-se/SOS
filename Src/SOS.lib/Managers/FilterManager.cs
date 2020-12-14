using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;
using SOS.Lib.Managers.Interfaces;

namespace SOS.Lib.Managers
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
        public async Task PrepareFilter(FilterBase filter)
        {
            if (filter.IncludeUnderlyingTaxa && filter.TaxonIds != null &&
                filter.TaxonIds.Any())
            {
                if (filter.TaxonIds.Contains(BiotaTaxonId)) // If Biota, then clear taxon filter
                {
                    filter.TaxonIds = new List<int>();
                }
                else
                {
                    filter.TaxonIds =
                        _taxonManager.TaxonTree.GetUnderlyingTaxonIds(filter.TaxonIds, true);
                }
            }

            // handle the area ids search
            if (filter.Areas?.Any() ?? false)
            {
                foreach (var areaFilter in filter.Areas)
                {
                    switch (areaFilter.AreaType)
                    {
                        case AreaType.County:
                            (filter.CountyIds ??= new List<string>()).Add(areaFilter.FeatureId);
                            break;
                        case AreaType.Municipality:
                            (filter.MunicipalityIds ??= new List<string>()).Add(areaFilter.FeatureId);
                            break;
                        case AreaType.Province:
                            (filter.ProvinceIds ??= new List<string>()).Add(areaFilter.FeatureId);
                            break;
                        case AreaType.Parish:
                            (filter.ParishIds ??= new List<string>()).Add(areaFilter.FeatureId);
                            break;
                        case AreaType.BirdValidationArea:
                            (filter.BirdValidationAreaIds ??= new List<string>()).Add(areaFilter.FeatureId);
                            break;
                        default:
                            var geometry = await _areaCache.GetGeometryAsync(areaFilter.AreaType, areaFilter.FeatureId);

                            if (geometry != null)
                            {
                                (filter.GeometryFilter ??= new GeometryFilter
                                {
                                    MaxDistanceFromPoint = 0
                                }).Geometries.Add(geometry);
                            }

                            break;
                    }
                }
            }
        }
    }
}
