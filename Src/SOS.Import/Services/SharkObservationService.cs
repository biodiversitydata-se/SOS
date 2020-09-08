using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.Verbatim.Shark;
using SOS.Lib.Services.Interfaces;

namespace SOS.Import.Services
{
    public class SharkObservationService : ISharkObservationService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly ILogger<SharkObservationService> _logger;
        private readonly SharkServiceConfiguration _sharkServiceConfiguration;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="sharkServiceConfiguration"></param>
        public SharkObservationService(
            IHttpClientService httpClientService,
            SharkServiceConfiguration sharkServiceConfiguration,
            ILogger<SharkObservationService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _sharkServiceConfiguration = sharkServiceConfiguration ??
                                         throw new ArgumentNullException(nameof(sharkServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<SharkJsonFile> GetAsync(string dataSetName)
        {
            return await GetDataAsync(new Uri(
                $"{_sharkServiceConfiguration.BaseAddress}/datasets/{dataSetName}/data.json?page=1&per_page=1"));
        }

        /// <inheritdoc />
        public async Task<SharkJsonFile> GetDataSetsAsync()
        {
            return await GetDataAsync(new Uri($"{_sharkServiceConfiguration.BaseAddress}/datasets/table.json"));
        }

        /// <summary>
        ///     Get json file from Shark
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private async Task<SharkJsonFile> GetDataAsync(Uri uri)
        {
            try
            {
                await using var fileStream = await _httpClientService.GetFileStreamAsync(uri);

                if (!fileStream?.CanRead ?? true)
                {
                    _logger.LogError($"Failed to get data from Shark ({uri.PathAndQuery})");
                    return null;
                }

                using var streamReader = new StreamReader(fileStream, Encoding.UTF7);
                var json = await streamReader.ReadToEndAsync();
                fileStream.Close();

                return JsonConvert.DeserializeObject<SharkJsonFile>(json);
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to get data from Shark ({uri.PathAndQuery})", e);
                return null;
            }
        }
    }
}