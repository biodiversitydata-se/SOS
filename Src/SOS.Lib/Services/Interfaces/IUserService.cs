﻿using Microsoft.IdentityModel.JsonWebTokens;
using SOS.Lib.Models.UserService;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Services.Interfaces
{
    /// <summary>
    ///     Interface for user service
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        ///  Get current user
        /// </summary>
        /// <returns></returns>
        Task<UserModel> GetUserAsync();

        /// <summary>
        /// Get user by id.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<UserModel> GetUserByIdAsync(int userId);

        /// <summary>
        /// Get authorities for user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        Task<IEnumerable<AuthorityModel>> GetUserAuthoritiesAsync(int userId, string authorizationApplicationIdentifier = null, string cultureCode = "sv-SE");

        /// <summary>
        /// Get user roles
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        Task<IEnumerable<RoleModel>> GetUserRolesAsync(int userId, string authorizationApplicationIdentifier = null, string cultureCode = "sv-SE");

        /// <summary>
        /// Get person information.
        /// </summary>
        /// <param name="personId"></param>        
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        Task<PersonModel> GetPersonAsync(int personId, string cultureCode = "sv-SE");

        Task<JsonWebToken> GetClientCredentialsAccessTokenAsync();
    }
}