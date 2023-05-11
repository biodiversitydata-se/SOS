using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SOS.Observations.Api.HealthChecks
{
    public partial class AzureSearchHealthCheck
    {
        /// <summary>
        /// Client used for querying SOS API using Azure API
        /// </summary>
        public class SosAzureClient
        {
            private readonly HttpClient _client;
            private readonly string _apiUrl;
            private readonly string _subscriptionKey;
            
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="apiUrl"></param>
            /// <param name="subscriptionKey"></param>
            public SosAzureClient(string apiUrl, string subscriptionKey)
            {
                _client = new HttpClient();
                _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                _apiUrl = apiUrl;
                _subscriptionKey = subscriptionKey;
            }

            /// <summary>
            /// Search observations.
            /// </summary>
            /// <param name="searchFilter"></param>
            /// <param name="cultureCode"></param>
            /// <param name="skip"></param>
            /// <param name="take"></param>
            /// <returns></returns>
            /// <exception cref="Exception"></exception>
            public async Task<PagedResultDto<Observation>> SearchObservations(SearchFilterDto searchFilter, string cultureCode, int skip, int take)
            {                
                var response = await _client.PostAsync($"{_apiUrl}Observations/Search?skip={skip}&take={take}&translationCultureCode={cultureCode}", new StringContent(JsonSerializer.Serialize(searchFilter), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var resultString = response.Content.ReadAsStringAsync().Result;
                    var jsonSerializerOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    };
                    var result = JsonSerializer.Deserialize<PagedResultDto<Observation>>(resultString, jsonSerializerOptions);
                    return result;
                }
                else
                {
                    throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
                }
            }

            /// <summary>
            /// Get data providers.
            /// </summary>
            /// <param name="cultureCode"></param>
            /// <returns></returns>
            /// <exception cref="Exception"></exception>
            public async Task<List<DataProviderDto>> GetDataProviders(string cultureCode)
            {
                var response = await _client.GetAsync($"{_apiUrl}DataProviders?cultureCode={cultureCode}");
                if (response.IsSuccessStatusCode)
                {
                    var resultString = response.Content.ReadAsStringAsync().Result;
                    var jsonSerializerOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    };
                    var result = JsonSerializer.Deserialize<List<DataProviderDto>>(resultString, jsonSerializerOptions);
                    return result;
                }
                else
                {
                    throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
                }
            }
        }
    }
}