using SOS.Lib.Models.Search;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using System.Threading.Tasks;

namespace SOS.Administration.Gui.Services
{
    public interface ISearchService
    {
        Task<PagedResult<SOSObservation>> SearchSOS(SearchFilterDto searchFilter, int take, int skip);
        Task<GeoGridResultDto> SearchSOSGeoAggregation(SearchFilterDto searchFilter);
        Task<PagedResult<TaxonAggregationItemDto>> SearchSOSTaxonAggregation(SearchFilterDto searchFilter, int take, int skip, double? bboxleft = null, double? bboxtop = null, double? bboxright = null, double? bboxbottom = null);
    }
}