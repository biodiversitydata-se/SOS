using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Models.ApiManagement;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Services
{
    /// <inheritdoc />
    public class ApiManagementUserService : IApiManagementUserService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly string _requestToken;
        private readonly ApiManagementServiceConfiguration _configuration;
        private readonly ILogger<ApiManagementUserService> _logger;

        private class ApiProperties
        {
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        private class ApiUser
        {
            public ApiProperties Properties { get; set; }
        }

        /// <summary>
        /// Get request headers
        /// </summary>
        private Dictionary<string, string> RequestHeaders => new Dictionary<string, string>
        {
            {"Authorization", _requestToken}
        };

        private string CreateToken()
        {
            const string id = "integration";
            var expiry = DateTime.UtcNow.AddHours(12);
            using var encoder = new HMACSHA512(Encoding.UTF8.GetBytes(_configuration.SigningKey));

            var dataToSign = $"{id}\n{expiry.ToString("O", CultureInfo.InvariantCulture)}";
            var hash = encoder.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
            var signature = Convert.ToBase64String(hash);
            var encodedToken = $"SharedAccessSignature uid={id}&ex={expiry:o}&sn={signature}";
            return encodedToken;
        }

        /// <summary>
        /// Get service request path
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private string GetRequestPath(string query) => $"{_configuration.BaseAddress}/subscriptions/{_configuration.SubscriptionId}/resourceGroups/{_configuration.ResourceGroup}/providers/Microsoft.ApiManagement/service/{_configuration.Service}/{query}?api-version=2020-12-01";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="apiManagementServiceConfiguration"></param>
        /// <param name="logger"></param>
        public ApiManagementUserService(IHttpClientService httpClientService,
            ApiManagementServiceConfiguration apiManagementServiceConfiguration,
            ILogger<ApiManagementUserService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _configuration = apiManagementServiceConfiguration ??
                             throw new ArgumentNullException(
                                 nameof(apiManagementServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _requestToken = CreateToken();
        }

        /// <inheritdoc />
        public async Task<ApiManagementUser> GetUserAsync(string id)
        {
            try
            {
                var response = await _httpClientService.GetDataAsync<ApiUser>(new Uri(GetRequestPath($"users/{id}")), RequestHeaders);

                if (response?.Properties == null)
                {
                    throw new Exception("Service request failed");
                }

                return new ApiManagementUser
                {
                    Email = response.Properties.Email,
                    FirstName = response.Properties.FirstName,
                    LastName = response.Properties.LastName
                };
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get user from api management", e);
                return null;
            }
        }
    }
}