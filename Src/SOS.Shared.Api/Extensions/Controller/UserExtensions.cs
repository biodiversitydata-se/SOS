using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Exceptions;
using SOS.Shared.Api.Dtos.Enum;
using System.Security.Claims;

namespace SOS.Shared.Api.Extensions.Controller;

public static class UserExtensions
{
    extension(ClaimsPrincipal user)
    {
        /// <summary>
        ///  Check if user is authorized to view protected observations
        /// </summary>
        /// <param name="protectedScope"></param>
        /// <returns></returns>
        private bool HasAccessToScope(string protectedScope)
        {
            return user?.Claims?.Count(c =>
                       (c.Type?.Equals("scope", StringComparison.CurrentCultureIgnoreCase) ?? false) &&
                       (c.Value?.Equals(protectedScope, StringComparison.CurrentCultureIgnoreCase) ?? false)) != 0;
        }

        /// <summary>
        /// Check if user is authorized
        /// </summary>
        /// <param name="protectionFilter"></param>
        /// <param name="protectedScope"></param>
        /// <exception cref="AuthenticationRequiredException"></exception>
        public void CheckAuthorization(string protectedScope, ProtectionFilterDto? protectionFilter)
        {
            if (!(protectionFilter ?? ProtectionFilterDto.Public).Equals(ProtectionFilterDto.Public) && (!user?.HasAccessToScope(protectedScope) ?? true))
            {
                throw new AuthenticationRequiredException("Not authorized");
            }
        }
    }

    extension(ControllerBase controller)
    {
        /// <summary>
        /// Get current users e-mail address
        /// </summary>
        public string? GetUserEmail() => controller?.User?.Claims?.FirstOrDefault(c => c.Type.Contains("emailaddress", StringComparison.CurrentCultureIgnoreCase))?.Value;

        /// <summary>
        /// Get id of current user
        /// </summary>
        public int GetUserId() => controller?.User?.GetUserId() ?? 0;
    }

    extension(ClaimsPrincipal claimsPrincipal)
    {
        /// <summary>
        /// Get id of current user
        /// </summary>
        public int GetUserId() => int.Parse(claimsPrincipal?.Claims?.FirstOrDefault(c => c.Type.Contains("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.CurrentCultureIgnoreCase) || c.Type.Contains("client_uaid", StringComparison.CurrentCultureIgnoreCase))?.Value ?? "0");
    }
}
