using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Services.Interfaces;
using System.Text.Json;

namespace SOS.Harvest.Services
{
    public class iNaturalistApiObservationService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly ILogger<iNaturalistApiObservationService> _logger;
        private const int PageSize = 100;
        private const int NumberOfRetries = 5;
        private const int MaxPage = 100;
        private static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = null,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new StringConverter()
            }
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="logger"></param>
        public iNaturalistApiObservationService(
            IHttpClientService httpClientService,
            ILogger<iNaturalistApiObservationService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SOS.Lib.Models.Verbatim.INaturalist.Service.ObservationsResponse> GetAsync(
            DateTime? updatedFromDate,            
            int? idAbove,
            int page,
            string orderBy = "id")
        {
            return await GetAsync(updatedFromDate, idAbove, page, 1, orderBy);
        }

        private async Task<SOS.Lib.Models.Verbatim.INaturalist.Service.ObservationsResponse> GetAsync(
            DateTime? updatedFromDate, 
            int? idAbove,
            int page,
            byte attempt,
            string orderBy = "id")
        {
            try
            {
                if (idAbove == null)
                {
                    idAbove = 0;
                }
                if (updatedFromDate == null)
                {
                    updatedFromDate = new DateTime(1900, 1, 1);
                }
                var observations = new List<DwcObservationVerbatim>();                
                var observationsResult = await _httpClientService.GetFileStreamAsync(
                    new Uri($"https://api.inaturalist.org/v1/observations?place_id=7599&order=asc&order_by={orderBy}" +
                            $"&updated_since={updatedFromDate}" +
                            $"&id_above={idAbove}" +
                            $"&page={page}" +
                            $"&per_page={PageSize}"),
                    new Dictionary<string, string>(
                        [
                            new KeyValuePair<string, string>("Accept", "application/json"),
                        ])
                    );

                var result = await JsonSerializer.DeserializeAsync<SOS.Lib.Models.Verbatim.INaturalist.Service.ObservationsResponse>(
                    observationsResult, _jsonSerializerOptions);
                return result;
            }
            catch (Exception e)
            {
                if (attempt < 5)
                {
                    _logger.LogWarning($"Failed to get data from iNaturalist API (updatedFromDate={updatedFromDate}, idAbove={idAbove}), attempt: {attempt}", e);
                    await Task.Delay(attempt * 5000);
                    return await GetAsync(updatedFromDate, idAbove, page, ++attempt);
                }

                _logger.LogError($"Failed to get data from iNaturalist API (updatedFromDate={updatedFromDate}, idAbove={idAbove})", e);
                throw;
            }
        }      

        public async IAsyncEnumerable<(ICollection<SOS.Lib.Models.Verbatim.INaturalist.Service.iNaturalistVerbatimObservation> Observations, int TotalCount)> GetByIterationAsync(
            int idAbove,
            int sleepSeconds = 1)
        {
            var endOfChunk = false;
            while (!endOfChunk)
            {
                SOS.Lib.Models.Verbatim.INaturalist.Service.ObservationsResponse? results = null;
                
                for (int i = 0; i < NumberOfRetries; i++)
                {
                    try
                    {
                        results = await GetAsync(null, idAbove, 1);
                        break;
                    }
                    catch (Exception) 
                    {
                        await Task.Delay(sleepSeconds * (i+1) * 2 * 1000); // incremental delay when error.
                    }
                }
                
                if (results == null) throw new Exception("Failed to get data from iNaturalist API");
                if (results.Total_results.GetValueOrDefault(0) == 0)
                {
                    endOfChunk = true;
                    yield break;
                }

                if (results.Results.Count < PageSize)
                {
                    endOfChunk = true;
                    yield return (results.Results, results.Total_results.GetValueOrDefault(0));
                    yield break;
                }

                yield return (results.Results, results.Total_results.GetValueOrDefault(0));
                idAbove = results.Results.Last().ObservationId!;
                await Task.Delay(sleepSeconds * 1000);
            }
        }    

        public async IAsyncEnumerable<(ICollection<SOS.Lib.Models.Verbatim.INaturalist.Service.iNaturalistVerbatimObservation> Observations, int TotalCount)> GetByIterationAsync(
            DateTime updatedFromDate,
            int sleepSeconds = 1)
        {
            var endOfChunk = false;
            var currentPage = 1;
            while (!endOfChunk)
            {
                SOS.Lib.Models.Verbatim.INaturalist.Service.ObservationsResponse? results = null;
                for (int i = 0; i < NumberOfRetries; i++)
                {
                    try
                    {
                        results = await GetAsync(updatedFromDate, null, currentPage, "updated_at");
                        break;
                    }
                    catch (Exception)
                    {
                        await Task.Delay(sleepSeconds * (i + 1) * 2 * 1000); // incremental delay when error.
                    }
                }

                if (results == null) throw new Exception("Failed to get data from iNaturalist API");
                if (results.Total_results.GetValueOrDefault(0) == 0)
                {
                    endOfChunk = true;
                    yield break;
                }

                if (results.Results.Count < PageSize)
                {
                    endOfChunk = true;
                    yield return (results.Results, results.Total_results.GetValueOrDefault(0));
                    yield break;
                }

                yield return (results.Results, results.Total_results.GetValueOrDefault(0));
                currentPage++;
                if (currentPage > MaxPage) // iNaturalist API doesn't work with deep pagination?
                {
                    updatedFromDate = results.Results.Last().Updated_at!.Value.DateTime;
                    currentPage = 1;
                }

                await Task.Delay(sleepSeconds * 1000);
            }
        }
    }
}