using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Resource.Interfaces;
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
        private readonly IAreaRepository _areaRepository;

        public FilterManager(ITaxonManager taxonManager, IAreaRepository areaRepository)
        {
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
            _areaRepository = areaRepository ?? throw new ArgumentNullException(nameof(areaRepository));
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
                    var area = await _areaRepository.GetAsync(areaFilter.Type, areaFilter.FeatureId);

                    if (area != null)
                    {
                        //if we already have the info needed for the search we skip polygon searches
                        if (area.AreaType == AreaType.County ||
                            area.AreaType == AreaType.Municipality ||
                            area.AreaType == AreaType.Province ||
                            area.AreaType == AreaType.Parish)
                        {
                            if (area.AreaType == AreaType.County)
                            {
                                if (preparedFilter.CountyIds == null)
                                {
                                    preparedFilter.CountyIds = new List<string>();
                                }

                                var list = preparedFilter.CountyIds.ToList();
                                list.Add(area.FeatureId);
                                preparedFilter.CountyIds = list;
                            }
                            else if (area.AreaType == AreaType.Municipality)
                            {
                                if (preparedFilter.MunicipalityIds == null)
                                {
                                    preparedFilter.MunicipalityIds = new List<string>();
                                }

                                var list = preparedFilter.MunicipalityIds.ToList();
                                list.Add(area.FeatureId);
                                preparedFilter.MunicipalityIds = list;
                            }
                            else if (area.AreaType == AreaType.Province)
                            {
                                if (preparedFilter.ProvinceIds == null)
                                {
                                    preparedFilter.ProvinceIds = new List<string>();
                                }

                                var list = preparedFilter.ProvinceIds.ToList();
                                list.Add(area.FeatureId);
                                preparedFilter.ProvinceIds = list;
                            }
                            else if (area.AreaType == AreaType.Parish)
                            {
                                if (preparedFilter.ParishIds == null)
                                {
                                    preparedFilter.ParishIds = new List<string>();
                                }

                                var list = preparedFilter.ParishIds.ToList();
                                list.Add(area.FeatureId);
                                preparedFilter.ParishIds = list;
                            }
                        }
                        else // we need to use the geometry filter
                        {
                            var geometry = await _areaRepository.GetGeometryAsync(areaFilter.Type, areaFilter.FeatureId);

                            if (preparedFilter.GeometryFilter == null)
                            {
                                preparedFilter.GeometryFilter = new GeometryFilter();
                                preparedFilter.GeometryFilter.MaxDistanceFromPoint = 0;
                            }

                            preparedFilter.GeometryFilter.Geometries.Add(geometry);
                        }
                    }
                }
            }

            return preparedFilter;
        }
    }
}
