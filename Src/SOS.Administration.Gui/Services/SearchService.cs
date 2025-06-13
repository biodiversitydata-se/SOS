using SOS.Administration.Gui.Clients;
using SOS.Administration.Gui.Dtos;
using System.Net.Http;

namespace SOS.Administration.Gui.Services
{
    public class SearchService : ISearchService
    {
        private readonly HttpClient _client;
        private readonly string _apiUrl;
        private readonly SosObservationsApiClient _sosObservationsApiClient;

        public SearchService()
        {
            _client = new HttpClient();
            _apiUrl = Settings.ApiTestConfiguration.ApiUrl;
        }

        public SearchService(SosObservationsApiClient sosObservationsApiClient)
        {
            _sosObservationsApiClient = sosObservationsApiClient;
        }

        public async Task<PagedResult<SOSObservation>> SearchSOS(SearchFilterDto searchFilter, int take, int skip)
        {
            var result = await _sosObservationsApiClient.SearchSOS(searchFilter, take, skip);
            return result;
        }
        public async Task<PagedResult<TaxonAggregationItemDto>> SearchSOSTaxonAggregation(SearchFilterDto searchFilter, int take, int skip, double? bboxleft = null, double? bboxtop = null, double? bboxright = null, double? bboxbottom = null)
        {
            var result = await _sosObservationsApiClient.SearchSOSTaxonAggregation(searchFilter, take, skip, bboxleft, bboxtop, bboxright, bboxbottom);
            return result;
        }
        public async Task<GeoGridResultDto> SearchSOSGeoAggregation(SearchFilterDto searchFilter)
        {
            var result = await _sosObservationsApiClient.SearchSOSGeoAggregation(searchFilter);
            return result;
        }

        public async Task<string> GetHealthStatus()
        {
            var result = await _sosObservationsApiClient.GetHealthStatus();
            return result;
        }
    }
}
