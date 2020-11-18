using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            // Map areas to area ids
            if (preparedFilter.Areas?.Any() ?? false)
            {
                var areaIds = preparedFilter.AreaIds?.ToList() ?? new List<int>();
                foreach (var feature in preparedFilter.Areas)
                {
                    var area = await _areaRepository.GetAsync(feature.Type, feature.Feature);
                    if (area != null)
                    {
                        areaIds.Add(area.Id);
                    }
                }

                preparedFilter.AreaIds = areaIds;
            }

            // handle the area ids search
            if (preparedFilter.AreaIds != null && preparedFilter.AreaIds.Any())
            {
                foreach (var areaId in preparedFilter.AreaIds)
                {
                    var area = await _areaRepository.GetAsync(areaId);

                    if (area != null)
                    {
                        //if we already have the info needed for the search we skip polygon searches
                        if (area.AreaType == AreaType.County ||
                            area.AreaType == AreaType.Municipality ||
                            area.AreaType == AreaType.Province)
                        {
                            if (area.AreaType == AreaType.County)
                            {
                                if (preparedFilter.CountyIds == null)
                                {
                                    preparedFilter.CountyIds = new List<int>();
                                }

                                var list = preparedFilter.CountyIds.ToList();
                                list.Add(area.Id);
                                preparedFilter.CountyIds = list;
                            }
                            else if (area.AreaType == AreaType.Municipality)
                            {
                                if (preparedFilter.MunicipalityIds == null)
                                {
                                    preparedFilter.MunicipalityIds = new List<int>();
                                }

                                var list = preparedFilter.MunicipalityIds.ToList();
                                list.Add(area.Id);
                                preparedFilter.MunicipalityIds = list;
                            }
                            else if (area.AreaType == AreaType.Province)
                            {
                                if (preparedFilter.ProvinceIds == null)
                                {
                                    preparedFilter.ProvinceIds = new List<int>();
                                }

                                var list = preparedFilter.ProvinceIds.ToList();
                                list.Add(area.Id);
                                preparedFilter.ProvinceIds = list;
                            }
                        }
                        else // we need to use the geometry filter
                        {
                            var geometry = await _areaRepository.GetGeometryAsync(areaId);

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
