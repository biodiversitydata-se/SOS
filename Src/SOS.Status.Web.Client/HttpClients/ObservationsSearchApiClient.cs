using SOS.Status.Web.Client.Abstractions;
using SOS.Status.Web.Client.Dtos.SosObsApi;
using SOS.Status.Web.Client.JsonConverters;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Status.Web.Client.HttpClients;

public class ObservationsSearchApiClient : IObservationSearchService
{
    private readonly HttpClient _http;

    private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(),
            new GeoJsonConverter()
        }
    };

    public ObservationsSearchApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<PagedResultDto<Observation>> SearchObservations(
        SearchFilterInternalDto filter,
        int skip = 0,
        int take = 100,
        string sortBy = "",
        string sortOrder = "Asc",
        bool validateSearchFilter = false,
        string translationCultureCode = "sv-SE",
        bool sensitiveObservations = false)
    {
        var url = $"api/observations/search?" +
            $"skip={skip}" +
            $"&take={take}" +
            $"&sortBy={sortBy}" +
            $"&sortOrder={sortOrder}" +
            $"&validateSearchFilter={validateSearchFilter.ToString().ToLower()}" +
            $"&translationCultureCode={translationCultureCode}" +
            $"&sensitiveObservations={sensitiveObservations.ToString().ToLower()}";

        try
        {
            var json = JsonSerializer.Serialize(filter, _jsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(url, content);


            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(_jsonSerializerOptions);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}, {error}");
            }
            //else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            //{
            //    var error = await response.Content.ReadAsStringAsync();
            //    return $"Error: {error}";
            //}
            //else
            //{
            //    return $"Unknown error: {response.StatusCode}";
            //}
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception: {ex.Message}");
        }
    }
}