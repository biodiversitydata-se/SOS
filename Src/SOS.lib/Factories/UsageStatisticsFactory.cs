using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgileObjects.AgileMapper.Extensions;
using SOS.Lib.Models.ApiManagement;
using SOS.Lib.Models.ApplicationInsights;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.UserService;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Factories
{
    /// <summary>
    /// Usage statistics factory
    /// </summary>
    public class UsageStatisticsFactory
    {
        private readonly IApiManagementUserService _apiManagementUserService;
        private readonly IUserService _userService;
        private readonly IDictionary<string, ApiManagementUser> _apiManagementUsers;
        private readonly IDictionary<int, UserModel> _users;
        private readonly SemaphoreSlim _apiManagementUserSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _userServiceUserSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userService"></param>
        public UsageStatisticsFactory(IApiManagementUserService apiManagementUserService, IUserService userService)
        {
            _apiManagementUserService = apiManagementUserService ??
                                        throw new ArgumentNullException(nameof(apiManagementUserService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _apiManagementUsers = new ConcurrentDictionary<string, ApiManagementUser>();
            _users = new ConcurrentDictionary<int, UserModel>();
        }

        /// <summary>
        /// Cast multiple UsageStatistics rows to models
        /// </summary>
        /// <param name="apiUsageStatisticsRows"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ApiUsageStatistics>> CastToApiUsageStatisticsAsync(
            IEnumerable<ApiUsageStatisticsRow> apiUsageStatisticsRows)
        {
            var tasks = new List<Task<ApiUsageStatistics>>();
            apiUsageStatisticsRows.ForEach(r => tasks.Add(CastToApiUsageStatisticAsync(r)));

            await Task.WhenAll(tasks);

            return tasks.Select(t => t.Result);
        }

        /// <summary>
        /// Cast api UsageStatistics Row to model
        /// </summary>
        /// <param name="apiUsageStatisticsRow"></param>
        /// <returns></returns>
        public async Task<ApiUsageStatistics> CastToApiUsageStatisticAsync(
         ApiUsageStatisticsRow apiUsageStatisticsRow)
        {
            try
            {
                ApiManagementUser apiManagementUser = null;
                if (!string.IsNullOrEmpty(apiUsageStatisticsRow.AccountId) && !_apiManagementUsers.TryGetValue(apiUsageStatisticsRow.AccountId, out apiManagementUser))
                {
                    try
                    {
                        await _apiManagementUserSemaphore.WaitAsync();
                        if (!_apiManagementUsers.TryGetValue(apiUsageStatisticsRow.AccountId, out apiManagementUser))
                        {
                            apiManagementUser = await _apiManagementUserService.GetUserAsync(apiUsageStatisticsRow.AccountId);
                            _apiManagementUsers.TryAdd(apiUsageStatisticsRow.AccountId, apiManagementUser);
                        }
                    }
                    finally
                    {
                        _apiManagementUserSemaphore.Release();
                    }
                }

                UserModel user = null;
                if (int.TryParse(apiUsageStatisticsRow.UserId, out var userId) && !_users.TryGetValue(userId, out user))
                {
                    try
                    {
                        await _userServiceUserSemaphore.WaitAsync();
                        if (!_users.TryGetValue(userId, out user))
                        {
                            user = await _userService.GetUserByIdAsync(userId);
                            _users.TryAdd(userId, user);
                        }
                    }
                    finally
                    {
                        _userServiceUserSemaphore.Release();
                    }
                }

                return new ApiUsageStatistics
                {
                    AccountId = apiUsageStatisticsRow.AccountId,
                    ApiManagementUserEmail = apiManagementUser?.Email,
                    ApiManagementUserName = $"{apiManagementUser?.FirstName} {apiManagementUser?.LastName}",
                    AverageDuration = apiUsageStatisticsRow.AverageDuration,
                    Date = apiUsageStatisticsRow.Date,
                    Endpoint = apiUsageStatisticsRow.Endpoint?.StartsWith('/') ?? false ? apiUsageStatisticsRow.Endpoint.Substring(1) : apiUsageStatisticsRow.Endpoint,
                    FailureCount = apiUsageStatisticsRow.FailureCount,
                    Method = apiUsageStatisticsRow.Method,
                    RequestCount = apiUsageStatisticsRow.RequestCount,
                    SumResponseCount = apiUsageStatisticsRow.SumResponseCount,
                    UserEmail = user?.EmailAddress,
                    UserId = apiUsageStatisticsRow.UserId,
                    UserName = user?.UserName
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}