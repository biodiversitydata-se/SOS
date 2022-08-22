using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        public IUserService UserService { get; set; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="logger"></param>
        public UserManager(
            IUserService userService,
            ILogger<UserManager> logger)
        {
            UserService = userService ?? throw new ArgumentNullException(nameof(userService));
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
    }
}