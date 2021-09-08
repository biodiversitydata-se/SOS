using System;
using System.Linq;
using System.Security.Claims;

namespace SOS.Lib.Extensions
{
    public static class UserExtensions
    {
        /// <summary>
        ///  Check if user is authorized to view protected observations
        /// </summary>
        /// <param name="user"></param>
        /// <param name="protectedScope"></param>
        /// <returns></returns>
        public static bool HasAccessToScope(this ClaimsPrincipal user, string protectedScope)
        {
            return user?.Claims?.Count(c =>
                       (c.Type?.Equals("scope", StringComparison.CurrentCultureIgnoreCase) ?? false) &&
                       (c.Value?.Equals(protectedScope, StringComparison.CurrentCultureIgnoreCase) ?? false)) != 0;
        }
    }
}
