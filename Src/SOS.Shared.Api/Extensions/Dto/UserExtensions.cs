using SOS.Lib.Models.UserService;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;

namespace SOS.Shared.Api.Extensions.Dto;

public static class UserExtensions
{

    extension(UserInformation userInformation)
    {
        public UserInformationDto ToUserInformationDto()
        {
            if (userInformation == null)
            {
                return null!;
            }

            return new UserInformationDto
            {
                Id = userInformation.Id,
                UserName = userInformation.UserName,
                FirstName = userInformation.FirstName,
                LastName = userInformation.LastName,
                Email = userInformation.Email,
                HasSensitiveSpeciesAuthority = userInformation.HasSensitiveSpeciesAuthority,
                HasSightingIndicationAuthority = userInformation.HasSightingIndicationAuthority,
                Roles = userInformation.Roles.Select(role => role.ToUserRoleDto()).ToArray()
            };
        }
    }

    extension(UserRole userRole)
    {
        public UserRoleDto ToUserRoleDto()
        {
            if (userRole == null) return null!;

            return new UserRoleDto
            {
                Id = userRole.Id,
                Name = userRole.Name,
                ShortName = userRole.ShortName,
                Description = userRole.Description,
                HasSensitiveSpeciesAuthority = userRole.HasSensitiveSpeciesAuthority,
                HasSightingIndicationAuthority = userRole.HasSightingIndicationAuthority,
                Authorities = userRole.Authorities?.Select(a => a.ToUserAuthorityDto()).ToList()!
            };
        }
    }

    extension(UserAuthority userAuthority)
    {
        public UserAuthorityDto ToUserAuthorityDto()
        {
            if (userAuthority == null) return null!;

            return new UserAuthorityDto
            {
                Id = userAuthority.Id,
                Name = userAuthority.Name,
                Areas = userAuthority.Areas?.Select(a => a.ToUserAreaDto()).ToList()!
            };
        }
    }

    extension(UserArea userArea)
    {
        public UserAreaDto ToUserAreaDto()
        {
            if (userArea == null) return null!;

            return new UserAreaDto
            {
                AreaType = (AreaTypeDto)userArea.AreaType,
                Buffer = userArea.Buffer,
                FeatureId = userArea.FeatureId,
                Name = userArea.Name
            };
        }
    }
}