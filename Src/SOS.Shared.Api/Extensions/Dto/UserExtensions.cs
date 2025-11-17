using SOS.Lib.Models.UserService;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;

namespace SOS.Shared.Api.Extensions.Dto;

public static class UserExtensions
{
   
    public static UserInformationDto ToUserInformationDto(this UserInformation userInformation)
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

    public static UserRoleDto ToUserRoleDto(this UserRole userRole)
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

    public static UserAuthorityDto ToUserAuthorityDto(this UserAuthority userAuthority)
    {
        if (userAuthority == null) return null!;

        return new UserAuthorityDto
        {
            Id = userAuthority.Id,
            Name = userAuthority.Name,
            Areas = userAuthority.Areas?.Select(a => a.ToUserAreaDto()).ToList()!
        };
    }

    public static UserAreaDto ToUserAreaDto(this UserArea userArea)
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