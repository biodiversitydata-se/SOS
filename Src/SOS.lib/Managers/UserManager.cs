using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Algorithm.Match;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.UserService;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Managers
{
    /// <summary>
    ///     User manager
    /// </summary>
    public class UserManager : IUserManager
    {
        private readonly ILogger<UserManager> _logger;
        private readonly IAreaCache _areaCache;
        public IUserService UserService { get; set; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="areaCache"></param>
        /// <param name="logger"></param>
        public UserManager(
            IUserService userService,
            IAreaCache areaCache,
            ILogger<UserManager> logger)
        {
            UserService = userService ?? throw new ArgumentNullException(nameof(userService));
            _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get user information.
        /// </summary>
        /// <param name="cultureCode"></param>
        /// <param name="applicationIdentifier"></param>
        /// <returns></returns>
        public async Task<UserInformation> GetUserInformationAsync(string applicationIdentifier, string cultureCode)
        {            
            var user = await UserService.GetUserAsync();            
            if (user == null) throw new AuthenticationRequiredException("User is null, probably due to missing authentication header.");
            var userRoles = await UserService.GetUserRolesAsync(user.Id, applicationIdentifier, cultureCode);
            var authorities = await UserService.GetUserAuthoritiesAsync(user.Id, applicationIdentifier, cultureCode) ??
                              new List<AuthorityModel>();
            
            var userInformation = new UserInformation
            {
                UserName = user.UserName,
                Id = user.Id,
                Email = user.EmailAddress
            };

            if (user.PersonId.HasValue)
            {
                var person = await UserService.GetPersonAsync(user.PersonId.Value, cultureCode);
                userInformation.FirstName = person.FirstName;
                userInformation.LastName = person.LastName;
            }            
            
            userInformation.Roles = new List<UserRole>();
            foreach (var role in userRoles)
            {
                var userRole = new UserRole
                {
                    Id = role.Id,
                    Name = role.RoleName,
                    ShortName = role.ShortName,
                    Description = role.Description
                    //AuthorityIds = role.Authorities != null ? role.Authorities.Select(m => m.Id).ToArray() : null
                };
                userRole.Authorities = await GetUserAuthoritiesAsync(role.Authorities);                
                userInformation.Roles.Add(userRole);
            }

            //userInformation.Authorities = new List<UserAuthority>();
            foreach (var authority in authorities)
            {
                //var userAuthority = new UserAuthority();
                //userAuthority.Id = authority.Id;
                //userAuthority.Name = authority.Name;
                //userAuthority.AuthorityIdentity = authority.AuthorityIdentity;

                if (authority.AuthorityIdentity == "Sighting" && authority.MaxProtectionLevel >= 3)
                {                    
                    userInformation.HasSensitiveSpeciesAuthority = true;
                }
                if (authority.AuthorityIdentity == "SightingIndication")
                {
                    userInformation.HasSightingIndicationAuthority = true;
                }

                //userInformation.Authorities.Add(userAuthority);
            }

            return userInformation;
        }

        private async Task<List<UserAuthority>> GetUserAuthoritiesAsync(IEnumerable<AuthorityModel> authorityModels)
        {
            if (authorityModels == null || !authorityModels.Any()) return null;

            List<UserAuthority> userAuthorities = new List<UserAuthority>();
            foreach(var authorityModel in authorityModels)
            {
                var userAuthority = await GetUserAuthorityAsync(authorityModel);
                userAuthorities.Add(userAuthority);
            }

            return userAuthorities;
        }

        private async Task<UserAuthority> GetUserAuthorityAsync(AuthorityModel authorityModel)
        {
            var userAuthority = new UserAuthority();
            userAuthority.Id = authorityModel.Id;
            userAuthority.Name = authorityModel.Name;
            userAuthority.Areas = await GetUserAreasAsync(authorityModel.Areas);
            return userAuthority;
        }

        private async Task <List<UserArea>> GetUserAreasAsync(IEnumerable<AreaModel> areaModels)
        {
            if (areaModels == null || !areaModels.Any()) return null;

            List<UserArea> userAreas = new List<UserArea>();
            foreach(var areaModel in areaModels)
            {
                var userArea = await GetUserAreaAsync(areaModel);
                userAreas.Add(userArea);
            }
            return userAreas;
        }

        private async Task<UserArea> GetUserAreaAsync(AreaModel areaModel)
        {
            AreaType areaType = (AreaType)areaModel.AreaTypeId;
            var areaInfo = await _areaCache.GetAsync(areaType, areaModel.FeatureId);
            return new UserArea
            {
                AreaType = areaType,
                FeatureId = areaModel.FeatureId,
                Name = areaInfo.Name
            };            
        }
    }
}