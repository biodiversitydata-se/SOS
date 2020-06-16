using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Models.UserService;
using SOS.Observations.Api.Services.Interfaces;

namespace SOS.Observations.Api.Services
{
    /// <summary>
    /// User service
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly UserServiceConfiguration _userServiceConfiguration;
        private readonly ILogger<UserService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="userServiceConfiguration"></param>
        /// <param name="logger"></param>
        public UserService(
            IHttpClientService httpClientService,
            UserServiceConfiguration userServiceConfiguration,
            ILogger<UserService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _userServiceConfiguration = userServiceConfiguration ??
                                        throw new ArgumentNullException(nameof(userServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<ResponseModel<IEnumerable<RoleModel>>> GetUserRolesAsync(int userId)
        {
            try
            {
                return await _httpClientService.GetDataAsync<ResponseModel<IEnumerable<RoleModel>>>(
                    new Uri($"{ _userServiceConfiguration.BaseAddress }/User/{ userId }/roles?applicationIdentifier=artportalen&localeId=175"));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get user roles", e);
            }

            return null;
        }
    }
}
