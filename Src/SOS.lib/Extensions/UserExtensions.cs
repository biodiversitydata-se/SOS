using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Extensions
{
    public static class UserExtensions
    {
        /// <summary>
        /// Check if user is authorized to view protected observations
        /// </summary>
        /// <param name="user"></param>
        /// <param name="protectedScope"></param>
        /// <param name="extendedAuthorizations"></param>
        /// <returns></returns>
        public static bool IsAuthorized(this ClaimsPrincipal user, string protectedScope, IEnumerable<ExtendedAuthorizationFilter> extendedAuthorizations)
        {
            return !(user?.Claims?.Count(c =>
                       (c.Type?.Equals("scope", StringComparison.CurrentCultureIgnoreCase) ?? false) &&
                       (c.Value?.Equals(protectedScope, StringComparison.CurrentCultureIgnoreCase) ?? false)) == 0
                   || (!extendedAuthorizations?.Any() ?? true));
        }
    }
}
