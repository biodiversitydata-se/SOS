using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Models.UserService;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Services.Interfaces;

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
        private readonly ILogger<UserService> _logger;

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
                    new Uri($"{ _userServiceConfiguration.BaseAddress }/User/Current"),
                   new Dictionary<string, string> { { "Authorization", authorizationHeader } });

                return response?.Success ?? false
                    ? response.Result
                    : throw new Exception(string.Concat(response.Messages?.Select(m => m.Text)));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get user", e);
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
                var response = await _httpClientService.GetDataAsync<ResponseModel<UserModel>>(
                    new Uri($"{ _userServiceConfiguration.BaseAddress }/User/{userId}"));

                return response.Success
                    ? response.Result
                    : throw new Exception(string.Concat(response.Messages?.Select(m => m.Text)));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get user", e);
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuthorityModel>> GetUserAuthoritiesAsync(int userId, string authorizationApplicationIdentifier = null)
        {
            try
            {
                var response = await _httpClientService.GetDataAsync<ResponseModel<IEnumerable<AuthorityModel>>>(
                    new Uri($"{ _userServiceConfiguration.BaseAddress }/User/{ userId }/authorities?applicationIdentifier={ authorizationApplicationIdentifier ?? "artportalen" }&lang=sv-SE"));

                return response.Success
                    ? response.Result
                    : throw new Exception(string.Concat(response.Messages?.Select(m => m.Text)));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get user authorities", e);
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RoleModel>> GetUserRolesAsync(int userId, string authorizationApplicationIdentifier = null)
        {
            try
            {
                var response = await _httpClientService.GetDataAsync<ResponseModel<IEnumerable<RoleModel>>>(
                    new Uri($"{ _userServiceConfiguration.BaseAddress }/User/{ userId }/roles?applicationIdentifier={ authorizationApplicationIdentifier ?? "artportalen" }&localeId=175"));

                return response.Success
                    ? response.Result
                    : throw new Exception(string.Concat(response.Messages?.Select(m => m.Text)));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get user roles", e);
            }

            return null;
        }
    }
}
