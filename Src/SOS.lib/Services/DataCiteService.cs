using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Models.DataCite;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Services
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class DataCiteService : IDataCiteService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly DataCiteServiceConfiguration _dataCiteServiceConfiguration;
        private readonly ILogger<DataCiteService> _logger;

        private Dictionary<string, string> GetBasciAuthenticationHeader()
        {
            return new Dictionary<string, string>
            {
                { "userName", _dataCiteServiceConfiguration.UserName },
                { "password", _dataCiteServiceConfiguration.Password }
            };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="dataCiteServiceConfiguration"></param>
        /// <param name="logger"></param>
        public DataCiteService(
            IHttpClientService httpClientService,
            DataCiteServiceConfiguration dataCiteServiceConfiguration,
            ILogger<DataCiteService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _dataCiteServiceConfiguration = dataCiteServiceConfiguration ??
                                            throw new ArgumentNullException(nameof(dataCiteServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<DOIMetadata> CreateDoiDraftAsync(DOIMetadata data)
        {
            if (data == null)
            {
                return null;
            }

            if (data.Attributes == null)
            {
                data.Attributes = new DOIAttributes();
            }

            // Create a DOI 
            data.Attributes.DOI = $"{_dataCiteServiceConfiguration.DoiPrefix}/{ (string.IsNullOrEmpty(data.Attributes.Suffix) ? Guid.NewGuid().ToString() : data.Attributes.Suffix) }";

            var doiRequest = new DOI<DOIMetadata>
            {
                Data = data
            };

            // Validate creators, title, publisher, publicationYear, resourceTypeGeneral
            var doi = await _httpClientService.PostDataAsync<DOI<DOIMetadata>>(
                new Uri($"{_dataCiteServiceConfiguration.BaseAddress}/dois"), doiRequest, GetBasciAuthenticationHeader(), "application/vnd.api+json");

            return doi?.Data;
        }

        /// <inheritdoc />
        public async Task<DOIMetadata> GetMetadataAsync(string prefix, string suffix)
        {
            try
            {
                return (await _httpClientService.GetDataAsync<DOI<DOIMetadata>>(
                    new Uri($"{ _dataCiteServiceConfiguration.BaseAddress }/dois/{ prefix }/{ suffix }"))).Data;
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get user roles", e);
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<bool> PublishDoiAsync(DOIMetadata data)
        {
            if (data?.Attributes == null)
            {
                return false;
            }

            data.Attributes.Event = "publish";
            data.Attributes.Url = $"{_dataCiteServiceConfiguration.DoiUrl}/{data.Id}";
            data.Attributes.Identifiers = new[]
            {
                new DOIIdentifier
                {
                    Identifier = $"https://doi.org/{data.Id}",
                    IdentifierType = "DOI"
                }
            };
            var doiRequest = new DOI<DOIMetadata>
            {
                Data = data
            };

            var response = await _httpClientService.PutDataAsync<dynamic>(
                new Uri($"{_dataCiteServiceConfiguration.BaseAddress}/dois/{data.Id}"), doiRequest, GetBasciAuthenticationHeader(), "application/vnd.api+json");

            return response != null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DOIMetadata>> SearchMetadataAsync(string searchFor)
        {
            try
            {
               var response = await _httpClientService.GetDataAsync<DOI<IEnumerable<DOIMetadata>>>(
                    new Uri($"{ _dataCiteServiceConfiguration.BaseAddress }/dois?client-id={_dataCiteServiceConfiguration.ClientId}&query=+{searchFor.Replace(" ", "+")}&sort=created:desc"));

               return response?.Data;
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to search DOI's", e);
            }

            return null;
        }
    }
}