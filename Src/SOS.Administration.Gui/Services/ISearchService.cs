using SOS.Administration.Gui.Dtos;
using System.Threading.Tasks;
using SOS.Lib.Models.Search;

namespace SOS.Administration.Gui.Services
{
    public interface ISearchService
    {
        Task<PagedResult<SOSObservation>> SearchSOS(SearchFilterDto searchFilter, int take, int skip);
        Task<GeoGridResultDto> SearchSOSGeoAggregation(SearchFilterDto searchFilter);
        Task<PagedResult<TaxonAggregationItemDto>> SearchSOSTaxonAggregation(SearchFilterDto searchFilter, int take, int skip, double? bboxleft = null, double? bboxtop = null, double? bboxright = null, double? bboxbottom = null);

        Task<string> GetHealthStatus();
    }
}