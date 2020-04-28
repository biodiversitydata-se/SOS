using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.Verbatim.Shark;

namespace SOS.Import.Services
{
    public class SharkObservationService : ISharkObservationService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly SharkServiceConfiguration _sharkServiceConfiguration;
        private readonly ILogger<SharkObservationService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="sharkServiceConfiguration"></param>
        public SharkObservationService(
            IHttpClientService httpClientService,
            SharkServiceConfiguration sharkServiceConfiguration,
            ILogger<SharkObservationService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _sharkServiceConfiguration = sharkServiceConfiguration ?? throw new ArgumentNullException(nameof(sharkServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get json file from Shark
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private async Task<SharkJsonFile> GetDataAsync(Uri uri)
        {
            try
            {
                var fileData = await _httpClientService.ReadFileDataAsync(uri);

                if (fileData == null)
                {
                    _logger.LogError($"Failed to get data from Shark ({uri.PathAndQuery})");
                    return null;
                }

                var json = Encoding.UTF8.GetString(Encoding.Convert(Encoding.UTF7, Encoding.UTF8, fileData.ToArray()));
                return JsonConvert.DeserializeObject<SharkJsonFile>(json);
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to get data from Shark ({uri.PathAndQuery})", e);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<SharkJsonFile> GetAsync(string dataSetName)
        {
            return await GetDataAsync(new Uri($"{ _sharkServiceConfiguration.WebServiceAddress }/{ dataSetName }/data.json?page=1&per_page=1"));
        }

        /// <inheritdoc />
        public async Task<SharkJsonFile> GetDataSetsAsync()
        {
            return await GetDataAsync(new Uri($"{ _sharkServiceConfiguration.WebServiceAddress }/table.json"));
        }
    }
}
