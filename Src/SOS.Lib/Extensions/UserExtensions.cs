using System;
using System.Linq;
using System.Security.Claims;

namespace SOS.Lib.Extensions;

public static class UserExtensions
{
    extension(ClaimsPrincipal user)
    {
        /// <summary>
        ///  Check if user is authorized to view protected observations
        /// </summary>
        /// <param name="protectedScope"></param>
        /// <returns></returns>
        public bool HasAccessToScope(string protectedScope)
        {
            return user?.Claims?.Count(c =>
                       (c.Type?.Equals("scope", StringComparison.CurrentCultureIgnoreCase) ?? false) &&
                       (c.Value?.Equals(protectedScope, StringComparison.CurrentCultureIgnoreCase) ?? false)) != 0;
        }
    }
}
