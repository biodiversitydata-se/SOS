using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
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
            if (TokenHelper.IsUserAdmin2Token(_authorizationProvider.GetAuthHeader(), _userServiceConfiguration.IdentityProvider.Authority, _logger))
                return await GetUserFromUserAdmin2Async();
            else
                return await GetUserFromUserAdmin1Async();
        }

        private async Task<UserModel> GetUserFromUserAdmin1Async()
        {
            try
            {
                var authorizationHeader = _authorizationProvider.GetAuthHeader();
                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    return null;
                }

                var response = await _httpClientService.GetDataAsync<ResponseModel<UserModel>>(
                    new Uri($"{_userServiceConfiguration.BaseAddress}/User/Current"),
                   new Dictionary<string, string> { { "Authorization", authorizationHeader } });

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

        private async Task<UserModel> GetUserFromUserAdmin2Async()
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
                    new Dictionary<string, string> { { "Authorization", authorizationHeader } });

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
            if (TokenHelper.IsUserAdmin2Token(_authorizationProvider.GetAuthHeader(), _userServiceConfiguration.IdentityProvider.Authority, _logger))
                return await GetUserByIdFromUserAdmin2Async(userId);
            else
                return await GetUserByIdFromUserAdmin1Async(userId);
        }

        private async Task<UserModel> GetUserByIdFromUserAdmin1Async(int userId)
        {
            try
            {
                var response = await _httpClientService.GetDataAsync<ResponseModel<UserModel>>(
                    new Uri($"{_userServiceConfiguration.BaseAddress}/User/{userId}"));

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

        private async Task<UserModel> GetUserByIdFromUserAdmin2Async(int userId)
        {
            try
            {
                await InvalidateAccessTokenAsync();
                var response = await _httpClientService.GetDataAsync<ResponseModel<UserModel>>(
                    new Uri($"{_userServiceConfiguration.UserAdmin2ApiBaseAddress}/Users/{userId}?artdataFormat=true"),
                    new Dictionary<string, string> { { "Authorization", $"Bearer {_jsonWebToken.EncodedToken}" } });

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
            if (TokenHelper.IsUserAdmin2Token(_authorizationProvider.GetAuthHeader(), _userServiceConfiguration.IdentityProvider.Authority, _logger))
                return await GetUserAuthoritiesFromUserAdmin2Async(userId);
            else
                return await GetUserAuthoritiesFromUserAdmin1Async(userId, authorizationApplicationIdentifier, cultureCode);
        }

        private async Task<IEnumerable<AuthorityModel>> GetUserAuthoritiesFromUserAdmin1Async(int userId, string authorizationApplicationIdentifier = null, string cultureCode = "sv-SE")
        {
            try
            {
                var response = await _httpClientService.GetDataAsync<ResponseModel<IEnumerable<AuthorityModel>>>(
                    new Uri($"{_userServiceConfiguration.BaseAddress}/User/{userId}/authorities?applicationIdentifier={authorizationApplicationIdentifier ?? "artportalen"}&lang={cultureCode}"));

                return response.Success
                    ? response.Result
                    : throw new Exception(string.Concat(response?.Messages?.Select(m => m.Text)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user authorities");
            }

            return null;
        }

        private async Task<IEnumerable<AuthorityModel>> GetUserAuthoritiesFromUserAdmin2Async(int userId, string authorizationApplicationIdentifier = null, string cultureCode = "sv-SE")
        {
            try
            {
                await InvalidateAccessTokenAsync();
                var response = await _httpClientService.GetDataAsync<ResponseModel<IEnumerable<AuthorityModel>>>(
                    new Uri($"{_userServiceConfiguration.UserAdmin2ApiBaseAddress}/Users/{userId}/permissions?applicationIdentifier={authorizationApplicationIdentifier ?? "artportalen"}&lang={cultureCode}&artdataFormat=true"),
                    new Dictionary<string, string> { { "Authorization", $"Bearer {_jsonWebToken.EncodedToken}" } });

                return response.Success
                    ? response.Result
                    : throw new Exception(string.Concat(response?.Messages?.Select(m => m.Text)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user authorities");
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RoleModel>> GetUserRolesAsync(int userId, string authorizationApplicationIdentifier = null, string cultureCode = "sv-SE")
        {
            if (TokenHelper.IsUserAdmin2Token(_authorizationProvider.GetAuthHeader(), _userServiceConfiguration.IdentityProvider.Authority, _logger))
                return await GetUserRolesFromUserAdmin2Async(userId, authorizationApplicationIdentifier, cultureCode);
            else
                return await GetUserRolesFromUserAdmin1Async(userId, authorizationApplicationIdentifier, cultureCode);
        }

        private async Task<IEnumerable<RoleModel>> GetUserRolesFromUserAdmin1Async(int userId, string authorizationApplicationIdentifier = null, string cultureCode = "sv-SE")
        {
            try
            {
                var cultureId = CultureCodeHelper.GetCultureId(cultureCode);
                var response = await _httpClientService.GetDataAsync<ResponseModel<IEnumerable<RoleModel>>>(
                    new Uri($"{_userServiceConfiguration.BaseAddress}/User/{userId}/roles?applicationIdentifier={authorizationApplicationIdentifier ?? "artportalen"}&localeId={cultureId}"));

                return response?.Success ?? false
                    ? response.Result
                    : throw new Exception(string.Concat(response?.Messages?.Select(m => m.Text) ?? Array.Empty<string>()));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user roles");
            }

            return null;
        }

        private async Task<IEnumerable<RoleModel>> GetUserRolesFromUserAdmin2Async(int userId, string authorizationApplicationIdentifier = null, string cultureCode = "sv-SE")
        {
            try
            {
                await InvalidateAccessTokenAsync();
                var cultureId = CultureCodeHelper.GetCultureId(cultureCode);
                var response = await _httpClientService.GetDataAsync<ResponseModel<IEnumerable<RoleModel>>>(
                    new Uri($"{_userServiceConfiguration.UserAdmin2ApiBaseAddress}/Users/{userId}/roles?applicationIdentifier={authorizationApplicationIdentifier ?? "artportalen"}&localeId={cultureId}&artdataFormat=true"),
                    new Dictionary<string, string> {{ "Authorization", $"Bearer {_jsonWebToken.EncodedToken}" }});

                return response?.Success ?? false
                    ? response.Result
                    : throw new Exception(string.Concat(response?.Messages?.Select(m => m.Text) ?? Array.Empty<string>()));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user roles");
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<PersonModel> GetPersonAsync(int personId, string cultureCode = "sv-SE")
        {
            if (TokenHelper.IsUserAdmin2Token(_authorizationProvider.GetAuthHeader(), _userServiceConfiguration.IdentityProvider.Authority, _logger))
                return await GetPersonFromUserAdmin2Async(personId, cultureCode);
            else
                return await GetPersonFromUserAdmin1Async(personId, cultureCode);
        }

        private async Task<PersonModel> GetPersonFromUserAdmin1Async(int personId, string cultureCode = "sv-SE")
        {
            try
            {
                var response = await _httpClientService.GetDataAsync<ResponseModel<PersonModel>>(
                    new Uri($"{_userServiceConfiguration.BaseAddress}/Person/{personId}?lang={cultureCode}"));

                return response?.Success ?? false
                    ? response.Result
                    : throw new Exception(string.Concat(response?.Messages?.Select(m => m.Text) ?? Array.Empty<string>()));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get person information");
            }

            return null;
        }

        private async Task<PersonModel> GetPersonFromUserAdmin2Async(int personId, string cultureCode = "sv-SE")
        {
            try
            {
                await InvalidateAccessTokenAsync();
                var response = await _httpClientService.GetDataAsync<ResponseModel<PersonModel>>(
                    new Uri($"{_userServiceConfiguration.UserAdmin2ApiBaseAddress}/Persons/{personId}?lang={cultureCode}&artdataFormat=true"),
                    new Dictionary<string, string> { { "Authorization", $"Bearer {_jsonWebToken.EncodedToken}" } });

                return response?.Success ?? false
                    ? response.Result
                    : throw new Exception(string.Concat(response?.Messages?.Select(m => m.Text) ?? Array.Empty<string>()));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get person information");
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
                TokenResponse tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse);
                var handler = new JsonWebTokenHandler();
                JsonWebToken token = handler.ReadJsonWebToken(tokenResponse.access_token);   
                return token;
            }
            else
            {
                throw new Exception("Failed to get access token: " + response.StatusCode);
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