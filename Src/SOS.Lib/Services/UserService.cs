using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Serilog.Core;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Helpers;
using SOS.Lib.Models.UserService;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Lib.Services
{
    /// <summary>
    /// User service
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly IHttpClientService _httpClientService;
        private readonly UserServiceConfiguration _userServiceConfiguration;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _tokenExpirationBuffer;
        private readonly ILogger<UserService> _logger;
        private JsonWebToken _jsonWebToken;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="authorizationProvider"></param>
        /// <param name="httpClientService"></param>
        /// <param name="userServiceConfiguration"></param>
        /// <param name="logger"></param>
        public UserService(
            IAuthorizationProvider authorizationProvider,
            IHttpClientService httpClientService,
            UserServiceConfiguration userServiceConfiguration,
            ILogger<UserService> logger)
        {
            _authorizationProvider =
                authorizationProvider ?? throw new ArgumentNullException(nameof(authorizationProvider));
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _userServiceConfiguration = userServiceConfiguration ??
                                        throw new ArgumentNullException(nameof(userServiceConfiguration)); 
            _tokenExpirationBuffer = TimeSpan.FromSeconds(userServiceConfiguration.TokenExpirationBufferInSeconds);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <inheritdoc />
        public async Task<UserModel> GetUserAsync()
        {
            try
            {
                var authorizationHeader = _authorizationProvider.GetAuthHeader();
                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    return null;
                }

                var response = await _httpClientService.GetDataAsync<ResponseModel<UserModel>>(
                    new Uri($"{_userServiceConfiguration.UserAdmin2ApiBaseAddress}/Users/Current?artdataFormat=true"),
                    new Dictionary<string, string> { { "Authorization", authorizationHeader } },
                    TimeSpan.FromSeconds(3),
                    5);

                return response?.Success ?? false
                    ? response.Result
                    : throw new Exception(string.Concat(response?.Messages?.Select(m => m.Text)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user");
            }

            return null;
        }        

        /// <summary>
        /// Get user by id.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<UserModel> GetUserByIdAsync(int userId)
        {
            try
            {
                await InvalidateAccessTokenAsync();
                var response = await _httpClientService.GetDataAsync<ResponseModel<UserModel>>(
                    new Uri($"{_userServiceConfiguration.UserAdmin2ApiBaseAddress}/Users/{userId}?artdataFormat=true"),
                    new Dictionary<string, string> { { "Authorization", $"Bearer {_jsonWebToken.EncodedToken}" } },
                    TimeSpan.FromSeconds(3),
                    5);

                return response.Success
                    ? response.Result
                    : throw new Exception(string.Concat(response.Messages?.Select(m => m.Text)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user");
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuthorityModel>> GetUserAuthoritiesAsync(int userId, string authorizationApplicationIdentifier = null, string cultureCode = "sv-SE")
        {
            try
            {
                await InvalidateAccessTokenAsync();
                var response = await _httpClientService.GetDataAsync<ResponseModel<IEnumerable<AuthorityModel>>>(
                    new Uri($"{_userServiceConfiguration.UserAdmin2ApiBaseAddress}/Users/{userId}/permissions?applicationIdentifier={authorizationApplicationIdentifier ?? "artportalen"}&lang={cultureCode}&artdataFormat=true"),
                    new Dictionary<string, string> { { "Authorization", $"Bearer {_jsonWebToken.EncodedToken}" } },
                    TimeSpan.FromSeconds(3),
                    5);

                return response.Success
                    ? response.Result
                    : throw new Exception(string.Concat(response?.Messages?.Select(m => m.Text)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user authorities. UserId={@userId}", userId);
            }

            return null;
        }        

        /// <inheritdoc />
        public async Task<IEnumerable<RoleModel>> GetUserRolesAsync(int userId, string authorizationApplicationIdentifier = null, string cultureCode = "sv-SE")
        {
            try
            {
                await InvalidateAccessTokenAsync();
                var cultureId = CultureCodeHelper.GetCultureId(cultureCode);
                var response = await _httpClientService.GetDataAsync<ResponseModel<IEnumerable<RoleModel>>>(
                    new Uri($"{_userServiceConfiguration.UserAdmin2ApiBaseAddress}/Users/{userId}/roles?applicationIdentifier={authorizationApplicationIdentifier ?? "artportalen"}&localeId={cultureId}&artdataFormat=true"),
                    new Dictionary<string, string> { { "Authorization", $"Bearer {_jsonWebToken.EncodedToken}" } },
                    TimeSpan.FromSeconds(3),
                    5);

                return response?.Success ?? false
                    ? response.Result
                    : throw new Exception(string.Concat(response?.Messages?.Select(m => m.Text) ?? Array.Empty<string>()));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user roles. UserId={@userId}", userId);
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<PersonModel> GetPersonAsync(int personId, string cultureCode = "sv-SE")
        {
            try
            {
                await InvalidateAccessTokenAsync();
                var response = await _httpClientService.GetDataAsync<ResponseModel<PersonModel>>(
                    new Uri($"{_userServiceConfiguration.UserAdmin2ApiBaseAddress}/Persons/{personId}?lang={cultureCode}&artdataFormat=true"),
                    new Dictionary<string, string> { { "Authorization", $"Bearer {_jsonWebToken.EncodedToken}" } },
                    TimeSpan.FromSeconds(3),
                    5);

                return response?.Success ?? false
                    ? response.Result
                    : throw new Exception(string.Concat(response?.Messages?.Select(m => m.Text) ?? Array.Empty<string>()));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get person information. PersonId={@personId}", personId);
            }

            return null;
        }

        public async Task<JsonWebToken> GetClientCredentialsAccessTokenAsync()
        {
            using var client = new HttpClient();
            var requestData = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _userServiceConfiguration.ClientId),
                new KeyValuePair<string, string>("client_secret", _userServiceConfiguration.ClientSecret),
                new KeyValuePair<string, string>("scope", _userServiceConfiguration.Scope)
            ]);

            var response = await client.PostAsync(_userServiceConfiguration.TokenUrl, requestData);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                try
                {
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse);

                    if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.access_token))
                    {
                        _logger.LogError("Token response was null or missing access_token. Response: {JsonResponse}", jsonResponse);
                        throw new Exception("Token response did not contain an access_token.");
                    }

                    var handler = new JsonWebTokenHandler();
                    var token = handler.ReadJsonWebToken(tokenResponse.access_token);

                    return token;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to parse access token from successful response. JsonResponse={JsonResponse}",
                        jsonResponse);
                    throw;
                }
            }
            else
            {
                string secretPreview = string.IsNullOrEmpty(_userServiceConfiguration.ClientSecret)
                    ? "<empty>"
                    : _userServiceConfiguration.ClientSecret.Substring(0, Math.Min(4, _userServiceConfiguration.ClientSecret.Length));

                _logger.LogError("Failed to get access token. " +
                                "StatusCode={StatusCode}, TokenUrl={TokenUrl}, ClientId={ClientId}, Scope={Scope}, ClientSecretPreview={SecretPreview}",
                                response.StatusCode,
                                _userServiceConfiguration.TokenUrl,
                                _userServiceConfiguration.ClientId,
                                _userServiceConfiguration.Scope,
                                secretPreview);

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Response content: {ErrorContent}", errorContent);

                throw new Exception($"Failed to get access token: {response.StatusCode}");
            }
        }

        private async Task InvalidateAccessTokenAsync()
        {
            if (_jsonWebToken == null || TokenExpiringSoon())
            {
                await _semaphoreSlim.WaitAsync();
                try
                {
                    if (_jsonWebToken == null || TokenExpiringSoon())
                    {
                        _jsonWebToken = await GetClientCredentialsAccessTokenAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to renew User API Access Token");
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }
        }

        private bool TokenExpiringSoon()
        {
            if (_jsonWebToken == null) return true;
            return DateTime.UtcNow.Add(_tokenExpirationBuffer) >= _jsonWebToken.ValidTo;           
        }
        
        public class TokenResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
        }
    }
}